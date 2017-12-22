using Magnesium;

namespace Neutrino
{
    class EffectVariantEncoder
    {
        {
            uint finalPosition = (uint)def.FrontFace;
            finalPosition |= ((uint) def.CullMode) << 1;
            finalPosition |= ((uint)def.Topology) << 3;
            return finalPosition;
        }

        {
            const uint THREE_BITS = 7U;
            const uint TWO_BITS = 3U;
            const uint FOUR_BITS = 15U;

            {
                FrontFace = (MgFrontFace)(code & 1U),
                CullMode = (MgCullModeFlagBits)((code >> 1) & TWO_BITS),
                Topology = (MgPrimitiveTopology)((code >> 3) & FOUR_BITS),
            };
        }
    }

    {
        // [0, 1 bit]
        public MgFrontFace FrontFace { get; set; }

        // [1- 2, 2 bits]
        public MgCullModeFlagBits CullMode { get; set; }

        // [3 - 6, 4 bits]
        public MgPrimitiveTopology Topology { get; set; }
    }
}
