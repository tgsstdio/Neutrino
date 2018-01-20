namespace Neutrino
{
    public class MgtfCamera
    {
        public MgtfCameraType ProjectionType { get; set; }
        public float? AspectRatio { get; set; }
        public double[] Values { get; set; }

        const int DENOMINATOR_0_0 = 0;
        const int ROWCOLUMN_1_1 = 1;
        const int ROWCOLUMN_2_2 = 2;
        const int ROWCOLUMN_2_3 = 3;
        const int ROWCOLUMN_3_2 = 4;
        const int ROWCOLUMN_3_3 = 5;

        public TkMatrix4 ToMatrix(double width, double height)
        {
            var dx = Values[DENOMINATOR_0_0];

            var d = (ProjectionType == MgtfCameraType.Perspective)
                ? (AspectRatio.HasValue)
                    ? AspectRatio.Value * dx
                    : ((width / height) * dx)
                : dx;

            float m00 = (float) (1 / d);
            float m11 = (float) Values[ROWCOLUMN_1_1];
            float m22 = (float) Values[ROWCOLUMN_2_2];
            float m23 = (float) Values[ROWCOLUMN_2_3];
            float m32 = (float) Values[ROWCOLUMN_3_2];
            float m33 = (float) Values[ROWCOLUMN_3_3];

            return new TkMatrix4
            (
                m00, 0, 0, 0,
                0, m11, 0, 0,
                0, 0, m22, m23,
                0, 0, m32, m33
            );
        }
    }    
}
