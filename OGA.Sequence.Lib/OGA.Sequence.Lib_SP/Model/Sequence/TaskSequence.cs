using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OGA.Sequence.Factory;
using OGA.Sequence.Model.Config;
using OGA.Sequence.Model.Results;
using OGA.Sequence.Model.Steps;
using OGA.Sequence.Model.Transitions;

namespace OGA.Sequence.Model.Sequence
{
    public class TaskSequence
    {
        #region Private Fields

        protected SequenceConfig _cfg;

        private Guid _firststepid;

        #endregion


        #region Public Properties

        /// <summary>
        /// Unique identifier of the test sequence.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Friendly name of the test sequence.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Friendly description for the test sequence.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of steps in the sequence.
        /// </summary>
        public List<TaskStep_abstract> Steps { get; protected set; }

        /// <summary>
        /// List of defined transitions in the sequence.
        /// </summary>
        public List<Transition_abstract> Transitions { get; protected set; }

        /// <summary>
        /// Id of the current step.
        /// </summary>
        public Guid? CurrentStepId { get; protected set; }

        /// <summary>
        /// Current state of the sequence.
        /// </summary>
        public eStepState State { get; private set; }

        /// <summary>
        /// Test sequence result data.
        /// </summary>
        public ResultList Results { get; protected set; }

        /// <summary>
        /// Array list of step states that will trigger the state change delegate.
        /// By default, is set to the operational states: Running, Completed, Aborted, Cancelled.
        /// </summary>
        public eStepState[] Cfg_StepStates_toNotify { get; set; } =
                new eStepState[] { eStepState.Running, eStepState.Aborted, eStepState.Cancelled, eStepState.Completed };

        /// <summary>
        /// Tells the sequence to clear collected result entries when execution starts.
        /// This removes all of the result entries from loading and validation checks, and slims down the result set.
        /// </summary>
        public bool Cfg_ClearResults_OnRunning { get; set; } = true;

        #endregion


        #region Public Delegates

        public delegate void delSequenceStateChange(TaskSequence seq, eStepState oldstate, eStepState newstate);
        private delSequenceStateChange _delOnSequenceStateChange;
        /// <summary>
        /// Assign a handler to this delegate to receive sequence state change events.
        /// </summary>
        public delSequenceStateChange OnSequenceStateChange
        {
            set
            {
                this._delOnSequenceStateChange = value;
            }
        }

        public delegate void delStepStateChange(TaskStep_abstract stp, eStepState oldstate, eStepState newstate);
        private delStepStateChange _delOnStepStateChange;
        /// <summary>
        /// Assign a handler to this delegate to be notified when a sequence step changes state.
        /// Property, StepStates_toNotify, defines what step states trigger this delegate.
        /// </summary>
        public delStepStateChange OnStepStateChange
        {
            set
            {
                this._delOnStepStateChange = value;
            }
        }

        #endregion


        #region ctor / dtor

        public TaskSequence()
        {
            State = eStepState.Unknown;

            Steps = new List<TaskStep_abstract>();
            Transitions = new List<Transition_abstract>();
        }

        #endregion


        #region Overrides

