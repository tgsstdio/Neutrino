using Magnesium;

namespace GltfDemo
{
    public class BufferViewInfo
    {
        public MgBufferUsageFlagBits Usage { get; set; }
       // public bool CopyRequired { get; set; }
        public int BufferIndex { get; internal set; }
        public int? ByteStride { get; internal set; }
    }    
}
