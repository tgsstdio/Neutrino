using Magnesium;

namespace Neutrino.UnitTests
{
    internal class MockDescriptorSetLayout : IMgDescriptorSetLayout
    {
        public void DestroyDescriptorSetLayout(IMgDevice device, IMgAllocationCallbacks allocator)
        {
            throw new System.NotImplementedException();
        }
    }
}