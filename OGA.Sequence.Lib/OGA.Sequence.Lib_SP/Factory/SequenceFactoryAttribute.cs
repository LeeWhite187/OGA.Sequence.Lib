using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Factory
{
    /// <summary>
    /// Decorate any Sequence Factories with this attribute so they can be discovered during process start and initialization.
    /// </summary>
    public class SequenceFactoryAttribute : Attribute
    {
    }
}
