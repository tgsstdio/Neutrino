using glTFLoader;
using glTFLoader.Schema;
using Magnesium;
using Magnesium.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace GltfDemo
{
    public partial class GltfModelLoader : IGltfModelLoader
    {
        private readonly IMgGraphicsConfiguration mConfiguration;
        private readonly MgOptimizedStorageBuilder mBuilder;
        public GltfModelLoader(
            IMgGraphicsConfiguration config,
            MgOptimizedStorageBuilder builder
        ) {
            mConfiguration = config;
            mBuilder = builder;
        }

        public void Load(IEffect effect, string modelFilePath)
        {
            var model = Interface.LoadModel(modelFilePath);

            var baseDir = System.IO.Path.GetDirectoryName(modelFilePath);

            var cachedBuffers = LoadModelBuffers(model, baseDir);
            var bufferViews = ExtractBufferViews(model);

            foreach (var mesh in model.Meshes)
            {
                var optimizedMesh = GenerateMesh(model, mesh, effect, cachedBuffers, bufferViews);
            }
        }

        private static List<byte[]> LoadModelBuffers(glTFLoader.Schema.Gltf model, string baseDir)
        {
            var cachedBuffers = new List<byte[]>();
            foreach (var selectedBuffer in model.Buffers)
            {
                if (selectedBuffer.Uri.StartsWith("data:") && DataURL.FromUri(selectedBuffer.Uri, out DataURL output))
                {
                    if (output.Data.Length != selectedBuffer.ByteLength)
                    {
                        throw new InvalidDataException($"The specified length of embedded data chunk ({selectedBuffer.ByteLength}) is not equal to the actual length of the data chunk ({output.Data.Length}).");
                    }
                    cachedBuffers.Add(output.Data);
                }
                else
                {
                    // OPEN FILE
                    string additionalFilePath = System.IO.Path.Combine(baseDir, selectedBuffer.Uri);
                    using (var fs = File.Open(additionalFilePath, FileMode.Open))
                    using (var br = new BinaryReader(fs))
                    {
                        // ONLY READ SPECIFIED CHUNK SIZE
                        cachedBuffers.Add(br.ReadBytes(selectedBuffer.ByteLength));
                    }
                }
            }
            return cachedBuffers;
        }

        private MgOptimizedStorageContainer GenerateMesh(
            glTFLoader.Schema.Gltf model,
            glTFLoader.Schema.Mesh mesh,
            IEffect effect,
            List<byte[]> buffers,
            BufferViewInfo[] bufferViews
        )
        {
            var shaderLocations = effect.GetShaderAttributeLocations();
            var accessors = new List<GltfMeshAccessor>();
            uint primitiveIndex = 0U;
            foreach (var primitive in mesh.Primitives)
            {
                if (primitive.Indices.HasValue)
                {
                    var accessor = ExtractAccessor(model, primitive.Indices.Value);
                    accessor.PrimitiveIndex = primitiveIndex;
                    accessor.Usage = MgBufferUsageFlagBits.INDEX_BUFFER_BIT;
                    accessors.Add(accessor);
                }

                foreach (var attr in primitive.Attributes)
                {
                    var locationName = attr.Key;
                    var accessorIndex = attr.Value;

                    var accessor = ExtractAccessor(model, accessorIndex);
                    accessor.PrimitiveIndex = primitiveIndex;
                    accessor.Usage = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT;
                    accessor.LocationIndex = shaderLocations[locationName];
                    accessor.LocationName = locationName;

                    accessors.Add(accessor);
                }
                primitiveIndex += 1;
            }

            var usedBufferViews = new bool[bufferViews.Length];
            var blockAllocations = new List<MgStorageBlockAllocationInfo>();
            foreach (var attr in accessors)
            {
                var allocation = new MgStorageBlockAllocationInfo
                {
                    MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                    Usage = attr.Usage,
                    ElementByteSize = attr.ElementByteSize,
                    Size = (ulong)(attr.NoOfComponents * attr.ElementCount * attr.ElementByteSize),
                };

                if (attr.BufferViewIndex.HasValue)
                {
                    usedBufferViews[attr.BufferViewIndex.Value] = true;
                }

                blockAllocations.Add(allocation);
            }

            var createInfo = new MgOptimizedStorageCreateInfo
            {
                Allocations = blockAllocations.ToArray(),
            };

            var meshData = mBuilder.Build(createInfo);

            var metaData = InitializeMetaData(meshData, usedBufferViews, bufferViews, accessors);

            // copy buffer data into device memory
            CopyBuffersInto(meshData, buffers, bufferViews, accessors);

            return meshData;
        }

        private static GltfMeshAccessor ExtractAccessor(Gltf model, int accessorIndex)
        {
            var buffAttr = new GltfMeshAccessor();
            var accessor = model.Accessors[accessorIndex];

            buffAttr.BufferViewIndex = accessor.BufferView;
            buffAttr.AccessorIndex = accessorIndex;
            buffAttr.Format = DetermineFormat(accessor.Type, accessor.ComponentType);
            buffAttr.ElementByteSize = DetermineByteSize(accessor.ComponentType);
            buffAttr.NoOfComponents = DetermineNoOfComponents(accessor.Type);
            buffAttr.ElementCount = accessor.Count;
            return buffAttr;
        }

        class MeshMetaData
        {
            public MeshDrawCommand[] Commands { get; set; }
            public MeshMetaPipeline Pipeline { get; set; }
        }

        class MeshDrawCommand
        {
            public uint PrimitiveIndex { get; set; }
            public List<VertexDataBinding> Vertices { get; set; }
            public IndexDataBinding IndexBuffer { get; set; }
            public DescriptorSetBinding[] DescriptorSets { get; set; }
        }

        class MeshMetaPipeline
        {
            public MgVertexInputBindingDescription[] VertexBindingDescriptions { get; set; }
            public MgVertexInputAttributeDescription[] VertexAttributeDescriptions { get; set; }
        }

        MeshMetaData InitializeMetaData(MgOptimizedStorageContainer container, bool[] usedBufferViews, BufferViewInfo[] bufferViews, List<GltfMeshAccessor> accessors)
        {
            var bindings = ExtractBindings(container, bufferViews, accessors);

            var inputAttributes = ExtractAttributeDescriptions(container, accessors);

            var commands = ExtractDrawCommand(container, accessors);

            return new MeshMetaData
            {
                Commands = commands,
                Pipeline = new MeshMetaPipeline
                {
                    VertexBindingDescriptions = bindings.ToArray(),
                    VertexAttributeDescriptions = inputAttributes.ToArray(),
                }
            };
        }

        private MeshDrawCommand[] ExtractDrawCommand(MgOptimizedStorageContainer container, List<GltfMeshAccessor> accessors)
        {
            const MgBufferUsageFlagBits VERT_MASK = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT;
            const MgBufferUsageFlagBits INDEX_MASK = MgBufferUsageFlagBits.INDEX_BUFFER_BIT;
            var primitives = new Dictionary<uint, MeshDrawCommand>();

            for (var i = 0; i < accessors.Count; i += 1)
            {
                var srcAttribute = accessors[i];
                if (!primitives.TryGetValue(srcAttribute.PrimitiveIndex, out MeshDrawCommand found))
                {
                    found = new MeshDrawCommand
                    {
                        PrimitiveIndex = srcAttribute.PrimitiveIndex,
                        Vertices = new List<VertexDataBinding>(),
                    };
                    primitives.Add(srcAttribute.PrimitiveIndex, found);
                }

                if ((srcAttribute.Usage & VERT_MASK) == VERT_MASK)
                {
                    var dstAttribute = container.Map.Allocations[i];
                    var dstInstance = container.Storage.Blocks[dstAttribute.BlockIndex];

                    var vertexInfo = new VertexDataBinding
                    {
                        BlockIndex = dstAttribute.BlockIndex,
                        Buffer = dstInstance.Buffer,
                        Offset = dstAttribute.Offset + dstInstance.MemoryOffset,
                    };
                    found.Vertices.Add(vertexInfo);
                }

                if ((srcAttribute.Usage & INDEX_MASK) == INDEX_MASK)
                {
                    var dstAttribute = container.Map.Allocations[i];
                    var dstInstance = container.Storage.Blocks[dstAttribute.BlockIndex];

                    var indexInfo = new IndexDataBinding
                    {
                        Buffer = dstInstance.Buffer,
                        IndexType = (srcAttribute.Format == MgFormat.R32_UINT)
                            ? MgIndexType.UINT32
                            : MgIndexType.UINT16,
                        Offset = dstAttribute.Offset + dstInstance.MemoryOffset,
                    };
                    found.IndexBuffer = indexInfo;
                }
            }

            var commands = new MeshDrawCommand[primitives.Count];
            primitives.Values.CopyTo(commands, 0);
            return commands;
        }

        class VertexDataBinding
        {
            public uint BlockIndex { get; set; }
            public IMgBuffer Buffer { get; set; }
            public ulong Offset { get; set; }
        }

        class IndexDataBinding
        {
            public IMgBuffer Buffer { get; set; }
            public ulong Offset { get; set; }
            public MgIndexType IndexType { get; set; }
        }

        class DescriptorSetBinding
        {
            public MgPipelineBindPoint Bindpoint { get; set; }
            public uint FirstBinding { get; set; }
            public IMgPipelineLayout Layout { get; set; }
            public IMgDescriptorSet[] DescriptorSets { get; set; }
            public uint[] Offsets { get; set; }
        }

        class IndirectBinding
        {
            public IMgBuffer Buffer { get; set; }
            public ulong Offset { get; set; }
            public uint DrawCount { get; set; }
            public uint Stride { get; set; }
        }

        private static List<MgVertexInputBindingDescription> ExtractBindings(MgOptimizedStorageContainer container, BufferViewInfo[] bufferViews, List<GltfMeshAccessor> accessors)
        {
            var bindings = new List<MgVertexInputBindingDescription>();
            const MgBufferUsageFlagBits VERT_MASK = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT;
            for (var i = 0U; i < container.Storage.Blocks.Length; i += 1)
            {
                var srcBlock = bufferViews[i];
                var dstBlock = container.Storage.Blocks[i];

                if ((dstBlock.Usage & VERT_MASK) == VERT_MASK)
                {
                    var byteStride = 0U;

                    if (srcBlock.ByteStride.HasValue)
                    {
                        byteStride = (uint)srcBlock.ByteStride.Value;
                    }
                    else
                    {
                        foreach (var order in dstBlock.PackingOrder)
                        {
                            var attrIndex = (int)order;
                            var srcAttribute = accessors[attrIndex];
                            var dstAttribute = container.Map.Allocations[attrIndex];

                            if ((dstAttribute.Usage & VERT_MASK) == VERT_MASK)
                            {
                                byteStride += (uint)(srcAttribute.ElementByteSize * srcAttribute.ElementCount);
                            }
                        }
                    }

                    var binding = new MgVertexInputBindingDescription
                    {
                        Binding = (uint)bindings.Count,
                        InputRate = MgVertexInputRate.VERTEX,
                        Stride = byteStride,
                    };
                    bindings.Add(binding);
                }
            }

            return bindings;
        }

        private static List<MgVertexInputAttributeDescription> ExtractAttributeDescriptions(MgOptimizedStorageContainer container, List<GltfMeshAccessor> accessors)
        {
            const MgBufferUsageFlagBits VERT_MASK = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT;
            var inputAttributes = new List<MgVertexInputAttributeDescription>();
            for (var i = 0; i < accessors.Count; i += 1)
            {
                var srcAttribute = accessors[i];
                var dstAttribute = container.Map.Allocations[i];

                if ((dstAttribute.Usage & VERT_MASK) == VERT_MASK)
                {
                    var attribute = new MgVertexInputAttributeDescription
                    {
                        Location = srcAttribute.LocationIndex,
                        Binding = dstAttribute.BlockIndex,
                        Format = srcAttribute.Format,
                        Offset = (uint)dstAttribute.Offset,
                    };

                    inputAttributes.Add(attribute);
                }
            }

            return inputAttributes;
        }

        private void CopyBuffersInto(MgOptimizedStorageContainer container, List<byte[]> buffers, BufferViewInfo[] bufferViews, List<GltfMeshAccessor> attributes)
        {
            for (var i = 0; i < attributes.Count; i += 1)
            {
                var src = attributes[i];

                if (src.BufferViewIndex.HasValue)
                {
                    var srcView = bufferViews[src.BufferViewIndex.Value];
                    var dst = container.Map.Allocations[i];

                    var dstBuffer = container.Storage.Blocks[dst.BlockIndex];

                    var dstDeviceMemory = dstBuffer.DeviceMemory;

                    var err = dstBuffer.DeviceMemory.MapMemory(
                        mConfiguration.Device,
                        dst.Offset,
                        dst.Size,
                        0U,
                        out IntPtr ppData);

                    if (err == Result.SUCCESS)
                    {
                        // COPY HERE
                        byte[] srcData = buffers[srcView.BufferIndex];

                        //Marshal.Copy(srcData, 0, ppData, (int) dst.Size);

                        dstBuffer.DeviceMemory.UnmapMemory(mConfiguration.Device);
                    }
                }
            }
        }

        private static BufferViewInfo[] ExtractBufferViews(glTFLoader.Schema.Gltf model)
        {
            var copyFromBufferView = new BufferViewInfo[model.BufferViews.Length];
            for (var i = 0; i < copyFromBufferView.Length; i += 1)
            {
                var src = model.BufferViews[i];
                var dst = new BufferViewInfo()
                {
                    BufferIndex = src.Buffer,
                    ByteStride = src.ByteStride,
                };
                switch (src.Target)
                {
                    case glTFLoader.Schema.BufferView.TargetEnum.ARRAY_BUFFER:
                        dst.Usage = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT;
                        break;
                    case glTFLoader.Schema.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER:
                        dst.Usage = MgBufferUsageFlagBits.INDEX_BUFFER_BIT;
                        break;
                    default:
                        throw new NotSupportedException($"specified target:({src.Target}) not supported");
                }
                copyFromBufferView[i] = dst;
            }

            return copyFromBufferView;
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
    }
}
