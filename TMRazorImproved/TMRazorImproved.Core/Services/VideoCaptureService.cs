using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using SharpAvi;
using SharpAvi.Output;
using SharpAvi.Codecs;

namespace TMRazorImproved.Core.Services
{
    public class VideoCaptureService : IVideoCaptureService
    {
        private readonly ILogger<VideoCaptureService> _logger;
        private readonly IClientInteropService _clientInterop;
        private readonly IWorldService _worldService;
        private readonly IConfigService _config;
        private string _videoPath = string.Empty;
        
        private AviWriter? _writer;
        private IAviVideoStream? _stream;
        private CancellationTokenSource? _cts;
        private Task? _captureTask;

        public bool IsRecording => _cts != null;

        public VideoCaptureService(ILogger<VideoCaptureService> logger, IClientInteropService clientInterop, IWorldService worldService, IConfigService config)
        {
            _logger = logger;
            _clientInterop = clientInterop;
            _worldService = worldService;
            _config = config;

            UpdatePath();
        }

        private void UpdatePath()
        {
            _videoPath = _config.CurrentProfile.Media.VideoPath;
            if (string.IsNullOrEmpty(_videoPath))
            {
                _videoPath = Path.Combine(AppContext.BaseDirectory, "Videos");
            }

            if (!Directory.Exists(_videoPath))
                Directory.CreateDirectory(_videoPath);
        }

        public async Task<bool> StartAsync(int fps = 0)
        {
            if (IsRecording) return false;

            if (fps == 0) fps = _config.CurrentProfile.Media.VideoFps;
            if (fps <= 0) fps = 15;

            UpdatePath();

            IntPtr hWnd = _clientInterop.GetWindowHandle();
            if (hWnd == IntPtr.Zero)
            {
                _logger.LogWarning("Cannot record video: UO Window handle is Zero.");
                return false;
            }

            try
            {
                string playerName = _worldService.Player?.Name ?? "Unknown";
                string fileName = $"{playerName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.avi";
                string fullPath = Path.Combine(_videoPath, fileName);

                RECT rect = default;
                GetWindowRect(hWnd, ref rect);
                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;

                // Dimensioni minime e arrotondamento per SharpAvi (deve essere multiplo di 2 o 4 spesso)
                width = Math.Max(width, 800);
                height = Math.Max(height, 600);
                if (width % 2 != 0) width++;
                if (height % 2 != 0) height++;

                _writer = new AviWriter(fullPath)
                {
                    FramesPerSecond = fps,
                    EmitIndex1 = true,
                };

                // Uncompressed video stream
                _stream = _writer.AddEncodingVideoStream(new UncompressedVideoEncoder(width, height));
                _stream.Name = "TMRazor Capture";

                _cts = new CancellationTokenSource();
                _captureTask = Task.Run(() => CaptureLoop(hWnd, width, height, fps, _cts.Token));

                _logger.LogInformation("Video recording started: {Path}", fullPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start video recording.");
                await StopAsync();
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (_cts == null) return;

            _cts.Cancel();
            if (_captureTask != null)
            {
                try { await _captureTask; } catch { }
            }

            _writer?.Close();
            _writer = null;
            _stream = null;
            _cts.Dispose();
            _cts = null;
            _captureTask = null;

            _logger.LogInformation("Video recording stopped.");
        }

        private async Task CaptureLoop(IntPtr hWnd, int width, int height, int fps, CancellationToken token)
        {
            int frameDelay = 1000 / fps;
            byte[] frameBuffer = new byte[width * height * 4];

            while (!token.IsCancellationRequested)
            {
                var startTime = DateTime.UtcNow;

                try
                {
                    // Cattura frame
                    CaptureFrame(hWnd, width, height, frameBuffer);
                    
                    // Scrivi su stream (SharpAvi aspetta BGR o BGRA a seconda dell'encoder)
                    _stream?.WriteFrame(true, frameBuffer);
                }
                catch (Exception ex)
                {
                    _logger.LogTrace("Error capturing frame: {Msg}", ex.Message);
                }

                int elapsed = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                int wait = Math.Max(1, frameDelay - elapsed);
                
                try { await Task.Delay(wait, token); } catch { break; }
            }
        }

        private void CaptureFrame(IntPtr hWnd, int width, int height, byte[] buffer)
        {
            // Usa lo stesso metodo di ScreenCaptureService per ottenere l'HBITMAP
            IntPtr hdc    = GetDC(IntPtr.Zero);
            IntPtr hdcMem = CreateCompatibleDC(hdc);
            IntPtr hBmp   = CreateCompatibleBitmap(hdc, width, height);
            IntPtr hOld   = SelectObject(hdcMem, hBmp);

            PrintWindow(hWnd, hdcMem, 0x00000002);

            // Converti HBITMAP in BitmapSource e poi in buffer
            BitmapSource bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(
                hBmp, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Assicuriamoci che sia nel formato corretto (BGRA32)
            if (bmpSrc.Format != PixelFormats.Bgra32)
            {
                bmpSrc = new FormatConvertedBitmap(bmpSrc, PixelFormats.Bgra32, null, 0);
            }

            bmpSrc.CopyPixels(buffer, width * 4, 0);

            SelectObject(hdcMem, hOld);
            DeleteObject(hBmp);
            DeleteDC(hdcMem);
            ReleaseDC(IntPtr.Zero, hdc);
        }

        #region P/Invoke
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left; public int top; public int right; public int bottom; }

        [DllImport("user32.dll")] private static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        [DllImport("user32.dll")] private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);
        [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]  private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]  private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
        [DllImport("gdi32.dll")]  private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]  private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);
        #endregion
    }
}
