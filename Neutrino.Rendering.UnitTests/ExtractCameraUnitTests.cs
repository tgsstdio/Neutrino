using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neutrino.UnitTests
{
    [TestClass]
    public class ExtractCameraUnitTests
    {
        [TestMethod]
        public void NoCamerasDefined()
        {
            var request = new MgStorageBlockAllocationRequest();
            GltfSceneLoader.ExtractCameras(null, request);
            var actual = request.ToArray();
            Assert.AreEqual(1, actual.Length);
        }
    }
}
