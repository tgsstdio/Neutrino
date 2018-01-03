using Magnesium;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neutrino.UnitTests
{
    [TestClass]
    public class SceneLoaderUnitTests
    {
        [TestMethod]
        public void TestMethod()
        {
            IMgGraphicsConfiguration config = new MockGraphicsConfiguration();
            var layout = new Magnesium.Utilities.VkDebugVertexPlatformMemoryLayout();
            var partitioner = new Magnesium.Utilities.MgOptimizedStoragePartitioner(layout);
            var verifier = new Magnesium.Utilities.MgOptimizedStoragePartitionVerifier(config);
            var builder = new Magnesium.Utilities.MgOptimizedStorageBuilder(config, partitioner, verifier);
            var pbrPath = new MockPbrEffectPath
            {
                FragPath = "Data/pbr.frag",
                VertPath = "Data/pbr.vert"
            };
            var loader = new GltfSceneLoader(config, builder, pbrPath);
            IMgDevice device = new MockDevice();
            IMgEffectFramework framework = new MockEffectFramework();
            loader.Load(device, framework, "Data/Triangle.gltf");
        }
    }
}
