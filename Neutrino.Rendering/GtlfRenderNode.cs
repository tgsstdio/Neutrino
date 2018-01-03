namespace Neutrino
{
    public class GtlfNodeInfo
    {
        public int? Parent { get; set; }
        public int[] Children { get; set; }
        public GltfBucketMarker CameraAllocation { get; set; }
        public TkMatrix4 Transform { get; set; }
        public bool IsMirrored { get; internal set; }
        public int NodeIndex { get; internal set; }
        public string Name { get; internal set; }
        public int? Mesh { get; internal set; }
    }
}
