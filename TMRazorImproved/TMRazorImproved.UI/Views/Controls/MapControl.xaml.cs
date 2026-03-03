using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.Views.Controls
{
    public partial class MapControl : UserControl
    {
        private IMapService? _mapService;
        private WriteableBitmap? _bitmap;
        private const int ViewSize = 512; // 512x512 pixel viewport

        public static readonly DependencyProperty MapIdProperty =
            DependencyProperty.Register(nameof(MapId), typeof(int), typeof(MapControl), new PropertyMetadata(0, OnMapChanged));

        public static readonly DependencyProperty PlayerXProperty =
            DependencyProperty.Register(nameof(PlayerX), typeof(int), typeof(MapControl), new PropertyMetadata(0, OnPositionChanged));

        public static readonly DependencyProperty PlayerYProperty =
            DependencyProperty.Register(nameof(PlayerY), typeof(int), typeof(MapControl), new PropertyMetadata(0, OnPositionChanged));

        public int MapId { get => (int)GetValue(MapIdProperty); set => SetValue(MapIdProperty, value); }
        public int PlayerX { get => (int)GetValue(PlayerXProperty); set => SetValue(PlayerXProperty, value); }
        public int PlayerY { get => (int)GetValue(PlayerYProperty); set => SetValue(PlayerYProperty, value); }

        public MapControl()
        {
            InitializeComponent();
            _mapService = App.GetService<IMapService>();
            
            // Inizializza bitmap 512x512
            _bitmap = new WriteableBitmap(ViewSize, ViewSize, 96, 96, PixelFormats.Bgr555, null);
            MapImage.Source = _bitmap;
            
            Loaded += (s, e) => RedrawMap();
        }

        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MapControl)d).RedrawMap();
        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MapControl)d).UpdateOverlay();

        private void RedrawMap()
        {
            if (_mapService == null || _bitmap == null) return;

            int startBlockX = (PlayerX >> 3) - (ViewSize / 16);
            int startBlockY = (PlayerY >> 3) - (ViewSize / 16);

            _bitmap.Lock();
            try
            {
                for (int by = 0; by < ViewSize / 8; by++)
                {
                    for (int bx = 0; bx < ViewSize / 8; bx++)
                    {
                        var pixels = _mapService.GetMapBlock(MapId, startBlockX + bx, startBlockY + by, true);
                        
                        // Scriviamo il blocco 8x8 nella bitmap
                        var rect = new Int32Rect(bx * 8, by * 8, 8, 8);
                        _bitmap.WritePixels(rect, pixels, 16, 0); // 16 bytes per riga (8 pixel * 2 bytes)
                    }
                }
            }
            finally
            {
                _bitmap.Unlock();
            }
            
            UpdateOverlay();
        }

        private void UpdateOverlay()
        {
            // Posiziona il marker del player al centro del controllo (dato che il redraw centra su di lui)
            // Se volessimo muovere il player senza ridisegnare la mappa ogni tile, dovremmo calcolare l'offset
            Canvas.SetLeft(PlayerMarker, ViewSize / 2 - 5);
            Canvas.SetTop(PlayerMarker, ViewSize / 2 - 5);
            
            // Se il player si è spostato di molto rispetto all'ultimo redraw, forziamo redraw
            // Per ora semplifichiamo: ricalcoliamo la mappa se cambia posizione significativa
            // (In un'app reale useremmo un buffer più grande e sposteremmo l'immagine)
            RedrawMap();
        }
    }
}
