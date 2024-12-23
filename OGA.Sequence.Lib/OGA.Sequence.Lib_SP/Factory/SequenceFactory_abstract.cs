using OGA.Sequence.Model.Steps;
using OGA.Sequence.Model.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Factory
{
    /// <summary>
    /// Derive from this type for any local assembly factories.
    /// </summary>
    abstract public class SequenceFactory_abstract
    {
        protected string _classname;

        /// <summary>
        /// Override this method with an implementation for creating step instances in your local assembly.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        abstract public TaskStep_abstract CreateStepbyName(string typename);
#else
        abstract public TaskStep_abstract? CreateStepbyName(string typename);
#endif

        /// <summary>
        /// Override this method with an implementation for creating transition instances in your local assembly.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        abstract public Transition_abstract CreateTransitionbyName(string typename);
#else
        abstract public Transition_abstract? CreateTransitionbyName(string typename);
#endif
    }
}
