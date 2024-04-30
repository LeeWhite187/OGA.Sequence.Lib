using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Steps
{
    /// <summary>
    /// First step executed in a sequence.
    /// </summary>
    public class TaskStep_SequenceStart : TaskStep_abstract
    {
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
            return 1;
        }

        /// <summary>
        /// Define any config validation logic for config used by the step.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        override protected async Task<int> ValidateConfig()
        {
            bool valpassed = true;

            // Do validation checks...

            // Return success if each check above passed...
            if (valpassed)
                return 1;
            else
                return -1;
        }

        #endregion
    }
}
