using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
}
