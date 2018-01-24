using Magnesium;
using System;
using System.IO;

namespace FreeImageTestDemo
{
    public class FreeImageSourceExaminer : IMgImageSourceExaminer
    {
        public MgImageSource DetermineSource(Stream fs)
        {
            FreeImageAPI.FIBITMAP dib = FreeImageAPI.FIBITMAP.Zero;
            try
            {
                var format = FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN;
                dib = FreeImageAPI.FreeImage.LoadFromStream(fs, ref format);

                if (dib.IsNull)
                {
                    throw new InvalidOperationException("Image data invalid");
                }

                var imageType = FreeImageAPI.FreeImage.GetImageType(dib);
                var colorType = FreeImageAPI.FreeImage.GetColorType(dib);
                var redMask = FreeImageAPI.FreeImage.GetRedMask(dib);
                var greenMask = FreeImageAPI.FreeImage.GetGreenMask(dib);
                var blueMask = FreeImageAPI.FreeImage.GetRedMask(dib);
                var bpp = FreeImageAPI.FreeImage.GetBPP(dib);
                var width = FreeImageAPI.FreeImage.GetWidth(dib);
                var height = FreeImageAPI.FreeImage.GetHeight(dib);
                var imageSize = FreeImageAPI.FreeImage.GetDIBSize(dib);

                var uncompressedSize = bpp * width * height;

                var zeroMipmap = new MgImageMipmap
                {
                    Offset = 0,
                    Width = width,
                    Height = height,
                    Size = uncompressedSize,
                };

                var imgSource = new MgImageSource
                {
                    Format = DetermineFormat(imageType, colorType, bpp, redMask, greenMask, blueMask),
                    Width = width,
                    Height = height,
                    Mipmaps = new[] { zeroMipmap },
                    Size = uncompressedSize,
                };
                return imgSource;
            }
            finally
            {
                if (!dib.IsNull)
                {
                    FreeImageAPI.FreeImage.UnloadEx(ref dib);
                }
            }
        }

        private static MgFormat DetermineFormat(
            FreeImageAPI.FREE_IMAGE_TYPE imageType,
            FreeImageAPI.FREE_IMAGE_COLOR_TYPE colorType,
            uint bpp,
            uint redMask,
            uint greenMask,
            uint blueMask
        )
        {
            switch (imageType)
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
                    return GetBitmapFormat(bpp, redMask, greenMask, blueMask);
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGB16:
                case FreeImageAPI.FREE_IMAGE_TYPE.FIT_RGBA16:
                    break;
                default:
                    throw new NotSupportedException("image type not supported");
            }

            if (colorType == FreeImageAPI.FREE_IMAGE_COLOR_TYPE.FIC_RGB)
            {
                switch (bpp)
                {
                    case 24:
                        return MgFormat.R8G8B8_UINT;
                    case 48:
                        return MgFormat.R16G16B16_UINT;
                    default:
                        throw new NotSupportedException("color type not supported for RGB");
                }
            }
            else if (colorType == FreeImageAPI.FREE_IMAGE_COLOR_TYPE.FIC_RGBALPHA)
            {
                switch (bpp)
                {
                    case 32:
                        return MgFormat.R8G8B8A8_UINT;
                    case 64:
                        return MgFormat.R16G16B16A16_UINT;
                    default:
                        throw new NotSupportedException("color type not supported for RGB");
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }


        private static MgFormat GetBitmapFormat(
            uint bpp,
            uint redMask,
            uint greenMask,
            uint blueMask
        )
        {
            // from FreeImage source code
            switch (bpp)
            {
                case 16:
                    if ((blueMask == FreeImageAPI.FreeImage.FI16_565_BLUE_MASK) &&
                        (greenMask == FreeImageAPI.FreeImage.FI16_565_GREEN_MASK) &&
                        (redMask == FreeImageAPI.FreeImage.FI16_565_RED_MASK))
                    {
                        // result = PixelFormat.Format16bppRgb565;
                        return MgFormat.R5G6B5_UNORM_PACK16;
                    }
                    else if ((blueMask == FreeImageAPI.FreeImage.FI16_555_BLUE_MASK) &&
                        (greenMask == FreeImageAPI.FreeImage.FI16_555_GREEN_MASK) &&
                        (redMask == FreeImageAPI.FreeImage.FI16_555_RED_MASK))
                    {
                        // result = PixelFormat.Format16bppRgb555;
                        return MgFormat.R5G5B5A1_UNORM_PACK16;
                    }
                    else
                    {
                        throw new NotSupportedException("color type not supported for bitmap");
                    }
                case 24:
                    // result = PixelFormat.Format24bppRgb;
                    return MgFormat.R8G8B8_UINT;
                // break;
                case 32:
                    // result = PixelFormat.Format32bppArgb;
                    return MgFormat.A8B8G8R8_UINT_PACK32;
                default:
                    throw new NotSupportedException("color type not supported for bitmap");
            }
        }
    }
}
