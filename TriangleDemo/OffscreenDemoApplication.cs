using Magnesium;
using Magnesium.Utilities;
using Neutrino;
using System;
using System.Collections.Generic;
using System.IO;

namespace TriangleDemo
{
    internal class OffscreenDemoApplication : IDemoApplication
    {
        private IMgImageSourceExaminer mImageExaminer;
        private MgOptimizedStorageBuilder mBuilder;
        private MgOptimizedStorageContainer mStaticStorage;

        public OffscreenDemoApplication(IMgImageSourceExaminer examiner, MgOptimizedStorageBuilder builder)
        {
            mBuilder = builder;
            mImageExaminer = examiner;
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

        public void Prepare(IMgGraphicsConfiguration configuration, IMgGraphicsDevice screen)
        {
            var loader = new Loader();
            var dataLoader = new DataLoader();

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
                var meshPrimitives = AllocateMeshes(staticRequest, metaData.Meshes, metaData.Accessors, metaData.BufferViews);

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
                // pipeline     

                // allocate dynamic data
                // lights 
                // materials
                // cameras
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
            var locations = new GltfPrimitiveStorageLocation[noOfMeshes];
            for (var i =0; i < noOfMeshes; i += 1)
            {
                var mesh = meshes[i];

                foreach (var primitive in mesh.Primitives)
                {
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

                    locations[i] = new GltfPrimitiveStorageLocation
                    {
                        FinalDefinition = PadVertexDefinition(primitive.InitialDefinition),
                        Index = indexLocation,
                        Vertex = vertexLocation,
                        CopyOperations = copyOperations.ToArray(),
                    };
                }
            }
            return locations;
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