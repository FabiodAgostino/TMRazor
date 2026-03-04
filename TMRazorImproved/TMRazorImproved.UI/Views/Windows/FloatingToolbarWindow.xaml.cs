using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class FloatingToolbarWindow : Window
    {
        public FloatingToolbarViewModel ViewModel { get; }

        private readonly IClientInteropService _clientInterop;
        private readonly DispatcherTimer _positionTimer;
        private bool _userMoved = false;

        public FloatingToolbarWindow(FloatingToolbarViewModel viewModel, IClientInteropService clientInterop)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            _clientInterop = clientInterop;
            InitializeComponent();

            // Timer per mantenere la toolbar sopra il client UO (aggiorna ogni 500ms)
            _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _positionTimer.Tick += OnPositionTimerTick;

            IsVisibleChanged += OnIsVisibleChanged;
            Closed += OnClosed;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                _userMoved = false;
                SnapToUOWindow();
                _positionTimer.Start();
            }
            else
            {
                _positionTimer.Stop();
            }
        }

        private void OnPositionTimerTick(object? sender, EventArgs e)
        {
            // Se l'utente ha spostato manualmente la toolbar non aggiorniamo la posizione
            if (!_userMoved)
                SnapToUOWindow();
        }

        /// <summary>
        /// Posiziona la toolbar al bordo superiore della finestra del client UO.
        /// </summary>
        private void SnapToUOWindow()
        {
            IntPtr hwnd = _clientInterop.FindUOWindow();
            if (hwnd == IntPtr.Zero) return;

            if (!GetWindowRect(hwnd, out RECT rect)) return;

            // Posiziona la toolbar in alto a sinistra, sopra la finestra UO
            Left = rect.left;
            Top  = rect.top - ActualHeight - 2;

            // Se la toolbar uscirebbe dallo schermo in alto, mostrala sovrapposta in alto
            if (Top < 0) Top = rect.top;

            // Adatta la larghezza alla finestra UO (max 800 per non essere troppo larga)
            double uoWidth = rect.right - rect.left;
            Width = Math.Clamp(uoWidth, 400, 800);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _userMoved = true;
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

        private void OnClosed(object? sender, EventArgs e)
        {
            _positionTimer.Stop();
            _positionTimer.Tick -= OnPositionTimerTick;
            IsVisibleChanged    -= OnIsVisibleChanged;
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
