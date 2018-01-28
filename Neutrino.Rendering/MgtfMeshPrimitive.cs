using Magnesium;

namespace Neutrino
{
    public class MgtfMeshPrimitive
    {
        public MgPrimitiveTopology Topology { get; set; }
        public PerVertexDefinition InitialDefinition { get; set; }
        public int? Material { get; set; }
        public uint VertexCount { get; set; }
        public IMgtfPerVertexDataLocator VertexLocations { get; set; }
        public uint IndexCount { get; set; }
    }
}
