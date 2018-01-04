using Magnesium;
using Magnesium.Utilities;

namespace Neutrino
{
    public class GltfScene
    {
        public GltfSceneLoader.GltfInstanceRenderGroup[] PerInstances { get; internal set; }
        public MgOptimizedStorageContainer Storage { get; internal set; }
        public IMgDescriptorSet[] DescriptorSets { get; internal set; }
        public GltfSceneLoader.GltfMaterialInfo[] Materials { get; internal set; }
        public IMgSampler[] Samplers { get; internal set; }
        internal Effect[] Effects { get; set; }
    }
}