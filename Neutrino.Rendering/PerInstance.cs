using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerInstance
    {
        public TkVector3 Translation;
        public TkVector3 Scale;
        public TkQuaternion Rotation;
        public int CameraIndex;
        public int MaterialIndex;
        public int LightIndex;
    }
}
