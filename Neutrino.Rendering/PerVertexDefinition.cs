namespace Neutrino
{
    // max 32 bits
    public struct PerVertexDefinition
    {
        // [0 - 2, 3]
        public PerVertexPositionType Position { get; set; }
        // [3 - 4, 2]
        public PerVertexNormalType Normal { get; set; }
        // [5 - 6, 2]
        public PerVertexTangentType Tangent { get; set; }
        // [7 - 11, 4]
        public PerVertexTexCoordsType TexCoords0 { get; set; }
        // [12 - 15, 4]
        public PerVertexTexCoordsType TexCoords1 { get; set; }
        // [16 - 18, 3]
        public PerVertexColorType Color0 { get; set; }
        // [19 - 21, 3]
        public PerVertexColorType Color1 { get; set; }
        // [22 - 23, 2]
        public PerVertexJointType Joints0 { get; set; }
        // [24 - 25, 2]
        public PerVertexJointType Joints1 { get; set; }
        // [26 - 27, 2]
        public PerVertexWeightsType Weights0 { get; set; }
        // [28 - 29, 2]
        public PerVertexWeightsType Weights1 { get; set; }
        // [30 - 31, 2]
        public PerVertexIndexType IndexType { get; set; }
    }
    
}
