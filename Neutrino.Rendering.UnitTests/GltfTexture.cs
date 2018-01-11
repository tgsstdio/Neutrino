using glTFLoader.Schema;

namespace Neutrino.UnitTests
{
    public class GltfTexture
    {
        public GltfTexture(Texture texture)
        {
            Image = texture.Source;
            Sampler = texture.Sampler;
        }

        public int? Image { get; }
        public int? Sampler { get; }
    }    
}
