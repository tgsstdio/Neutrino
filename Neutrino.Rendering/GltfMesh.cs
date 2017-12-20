using glTFLoader.Schema;
using Magnesium;
using System;
using System.Collections.Generic;

namespace Neutrino
{
    public class GltfMesh
    {
        public string Name { get; set; }
        public GltfMeshPrimitive[] Primitives { get; private set; }

        public GltfMesh(Mesh mesh, GltfAccessor[] accessors, GltfBufferView[] bufferViews)
        {
            Name = mesh.Name;
            InitializePrimitives(mesh, accessors);
        }

        private void InitializePrimitives(Mesh mesh, GltfAccessor[] accessors)
        {
            var noOfItems = mesh.Primitives != null ? mesh.Primitives.Length : 0;
            Primitives = new GltfMeshPrimitive[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                var pri = mesh.Primitives[i];

                var result = new GltfMeshPrimitive
                {
                    Indices = pri.Indices,
                    Topology = DetermineTopology(pri.Mode),
                    Material = pri.Material,
                };

                var perVertexData = new List<int>();

                var definition = new PerVertexDefinition
                {
                    IndexType = ExtractIndexType(pri.Indices, accessors),

                    Position = ExtractPosition(pri.Attributes, accessors, perVertexData),
                    Normal = ExtractNormals(pri.Attributes, accessors, perVertexData),
                    Tangents = ExtractTangents(pri.Attributes, accessors, perVertexData),
                    Color0 = ExtractColor("COLOR_0", pri.Attributes, accessors, perVertexData),
                    Color1 = ExtractColor("COLOR_1", pri.Attributes, accessors, perVertexData),
                    TexCoords0 = ExtractTexCoords("TEXCOORDS_0", pri.Attributes, accessors, perVertexData),
                    TexCoords1 = ExtractTexCoords("TEXCOORDS_1", pri.Attributes, accessors, perVertexData),
                    Joints0 = ExtractJoints("JOINTS_0", pri.Attributes, accessors, perVertexData),
                    Joints1 = ExtractJoints("JOINTS_1", pri.Attributes, accessors, perVertexData),
                    Weights0 = ExtractWeights("WEIGHTS_0", pri.Attributes, accessors, perVertexData),
                    Weights1 = ExtractWeights("WEIGHTS_1", pri.Attributes, accessors, perVertexData),
                };

                result.Definition = PerVertexDefinitionEncoder.Encode(definition);
                result.VertexData = perVertexData.ToArray();

                Primitives[i] = result;
            }
        }

        private PerVertexJointType ExtractJoints(string name, Dictionary<string, int> attributes, GltfAccessor[] accessors, List<int> perVertexData)
        {
            if (attributes.TryGetValue(name, out int accessorIndex))
            {
                var selected = accessors[accessorIndex];

                bool isInvalid = true;
                var result = PerVertexJointType.None;

                if (selected.ElementType == GltfElementType.Byte)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexJointType.Byte4;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == GltfElementType.UshortNorm)
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

                perVertexData.Add(accessorIndex);
                return result;
            }
            else
            {
                return PerVertexJointType.None;
            }
        }

        private PerVertexTexCoordsType ExtractTexCoords(string name, Dictionary<string, int> attributes, GltfAccessor[] accessors, List<int> perVertexData)
        {
            if (attributes.TryGetValue(name, out int accessorIndex))
            {
                var selected = accessors[accessorIndex];

                bool isInvalid = true;
                var result = PerVertexTexCoordsType.None;

                if (selected.ElementType == GltfElementType.Float)
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
                else if (selected.ElementType == GltfElementType.ByteNorm)
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
                else if (selected.ElementType == GltfElementType.UshortNorm)
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
                else if (selected.ElementType == GltfElementType.Half)
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

                perVertexData.Add(accessorIndex);
                return result;
            }
            else
            {
                return PerVertexTexCoordsType.None;
            }
        }

        private PerVertexColorType ExtractColor(string name, Dictionary<string, int> attributes, GltfAccessor[] accessors, List<int> perVertexData)
        {
            if (attributes.TryGetValue(name, out int accessorIndex))
            {
                var selected = accessors[accessorIndex];

                bool isInvalid = true;
                var result = PerVertexColorType.None;

                if (selected.ElementType == GltfElementType.Float)
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
                else if (selected.ElementType == GltfElementType.ByteNorm)
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
                else if (selected.ElementType == GltfElementType.UshortNorm)
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

                perVertexData.Add(accessorIndex);
                return result;
            }
            else
            {
                return PerVertexColorType.None;
            }
        }

