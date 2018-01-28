using Magnesium;

namespace Neutrino
{
    public interface IEffectVariantFactory
    {
        EffectVariant Initialize(
            IMgDevice device,
            IMgPipelineLayout layout,
            IMgRenderPass renderPass,
            PerVertexInputPipelineState vertexInput,
            EffectVariantOptions options
        );
    }
}
