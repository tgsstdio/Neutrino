using System;

namespace Neutrino
{
    public class PerVertexDefinitionEncoder
    {
        public static uint Encode(PerVertexDefinition def)
        {
            uint finalResult = ((uint)def.Position);
            finalResult |= ((uint)def.Normal) << 3;
            finalResult |= ((uint)def.Tangent) << 5;
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
                Tangent = (PerVertexTangentType)((code >> 5) & TWO_BITS),
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


        public static PerVertexDefinition Extract(IMgtfPerVertexDataLocator locator, MgtfAccessor[] accessors)
        {
            return new PerVertexDefinition
            {
                IndexType = ExtractIndexType(locator.Indices, accessors),
                Position = ExtractPosition(locator, accessors),
                Normal = ExtractNormals(locator, accessors),
                Tangent = ExtractTangents(locator, accessors),
                TexCoords0 = ExtractTexCoords(locator.TexCoords0, accessors),
                TexCoords1 = ExtractTexCoords(locator.TexCoords1, accessors),
                Weights0 = ExtractWeights(locator.Weights0, accessors),
                Weights1 = ExtractWeights(locator.Weights1, accessors),
                Color0 = ExtractColor(locator.Color0, accessors),
                Color1 = ExtractColor(locator.Color1, accessors),
                Joints0 = ExtractJoints(locator.Joints0, accessors),
                Joints1 = ExtractJoints(locator.Joints1, accessors),
            };
        }

        private static PerVertexIndexType ExtractIndexType(int? indices,
            MgtfAccessor[] accessors)
        {
            if (indices.HasValue)
            {
                var selected = accessors[indices.Value];
                if (selected.ElementType == MgtfElementType.Uint)
                {
                    return PerVertexIndexType.Uint32;
                }
                else if (selected.ElementType == MgtfElementType.Ushort)
                {
                    return PerVertexIndexType.Uint16;
                }

                throw new NotSupportedException(
                    string.Format(
                        "Indices format not support : {0}",
                        selected.ElementType)
                );
            }
            else
            {
                return PerVertexIndexType.None;
            }
        }

        private static PerVertexPositionType ExtractPosition(
            IMgtfPerVertexDataLocator locator,
            MgtfAccessor[] accessors)
        {
            if (locator.Position.HasValue)
            {
                var selected = accessors[locator.Position.Value];

                bool isInvalid = true;
                var result = PerVertexPositionType.None;
                if (selected.ElementType == MgtfElementType.Float)
                {
                    if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexPositionType.Float3;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 2)
                    {
                        result = PerVertexPositionType.Float2;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.Half)
                {
                    if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexPositionType.Half3;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 2)
                    {
                        result = PerVertexPositionType.Half2;
                        isInvalid = false;
                    }
                }

                if (isInvalid)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Format not support : {0}",
                            selected.ElementType)
                    );
                }

