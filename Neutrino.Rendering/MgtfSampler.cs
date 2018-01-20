using Magnesium;

namespace Neutrino
{
    public class MgtfSampler
    {
        public MgSamplerAddressMode AddressModeU { get; set; }
        public MgSamplerAddressMode AddressModeV { get; set; }
        public MgFilter MinFilter { get; set;  }
        public MgFilter MagFilter { get; set;  }
        public MgSamplerMipmapMode MipmapMode { get; set; }
    }
    
}
