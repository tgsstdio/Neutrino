using Magnesium;

namespace Neutrino
{
    public struct EffectVariantOptions
    {
        // [0, 1 bit]
        public MgFrontFace FrontFace { get; set; }

        // [1- 2, 2 bits]
        public MgCullModeFlagBits CullMode { get; set; }

        // [3 - 6, 4 bits]
        public MgPrimitiveTopology Topology { get; set; }
    }
}
