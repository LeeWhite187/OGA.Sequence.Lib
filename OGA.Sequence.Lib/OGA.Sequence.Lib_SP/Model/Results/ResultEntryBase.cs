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
        public DateTime EntryTimeUTC { get; set; }

        public virtual eEntryType EntryType { get => eEntryType.Base; }

        public Guid ObjId { get; set; }
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
