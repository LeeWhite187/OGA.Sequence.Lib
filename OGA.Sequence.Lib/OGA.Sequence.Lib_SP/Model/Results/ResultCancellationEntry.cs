using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Result entry type that indicates when a cancellation occurred.
    /// This occurs when a user or external agent cancels a sequence.
    /// </summary>
    public class ResultCancellationEntry: ResultEntryBase
    {
        public string SourceType { get; set; }
        public string SourceId { get; set; }

        override public eEntryType EntryType { get => eEntryType.Cancellation; }

        override public string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToLogEntry());

            sb.AppendLine($"SourceType = {(SourceType ?? "")}");
            sb.AppendLine($"SourceId = {(SourceId ?? "")}");

            return sb.ToString();
        }
    }
}
