using Magnesium;

namespace Neutrino
{
    public class Effect
    {
        public IMgDescriptorSetLayout DescriptorSetLayout { get; set; }
        public IMgPipelineLayout Layout { get; set; }
        public EffectPipelineDictionary Variants { get; set; }
    }
}
