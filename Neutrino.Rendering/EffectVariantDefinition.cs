using Magnesium;

namespace Neutrino
{
    public class EffectVariantEncoder
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
}
