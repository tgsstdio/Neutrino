using Magnesium;

namespace MonoGame.Graphics
{
	public class EffectPipeline
	{		
		//public ShaderProgram Program {get;set;}
		public VertexLayout Layout {get;set;}
		public ushort Options { get; set;}
		//public RenderPass Destination { get; set; }

		public IMgPipeline Handle { get; set; }

		public MgPipelineVertexInputStateCreateInfo ModelFormat { get; set; }
		public MgPipelineInputAssemblyStateCreateInfo IndexFormat { get; set; }
		public MgPipelineShaderStageCreateInfo[] ShaderModules { get; set; }
		public MgPipelineRasterizationStateCreateInfo Rasterization { get; set; }
		public MgPipelineDepthStencilStateCreateInfo DepthStencilState {get;set;}
		public MgPipelineColorBlendStateCreateInfo ColorBlendState { get; set; }
	}
}

