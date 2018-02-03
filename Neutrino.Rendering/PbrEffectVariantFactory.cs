using Magnesium;
using System;
using System.Diagnostics;

namespace Neutrino
{
    public class PbrEffectSettings
    {
        public uint NoOfCamerasPerGroup { get; set; }
        public uint NoOfLightsPerGroup { get; set; }
        public uint NoOfMaterialsPerGroup { get; internal set; }
        public uint NoOfTexturesPerGroup { get; internal set; }
    }

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

    public class PbrEffectVariantFactory : IEffectVariantFactory
    {
        private IPbrEffectPath mPath;
        private PbrEffectSettings mSettings;
        public PbrEffectVariantFactory(IPbrEffectPath path, PbrEffectSettings settings)
        {
            mPath = path;
        }

        public EffectLayout CreateEffectLayout(IMgDevice device)
        {
            var pDsCreateInfo = new MgDescriptorSetLayoutCreateInfo
            {
                Bindings = new[]
                {
                    // WORLD DATA
                        // CAMERAS 
                        // LIGHTS
                    new MgDescriptorSetLayoutBinding
                    {
                        Binding = 0,
                        DescriptorType = MgDescriptorType.UNIFORM_BUFFER,
                        DescriptorCount = 1,
                        StageFlags = MgShaderStageFlagBits.VERTEX_BIT,
                    },
                    // MATERIALS
                    new MgDescriptorSetLayoutBinding
                    {
                        Binding = 1,
                        DescriptorType = MgDescriptorType.UNIFORM_BUFFER,
                        DescriptorCount = mSettings.NoOfMaterialsPerGroup,
                        StageFlags = MgShaderStageFlagBits.FRAGMENT_BIT,
                    },
                    // TEXTURES
                    new MgDescriptorSetLayoutBinding
                    {
                        Binding = 5,
                        DescriptorType = MgDescriptorType.COMBINED_IMAGE_SAMPLER,
                        DescriptorCount = mSettings.NoOfTexturesPerGroup,
                        StageFlags = MgShaderStageFlagBits.FRAGMENT_BIT,
                    },
                }
            };

            var err = device.CreateDescriptorSetLayout(pDsCreateInfo, null, out IMgDescriptorSetLayout dsLayout);
            if (err != Result.SUCCESS)
                throw new InvalidOperationException("CreateDescriptorSetLayout failed");

            var pCreateInfo = new MgPipelineLayoutCreateInfo
            {
                SetLayouts = new[]
                {
                    dsLayout,
                }
            };

            err = device.CreatePipelineLayout(pCreateInfo, null, out IMgPipelineLayout layout);
            if (err != Result.SUCCESS)
                throw new InvalidOperationException("CreatePipelineLayout failed");

            return new EffectLayout(dsLayout, layout);
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
