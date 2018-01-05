using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraUBO
    {
        public TkMatrix4 ProjectionMatrix { get; set; }
        public TkMatrix4 ViewMatrix { get; set; }
        public TkMatrix4 CameraPosition { get; set; }
    }
}
