using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerInstance
    {
        public TkVector3 Position { get; set; }
        public TkVector3 Scale { get; set; }
        public TkVector4 Rotation { get; set; }
        public uint CameraIndex { get; set; }
        public uint MaterialIndex { get; set; }
    }
}
