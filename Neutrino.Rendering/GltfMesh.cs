namespace Neutrino
{
    public class GltfMesh
    {
        public string Name { get; set; }
        public float[] Weights { get; set; }
        public GltfMeshPrimitive[] Primitives { get; set; }
    }
}
