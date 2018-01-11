using Magnesium;

namespace Neutrino
{
    public class GltfMeshPrimitive
    {
        public MgPrimitiveTopology Topology { get; set; }
        public PerVertexDefinition InitialDefinition { get; set; }
        public int? Material { get; set; }
        public uint VertexCount { get; set; }
        public IPerVertexDataLocator VertexLocations { get; set; }
    }
}
