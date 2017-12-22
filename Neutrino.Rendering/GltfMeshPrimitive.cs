using Magnesium;

namespace Neutrino
{
    public class GltfMeshPrimitive
    {
        public MgPrimitiveTopology Topology { get; internal set; }
        public uint Definition { get; internal set; }
        public GltfBucketMarker Material { get; internal set; }
        internal IPerVertexDataLocator VertexLocations { get; set; }
    }
}
