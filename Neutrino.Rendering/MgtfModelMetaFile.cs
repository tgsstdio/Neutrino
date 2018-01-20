namespace Neutrino
{
    public class MgtfModelMetaFile
    {
        public MgtfAccessor[] Accessors { get; internal set; }
        public MgtfCamera[] Cameras { get; internal set; }
        public MgtfBufferView[] BufferViews { get; internal set; }
        public MgtfMaterial[] Materials { get; internal set; }
        public MgtfMesh[] Meshes { get; internal set; }
        public MgtfNode[] Nodes { get; internal set; }
        public MgtfSampler[] Samplers { get; internal set; }
        public MgtfTexture[] Textures { get; internal set; }
    }
}
