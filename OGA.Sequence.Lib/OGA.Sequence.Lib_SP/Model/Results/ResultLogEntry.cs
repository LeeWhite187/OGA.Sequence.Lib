using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Holds log messages generated during sequence step execution.
    /// Create entries of this type if you wish to aggregate logs with result data.
    /// </summary>
    public class ResultLogEntry: ResultEntryBase
    {
        public string Severity { get; set; }

        public string Class { get; set; }

        public int LineNo { get; set; }

        public string Message { get; set; }

        override public eEntryType EntryType { get => eEntryType.LogMessage; }

        override public string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToLogEntry());

            sb.AppendLine($"Severity = {(Severity ?? "")}");
            sb.AppendLine($"Class = {(Class ?? "")}");
            sb.AppendLine($"LineNo = {(LineNo.ToString())}");
            sb.AppendLine($"Message = {(Message ?? "")}");

            return sb.ToString();
        }
    }
}
