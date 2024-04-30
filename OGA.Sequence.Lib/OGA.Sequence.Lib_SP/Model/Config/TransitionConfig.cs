using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Steps
{
    /// <summary>
    /// Serializable config for a sequence transition.
    /// </summary>
    public class TransitionConfig : ConfigBase
    {
        #region Properties

        /// <summary>
        /// Id of the step the transition is assigned to.
        /// </summary>
        public Guid StepId { get; set; }

        /// <summary>
        /// Id of the next step to execute.
        /// </summary>
        public Guid NextStepId { get; set; }

        /// <summary>
        /// Defines the type of transition.
        /// </summary>
        public string TransitionType { get; set; }

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public TransitionConfig() : base()
        {
        }
    }
}
