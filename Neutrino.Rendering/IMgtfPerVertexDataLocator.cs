namespace Neutrino
{
    public interface IMgtfPerVertexDataLocator
    {
        int? Indices { get; }
        int? Position { get; }
        int? Normal { get; }
        int? Tangent { get; }
        int? TexCoords0 { get; }
        int? TexCoords1 { get; }
        int? Color0 { get; }
        int? Color1 { get; }
        int? Joints0 { get; }
        int? Joints1 { get; }
        int? Weights0 { get; }
        int? Weights1 { get; }
    }
}