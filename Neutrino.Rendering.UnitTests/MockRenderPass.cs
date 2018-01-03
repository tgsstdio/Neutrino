using Magnesium;

namespace Neutrino.UnitTests
{
    internal class MockRenderPass : IMgRenderPass
    {
        public void DestroyRenderPass(IMgDevice device, IMgAllocationCallbacks allocator)
        {
            throw new System.NotImplementedException();
        }
    }
}