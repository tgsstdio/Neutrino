using Magnesium;
using Magnesium.Utilities;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Neutrino
{
    public class GltfBucketContainer
    {
        public int Count { get; set; }
        public int BucketSize { get; set; }
        public int[] Slots { get; set; }

        public int GetAllocation(int? source)
        {
            return source.HasValue
                ? Slots[source.Value + 1 / BucketSize]
                : Slots[0];
        }
    }

    // ALWAYS RETURNS AT LEAST ONE ARRAY/BUCKET FOR ALLOCATION
    public class GltfBucketAllocationInfo<TGltfSrc, TStruct>
    {
        public int BucketSize { get; set; }
        public MgBufferUsageFlagBits Usage { get; set; }
        public MgMemoryPropertyFlagBits MemoryPropertyFlags { get; set; }
        public uint ElementByteSize { get; set; }

        public GltfBucketContainer Extract(TGltfSrc[] cameras, MgStorageBlockAllocationRequest request)
        {
            var slots = new List<int>();
            if (cameras != null)
            {
                var noOfBuckets = cameras.Length / BucketSize;
                var remainder = cameras.Length % BucketSize;

                for (var i = 0; i < noOfBuckets; i += 1)
                {
                    RequestNewSlot(request, slots);
                }

                if (remainder > 0)
                {
                    RequestNewSlot(request, slots);
                }
            }

            // ALWAYS PRODUCED BUCKET FOR DEFAULT
            if (slots.Count <= 0)
            {
                RequestNewSlot(request, slots);
            }

            return new GltfBucketContainer
            {
                Count = cameras != null ? cameras.Length : 0,
                BucketSize = BucketSize,
                Slots = slots.ToArray(),
            };
        }

        private void RequestNewSlot(MgStorageBlockAllocationRequest request, List<int> slots)
        {

            var STRIDE = Marshal.SizeOf(typeof(TStruct));
            var totalSize = (ulong)(STRIDE * BucketSize);

            var index = request.Insert(
                new MgStorageBlockAllocationInfo
                {
                    ElementByteSize = this.ElementByteSize,
                    MemoryPropertyFlags = this.MemoryPropertyFlags,
                    Usage = this.Usage,
                    Size = totalSize,
                }                
            );
            slots.Add(index);
        }
    } 
    
}
