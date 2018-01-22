using glTFLoader.Schema;
using Magnesium;
using System;

namespace Neutrino
{
    public class Loader
    {
        public MgtfModelMetaFile LoadMetaData(Gltf model)
        {
            var accessors = ExtractAccessors(model.Accessors);

            return new MgtfModelMetaFile
            {
                Accessors = accessors,
                BufferViews = ExtractBufferViews(model.BufferViews),
                Cameras = ExtractCameras(model.Cameras),
                Materials = ExtractMaterials(model.Materials),
                Meshes = ExtractMeshes(model.Meshes, accessors),
                Nodes = ExtractNodes(model.Nodes),
                Samplers = ExtractSamplers(model.Samplers),
                Textures = ExtractTextures(model.Textures),
            };
        }

        #region ExtractNodes methods 

        private static MgtfNode[] ExtractNodes(Node[] nodes)
        {
            var noOfNodes = nodes != null ? nodes.Length : 0;
            var allNodes = new MgtfNode[noOfNodes];

            for (var i = 0; i < noOfNodes; i += 1)
            {
                allNodes[i] = ExtractNewNode(nodes[i]);
            }

            RelinkParents(allNodes);
            return allNodes;
        }

        private static MgtfNode ExtractNewNode(Node src)
        {
            return new MgtfNode
            {
                Name = src.Name,
                Camera = src.Camera,
                Children = src.Children ?? (new int[] { }),
                Transform = GenerateTransform(src),
                Mesh = src.Mesh,
            };
        }

        private static TkMatrix4 GenerateTransform(Node srcNode)
        {
            if (srcNode.Matrix != null && srcNode.Matrix.Length == 16)
            {
                var src = srcNode.Matrix;
                return new TkMatrix4(
                    src[0], src[1], src[2], src[3],
                    src[4], src[5], src[6], src[7],
                    src[8], src[9], src[10], src[11],
                    src[12], src[13], src[14], src[15]
                );
            }
            else
            {
                var firstOp =
                    (srcNode.Scale != null && srcNode.Scale.Length == 3)
                    ? TkMatrix4.CreateScale(srcNode.Scale[0], srcNode.Scale[1], srcNode.Scale[2])
                    : TkMatrix4.CreateScale(1, 1, 1);

                var quat = new TkQuaternion(0f, 0f, 0f, 1f);

                if (srcNode.Rotation != null && srcNode.Rotation.Length == 4)
                {
                    quat = new TkQuaternion(
                        srcNode.Rotation[0],
                        srcNode.Rotation[1],
                        srcNode.Rotation[2],
                        srcNode.Rotation[3]);
                }

                var secondOp = TkMatrix4.CreateFromQuaternion(quat);

                var thirdOp =
                    (srcNode.Translation != null && srcNode.Translation.Length == 3)
                    ? TkMatrix4.CreateTranslation(srcNode.Translation[0], srcNode.Translation[1], srcNode.Translation[2])
                    : TkMatrix4.CreateTranslation(0, 0, 0);

                // T * (R * S)
                TkMatrix4.Mult(ref secondOp, ref firstOp, out TkMatrix4 result);
                return TkMatrix4.Mult(thirdOp, result);
            }
        }


        public static void RelinkParents(MgtfNode[] allNodes)
        {
            for (var i = 0; i < allNodes.Length; i += 1)
            {                     
                var parent = allNodes[i];
                
                var noOfChildren = parent.Children != null ? parent.Children.Length : 0;

                if (parent.Children != null)
                {
                    foreach (var childIndex in parent.Children)
                    {
                        var child = allNodes[childIndex];
                        child.Parent = i;
                    }
                }
            }
        }

        #endregion

        #region ExtractAccessors methods

        private static MgtfAccessor[] ExtractAccessors(Accessor[] accessors)
        {
            var noOfItems = accessors != null ? accessors.Length : 0;
            var output = new MgtfAccessor[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = ExtractNewAccessor(accessors[i]);
            }

            return output;
        }

