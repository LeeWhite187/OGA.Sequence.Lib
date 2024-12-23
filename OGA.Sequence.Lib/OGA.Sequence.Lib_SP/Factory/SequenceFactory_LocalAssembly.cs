using OGA.Sequence.Model.Transitions;
using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Factory
{
    /// <summary>
    /// Can generate sequence objects by name, for object types in this assembly.
    /// </summary>
    [SequenceFactory]
    public class SequenceFactory_LocalAssembly : SequenceFactory_abstract
    {
        public SequenceFactory_LocalAssembly ()
        {
            this._classname = nameof(SequenceFactory_LocalAssembly);
        }

        /// <summary>
        /// Override this method with an implementation for creating step instances in your local assembly.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        override public TaskStep_abstract CreateStepbyName(string typename)
#else
        override public TaskStep_abstract? CreateStepbyName(string typename)
#endif
        {
            if (string.IsNullOrEmpty(typename))
                return null;

            var searchterm = typename.ToLower();

            TaskStep_abstract obj = null;

            // Create types for the local assembly...
            if(searchterm == nameof(TaskStep_DelayType).ToLower())
                obj = new TaskStep_DelayType();
            else if(searchterm == nameof(TaskStep_NoActionType).ToLower())
                obj = new TaskStep_NoActionType();
            else if(searchterm == nameof(TaskStep_SequenceStart).ToLower())
                obj = new TaskStep_SequenceStart();
            else
                return null;

            return obj;
        }

        /// <summary>
        /// Override this method with an implementation for creating transition instances in your local assembly.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        override public Transition_abstract CreateTransitionbyName(string typename)
#else
        override public Transition_abstract? CreateTransitionbyName(string typename)
#endif
        {
            if (string.IsNullOrEmpty(typename))
                return null;

            var searchterm = typename.ToLower();

            Transition_abstract obj = null;

            // Create types for the local assembly...
            if(searchterm == nameof(Transition_AlwaysTrue).ToLower())
                obj = new Transition_AlwaysTrue();
            else
                return null;

            return obj;
        }
    }
}
