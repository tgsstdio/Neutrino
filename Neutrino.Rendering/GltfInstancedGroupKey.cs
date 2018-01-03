using System;

namespace Neutrino
{
    public struct GltfInstancedGroupKey : IComparable<GltfInstancedGroupKey>
    {
        public int MeshIndex { get; set; }
        public int CameraSlotIndex { get; internal set; }
        public int TextureSlotIndex { get; internal set; }
        public int MaterialSlotIndex { get; set; }
        public EffectVariantKey VariantKey { get; set; }

        public int CompareTo(GltfInstancedGroupKey other)
        {
            if (MeshIndex < other.MeshIndex)
                return -1;

            if (MeshIndex > other.MeshIndex)
                return 1;

            if (CameraSlotIndex < other.CameraSlotIndex)
                return -1;

            if (CameraSlotIndex > other.CameraSlotIndex)
                return 1;

            if (TextureSlotIndex < other.TextureSlotIndex)
                return -1;

            if (TextureSlotIndex > other.TextureSlotIndex)
                return 1;

            if (MaterialSlotIndex < other.MaterialSlotIndex)
                return -1;

            if (MaterialSlotIndex > other.MaterialSlotIndex)
                return 1;

            return VariantKey.CompareTo(other.VariantKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 13;
                hash = (7 * hash) + MeshIndex.GetHashCode();
                hash = (7 * hash) + CameraSlotIndex.GetHashCode();
                hash = (7 * hash) + TextureSlotIndex.GetHashCode();
                hash = (7 * hash) + MaterialSlotIndex.GetHashCode();
                hash = (7 * hash) + VariantKey.GetHashCode();
                return hash;
            }
        }
    }
}
