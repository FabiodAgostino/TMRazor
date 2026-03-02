using System;
using System.Runtime.InteropServices;

namespace Ultima.Data
{
    public class Bitmap : IDisposable
    {
        public int Width { get; }
        public int Height { get; }
        public Imaging.PixelFormat PixelFormat { get; }
        public byte[] PixelData { get; private set; }

        public Bitmap(int width, int height, Imaging.PixelFormat format = Imaging.PixelFormat.Format16bppArgb1555)
        {
            Width = width;
            Height = height;
            PixelFormat = format;
            int bpp = (format == Imaging.PixelFormat.Format16bppArgb1555 || format == Imaging.PixelFormat.Format16bppRgb555) ? 2 : 4;
            PixelData = new byte[width * height * bpp];
        }

        public Bitmap(int width, int height, int stride, Imaging.PixelFormat format, IntPtr scan0)
        {
            Width = width;
            Height = height;
            PixelFormat = format;
            int bpp = (format == Imaging.PixelFormat.Format16bppArgb1555 || format == Imaging.PixelFormat.Format16bppRgb555) ? 2 : 4;
            int length = stride * height; 
            PixelData = new byte[length];
            Marshal.Copy(scan0, PixelData, 0, length);
        }

        public Bitmap(Bitmap original)
        {
            Width = original.Width;
            Height = original.Height;
            PixelFormat = original.PixelFormat;
            PixelData = new byte[original.PixelData.Length];
            Array.Copy(original.PixelData, PixelData, original.PixelData.Length);
        }

        public Imaging.BitmapData LockBits(Rectangle rect, Imaging.ImageLockMode flags, Imaging.PixelFormat format)
        {
            return new Imaging.BitmapData(this);
        }

        public void UnlockBits(Imaging.BitmapData data)
        {
            data.Dispose();
        }

        public void Save(string path, Imaging.ImageFormat format = Imaging.ImageFormat.Bmp)
        {
        }

        public void Save(System.IO.Stream stream, Imaging.ImageFormat format = Imaging.ImageFormat.Bmp)
        {
        }

        public void Dispose()
        {
            PixelData = null;
        }
    }
    
    public struct Rectangle 
    {
        public int X, Y, Width, Height;
        public Rectangle(int x, int y, int w, int h) { X=x;Y=y;Width=w;Height=h; }
    }

    public struct Point 
    {
        public int X, Y;
        public Point(int x, int y) { X=x;Y=y; }
        public static Point Empty => new Point(0, 0);
    }

    public struct Color 
    {
        public byte A, R, G, B;
        public static Color Transparent => new Color { A = 0, R = 0, G = 0, B = 0 };
        public static Color FromArgb(int a, int r, int g, int b) => new Color { A = (byte)a, R = (byte)r, G = (byte)g, B = (byte)b };
        public static Color FromArgb(int r, int g, int b) => new Color { A = 255, R = (byte)r, G = (byte)g, B = (byte)b };
    }

    public class Graphics : IDisposable
    {
        private Bitmap _bmp;
        private Graphics(Bitmap bmp) { _bmp = bmp; }
        public static Graphics FromImage(Bitmap bmp) { return new Graphics(bmp); }
        
        public void Clear(Color color) 
        {
            if (_bmp.PixelData != null)
                Array.Clear(_bmp.PixelData, 0, _bmp.PixelData.Length);
        }
        
        public void DrawImage(Bitmap src, int x, int y) 
        {
            int srcBpp = (src.PixelFormat == Imaging.PixelFormat.Format16bppArgb1555 || src.PixelFormat == Imaging.PixelFormat.Format16bppRgb555) ? 2 : 4;
            int dstBpp = (_bmp.PixelFormat == Imaging.PixelFormat.Format16bppArgb1555 || _bmp.PixelFormat == Imaging.PixelFormat.Format16bppRgb555) ? 2 : 4;
            
            if (dstBpp != srcBpp) return;
            
            int dstStride = _bmp.Width * dstBpp;
            int srcStride = src.Width * srcBpp;
            
            for (int sy = 0; sy < src.Height; sy++) 
            {
                int dy = y + sy;
                if (dy < 0 || dy >= _bmp.Height) continue;
                
                for (int sx = 0; sx < src.Width; sx++) 
                {
                    int dx = x + sx;
                    if (dx < 0 || dx >= _bmp.Width) continue;
                    
                    int dstIdx = dy * dstStride + dx * dstBpp;
                    int srcIdx = sy * srcStride + sx * srcBpp;
                    
                    for (int b = 0; b < dstBpp; b++)
                        _bmp.PixelData[dstIdx + b] = src.PixelData[srcIdx + b];
                }
            }
        }
        
        public void DrawImageUnscaled(Bitmap src, int x, int y)
        {
            DrawImage(src, x, y);
        }

        public void DrawImageUnscaled(Bitmap src, int x, int y, int width, int height)
        {
            DrawImage(src, x, y);
        }
        
        public void Dispose() {}
    }
}

namespace Ultima.Data.Imaging
{
    public enum ImageFormat
    {
        Bmp,
        Tiff,
        Png,
        Jpeg,
        Gif
    }

    public enum PixelFormat 
    {
        Format16bppArgb1555,
        Format16bppRgb555,
        Format32bppArgb
    }
    
    public enum ImageLockMode 
    { 
        WriteOnly, 
        ReadOnly, 
        ReadWrite 
    }

    public unsafe class BitmapData : IDisposable
    {
        private System.Runtime.InteropServices.GCHandle _handle;
        public IntPtr Scan0 => _handle.AddrOfPinnedObject();
        public int Stride { get; }
        public int Width { get; }
        public int Height { get; }
        
        public BitmapData(global::Ultima.Data.Bitmap bmp) 
        {
            _handle = System.Runtime.InteropServices.GCHandle.Alloc(bmp.PixelData, System.Runtime.InteropServices.GCHandleType.Pinned);
            int bpp = (bmp.PixelFormat == PixelFormat.Format16bppArgb1555 || bmp.PixelFormat == PixelFormat.Format16bppRgb555) ? 2 : 4;
            Stride = bmp.Width * bpp;
            Width = bmp.Width;
            Height = bmp.Height;
        }

        public void Dispose() 
        {
            if (_handle.IsAllocated) _handle.Free();
        }
    }
}
