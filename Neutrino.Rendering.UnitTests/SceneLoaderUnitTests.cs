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
            var loader = new GltfSceneLoader(config, builder);
            loader.Load("Data/Triangle.gltf");
        }
    }
}
