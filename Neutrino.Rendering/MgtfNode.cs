namespace Neutrino
{
    public class MgtfNode
    {
        public int? Parent { get; set; }
        public int? Camera { get; set; }
        public int[] Children { get; set; }
        public TkMatrix4 Transform { get; set; }
        public string Name { get; set; }
        public int? Mesh { get; set; }
    }
}