        private static MgtfAccessor ExtractNewAccessor(Accessor srcAccessor)
        {
            var elementByteSize = DetermineByteSize(srcAccessor.ComponentType);
            var noOfComponents = DetermineNoOfComponents(srcAccessor.Type);
            var totalByteSize = (ulong)(elementByteSize * noOfComponents * srcAccessor.Count);

            return new MgtfAccessor
            {
                BufferView = srcAccessor.BufferView,
                ViewOffset = srcAccessor.ByteOffset,
                Format = DetermineFormat(
                    srcAccessor.Type,
                    srcAccessor.ComponentType),
                ElementType = DetermineElementType(srcAccessor.ComponentType),
                ElementByteSize = elementByteSize,
                NoOfComponents = noOfComponents,
                ElementCount = srcAccessor.Count,
                TotalByteSize = totalByteSize,
            };
        }

        private static GltfElementType DetermineElementType(Accessor.ComponentTypeEnum componentType)
        {
            switch (componentType)
            {
                case Accessor.ComponentTypeEnum.BYTE:
                    return GltfElementType.SByte;
                case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    return GltfElementType.Byte;
                case Accessor.ComponentTypeEnum.SHORT:
                    return GltfElementType.Short;
                case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    return GltfElementType.Ushort;
                case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    return GltfElementType.Uint;
                case Accessor.ComponentTypeEnum.FLOAT:
                    return GltfElementType.Float;
                default:
                    throw new NotSupportedException();
            }
        }

        private static uint DetermineByteSize(Accessor.ComponentTypeEnum componentType)
        {
            switch (componentType)
            {
                case Accessor.ComponentTypeEnum.BYTE:
                    return 1U;
                case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    return 1U;
                case Accessor.ComponentTypeEnum.SHORT:
                    return 2U;
                case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    return 2U;
                case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    return 4U;
                case Accessor.ComponentTypeEnum.FLOAT:
                    return 4U;
                default:
                    throw new NotSupportedException();
            }
        }

        private static uint DetermineNoOfComponents(Accessor.TypeEnum type)
        {
            switch (type)
            {
                case Accessor.TypeEnum.SCALAR:
                    return 1U;
                case Accessor.TypeEnum.VEC2:
                    return 2U;
                case Accessor.TypeEnum.VEC3:
                    return 3U;
                case Accessor.TypeEnum.VEC4:
                    return 4U;
                case Accessor.TypeEnum.MAT2:
                    return 4U;
                case Accessor.TypeEnum.MAT3:
                    return 9U;
                case Accessor.TypeEnum.MAT4:
                    return 16U;
                default:
                    throw new NotSupportedException();
            }
        }

        private static MgFormat DetermineFormat(Accessor.TypeEnum type, Accessor.ComponentTypeEnum componentType)
        {
            if (type == Accessor.TypeEnum.SCALAR)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else if (type == Accessor.TypeEnum.VEC2)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8G8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8G8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32G32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16G16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16G16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32G32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else if (type == Accessor.TypeEnum.VEC3)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8G8B8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8G8B8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32G32B32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16G16B16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16G16B16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32G32B32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else if (type == Accessor.TypeEnum.VEC4)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8G8B8A8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8G8B8A8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32G32B32A32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16G16B16A16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16G16B16A16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32G32B32A32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

        }

        #endregion

        #region ExtractBufferViews methods

        private static MgtfBufferView[] ExtractBufferViews(BufferView[] bufferViews)
        {
            var noOfItems = bufferViews != null ? bufferViews.Length : 0;
            var output = new MgtfBufferView[noOfItems];
            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = ExtractBufferView(bufferViews[i]);
            }
            return output;
        }

        private static MgtfBufferView ExtractBufferView(BufferView src)
        {
            return new MgtfBufferView
            {
                BufferIndex = src.Buffer,
                ByteStride = src.ByteStride,
                BufferOffset = src.ByteOffset,
                ByteLength = src.ByteLength,
                Usage = DetrimineUsage(src),
            };
        }

        private static MgBufferUsageFlagBits DetrimineUsage(BufferView src)
        {
            switch (src.Target)
            {
                case glTFLoader.Schema.BufferView.TargetEnum.ARRAY_BUFFER:
                    return MgBufferUsageFlagBits.VERTEX_BUFFER_BIT;
                case glTFLoader.Schema.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER:
                    return MgBufferUsageFlagBits.INDEX_BUFFER_BIT;
                default:
                    throw new NotSupportedException($"specified target:({src.Target}) not supported");
            }
        }

