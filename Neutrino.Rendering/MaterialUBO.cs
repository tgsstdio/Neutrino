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
        public int BaseTexture { get; set; }
        public int BaseTextureTexCoords { get; set; }
        public int EmissiveTexture { get; set; }
        public int EmissiveTexCoords { get; set; }

        // 8
        public Color3f EmissiveFactor { get; set; }
        public float NormalScale { get; set; }

        // 12
        public int MetalicRoughnessTexture { get; set; }
        public int MetalicRoughnessTexCoords { get; set; }
        public float MetallicFactor { get; set; }
        public float RoughnessFactor { get; set; }

        // 16
        public int NormalTexture { get; set; }
        public int NormalTexCoords { get; set; }
        public int OcclusionTexture { get; set; }
        public int OcclusionTexCoords { get; set; }

        // 20
        public float OcclusionStrength { get; set; }
        public float AlphaCutoff { get; set; }
        public float A { get; set; }
        public float B { get; set; }

        // 24
        public float C { get; set; }
        public float PerceptualRoughness { get; set; }
        public float Metallic { get; set; }
        public float F { get; set; }

    }
}
