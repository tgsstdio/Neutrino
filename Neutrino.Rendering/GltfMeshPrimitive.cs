using Magnesium;

namespace Neutrino
{
    public class GltfMeshPrimitive
    {
        public MgPrimitiveTopology Topology { get; internal set; }
        public uint Definition { get; internal set; }
        public int? Material { get; internal set; }
        internal IPerVertexDataLocator VertexLocations { get; set; }
    }
}
