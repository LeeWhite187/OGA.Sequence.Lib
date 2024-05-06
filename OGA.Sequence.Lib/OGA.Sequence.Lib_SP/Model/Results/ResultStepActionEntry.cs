using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    /// <summary>
    /// Holds an action performed during a sequence step.
    /// Create entries of this type to record actions that a step performed, such as: setting a value, saving a file, etc...
    /// </summary>
    public class ResultStepActionEntry: ResultEntryBase
    {
        public string Name { get; set; }

        public string DataType { get; set; }
        /// <summary>
        /// Holds the action data.
        /// Can be simple string, or a json-serialized object.
        /// If json, set the DataType property, so the schema is known.
        /// </summary>
        public string Data { get; set; }

        override public eEntryType EntryType { get => eEntryType.StepAction; }

        override public string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.ToLogEntry());

            sb.AppendLine($"Name = {(Name ?? "")}");
            sb.AppendLine($"DataType = {(DataType ?? "")}");
            sb.AppendLine($"Data = {(Data ?? "")}");

            return sb.ToString();
        }
    }
}
