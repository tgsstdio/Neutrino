using Magnesium;

namespace Neutrino.UnitTests
{
    public class GltfMaterial
    {
        public MgVec4f BaseColorFactor { get; set; }
        public GltfMaterialTexture BaseColorTexture { get; set; }
        public GltfMaterialTexture NormalTexture { get; set; }
        public float NormalScale { get; set; }
        public GltfMaterialTexture EmissiveTexture { get; set; }
        public Color3f EmissiveFactor { get; set; }
        public GltfMaterialTexture OcclusionTexture { get; set; }
        public float OcclusionStrength { get; set; }
        public bool DoubleSided { get; set; }
        public float AlphaCutoff { get; set; }
        public AlphaModeEquation AlphaMode { get; set; }

        public class GltfMaterialTexture
        {
            public int? Texture { get; set; }
            public int TexCoords { get; set; }
        }

        public class AlphaModeEquation
        {
            public float A { get; set; }
            public float B { get; set; }
            public float C { get; set; }
        }
    }
    
}
