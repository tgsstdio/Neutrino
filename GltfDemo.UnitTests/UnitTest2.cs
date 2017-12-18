using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Magnesium.Utilities;

namespace GltfDemo.UnitTests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void RoundUp_4Bytes_3_4()
        {
            const ulong ELEMENT_SIZE = 4UL;
            var actual = MgOptimizedStoragePartitionVerifier.UpperBounded(3, ELEMENT_SIZE);
            Assert.AreEqual(4UL, actual);
        }

        [TestMethod]
        public void RoundUp_4Bytes_4_4()
        {
            const ulong ELEMENT_SIZE = 4UL;
            var actual = MgOptimizedStoragePartitionVerifier.UpperBounded(4, ELEMENT_SIZE);
            Assert.AreEqual(4UL, actual);
        }

        [TestMethod]
        public void RoundUp_4Bytes_63_64()
        {
            const ulong ELEMENT_SIZE = 4UL;
            var actual = MgOptimizedStoragePartitionVerifier.UpperBounded(63, ELEMENT_SIZE);
            Assert.AreEqual(64UL, actual);
        }

        [TestMethod]
        public void RoundUp_2Bytes_5_6()
        {
            const ulong ELEMENT_SIZE = 2UL;
            var actual = MgOptimizedStoragePartitionVerifier.UpperBounded(5, ELEMENT_SIZE);
            Assert.AreEqual(6UL, actual);
        }

        [TestMethod]
        public void RoundUp_2Bytes_2_2()
        {
            const ulong ELEMENT_SIZE = 2UL;
            var actual = MgOptimizedStoragePartitionVerifier.UpperBounded(2UL, ELEMENT_SIZE);
            Assert.AreEqual(2UL, actual);
        }
    }
}
