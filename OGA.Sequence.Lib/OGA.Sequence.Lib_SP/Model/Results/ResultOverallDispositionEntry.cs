using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Special result entry type that indicates the disposition of a step, phase, or the overall sequence.
    /// </summary>
    public class ResultOverallDispositionEntry: ResultEntryBase
    {
        public eDisposition Disposition { get; set; }

        override public eEntryType EntryType { get => eEntryType.OverallDisposition; }

        override public string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToLogEntry());

            sb.AppendLine($"Disposition = {(Disposition.ToString())}");

            return sb.ToString();
        }
    }
}
