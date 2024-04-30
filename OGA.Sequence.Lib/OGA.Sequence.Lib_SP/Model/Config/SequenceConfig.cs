using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Steps
{
    /// <summary>
    /// Serializable config for a sequence.
    /// </summary>
    public class SequenceConfig : ConfigBase
    {
        #region Properties

        /// <summary>
        /// List of steps.
        /// </summary>
        public List<StepConfig> StepList { get; set; }

        /// <summary>
        /// List of transitions.
        /// </summary>
        public List<TransitionConfig> TransitionList { get; set; }

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public SequenceConfig() : base()
        {
            StepList = new List<StepConfig>();
            TransitionList = new List<TransitionConfig>();
        }

        public StepConfig AddStep(string steptype)
        {
            var ss = new StepConfig();
            ss.Id = Guid.NewGuid();
            ss.DisplayOrder = GetNextStepDisplayOrder();
            ss.StepType = steptype;

            this.StepList.Add(ss);

            return ss;
        }

        public TransitionConfig AddTransition(string transtype, Guid sourcestepid, Guid targetstepid)
        {
            var tr = new TransitionConfig();
            tr.TransitionType = transtype;
            tr.Id = Guid.NewGuid();
            tr.DisplayOrder = GetNextTransDisplayOrder();
            tr.StepId = sourcestepid;
            tr.NextStepId = targetstepid;

            this.TransitionList.Add(tr);

            return tr;
        }


        private int GetNextStepDisplayOrder()
        {
            try
            {
                var max = this.StepList.Max(x => x.DisplayOrder);
                return max + 1;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        private int GetNextTransDisplayOrder()
        {
            try
            {
                var max = this.TransitionList.Max(x => x.DisplayOrder);
                return max + 1;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
    }
}