        /// <summary>
        /// Define any config validation logic for config used by a derived sequence type.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<int> ValidateConfig()
        {
            bool valpassed = true;

            // Add logic to check 



            // Return success if each check above passed...
            if (valpassed)
                return 1;
            else
                return -1;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Loads config into the sequence.
        /// Will validate configuration after loading.
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public async Task<int> Load(SequenceConfig cfg)
        {
            bool success = false;

            // Make an early determination of our Id in case we need it for logging...
            Guid id = (cfg == null ? Guid.Empty : cfg.Id);

            try
            {
                // Ensure the results list exists...
                this.Results = new ResultList();

                // Signal the loading start...
                this.Results?.Add_StartEntry(eResultPhase.Loading, eObjectType.Sequence, id);

                // Do some quick and dirty validation...
                {
                    if(cfg == null)
                    {
                        // Config is null.
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Sequence config is null.", eObjectType.SequenceConfig, id);
                        return -1;
                    }
                    if(cfg.Id == Guid.Empty)
                    {
                        // Sequence Id is empty.
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Sequence config contains blank sequence Id.", eObjectType.SequenceConfig, id);
                        return -2;
                    }
                    if(cfg.StepList == null || cfg.StepList.Count == 0)
                    {
                        // No steps to run.
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Sequence config contains no steps to execute.", eObjectType.SequenceConfig, id);
                        return -3;
                    }
                }

                // Accept given config so we can test it and setup steps...
                this._cfg = cfg;

                // Set the local properties...
                {
                    this.Name = this._cfg.Name;
                    this.Description = this._cfg.Description;
                    this.Id = this._cfg.Id;
                }

                // Populate the step list from config...
                var res2 = await this.LoadStepsfromConfig();
                if(res2 != 1)
                {
                    // Something went wrong.
                    // The call already published an error.
                    // We can leave.
                    return -4;
                }

                // Populate the transition list from config...
                var res3 = await this.LoadTransitionsfromConfig();
                if(res3 != 1)
                {
                    // Something went wrong.
                    // The call already published an error.
                    // We can leave.
                    return -5;
                }

                // Do all sequence-wide validation checks...
                var res = await DoValidateConfig();
                if (res != 1)
                {
                    // Cross step validation checks failed.
                    // The call already published an error.
                    // We can leave.
                    return -6;
                }
                // We've loaded steps and transitions from config, and performed validations.
                // We can consider the sequence loaded and validated.

                success = true;

                // Set the loading disposition to good...
                this.Results?.Add_DispositionEntry(eResultPhase.Loading, eObjectType.Sequence, this.Id, eDisposition.Pass);

                // Set the ready state...
                this.UpdateState(eStepState.Ready);

                return 1;
            }
            finally
            {
                // See if we loaded and validated...
                if(!success)
                {
                    // Failed to load and validate.

                    // Clear steps and such...
                    this._cfg = null;

                    // Have each defined step unhook any callbacks we gave it...
                    foreach (var s in this.Steps)
                        s.OnStateChange = null;

                    this.Steps.Clear();
                    this.Transitions.Clear();

                    // Set the loading disposition to bad...
                    this.Results?.Add_DispositionEntry(eResultPhase.Loading, eObjectType.Sequence, this.Id, eDisposition.Fail);
                }

                // Signal the loading end...
                this.Results?.Add_EndEntry(eResultPhase.Loading, eObjectType.Sequence, id);
            }
        }

        /// <summary>
        /// Public validation call.
        /// Will perform local checks, then call the derived type's validate.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        public async Task<int> DoValidateConfig()
        {
            bool valpassed = true;

            // Call the private validate method...
            // Wrap it in a try-catch to ensure the derived method doesn't throw and unwind us.
            try
            {
                // Call the private validate method...
                var res1 = await _ValidateConfig();
                if(res1 != 1)
                {
                    // Local validate failed.
                    // It has already published.
                    valpassed = false;
                }
            }
            catch (Exception ex)
            {
                // Derived validate threw.

                // Add an error to the results...
                this.Results?.Add_ErrorResult(eResultPhase.Loading, "Transition config failed validation.", eObjectType.TransitionConfig, this.Id);
                valpassed = false;
            }

            // Call the derived validate method...
            var res2 = await ValidateConfig();
            if(res2 != 1)
            {
                // Derived validate failed.

                // Add an error to the results...
                this.Results?.Add_ErrorResult(eResultPhase.Loading, "Transition config failed validation.", eObjectType.TransitionConfig, this.Id);
                valpassed = false;
            }

            // Return success if each check above passed...
            if (valpassed)
                return 1;
            else
                return -1;
        }

        /// <summary>
        /// Called to perform the sequence execution.
        /// Returns 1 on success, 0 if user-cancelled, negatives are aborts for internal errors.
        /// </summary>
        /// <returns></returns>
        public async Task<int> Execute(CancellationToken token = default(CancellationToken))
        {
            try
            {
                // Ensure the sequence is ready...
                if(this.State != eStepState.Ready)
                {
                    // The sequence cannot be started, since it is not ready.

                    // Add a result entry for the false start...
                    this.Results?.Add_ErrorResult(eResultPhase.Running, "Failed to start. Sequence not ready.", eObjectType.Sequence, this.Id);

                    return -1;
                }

                // Before we signal the running state, we need to determine if we are to clear out all result data from loading and validation checks...
                if(this.Cfg_ClearResults_OnRunning)
                {
                    // We are to clear the result data from loading and validation checks.
                    this.Results.ClearEntries();
                }

                // Signal the sequence start...
                this.Results?.Add_StartEntry(eResultPhase.Running, eObjectType.Sequence, this.Id);

                // Update running state...
                this.UpdateState(eStepState.Running);

                // Set the current step to the first step...
                this.CurrentStepId = this._firststepid;

                // Wrap the execution loop in a try-catch, that looks for the cancellation...
                try
                {
                    // Iterate steps until completed, an error occurs, or we get cancelled...
                    while(!token.IsCancellationRequested)
                    {
                        // Load the current step...
                        var cstep = this.GetStepbyId(this.CurrentStepId);
                        if(cstep == null)
                        {
                            // Step not found.
                            // Cannot continue sequence.

                            // Clear the current step...
                            this.CurrentStepId = null;

                            // Update to the aborted state...
                            this.UpdateState(eStepState.Aborted);

                            // Add an error...
                            this.Results?.Add_ErrorResult(eResultPhase.Running, "Sequence missing current step.", eObjectType.Sequence, this.Id);

                            // Set sequence dispo...
                            this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Aborted);

                            return -1;
                        }
                        // If here, we have the step.

                        // Execute it...
                        var resstep = await cstep.Execute(token);
                        if(resstep == 0)
                        {
                            // Execution was cancelled during step.

                            // Set a cancellation result...
                            this.Results?.Add_CancellationEntry(eResultPhase.Running, "cancelled", eObjectType.Step, cstep.Id, "", "");

                            // Clear the current step...
                            this.CurrentStepId = null;

                            // Update to the cancelled state...
                            this.UpdateState(eStepState.Cancelled);

                            // Set sequence dispo...
                            this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Cancelled);

                            return 0;
                        }
                        if(resstep < 0)
                        {
                            // Step was aborted on error.

                            // Clear the current step...
                            this.CurrentStepId = null;

                            // Update to the aborted state...
                            this.UpdateState(eStepState.Aborted);

                            // Add an error...
                            this.Results?.Add_ErrorResult(eResultPhase.Running, "Sequence aborted on error.", eObjectType.Sequence, this.Id);

                            // Set sequence dispo...
                            this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Aborted);

                            return -1;
                        }
                        // If here, the step completed.

                        // See if the step is terminal...
                        if(cstep.IsTerminalStep)
                        {
                            // No more steps to execute.
                            // We are done.

                            // All steps have finished.

                            // Clear the current step...
                            this.CurrentStepId = null;

                            // Update to the completed state...
                            this.UpdateState(eStepState.Completed);

                            // Set sequence dispo...
                            this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Completed);

                            return 1;
                        }
                        else
                        {
                            // Not a terminal step.
                            // We need to determine the next step.
                            // We will iterate the associated transitions for the step, and see which is satisfied.

                            var tl = this.GetTransitionsforCurrentStep();
                            if(tl == null || tl.Count == 0)
                            {
                                // No transitions defined for the step.

                                // Clear the current step...
                                this.CurrentStepId = null;

                                // Update to the aborted state...
                                this.UpdateState(eStepState.Aborted);

                                // Add an error...
                                this.Results?.Add_ErrorResult(eResultPhase.Running, "Sequence missing transition for step.", eObjectType.Sequence, this.Id);

                                // Set sequence dispo...
                                this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Aborted);

                                return -1;
                            }
                            // We have transitions to evaluate.

                            // Look for the first active transition...
                            Transition_abstract activetrans = null;
                            // We will sort them in display order, first.
                            tl.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
                            foreach(var tr in tl)
                            {
                                // Evaluate the transition...
                                var tres = await tr.IsTrue(cstep);

                                // See if it is active...
                                if(tres == 1)
                                {
                                    // Active transition.

                                    activetrans = tr;

                                    // Leave the transition search loop...
                                    break;
                                }
                            }

                            // See if we found an active transition...
                            if(activetrans == null)
                            {
                                // No active transition was found.
                                // Our implementation requires each step to have one.
                                // So, we must be in error.

                                // Clear the current step...
                                this.CurrentStepId = null;

                                // Update to the aborted state...
                                this.UpdateState(eStepState.Aborted);

                                // Add an error...
                                this.Results?.Add_ErrorResult(eResultPhase.Running, "Sequence has no active transition for step.", eObjectType.Sequence, this.Id);

                                // Set sequence dispo...
                                this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Aborted);

                                return -1;
                            }
                            // An active transition was found.
                            // We will use it to advance to the next step.

