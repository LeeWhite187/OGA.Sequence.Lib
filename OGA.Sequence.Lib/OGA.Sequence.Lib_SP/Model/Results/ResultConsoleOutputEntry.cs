using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Holds console output generated during sequence step execution.
    /// Create entries of this type if you wish to publish conosole output with result data.
    /// </summary>
    public class ResultConsoleOutputEntry: ResultEntryBase
    {
        public string Message { get; set; }

        public bool IsError { get; set; }

        override public eEntryType EntryType { get => eEntryType.ConsoleOutput; }

        override public string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToLogEntry());

            sb.AppendLine($"Message = {(Message ?? "")}");
            sb.AppendLine($"IsError = {(IsError.ToString())}");

            return sb.ToString();
        }
    }
}
