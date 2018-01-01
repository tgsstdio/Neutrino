namespace Neutrino
{
    class GltfInterleavedOperation
    {
        public int Count { get; set; }
        public uint TotalSize { get; set; }
        public ulong SrcOffset { get; set; }
        public ulong DstStride { get; set; }
        public int BufferIndex { get; internal set; }
        public uint ByteStride { get; internal set; }
    }
}
