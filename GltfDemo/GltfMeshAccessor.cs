using glTFLoader.Schema;
using Magnesium;

namespace GltfDemo
{
    public class GltfMeshAccessor
    {
        public uint LocationIndex { get; internal set; }
        public int AccessorIndex { get; internal set; }
        public string LocationName { get; internal set; }
        public MgFormat Format { get; internal set; }
        public uint ElementByteSize { get; internal set; }
        public uint NoOfComponents { get; internal set; }
        public int ElementCount { get; internal set; }
        public int? BufferViewIndex { get; internal set; }
        public MgBufferUsageFlagBits Usage { get; internal set; }
        public uint PrimitiveIndex { get; internal set; }
    }
}
