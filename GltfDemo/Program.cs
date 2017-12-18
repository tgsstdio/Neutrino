using System;
using Magnesium;
using Magnesium.Utilities;

namespace GltfDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            //const string modelFilePath = "glTF/TriangleWithoutIndices.gltf";
            const string modelFilePath = "glTF/Triangle.gltf";

            // collate attributes to allocations
            var partition = new MockPartition();
            partition.ReturnValue = 0;
            partition.IsValid = true;
            IMgGraphicsConfiguration config = new MockGraphicsConfiguration(partition);

            IMgPlatformMemoryLayout layout = new WGLPlatformMemoryLayout();
            IMgOptimizedStoragePartitioner segmenter = new MgOptimizedStoragePartitioner(layout);
            IMgOptimizedStoragePartitionVerifier verifier = new MgOptimizedStoragePartitionVerifier(config);
            var builder = new MgOptimizedStorageBuilder(
                config,
                segmenter,
                verifier);

            IGltfModelLoader loader = new GltfModelLoader(config, builder);
            var effect = new OrdinaryEffect();
            loader.Load(effect, modelFilePath);

            Console.ReadKey();
        }
    }
}
