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
            PadMeshes(meshes);

            var meshLocations = AllocateMeshes(request, meshes, accessors, bufferViews);



            var nodes = ExtractNodes(model, cameras);
        }

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

        private void PadMeshes(GltfMesh[] meshes)
        {
            var noOfMeshes = meshes.Length;
            for (var i = 0; i < noOfMeshes; i += 1)
            {
                var currentMesh = meshes[i];

                foreach (var primitive in currentMesh.Primitives)
                {
                    var modelDefinition = primitive.InitialDefinition;
                    primitive.FinalDefinition = new PerVertexDefinition {
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
            }
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

                    finalLocation.CopyOperations = GenerateCopyOps(request, accessors, bufferViews, locator, finalLocation);
                    locations.Add(finalLocation);
                }
            }
            return locations.ToArray();
        }

        private static GltfInterleavedOperation[] GenerateCopyOps(MgStorageBlockAllocationRequest request, GltfAccessor[] accessors, GltfBufferView[] bufferViews, IPerVertexDataLocator locator, GltfPrimitiveStorageLocation finalLocation)
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

            var paddingByteStride = new uint[]
            {

            };

            var paddingTotalByteStride = new uint[]
            {

            };

            var copyOps = new List<GltfInterleavedOperation>();

            var vertexBufferStride = 0U;
            for (var i =0; i < vertexFields.Length; i += 1)
            {
                var field = vertexFields[i];
                if (field.HasValue)
                {
                    var selected = accessors[field.Value];
                    var op = CreateCopyOp(copyOps, selected, bufferViews);
                    vertexBufferStride += op.ByteStride;
                    totalSize += selected.TotalByteSize;
                }
                else
                {
                    vertexBufferStride += paddingByteStride[i];
                    totalSize += paddingTotalByteStride[i];
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

            return copyOps.ToArray();
        }

        private static GltfInterleavedOperation CreateCopyOp(List<GltfInterleavedOperation> destination, GltfAccessor selected, GltfBufferView[] bufferViews)
        {
            if (!selected.BufferView.HasValue)
                throw new InvalidOperationException("unable to locate bufferview");

            var view = bufferViews[selected.BufferView.Value];

            var op = new GltfInterleavedOperation
            {
                BufferIndex = view.BufferIndex,                
                Count = selected.ElementCount,
                SrcOffset = (ulong)(view.BufferOffset + selected.ViewOffset),
                TotalSize = (uint) (selected.ElementCount * selected.NoOfComponents * selected.ElementByteSize),
            };

            op.ByteStride =
                (view.ByteStride.HasValue)
                ? (uint) view.ByteStride.Value
                : selected.NoOfComponents * selected.ElementByteSize;                    

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
