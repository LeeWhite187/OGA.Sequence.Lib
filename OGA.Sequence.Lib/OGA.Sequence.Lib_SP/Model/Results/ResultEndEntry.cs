using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Indicates when a sequence, phase, or step started or ended.
    /// </summary>
    public class ResultEndEntry: ResultEntryBase
    {
        override public eEntryType EntryType { get => eEntryType.End; }
    }
}
