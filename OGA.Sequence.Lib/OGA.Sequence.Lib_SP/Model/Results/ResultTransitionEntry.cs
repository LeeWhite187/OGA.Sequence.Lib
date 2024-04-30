using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Indicates when a sequence changes step via transition.
    /// Contains the transitionId, and relevant properties that caused the transition.
    /// </summary>
    public class ResultTransitionEntry : ResultEntryBase
    {

        override public eEntryType EntryType { get => eEntryType.Transition; }
    }
}