                            // Get the next step...
                            // Check that the next step exists...
                            var nextstep = this.GetStepbyId(activetrans.NextStepId);
                            if(nextstep == null)
                            {
                                // Step not found.
                                // Cannot continue sequence.

                                // Clear the current step...
                                this.CurrentStepId = null;

                                // Update to the aborted state...
                                this.UpdateState(eStepState.Aborted);

                                // Add an error...
                                this.Results?.Add_ErrorResult(eResultPhase.Running, "Sequence missing step from transition.", eObjectType.Sequence, this.Id);

                                // Set sequence dispo...
                                this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Aborted);

                                return -1;
                            }
                            // We have a step to execute.

                            // Mark what transition we used...
                            this.Results?.Add_Transition(activetrans.Id, (Guid)this.CurrentStepId, activetrans.NextStepId);

                            // Update current step to the target step...
                            this.CurrentStepId = activetrans.NextStepId;
                        }
                    }
                    // Bottom of the step iteration loop.

                    // All steps have finished.

                    // Clear the current step...
                    this.CurrentStepId = null;

                    // Update to the completed state...
                    this.UpdateState(eStepState.Completed);

                    // Set sequence dispo...
                    this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Completed);

                    // Set an overall dispo...
                    this.Results?.Add_OverallDispositionEntry(eObjectType.Sequence, this.Id, eDisposition.Completed);

                    return 1;
                }
                catch (OperationCanceledException oce)
                {
                    // The sequence was cancelled.

                    // Clear the current step...
                    this.CurrentStepId = null;

                    // Update to the Cancelled state...
                    this.UpdateState(eStepState.Cancelled);

                    // Set sequence dispo...
                    this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Cancelled);

                    // Set an overall dispo...
                    this.Results?.Add_OverallDispositionEntry(eObjectType.Sequence, this.Id, eDisposition.Cancelled);

                    return 0;
                }
            }
            catch (Exception ex)
            {
                // Exception occurred.
                // We will consider the sequence as aborted.

                // Clear the current step...
                this.CurrentStepId = null;

                // Update to the aborted state...
                this.UpdateState(eStepState.Aborted);

                // Add an error...
                this.Results?.Add_ErrorResult(eResultPhase.Running, "Exception aborted sequence on error.", eObjectType.Sequence, this.Id);

                // Set sequence dispo...
                this.Results?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Sequence, this.Id, eDisposition.Aborted);

                // Set an overall dispo...
                this.Results?.Add_OverallDispositionEntry(eObjectType.Sequence, this.Id, eDisposition.Cancelled);

                return -2;
            }
            finally
            {
                // Signal the sequence end...
                this.Results?.Add_EndEntry(eResultPhase.Running, eObjectType.Sequence, this.Id);

                // Disconnect all step callbacks for easy cleanup...
                foreach(var s in this.Steps)
                    s.OnStateChange = null;
            }
        }

        #endregion


        #region Status Updates

        protected void UpdateState(eStepState state)
        {
            if(State == state)
            {
                // No change.
                return;
            }

            // Save the old state, so we can report it...
            var oldstate = this.State;

            // Update state...
            this.State = state;

            // Report it...
            this.Results?.Add_ObjStateChange(eObjectType.Step, this.Id, oldstate.ToString(), this.State.ToString());

            Fire_OnSequenceStateChange(oldstate, this.State);
        }

        #endregion


        #region Delegate Calls

        /// <summary>
        /// Calls state change delegate on a different thread, to prevent stalling execution.
        /// </summary>
        /// <param name="oldstate"></param>
        /// <param name="newstate"></param>
        private void Fire_OnSequenceStateChange(eStepState oldstate, eStepState newstate)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (_delOnSequenceStateChange != null)
                        this._delOnSequenceStateChange(this, oldstate, newstate);
                }
                catch(Exception e) { }
            });
        }


        /// <summary>
        /// Called each time a sequence step changes state.
        /// This callback is already called on an alternate thread, to prevent the step from stalling if we take too long to handle this call.
        /// </summary>
        /// <param name="stp"></param>
        /// <param name="oldstate"></param>
        /// <param name="newstate"></param>
        private void CALLBACK_OnStepStateChange(TaskStep_abstract stp, eStepState oldstate, eStepState newstate)
        {
            // We will only call the delegate for states that are set to notify.
            if(Cfg_StepStates_toNotify == null || Cfg_StepStates_toNotify.Length == 0)
            {
                // No step states are defined for notification.
                return;
            }

            // See if we are to notify for the current step state...
            if(!Cfg_StepStates_toNotify.Contains(newstate))
            {
                // Notification is not enabled for this step state.
                return;
            }
            // We are to notify for this step state.

            // Call the external delegate...
            // No need to do a Task.Run, as we are already running in an alternate thread.
            // And, this callback is already wrapped in a try-catch by the step's delegate.
            // So, we can simple call our delegate.
            if (_delOnStepStateChange != null)
                this._delOnStepStateChange(stp, oldstate, newstate);
        }

        #endregion


        #region Private Methods

        private List<Transition_abstract> GetTransitionsforCurrentStep()
        {
            try
            {
                var tl = this.Transitions.Where(n=>n.StepId == this.CurrentStepId).ToList();
                if(tl == null)
                    return new List<Transition_abstract>();

                return tl;
            }
            catch(Exception e)
            {
                return new List<Transition_abstract>();
            }
        }

