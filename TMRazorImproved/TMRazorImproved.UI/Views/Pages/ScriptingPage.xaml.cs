using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class ScriptingPage : Page
    {
        private readonly ScriptingViewModel _viewModel;
        private bool _syncingEditor;
        private bool _initialized;

        public ScriptingPage(ScriptingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            // Wire editor changes → ViewModel (permanent, singleton lifecycle)
            ScriptEditor.TextChanged += OnEditorTextChanged;

            // Wire ViewModel code changes → editor (e.g. after file load)
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

            LoadSyntaxHighlighting();

            // Configure editor options
            ScriptEditor.Options.ConvertTabsToSpaces = true;
            ScriptEditor.Options.IndentationSize     = 4;
            ScriptEditor.WordWrap                    = false;

            // Sync initial code from ViewModel → editor
            _syncingEditor = true;
            ScriptEditor.Text = _viewModel.ScriptCode;
            _syncingEditor = false;
        }

        // ------------------------------------------------------------------
        // Syntax highlighting
        // ------------------------------------------------------------------

        private void LoadSyntaxHighlighting()
        {
            try
            {
                const string resourceName =
                    "TMRazorImproved.UI.Resources.Highlighting.Python.xshd";

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

        // ------------------------------------------------------------------
        // Editor ↔ ViewModel sync
        // ------------------------------------------------------------------

        private void OnEditorTextChanged(object? sender, EventArgs e)
        {
            if (_syncingEditor) return;
            _syncingEditor = true;
            _viewModel.ScriptCode = ScriptEditor.Text;
            _syncingEditor = false;
        }

        private void OnViewModelPropertyChanged(object? sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ScriptingViewModel.ScriptCode)) return;
            if (_syncingEditor) return;

            _syncingEditor = true;
            ScriptEditor.Text = _viewModel.ScriptCode;
            _syncingEditor = false;
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
    }
}
