using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Results
{
    public class ResultList
    {
        public eDisposition Disposition { get; set; }

        public List<ResultEntryBase> Entries { get; set; }


        public ResultList()
        {
            Entries = new List<ResultEntryBase>();
        }

        /// <summary>
        /// Call this method when a sequence object changes state.
        /// </summary>
        /// <param name="objtype"></param>
        /// <param name="objid"></param>
        /// <param name="oldstate"></param>
        /// <param name="newstate"></param>
        public void Add_ObjStateChange(eObjectType objtype, Guid objid, string oldstate, string newstate)
        {
            // Create a new result entry for the state change...
            var re = new ResultStateChangeEntry();
            re.Id = Guid.NewGuid();
            re.Name = "StateChange";
            re.Description = "";
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = eResultPhase.NA;

            re.Parameters.Add("oldstate", oldstate ?? "");
            re.Parameters.Add("newstate", newstate ?? "");

            this.Entries.Add(re);
        }

        /// <summary>
        /// Call this method when a sequence transition occurs.
        /// </summary>
        /// <param name="transid"></param>
        /// <param name="prevstepid"></param>
        /// <param name="nextstepid"></param>
        /// <param name="props"></param>
        public void Add_Transition(Guid transid, Guid prevstepid, Guid nextstepid, Dictionary<string, string> props = null)
        {
            // Create a new result entry for the state change...
            var re = new ResultStateChangeEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Transition";
            re.Description = "";
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = eObjectType.Transition;
            re.ObjId = transid;
            re.Phase = eResultPhase.Running;

            // Copy over any properties we were given...
            if(props != null)
            {
                foreach(var p in props)
                    re.Parameters.Add(p.Key, p.Value);
            }

            this.Entries.Add(re);
        }

        /// <summary>
        /// Call this method when a sequence, phase, or step starts.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="objtype"></param>
        /// <param name="objid"></param>
        public void Add_StartEntry(eResultPhase phase, eObjectType objtype, Guid objid)
        {
            // Create a new result entry for the start...
            var re = new ResultStartEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Started";
            re.Description = "";
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            this.Entries.Add(re);
        }
        /// <summary>
        /// Call this method when a sequence, phase, or step ends.
        /// </summary>
        /// <param name="objtype"></param>
        /// <param name="objid"></param>
        public void Add_EndEntry(eResultPhase phase, eObjectType objtype, Guid objid)
        {
            // Create a new result entry for the start...
            var re = new ResultEndEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Ended";
            re.Description = "";
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            this.Entries.Add(re);
        }
        public void Add_ErrorResult(eResultPhase phase, string msg, eObjectType objtype, Guid objid)
        {
            // Create a new result entry for the error...
            var re = new ResultErrorEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Error";
            re.Description = msg;
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            this.Entries.Add(re);
        }
        public void Add_ValidationError(string msg, eObjectType objtype, Guid objid)
        {
            // Create a new result entry for the error...
            var re = new ResultErrorEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Error";
            re.Description = msg;
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = eResultPhase.Validation;

            this.Entries.Add(re);
        }
        public void Add_CancellationEntry(eResultPhase phase, string msg, eObjectType objtype, Guid objid, string sourcetype, string sourceid)
        {
            // Create a new result entry for the error...
            var re = new ResultCancellationEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Cancellation";
            re.Description = msg;
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            re.SourceType = sourcetype;
            re.SourceId = sourceid;

            this.Entries.Add(re);
        }
        public void Add_DispositionEntry(eResultPhase phase, eObjectType objtype, Guid objid, eDisposition dispo)
        {
            // Create a new result entry for the error...
            var re = new ResultDispositionEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Disposition";
            re.Description = "";
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            re.Disposition = dispo;

            this.Entries.Add(re);
        }
        public void Add_OverallDispositionEntry(eObjectType objtype, Guid objid, eDisposition dispo)
        {
            // Create a new result entry for the error...
            var re = new ResultOverallDispositionEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Overall_Disposition";
            re.Description = "";
            re.DisplayOrder = this.GetNextDisplayOrder();
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = eResultPhase.Running;

            re.Disposition = dispo;

            this.Entries.Add(re);
        }

        /// <summary>
        /// Determines the next display order value to use.
        /// </summary>
        /// <returns></returns>
        private int GetNextDisplayOrder()
        {
            try
            {
                int res = this.Entries.Max(x => x.DisplayOrder);
                return res + 1;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }


        public string ToLogEntry()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var re in this.Entries)
            {
                sb.Append(re.ToLogEntry());
            }

            return sb.ToString();
        }
    }
}
