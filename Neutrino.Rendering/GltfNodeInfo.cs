using glTFLoader.Schema;

namespace Neutrino
{
    public class GltfNodeInfo
    {
        public GltfNodeInfo(Node src)
        {
            Name = src.Name;
            Camera = src.Camera;
            Children = src.Children ?? (new int[] { });
            Transform = GenerateTransform(src);
            Mesh = src.Mesh;
        }

        public int? Parent { get; set; }
        public int? Camera { get; set; }
        public int[] Children { get; set; }
        public TkMatrix4 Transform { get; set; }
        public string Name { get; set; }
        public int? Mesh { get; set; }

        public static TkMatrix4 GenerateTransform(Node srcNode)
        {
            if (srcNode.Matrix != null && srcNode.Matrix.Length == 16)
            {
                var src = srcNode.Matrix;
                return new TkMatrix4(
                    src[0], src[1], src[2], src[3],
                    src[4], src[5], src[6], src[7],
                    src[8], src[9], src[10], src[11],
                    src[12], src[13], src[14], src[15]
                );
            }
            else
            {
                var firstOp =
                    (srcNode.Scale != null && srcNode.Scale.Length == 3)
                    ? TkMatrix4.CreateScale(srcNode.Scale[0], srcNode.Scale[1], srcNode.Scale[2])
                    : TkMatrix4.CreateScale(1, 1, 1);

                var quat = new TkQuaternion(0f, 0f, 0f, 1f);

                if (srcNode.Rotation != null && srcNode.Rotation.Length == 4)
                {
                    quat = new TkQuaternion(
                        srcNode.Rotation[0],
                        srcNode.Rotation[1],
                        srcNode.Rotation[2],
                        srcNode.Rotation[3]);
                }

                var secondOp = TkMatrix4.CreateFromQuaternion(quat);

                var thirdOp =
                    (srcNode.Translation != null && srcNode.Translation.Length == 3)
                    ? TkMatrix4.CreateTranslation(srcNode.Translation[0], srcNode.Translation[1], srcNode.Translation[2])
                    : TkMatrix4.CreateTranslation(0, 0, 0);

                // T * (R * S)
                TkMatrix4.Mult(ref secondOp, ref firstOp, out TkMatrix4 result);
                return TkMatrix4.Mult(thirdOp, result);
            }
        }
    }
}
