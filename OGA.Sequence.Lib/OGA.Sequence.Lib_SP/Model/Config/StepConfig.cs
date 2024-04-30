using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Steps
{
    /// <summary>
    /// Serializable config for a sequence step.
    /// </summary>
    public class StepConfig : ConfigBase
    {
        #region Properties

        /// <summary>
        /// Indicates the step is the last in the sequence.
        /// The sequence will end when this step completes.
        /// If set, the sequence will end at this step, regardless of any defined transitions.
        /// This can be used to short a sequence if needed.
        /// </summary>
        public bool IsTerminalStep { get; set; }

        /// <summary>
        /// Defines the type of step.
        /// </summary>
        public string StepType { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public StepConfig() : base()
        {
        }
    }
}