        #endregion

        #region ExtractCameras methods 

        private static MgtfCamera[] ExtractCameras(Camera[] cameras)
        {
            var count = cameras != null ? cameras.Length : 0;

            var output = new MgtfCamera[count];
            for (var i = 0; i < count; i += 1)
            {
                output[i] = ExtractNewCamera(cameras[i]);
            }
            return output;
        }

        private static MgtfCamera ExtractNewCamera(Camera camera)
        {
            double m_0_0 = 0;
            double m_1_1 = 0;
            double m_2_2 = 0;
            double m_2_3 = 0f;
            double m_3_2 = 0f;
            double m_3_3 = 0f;
            float? aspectRatio = null;

            if (camera.Type == Camera.TypeEnum.orthographic)
            {
                m_0_0 = camera.Orthographic.Xmag;
                m_1_1 = 1 / camera.Orthographic.Ymag;

                var a = camera.Orthographic.Znear - camera.Orthographic.Zfar;
                var b = camera.Orthographic.Zfar + camera.Orthographic.Znear;

                m_2_2 = 2 / a;
                m_2_3 = b / a;

                m_3_2 = 0;
                m_3_3 = 1;
            }
            else
            {
                aspectRatio = camera.Perspective.AspectRatio;

                var q = Math.Tan(0.5 * camera.Perspective.Yfov);

                m_0_0 = q;
                m_1_1 = 1 / q;
                m_3_2 = -1;
                m_3_3 = 0;

                if (camera.Perspective.Zfar.HasValue)
                {
                    var r = (double) camera.Perspective.Zfar.Value;
                    var s = camera.Perspective.Znear - r;
                    var t = r + camera.Perspective.Znear;
                    var u = 2 * r * camera.Perspective.Znear;

                    // FINITE 
                    m_2_2 = t / s;
                    m_2_3 = u / s;
                }
                else
                {
                    // INFINITE
                    m_2_2 = -1;
                    m_2_3 = -2 * camera.Perspective.Znear;
                }
            }

            var values = new double
            []
            {
                m_0_0,
                m_1_1,
                m_2_2,
                m_2_3,
                m_3_2,
                m_3_3,
            };


            return new MgtfCamera
            {
                ProjectionType = (camera.Type == Camera.TypeEnum.orthographic)
                    ? MgtfCameraType.Orthogonal
                    : MgtfCameraType.Perspective,
                AspectRatio = aspectRatio,
                Values = values,
            };
        }

        #endregion

        #region ExtractMaterials methods 

        private static MgtfMaterial[] ExtractMaterials(Material[] materials)
        {
            var count = materials != null ? materials.Length : 0;

            var output = new MgtfMaterial[count];
            for (var i = 0; i < count; i += 1)
            {
                output[i] = ExtractNewMaterial(materials[i]);
            }
            return output;
        }

