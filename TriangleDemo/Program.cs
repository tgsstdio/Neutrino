using DryIoc;
using Magnesium.Utilities;
using OpenTK;
using System;

namespace TriangleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var container = new Container())
                using (var window = new NativeWindow())
                {
                    window.Title = "Offscreen Demo ";
                    window.Visible = true;

                    container.RegisterInstance<INativeWindow>(window);

                    container.Register<Magnesium.MgDriverContext>(Reuse.Singleton);
                    container.Register<Magnesium.IMgPresentationSurface,
                        Magnesium.PresentationSurfaces.OpenTK.VulkanPresentationSurface>(Reuse.Singleton);

                    container.Register<Magnesium.IMgGraphicsConfiguration,
                        Magnesium.MgDefaultGraphicsConfiguration>(Reuse.Singleton);
                    container.Register<Magnesium.IMgImageTools, Magnesium.MgImageTools>(Reuse.Singleton);

                    // Magnesium.VUlkan
                    container.Register<Magnesium.IMgEntrypoint,
                        Magnesium.Vulkan.VkEntrypoint>(Reuse.Singleton);

                    // SCOPE
                    container.Register<Magnesium.IMgPresentationBarrierEntrypoint,
                        Magnesium.MgPresentationBarrierEntrypoint>(Reuse.Singleton);

                    using (var scope = container.OpenScope())
                    {
                        // GAME START
                        scope.Register<Example>(Reuse.InCurrentScope);

                        scope.Register<IDemoApplication, OffscreenDemoApplication>(Reuse.InCurrentScope);
                        scope.Register<IMgImageSourceExaminer, FreeImageSourceExaminer>(Reuse.InCurrentScope);

                        //container.Register<IDemoApplication, TriangleDemoApplication>(new PerScopeLifetime());

                        scope.Register<IMgPlatformMemoryLayout, VkPlatformMemoryLayout>(Reuse.InCurrentScope);
                        scope.Register<IMgOptimizedStoragePartitioner,
                            MgOptimizedStoragePartitioner>(Reuse.InCurrentScope);
                        scope.Register<IMgOptimizedStoragePartitionVerifier,
                            MgOptimizedStoragePartitionVerifier>(Reuse.InCurrentScope);
                        scope.Register<MgOptimizedStorageBuilder>(Reuse.InCurrentScope);

                        // GAME END

                        scope.Register<Magnesium.IMgGraphicsDevice, Magnesium.MgDefaultGraphicsDevice>(Reuse.InCurrentScope);
                        scope.Register<Magnesium.IMgGraphicsDeviceContext, Magnesium.MgDefaultGraphicsDeviceContext>(Reuse.InCurrentScope);
                        scope.Register<Magnesium.IMgPresentationLayer, Magnesium.MgPresentationLayer>(Reuse.InCurrentScope);
                        scope.Register<Magnesium.IMgSwapchainCollection, Magnesium.MgSwapchainCollection>(Reuse.InCurrentScope);
                        scope.Register<MgGraphicsConfigurationManager>(Reuse.InCurrentScope);


                        using (var driver = scope.Resolve<Magnesium.MgDriverContext>())
                        {
                            driver.Initialize(
                                new Magnesium.MgApplicationInfo
                                {
                                    ApplicationName = "OffscreenDemo",
                                    ApiVersion = Magnesium.MgApplicationInfo.GenerateApiVersion(1, 0, 30),
                                    ApplicationVersion = 1,
                                    EngineName = "Magnesium",
                                    EngineVersion = 1,
                                },
                                Magnesium.MgInstanceExtensionOptions.ALL);

                            using (var graphicsConfiguration = scope.Resolve<Magnesium.IMgGraphicsConfiguration>())
                            using (var secondLevel = scope.OpenScope())
                            using (var gameWindow = new GameWindow(window))
                            using (var example = secondLevel.Resolve<Example>())
                            {
                                try
                                {
                                    example.Initialize();
                                    gameWindow.RenderFrame += (sender, e) =>
                                    {
                                        example.Render();
                                    };

                                    gameWindow.Run(60, 60);
                                }
                                finally
                                {
                                    example.Dispose();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
