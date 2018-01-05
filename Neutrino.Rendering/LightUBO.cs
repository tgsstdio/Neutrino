using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutrino.Library
{
    class LightUBO
    {
        public TkVector4 LightColor { get; set; }
        public TkVector4 LightDirection { get; set; }
    }
}
