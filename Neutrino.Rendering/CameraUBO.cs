using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    struct CameraUBO
    {
        public TkMatrix4 ProjectionMatrix { get; set; }
        public TkMatrix4 ViewMatrix { get; set; }
    }
}
