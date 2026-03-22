using System.Diagnostics;
using System.Windows;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Windows
{
    /// <summary>
    /// Dialogo custom multi-pulsante in stile TMRazor — rimpiazza RE_MessageBox del legacy WinForms (FR-081).
    /// </summary>
    public partial class MessageDialog : FluentWindow
    {
        private MessageDialog(string title, string message,
            string? link, string ok, string? no, string? cancel)
        {
            InitializeComponent();

            Title = title;
            MessageText.Text = message;

            OkButton.Content = ok;
            OkButton.Click += (_, _) =>
            {
                DialogResult = ok == "Yes" ? true : true;
                Close();
            };

            if (no != null)
            {
                NoButton.Content = no;
                NoButton.Visibility = Visibility.Visible;
                NoButton.Click += (_, _) => { DialogResult = false; Close(); };
            }

            if (cancel != null)
            {
                CancelButton.Content = cancel;
                CancelButton.Visibility = Visibility.Visible;
                CancelButton.Click += (_, _) => { DialogResult = null; Close(); };
            }

            if (link != null)
            {
                LinkRun.Text = link;
                LinkAnchor.NavigateUri = new System.Uri(link);
                LinkAnchor.RequestNavigate += (_, e) =>
                    Process.Start(new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true });
                LinkBlock.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Mostra un dialogo modale e ritorna: true=Ok/Yes, false=No, null=Cancel/closed.
        /// </summary>
        public static bool? Show(string title, string message, string? link = null,
            string ok = "Ok", string? no = null, string? cancel = "Cancel",
            Window? owner = null)
        {
            var dlg = new MessageDialog(title, message, link, ok, no, cancel);
            if (owner != null) dlg.Owner = owner;
            dlg.ShowDialog();
            return dlg.DialogResult;
        }
    }
}
