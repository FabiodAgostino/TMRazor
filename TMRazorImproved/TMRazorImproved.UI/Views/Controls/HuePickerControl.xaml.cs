using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.UI.Views.Controls
{
    public partial class HuePickerControl : UserControl
    {
        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register(nameof(Hue), typeof(int), typeof(HuePickerControl), 
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHueChanged));

        public int Hue
        {
            get => (int)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public static readonly DependencyProperty PreviewBrushProperty =
            DependencyProperty.Register(nameof(PreviewBrush), typeof(Brush), typeof(HuePickerControl), new PropertyMetadata(Brushes.Black));

        public Brush PreviewBrush
        {
            get => (Brush)GetValue(PreviewBrushProperty);
            set => SetValue(PreviewBrushProperty, value);
        }

        public static readonly DependencyProperty PreviewForegroundProperty =
            DependencyProperty.Register(nameof(PreviewForeground), typeof(Brush), typeof(HuePickerControl), new PropertyMetadata(Brushes.White));

        public Brush PreviewForeground
        {
            get => (Brush)GetValue(PreviewForegroundProperty);
            set => SetValue(PreviewForegroundProperty, value);
        }

        public ICommand PickInGameCommand { get; }

        public HuePickerControl()
        {
            InitializeComponent();
            PickInGameCommand = new RelayCommand(OnPickInGame);
            UpdatePreview();
        }

        private static void OnHueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HuePickerControl control)
            {
                control.UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            int h = Hue & 0x3FFF;
            if (h > 0 && h < 3000)
            {
                // Ultima.Hues.GetHue is 1-based for the list? 
                // Old code says: Ultima.Hues.GetHue(hue - 1).GetColor(30)
                var hueData = Ultima.Hues.GetHue(h); 
                var color = hueData.GetColor(30);
                
                var wpfColor = Color.FromRgb(color.R, color.G, color.B);
                PreviewBrush = new SolidColorBrush(wpfColor);

                // Calculate brightness to set foreground
                double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
                PreviewForeground = brightness < 0.5 ? Brushes.White : Brushes.Black;
            }
            else
            {
                PreviewBrush = Brushes.Black;
                PreviewForeground = Brushes.White;
            }
        }

        private void OnPickInGame()
        {
            var packetService = App.GetService<IPacketService>();
            if (packetService == null) return;

            // Packet 0x95: Hue Picker
            // Length: 9
            // [0] 0x95
            // [1..4] Target Serial (0xFFFFFFFF for manual)
            // [5..6] Model (0x0FAB is default)
            // [7..8] Color (unused on send usually)
            
            byte[] data = new byte[9];
            data[0] = 0x95;
            
            // Serial 0xFFFFFFFF
            data[1] = 0xFF; data[2] = 0xFF; data[3] = 0xFF; data[4] = 0xFF;
            
            // Model 0x0FAB
            data[5] = 0x0F; data[6] = 0xAB;
            
            // Color 0
            data[7] = 0; data[8] = 0;

            packetService.SendToClient(data);

            // Register a one-time filter or viewer for the response (0x95 from client)
            packetService.RegisterFilter(PacketPath.ClientToServer, 0x95, OnHuePickerResponse);
        }

        private bool OnHuePickerResponse(byte[] data)
        {
            if (data.Length >= 9 && data[0] == 0x95)
            {
                // Response 0x95:
                // [0] 0x95
                // [1..4] Serial
                // [5..6] Model
                // [7..8] Hue
                
                int hue = (data[7] << 8) | data[8];
                
                // Use Dispatcher to update UI
                Dispatcher.Invoke(() => 
                {
                    Hue = hue;
                });

                // Unregister after one hit
                var packetService = App.GetService<IPacketService>();
                packetService?.UnregisterFilter(PacketPath.ClientToServer, 0x95, OnHuePickerResponse);
                
                return false; // Block the packet from going to server? 
                // In classic Razor it was likely blocked or handled.
            }
            return true;
        }
    }
}
