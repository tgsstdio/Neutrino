namespace Neutrino
{
    public class GltfInstancedDraw
    {
        public EffectVariantKey Key { get; set; }
        public PerInstance Instance { get; set; }
        public GltfInstancedGroupKey GroupKey { get; internal set; }
    }
}
