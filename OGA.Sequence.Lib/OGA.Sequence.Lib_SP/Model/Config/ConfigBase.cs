using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Model.Steps
{
    /// <summary>
    /// Serializable config for a sequence object.
    /// </summary>
    public class ConfigBase
    {
        #region Properties

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Friendly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Friendly description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Display order in a listing.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Parameter listing for the object.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public ConfigBase()
        {
            Parameters = new Dictionary<string, string>();
        }
    }
}
