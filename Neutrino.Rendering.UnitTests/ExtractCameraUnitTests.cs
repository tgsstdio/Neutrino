﻿using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neutrino.UnitTests
{
    [TestClass]
    public class ExtractCameraUnitTests
    {
        [StructLayout(LayoutKind.Sequential)]
        struct CameraUnit
        {
            public float Number { get; set; }
        }

        [TestMethod]
        public void NoCamerasDefined()
        {
            var request = new MgStorageBlockAllocationRequest();
            var cameras = new GltfBucketAllocationInfo<CameraUnit>
            {
                BucketSize = 16,
                Usage = Magnesium.MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = Magnesium.MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                ElementByteSize = 0,
            };

            var result = cameras.Prepare(0, request);
            Assert.AreEqual(0, result.Count);
            Assert.IsNotNull(result.Slots);
            Assert.AreEqual(1, result.Slots.Length);
            var actual = request.ToArray();
            Assert.AreEqual(1, actual.Length);
        }

        [TestMethod]
        public void EmptyArray()
        {
            var request = new MgStorageBlockAllocationRequest();
            var cameras = new GltfBucketAllocationInfo<CameraUnit>
            {
                BucketSize = 16,
                Usage = Magnesium.MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = Magnesium.MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                ElementByteSize = 0,                
            };

            var result = cameras.Prepare(0, request);
            Assert.AreEqual(0, result.Count);
            Assert.IsNotNull(result.Slots);
            Assert.AreEqual(1, result.Slots.Length);
            var actual = request.ToArray();
            Assert.AreEqual(1, actual.Length);
        }

        [TestMethod]
        public void OneBucket_0()
        {
            var request = new MgStorageBlockAllocationRequest();
            var cameras = new GltfBucketAllocationInfo<CameraUnit>
            {
                BucketSize = 3,
                Usage = Magnesium.MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = Magnesium.MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                ElementByteSize = 0,
            };

            var result = cameras.Prepare(2, request);
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(result.Slots);
            Assert.AreEqual(1, result.Slots.Length);
            var actual = request.ToArray();
            Assert.AreEqual(1, actual.Length);
        }

        [TestMethod]
        public void OneBucket_1()
        {
            var request = new MgStorageBlockAllocationRequest();
            var cameras = new GltfBucketAllocationInfo<CameraUnit>
            {
                BucketSize =3,
                Usage = Magnesium.MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = Magnesium.MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                ElementByteSize = 0,                
            };

            var result = cameras.Prepare(3, request);
            Assert.AreEqual(3, result.Count);
            Assert.IsNotNull(result.Slots);
            Assert.AreEqual(1, result.Slots.Length);
            var actual = request.ToArray();
            Assert.AreEqual(1, actual.Length);
        }

        [TestMethod]
        public void TwoBuckets_0()
        {
            var request = new MgStorageBlockAllocationRequest();
            var cameras = new GltfBucketAllocationInfo<CameraUnit> {
                BucketSize = 3,
                Usage = Magnesium.MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = Magnesium.MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                ElementByteSize = 0,            
            };

            var result = cameras.Prepare(4, request);
            Assert.AreEqual(4, result.Count);
            Assert.IsNotNull(result.Slots);
            Assert.AreEqual(2, result.Slots.Length);
            var actual = request.ToArray();
            Assert.AreEqual(2, actual.Length);
        }

        [TestMethod]
        public void TwoBuckets_1()
        {
            var request = new MgStorageBlockAllocationRequest();
            var cameras = new GltfBucketAllocationInfo<CameraUnit>
            {
                BucketSize = 2,
                Usage = Magnesium.MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = Magnesium.MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                ElementByteSize = 0,
            };

            var result = cameras.Prepare(4, request);
            Assert.AreEqual(4, result.Count);
            Assert.IsNotNull(result.Slots);
            Assert.AreEqual(2, result.Slots.Length);
            var actual = request.ToArray();
            Assert.AreEqual(2, actual.Length);
        }
    }
}
