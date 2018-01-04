using Magnesium;

namespace Neutrino
{
    public interface IGltfSceneLoader
    {
        GltfScene Load(IMgDevice device, IMgEffectFramework framework, string modelFilePath);
    }
}