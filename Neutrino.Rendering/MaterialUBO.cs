using Magnesium;
using System.Runtime.InteropServices;

namespace Neutrino
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialUBO
    {
        // 0 : baseColorTexture
        // 1 : metallicRoughnessTexture	
        // 2:  normalTexture
        // 3:  occlusionTexture
        // 4:  emissiveTexture

        // 0
        public MgVec4f BaseColorFactor { get; set; }

        // 4 
        public ushort BaseTexture { get; set; }
        public ushort BaseTextureTexCoords { get; set; }

        public ushort MetalicRoughnessTexture { get; set; }
        public ushort MetalicRoughnessTexCoords { get; set; }

        public ushort NormalTexture { get; set; }
        public ushort NormalTexCoords { get; set; }

        public ushort EmissiveTexture { get; set; }
        public ushort EmissiveTexCoords { get; set; }

        // 8
        public ushort OcclusionTexture { get; set; }
        public ushort OcclusionTexCoords { get; set; }

        public float MetallicFactor { get; set; }
        public float RoughnessFactor { get; set; }
        public float NormalScale { get; set; }

        // 12
        public Color3f EmissiveFactor { get; set; }
        public float OcclusionStrength { get; set; }

        // 16
        public float AlphaCutoff { get; set; }
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
    }
}
