using Magnesium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neutrino
{
    class PbrEffect
    {
        public EffectVariant Initialize(
            IMgEffectFramework framework,
            PerVertexDefinition definition,
            EffectVariantOptions options
        )
        {
            IMgDevice device = null;

            var createInfo = new MgGraphicsPipelineCreateInfo
            {
                
                VertexInputState = GenerateVertexInputState(definition),
                RenderPass = framework.Renderpass,
                RasterizationState = new MgPipelineRasterizationStateCreateInfo
                {
                    FrontFace = options.FrontFace,
                    CullMode = options.CullMode,
                },
                InputAssemblyState = new MgPipelineInputAssemblyStateCreateInfo
                {
                    Topology = options.Topology,
                },
            };

            var err = device.CreateGraphicsPipelines(null,
                new[] { createInfo }, null, out IMgPipeline[] pPipelines);

            return new EffectVariant
            {
                Definition = PerVertexDefinitionEncoder.Encode(definition),
                Options = EffectVariantEncoder.Encode(options),
                Pipeline = pPipelines[0],
            };
        }

        private MgPipelineVertexInputStateCreateInfo GenerateVertexInputState(PerVertexDefinition definition)
        {
            throw new NotImplementedException();
        }
    }
}
