using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OGA.Sequence.Model.Steps;
using OGA.Sequence.Model;
using OGA.Sequence.Model.Results;

namespace OGA.Sequence.Model.Transitions
{
    abstract public class Transition_abstract
    {
        #region Private Fields

        protected TransitionConfig _cfg;

        protected ResultList _resultsref;

        #endregion


        #region Public Properties

        /// <summary>
        /// Unique identifier of the transition.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display order of the transition in a listing.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Friendly name of the transition.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Friendly description for the transition.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Id of the step the transition is assigned to.
        /// </summary>
        public Guid StepId { get; set; }

        /// <summary>
        /// Id of the next step to execute.
        /// </summary>
        public Guid NextStepId { get; set; }

        /// <summary>
        /// Current state of the transition.
        /// This is used to determine when each is ready for execution.
        /// </summary>
        public eTransitionState State { get; protected set; }

        #endregion


        #region ctor / dtor

        public Transition_abstract()
        {
            State = eTransitionState.Unknown;
        }

        #endregion


        #region Overrides

        /// <summary>
        /// Define any config validation logic for config used by the transition.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        abstract protected Task<int> ValidateConfig();

        /// <summary>
        /// Called to evaluate if the transition is true or false.
        /// Returns 1 if true, 0 if false, negatives are aborts for internal errors.
        /// </summary>
        /// <param name="currentstep"></param>
        /// <returns></returns>
        abstract public Task<int> IsTrue(TaskStep_abstract currentstep);

        #endregion


        #region Public Methods

        /// <summary>
        /// Loads config into the transition.
        /// Will validate configuration after loading.
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public async Task<int> Load(TransitionConfig cfg, ResultList results)
        {
            bool success = false;

            // Make an early determination of our Id in case we need it for logging...
            Guid id = (cfg == null ? Guid.Empty : cfg.Id);

            try
            {
                // Accept the results listing...
                this._resultsref = results;

                // Signal the loading start...
                this._resultsref?.Add_StartEntry(eResultPhase.Loading, eObjectType.Transition, id);

                // Do some quick and dirty validation...
                {
                    if(cfg == null)
                    {
                        // Config is null.
                        _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Transition config is null.", eObjectType.TransitionConfig, id);
                        return -1;
                    }
                    if(cfg.Id == Guid.Empty)
                    {
                        // Transition Id is empty.
                        this._resultsref.Add_ErrorResult(eResultPhase.Loading, "Transition config contains blank sequence Id.", eObjectType.TransitionConfig, id);
                        return -2;
                    }
                }

                // Accept given config so we can test it...
                this._cfg = cfg;

                // Set the local properties...
                {
                    this.Name = this._cfg.Name;
                    this.Description = this._cfg.Description;
                    this.Id = this._cfg.Id;

                    this.StepId = this._cfg.StepId;
                    this.NextStepId = this._cfg.NextStepId;
                    this.DisplayOrder = this._cfg.DisplayOrder;
                }

                // Validate configuration...
                var res = await DoValidateConfig();
                if (res != 1)
                {
                    // Failed config.

                    // Add an error to the results...
                    this._resultsref.Add_ErrorResult(eResultPhase.Loading, "Transition failed to validate config.", eObjectType.TransitionConfig, this.Id);

                    return -3;
                }

                success = true;

                // Set the loading disposition to good...
                this._resultsref?.Add_DispositionEntry(eResultPhase.Loading, eObjectType.Transition, this.Id, eDisposition.Pass);

                // Set the ready state...
                this.UpdateState(eTransitionState.Ready);

                return 1;
            }
            catch (Exception ex)
            {
                this._resultsref.Add_ErrorResult(eResultPhase.Loading, "Exception during transition load.", eObjectType.Transition, Guid.Empty);

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
                    this._resultsref?.Add_DispositionEntry(eResultPhase.Loading, eObjectType.Transition, this.Id, eDisposition.Fail);
                }

                // Signal the loading end...
                this._resultsref?.Add_EndEntry(eResultPhase.Loading, eObjectType.Transition, id);
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
                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Transition config failed validation.", eObjectType.TransitionConfig, this.Id);
                valpassed = false;
            }

            // Call the derived validate method...
            var res2 = await ValidateConfig();
            if(res2 != 1)
            {
                // Derived validate failed.

                // Add an error to the results...
                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Transition config failed validation.", eObjectType.TransitionConfig, this.Id);
                valpassed = false;
            }

            // Return success if each check above passed...
            if (valpassed)
                return 1;
            else
                return -1;
        }

        #endregion


        #region Status Updates

        protected void UpdateState(eTransitionState state)
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

                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Transition missing config.", eObjectType.TransitionConfig, this.Id);
                valpassed = false;
            }
            if(this._cfg.StepId == Guid.Empty)
            {
                // Source step is blank.

                // Add an error to the results...
                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Transition missing source step.", eObjectType.TransitionConfig, this.Id);
                valpassed = false;
            }
            if(this._cfg.NextStepId == Guid.Empty)
            {
                // Target step is blank.

                // Add an error to the results...
                _resultsref?.Add_ErrorResult(eResultPhase.Loading, "Transition missing target step.", eObjectType.TransitionConfig, this.Id);
                valpassed = false;
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