                return result;
            }
            else
            {
                return PerVertexPositionType.None;
            }
        }

        private static PerVertexNormalType ExtractNormals(IMgtfPerVertexDataLocator locator,
            MgtfAccessor[] accessors)
        {
            if (locator.Normal.HasValue)
            {
                var selected = accessors[locator.Normal.Value];
                var result = PerVertexNormalType.None;
                if (selected.ElementType == MgtfElementType.Float && selected.NoOfComponents == 3)
                {
                    result = PerVertexNormalType.Float3;
                }
                else if (selected.ElementType == MgtfElementType.Half && selected.NoOfComponents == 3)
                {
                    result = PerVertexNormalType.Half3;
                }
                else
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Format not support : {0}",
                            selected.ElementType)
                    );
                }

                return result;
            }
            else
            {
                return PerVertexNormalType.None;
            }
        }

        private static PerVertexTangentType ExtractTangents(IMgtfPerVertexDataLocator locator,
            MgtfAccessor[] accessors)
        {
            if (locator.Tangent.HasValue)
            {
                var selected = accessors[locator.Tangent.Value];

                var result = PerVertexTangentType.None;
                if (selected.ElementType == MgtfElementType.Float && selected.NoOfComponents == 4)
                {
                    result = PerVertexTangentType.Float4;
                }
                else if (selected.ElementType == MgtfElementType.Half && selected.NoOfComponents == 4)
                {
                    result = PerVertexTangentType.Half4;
                }
                else
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Format not support : {0}",
                            selected.ElementType)
                    );
                }
                return result;
            }
            else
            {
                return PerVertexTangentType.None;
            }
        }

        private static PerVertexTexCoordsType ExtractTexCoords(int? texCoords,
            MgtfAccessor[] accessors)
        {
            if (texCoords.HasValue)
            {
                var selected = accessors[texCoords.Value];

                bool isInvalid = true;
                var result = PerVertexTexCoordsType.None;

                if (selected.ElementType == MgtfElementType.Float)
                {
                    if (selected.NoOfComponents == 2)
                    {
                        result = PerVertexTexCoordsType.Float2;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexTexCoordsType.Float3;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.ByteNorm)
                {
                    if (selected.NoOfComponents == 2)
                    {
                        result = PerVertexTexCoordsType.ByteUnorm2;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexTexCoordsType.ByteUnorm3;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.UshortNorm)
                {
                    if (selected.NoOfComponents == 2)
                    {
                        result = PerVertexTexCoordsType.UshortUnorm2;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexTexCoordsType.UshortUnorm3;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.Half)
                {
                    if (selected.NoOfComponents == 2)
                    {
                        result = PerVertexTexCoordsType.Half2;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexTexCoordsType.Half3;
                        isInvalid = false;
                    }
                }

                if (isInvalid)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Format not support : {0}",
                            selected.ElementType)
                    );
                }

                return result;
            }
            else
            {
                return PerVertexTexCoordsType.None;
            }
        }

        private static PerVertexWeightsType ExtractWeights(int? weights,
            MgtfAccessor[] accessors)
        {
            if (weights.HasValue)
            {
                var selected = accessors[weights.Value];

                bool isInvalid = true;
                var result = PerVertexWeightsType.None;

                if (selected.ElementType == MgtfElementType.Float)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexWeightsType.Float4;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.ByteNorm)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexWeightsType.ByteUnorm4;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.UshortNorm)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexWeightsType.UshortUnorm4;
                        isInvalid = false;
                    }
                }

                if (isInvalid)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Format not support : {0}",
                            selected.ElementType)
                    );
                }

                return result;
            }
            else
            {
                return PerVertexWeightsType.None;
            }
        }

        private static PerVertexColorType ExtractColor(int? color,
            MgtfAccessor[] accessors)
        {
            if (color.HasValue)
            {
                var selected = accessors[color.Value];

                bool isInvalid = true;
                var result = PerVertexColorType.None;

                if (selected.ElementType == MgtfElementType.Float)
                {
                    if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexColorType.FloatRGB;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexColorType.FloatRGBA;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.ByteNorm)
                {
                    if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexColorType.ByteUnormRGB;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexColorType.ByteUnormRGBA;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.UshortNorm)
                {
                    if (selected.NoOfComponents == 3)
                    {
                        result = PerVertexColorType.UshortUnormRGB;
                        isInvalid = false;
                    }
                    else if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexColorType.UshortUnormRGBA;
                        isInvalid = false;
                    }
                }

                if (isInvalid)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Format not support : {0}",
                            selected.ElementType)
                    );
                }

                return result;
            }
            else
            {
                return PerVertexColorType.None;
            }
        }

        private static PerVertexJointType ExtractJoints(int? joints,
            MgtfAccessor[] accessors)
        {
            if (joints.HasValue)
            {
                var selected = accessors[joints.Value];

                bool isInvalid = true;
                var result = PerVertexJointType.None;

                if (selected.ElementType == MgtfElementType.Byte)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexJointType.Byte4;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == MgtfElementType.UshortNorm)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexJointType.Ushort4;
                        isInvalid = false;
                    }
                }

                if (isInvalid)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Format not support : {0}",
                            selected.ElementType)
                    );
                }

                return result;
            }
            else
            {
                return PerVertexJointType.None;
            }
        }
    }
    
}
