namespace Neutrino
{
    class GltfPrimitiveStorageLocation
    {
        public int? Index { get; set; }
        public int Vertex { get; set; }
        public GltfInterleavedOperation[] CopyOperations { get; internal set; }
    }
}
