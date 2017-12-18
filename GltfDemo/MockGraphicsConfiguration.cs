using System;
using Magnesium;

namespace GltfDemo
{
    internal class MockGraphicsConfiguration : IMgGraphicsConfiguration
    {
        private IMgDevice mDevice;
        private IMgThreadPartition mPartition;
        public MockGraphicsConfiguration(IMgThreadPartition partition)
        {
            mDevice = new MockDevice();
            mPartition = partition;
        }

        public IMgDevice Device
        {
            get
            {
                return mDevice;
            }
        }

        public IMgThreadPartition Partition
        {
            get
            {
                return mPartition;
            }
        }            


        public IMgQueue Queue => throw new NotImplementedException();

        public MgPhysicalDeviceMemoryProperties MemoryProperties => throw new NotImplementedException();

        public void Dispose()
        {
 
        }

        public void Initialize(uint width, uint height)
        {

        }
    }
}