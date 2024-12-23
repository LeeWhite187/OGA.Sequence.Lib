using OGA.Sequence.Model.Steps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OGA.Sequence.Model.Results
{
    public class ResultList
    {
        private int _lastentryid;

        #region Public Properties

        public eDisposition Disposition { get; set; }

        public List<ResultEntryBase> Entries { get; set; }

        /// <summary>
        /// Array list of result types that will trigger the result output delegate.
        /// By default, is set to the operational states: Cancellation, StepAction, ConsoleOutput, Log, Disposition, OverallDisposition, Error.
        /// </summary>
        public eEntryType[] Cfg_ResultTypes_toNotify { get; set; } =
                new eEntryType[]
                {
                    //eEntryType.StateChange,
                    eEntryType.Transition,
                    eEntryType.Start,
                    eEntryType.StepAction,
                    eEntryType.ConsoleOutput,
                    eEntryType.LogMessage,
                    eEntryType.End,
                    eEntryType.Error,
                    eEntryType.Cancellation,
                    eEntryType.Disposition,
                    eEntryType.OverallDisposition
                };

        /// <summary>
        /// Array list of sequence phases where results will notify.
        /// By default, set to the Running phase.
        /// </summary>
        public eResultPhase[] Cfg_ResultPhase_toNotify { get; set; } =
                new eResultPhase[]
                {
                    //eResultPhase.Loading,
                    //eResultPhase.Validation,
                    eResultPhase.Running,
                    //eResultPhase.Reporting
                };

        #endregion


        #region Public Delegates

        public delegate void delResultEntryAdded(ResultEntryBase entry);

        private delResultEntryAdded _delOnResultEntryAdded;
        /// <summary>
        /// Assign a handler to this delegate to receive result entries as they occur.
        /// </summary>
        public delResultEntryAdded OnResultEntryAdded
        {
            set
            {
                this._delOnResultEntryAdded = value;
            }
        }

        #endregion


        #region ctor / dtor

        public ResultList()
        {
            Entries = new List<ResultEntryBase>();
        }

        #endregion


        #region Add Methods

        /// <summary>
        /// Call this to add step actions.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="datatype"></param>
        /// <param name="data"></param>
        public void Add_StepActionEntry(string name, string datatype, Guid objid, string data)
        {
            // Create a new result entry for the step action message...
            var re = new ResultStepActionEntry();
            re.Id = Guid.NewGuid();
            re.Name = "StepAction";
            re.Description = "";
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = eObjectType.Step;
            re.ObjId = objid;
            re.Phase = eResultPhase.Running;

            re.Name = name ?? "";
            re.DataType = datatype ?? "";
            re.Data = data ?? "";

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }

        /// <summary>
        /// Call this to aggregate user actions.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="actiontype"></param>
        /// <param name="response"></param>
        /// <param name="category"></param>
        public void Add_UserActionEntry(string username, string actiontype, string response, string category = "")
        {
            // Create a new result entry for the user action message...
            var re = new ResultUserActionEntry();
            re.Id = Guid.NewGuid();
            re.Name = "UserAction";
            re.Description = "";
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = eObjectType.Console;
            re.ObjId = Guid.Empty;
            re.Phase = eResultPhase.Running;

            re.UserName = username ?? "";
            re.ActionType = actiontype ?? "";
            re.Response = response ?? "";
            re.Category = category ?? "";

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }

        /// <summary>
        /// Call this to aggregate log messages with result data.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="severity"></param>
        /// <param name="classname"></param>
        /// <param name="lineno"></param>
        public void Add_LogEntry(string msg, string severity = "Info", string classname = "", int? lineno = 0)
        {
            // Create a new result entry for the log message...
            var re = new ResultLogEntry();
            re.Id = Guid.NewGuid();
            re.Name = "LogEntry";
            re.Description = msg;
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = eObjectType.Console;
            re.ObjId = Guid.Empty;
            re.Phase = eResultPhase.Running;

            re.Severity = severity ?? "";
            re.Class = classname ?? "";
            re.LineNo = lineno ?? 0;
            re.Message = msg ?? "";

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }

        /// <summary>
        /// Call this for console output, that occurs outside a step action.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="iserror"></param>
        public void Add_ConsoleOutputEntry(string msg, bool iserror = false)
        {
            // Create a new result entry for the console entry...
            var re = new ResultConsoleOutputEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Console";
            re.Description = msg;
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = eObjectType.Console;
            re.ObjId = Guid.Empty;
            re.Phase = eResultPhase.Running;

            re.Message = msg ?? "";
            re.IsError = iserror;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }

        /// <summary>
        /// Call this for step logic that outputs to the console.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="stepid"></param>
        /// <param name="iserror"></param>
        public void Add_StepConsoleOutputEntry(string msg, Guid stepid, bool iserror = false)
        {
            // Create a new result entry for the console entry...
            var re = new ResultConsoleOutputEntry();
            re.Id = Guid.NewGuid();
            re.Name = "StepConsole";
            re.Description = msg;
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = eObjectType.Step;
            re.ObjId = stepid;
            re.Phase = eResultPhase.Running;

            re.Message = msg ?? "";
            re.IsError = iserror;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
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
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = eResultPhase.NA;

            re.Parameters.Add("oldstate", oldstate ?? "");
            re.Parameters.Add("newstate", newstate ?? "");

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
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

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
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
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
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
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }
        public void Add_ErrorResult(eResultPhase phase, string msg, eObjectType objtype, Guid objid)
        {
            // Create a new result entry for the error...
            var re = new ResultErrorEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Error";
            re.Description = msg;
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }
        public void Add_ValidationError(string msg, eObjectType objtype, Guid objid)
        {
            // Create a new result entry for the error...
            var re = new ResultErrorEntry();
            re.Id = Guid.NewGuid();
            re.Name = "ValidationError";
            re.Description = msg;
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = eResultPhase.Validation;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }
        public void Add_CancellationEntry(eResultPhase phase, string msg, eObjectType objtype, Guid objid, string sourcetype, string sourceid)
        {
            // Create a new result entry for the error...
            var re = new ResultCancellationEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Cancellation";
            re.Description = msg;
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            re.SourceType = sourcetype;
            re.SourceId = sourceid;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }
        public void Add_DispositionEntry(eResultPhase phase, eObjectType objtype, Guid objid, eDisposition dispo)
        {
            // Create a new result entry for the error...
            var re = new ResultDispositionEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Disposition";
            re.Description = "";
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = phase;

            re.Disposition = dispo;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }
        public void Add_OverallDispositionEntry(eObjectType objtype, Guid objid, eDisposition dispo)
        {
            // Create a new result entry for the error...
            var re = new ResultOverallDispositionEntry();
            re.Id = Guid.NewGuid();
            re.Name = "Overall_Disposition";
            re.Description = "";
            re.Parameters = new Dictionary<string, string>();

            re.EntryTimeUTC = DateTime.UtcNow;
            re.ObjType = objtype;
            re.ObjId = objid;
            re.Phase = eResultPhase.Running;

            re.Disposition = dispo;

            this.priv_AddEntry(re);

            this.Fire_OnResultEntryAdded(re);
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Called by the sequence logic when it needs to clear result data.
        /// This may be called when the sequence enters the running phase, to clear out any loading and validation entries that are only needed for diagnostic purposes.
        /// </summary>
        public void ClearEntries()
        {
            lock(this.Entries)
            {
                this.Entries.Clear();
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

        #endregion


        #region Delegate Calls

        /// <summary>
        /// Calls the external delegate on its own thread, to prevent stalling sequence progression.
        /// </summary>
        /// <param name="entry"></param>
        private void Fire_OnResultEntryAdded(ResultEntryBase entry)
        {
            if (entry == null)
                return;

            if (this.Cfg_ResultPhase_toNotify == null || this.Cfg_ResultPhase_toNotify.Length == 0)
                return;

            // We will only notify for result entries for the desired phase and entry types...
            if (!this.Cfg_ResultPhase_toNotify.Contains(entry.Phase))
                return;
            if (!this.Cfg_ResultTypes_toNotify.Contains(entry.EntryType))
                return;

            _ = Task.Run(() =>
            {
                // Wrap the delegate in a try-catch to ensure any exception it may throw won't hit the task scheduler and create an unhandled exception.
                try
                {
                    if(this._delOnResultEntryAdded != null)
                    {
                        try
                        {
                            this._delOnResultEntryAdded(entry);
                        } catch(Exception ex) { }
                    }
                }
                catch (Exception ex) { }
            });
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Wraps the actual result entry addition, to ensure each entry gets a unique order id, and protects the listing from cross-thread updates.
        /// </summary>
        /// <param name="re"></param>
        private void priv_AddEntry(ResultEntryBase re)
        {
            lock(this.Entries)
            {
                this._lastentryid++;
                re.DisplayOrder = this._lastentryid;
                this.Entries.Add(re);
            }
        }

        #endregion
    }
}
