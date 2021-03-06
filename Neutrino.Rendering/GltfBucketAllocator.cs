﻿using Magnesium;
using Magnesium.Utilities;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Neutrino
{
    // ALWAYS RETURNS AT LEAST ONE ARRAY/BUCKET FOR ALLOCATION
    public class GltfBucketAllocationInfo<TStruct>
    {
        public int BucketSize { get; set; }
        public MgBufferUsageFlagBits Usage { get; set; }
        public MgMemoryPropertyFlagBits MemoryPropertyFlags { get; set; }
        public uint ElementByteSize { get; set; }

        public GltfBucketContainer Prepare(int length, MgStorageBlockAllocationRequest request)
        {
            var slots = new List<int>();
            if (length > 0)
            {
                var noOfBuckets = length / BucketSize;
                var remainder = length % BucketSize;

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
                Count = length,
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
