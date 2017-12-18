using System;
using Magnesium;

namespace GltfDemo
{
    internal class MockBuffer : IMgBuffer
    {
        public ulong BufferSize { get; set; }
        public MockBuffer(MgBufferCreateInfo createInfo)
        {
            BufferSize = createInfo.Size;
        }

        public Result BindBufferMemory(IMgDevice device, IMgDeviceMemory memory, ulong memoryOffset)
        {
            return Result.SUCCESS;
        }

        public void DestroyBuffer(IMgDevice device, IMgAllocationCallbacks allocator)
        {
            
        }
    }
}