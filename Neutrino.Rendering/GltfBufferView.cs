using glTFLoader.Schema;
using Magnesium;
using System;

namespace Neutrino
{
    public class GltfBufferView
    {        
        public GltfBufferView(BufferView src)
        {
            BufferIndex = src.Buffer;
            ByteStride = src.ByteStride;
            BufferOffset = src.ByteOffset;            
            Usage = DetrimineUsage(src);
        }

        private static MgBufferUsageFlagBits DetrimineUsage(BufferView src)
        {
            switch (src.Target)
            {
                case glTFLoader.Schema.BufferView.TargetEnum.ARRAY_BUFFER:
                    return MgBufferUsageFlagBits.VERTEX_BUFFER_BIT;
                case glTFLoader.Schema.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER:
                    return MgBufferUsageFlagBits.INDEX_BUFFER_BIT;
                default:
                    throw new NotSupportedException($"specified target:({src.Target}) not supported");
            }
        }

        public MgBufferUsageFlagBits Usage { get; set; }
        public int BufferIndex { get; private set; }
        public int? ByteStride { get; private set; }
        public int BufferOffset { get; private set; }
    }
}
