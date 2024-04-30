using OGA.Sequence.Model.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Steps
{
    abstract public class TaskStep_abstract
    {
        #region Private Fields

        protected StepConfig _cfg;

        protected ResultList _resultsref;

        #endregion


        #region Public Properties

        /// <summary>
        /// Unique identifier of the test step.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display order of the step in a listing.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Friendly name of the test step.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Friendly description for the test step.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates the step is the last in the sequence.
        /// The sequence will end when this step completes.
        /// If set, the sequence will end at this step, regardless of any defined transitions.
        /// This can be used to short a sequence if needed.
        /// </summary>
        public bool IsTerminalStep { get; set; }

        /// <summary>
        /// Current state of the step.
        /// </summary>
        public eStepState State { get; private set; }

        /// <summary>
        /// Indicates the step is the sequence start step.
        /// </summary>
        public bool IsSeqStart { get; }

        #endregion


        #region ctor / dtor

        public TaskStep_abstract()
        {
            State = eStepState.Unknown;
        }

        #endregion


        #region Overrides

        /// <summary>
        /// Override this method with step logic implementation.
        /// Be sure to provide step disposition before exit, or the step will be considered aborted.
        /// Called to perform the step's function.
        /// Returns 1 on success, 0 if user-cancelled, negatives are aborts for internal errors.
        /// </summary>
        /// <returns></returns>
        abstract protected Task<int> Local_Execute(CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Define any config validation logic for config used by the step.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        abstract protected Task<int> ValidateConfig();

        #endregion


        #region Public Methods

        /// <summary>
        /// Loads config into the step.
        /// Will validate configuration after loading.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public async Task<int> Load(StepConfig cfg, ResultList results)
        {
            bool success = false;

            // Make an early determination of our Id in case we need it for logging...
            Guid id = (cfg == null ? Guid.Empty : cfg.Id);

            try
            {
                // Accept the results listing...
                this._resultsref = results;

                // Signal the loading start...
                this._resultsref?.Add_StartEntry(eResultPhase.Loading, eObjectType.Step, id);

                // Do some quick and dirty validation...
                {
                    if(cfg == null)
                    {
                        // Config is null.
                        _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Step config is null.", eObjectType.StepConfig, id);
                        return -1;
                    }
                    if(cfg.Id == Guid.Empty)
                    {
                        // Transition Id is empty.
                        this._resultsref.Add_ErrorResult(eResultPhase.Loading, "Step config contains blank sequence Id.", eObjectType.StepConfig, id);
                        return -2;
                    }
                }

                // Accept given config so we can test it and setup steps...
                this._cfg = cfg;

                // Set the local properties...
                {
                    this.Name = this._cfg.Name;
                    this.Description = this._cfg.Description;
                    this.Id = this._cfg.Id;

                    this.IsTerminalStep = this._cfg.IsTerminalStep;
                    this.DisplayOrder = this._cfg.DisplayOrder;
                }

                // Validate configuration...
                var res = await DoValidateConfig();
                if (res != 1)
                {
                    // Failed config.

                    // Add an error to the results...
                    this._resultsref.Add_ErrorResult(eResultPhase.Loading, "Step failed to validate config.", eObjectType.StepConfig, this.Id);

                    return -3;
                }

                success = true;

                // Set the loading disposition to good...
                this._resultsref?.Add_DispositionEntry(eResultPhase.Loading, eObjectType.Step, this.Id, eDisposition.Pass);

                // Set the ready state...
                this.UpdateState(eStepState.Ready);

                return 1;
            }
            catch (Exception ex)
            {
                this._resultsref.Add_ErrorResult(eResultPhase.Loading, "Exception during step load.", eObjectType.Step, Guid.Empty);

                return -4;
            }
            finally
            {
                // See if we loaded and validated...
                if(!success)
                {
                    // Failed to load and validate.

                    // Clear config and such...
                    this._cfg = null;

                    // Set the loading disposition to bad...
                    this._resultsref?.Add_DispositionEntry(eResultPhase.Loading, eObjectType.Step, this.Id, eDisposition.Fail);
                }

                // Signal the loading end...
                this._resultsref?.Add_EndEntry(eResultPhase.Loading, eObjectType.Step, id);
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
                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Step config failed validation.", eObjectType.StepConfig, this.Id);
                valpassed = false;
            }

            // Call the derived validate method...
            var res2 = await ValidateConfig();
            if(res2 != 1)
            {
                // Derived validate failed.

                // Add an error to the results...
                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Step config failed validation.", eObjectType.StepConfig, this.Id);
                valpassed = false;
            }

            // Return success if each check above passed...
            if (valpassed)
                return 1;
            else
                return -1;
        }

        /// <summary>
        /// Public method called by the sequencer, when wanting to execute a particular step.
        /// Returns 1 on success, 0 if user-cancelled, negatives are aborts for internal errors.
        /// </summary>
        /// <returns></returns>
        public async Task<int> Execute(CancellationToken token = default(CancellationToken))
        {
            try
            {
                // Signal the step start...
                this._resultsref?.Add_StartEntry(eResultPhase.Running, eObjectType.Step, this.Id);

                // Update running state...
                this.UpdateState(eStepState.Running);

                // Call the step-specific execute method...
                var res = await Local_Execute(token);
                if(res == 0)
                {
                    // Step was user-cancelled.

                    // Update to the cancelled state...
                    this.UpdateState(eStepState.Cancelled);

                    // Set step dispo...
                    this._resultsref?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Step, this.Id, eDisposition.Cancelled);

                    return 0;
                }
                if(res < 0)
                {
                    // Error occurred.
                    // We will consider the step as aborted.
                  
                    // Update to the aborted state...
                    this.UpdateState(eStepState.Aborted);

                    // Add an error...
                    this._resultsref?.Add_ErrorResult(eResultPhase.Running, "Step aborted on error.", eObjectType.Step, this.Id);

                    // Set step dispo...
                    this._resultsref?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Step, this.Id, eDisposition.Aborted);

                    return -1;
                }
                // Step completed.
                  
                // Update to completed...
                this.UpdateState(eStepState.Completed);

                // Set step dispo...
                this._resultsref?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Step, this.Id, eDisposition.Completed);

                return 1;
            }
            catch (OperationCanceledException oce)
            {
                // Execution was cancelled.

                // Update to the Cancelled state...
                this.UpdateState(eStepState.Cancelled);

                // Set step dispo...
                this._resultsref?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Step, this.Id, eDisposition.Cancelled);

                return 0;
            }
            catch (Exception ex)
            {
                // Exception occurred.
                // We will consider the step as aborted.
                  
                // Update to the aborted state...
                this.UpdateState(eStepState.Aborted);

                // Add an error...
                this._resultsref?.Add_ErrorResult(eResultPhase.Running, "Step aborted on exception.", eObjectType.Step, this.Id);

                // Set step dispo...
                this._resultsref?.Add_DispositionEntry(eResultPhase.Running, eObjectType.Step, this.Id, eDisposition.Aborted);

                return -2;
            }
            finally
            {
                // Signal the step end...
                this._resultsref?.Add_EndEntry(eResultPhase.Running, eObjectType.Step, this.Id);
            }
        }

        #endregion


        #region Private Methods

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

                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Step missing config.", eObjectType.StepConfig, this.Id);
                valpassed = false;
            }

            // Include any mandatory validation checks for all step types, here...

            // Return success if each check above passed...
            if (valpassed)
                return 1;
            else
                return -1;
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
            this._resultsref?.Add_ObjStateChange(eObjectType.Step, this.Id, oldstate.ToString(), this.State.ToString());
        }

        #endregion
    }
}
