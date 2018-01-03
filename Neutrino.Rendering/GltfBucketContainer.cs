namespace Neutrino
{
    public class GltfBucketContainer
    {
        public int Count { get; set; }
        public int BucketSize { get; set; }
        public int[] Slots { get; set; }

        public GltfBucketMarker GetAllocation(int? source)
        {
            return new GltfBucketMarker
            {
                BucketIndex = source.HasValue
                    ? (source.Value + 1) / BucketSize
                    : 0,
                StorageIndex = source.HasValue
                    ? Slots[(source.Value + 1) / BucketSize]
                    : Slots[0],
                Offset = source.HasValue
                    ? (source.Value + 1) % BucketSize
                    : 0,
            };
        }
    }
    
}
