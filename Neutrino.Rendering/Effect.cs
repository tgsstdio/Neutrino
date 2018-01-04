using Magnesium;

namespace Neutrino
{
    class Effect
    {
        public IMgDescriptorSetLayout DescriptorSetLayout { get; set; }
        public IMgPipelineLayout Layout { get; set; }
        public EffectPipelineDictionary Variants { get; set; }
    }
}
