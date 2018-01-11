using System.Collections.Generic;

namespace Neutrino
{
    public class PerVertexDataLocator : IPerVertexDataLocator
    {
        public PerVertexDataLocator(int? indices, Dictionary<string, int> attributes)
        {
            this.Indices = indices;
            this.Position = SetVertexIndex("POSITION", attributes);
            this.Normal = SetVertexIndex("NORMAL", attributes);
            this.Tangent = SetVertexIndex("TANGENT", attributes);
            this.TexCoords0 = SetVertexIndex("TEXCOORDS_0", attributes);
            this.TexCoords1 = SetVertexIndex("TEXCOORDS_1", attributes);
            this.Color0 = SetVertexIndex("COLOR_0", attributes);
            this.Color1 = SetVertexIndex("COLOR_1", attributes);
            this.Joints0 = SetVertexIndex("JOINTS_0", attributes);
            this.Joints1 = SetVertexIndex("JOINTS_1", attributes);
            this.Weights0 = SetVertexIndex("WEIGHTS_0", attributes);
            this.Weights1 = SetVertexIndex("WEIGHTS_1", attributes);
        }

        public int? Indices { get; }
        public int? Position { get; }
        public int? Normal { get; }
        public int? Tangent { get; }
        public int? TexCoords0 { get; }
        public int? TexCoords1 { get; }
        public int? Color0 { get; }
        public int? Color1 { get; }
        public int? Joints0 { get; }
        public int? Joints1 { get; }
        public int? Weights0 { get; }
        public int? Weights1 { get; }

        private static int? SetVertexIndex(string name, Dictionary<string, int> attributes)
        {
            if (attributes.TryGetValue(name, out int value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}