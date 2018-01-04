using System.Collections.Generic;

namespace Neutrino
{
    public class GltfTextureBucket
    {
        public int Index { get; set; }
        public Dictionary<int, GltfCombinedImageSampler> Textures { get; set; }
    }
}
