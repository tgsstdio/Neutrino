using Magnesium;

namespace Neutrino
{
    public class GltfMeshPrimitive
    {
        public MgPrimitiveTopology Topology { get; internal set; }
        public int? Indices { get; internal set; }
        public uint Definition { get; internal set; }
        public int[] VertexData { get; internal set; }
        public int? Material { get; internal set; }
    }
}
