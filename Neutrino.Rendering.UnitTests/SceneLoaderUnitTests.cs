using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Neutrino.UnitTests
{

    [TestClass]
    public partial class SceneLoaderUnitTests
    {
        // public void TestMethod()
        //{
        //    IMgGraphicsConfiguration config = new MockGraphicsConfiguration();
        //    var layout = new Magnesium.Utilities.VkDebugVertexPlatformMemoryLayout();
        //    var partitioner = new Magnesium.Utilities.MgOptimizedStoragePartitioner(layout);
        //    var verifier = new Magnesium.Utilities.MgOptimizedStoragePartitionVerifier(config);
        //    var builder = new Magnesium.Utilities.MgOptimizedStorageBuilder(config, partitioner, verifier);
        //    var pbrPath = new MockPbrEffectPath
        //    {
        //        FragPath = "Data/pbr.frag",
        //        VertPath = "Data/pbr.vert"
        //    };
        //    var loader = new GltfSceneLoader(config, builder, pbrPath);
        //    IMgDevice device = new MockDevice();
        //    IMgEffectFramework framework = new MockEffectFramework();
        //    loader.Load(device, framework, "Data/Triangle.gltf");
        //}

        [TestMethod]
        public void LoadFile()
        {
            var loader = new Loader();

            using (var fs = File.Open("Data/Triangle.gltf", FileMode.Open))
            {
                var model = glTFLoader.Interface.LoadModel(fs);
                var actual = loader.LoadMetaData(model);
                Assert.IsNotNull(actual);
            }
        }
    }
}