        private PerVertexWeightsType ExtractWeights(string name, Dictionary<string, int> attributes, GltfAccessor[] accessors, List<int> perVertexData)
        {
            if (attributes.TryGetValue(name, out int accessorIndex))
            {
                var selected = accessors[accessorIndex];

                bool isInvalid = true;
                var result = PerVertexWeightsType.None;

                if (selected.ElementType == GltfElementType.Float)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexWeightsType.Float4;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == GltfElementType.ByteNorm)
                {
                    if (selected.NoOfComponents == 4)
                    {
                        result = PerVertexWeightsType.ByteUnorm4;
                        isInvalid = false;
                    }
                }
                else if (selected.ElementType == GltfElementType.UshortNorm)
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

                perVertexData.Add(accessorIndex);
                return result;
            }
            else
            {
                return PerVertexWeightsType.None;
            }
        }

        private PerVertexTangentType ExtractTangents(Dictionary<string, int> attributes, GltfAccessor[] accessors, List<int> perVertexData)
        {
            if (attributes.TryGetValue("TANGENT", out int accessorIndex))
            {
                var selected = accessors[accessorIndex];

                var result = PerVertexTangentType.None;
                if (selected.ElementType == GltfElementType.Float && selected.NoOfComponents == 4)
                {
                    result = PerVertexTangentType.Float4;
                }
                else if (selected.ElementType == GltfElementType.Half && selected.NoOfComponents == 4)
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

                perVertexData.Add(accessorIndex);
                return result;                
            }
            else
            {
                return PerVertexTangentType.None;
            }
        }

        #region ExtractNormals methods

        private PerVertexNormalType ExtractNormals(Dictionary<string, int> attributes, GltfAccessor[] accessors, List<int> perVertexData)
        {
            if (attributes.TryGetValue("NORMAL", out int accessorIndex))
            {
                var selected = accessors[accessorIndex];

                var result = PerVertexNormalType.None;
                if (selected.ElementType == GltfElementType.Float && selected.NoOfComponents == 3)
                {
                    result = PerVertexNormalType.Float3;
                }
                else if (selected.ElementType == GltfElementType.Half && selected.NoOfComponents == 3)
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

                perVertexData.Add(accessorIndex);
                return result;
            }
            else
            {
                return PerVertexNormalType.None;
            }
        }

        #endregion

        #region ExtractIndices methods

        private static PerVertexIndexType ExtractIndexType(int? indices, GltfAccessor[] accessors)
        {
            if (indices.HasValue)
            {
                var selected = accessors[indices.Value];
                if (selected.ElementType == GltfElementType.Uint)
                {
                    return PerVertexIndexType.Uint32;
                }
                else if (selected.ElementType == GltfElementType.Ushort)
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

        #endregion

        #region ExtractPosition methods

        private static PerVertexPositionType ExtractPosition(
            Dictionary<string, int> attributes,
            GltfAccessor[] accessors,
            List<int> vertexData)
        {
            if (attributes.TryGetValue("POSITION", out int accessorIndex))
            {
                var selected = accessors[accessorIndex];

                bool isInvalid = true;
                var result = PerVertexPositionType.None;
                if (selected.ElementType == GltfElementType.Float)
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
                else if (selected.ElementType == GltfElementType.Half)
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

                vertexData.Add(accessorIndex);
                return result;
            }
            else
            {
                return PerVertexPositionType.None;
            }
        }

        #endregion

        private static MgPrimitiveTopology DetermineTopology(MeshPrimitive.ModeEnum mode)
        {
            switch (mode) {
                case MeshPrimitive.ModeEnum.LINES:
                    return MgPrimitiveTopology.LINE_LIST;
                case MeshPrimitive.ModeEnum.LINE_STRIP:
                    return MgPrimitiveTopology.LINE_STRIP;
                case MeshPrimitive.ModeEnum.POINTS:
                    return MgPrimitiveTopology.POINT_LIST;
                case MeshPrimitive.ModeEnum.TRIANGLES:
                    return MgPrimitiveTopology.TRIANGLE_LIST;
                case MeshPrimitive.ModeEnum.TRIANGLE_FAN:
                    return MgPrimitiveTopology.TRIANGLE_FAN;
                case MeshPrimitive.ModeEnum.TRIANGLE_STRIP:
                    return MgPrimitiveTopology.TRIANGLE_STRIP;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
