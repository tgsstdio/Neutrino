using Magnesium;

namespace Neutrino
{
    class EffectVariantEncoder
    {
        public static uint Encode(EffectVariantOptions def)
        {
            uint finalPosition = (uint)def.FrontFace;
            finalPosition |= ((uint) def.CullMode) << 1;
            finalPosition |= ((uint)def.Topology) << 3;
            return finalPosition;
        }

        public static EffectVariantOptions Decode(uint code)
        {
            const uint THREE_BITS = 7U;
            const uint TWO_BITS = 3U;
            const uint FOUR_BITS = 15U;

            return new EffectVariantOptions
            {
                FrontFace = (MgFrontFace)(code & 1U),
                CullMode = (MgCullModeFlagBits)((code >> 1) & TWO_BITS),
                Topology = (MgPrimitiveTopology)((code >> 3) & FOUR_BITS),
            };
        }
    }

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
