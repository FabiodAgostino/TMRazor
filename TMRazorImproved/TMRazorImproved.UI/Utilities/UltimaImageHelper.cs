using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ultima.Data;

namespace TMRazorImproved.UI.Utilities
{
    public static class UltimaImageHelper
    {
        public static BitmapSource? ToBitmapSource(Bitmap? bitmap)
        {
            if (bitmap == null || bitmap.PixelData == null) return null;

            int width = bitmap.Width;
            int height = bitmap.Height;
            
            // WPF BGRA32 is 4 bytes per pixel
            byte[] bgraData = new byte[width * height * 4];

            if (bitmap.PixelFormat == Ultima.Data.Imaging.PixelFormat.Format16bppArgb1555)
            {
                for (int i = 0; i < width * height; i++)
                {
                    ushort pixel = BitConverter.ToUInt16(bitmap.PixelData, i * 2);
                    
                    // ARGB 1555: A(1) R(5) G(5) B(5)
                    byte a = (byte)((pixel >> 15) != 0 ? 255 : 0);
                    byte r = (byte)((pixel >> 10) & 0x1F);
                    byte g = (byte)((pixel >> 5) & 0x1F);
                    byte b = (byte)(pixel & 0x1F);

                    // Normalize to 8-bit
                    bgraData[i * 4 + 0] = (byte)((b << 3) | (b >> 2)); // B
                    bgraData[i * 4 + 1] = (byte)((g << 3) | (g >> 2)); // G
                    bgraData[i * 4 + 2] = (byte)((r << 3) | (r >> 2)); // R
                    bgraData[i * 4 + 3] = a;                          // A
                }
            }
            else if (bitmap.PixelFormat == Ultima.Data.Imaging.PixelFormat.Format32bppArgb)
            {
                // Already 32-bit, but check channel order (UO is usually ARGB, WPF is BGRA)
                for (int i = 0; i < width * height; i++)
                {
                    bgraData[i * 4 + 0] = bitmap.PixelData[i * 4 + 0]; // B
                    bgraData[i * 4 + 1] = bitmap.PixelData[i * 4 + 1]; // G
                    bgraData[i * 4 + 2] = bitmap.PixelData[i * 4 + 2]; // R
                    bgraData[i * 4 + 3] = bitmap.PixelData[i * 4 + 3]; // A
                }
            }

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, bgraData, width * 4);
        }
    }
}
