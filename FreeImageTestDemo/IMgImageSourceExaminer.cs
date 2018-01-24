using Magnesium;
using System.IO;

namespace FreeImageTestDemo
{
    public interface IMgImageSourceExaminer
    {
        MgImageSource DetermineSource(Stream fs);
    }
}
