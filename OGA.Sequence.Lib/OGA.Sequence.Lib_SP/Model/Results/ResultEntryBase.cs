using OGA.Sequence.Model.Config;
using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    public class ResultEntryBase : ConfigBase
    {
        /// <summary>
        /// Time of entry.
        /// Set during creation.
        /// </summary>
        public DateTime EntryTimeUTC { get; set; }

        /// <summary>
        /// Indicates the result type.
        /// </summary>
        public virtual eEntryType EntryType { get => eEntryType.Base; }

        /// <summary>
        /// Relevant resource id that generated the result entry.
        /// </summary>
        public Guid ObjId { get; set; }
        /// <summary>
        /// Type of resource, creating the entry.
        /// </summary>
        public eObjectType ObjType { get; set; }

        /// <summary>
        /// Associated the result entry with loading, validation, runtime, reporting, etc...
        /// </summary>
        public eResultPhase Phase { get; set; }


        public ResultEntryBase() : base()
        {

        }

        public virtual string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{EntryTimeUTC.ToString("HH:mm:ss")}|{EntryType.ToString()}|{Phase.ToString()}|{ObjType.ToString()}|{ObjId.ToString()}");

            sb.AppendLine(ParmsToLogEntry());

            return sb.ToString();
        }

        public string ParmsToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            if(this.Parameters != null && this.Parameters.Count != 0)
            {
                foreach(var p in this.Parameters)
                    sb.AppendLine($"{(p.Key ?? "")} = {(p.Value ?? "")}");
            }

            return sb.ToString();
        }
    }
}
