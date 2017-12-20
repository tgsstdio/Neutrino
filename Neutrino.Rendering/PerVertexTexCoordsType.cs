namespace Neutrino
{
    /// <summary>
    ///  MAX 8 / 15 => 4 bits
    /// </summary>
    public enum PerVertexTexCoordsType : byte
    {
        None = 0,
        Half2,
        ByteUnorm2,
        UshortUnorm2,
        Float2,
        Half3,
        ByteUnorm3,
        UshortUnorm3,
        Float3,
    }    
}
