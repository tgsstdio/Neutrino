using System.Collections.Generic;

namespace Neutrino
{
    public class GltfTextureContainer
    {
        public int BucketSize { get; set; }

        public List<GltfTextureBucket> Buckets { get; set; }

        public GltfTextureMarker[] GetAllocation(int[] textures)
        {
            var noOfChecks = textures != null ? textures.Length : 0;

            var result = new List<GltfTextureMarker>();
            foreach (var bucket in Buckets)
            {
                var matches = 0;
                result.Clear();

                // NO-CHANGE SCAN
                foreach (var query in textures)
                {
                    if (bucket.Textures.TryGetValue(query, out GltfCombinedImageSampler item))
                    {
                        result.Add(
                            new GltfTextureMarker
                            {
                                SetIndex = bucket.Index,
                                Offset = item.Offset,
                            });
                    }
                    else
                    {
                        // 
                        break;
                    }
                }

                if (matches == noOfChecks)
                {
                    result.ToArray();
                }
                else
                {
                    // TODO : something here
                }
            }

            return result.ToArray();
        }
    }
}
