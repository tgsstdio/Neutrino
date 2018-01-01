using Magnesium;

namespace Neutrino
{
    interface IEffectVariantFactory
    {
        EffectVariant Initialize(
            IMgDevice device,
            IMgPipelineLayout layout,
            IMgEffectFramework framework,
            PerVertexInputPipelineState vertexInput,
            EffectVariantOptions options
        );
    }
}
