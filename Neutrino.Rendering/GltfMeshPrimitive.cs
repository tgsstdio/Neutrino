using Magnesium;

namespace Neutrino
{
    public class GltfMeshPrimitive
    {
        public MgPrimitiveTopology Topology { get; internal set; }
        public PerVertexDefinition InitialDefinition { get; internal set; }
        public GltfBucketMarker Material { get; internal set; }
        public PerVertexDefinition FinalDefinition { get; internal set; }
        public uint VertexCount { get; internal set; }
        internal IPerVertexDataLocator VertexLocations { get; set; }
    }
}
