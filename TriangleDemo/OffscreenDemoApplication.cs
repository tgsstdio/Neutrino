using Magnesium;
using Magnesium.Utilities;
using Neutrino;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace TriangleDemo
{
    internal class OffscreenDemoApplication : IDemoApplication
    {
        private IMgImageSourceExaminer mImageExaminer;
        private MgOptimizedStorageBuilder mBuilder;
        private MgOptimizedStorageContainer mStaticStorage;
        private IEffectVariantFactory mPbrFactory;

        public OffscreenDemoApplication(IMgImageSourceExaminer examiner, MgOptimizedStorageBuilder builder, IEffectVariantFactory pbrFactory)
        {
            mBuilder = builder;
            mImageExaminer = examiner;
            mPbrFactory = pbrFactory;
        }

        public MgGraphicsDeviceCreateInfo Initialize()
        {
            return new MgGraphicsDeviceCreateInfo
            {
                Swapchain = new MgDeviceFormatSetting
                {
                    Color = MgColorFormatOption.AUTO_DETECT,
                    DepthStencil = MgDepthFormatOption.AUTO_DETECT,
                },
                RenderPass = new MgDeviceFormatSetting
                {
                    Color = MgColorFormatOption.AUTO_DETECT,
                    DepthStencil = MgDepthFormatOption.AUTO_DETECT,
                },
                Width = 640,
                Height = 480,
                MinDepth = 0f,
                MaxDepth = 1f,
                Samples = MgSampleCountFlagBits.COUNT_1_BIT,
            };
        }

        #region Prepare methods 

        class ScenePass
        {
            public IMgEffectFramework RenderTarget { get; set; }
            public EffectPipelineDictionary Variants { get; set; }

            public ScenePassEffect[] Effects { get; set; }

            internal void Initialize(IMgDevice device, SceneMeshPrimitive[] meshPrimitives, MgtfMaterial[] materials)
            {
                Variants = new EffectPipelineDictionary();
                foreach(var eff in Effects)
                {
                    InitializeVariants(device, eff, meshPrimitives, materials);
                }
            }

            private static EffectVariantOptions ExtractVariantOptions(
                MgFrontFace front,
                MgPrimitiveTopology topology,
                MgtfMaterial material)
            {
                return new EffectVariantOptions
                {
                    FrontFace = front,
                    CullMode =
                        material.DoubleSided
                        ? MgCullModeFlagBits.NONE
                        : MgCullModeFlagBits.BACK_BIT,
                };
            }

            private void InitializeVariants(
                IMgDevice device,
                ScenePassEffect passEffect,
                SceneMeshPrimitive[] meshPrimitives,
                MgtfMaterial[] materials)
            {
                foreach (var primitive in meshPrimitives)
                {
                    var vertexInput = new PerVertexInputPipelineState(primitive.VertexDefinition);
                    var material = materials[primitive.Material];

                    foreach (var frontFace in new[] { MgFrontFace.COUNTER_CLOCKWISE, MgFrontFace.CLOCKWISE })
                    {
                        var options = ExtractVariantOptions(frontFace, primitive.Topology, material);

                        var ev = passEffect.Factory.Initialize(device,
                            passEffect.EffectLayout.Layout,
                            RenderTarget.Renderpass,
                            vertexInput,
                            options);
                        Variants.Add(ev.Key, ev);
                    }
                }
             }
        }

        class ScenePassEffect
        {
            public EffectLayout EffectLayout { get; set; }
            public IEffectVariantFactory Factory { get; set; }
        }

        public void Prepare(IMgGraphicsConfiguration configuration, IMgGraphicsDevice screen)
        {
            var loader = new Loader();
            var dataLoader = new DataLoader();
            var device = configuration.Device;

            using (var fs = File.Open("Data/Triangle.gltf", FileMode.Open))
            {
                // load model file
                var model = glTFLoader.Interface.LoadModel(fs);
                // load meta data
                var metaData = loader.LoadMetaData(model);
                // load data
                var data = dataLoader.LoadData(".", model);

                var staticRequest = new MgStorageBlockAllocationRequest();
                // allocate partitions for static data                
                // mesh
                var meshLocations = AllocateMeshes(staticRequest, metaData.Meshes, metaData.Accessors, metaData.BufferViews);

                var meshPrimitives = ExtractMeshPrimitives(metaData.Materials,
                    out MgtfMaterial[] sceneMaterials,
                    metaData.Meshes,
                    meshLocations);

                // images
                var images = ExamineImages(data.Images, data.Buffers);

                // initialize static data storage 

                var staticCreateInfo = new MgOptimizedStorageCreateInfo
                {
                    Allocations = staticRequest.ToArray(),
                };

                mStaticStorage = mBuilder.Build(staticCreateInfo);

                // build static artifacts
                // render target
                // descriptor set layout + pipeline layout   
                var pass = new ScenePass
                {
                    RenderTarget = screen,
                    Effects = new[] {
                        new ScenePassEffect
                        {
                            Factory = mPbrFactory,
                            EffectLayout = mPbrFactory.CreateEffectLayout(configuration.Device),
                        }
                    }
                };

                pass.Initialize(
                    configuration.Device,
                    meshPrimitives,
                    sceneMaterials);

                // allocate dynamic data
                var dynamicRequest = new MgStorageBlockAllocationRequest();

                var limits = new MgPhysicalDeviceLimits();
                var worldData = AllocateWorldData(dynamicRequest, limits.MaxUniformBufferRange);

                var storageIndices = AllocateMaterials(sceneMaterials, dynamicRequest, limits);

                // per instance data

                // build dynamic artifacts
                // semaphores 
                // fences
                // descriptor sets

                // initialize dynamic data storage           

                // copy data across
                // buffers 
                // images

                // map dynamic data 

                // build command buffers
            }
        }

        private static int[] AllocateMaterials(MgtfMaterial[] materials, MgStorageBlockAllocationRequest dynamicRequest, MgPhysicalDeviceLimits limits)
        {
            // materials
            var query = new PerMaterialTextureStorageQuery(limits);
            var noOfSamplers = query.GetMaxNoOfCombinedImageSamplers();

            const uint PBR_TEXTURES_PER_MATERIALS = 5U;
            var blockSize = (uint)Marshal.SizeOf(typeof(MaterialUBO));

            const uint HI_RES = 32U;
            const uint LOW_RES = 16U;

            // GONNA RESERVE 5 BINDINGS SLOTS 
            // IN VK, a global binding range is used by all descriptors     
            // IN OPENGL/WEBGL, buffer and textures have the own binding range
            // THEREFORE, Mg standard is not to overlap binding values between descriptors
            // OTHERWISE, separate sets implementation (TODO) could be used to handle separate ranges
            const uint NO_OF_RESERVED_BINDINGS = 5U;
            var range = noOfSamplers - NO_OF_RESERVED_BINDINGS;

            if (noOfSamplers < LOW_RES)
                throw new InvalidOperationException("not enough combined samplers for pbr");

            // pick between hi res and low res
            bool isHighRes = (noOfSamplers >= HI_RES);
            uint bindableImages = isHighRes ? 32U : 16U;

            var elementRange = query.GetElementRange(
                MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                PBR_TEXTURES_PER_MATERIALS,
                blockSize);

            if (isHighRes)
            {
                // floor(no_of_samplers - reserved_slots / textures_per_mat) = floor((32 - 5)/5) = 5
                if (elementRange < 5U)
                    throw new InvalidOperationException("hi res not applicable");
                elementRange = 5U;
            }
            else
            {
                // floor(no_of_samplers - reserved_slots / textures_per_mat) = floor((16 - 5)/5) = 2
                if (elementRange < 2U)
                    throw new InvalidOperationException("low res not applicable");
                elementRange = 2U;
            }

            var noOfAllocations = (materials.Length / elementRange);
            noOfAllocations += (materials.Length % elementRange == 0) ? 0 : 1;

            var info = new MgStorageBlockAllocationInfo
            {
                Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
                Size = elementRange * blockSize,
            };

            var materialIndices = new int[noOfAllocations];
            for (var i = 0; i < noOfAllocations; i += 1)
            {
                materialIndices[i] = dynamicRequest.Insert(info);
            }
            return materialIndices;
        }

        public class WorldData
        {
            public int StorageIndex { get; set; }
            public uint MaxNoOfCameras { get; set; }
            public uint MaxNoOfLights { get; set; }            
        }

        private WorldData AllocateWorldData(MgStorageBlockAllocationRequest dynamiceRequest, uint maxUniformBufferRange)
        {
            const uint LOW_RES = 16384;
            const uint HIGH_RES = 65536;

            if (maxUniformBufferRange < LOW_RES)
            {
                throw new InvalidOperationException("not enough space");
            }

            var noOfCameras = 32U;
            var noOfLights = 256U;

            if (maxUniformBufferRange >= HIGH_RES)
            {
                noOfCameras = 128U;
                noOfLights = 1024U;
            }

            var upperLimit = (maxUniformBufferRange >= HIGH_RES)
                ? HIGH_RES
                : LOW_RES;

            var cameraStride = Marshal.SizeOf(typeof(CameraUBO));
            var lightStride = Marshal.SizeOf(typeof(LightUBO));

            var cameraLength = cameraStride * noOfCameras;
            var lightLength = lightStride * noOfLights;

            var totalSize = (ulong)(cameraLength + lightLength);

            if (totalSize > upperLimit)
            {
                throw new InvalidOperationException("not enough space for uniform block");
            }

            var location = dynamiceRequest.Insert(
                new MgStorageBlockAllocationInfo
                {
                    Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                    MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
                    Size = totalSize,
                }
            );

            return new WorldData
            {
                StorageIndex = location,
                MaxNoOfCameras = noOfCameras,
                MaxNoOfLights = noOfLights,
            };
        }

        private SceneMeshPrimitive[] ExtractMeshPrimitives(
            MgtfMaterial[] inputMaterials,
            out MgtfMaterial[] outputMaterials,
            MgtfMesh[] meshes,
            GltfPrimitiveStorageLocation[] meshLocations)
        {
            var primitives = new List<SceneMeshPrimitive>();

            // add default
            var sceneMaterials = new List<MgtfMaterial>()
            {
                new MgtfMaterial
                {
                    BaseColorTexture = null,
                    NormalTexture = null,
                    EmissiveTexture = null,
                    OcclusionTexture = null,
                    RoughnessTexture = null,
                    AlphaMode = new MgtfAlphaModeEquation { A = 1, B = 0, C = 0},
                    BaseColorFactor = new MgVec4f(1f, 1f, 1f, 1f),
                    EmissiveFactor = new Color3f { R = 0f, G = 0f, B = 0f },
                    AlphaCutoff = 0.5f,
                    MetallicFactor = 1f,
                    RoughnessFactor = 1f,
                    OcclusionStrength = 1f,
                },
            };
            sceneMaterials.AddRange(inputMaterials);            

            outputMaterials = sceneMaterials.ToArray();            

            foreach(var location in meshLocations)
            {
                var mesh = meshes[location.Mesh];

                var srcPrimitive = mesh.Primitives[location.MeshPrimitive];               

                var primitive = new SceneMeshPrimitive
                {
                    Mesh = location.Mesh,
                    Topology = srcPrimitive.Topology,
                    Indices = 
                        location.Index.HasValue
                        ? new int[] {location.Index.Value }
                        : new int[] { },
                    Vertices = new int[] {location.Vertex},
                    VertexDefinition = location.FinalDefinition,
                    Material = 
                        !srcPrimitive.Material.HasValue
                        ? 0
                        : (srcPrimitive.Material.Value + 1), 
                    IndexCount = srcPrimitive.IndexCount,
                    VertexCount = srcPrimitive.VertexCount,
                };

                primitives.Add(primitive);
            }

            return primitives.ToArray();
        }

        class SceneMeshPrimitive 
        {
            public int Mesh { get; set; }
            public int Material { get; set; }

            public MgPrimitiveTopology Topology { get; set; }
            public PerVertexDefinition VertexDefinition { get; set; }
            public int[] Vertices { get; set; }
            public int[] Indices { get; set; }
            public uint IndexCount { get; internal set; }
            public uint VertexCount { get; internal set; }
        }



        private MgImageSource[] ExamineImages(MgtfImage[] images, MgtfBuffer[] buffers)
        {
            var locations = new List<MgImageSource>();

            foreach (var img in images)
            {
                byte[] srcData = null;

                int srcOffset = 0;
                int srcLength = 0;
                if (img.Source != null)
                {
                    srcData = img.Source;
                    srcOffset = 0;
                    srcLength = img.Source.Length;
                }
                else
                {
                    if (!img.Buffer.HasValue)
                    {
                        throw new InvalidOperationException("");
                    }
                    var selectedBuffer = buffers[img.Buffer.Value];
                    srcData = selectedBuffer.Data;

                    srcOffset = (int) img.SrcOffset;
                    srcLength = (int) img.SrcLength;
                }

                using (var ms = new MemoryStream(srcData, srcOffset, srcLength))
                {
                    var location = mImageExaminer.DetermineSource(ms);
                    locations.Add(location);
                }                
            }
            return locations.ToArray();
        }

        private GltfPrimitiveStorageLocation[] AllocateMeshes(
            MgStorageBlockAllocationRequest request, 
            MgtfMesh[] meshes,
            MgtfAccessor[] accessors,
            MgtfBufferView[] bufferViews)
        {
            var noOfMeshes = meshes != null ? meshes.Length : 0;
            var locations = new List<GltfPrimitiveStorageLocation>();
            for (var i =0; i < noOfMeshes; i += 1)
            {
                var mesh = meshes[i];

                var noOfPrimitives = mesh.Primitives != null
                    ? mesh.Primitives.Length
                    : 0;

                for (var j= 0; j < noOfPrimitives; j += 1)
                {
                    var primitive = mesh.Primitives[j];
                    var locator = primitive.VertexLocations;

                    int? indexLocation = null;
                    GltfInterleavedOperation indexCopy = null;
                    if (locator.Indices.HasValue)
                    {
                        var accessor = accessors[locator.Indices.Value];

                        indexCopy = GenerateCopyOperation(accessor, bufferViews);
                        indexCopy.DstStride = accessor.ElementByteSize;

                        indexLocation = request.Insert(GetIndexAllocation(accessor));
                    }
                        
                    var vertexLocation = ExtractVertices(
                        primitive.VertexCount, 
                        request,
                        accessors,
                        bufferViews,
                        locator,
                        out GltfInterleavedOperation[] vertexCopies);

                    var copyOperations = new List<GltfInterleavedOperation>();
                    if (indexCopy != null)
                    {
                        copyOperations.Add(indexCopy);
                    }
                    copyOperations.AddRange(vertexCopies);

                    var location = new GltfPrimitiveStorageLocation
                    {
                        Mesh = i,
                        MeshPrimitive = j,
                        FinalDefinition = PadVertexDefinition(primitive.InitialDefinition),
                        Index = indexLocation,
                        Vertex = vertexLocation,
                        CopyOperations = copyOperations.ToArray(),
                    };

                    locations.Add(location);
                }
            }
            return locations.ToArray();
        }

        private static PerVertexDefinition PadVertexDefinition(PerVertexDefinition modelDefinition)
        {
            PerVertexDefinition DEFAULT_PADDING = new PerVertexDefinition
            {
                Position = PerVertexPositionType.Float3,
                Normal = PerVertexNormalType.Float3,
                Tangent = PerVertexTangentType.Float4,
                TexCoords0 = PerVertexTexCoordsType.Float2,
                TexCoords1 = PerVertexTexCoordsType.Float2,
                Color0 = PerVertexColorType.FloatRGBA,
                Color1 = PerVertexColorType.FloatRGBA,
                Joints0 = PerVertexJointType.Byte4,
                Joints1 = PerVertexJointType.Byte4,
                Weights0 = PerVertexWeightsType.Float4,
                Weights1 = PerVertexWeightsType.Float4,
            };

            return new PerVertexDefinition
            {
                Position =
                    (modelDefinition.Position == PerVertexPositionType.None)
                    ? DEFAULT_PADDING.Position
                    : modelDefinition.Position,
                Normal =
                    (modelDefinition.Normal == PerVertexNormalType.None)
                    ? DEFAULT_PADDING.Normal
                    : modelDefinition.Normal,
                Tangent =
                    (modelDefinition.Tangent == PerVertexTangentType.None)
                    ? DEFAULT_PADDING.Tangent
                    : modelDefinition.Tangent,
                TexCoords0 = (modelDefinition.TexCoords0 == PerVertexTexCoordsType.None)
                    ? DEFAULT_PADDING.TexCoords0
                    : modelDefinition.TexCoords0,
                TexCoords1 = (modelDefinition.TexCoords1 == PerVertexTexCoordsType.None)
                    ? DEFAULT_PADDING.TexCoords1
                    : modelDefinition.TexCoords1,
                Color0 = (modelDefinition.Color0 == PerVertexColorType.None)
                    ? DEFAULT_PADDING.Color0
                    : modelDefinition.Color0,
                Color1 = (modelDefinition.Color1 == PerVertexColorType.None)
                    ? DEFAULT_PADDING.Color1
                    : modelDefinition.Color1,
                Joints0 = (modelDefinition.Joints0 == PerVertexJointType.None)
                    ? DEFAULT_PADDING.Joints0
                    : modelDefinition.Joints0,
                Joints1 = (modelDefinition.Joints1 == PerVertexJointType.None)
                    ? DEFAULT_PADDING.Joints1
                    : modelDefinition.Joints1,
                Weights0 = (modelDefinition.Weights0 == PerVertexWeightsType.None)
                    ? DEFAULT_PADDING.Weights0
                    : modelDefinition.Weights0,
                Weights1 = (modelDefinition.Weights1 == PerVertexWeightsType.None)
                    ? DEFAULT_PADDING.Weights1
                    : modelDefinition.Weights1,
            };
        }

        private static MgStorageBlockAllocationInfo GetIndexAllocation(MgtfAccessor accessor)
        {
            return new MgStorageBlockAllocationInfo
            {
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                Usage = MgBufferUsageFlagBits.INDEX_BUFFER_BIT,
                Size = accessor.TotalByteSize,
                ElementByteSize = accessor.ElementByteSize,
            }; 
        }

        private static int ExtractVertices(uint vertexCount, 
            MgStorageBlockAllocationRequest request,
            MgtfAccessor[] accessors,
            MgtfBufferView[] bufferViews,
            IMgtfPerVertexDataLocator locator,
            out GltfInterleavedOperation[] vertexCopies)
        {
            var totalSize = 0UL;
            var vertexFields = new int?[]
            {
                locator.Position,
                locator.Normal,
                locator.Tangent,
                locator.TexCoords0,
                locator.TexCoords1,
                locator.Color0,
                locator.Color1,
                locator.Joints0,
                locator.Joints1,
                locator.Weights0,
                locator.Weights1,
            };

            var DEFAULT_PADDING_BYTE_STRIDE = new uint[]
            {
                12U,
                12U,
                16U,
                8U,
                8U,
                16U,
                16U,
                4U,
                4U,
                16U,
                16U,
            };

            var copyOps = new List<GltfInterleavedOperation>();

            var vertexBufferStride = 0U;
            for (var i = 0; i < vertexFields.Length; i += 1)
            {
                var field = vertexFields[i];
                if (field.HasValue)
                {
                    var selected = accessors[field.Value];
                    var op = GenerateCopyOperation(selected, bufferViews);
                    copyOps.Add(op);
                    vertexBufferStride += op.ByteStride;
                    totalSize += selected.TotalByteSize;
                }
                else
                {
                    vertexBufferStride += DEFAULT_PADDING_BYTE_STRIDE[i];
                    totalSize += vertexCount * DEFAULT_PADDING_BYTE_STRIDE[i];
                }
            }

            foreach (var op in copyOps)
            {
                op.DstStride = vertexBufferStride;
            }

            var vertexInfo = new MgStorageBlockAllocationInfo
            {
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                Usage = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT,
                ElementByteSize = vertexBufferStride,
                Size = totalSize,
            };

            vertexCopies = copyOps.ToArray();
            return request.Insert(vertexInfo);
        }

        private static GltfInterleavedOperation GenerateCopyOperation(MgtfAccessor accessor, MgtfBufferView[] bufferViews)
        {
            if (!accessor.BufferView.HasValue)
                throw new InvalidOperationException("unable to locate bufferview");

            var view = bufferViews[accessor.BufferView.Value];

            var op = new GltfInterleavedOperation
            {
                BufferIndex = view.BufferIndex,
                Count = accessor.ElementCount,
                SrcOffset = (ulong)(view.BufferOffset + accessor.ViewOffset),
                TotalSize = (uint)(accessor.ElementCount * accessor.NoOfComponents * accessor.ElementByteSize),
                ByteStride = (view.ByteStride.HasValue)
                    ? (uint)view.ByteStride.Value
                    : accessor.NoOfComponents * accessor.ElementByteSize,
            };

            return op;
        }

        #endregion

        public void ReleaseManagedResources(IMgGraphicsConfiguration configuration)
        {

        }

        public void ReleaseUnmanagedResources(IMgGraphicsConfiguration configuration)
        {
            if (mStaticStorage != null)
            {
                mStaticStorage.Storage.Destroy(configuration.Device, null);
            }
        }

        public IMgSemaphore[] Render(IMgQueue queue, uint layerNo, IMgSemaphore semaphore)
        {
            return new IMgSemaphore[] { };
        }

        public void Update(IMgGraphicsConfiguration configuration)
        {
           
        }
    }
}