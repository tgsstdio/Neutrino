using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerInstance
    {
        public TkVector3 Translation { get; set; }
        public TkVector3 Scale { get; set; }
        public TkVector4 Rotation { get; set; }
        public int CameraIndex { get; set; }
        public int MaterialIndex { get; set; }
    }
}
