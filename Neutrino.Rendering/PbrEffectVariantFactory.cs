using Magnesium;
using System;
using System.Diagnostics;

namespace Neutrino
{
    public class PbrEffectVariantFactory : IEffectVariantFactory
    {
        private IPbrEffectPath mPath;
        public PbrEffectVariantFactory(IPbrEffectPath path)
        {
            mPath = path;
        }

        public EffectVariant Initialize(
            IMgDevice device,
            IMgPipelineLayout layout,
            IMgRenderPass renderPass,
            PerVertexInputPipelineState vertexInput,            
            EffectVariantOptions options
        )
        {
            using (var vertFs = mPath.OpenVertexShader())
            using (var fragFs = mPath.OpenFragmentShader())
            {
                var vsCreateInfo = new MgShaderModuleCreateInfo
                {
                    Code = vertFs,
                    CodeSize = new UIntPtr((ulong)vertFs.Length),
                };
                device.CreateShaderModule(vsCreateInfo, null, out IMgShaderModule vsModule);
                
                var fsCreateInfo = new MgShaderModuleCreateInfo
                {
                    Code = fragFs,
                    CodeSize = new UIntPtr((ulong)fragFs.Length),
                };
                device.CreateShaderModule(fsCreateInfo, null, out IMgShaderModule fsModule);                

                var createInfo = new MgGraphicsPipelineCreateInfo
                {
                    Stages = new MgPipelineShaderStageCreateInfo[]
                    {
                        new MgPipelineShaderStageCreateInfo
                        {
                            Stage = MgShaderStageFlagBits.VERTEX_BIT,
                            Module = vsModule,
                            Name = "vertFunc",
                        },
                        new MgPipelineShaderStageCreateInfo
                        {
                            Stage = MgShaderStageFlagBits.FRAGMENT_BIT,
                            Module = fsModule,
                            Name = "fragFunc",
                        },
                    },

                    ColorBlendState = new MgPipelineColorBlendStateCreateInfo
                    {
                        Attachments = new[]
                        {
                            new MgPipelineColorBlendAttachmentState
                            {
                                ColorWriteMask =  MgColorComponentFlagBits.ALL_BITS,
                                BlendEnable = false,
                            }
                        },
                    },
                    DepthStencilState = new MgPipelineDepthStencilStateCreateInfo
                    {
                        DepthTestEnable = true,
                        DepthWriteEnable = true,
                        DepthCompareOp = MgCompareOp.LESS_OR_EQUAL,
                        DepthBoundsTestEnable = false,
                        Back = new MgStencilOpState
                        {
                            FailOp = MgStencilOp.KEEP,
                            PassOp = MgStencilOp.KEEP,
                            CompareOp = MgCompareOp.ALWAYS,
                        },
                        StencilTestEnable = false,
                        Front = new MgStencilOpState
                        {
                            FailOp = MgStencilOp.KEEP,
                            PassOp = MgStencilOp.KEEP,
                            CompareOp = MgCompareOp.ALWAYS,
                        },
                    },
                    DynamicState = new MgPipelineDynamicStateCreateInfo
                    {
                        DynamicStates = new[]
                        {
                            MgDynamicState.VIEWPORT,
                            MgDynamicState.SCISSOR,
                        }
                    },
                    InputAssemblyState = new MgPipelineInputAssemblyStateCreateInfo
                    {
                        Topology = options.Topology,
                    },
                    Layout = layout,
                    MultisampleState = new MgPipelineMultisampleStateCreateInfo
                    {
                        RasterizationSamples = MgSampleCountFlagBits.COUNT_1_BIT,
                        SampleMask = null,
                    },                    
                    RasterizationState = new MgPipelineRasterizationStateCreateInfo
                    {
                        PolygonMode = MgPolygonMode.FILL,
                        FrontFace = options.FrontFace,
                        CullMode = options.CullMode,
                        DepthClampEnable = false,
                        RasterizerDiscardEnable = false,
                        DepthBiasEnable = false,
                        LineWidth = 1.0f,
                    },
                    RenderPass = renderPass,
                    VertexInputState = vertexInput.VertexInputState,
                    ViewportState = null,
                };

                var err = device.CreateGraphicsPipelines(null,
                    new[] { createInfo }, null, out IMgPipeline[] pPipelines);

                Debug.Assert(err == Result.SUCCESS);

                vsModule.DestroyShaderModule(device, null);
                fsModule.DestroyShaderModule(device, null);

                return new EffectVariant
                {
                    Key = new EffectVariantKey
                    {
                        Definition = vertexInput.VertexMask,
                        Options = EffectVariantEncoder.Encode(options),
                    },
                    Pipeline = pPipelines[0],
                };
            }
        }

    }
}
