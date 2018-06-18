using Magnesium;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Neutrino.UnitTests
{
    [TestClass]
    public class InstanceGroupsDrawUnits
    {
        class EffectPass
        {
            public string Name { get; set; }
        }

        class InstanceGroupPool
        {

        }

        struct SceneLight
        {

        }

        struct SceneCamera
        {

        }

        class SceneData
        {
            public SceneCamera[] Cameras { get; set; }
            public SceneLight[] Lights { get; set; }
        }

        class MeshPrimitive
        {
            public int[] Vertices { get; set; }
            public int[] Indices { get; set; }
        }

        class InstanceStorage
        {
            public int Storage { get; set; }
            public ulong Offset { get; set; }
        }

        class InstanceDrawCall
        {
            public int InstanceCount { get; set; }
            public int[] PerInstance { get; set; }
            public int[] Cameras { get; set; }
            public int[] Lights { get; set; }
            public int[] Materials { get; set; }
            public int[] Textures { get; set; }
            public IMgDescriptorSet[] DescriptorSets { get; set; }
        }

        class InstanceGroup
        {
            public int MeshPrimitive { get; set; }
            public EffectVariant Variant { get; set; }
            public List<InstanceDrawCall> DrawCalls { get; set; }
        }

        class EffectPassGraph
        {
            public EffectPass Pass { get; set; }

        }

        [TestMethod]
        public void BuildGraph()
        {
            var pass0 = new EffectPass { Name = "depthPass" };
            var pass1 = new EffectPass { Name = "pbrPass" };
        }

        [TestMethod]
        public void AsUniformBufferUsage()
        {
            var expected = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT;
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxPerStageDescriptorStorageBuffers = 0,
                    MaxUniformBufferRange = 100,
                    MaxStorageBufferRange = 1000,
                }                
            );

            MgBufferUsageFlagBits actual = selector.GetAppropriateBufferUsage();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AsStorageBufferUsage()
        {
            var expected = MgBufferUsageFlagBits.STORAGE_BUFFER_BIT;
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxUniformBufferRange = 100,
                    MaxStorageBufferRange = 1000,
                }                
            );

            MgBufferUsageFlagBits actual = selector.GetAppropriateBufferUsage();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NewMaterials_0()
        {
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxUniformBufferRange = 100,
                    MaxStorageBufferRange = 400,                        
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxPerStageDescriptorSampledImages = 10,
                }
                
            );

            var actual = selector.GetElementRange(
                MgBufferUsageFlagBits.STORAGE_BUFFER_BIT,
                5U,                
                20U);
            // since it's storage is should be limited by 
                // number of texture slots / 5 => 2 << SMALLER
                // number of storage space / data size => 20U
            var expected = 2U;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NewMaterials_1()
        {
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxUniformBufferRange = 100,
                    MaxStorageBufferRange = 500,
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxPerStageDescriptorSampledImages = 100,
                }                
            );

            var actual = selector.GetElementRange(
                MgBufferUsageFlagBits.STORAGE_BUFFER_BIT,
                5U,
                20U);
            // since it's storage is should be limited by the 
                // number of texture slots / 5 => 20 << SMALLER
                // number of storage space / data size => 25U
            var expected = 20U;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NewMaterials_2()
        {
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxUniformBufferRange = 100,
                    MaxStorageBufferRange = 300,
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxPerStageDescriptorSampledImages = 100,
                }                
            );

            var actual = selector.GetElementRange(
                MgBufferUsageFlagBits.STORAGE_BUFFER_BIT,
                5U,
                20U);
            // since it's storage is should be limited by the 
            // number of texture slots / 5 => 20 
            // number of storage space / data size => 15U << SMALLER
            var expected = 15U;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NewMaterials_4()
        {
            var selector = new PerMaterialTextureStorageQuery(                  
                new MgPhysicalDeviceLimits
                {
                    MaxUniformBufferRange = 100,
                    MaxStorageBufferRange = 400,
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxPerStageDescriptorSampledImages = 10,
                }                
            );

            var actual = selector.GetElementRange(
                MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                5U,
                20U);
            // since it's storage is should be limited by the 
                // number of texture slots / 5 => 2U 
                // number of storage space / data size => 5U << SMALLER
            var expected = 2U;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NewMaterials_5()
        {
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxUniformBufferRange = 100,
                    MaxStorageBufferRange = 500,
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxPerStageDescriptorSampledImages = 100,
                }                
            );

            var actual = selector.GetElementRange(
                MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                5U,
                20U);
            // since it's storage is should be limited by the 
            // number of texture slots / 5 => 20  
            // number of storage space / data size => 5U << SMALLER
            var expected = 5U;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NewMaterials_6()
        {
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxUniformBufferRange = 200,
                    MaxStorageBufferRange = 300,
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxPerStageDescriptorSampledImages = 100,
                }                
            );

            var actual = selector.GetElementRange(
                MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                5U,
                20U);
            // since it's storage is should be limited by the 
            // number of texture slots / 5 => 20 
            // number of storage space / data size => 10U << SMALLER
            var expected = 10U;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NewMaterials_7()
        {
            var selector = new PerMaterialTextureStorageQuery(
                new MgPhysicalDeviceLimits
                {
                    MaxUniformBufferRange = 400,
                    MaxStorageBufferRange = 300,
                    MaxPerStageDescriptorStorageBuffers = 1,
                    MaxPerStageDescriptorSampledImages = 100,
                }                
            );

            var actual = selector.GetElementRange(
                MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                5U,
                20U);
            // since it's storage is should be limited by the 
            // number of texture slots / 5 => 20  << SAME
            // number of storage space / data size => 20U << SAME
            var expected = 20U;
            Assert.AreEqual(expected, actual);
        }
    }
}
