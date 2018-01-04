using Magnesium;

namespace TriangleDemo
{
    internal class TriangleDemoApplication : IDemoApplication
    {
        public MgGraphicsDeviceCreateInfo Initialize()
        {
            return new MgGraphicsDeviceCreateInfo
            {
                Swapchain = new MgDeviceFormatSetting
                {
                    Color = MgColorFormatOption.AUTO_DETECT,
                    DepthStencil = MgDepthFormatOption.AUTO_DETECT,
                },
                RenderPass = new MgDeviceFormatSetting
                {
                    Color = MgColorFormatOption.AUTO_DETECT,
                    DepthStencil = MgDepthFormatOption.AUTO_DETECT,
                },
                Width = 640,
                Height = 480,
                MinDepth = 0f,
                MaxDepth = 1f,
                Samples = MgSampleCountFlagBits.COUNT_1_BIT,
            };
        }

        public void Prepare(IMgGraphicsConfiguration configuration, IMgGraphicsDevice screen)
        {
        
        }

        public void ReleaseManagedResources(IMgGraphicsConfiguration configuration)
        {

        }

        public void ReleaseUnmanagedResources(IMgGraphicsConfiguration configuration)
        {
           
        }

        public IMgSemaphore[] Render(IMgQueue queue, uint layerNo, IMgSemaphore semaphore)
        {
            return new IMgSemaphore[] { };
        }

        public void Update(IMgGraphicsConfiguration configuration)
        {
           
        }
    }
}