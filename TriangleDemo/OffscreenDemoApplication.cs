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
            // load model file

            // allocate partitions for static data
                // mesh
                // images

            // initialize static data storage 

            // build static artifacts
                // render target
                // descriptor set layout + pipeline layout                
                // pipeline          

            // allocate dynamic data
                // lights 
                // materials
                // cameras
                // per instance data

            // build dynamic artifacts
                // semaphores 
                // fences
                // descriptor sets

            // initialize dynamic data storage           

            // copy data across
            // buffers 
            // images

            // map dynamic data 

            // build command buffers
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