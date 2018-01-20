using Magnesium;

namespace Neutrino
{
    public class GltfAccessor
    {
        public int? BufferView { get; set; }
        public int ViewOffset { get; set; }
        public MgFormat Format { get; set; }
        public GltfElementType ElementType { get; set; }
        public uint ElementByteSize { get; set; }
        public uint NoOfComponents { get; set; }
        public int ElementCount { get; set; }
        public ulong TotalByteSize { get; set; }
    }
}
