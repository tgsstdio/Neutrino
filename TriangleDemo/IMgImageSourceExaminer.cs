using Magnesium;
using System.IO;

namespace TriangleDemo
{
    public interface IMgImageSourceExaminer
    {
        MgImageSource DetermineSource(Stream fs);
    }
}
