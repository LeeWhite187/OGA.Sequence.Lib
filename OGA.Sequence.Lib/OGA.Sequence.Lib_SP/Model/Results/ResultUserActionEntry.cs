using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Holds user actions performed during sequence step execution.
    /// Create entries of this type if you wish to identify any captured user events, such as data entry, responses, button presses, etc.
    /// </summary>
    public class ResultUserActionEntry: ResultEntryBase
    {
        public string UserName { get; set; }
        public string ActionType { get; set; }
        public string Category { get; set; }
        public string Response { get; set; }

        override public eEntryType EntryType { get => eEntryType.UserAction; }


        override public string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToLogEntry());

            sb.AppendLine($"UserName = {(UserName ?? "")}");
            sb.AppendLine($"ActionType = {(ActionType ?? "")}");
            sb.AppendLine($"Category = {(Category ?? "")}");
            sb.AppendLine($"Response = {(Response ?? "")}");

            return sb.ToString();
        }
    }
}
