using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Steps
{
    /// <summary>
    /// Performs a simple delay.
    /// </summary>
    public class TaskStep_DelayType : TaskStep_abstract
    {
        private int _delaytime;

        static public string CONST_CONFIGPARM_DelayTime = "delaytime";

        #region Overrides

        /// <summary>
        /// Override this method with step logic implementation.
        /// Be sure to provide step disposition before exit, or the step will be considered aborted.
        /// Called to perform the step's function.
        /// Returns 1 on success, 0 if user-cancelled, negatives are aborts for internal errors.
        /// </summary>
        /// <returns></returns>
        override protected async Task<int> Local_Execute(CancellationToken token = default(CancellationToken))
        {
            try
            {
                // Delay if positive...
                if(this._delaytime > 0)
                {
                    // Wait for the configured delay time...
                    await Task.Delay(this._delaytime, token);
                }

                return 1;
            }
            catch (OperationCanceledException oce)
            {
                // Execution was cancelled.

                return 0;
            }
        }

        /// <summary>
        /// Define any config validation logic for config used by the step.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        override protected async Task<int> ValidateConfig()
        {
            bool valpassed = true;

            try
            {
                // Retrieve the delay time, and convert it for use...
                string tempval = this._cfg.Parameters[CONST_CONFIGPARM_DelayTime];
                int val = Convert.ToInt32(tempval);

                // Do a sanity check on it...
                if(val < 0)
                {
                    // Delay cannot be negative.

                    _resultsref?.Add_ErrorResult(eResultPhase.Loading, $"Step Config contains negative ${CONST_CONFIGPARM_DelayTime}.", eObjectType.Step, this.Id);
                    valpassed = false;
                }
                else
                {
                    this._delaytime = val;
                }
            }
            catch (Exception ex)
            {
                // Failed to retrieve config.

                _resultsref?.Add_ErrorResult(eResultPhase.Loading, $"Step Config Missing ${CONST_CONFIGPARM_DelayTime}.", eObjectType.Step, this.Id);
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
