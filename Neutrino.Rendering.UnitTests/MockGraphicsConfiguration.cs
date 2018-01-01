using Magnesium;

namespace Neutrino.UnitTests
{
    internal class MockGraphicsConfiguration : IMgGraphicsConfiguration
    {
        public IMgDevice Device => throw new System.NotImplementedException();

        public IMgThreadPartition Partition => throw new System.NotImplementedException();

        public IMgQueue Queue => throw new System.NotImplementedException();

        public MgPhysicalDeviceMemoryProperties MemoryProperties => throw new System.NotImplementedException();

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(uint width, uint height)
        {
            throw new System.NotImplementedException();
        }
    }
}