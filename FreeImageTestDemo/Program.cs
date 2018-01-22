using Magnesium;
using System;
using System.IO;

namespace FreeImageTestDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var fs = File.Open("Profile.jpg", FileMode.Open))
            {
                FreeImageAPI.FREE_IMAGE_FORMAT format = FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN;

                var dib = FreeImageAPI.FreeImage.LoadFromStream(fs, ref format);

                if (dib.IsNull)
                {
                    throw new InvalidOperationException("Image data invalid");
                }

                var imageType = FreeImageAPI.FreeImage.GetImageType(dib);
                var colorType = FreeImageAPI.FreeImage.GetColorType(dib);
                var red = FreeImageAPI.FreeImage.GetRedMask(dib);
                var green = FreeImageAPI.FreeImage.GetGreenMask(dib);
                var blue = FreeImageAPI.FreeImage.GetRedMask(dib);
                var bpp = FreeImageAPI.FreeImage.GetBPP(dib);
                var width = FreeImageAPI.FreeImage.GetWidth(dib);
                var height = FreeImageAPI.FreeImage.GetHeight(dib);
                var imageSize = FreeImageAPI.FreeImage.GetDIBSize(dib);

                var totalSize = bpp * width * height;

                Console.WriteLine("{0} {1} {2}", bpp, width, height);

                var zeroMipmap = new MgImageMipmap
                {
                    Offset = 0,
                    Width = width,
                    Height = height,
                    Size = totalSize,
                };

                var imgSource = new MgImageSource
                {
                    Format = DetermineFormat(imageType, colorType, bpp),
                    Width = width,
                    Height = height,    
                    Mipmaps = new[] {zeroMipmap},                    
                };

                FreeImageAPI.FreeImage.UnloadEx(ref dib);
            }
        }

        public static MgFormat DetermineFormat(
            FreeImageAPI.FREE_IMAGE_TYPE imageType,
            FreeImageAPI.FREE_IMAGE_COLOR_TYPE colorType,
            uint bpp
        )
        {
            switch(imageType)
            {
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_INT16:
                    return MgFormat.R16_SINT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_INT32:
                    return MgFormat.R32_SINT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_UINT16:
                    return MgFormat.R16_UINT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_UINT32:
                    return MgFormat.R32_UINT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_FLOAT:
                    return MgFormat.R32_SFLOAT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_DOUBLE:
                    return MgFormat.R64_SFLOAT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGBF:
                    return MgFormat.R32G32B32_SFLOAT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGBAF:
                    return MgFormat.R32G32B32A32_SFLOAT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_COMPLEX:
                    return MgFormat.R64G64_SFLOAT;
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_BITMAP:
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGB16:
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGBA16:
                    break;
                default:
                    throw new NotSupportedException("image type not supported");
            }

            if (colorType == FreeImageAPI.FREE_IMAGE_COLOR_TYPE.FIC_RGB)
            {
                switch(bpp)
                {
                    case 24:
                        return MgFormat.R8G8B8_SINT;
                    case 48:
                        return MgFormat.R16G16B16_SINT;
                    
                }

                return MgFormat.UNDEFINED;
            }
            else if (colorType == FreeImageAPI.FREE_IMAGE_COLOR_TYPE.FIC_RGBALPHA)
            {
                return MgFormat.UNDEFINED;
            }
            else
            {
                throw new NotSupportedException();
            }

        }
    }
}
