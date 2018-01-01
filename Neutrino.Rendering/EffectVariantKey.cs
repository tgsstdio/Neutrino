using System;

namespace Neutrino
{
    public struct EffectVariantKey : IComparable<EffectVariantKey>
    {
        public uint Definition { get; set; }
        public uint Options { get; set; }

        public int CompareTo(EffectVariantKey other)
        {
            if (Definition < other.Definition)
                return -1;

            if (Definition > other.Definition)
                return 1;

            if (Options < other.Options)
                return -1;

            if (Options > other.Options)
                return 1;

            return 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 13;
                hash = (7 * hash) + Definition.GetHashCode();
                hash = (7 * hash) + Options.GetHashCode();
                return hash;
            }
        }
    }
}
