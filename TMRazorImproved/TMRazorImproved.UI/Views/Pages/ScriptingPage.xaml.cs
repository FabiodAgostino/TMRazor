using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using TMRazorImproved.UI.ViewModels;
using TMRazorImproved.UI.Utilities;
using TMRazorImproved.UI.Views.Windows;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class ScriptingPage : Page
    {
        private readonly ScriptingViewModel _viewModel;
        private bool _initialized;
        private CompletionWindow? _completionWindow;
        private BreakpointMargin? _bpMargin;
        private CurrentLineHighlighter? _lineHighlighter;

        public ScriptingPage(ScriptingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            // Completion events
            ScriptEditor.TextArea.TextEntering += OnTextEntering;
            ScriptEditor.TextArea.TextEntered += OnTextEntered;

            // Wire ViewModel language changes → editor
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // Auto-scroll log when new entries arrive
            _viewModel.LogEntries.CollectionChanged += OnLogEntriesChanged;

            Loaded += OnLoaded;
        }

        // ------------------------------------------------------------------
        // One-time initialization on first load
        // ------------------------------------------------------------------

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_initialized) return;
            _initialized = true;

            CompletionService.Initialize();
            LoadSyntaxHighlighting(_viewModel.SelectedLanguage);

            // Configure editor options
            ScriptEditor.Options.ConvertTabsToSpaces = true;
            ScriptEditor.Options.IndentationSize     = 4;
            ScriptEditor.WordWrap                    = false;

            // Add breakpoint margin (left of line numbers)
            _bpMargin = new BreakpointMargin(_viewModel);
            ScriptEditor.TextArea.LeftMargins.Insert(0, _bpMargin);

            // Add current-line background renderer
            _lineHighlighter = new CurrentLineHighlighter(_viewModel);
            ScriptEditor.TextArea.TextView.BackgroundRenderers.Add(_lineHighlighter);

            // Redraw margin when breakpoints or current line changes
            _viewModel.BreakpointsChanged += () => _bpMargin?.InvalidateVisual();
            _viewModel.PropertyChanged    += (_, args) =>
            {
                if (args.PropertyName == nameof(ScriptingViewModel.CurrentDebugLine) ||
                    args.PropertyName == nameof(ScriptingViewModel.IsPaused))
                {
                    _bpMargin?.InvalidateVisual();
                    _lineHighlighter?.Redraw();
                }
            };
        }

        // ------------------------------------------------------------------
        // Code Completion
        // ------------------------------------------------------------------

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true. We still want to insert the character that was typed.
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                // Find the word before the dot
                int offset = ScriptEditor.CaretOffset;
                if (offset < 2) return;

                int start = offset - 2;
                while (start >= 0 && (char.IsLetterOrDigit(ScriptEditor.Document.GetCharAt(start)) || ScriptEditor.Document.GetCharAt(start) == '_'))
                {
                    start--;
                }
                start++;

                string context = ScriptEditor.Document.GetText(start, offset - start - 1);
                ShowCompletion(context);
            }
            else if (char.IsLetter(e.Text[0]) && _completionWindow == null)
            {
                // We could show global completion here, but it might be too noisy.
                // ShowCompletion(null); 
            }
        }

        private void ShowCompletion(string? context)
        {
            var completionData = CompletionService.GetCompletionData(context);
            if (completionData == null || completionData.Count == 0) return;

            _completionWindow = new CompletionWindow(ScriptEditor.TextArea);
            IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
            foreach (var item in completionData)
            {
                data.Add(item);
            }

            _completionWindow.Show();
            _completionWindow.Closed += (o, args) => _completionWindow = null;
        }

        // ------------------------------------------------------------------
        // Syntax highlighting
        // ------------------------------------------------------------------

        private void LoadSyntaxHighlighting(Shared.Enums.ScriptLanguage language)
        {
            try
            {
                string resourceName = language switch
                {
                    Shared.Enums.ScriptLanguage.Python => "TMRazorImproved.UI.Resources.Highlighting.Python.xshd",
                    Shared.Enums.ScriptLanguage.UOSteam => "TMRazorImproved.UI.Resources.Highlighting.UOSteam.xshd",
                    Shared.Enums.ScriptLanguage.CSharp => "TMRazorImproved.UI.Resources.Highlighting.CSharp.xshd",
                    _ => ""
                };

                if (string.IsNullOrEmpty(resourceName))
                {
                    ScriptEditor.SyntaxHighlighting = null;
                    return;
                }

                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(resourceName);

                if (stream is null) return;

                using var reader = new XmlTextReader(stream);
                ScriptEditor.SyntaxHighlighting =
                    HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            catch
            {
                // Syntax highlighting is cosmetic; never crash on failure
            }
        }

        private void OnViewModelPropertyChanged(object? sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScriptingViewModel.SelectedLanguage))
            {
                LoadSyntaxHighlighting(_viewModel.SelectedLanguage);
            }
        }

        // ------------------------------------------------------------------
        // Auto-scroll log to bottom
        // ------------------------------------------------------------------

        private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            if (LogListBox.Items.Count == 0) return;

            LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
        }

        // ------------------------------------------------------------------
        // Output Window Copy Support
        // ------------------------------------------------------------------
        private void OpenApiReference_Click(object sender, RoutedEventArgs e)
        {
            var win = new ApiReferenceWindow { Owner = Window.GetWindow(this) };
            win.Show();
        }

        private void LogListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var selectedItems = LogListBox.SelectedItems;
                if (selectedItems.Count > 0)
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (ScriptLogEntry item in selectedItems)
                    {
                        sb.AppendLine($"[{item.FormattedTime}] {item.Text}");
                    }
                    try
                    {
                        Clipboard.SetText(sb.ToString().TrimEnd());
                    }
                    catch { /* Ignore clipboard errors */ }
                    e.Handled = true;
                }
            }
        }
    }

    // -----------------------------------------------------------------------
    // Breakpoint gutter: red circle per breakpoint, yellow arrow per current line
    // -----------------------------------------------------------------------
    internal sealed class BreakpointMargin : AbstractMargin
    {
        private readonly ScriptingViewModel _vm;
        private new const double Width = 18;

        public BreakpointMargin(ScriptingViewModel vm) => _vm = vm;

        protected override Size MeasureOverride(Size availableSize)
            => new Size(Width, 0);

        protected override void OnRender(DrawingContext dc)
        {
            var textView = TextView;
            if (textView == null || !textView.VisualLinesValid) return;

            var breakpoints = _vm.Breakpoints.ToList();
            int currentLine = _vm.CurrentDebugLine;
            bool isPaused   = _vm.IsPaused;

            foreach (var line in textView.VisualLines)
            {
                int lineNum = line.FirstDocumentLine.LineNumber;
                double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop)
                           - textView.VerticalOffset;
                double h = line.Height;

                if (breakpoints.Contains(lineNum))
                {
                    // Red circle
                    double r = Math.Min(h, Width) * 0.4;
                    dc.DrawEllipse(Brushes.Crimson, null,
                        new Point(Width / 2, y + h / 2), r, r);
                }

                if (isPaused && lineNum == currentLine)
                {
                    // Yellow arrow (right-pointing triangle)
                    var pts = new[]
                    {
                        new Point(3,          y + h / 2 - 5),
                        new Point(3,          y + h / 2 + 5),
                        new Point(Width - 2,  y + h / 2),
                    };
                    var geo = new StreamGeometry();
                    using (var ctx = geo.Open())
                    {
                        ctx.BeginFigure(pts[0], true, true);
                        ctx.LineTo(pts[1], true, false);
                        ctx.LineTo(pts[2], true, false);
                    }
                    dc.DrawGeometry(Brushes.Gold, null, geo);
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var textView = TextView;
            if (textView == null) return;

            var pos = e.GetPosition(textView);
            pos = new Point(0, pos.Y + textView.VerticalOffset);
            var visualLine = textView.GetVisualLineFromVisualTop(pos.Y);
            if (visualLine == null) return;

            int line = visualLine.FirstDocumentLine.LineNumber;
            _vm.ToggleBreakpoint(line);
            InvalidateVisual();
            e.Handled = true;
        }
    }

    // -----------------------------------------------------------------------
    // Current-line yellow background highlight (shown while paused)
    // -----------------------------------------------------------------------
    internal sealed class CurrentLineHighlighter : IBackgroundRenderer
    {
        private readonly ScriptingViewModel _vm;
        private TextView? _textView;

        public CurrentLineHighlighter(ScriptingViewModel vm) => _vm = vm;

        public KnownLayer Layer => KnownLayer.Background;

        public void SetTextView(TextView tv) => _textView = tv;

        public void Redraw()
        {
            _textView?.Redraw();
        }

        public void Draw(TextView textView, DrawingContext dc)
        {
            _textView = textView;
            if (!_vm.IsPaused || _vm.CurrentDebugLine <= 0) return;
            if (!textView.VisualLinesValid) return;

            var brush = new SolidColorBrush(Color.FromArgb(60, 255, 220, 0));
            brush.Freeze();

            foreach (var vl in textView.VisualLines)
            {
                if (vl.FirstDocumentLine.LineNumber != _vm.CurrentDebugLine) continue;
                double y = vl.GetTextLineVisualYPosition(vl.TextLines[0], VisualYPosition.TextTop)
                           - textView.VerticalOffset;
                dc.DrawRectangle(brush, null,
                    new Rect(0, y, textView.ActualWidth, vl.Height));
                break;
            }
        }
    }
}
