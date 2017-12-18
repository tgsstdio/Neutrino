using System;
using Magnesium;

namespace GltfDemo
{
    internal class MockMemory : IMgDeviceMemory
    {
        public void FreeMemory(IMgDevice device, IMgAllocationCallbacks allocator)
        {
            throw new NotImplementedException();
        }

        public Result MapMemory(IMgDevice device, ulong offset, ulong size, uint flags, out IntPtr ppData)
        {
            ppData = IntPtr.Zero;
            return Result.SUCCESS;
        }

        public void UnmapMemory(IMgDevice device)
        {
            // LEFT BLANK
        }
    }
}