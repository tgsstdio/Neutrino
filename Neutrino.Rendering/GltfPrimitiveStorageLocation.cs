namespace Neutrino
{
    public class GltfPrimitiveStorageLocation
    {
        public int? Index { get; set; }
        public int Vertex { get; set; }
        public GltfInterleavedOperation[] CopyOperations { get; set; }
        public PerVertexDefinition FinalDefinition { get; set; }
        public int Mesh { get; set; }
        public int MeshPrimitive { get; set; }
    }
}
