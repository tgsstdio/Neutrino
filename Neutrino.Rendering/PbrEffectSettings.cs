namespace Neutrino
{
    public class PbrEffectSettings
    {
        public uint NoOfCamerasPerGroup { get; set; }
        public uint NoOfLightsPerGroup { get; set; }
        public uint NoOfMaterialsPerGroup { get; internal set; }
        public uint NoOfTexturesPerGroup { get; internal set; }
    }
}
