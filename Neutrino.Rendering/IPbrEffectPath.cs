using System.IO;

namespace Neutrino
{
    public interface IPbrEffectPath
    {
        Stream OpenVertexShader();
        Stream OpenFragmentShader();
    }
}
