using Magnesium;

namespace Neutrino
{
    public class PerTextureStorageSettings
    {
        public uint ElementRange { get; set; }
        public MgBufferUsageFlagBits Usage { get; set; }
        public ulong MinimumAlignment { get; set; }
        public ulong AllocationSize { get; set; }
    }
}
