using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Wpf.Ui.Controls;

namespace TMRazorImproved.UI.Views.Windows
{
    public partial class ChangelogWindow : FluentWindow
    {
        private const string ChangelogUrl = "https://github.com/FabiodAgostino/TMRazor/blob/main/CHANGELOG.md";
        private const string ChangelogFileName = "CHANGELOG.md";

        public ChangelogWindow()
        {
            InitializeComponent();
            LoadContent();
        }

        private void LoadContent()
        {
            var version = Assembly.GetExecutingAssembly()
                              .GetName().Version ?? new Version(1, 0, 0);

            TitleText.Text = "TMRazor Improved — Changelog";
            VersionText.Text = $"Versione {version.Major}.{version.Minor}.{version.Build}";

            string changelogPath = Path.Combine(AppContext.BaseDirectory, ChangelogFileName);
            if (File.Exists(changelogPath))
            {
                ChangelogContent.Text = File.ReadAllText(changelogPath);
            }
            else
            {
                ChangelogContent.Text = GetEmbeddedChangelog();
            }
        }

        private static string GetEmbeddedChangelog()
        {
            return
                "## v1.0.0 — Rilascio iniziale\r\n" +
                "\r\n" +
                "### Nuovo\r\n" +
                "- Migrazione completa da TMRazor WinForms a WPF + .NET 10\r\n" +
                "- Architettura MVVM con Dependency Injection\r\n" +
                "- Interfaccia moderna con WPF-UI (Fluent Design)\r\n" +
                "- Plugin nativo ClassicUO (TMRazorPlugin) per comunicazione IPC\r\n" +
                "- Supporto profili multipli con selezione automatica per shard\r\n" +
                "\r\n" +
                "### Agenti\r\n" +
                "- AutoLoot con filtro proprietà OPL\r\n" +
                "- Scavenger con filtro proprietà OPL\r\n" +
                "- Dress con gestione conflitto armi a due mani\r\n" +
                "- BandageHeal con calcolo delay DEX-based\r\n" +
                "- Organizer, Restock, Vendor Buy/Sell\r\n" +
                "- Friends con aggiunta manuale giocatori e gilde\r\n" +
                "\r\n" +
                "### Scripting\r\n" +
                "- API Python-like per automazione avanzata\r\n" +
                "- Script Recorder integrato\r\n" +
                "- LegacyMacroMigrator per conversione macro esistenti\r\n" +
                "\r\n" +
                "### Filtri\r\n" +
                "- MobileFilter, TargetFilterManager\r\n" +
                "- WallStaticFilter, StaffFilter\r\n" +
                "- VetRewardGumpFilter\r\n" +
                "\r\n" +
                "Per la lista completa visita: " + ChangelogUrl;
        }

        private void OpenOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(ChangelogUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Impossibile aprire il browser:\n{ex.Message}", "Errore",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
