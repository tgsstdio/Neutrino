using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color3f
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
    }
}
