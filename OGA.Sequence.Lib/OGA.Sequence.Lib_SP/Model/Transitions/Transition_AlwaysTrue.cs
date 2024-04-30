using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OGA.Sequence.Model.Steps;
using OGA.Sequence.Model.Transitions;

namespace OGA.Sequence.Model.Transitions
{
    public class Transition_AlwaysTrue : Transition_abstract
    {
        #region Overrides

        /// <summary>
        /// Define any config validation logic for config used by the transition.
        /// Return 1 if good.
        /// </summary>
        /// <returns></returns>
        override protected async Task<int> ValidateConfig()
        {
            return 1;
        }

        /// <summary>
        /// Called to evaluate if the transition is true or false.
        /// Returns 1 if true, 0 if false, negatives are aborts for internal errors.
        /// </summary>
        /// <param name="currentstep"></param>
        /// <returns></returns>
        override public async Task<int> IsTrue(TaskStep_abstract currentstep)
        {
            return 1;
        }

        #endregion
    }
}