#if (NET452 || NET48)
        private TaskStep_abstract GetStepbyId(Guid? id)
#else
        private TaskStep_abstract? GetStepbyId(Guid? id)
#endif
        {
            try
            {
                var cstep = this.Steps.Where(n=>n.Id == this.CurrentStepId).FirstOrDefault();
                if (cstep == null)
                    return null;

                return cstep;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        private async Task<int> LoadStepsfromConfig()
        {
            int startstepcounter = 0;

            TaskStep_abstract stp = null;

            try
            {
                // Create the step listing from given config...
                foreach(var s in this._cfg.StepList)
                {
                    if(s.Id == Guid.Empty)
                    {
                        // Step Id not defined.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains step with empty Id.", eObjectType.StepConfig, Guid.Empty);
                        return -1;
                    }
                    if(string.IsNullOrEmpty(s.StepType))
                    {
                        // Step type is blank.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains step with blank step type.", eObjectType.StepConfig, s.Id);
                        return -1;
                    }

                    // Check on start steps...
                    if(s.StepType == nameof(TaskStep_SequenceStart))
                    {
                        // We are loading a start step.

                        // Increment our start step counter, to ensure we have only one in the sequence...
                        startstepcounter++;

                        // Ensure we have not loaded a second...
                        if(startstepcounter > 1)
                        {
                            // Add an error to the results listing...
                            this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains more than one start step.", eObjectType.StepConfig, s.Id);
                            return -1;
                        }
                    }

                    // Create a step instance for the current config entry...
                    var res = SequenceFactory.GetStepInstance(s.StepType);
                    if(res.res != 1 || res.instance == null)
                    {
                        // Failed to locate the step type.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains unknown StepType.", eObjectType.StepConfig, s.Id);
                        return -2;
                    }
                    // If here, we got a step instance.

                    var si = res.instance;

                    // Tell the step to load config...
                    // The step will attempt to load it and validate the config for use.
                    var resload = await si.Load(s, this.Results);
                    if(resload != 1)
                    {
                        // The current step instance failed to load and validate the given config.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Step failed to load config.", eObjectType.Step, s.Id);
                        return -3;
                    }
                    // If here, we have created a loaded step instance with validated config.
                    // We can add it to our listing.

                    // Give the step a state change callback, so we can be notified when it changes state...
                    si.OnStateChange = this.CALLBACK_OnStepStateChange;

                    // Add the minted step...
                    this.Steps.Add(si);

                    // If the current step is the sequence start, we will make a reference to it...
                    if(s.StepType == nameof(TaskStep_SequenceStart))
                    {
                        // The current step is the sequence start.
                        // Save it as our starting point...
                        this._firststepid = s.Id;
                    }
                }

                // Ensure we have exactly one start step...
                if(startstepcounter == 0)
                {
                    // Add an error to the results listing...
                    this.Results.Add_ErrorResult(eResultPhase.Loading, "Config missing start step.", eObjectType.SequenceConfig, this._cfg.Id);
                    return -1;
                }
                if(startstepcounter  > 1)
                {
                    // Add an error to the results listing...
                    this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains multiple start steps.", eObjectType.SequenceConfig, this._cfg.Id);
                    return -1;
                }

                return 1;
            }
            catch(Exception e)
            {
                // Exception occurred while loading steps from config.
                // We must report this.

                // If we have a current step instance, we need to tell it to release any callbacks we gave it...
                if(stp != null)
                    stp.OnStateChange = null;

                // Add an error to the results listing...
                this.Results.Add_ErrorResult(eResultPhase.Loading, "Exception while loading step config.", eObjectType.StepConfig, Guid.Empty);
                return -4;
            }
        }

        private async Task<int> LoadTransitionsfromConfig()
        {
            try
            {
                // Create the transition listing from given config...
                foreach(var s in this._cfg.TransitionList)
                {
                    if(s.Id == Guid.Empty)
                    {
                        // Transition Id not defined.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains transition with empty Id.", eObjectType.TransitionConfig, Guid.Empty);
                        return -1;
                    }
                    if(string.IsNullOrEmpty(s.TransitionType))
                    {
                        // Transition type is blank.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains transition with blank transition type.", eObjectType.TransitionConfig, s.Id);
                        return -1;
                    }

                    // Create a transition instance for the current config entry...
                    var res = SequenceFactory.GetTransitionInstance(s.TransitionType);
                    if(res.res != 1 || res.instance == null)
                    {
                        // Failed to locate the transition type.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Config contains unknown TransitionType.", eObjectType.TransitionConfig, s.Id);
                        return -2;
                    }
                    // If here, we got a transition instance.

                    var ti = res.instance;

                    // Tell the transition to load config...
                    // The transition will attempt to load it and validate the config for use.
                    var resload = await ti.Load(s, this.Results);
                    if(resload != 1)
                    {
                        // The current transition instance failed to load and validate the given config.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Transition failed to load config.", eObjectType.Transition, s.Id);
                        return -3;
                    }
                    // If here, we have created a loaded transition instance with validated config.
                    // We can add it to our listing.

                    // Add the minted transition...
                    this.Transitions.Add(ti);
                }

                return 1;
            }
            catch(Exception e)
            {
                // We must report this.

                // Add an error to the results listing...
                this.Results.Add_ErrorResult(eResultPhase.Loading, "Exception while loading transition config.", eObjectType.TransitionConfig, Guid.Empty);
                return -4;
            }
        }

        /// <summary>
        /// Private validation call.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        private async Task<int> _ValidateConfig()
        {
            bool valpassed = true;

            if(this._cfg == null)
            {
                // No config set.

                this.Results?.Add_ErrorResult(eResultPhase.Loading, "Sequence missing config.", eObjectType.Sequence, Guid.Empty);
                valpassed = false;
            }
            if(this._cfg.Id == Guid.Empty)
            {
                // Sequence Id is empty.

                // Add an error to the results...
                this.Results?.Add_ErrorResult(eResultPhase.Loading, "Sequence has blank Id.", eObjectType.SequenceConfig, this.Id);
                valpassed = false;
            }
            if(this._cfg.StepList == null || this._cfg.StepList.Count == 0)
            {
                // No steps to run.

                // Add an error to the results...
                this.Results?.Add_ErrorResult(eResultPhase.Loading, "Sequence config has no steps to run.", eObjectType.SequenceConfig, this.Id);
                valpassed = false;
            }

            // Check if the local step list exists...
            if(this.Steps == null || this.Steps.Count == 0)
            {
                // Not created yet.

                // Add an error to the results...
                this.Results?.Add_ErrorResult(eResultPhase.Loading, "Sequence missing step list.", eObjectType.SequenceConfig, this.Id);
                valpassed = false;
            }

            // Make sure at least one step is defined...
            if(this.Steps.Count == 0)
            {
                // No steps defined.
                // We must report this.

                // Add an error to the results listing...
                this.Results.Add_ErrorResult(eResultPhase.Loading, "Sequence contains no steps.", eObjectType.Sequence, this.Id);
                valpassed = false;
            }

            // We would have identified the start step by setting a pointer to it in our sequence.
            if(this._firststepid == Guid.Empty)
            {
                // No first step set.
                // We must report this.

                // Add an error to the results listing...
                this.Results.Add_ErrorResult(eResultPhase.Loading, "Sequence missing starting step.", eObjectType.Sequence, this.Id);
                valpassed = false;
            }

            // Make sure each step has a transition or is defined as a terminal step...
            {
                // Iterate each step...
                foreach(var s in this.Steps)
                {
                    if(s.IsTerminalStep)
                    {
                        // Doesn't require a transition.
                        continue;
                    }

                    // Look for a transition with this step as a source...
                    var ts = this.Transitions.Where(m => m.StepId == s.Id).FirstOrDefault();
                    if(ts == null)
                    {
                        // Current step has no transition, and is not marked as terminal.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Step missing transition.", eObjectType.Step, s.Id);
                        valpassed = false;
                    }
                }
            }

            // Make sure each transition has a valid step target...
            {
                // Iterate each transition...
                foreach(var t in this.Transitions)
                {
                    // Look for a matching step the transition leads to...
                    var ts = this.Steps.Where(m => m.Id == t.NextStepId).FirstOrDefault();
                    if(ts == null)
                    {
                        // Current transition has missing target step.
                        // We must report this.

                        // Add an error to the results listing...
                        this.Results.Add_ErrorResult(eResultPhase.Loading, "Step missing transition.", eObjectType.Step, t.Id);
                        valpassed = false;
                    }
                }
            }

            // Make sure each step is in the ready state...
            foreach(var s in this.Steps)
            {
                if(s.State != eStepState.Ready)
                {
                    // Current step is not ready to start.
                    // We must report this.

                    // Add an error to the results listing...
                    this.Results.Add_ErrorResult(eResultPhase.Loading, "Step Not Ready.", eObjectType.Step, s.Id);
                    valpassed = false;
                }
            }

            // Make sure each transition is in the ready state...
            foreach(var s in this.Transitions)
            {
                if(s.State != eTransitionState.Ready)
                {
                    // Current transition is not ready.
                    // We must report this.

                    // Add an error to the results listing...
                    this.Results.Add_ErrorResult(eResultPhase.Loading, "Transition Not Ready.", eObjectType.Transition, s.Id);
                    valpassed = false;
                }
            }

            // Return success if each check above passed...
            if (valpassed)
                return 1;
            else
                return -1;
        }

        #endregion
    }
}
