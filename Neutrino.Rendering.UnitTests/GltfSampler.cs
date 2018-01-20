using Magnesium;

namespace Neutrino.UnitTests
{
    public class GltfSampler
    {
        public MgSamplerAddressMode AddressModeU { get; set; }
        public MgSamplerAddressMode AddressModeV { get; set; }
        public MgFilter MinFilter { get; set;  }
        public MgFilter MagFilter { get; set;  }
        public MgSamplerMipmapMode MipmapMode { get; set; }
    }
    
}