        public static MgtfMaterial ExtractNewMaterial(Material src)
        {
            MgtfMaterialTexture BaseColorTexture = null;
            var baseColorFactor = new MgVec4f(1f, 1f, 1f, 1f);
            if (src.PbrMetallicRoughness != null)
            {
                baseColorFactor = new MgVec4f
                {
                    X = src.PbrMetallicRoughness.BaseColorFactor[0],
                    Y = src.PbrMetallicRoughness.BaseColorFactor[1],
                    Z = src.PbrMetallicRoughness.BaseColorFactor[2],
                    W = src.PbrMetallicRoughness.BaseColorFactor[3],
                };

                if (src.PbrMetallicRoughness.BaseColorTexture != null)
                {
                    BaseColorTexture = new MgtfMaterialTexture
                    {
                        Texture = src.PbrMetallicRoughness.BaseColorTexture.Index,
                        TexCoords = src.PbrMetallicRoughness.BaseColorTexture.TexCoord,
                    };
                }
            }

            MgtfMaterialTexture NormalTexture = null;
            float normalScale = 1f;
            if (src.NormalTexture != null)
            {
                NormalTexture = new MgtfMaterialTexture
                {
                    Texture = src.NormalTexture.Index,
                    TexCoords = src.NormalTexture.TexCoord,
                };
                normalScale = src.NormalTexture.Scale;
            }

            MgtfMaterialTexture EmissiveTexture = null;
            if (src.EmissiveTexture != null)
            {
                EmissiveTexture = new MgtfMaterialTexture
                {
                    Texture = src.EmissiveTexture.Index,
                    TexCoords = src.EmissiveTexture.TexCoord,
                };
            }

            var emissiveFactor = new Color3f { R = 0f, G = 0f, B = 0f };
            if (src.EmissiveFactor != null)
            {
                emissiveFactor = new Color3f
                {
                    R = src.EmissiveFactor[0],
                    G = src.EmissiveFactor[1],
                    B = src.EmissiveFactor[2]
                };
            }

            MgtfMaterialTexture OcclusionTexture = null;
            float strength = 1f;
            if (src.OcclusionTexture != null)
            {
                OcclusionTexture = new MgtfMaterialTexture
                {
                    Texture = src.OcclusionTexture.Index,
                    TexCoords = src.OcclusionTexture.TexCoord,
                };
                strength = src.OcclusionTexture.Strength;
            }

            return new MgtfMaterial
            {
                DoubleSided = src.DoubleSided,
                AlphaCutoff = src.AlphaCutoff,
                AlphaMode = ExtractAlphaMode(src.AlphaMode),
                BaseColorTexture = BaseColorTexture,
                BaseColorFactor = baseColorFactor,
                NormalTexture = NormalTexture,
                NormalScale = normalScale,
                EmissiveTexture = EmissiveTexture,
                EmissiveFactor = emissiveFactor,
                OcclusionTexture = OcclusionTexture,
                OcclusionStrength = strength,
            };
        }


        private static MgtfAlphaModeEquation ExtractAlphaMode(Material.AlphaModeEnum alphaMode)
        {
            switch (alphaMode)
            {
                case Material.AlphaModeEnum.BLEND:
                    return new MgtfAlphaModeEquation
                    {
                        A = 0f,
                        B = 0f,
                        C = 1f,
                    };
                case Material.AlphaModeEnum.MASK:
                    return new MgtfAlphaModeEquation
                    {
                        A = 0f,
                        B = 1f,
                        C = 0f,
                    };
                case Material.AlphaModeEnum.OPAQUE:
                    return new MgtfAlphaModeEquation
                    {
                        A = 1f,
                        B = 0f,
                        C = 1f,
                    };
                default:
                    throw new NotSupportedException();
            }
        }


        #endregion

        #region ExtractMeshes methods 

        private static MgtfMesh[] ExtractMeshes(Mesh[] meshes, MgtfAccessor[] accessors)
        {
            var noOfItems = meshes != null ? meshes.Length : 0;
            var output = new MgtfMesh[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = ExtractNewMesh(meshes[i], accessors);
            }
            return output;
        }

        private static MgtfMesh ExtractNewMesh(Mesh mesh, MgtfAccessor[] accessors)
        {
            return new MgtfMesh
            {
                Name = mesh.Name,
                Weights = mesh.Weights,
                Primitives = InitializePrimitives(mesh, accessors),
            };
        }

        private static MgtfMeshPrimitive[] InitializePrimitives(
            Mesh mesh,
            MgtfAccessor[] accessors)
        {
            var noOfItems = mesh.Primitives != null ? mesh.Primitives.Length : 0;
            var output = new MgtfMeshPrimitive[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                var srcPrimitive = mesh.Primitives[i];

                var vertexLocations = new PerVertexDataLocator(
                    srcPrimitive.Indices,
                    srcPrimitive.Attributes);
                var definition = PerVertexDefinitionEncoder.Extract(
                    vertexLocations,
                    accessors);

                var temp = new MgtfMeshPrimitive
                {
                    Topology = DetermineTopology(srcPrimitive.Mode),
                    VertexLocations = vertexLocations,
                    InitialDefinition = definition,
                    Material = srcPrimitive.Material,
                    VertexCount =
                        vertexLocations.Position.HasValue
                        ? (uint)accessors[vertexLocations.Position.Value].ElementCount
                        : 0U
                };

                output[i] = temp;
            }

            return output;
        }

