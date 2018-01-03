using Magnesium;

namespace Neutrino.UnitTests
{
    internal class MockGraphicsPipeline : IMgPipeline
    {
        public void DestroyPipeline(IMgDevice device, IMgAllocationCallbacks allocator)
        {
            throw new System.NotImplementedException();
        }
    }
}