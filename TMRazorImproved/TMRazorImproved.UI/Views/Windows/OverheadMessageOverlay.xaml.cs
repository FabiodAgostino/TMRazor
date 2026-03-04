using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class OverheadMessageOverlay : Window
    {
        public OverheadMessageOverlayViewModel ViewModel { get; }

        private readonly IClientInteropService _clientInterop;
        private readonly DispatcherTimer _positionTimer;

        public OverheadMessageOverlay(OverheadMessageOverlayViewModel viewModel, IClientInteropService clientInterop)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            _clientInterop = clientInterop;
            InitializeComponent();

            // Segue la finestra UO ogni 500ms
            _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _positionTimer.Tick += OnPositionTick;

            IsVisibleChanged += OnIsVisibleChanged;
            Closed += OnClosed;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                SnapToUOWindow();
                _positionTimer.Start();
            }
            else
            {
                _positionTimer.Stop();
            }
        }

        private void OnPositionTick(object? sender, EventArgs e) => SnapToUOWindow();

        /// <summary>
        /// Ridimensiona e posiziona l'overlay per coprire esattamente la finestra del client UO.
        /// </summary>
        private void SnapToUOWindow()
        {
            IntPtr hwnd = _clientInterop.FindUOWindow();
            if (hwnd == IntPtr.Zero) return;

            if (!GetWindowRect(hwnd, out RECT rect)) return;

            Left   = rect.left;
            Top    = rect.top;
            Width  = rect.right  - rect.left;
            Height = rect.bottom - rect.top;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _positionTimer.Stop();
            ViewModel.Dispose();
        }

        #region P/Invoke
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left, top, right, bottom; }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        #endregion
    }
}
