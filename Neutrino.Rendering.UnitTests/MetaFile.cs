namespace Neutrino.UnitTests
{
    public class MetaFile
    {
        public GltfAccessor[] Accessors { get; internal set; }
        public GltfCamera[] Cameras { get; internal set; }
        public GltfBufferView[] BufferViews { get; internal set; }
        public GltfMaterial[] Materials { get; internal set; }
        public GltfMesh[] Meshes { get; internal set; }
        public GltfNodeInfo[] Nodes { get; internal set; }
        public GltfSampler[] Samplers { get; internal set; }
        public GltfTexture[] Textures { get; internal set; }
    }
}
