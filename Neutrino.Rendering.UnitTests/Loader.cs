using glTFLoader.Schema;
using Magnesium;
using System;

namespace Neutrino.UnitTests
{
    public class Loader
    {
        public MetaFile LoadMetaData(Gltf model)
        {
            var accessors = ExtractAccessors(model.Accessors);

            return new MetaFile
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

        private static GltfNodeInfo[] ExtractNodes(Node[] nodes)
        {
            var noOfNodes = nodes != null ? nodes.Length : 0;
            var allNodes = new GltfNodeInfo[noOfNodes];

            for (var i = 0; i < noOfNodes; i += 1)
            {
                allNodes[i] = new GltfNodeInfo(nodes[i]);
            }

            RelinkParents(allNodes);
            return allNodes;
        }

        public static void RelinkParents(GltfNodeInfo[] allNodes)
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

        private static GltfAccessor[] ExtractAccessors(Accessor[] accessors)
        {
            var noOfItems = accessors != null ? accessors.Length : 0;
            var output = new GltfAccessor[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = new GltfAccessor(accessors[i]);
            }

            return output;
        }

        private static GltfBufferView[] ExtractBufferViews(BufferView[] bufferViews)
        {
            var noOfItems = bufferViews != null ? bufferViews.Length : 0;
            var output = new GltfBufferView[noOfItems];
            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = new GltfBufferView(bufferViews[i]);
            }
            return output;
        }

        #region ExtractCameras methods 

        private static GltfCamera[] ExtractCameras(Camera[] cameras)
        {
            var count = cameras != null ? cameras.Length : 0;

            var output = new GltfCamera[count];
            for (var i = 0; i < count; i += 1)
            {
                output[i] = ExtractNewCamera(cameras[i]);
            }
            return output;
        }

        private static GltfCamera ExtractNewCamera(Camera camera)
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


            return new GltfCamera
            {
                ProjectionType = (camera.Type == Camera.TypeEnum.orthographic)
                    ? GltfCamera.CameraType.Orthogonal
                    : GltfCamera.CameraType.Perspective,
                AspectRatio = aspectRatio,
                Values = values,
            };
        }

        #endregion

        #region ExtractMaterials methods 

        private static GltfMaterial[] ExtractMaterials(Material[] materials)
        {
            var count = materials != null ? materials.Length : 0;

            var output = new GltfMaterial[count];
            for (var i = 0; i < count; i += 1)
            {
                output[i] = ExtractNewMaterial(materials[i]);
            }
            return output;
        }

        public static GltfMaterial ExtractNewMaterial(Material src)
        {
            GltfMaterial.GltfMaterialTexture BaseColorTexture = null;
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
                    BaseColorTexture = new GltfMaterial.GltfMaterialTexture
                    {
                        Texture = src.PbrMetallicRoughness.BaseColorTexture.Index,
                        TexCoords = src.PbrMetallicRoughness.BaseColorTexture.TexCoord,
                    };
                }
            }

            GltfMaterial.GltfMaterialTexture NormalTexture = null;
            float normalScale = 1f;
            if (src.NormalTexture != null)
            {
                NormalTexture = new GltfMaterial.GltfMaterialTexture
                {
                    Texture = src.NormalTexture.Index,
                    TexCoords = src.NormalTexture.TexCoord,
                };
                normalScale = src.NormalTexture.Scale;
            }

            GltfMaterial.GltfMaterialTexture EmissiveTexture = null;
            if (src.EmissiveTexture != null)
            {
                EmissiveTexture = new GltfMaterial.GltfMaterialTexture
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

            GltfMaterial.GltfMaterialTexture OcclusionTexture = null;
            float strength = 1f;
            if (src.OcclusionTexture != null)
            {
                OcclusionTexture = new GltfMaterial.GltfMaterialTexture
                {
                    Texture = src.OcclusionTexture.Index,
                    TexCoords = src.OcclusionTexture.TexCoord,
                };
                strength = src.OcclusionTexture.Strength;
            }

            return new GltfMaterial
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


        private static GltfMaterial.AlphaModeEquation ExtractAlphaMode(Material.AlphaModeEnum alphaMode)
        {
            switch (alphaMode)
            {
                case Material.AlphaModeEnum.BLEND:
                    return new GltfMaterial.AlphaModeEquation
                    {
                        A = 0f,
                        B = 0f,
                        C = 1f,
                    };
                case Material.AlphaModeEnum.MASK:
                    return new GltfMaterial.AlphaModeEquation
                    {
                        A = 0f,
                        B = 1f,
                        C = 0f,
                    };
                case Material.AlphaModeEnum.OPAQUE:
                    return new GltfMaterial.AlphaModeEquation
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

        private static GltfMesh[] ExtractMeshes(Mesh[] meshes, GltfAccessor[] accessors)
        {
            var noOfItems = meshes != null ? meshes.Length : 0;
            var output = new GltfMesh[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = ExtractNewMesh(meshes[i], accessors);
            }
            return output;
        }

        private static GltfMesh ExtractNewMesh(Mesh mesh, GltfAccessor[] accessors)
        {
            return new GltfMesh
            {
                Name = mesh.Name,
                Weights = mesh.Weights,
                Primitives = InitializePrimitives(mesh, accessors),
            };
        }

        private static GltfMeshPrimitive[] InitializePrimitives(
            Mesh mesh,
            GltfAccessor[] accessors)
        {
            var noOfItems = mesh.Primitives != null ? mesh.Primitives.Length : 0;
            var output = new GltfMeshPrimitive[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                var srcPrimitive = mesh.Primitives[i];

                var vertexLocations = new PerVertexDataLocator(
                    srcPrimitive.Indices,
                    srcPrimitive.Attributes);
                var definition = PerVertexDefinitionEncoder.Extract(
                    vertexLocations,
                    accessors);

                var temp = new GltfMeshPrimitive
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

        private static GltfSampler[] ExtractSamplers(Sampler[] samplers)
        {
            var noOfSamplers = samplers != null ? samplers.Length : 0;

            var output = new GltfSampler[noOfSamplers];
            for (var i = 0; i < noOfSamplers; i += 1)
            {
                output[i] = new GltfSampler(samplers[i]);
            }
            return output;
        }

        private static GltfTexture[] ExtractTextures(Texture[] textures)
        {
            var noOfSamplers = textures != null ? textures.Length : 0;

            var output = new GltfTexture[noOfSamplers];
            for (var i = 0; i < noOfSamplers; i += 1)
            {
                output[i] = new GltfTexture(textures[i]);
            }
            return output;
        }
    }
    
}
