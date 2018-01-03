using Magnesium;

namespace Neutrino.UnitTests
{
    internal class MockPipelineLayout : IMgPipelineLayout
    {
        public void DestroyPipelineLayout(IMgDevice device, IMgAllocationCallbacks allocator)
        {
            throw new System.NotImplementedException();
        }
    }
}