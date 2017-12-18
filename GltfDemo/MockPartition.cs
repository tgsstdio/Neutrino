using System;
using Magnesium;

namespace GltfDemo
{
    internal class MockPartition : IMgThreadPartition
    {
        public IMgCommandPool CommandPool => throw new NotImplementedException();

        public IMgCommandBuffer[] CommandBuffers => throw new NotImplementedException();

        public IMgQueue Queue => throw new NotImplementedException();

        public IMgDevice Device => throw new NotImplementedException();

        public IMgPhysicalDevice PhysicalDevice => throw new NotImplementedException();

        public void Dispose()
        {

        }

        public uint ReturnValue { get; set; }
        public bool IsValid { get; set; }
        public bool GetMemoryType(uint typeBits, MgMemoryPropertyFlagBits memoryPropertyFlags, out uint typeIndex)
        {
            typeIndex = ReturnValue;
            return IsValid;
        }
    }
}