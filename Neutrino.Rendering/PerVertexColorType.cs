namespace Neutrino
{
    /// <summary>
    ///  MAX 7 / 7 => 3 bits
    /// </summary>
    public enum PerVertexColorType : byte
    {
        None = 0,
        ByteUnormRGB,
        ByteUnormRGBA,
        UshortUnormRGB,
        UshortUnormRGBA,
        FloatRGB,
        FloatRGBA,
    }    
}
