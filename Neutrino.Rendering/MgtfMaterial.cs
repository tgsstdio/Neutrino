using Magnesium;

namespace Neutrino
{
    public class MgtfMaterial
    {
        public MgVec4f BaseColorFactor { get; set; }
        public MgtfMaterialTexture BaseColorTexture { get; set; }
        public MgtfMaterialTexture NormalTexture { get; set; }
        public float NormalScale { get; set; }
        public MgtfMaterialTexture EmissiveTexture { get; set; }
        public Color3f EmissiveFactor { get; set; }
        public MgtfMaterialTexture OcclusionTexture { get; set; }
        public float OcclusionStrength { get; set; }
        public bool DoubleSided { get; set; }
        public float AlphaCutoff { get; set; }
        public MgtfAlphaModeEquation AlphaMode { get; set; }
    }
    
}
