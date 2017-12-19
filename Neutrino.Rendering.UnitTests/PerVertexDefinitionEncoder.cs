namespace Neutrino.UnitTests
{
    public class PerVertexDefinitionEncoder
    {
        public static uint Encode(PerVertexDefinition def)
        {
            uint finalResult = ((uint)def.Position);
            finalResult |= ((uint)def.Normal) << 3;
            finalResult |= ((uint)def.Tangents) << 5;
            finalResult |= ((uint)def.TexCoords0) << 7;
            finalResult |= ((uint)def.TexCoords1) << 12;
            finalResult |= ((uint)def.Color0) << 16;
            finalResult |= ((uint)def.Color1) << 19;
            finalResult |= ((uint)def.Joints0) << 22;
            finalResult |= ((uint)def.Joints1) << 24;
            finalResult |= ((uint)def.Weights0) << 26;
            finalResult |= ((uint)def.Weights1) << 28;
            finalResult |= ((uint)def.IndexType) << 30;
            return finalResult;
        }

        public static PerVertexDefinition Decode(uint code)
        {
            const uint THREE_BITS = 7U;
            const uint TWO_BITS = 3U;
            const uint FOUR_BITS = 15U;
            return new PerVertexDefinition
            {
                Position = (PerVertexPositionType)(code & THREE_BITS),
                Normal = (PerVertexNormalType)((code >> 3) & TWO_BITS),
                Tangents = (PerVertexTangentType)((code >> 5) & TWO_BITS),
                TexCoords0 = (PerVertexTexCoordsType)((code >> 7) & 15U),
                TexCoords1 = (PerVertexTexCoordsType)((code >> 12) & FOUR_BITS),
                Color0 = (PerVertexColorType)((code >> 16) & THREE_BITS),
                Color1 = (PerVertexColorType)((code >> 19) & THREE_BITS),
                Joints0 = (PerVertexJointType)((code >> 22) & TWO_BITS),
                Joints1 = (PerVertexJointType)((code >> 24) & TWO_BITS),
                Weights0 = (PerVertexWeightsType)((code >> 26) & TWO_BITS),
                Weights1 = (PerVertexWeightsType)((code >> 28) & TWO_BITS),
                IndexType = (PerVertexIndexType)((code >> 30) & TWO_BITS),
            };
        }

    }
    
}
