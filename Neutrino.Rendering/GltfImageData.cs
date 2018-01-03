namespace Neutrino
{
    public class GltfImageData
    {
        public string Name { get; set; }
        public GltfImageMimeType MimeType { get; set; }
        public byte[] Source { get; set; }
        public ulong SrcOffset { get; set; }
        public ulong SrcLength { get; set; }
    }
}
