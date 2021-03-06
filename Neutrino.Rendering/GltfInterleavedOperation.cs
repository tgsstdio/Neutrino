﻿namespace Neutrino
{
    public class GltfInterleavedOperation
    {
        public int Count { get; set; }
        public uint TotalSize { get; set; }
        public ulong SrcOffset { get; set; }
        public ulong DstStride { get; set; }
        public int BufferIndex { get; set; }
        public uint ByteStride { get; set; }
    }
}
