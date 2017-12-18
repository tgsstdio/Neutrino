using Magnesium.Utilities;
using System.Collections.Generic;

namespace Neutrino
{
    public class MgStorageBlockAllocationRequest
    {
        private List<MgStorageBlockAllocationInfo> mItems;

        public MgStorageBlockAllocationRequest()
        {
            mItems = new List<MgStorageBlockAllocationInfo>();
        }

        public int Insert(MgStorageBlockAllocationInfo info)
        {
            int count = mItems.Count;
            mItems.Add(info);
            return count;
        }

        public void Clear()
        {
            mItems.Clear();
        }

        public MgStorageBlockAllocationInfo[] ToArray()
        {
            return mItems.ToArray();
        }
    }
}