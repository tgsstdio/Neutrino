using Magnesium;
using System;
using System.IO;

namespace TriangleDemo
{
    public class BitmapImageSourceExaminer : IMgImageSourceExaminer
    {
        public MgImageSource DetermineSource(Stream fs)
        {
            using (var item = new System.Drawing.Bitmap(fs))
            {
                var width = (uint)item.Width;
                var height = (uint)item.Height;
                var pixelFormat = item.PixelFormat;
                var bpp = GetBpp(pixelFormat);

                var uncompressedSize = bpp * width * height;

                var zeroMipmap = new MgImageMipmap
                {
                    Offset = 0,
                    Width = width,
                    Height = height,
                    Size = uncompressedSize,
                };

                return new MgImageSource
                {
                    Format = DetermineFormat(pixelFormat),
                    Width = width,
                    Height = height,
                    Mipmaps = new[] { zeroMipmap },
                    Size = uncompressedSize,
                };
            }
        }

        private uint GetBpp(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                //
                // Summary:
                //     Specifies that the format is 16 bits per pixel; 5 bits each are used for the
                //     red, green, and blue components. The remaining bit is not used.
                //  Format16bppRgb555 = 135173,
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                //
                // Summary:
                //     Specifies that the format is 16 bits per pixel; 5 bits are used for the red component,
                //     6 bits are used for the green component, and 5 bits are used for the blue component.
                //  Format16bppRgb565 = 135174,
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                //
                // Summary:
                //     The pixel format is 16 bits per pixel. The color information specifies 32,768
                //     shades of color, of which 5 bits are red, 5 bits are green, 5 bits are blue,
                //     and 1 bit is alpha.
                //  Format16bppArgb1555 = 397319,
                case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                //
                // Summary:
                //     The pixel format is 16 bits per pixel. The color information specifies 65536
                //     shades of gray.
                //  Format16bppGrayScale = 1052676,
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return 16U;

                //
                // Summary:
                //     Specifies that the format is 24 bits per pixel; 8 bits each are used for the
                //     red, green, and blue components.
                //  Format24bppRgb = 137224,
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return 24U;

                //
                // Summary:
                //     Specifies that the format is 32 bits per pixel; 8 bits each are used for the
                //     red, green, and blue components. The remaining 8 bits are not used.
                //  Format32bppRgb = 139273,
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                //
                // Summary:
                //     Specifies that the format is 32 bits per pixel; 8 bits each are used for the
                //     alpha, red, green, and blue components. The red, green, and blue components are
                //     premultiplied, according to the alpha component.
                //  Format32bppPArgb = 925707,
                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                //
                // Summary:
                //     Specifies that the format is 32 bits per pixel; 8 bits each are used for the
                //     alpha, red, green, and blue components.
                //  Format32bppArgb = 2498570,
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                //
                // Summary:
                //     The default pixel format of 32 bits per pixel. The format specifies 24-bit color
                //     depth and an 8-bit alpha channel.
                //  Canonical = 2097152,
                case System.Drawing.Imaging.PixelFormat.Canonical:
                    return 32U;

                //
                // Summary:
                //     Specifies that the format is 48 bits per pixel; 16 bits each are used for the
                //     red, green, and blue components.
                //  Format48bppRgb = 1060876,
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return 48U;

                //
                // Summary:
                //     Specifies that the format is 64 bits per pixel; 16 bits each are used for the
                //     alpha, red, green, and blue components. The red, green, and blue components are
                //     premultiplied according to the alpha component.
                //  Format64bppPArgb = 1851406,
                case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:

                //
                // Summary:
                //     Specifies that the format is 64 bits per pixel; 16 bits each are used for the
                //     alpha, red, green, and blue components.
                //  Format64bppArgb = 3424269
                case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                    return 64U;

                default:
                    throw new NotSupportedException("image type not supported");
            }
        }

        private static MgFormat DetermineFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                //
                // Summary:
                //     Specifies that the format is 16 bits per pixel; 5 bits each are used for the
                //     red, green, and blue components. The remaining bit is not used.
                //  Format16bppRgb555 = 135173,
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    return MgFormat.R5G5B5A1_UNORM_PACK16;
                //
                // Summary:
                //     Specifies that the format is 16 bits per pixel; 5 bits are used for the red component,
                //     6 bits are used for the green component, and 5 bits are used for the blue component.
                //  Format16bppRgb565 = 135174,
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    return MgFormat.R5G6B5_UNORM_PACK16;
                //
                // Summary:
                //     The pixel format is 16 bits per pixel. The color information specifies 32,768
                //     shades of color, of which 5 bits are red, 5 bits are green, 5 bits are blue,
                //     and 1 bit is alpha.
                //  Format16bppArgb1555 = 397319,
                case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    return MgFormat.A1R5G5B5_UNORM_PACK16;
                //
                // Summary:
                //     The pixel format is 16 bits per pixel. The color information specifies 65536
                //     shades of gray.
                //  Format16bppGrayScale = 1052676,
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return MgFormat.R16_UINT;

                //
                // Summary:
                //     Specifies that the format is 24 bits per pixel; 8 bits each are used for the
                //     red, green, and blue components.
                //  Format24bppRgb = 137224,
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return MgFormat.R8G8B8_UINT;

                //
                // Summary:
                //     Specifies that the format is 32 bits per pixel; 8 bits each are used for the
                //     red, green, and blue components. The remaining 8 bits are not used.
                //  Format32bppRgb = 139273,
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return MgFormat.R8G8B8A8_UINT;
                //
                // Summary:
                //     Specifies that the format is 32 bits per pixel; 8 bits each are used for the
                //     alpha, red, green, and blue components. The red, green, and blue components are
                //     premultiplied, according to the alpha component.
                //  Format32bppPArgb = 925707,
                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    return MgFormat.R8G8B8A8_UINT;
                //
                // Summary:
                //     Specifies that the format is 32 bits per pixel; 8 bits each are used for the
                //     alpha, red, green, and blue components.
                //  Format32bppArgb = 2498570,
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return MgFormat.R8G8B8A8_UINT;
                //
                // Summary:
                //     The default pixel format of 32 bits per pixel. The format specifies 24-bit color
                //     depth and an 8-bit alpha channel.
                //  Canonical = 2097152,
                case System.Drawing.Imaging.PixelFormat.Canonical:
                    return MgFormat.D24_UNORM_S8_UINT;

                //
                // Summary:
                //     Specifies that the format is 48 bits per pixel; 16 bits each are used for the
                //     red, green, and blue components.
                //  Format48bppRgb = 1060876,
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return MgFormat.R16G16B16_UINT;

                //
                // Summary:
                //     Specifies that the format is 64 bits per pixel; 16 bits each are used for the
                //     alpha, red, green, and blue components. The red, green, and blue components are
                //     premultiplied according to the alpha component.
                //  Format64bppPArgb = 1851406,
                case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
                    return MgFormat.R16G16B16A16_UINT;

                //
                // Summary:
                //     Specifies that the format is 64 bits per pixel; 16 bits each are used for the
                //     alpha, red, green, and blue components.
                //  Format64bppArgb = 3424269
                case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                    return MgFormat.R16G16B16A16_UINT;

                default:
                    throw new NotSupportedException("image type not supported");
            }
        }
    }
}
