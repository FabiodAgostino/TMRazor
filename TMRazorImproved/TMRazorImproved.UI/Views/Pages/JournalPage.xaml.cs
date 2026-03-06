using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TMRazorImproved.UI.ViewModels;

namespace TMRazorImproved.UI.Views.Pages
{
    public partial class JournalPage : Page
    {
        public JournalViewModel ViewModel { get; }

        public JournalPage(JournalViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void JournalListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var selectedItems = JournalListView.SelectedItems;
                if (selectedItems.Count > 0)
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (TMRazorImproved.Shared.Models.JournalEntry item in selectedItems)
                    {
                        string time = item.Timestamp.ToString("HH:mm:ss");
                        string name = string.IsNullOrWhiteSpace(item.Name) ? "" : $"{item.Name}: ";
                        sb.AppendLine($"[{time}] {name}{item.Text}");
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
