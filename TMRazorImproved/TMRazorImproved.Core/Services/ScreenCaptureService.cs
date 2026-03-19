using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public class ScreenCaptureService : IScreenCaptureService
    {
        private readonly ILogger<ScreenCaptureService> _logger;
        private readonly IClientInteropService _clientInterop;
        private readonly IWorldService _worldService;
        private readonly IConfigService _config;
        private string _capturePath = string.Empty;

        public ScreenCaptureService(ILogger<ScreenCaptureService> logger, IClientInteropService clientInterop, IWorldService worldService, IConfigService config)
        {
            _logger = logger;
            _clientInterop = clientInterop;
            _worldService = worldService;
            _config = config;

            UpdatePath();
        }

        private void UpdatePath()
        {
            _capturePath = _config.CurrentProfile.Media.ScreenshotPath;
            if (string.IsNullOrEmpty(_capturePath))
            {
                _capturePath = Path.Combine(AppContext.BaseDirectory, "Screenshots");
            }

            if (!Directory.Exists(_capturePath))
                Directory.CreateDirectory(_capturePath);
        }

        public void SetCapturePath(string path)
        {
            _capturePath = path;
            if (!Directory.Exists(_capturePath))
                Directory.CreateDirectory(_capturePath);
        }

        public string GetCapturePath()
        {
            UpdatePath();
            return _capturePath;
        }

        public Task<string> CaptureAsync()
        {
            UpdatePath();
            return Task.Run(() =>
            {
                IntPtr hWnd = _clientInterop.GetWindowHandle();
                if (hWnd == IntPtr.Zero)
                {
                    _logger.LogWarning("Cannot capture: UO Window handle is Zero.");
                    return string.Empty;
                }

                try
                {
                    string playerName = _worldService.Player?.Name ?? "Unknown";
                    string format = _config.CurrentProfile.Media.ScreenshotFormat.ToUpper();
                    string extension = format == "PNG" ? "png" : "jpg";
                    string fileName = $"{playerName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.{extension}";
                    string fullPath = Path.Combine(_capturePath, fileName);

                    // Usa PrintWindow via P/Invoke per catturare la finestra UO
                    // poi converte in BitmapSource WPF tramite interop
                    RECT rect = default;
                    GetWindowRect(hWnd, ref rect);
                    int width  = Math.Max(rect.right - rect.left, 800);
                    int height = Math.Max(rect.bottom - rect.top, 600);

                    // Crea un DIB compatibile via GDI (nessun System.Drawing)
                    IntPtr hdc    = GetDC(IntPtr.Zero);
                    IntPtr hdcMem = CreateCompatibleDC(hdc);
                    IntPtr hBmp   = CreateCompatibleBitmap(hdc, width, height);
                    IntPtr hOld   = SelectObject(hdcMem, hBmp);

                    PrintWindow(hWnd, hdcMem, 0x00000002);

                    SelectObject(hdcMem, hOld);
                    DeleteDC(hdcMem);
                    ReleaseDC(IntPtr.Zero, hdc);

                    // Converti HBITMAP in BitmapSource WPF
                    BitmapSource bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(
                        hBmp, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    DeleteObject(hBmp);

                    // Salva usando l'encoder appropriato
                    BitmapEncoder encoder;
                    if (format == "PNG")
                    {
                        encoder = new PngBitmapEncoder();
                    }
                    else
                    {
                        encoder = new JpegBitmapEncoder { QualityLevel = _config.CurrentProfile.Media.ScreenshotQuality };
                    }

                    encoder.Frames.Add(BitmapFrame.Create(bmpSrc));
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    _logger.LogInformation("Screenshot saved to {Path}", fullPath);
                    return fullPath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to capture screenshot.");
                    return string.Empty;
                }
            });
        }

        #region P/Invoke
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);
        #endregion
    }
}
