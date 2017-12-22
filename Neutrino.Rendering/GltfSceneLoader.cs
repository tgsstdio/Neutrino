using glTFLoader;
using glTFLoader.Schema;
using Magnesium;
using Magnesium.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerInstance
    {
        public TkVector3 Position { get; set; }
        public TkVector3 Scale { get; set; }
        public TkVector4 Rotation { get; set; }
        public uint CameraIndex { get; set; }
        public uint MaterialIndex { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Color3f
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialUBO
    {
        // 0 : baseColorTexture
        // 1 : metallicRoughnessTexture	
        // 2:  normalTexture
        // 3:  occlusionTexture
        // 4:  emissiveTexture

        // 0
        public MgVec4f BaseColorFactor { get; set; }

        // 4 
        public ushort BaseTexture { get; set; }
        public ushort BaseTextureTexCoords { get; set; }

        public ushort MetalicRoughnessTexture { get; set; }
        public ushort MetalicRoughnessTexCoords { get; set; }

        public ushort NormalTexture { get; set; }
        public ushort NormalTexCoords { get; set; }

        public ushort EmissiveTexture { get; set; }
        public ushort EmissiveTexCoords { get; set; }

        // 8
        public ushort OcclusionTexture { get; set; }
        public ushort OcclusionTexCoords { get; set; }

        public float MetallicFactor { get; set; }
        public float RoughnessFactor { get; set; }
        public float NormalScale { get; set; }

        // 12
        public Color3f EmissiveFactor { get; set; }
        public float OcclusionStrength { get; set; }

        // 16
        public float AlphaCutoff { get; set; }
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
    }

    public class GltfSceneLoader : IGltfSceneLoader
    {
        private readonly IMgGraphicsConfiguration mConfiguration;
        private readonly MgOptimizedStorageBuilder mBuilder;
        public GltfSceneLoader(
            IMgGraphicsConfiguration config,
            MgOptimizedStorageBuilder builder
        ) {
            mConfiguration = config;
            mBuilder = builder;
        }

        public void Load(string modelFilePath)
        {
            var model = Interface.LoadModel(modelFilePath);
            var baseDir = Path.GetDirectoryName(modelFilePath);

            var buffers = ExtractBuffers(model, baseDir);

            var request = new MgStorageBlockAllocationRequest();

            var cameraAllocationInfo = new GltfBucketAllocationInfo<CameraUBO>
            {
                BucketSize = 16,
                Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
            };            

            var cameras = cameraAllocationInfo.Extract(model.Cameras != null ? model.Cameras.Length : 0, request);

            var accessors = ExtractAccessors(model);

            var bufferViews = ExtractBufferViews(model);

            var materialAllocationInfo = new GltfBucketAllocationInfo<MaterialUBO>
            {
                BucketSize = 16,
                Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
            };

            var materials = materialAllocationInfo.Extract(model.Materials != null ? model.Materials.Length : 0, request);

            var meshes = ExtractMeshes(model, accessors, materials);

            var meshLocations = AllocateMeshes(request, meshes, accessors, bufferViews);

            var nodes = ExtractNodes(model, cameras);
        }

        class GltfPrimitiveStorageLocation
        {
            public int? Index { get; set; }
            public int Vertex { get; set; }
            public InterleavedOperation[] CopyOperations { get; internal set; }
        }

        class InterleavedOperation
        {
            public int Count { get; set; }
            public uint LengthInBytes { get; set; }
            public ulong SrcOffset { get; set; }
            public ulong DstStride { get; set; }
            public int BufferIndex { get; internal set; }
            public object ByteStride { get; internal set; }
        }

        private GltfPrimitiveStorageLocation[] AllocateMeshes(MgStorageBlockAllocationRequest request, GltfMesh[] meshes, GltfAccessor[] accessors, GltfBufferView[] bufferViews)
        {
            var locations = new List<GltfPrimitiveStorageLocation>();
            foreach(var mesh in meshes)
            {
                foreach (var primitive in mesh.Primitives)
                {
                    var locator = primitive.VertexLocations;

                    var finalLocation = new GltfPrimitiveStorageLocation { };

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

                    var copyOps = new List<InterleavedOperation>();

                    var totalElementSize = 0U;
                    foreach(var field in vertexFields)
                    {
                        if (field.HasValue)
                        {
                            var selected = accessors[field.Value];
                            var op = CreateCopyOp(copyOps, selected, bufferViews);
                            totalElementSize += op.LengthInBytes;
                            totalSize += selected.TotalByteSize;
                        }
                        else
                        {
                            throw new InvalidOperationException("Position required");
                        }
                    }

                    foreach(var op in copyOps)
                    {
                        op.DstStride = totalElementSize;
                    }

                    var vertexInfo = new MgStorageBlockAllocationInfo
                    {
                        MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                        Usage = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT,
                        ElementByteSize = totalElementSize,
                        Size = totalSize,
                    };

                    finalLocation.Vertex = request.Insert(vertexInfo);

                    if (locator.Indices.HasValue)
                    {
                        var selected = accessors[locator.Indices.Value];
                        var op = CreateCopyOp(copyOps, selected, bufferViews);
                        op.DstStride = selected.ElementByteSize;

                        var indexInfo = new MgStorageBlockAllocationInfo
                        {
                            MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                            Usage = MgBufferUsageFlagBits.INDEX_BUFFER_BIT,     
                            Size = selected.TotalByteSize,
                            ElementByteSize = selected.ElementByteSize,
                        };

                        finalLocation.Index = request.Insert(indexInfo);
                    }

                    finalLocation.CopyOperations = copyOps.ToArray();
                    locations.Add(finalLocation);
                }                
            }
            return locations.ToArray();
        }

        private static InterleavedOperation CreateCopyOp(List<InterleavedOperation> destination, GltfAccessor selected, GltfBufferView[] bufferViews)
        {
            if (!selected.BufferView.HasValue)
                throw new InvalidOperationException("unable to locate bufferview");

            var view = bufferViews[selected.BufferView.Value];

            var op = new InterleavedOperation
            {
                BufferIndex = view.BufferIndex,                
                Count = selected.ElementCount,
                SrcOffset = (ulong)(view.BufferOffset + selected.ViewOffset),
                LengthInBytes = selected.NoOfComponents * selected.ElementByteSize,
            };

            op.ByteStride =
                (view.ByteStride.HasValue)
                ? (uint) view.ByteStride.Value
                : op.LengthInBytes;                    

            destination.Add(op);
            return op;
        }

        private GltfMesh[] ExtractMeshes(Gltf model, GltfAccessor[] accessors, GltfBucketContainer materials)
        {
            var noOfItems = model.Meshes != null ? model.Meshes.Length : 0;
            var output = new GltfMesh[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                var result = new GltfMesh(model.Meshes[i], accessors, materials);
                output[i] = result;
            }
            return output;
        }

        private GltfAccessor[] ExtractAccessors(Gltf model)
        {
            var noOfItems = model.Accessors != null ? model.Accessors.Length : 0;
            var output = new GltfAccessor[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = new GltfAccessor(model.Accessors[i]);                 
            }

            return output;
        }

        private static GltfBufferView[] ExtractBufferViews(glTFLoader.Schema.Gltf model)
        {
            var noOfItems = model.BufferViews != null ? model.BufferViews.Length : 0;
            var output = new GltfBufferView[noOfItems];
            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = new GltfBufferView(model.BufferViews[i]);
            }
            return output;
        }


        private GtlfRenderNode[] ExtractNodes(Gltf model, GltfBucketContainer cameras)
        {
            var noOfNodes = model.Nodes != null ? model.Nodes.Length : 0;
            var allNodes = new GtlfRenderNode[noOfNodes];

            for (var i = 0; i < noOfNodes; i += 1)
            {
                var srcNode = model.Nodes[i];
                var destNode = new GtlfRenderNode { };

                destNode.Name = srcNode.Name;
                destNode.NodeIndex = i;
                destNode.CameraAllocation = cameras.GetAllocation(srcNode.Camera);
                destNode.Children = srcNode.Children ?? (new int[] { });
                destNode.Transform = GenerateTransform(srcNode);
                destNode.IsMirrored = destNode.Transform.Determinant < 0;
                destNode.Mesh = srcNode.Mesh;

                // TODO: meshes

                allNodes[i] = destNode;
            }

            LinkToParents(allNodes);
            return allNodes;
        }

        public static TkMatrix4 GenerateTransform(Node srcNode)
        {
           if (srcNode.Matrix != null && srcNode.Matrix.Length == 16)
           {
                var src = srcNode.Matrix;
                return new TkMatrix4(
                    src[0],   src[1],  src[2],  src[3],
                    src[4],   src[5],  src[6],  src[7],
                    src[8],   src[9], src[10], src[11],
                    src[12], src[13], src[14], src[15]
                );
           }
           else
           {
                var firstOp =
                    (srcNode.Scale != null && srcNode.Scale.Length == 3)
                    ? TkMatrix4.CreateScale(srcNode.Scale[0], srcNode.Scale[1], srcNode.Scale[2])
                    : TkMatrix4.CreateScale(1, 1, 1);
                var secondOp =
                    (srcNode.Rotation != null && srcNode.Rotation.Length == 4)
                    ? TkMatrix4.Identity // TODO QUATERNION
                    : TkMatrix4.Identity;
                var thirdOp =
                    (srcNode.Translation != null && srcNode.Translation.Length == 3)
                    ? TkMatrix4.CreateTranslation(srcNode.Translation[0], srcNode.Translation[1], srcNode.Translation[2])
                    : TkMatrix4.CreateTranslation(0, 0, 0);

                // T * (R * S)
                TkMatrix4.Mult(ref secondOp, ref firstOp, out TkMatrix4 result);
                return TkMatrix4.Mult(thirdOp, result);
           }
        }

        public static void LinkToParents(GtlfRenderNode[] allNodes)
        {
            foreach (var srcParentNode in allNodes)
            {
                var noOfChildren = srcParentNode.Children != null ? srcParentNode.Children.Length : 0;

                if (srcParentNode.Children != null)
                {
                    foreach (var childIndex in srcParentNode.Children)
                    {
                        var child = allNodes[childIndex];
                        child.Parent = srcParentNode.NodeIndex;
                    }
                }
            }
        }

        private static List<byte[]> ExtractBuffers(glTFLoader.Schema.Gltf model, string baseDir)
        {
            var buffers = new List<byte[]>();
            foreach (var selectedBuffer in model.Buffers)
            {
                if (selectedBuffer.Uri.StartsWith("data:") && DataURL.FromUri(selectedBuffer.Uri, out DataURL output))
                {
                    if (output.Data.Length != selectedBuffer.ByteLength)
                    {
                        throw new InvalidDataException(
                            string.Format(
                                "The specified length of embedded data chunk ({0}) is not equal to the actual length of the data chunk ({1}).",
                                selectedBuffer.ByteLength,
                                output.Data.Length)
                        );
                    }
                    buffers.Add(output.Data);
                }
                else
                {
                    // OPEN FILE
                    string additionalFilePath = System.IO.Path.Combine(baseDir, selectedBuffer.Uri);
                    using (var fs = File.Open(additionalFilePath, FileMode.Open))
                    using (var br = new BinaryReader(fs))
                    {
                        // ONLY READ SPECIFIED CHUNK SIZE
                        buffers.Add(br.ReadBytes(selectedBuffer.ByteLength));
                    }
                }
            }
            return buffers;
        }

  
    }
}
