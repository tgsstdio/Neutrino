using Magnesium;

namespace Neutrino
{
    public interface IGltfSceneLoader
    {
        void Load(IMgDevice device, IMgEffectFramework framework, string modelFilePath);
    }
}