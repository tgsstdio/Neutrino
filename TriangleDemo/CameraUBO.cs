using Neutrino;
using System.Runtime.InteropServices;

namespace TriangleDemo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraUBO
    {
        public TkMatrix4 ModelViewProjection { get; set; }
        public TkVector4 Position { get; set; }
    }
}
