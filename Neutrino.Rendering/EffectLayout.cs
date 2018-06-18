using Magnesium;

namespace Neutrino
{
    public class EffectLayout
    {
        public EffectLayout(
            IMgDescriptorSetLayout descriptorSetLayout,
            IMgPipelineLayout layout
        )
        {
            DescriptorSetLayout = descriptorSetLayout;
            Layout = layout;
        }

        public IMgDescriptorSetLayout DescriptorSetLayout { get; }
        public IMgPipelineLayout Layout { get; }

        public void Destroy(IMgDevice device)
        {
            if (Layout != null)
            {
                Layout.DestroyPipelineLayout(device, null);
            }

            if (DescriptorSetLayout != null)
            {
                DescriptorSetLayout.DestroyDescriptorSetLayout(device, null);
            }
        }
    }
}
