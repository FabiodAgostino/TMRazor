using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TMRazorImproved.Core.Services.Scripting;
using Wpf.Ui.Controls;
using WinControls = System.Windows.Controls;
using Win = System.Windows;

namespace TMRazorImproved.UI.Views.Windows
{
    /// <summary>
    /// Finestra di riferimento API — mostra tutti i metodi di scripting con
    /// descrizione (da XML docs), firma e tipo di ritorno.
    /// </summary>
    public partial class ApiReferenceWindow : FluentWindow
    {
        // ------------------------------------------------------------------
        // Helper models for the UI
        // ------------------------------------------------------------------

        private class ApiNode
        {
            public ApiDoc Api     { get; init; } = null!;
            public string Header  { get; init; } = string.Empty;
        }

        private class MethodNode
        {
            public MethodDoc Method { get; init; } = null!;
            public ApiDoc    Api    { get; init; } = null!;
            public string    Header { get; init; } = string.Empty;
        }

        private class ParamRow
        {
            public string Display  { get; init; } = string.Empty; // "name" in monospace
            public string TypeInfo { get; init; } = string.Empty; // "type  [= default]"
        }

        // ------------------------------------------------------------------
        // State
        // ------------------------------------------------------------------

        private IReadOnlyList<ApiDoc> _allApis = Array.Empty<ApiDoc>();
        private string _currentFilter = string.Empty;

        // ------------------------------------------------------------------
        // Ctor
        // ------------------------------------------------------------------

        public ApiReferenceWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading...";
                var svc = new AutoDocService();
                _allApis = svc.GetApis();
                PopulateTree(_allApis);
                int total = _allApis.Sum(a => a.Methods.Count);
                StatusText.Text   = $"{_allApis.Count} APIs · {total} methods";
                FooterText.Text   = $"TMRazorImproved.Core — {_allApis.Count} API objects, {total} methods";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        // ------------------------------------------------------------------
        // Tree population
        // ------------------------------------------------------------------

        private void PopulateTree(IEnumerable<ApiDoc> apis, string filter = "")
        {
            ApiTree.Items.Clear();
            filter = filter.Trim().ToLowerInvariant();

            foreach (var api in apis)
            {
                // Filter: only include APIs that have matching methods (or match by name)
                var methods = filter.Length == 0
                    ? api.Methods
                    : api.Methods.Where(m =>
                        m.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        api.VarName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

                if (filter.Length > 0 && methods.Count == 0 &&
                    !api.VarName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                bool expand = filter.Length > 0;
                var groupItem = new WinControls.TreeViewItem
                {
                    Header     = $"  {api.VarName}  ({methods.Count})",
                    Tag        = new ApiNode { Api = api, Header = api.VarName },
                    IsExpanded = expand,
                    FontWeight = System.Windows.FontWeights.SemiBold,
                };

                foreach (var method in methods)
                {
                    var methodItem = new WinControls.TreeViewItem
                    {
                        Header     = $"  .{method.Name}()",
                        Tag        = new MethodNode { Method = method, Api = api, Header = method.Name },
                        FontWeight = System.Windows.FontWeights.Normal,
                    };
                    groupItem.Items.Add(methodItem);
                }

                ApiTree.Items.Add(groupItem);
            }
        }

        // ------------------------------------------------------------------
        // Selection → details panel
        // ------------------------------------------------------------------

        private void ApiTree_SelectedItemChanged(object sender,
            RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not WinControls.TreeViewItem item) return;

            if (item.Tag is ApiNode apiNode)
                ShowApiDetail(apiNode.Api);
            else if (item.Tag is MethodNode methodNode)
                ShowMethodDetail(methodNode.Api, methodNode.Method);
        }

        private void ShowApiDetail(ApiDoc api)
        {
            PlaceholderPanel.Visibility  = Visibility.Collapsed;
            MethodDetailPanel.Visibility = Visibility.Collapsed;
            ApiDetailPanel.Visibility    = Visibility.Visible;

            ApiNameText.Text        = api.VarName;
            ApiTypeText.Text        = $"C# Type: {api.TypeName}";
            ApiDescText.Text        = string.IsNullOrEmpty(api.Description)
                                        ? "No description available."
                                        : api.Description;
            ApiMethodCountText.Text = $"{api.Methods.Count} public methods";
        }

        private void ShowMethodDetail(ApiDoc api, MethodDoc method)
        {
            PlaceholderPanel.Visibility  = Visibility.Collapsed;
            ApiDetailPanel.Visibility    = Visibility.Collapsed;
            MethodDetailPanel.Visibility = Visibility.Visible;

            // Stub warning
            StubWarningBorder.Visibility = method.IsStub ? Visibility.Visible : Visibility.Collapsed;

            // Header
            MethodApiText.Text  = api.VarName;
            MethodNameText.Text = method.Name;

            // Signature (colored in the dark box)
            MethodSignatureText.Text = BuildSignatureDisplay(api.VarName, method);

            // Description
            MethodDescText.Text = string.IsNullOrEmpty(method.Description)
                ? "No description available."
                : method.Description;

            // Parameters
            if (method.Parameters.Count > 0)
            {
                ParamsPanel.Visibility = Visibility.Visible;
                ParamsList.ItemsSource = method.Parameters.Select(p => new ParamRow
                {
                    Display  = p.Name,
                    TypeInfo = p.HasDefault
                        ? $"{p.TypeName}  (optional, default: {p.DefaultValue})"
                        : p.TypeName,
                }).ToList();
            }
            else
            {
                ParamsPanel.Visibility = Visibility.Collapsed;
            }

            // Return type
            ReturnTypeText.Text = method.ReturnType;
        }

        private static string BuildSignatureDisplay(string varName, MethodDoc method)
        {
            string parms = string.Join(", ", method.Parameters.Select(p =>
                p.HasDefault ? $"{p.TypeName} {p.Name} = {p.DefaultValue}"
                             : $"{p.TypeName} {p.Name}"));
            return $"{varName}.{method.Name}({parms}) → {method.ReturnType}";
        }

        // ------------------------------------------------------------------
        // Search
        // ------------------------------------------------------------------

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentFilter = SearchBox.Text;
            PopulateTree(_allApis, _currentFilter);
        }

        // ------------------------------------------------------------------
        // Buttons
        // ------------------------------------------------------------------

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Title      = "Esporta API Reference",
                Filter     = "Markdown (*.md)|*.md|Testo (*.txt)|*.txt|Tutti i file (*.*)|*.*",
                FileName   = "API_Reference.md",
                DefaultExt = ".md",
            };

            if (dlg.ShowDialog(this) != true) return;

            try
            {
                ExportButton.IsEnabled = false;
                var svc = new AutoDocService();
                await svc.ExportAsync(dlg.FileName);
                Win.MessageBox.Show(this,
                    $"Documentazione esportata:\n{dlg.FileName}",
                    "Esportazione completata",
                    Win.MessageBoxButton.OK, Win.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Win.MessageBox.Show(this, $"Errore durante l'esportazione:\n{ex.Message}",
                    "Errore", Win.MessageBoxButton.OK, Win.MessageBoxImage.Error);
            }
            finally
            {
                ExportButton.IsEnabled = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
