using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public class ScreenCaptureService : IScreenCaptureService
    {
        private readonly ILogger<ScreenCaptureService> _logger;
        private readonly IClientInteropService _clientInterop;
        private readonly IWorldService _worldService;
        private string _capturePath;

        public ScreenCaptureService(ILogger<ScreenCaptureService> logger, IClientInteropService clientInterop, IWorldService worldService)
        {
            _logger = logger;
            _clientInterop = clientInterop;
            _worldService = worldService;
            _capturePath = Path.Combine(AppContext.BaseDirectory, "Screenshots");

            if (!Directory.Exists(_capturePath))
                Directory.CreateDirectory(_capturePath);
        }

        public void SetCapturePath(string path)
        {
            _capturePath = path;
            if (!Directory.Exists(_capturePath))
                Directory.CreateDirectory(_capturePath);
        }

        public string GetCapturePath() => _capturePath;

        public Task<string> CaptureAsync()
        {
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
                    using (Bitmap bmp = CaptureWindow(hWnd))
                    {
                        string playerName = _worldService.Player?.Name ?? "Unknown";
                        string fileName = $"{playerName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jpg";
                        string fullPath = Path.Combine(_capturePath, fileName);

                        bmp.Save(fullPath, ImageFormat.Jpeg);
                        _logger.LogInformation("Screenshot saved to {Path}", fullPath);
                        return fullPath;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to capture screenshot.");
                    return string.Empty;
                }
            });
        }

        private Bitmap CaptureWindow(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            // In case of minimized window or weird sizes
            if (width <= 0) width = 800;
            if (height <= 0) height = 600;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                IntPtr hdcDest = gfx.GetHdc();

                // PW_RENDERFULLCONTENT (0x00000002) is important for capturing hardware accelerated windows like UO
                bool success = PrintWindow(hWnd, hdcDest, 0x00000002);

                gfx.ReleaseHdc(hdcDest);
            }

            return bmp;
        }

        #region P/Invoke
        private const int SRCCOPY = 0x00CC0020;

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
        #endregion
    }
}
