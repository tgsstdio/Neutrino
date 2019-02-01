using System.IO;
using Neutrino;

namespace TriangleDemo
{
    internal class DefaultPbrEffectPath : IPbrEffectPath
    {
        public Stream OpenFragmentShader()
        {
            return File.OpenRead("Data/pbrEffect.frag.spv");
        }

        public Stream OpenVertexShader()
        {
           return File.OpenRead("Data/pbrEffect.vert.spv");
        }
    }
}