        private static MgPrimitiveTopology DetermineTopology(MeshPrimitive.ModeEnum mode)
        {
            switch (mode)
            {
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

        #endregion

        #region ExtractSamplers methods

        private static MgtfSampler[] ExtractSamplers(Sampler[] samplers)
        {
            var noOfSamplers = samplers != null ? samplers.Length : 0;

            var output = new MgtfSampler[noOfSamplers];
            for (var i = 0; i < noOfSamplers; i += 1)
            {
                output[i] = ExtractNewSampler(samplers[i]);
            }
            return output;
        }

        private static MgtfSampler ExtractNewSampler(Sampler src)
        {
            return new MgtfSampler
            {
                AddressModeU = GetAddressModeU(src.WrapS),
                AddressModeV = GetAddressModeV(src.WrapT),
                MinFilter = GetMinFilter(src.MinFilter),
                MagFilter = GetMagFilter(src.MagFilter),
                MipmapMode = GetMipmapMode(src.MinFilter),
            };
        }

        private static MgSamplerAddressMode GetAddressModeV(Sampler.WrapTEnum wrapT)
        {
            switch (wrapT)
            {
                case Sampler.WrapTEnum.CLAMP_TO_EDGE:
                    return MgSamplerAddressMode.CLAMP_TO_EDGE;
                case Sampler.WrapTEnum.MIRRORED_REPEAT:
                    return MgSamplerAddressMode.MIRRORED_REPEAT;
                case Sampler.WrapTEnum.REPEAT:
                    return MgSamplerAddressMode.REPEAT;
                default:
                    throw new NotSupportedException();
            }
        }

        private static MgSamplerAddressMode GetAddressModeU(Sampler.WrapSEnum wrapS)
        {
            switch (wrapS)
            {
                case Sampler.WrapSEnum.CLAMP_TO_EDGE:
                    return MgSamplerAddressMode.CLAMP_TO_EDGE;
                case Sampler.WrapSEnum.MIRRORED_REPEAT:
                    return MgSamplerAddressMode.MIRRORED_REPEAT;
                case Sampler.WrapSEnum.REPEAT:
                    return MgSamplerAddressMode.REPEAT;
                default:
                    throw new NotSupportedException();
            }
        }

        private static MgSamplerMipmapMode GetMipmapMode(Sampler.MinFilterEnum? minFilter)
        {
            if (minFilter.HasValue)
            {
                switch (minFilter.Value)
                {
                    case Sampler.MinFilterEnum.LINEAR:
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_LINEAR:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_LINEAR:
                        return MgSamplerMipmapMode.LINEAR;

                    case Sampler.MinFilterEnum.NEAREST:
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_NEAREST:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_NEAREST:
                        return MgSamplerMipmapMode.NEAREST;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                return MgSamplerMipmapMode.NEAREST;
            }
        }

        private static MgFilter GetMinFilter(Sampler.MinFilterEnum? minFilter)
        {
            if (minFilter.HasValue)
            {
                switch (minFilter.Value)
                {
                    case Sampler.MinFilterEnum.LINEAR:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_LINEAR:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_NEAREST:
                        return MgFilter.LINEAR;

                    case Sampler.MinFilterEnum.NEAREST:
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_NEAREST:
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_LINEAR:
                        return MgFilter.NEAREST;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                return MgFilter.LINEAR;
            }
        }

        private static MgFilter GetMagFilter(Sampler.MagFilterEnum? magFilter)
        {
            if (magFilter.HasValue)
            {
                switch (magFilter.Value)
                {
                    case Sampler.MagFilterEnum.LINEAR:
                        return MgFilter.LINEAR;
                    case Sampler.MagFilterEnum.NEAREST:
                        return MgFilter.NEAREST;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                return MgFilter.LINEAR;
            }
        }

        #endregion

        #region ExtractTextures methods 

        private static MgtfTexture[] ExtractTextures(Texture[] textures)
        {
            var noOfSamplers = textures != null ? textures.Length : 0;

            var output = new MgtfTexture[noOfSamplers];
            for (var i = 0; i < noOfSamplers; i += 1)
            {
                output[i] = ExtractNewTexture(textures[i]);
            }
            return output;
        }

        private static MgtfTexture ExtractNewTexture(Texture texture)
        {
            return new MgtfTexture
            {
                Image = texture.Source,
                Sampler = texture.Sampler,
            };
        }

        #endregion
    }
    
}
