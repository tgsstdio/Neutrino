using Magnesium;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Neutrino
{
    public class PerVertexInputPipelineState
    {
        public uint VertexMask { get; }
        public MgPipelineVertexInputStateCreateInfo VertexInputState { get; }

        public PerVertexInputPipelineState(PerVertexDefinition definition)
        {
            VertexMask = PerVertexDefinitionEncoder.Encode(definition);
            VertexInputState = GenerateVertexInputState(definition);
        }

        private MgPipelineVertexInputStateCreateInfo GenerateVertexInputState(PerVertexDefinition definition)
        {
            PerVertexStrideInfo[] strides = new PerVertexStrideInfo[]
            {
                GetPositionStride(definition.Position), // 0 
                GetNormalStride(definition.Normal), // 1 
                GetTangentStride(definition.Tangent), // 2
                GetTexCoordsStride(definition.TexCoords0), // 3
                GetTexCoordsStride(definition.TexCoords1), // 4
                GetColorStride(definition.Color0), // 5
                GetJointsStride(definition.Joints0), // 6
                GetWeightsStride(definition.Weights0), // 7
                GetColorStride(definition.Color1), // 8
                GetJointsStride(definition.Joints1), // 9
                GetWeightsStride(definition.Weights1), // 10
            };

            var attributes = new List<MgVertexInputAttributeDescription>();
            var perVertexStride = 0U;
            var location = 0U;
            foreach (var stride in strides)
            {
                if (stride.Range > 0)
                {
                    var attribute = new MgVertexInputAttributeDescription
                    {
                        Binding = 0U,
                        Format = stride.Format,
                        Location = location,
                        Offset = perVertexStride,
                    };

                    location += 1;

                    attributes.Add(attribute);
                }
                perVertexStride += stride.Range;
            }

            var perInstanceStride = (uint)Marshal.SizeOf(typeof(PerInstance));
            IncludePerInstanceAttributes(attributes);

            return new MgPipelineVertexInputStateCreateInfo
            {
                VertexBindingDescriptions = new[]
                {
                    new MgVertexInputBindingDescription
                    {
                        Binding = 0U,
                        InputRate = MgVertexInputRate.VERTEX,
                        Stride = perVertexStride,
                    },
                    new MgVertexInputBindingDescription
                    {
                        Binding = 1U,
                        InputRate = MgVertexInputRate.INSTANCE,
                        Stride = perInstanceStride,
                    },
                },
                VertexAttributeDescriptions = attributes.ToArray(),
            };
        }

        private static void IncludePerInstanceAttributes(List<MgVertexInputAttributeDescription> attributes)
        {
            {
                var attribute = new MgVertexInputAttributeDescription
                {
                    Binding = 1U,
                    Format = MgFormat.R32G32B32_SFLOAT,
                    Location = 11,
                    Offset = (uint)Marshal.OffsetOf(typeof(PerInstance), "Translation"),
                };

                attributes.Add(attribute);
            }

            {
                var attribute = new MgVertexInputAttributeDescription
                {
                    Binding = 1U,
                    Format = MgFormat.R32G32B32_SFLOAT,
                    Location = 12,
                    Offset = (uint)Marshal.OffsetOf(typeof(PerInstance), "Scale"),
                };

                attributes.Add(attribute);
            }

            {
                var attribute = new MgVertexInputAttributeDescription
                {
                    Binding = 1U,
                    Format = MgFormat.R32G32B32A32_SFLOAT,
                    Location = 13,
                    Offset = (uint)Marshal.OffsetOf(typeof(PerInstance), "Rotation"),
                };

                attributes.Add(attribute);
            }

            {
                var attribute = new MgVertexInputAttributeDescription
                {
                    Binding = 1U,
                    Format = MgFormat.R32_SINT,
                    Location = 14,
                    Offset = (uint)Marshal.OffsetOf(typeof(PerInstance), "CameraIndex"),
                };

                attributes.Add(attribute);
            }

            {
                var attribute = new MgVertexInputAttributeDescription
                {
                    Binding = 1U,
                    Format = MgFormat.R32_SINT,
                    Location = 15,
                    Offset = (uint)Marshal.OffsetOf(typeof(PerInstance), "MaterialIndex"),
                };

                attributes.Add(attribute);
            }
        }

        private PerVertexStrideInfo GetTexCoordsStride(PerVertexTexCoordsType definition)
        {
            switch (definition)
            {
                case PerVertexTexCoordsType.None:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.UNDEFINED,
                        Range = 0U,
                    };
                default:
                case PerVertexTexCoordsType.ByteUnorm2:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R8G8_UNORM,
                        Range = 2U * sizeof(byte),
                    };
                case PerVertexTexCoordsType.ByteUnorm3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R8G8B8_UNORM,
                        Range = 3U * sizeof(byte),
                    };
                case PerVertexTexCoordsType.Half2:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16_SFLOAT,
                        Range = 2U * sizeof(UInt16),
                    };
                case PerVertexTexCoordsType.Half3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16_SFLOAT,
                        Range = 3U * sizeof(UInt16),
                    };

                case PerVertexTexCoordsType.UshortUnorm2:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16_UNORM,
                        Range = 2U * sizeof(UInt16),
                    };
                case PerVertexTexCoordsType.UshortUnorm3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16_UNORM,
                        Range = 3U * sizeof(UInt16),
                    };
                case PerVertexTexCoordsType.Float2:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R32G32_SFLOAT,
                        Range = 2U * sizeof(float),
                    };
                case PerVertexTexCoordsType.Float3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R32G32B32_SFLOAT,
                        Range = 3U * sizeof(float),
                    };
            }
        }

        private PerVertexStrideInfo GetWeightsStride(PerVertexWeightsType definition)
        {
            switch (definition)
            {
                case PerVertexWeightsType.None:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.UNDEFINED,
                        Range = 0U,
                    };
                case PerVertexWeightsType.Float4:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R32G32B32A32_SFLOAT,
                        Range = 4U * sizeof(float),
                    };
                case PerVertexWeightsType.UshortUnorm4:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16A16_UNORM,
                        Range = 4U * sizeof(UInt16),
                    };
                default:
                case PerVertexWeightsType.ByteUnorm4:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R8G8B8A8_UNORM,
                        Range = 4U * sizeof(byte),
                    };
            }
        }

        private PerVertexStrideInfo GetJointsStride(PerVertexJointType definition)
        {
            switch (definition)
            {
                case PerVertexJointType.None:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.UNDEFINED,
                        Range = 0U,
                    };
                default:
                case PerVertexJointType.Byte4:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R8G8B8A8_UINT,
                        Range = 4U * sizeof(byte),
                    };
                case PerVertexJointType.Ushort4:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16A16_UINT,
                        Range = 4U * sizeof(UInt16),
                    };
            }
        }

        private PerVertexStrideInfo GetColorStride(PerVertexColorType definition)
        {
            switch (definition)
            {
                case PerVertexColorType.ByteUnormRGB:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R8G8B8_UNORM,
                        Range = 3U * sizeof(byte),
                    };
                case PerVertexColorType.ByteUnormRGBA:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R8G8B8A8_UNORM,
                        Range = 4U * sizeof(byte),
                    };
                case PerVertexColorType.UshortUnormRGB:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16_UNORM,
                        Range = 3U * sizeof(UInt16),
                    };
                case PerVertexColorType.UshortUnormRGBA:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16A16_UNORM,
                        Range = 4U * sizeof(UInt16),
                    };
                case PerVertexColorType.FloatRGB:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16_SFLOAT,
                        Range = 3U * sizeof(float),
                    };
                default:
                case PerVertexColorType.FloatRGBA:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16A16_SFLOAT,
                        Range = 4U * sizeof(float),
                    };
            }
        }

        private PerVertexStrideInfo GetTangentStride(PerVertexTangentType definition)
        {
            switch (definition)
            {
                case PerVertexTangentType.Half4:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16A16_SFLOAT,
                        Range = 4U * sizeof(UInt16),
                    };
                case PerVertexTangentType.None:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.UNDEFINED,
                        Range = 0U,
                    };
                default:
                case PerVertexTangentType.Float4:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R32G32B32A32_SFLOAT,
                        Range = 4U * sizeof(float),
                    };
            }
        }

        private static PerVertexStrideInfo GetPositionStride(PerVertexPositionType definition)
        {
            switch (definition)
            {
                case PerVertexPositionType.Float2:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R32G32B32A32_SFLOAT,
                        Range = 2U * sizeof(float),
                    };
                case PerVertexPositionType.Half2:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16_SFLOAT,
                        Range = 2U * sizeof(UInt16),
                    };
                case PerVertexPositionType.Half3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16_SFLOAT,
                        Range = 3U * sizeof(UInt16),
                    };
                case PerVertexPositionType.None:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.UNDEFINED,
                        Range = 0U,
                    };
                default:
                case PerVertexPositionType.Float3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R32G32B32_SFLOAT,
                        Range = 3U * sizeof(float),
                    };
            }
        }

        public static PerVertexStrideInfo GetNormalStride(PerVertexNormalType definition)
        {
            switch (definition)
            {
                case PerVertexNormalType.Half3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R16G16B16_SFLOAT,
                        Range = 3U * sizeof(UInt16),
                    };
                case PerVertexNormalType.None:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.UNDEFINED,
                        Range = 0U,
                    };
                default:
                case PerVertexNormalType.Float3:
                    return new PerVertexStrideInfo
                    {
                        Format = MgFormat.R32G32B32_SFLOAT,
                        Range = 3U * sizeof(float),
                    };
            }
        }
    }
}
