namespace Neutrino
{
    public class MgtfImage
    {
        public string Name { get; set; }
        public MgtfImageMimeType MimeType { get; set; }
        public byte[] Source { get; set; }
        public ulong SrcOffset { get; set; }
        public ulong SrcLength { get; set; }
        public int? Buffer { get; set; }
    }
}
