using Magnesium;
using Magnesium.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Neutrino.UnitTests
{
    class ScenePass
    {
        IMgEffectFramework Target { get; set; }
    }

    class MeshPrimitive
    {
        public uint PerVertexDefinition { get; set; }
        public int Mesh { get; set; }
        public int Material { get; set; }
        public int[] Vertices { get; set; }
        public int[] Indices { get; set; }
        public uint IndexCount { get; internal set; }
        public uint VertexCount { get; internal set; }
    }

    class RenderPool
    {
        public HashSet<int> Allocations { get; set; }
    }

    class SceneMaterial
    {

    }

    class SceneEffectVariant
    {
        public IMgPipelineLayout Layout { get; set; }
    }

    class SceneEffect
    {
        public SceneEffectVariant[] Variants { get; set; }
    }

    class SceneMesh
    {
        public int[] Effects { get; set; }
        public int[] MeshPrimitives { get; set; }
    }

    class SceneNode
    {
        public float Transform { get; set; }
        public int Mesh { get; set; }
    }

    class RenderScene
    {
        public SceneNode[] Nodes { get; set; }
        public SceneMesh[] Meshes { get; set; }
        public MeshPrimitive[] MeshPrimitives { get; set; }
        public List<SceneMaterial> Materials { get; set; }
    }

    class RenderGraph
    {
        public int GraphPriority { get; set; }
        public RenderScene Scene { get; set; }
        public ScenePass Pass { get; set; }

        public SceneEffect[] Effects { get; set; }
        public Dictionary<EffectVariantKey, RenderGroup> Groups { get; set; }
        public int MaterialsPerSet { get; set; }

        public IMgDescriptorSet[] DescriptorSets { get; set; }

        // ADD VARIANTS 

        public void FillAllNodes()
        {
            for (var i = 0; i < Scene.Nodes.Length; i += 1)
            {
                FillNodeAt(i);
            }
        }

        public void FillNodeAt(int i)
        {
            var node = Scene.Nodes[i];

            var mesh = Scene.Meshes[node.Mesh];

            foreach (var effect in mesh.Effects)
            {
                foreach (var primitiveIndex in mesh.MeshPrimitives)
                {
                    // build key 

                    var meshPrimitive = Scene.MeshPrimitives[primitiveIndex];

                    var key = new EffectVariantKey
                    {
                        Effect = effect,
                        Definition = GenerateVertexDefinition(meshPrimitive),
                        Options = GenerateOptionKey(node, meshPrimitive),
                    };

                    var setIndex = (meshPrimitive.Material / MaterialsPerSet);

                    if (!Groups.TryGetValue(key, out RenderGroup found))
                    {
                        // add new group to specific range
                        var noOfRanges = Math.DivRem(Scene.Materials.Count, MaterialsPerSet, out int remainder);
                        if (remainder > 0)
                        {
                            noOfRanges += 1;
                        }

                        found = new RenderGroup
                        {
                            Ranges = new List<RenderInstanceRange>(noOfRanges),
                        };

                        // fill ranges with nulls
                        for (var j = 0; j < noOfRanges; j += 1)
                        {
                            found.Ranges[j] = null;
                        }
                    }
                    var range = found.Ranges[setIndex];

                    if (range == null)
                    {
                        range = new RenderInstanceRange
                        {
                            InstanceDrawCalls = new List<RenderInstanceChunk>(),
                        };

                        found.Ranges[setIndex] = range;
                    }

                    var noOfDrawCalls = range.InstanceDrawCalls.Count;
                    if (noOfDrawCalls == 0)
                    {
                        range.InstanceDrawCalls.Add(RequestNewChunk());
                    }

                    var lastChunk =
                        range.InstanceDrawCalls[
                            (noOfDrawCalls > 2)
                            ? noOfDrawCalls - 1
                            : 0
                        ];

                    if (lastChunk.IsFull())
                    {
                        lastChunk = RequestNewChunk();
                        range.InstanceDrawCalls.Add(lastChunk);
                    }

                    var drawCall = new DrawInstance
                    {
                        Node = i,
                        MeshPrimitive = primitiveIndex,
                    };
                    lastChunk.DrawCalls.Add(drawCall);

                }
            }
        }

        MgOptimizedStorageContainer mStaticStorage;
        MgOptimizedStorageContainer mDynamicStorage;
        public void Build(IMgCommandBuffer cmdBuf, int sliceIndex)
        {
            cmdBuf.BeginCommandBuffer(new MgCommandBufferBeginInfo { });

            // TODO: RENDER PASS STUFF
                // CLEAR COLOR
                // VIEWPORTS 
                // SCISSORS

            foreach (var group in Groups.Values)
            {
                cmdBuf.CmdBindPipeline(MgPipelineBindPoint.GRAPHICS, group.Pipeline);
                var noOfDescriptorSets = DescriptorSets.Length;
                for (var i = 0; i < noOfDescriptorSets; i += 1)
                {
                    cmdBuf.CmdBindDescriptorSets(
                        MgPipelineBindPoint.GRAPHICS,
                        group.Layout,
                        0,
                        new[] { DescriptorSets[i] },
                        null
                    );

                    var range = group.Ranges[i];

                    var meshPrimitive = Scene.MeshPrimitives[range.MeshPrimitive];
                    foreach (var drawCall in range.InstanceDrawCalls)
                    {
                        // TODO storage cmdBuf.CmdBindVertexBuffers();
                        var perInstance = mDynamicStorage.Map.Allocations[drawCall.PerInstance[sliceIndex]];
                        var perInstanceBlock = mDynamicStorage.Storage.Blocks[perInstance.BlockIndex];

                        var perVertex = mStaticStorage.Map.Allocations[meshPrimitive.Vertices[sliceIndex]];
                        var perVertexBlock = mStaticStorage.Storage.Blocks[perVertex.BlockIndex];

                        cmdBuf.CmdBindVertexBuffers(
                            0,
                            new[] {
                                perVertexBlock.Buffer,
                                perInstanceBlock.Buffer,
                            },
                            new []
                            {
                                perVertex.Offset,
                                perInstance.Offset,
                            }
                        );

                        switch (group.DrawMode)
                        {
                            case LocalDrawMode.Draw:
                                CmdDraw(cmdBuf, meshPrimitive, drawCall, sliceIndex);
                                break;
                            case LocalDrawMode.DrawIndexed:
                                CmdDrawIndexed(cmdBuf, meshPrimitive, drawCall, sliceIndex);
                                break;
                            default:
                                throw new InvalidOperationException("draw mode not implemented");
                        }
                    }
                }
            }

            cmdBuf.EndCommandBuffer();
        }

        private void CmdDraw(IMgCommandBuffer cmdBuf, MeshPrimitive meshPrimitive, RenderInstanceChunk drawCall, int sliceIndex)
        {
            cmdBuf.CmdDraw(
                meshPrimitive.VertexCount,
                drawCall.InstanceCount,
                drawCall.FirstVertex,
                drawCall.FirstInstance);
        }

        private void CmdDrawIndexed(IMgCommandBuffer cmdBuf, MeshPrimitive meshPrimitive, RenderInstanceChunk drawCall, int sliceIndex)
        {
            var allocation = mStaticStorage.Map.Allocations[meshPrimitive.Indices[sliceIndex]];
            var indexBlock = mStaticStorage.Storage.Blocks[allocation.BlockIndex];

            var vertexDef = PerVertexDefinitionEncoder.Decode(meshPrimitive.PerVertexDefinition);

            var indexType = vertexDef.IndexType == PerVertexIndexType.Uint16
                ? MgIndexType.UINT16
                : MgIndexType.UINT32;

            cmdBuf.CmdBindIndexBuffer(indexBlock.Buffer, allocation.Offset, indexType);
            cmdBuf.CmdDrawIndexed(meshPrimitive.IndexCount,
                drawCall.InstanceCount,
                drawCall.FirstIndex, 
                drawCall.VertexOffset,
                drawCall.FirstInstance);
        }


        private RenderInstanceChunk RequestNewChunk()
        {
            return new RenderInstanceChunk
            {
                InstanceCount = 0,
                PerInstance = new int[] { },
                DrawCalls = new List<DrawInstance>(),
            };
        }

        private uint GenerateVertexDefinition(MeshPrimitive meshPrimitive)
        {
            return meshPrimitive.PerVertexDefinition;
        }

        private uint GenerateOptionKey(SceneNode node, MeshPrimitive meshPrimitive)
        {
            var transform = node.Transform;
            var material = Scene.Materials[meshPrimitive.Material];

            return 0U;
        }

        public void Clear()
        {

        }
    }

    public class GlobalData
    {

    }

    public enum LocalDrawMode
    {
        Draw,
        DrawIndexed,
        DrawIndirect,
        DrawIndexedIndirect,
    }

    public class RenderGroup
    {
        public LocalDrawMode DrawMode { get; set; }
        public IMgPipeline Pipeline { get; set; }
        public IMgPipelineLayout Layout { get; set; }
        public List<RenderInstanceRange> Ranges { get; set; }
    }

    public class RenderInstanceRange
    {
        public List<RenderInstanceChunk> InstanceDrawCalls { get; set; }
        public int MeshPrimitive { get; internal set; }
    }

    public class RenderInstanceChunk
    {
        public uint InstanceCount { get; set; }
        public int[] PerInstance { get; set; }
        public List<DrawInstance> DrawCalls { get; set; }
        public uint FirstIndex { get; internal set; }
        public int VertexOffset { get; internal set; }
        public uint FirstInstance { get; internal set; }
        public uint FirstVertex { get; internal set; }

        internal bool IsFull()
        {
            throw new NotImplementedException();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DrawInstance
    {
        public int Node { get; set; }
        public int MeshPrimitive { get; set; }
    }

    [TestClass]
    public class RenderGraphUnitTests
    {
        public RenderGroup GenerateGroup(uint noOfMaterials, uint upperLimit)
        {
            return new RenderGroup
            {
                Chunks = new List<RenderInstanceChunk>(),
            };
        }

        [TestMethod]
        public void TestMethod()
        {
            const uint NO_OF_MATERIALS = 11U;
            const uint UPPER_LIMIT = 5;

            var group = GenerateGroup(NO_OF_MATERIALS, UPPER_LIMIT);
            Assert.IsNotNull(group.Chunks);
        }
    }
}
