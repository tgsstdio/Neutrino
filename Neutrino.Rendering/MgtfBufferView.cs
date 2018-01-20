using Magnesium;

namespace Neutrino
{
    public class MgtfBufferView
    {        
        public MgBufferUsageFlagBits Usage { get; set; }
        public int BufferIndex { get; set; }
        public int? ByteStride { get; set; }
        public int BufferOffset { get; set; }
        public int ByteLength { get; set; }
    }
}
