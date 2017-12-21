using glTFLoader;
using glTFLoader.Schema;
using Magnesium;
using Magnesium.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neutrino
{

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

            var cameraAllocationInfo = new GltfBucketAllocationInfo<Camera, CameraUBO>
            {
                BucketSize = 16,
                Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
            };            

            var cameras = cameraAllocationInfo.Extract(model.Cameras, request);

            var accessors = ExtractAccessors(model);

            var bufferViews = ExtractBufferViews(model);

            var meshes = ExtractMeshes(model, accessors);

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

        private GltfMesh[] ExtractMeshes(Gltf model, GltfAccessor[] accessors)
        {
            var noOfItems = model.Meshes != null ? model.Meshes.Length : 0;
            var output = new GltfMesh[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                var result = new GltfMesh(model.Meshes[i], accessors);
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
                destNode.Children = srcNode.Children != null ? srcNode.Children : new int[] { };
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
