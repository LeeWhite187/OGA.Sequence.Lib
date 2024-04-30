using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Result entry type that contains an error that occurred during setup, validation, or running.
    /// </summary>
    public class ResultErrorEntry: ResultEntryBase
    {
        override public eEntryType EntryType { get => eEntryType.Error; }
    }
}
