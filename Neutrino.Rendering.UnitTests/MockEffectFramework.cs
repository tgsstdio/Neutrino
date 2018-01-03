using Magnesium;

namespace Neutrino.UnitTests
{
    internal class MockEffectFramework : IMgEffectFramework
    {
        public MgRect2D Scissor => throw new System.NotImplementedException();

        public MgViewport Viewport => throw new System.NotImplementedException();

        public MgRenderPassCreateInfo RenderpassInfo => throw new System.NotImplementedException();

        private IMgRenderPass mRenderPass = new MockRenderPass();
        public IMgRenderPass Renderpass
        {
            get
            {
                return mRenderPass;
            }
        }

        public IMgFramebuffer[] Framebuffers => throw new System.NotImplementedException();

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}