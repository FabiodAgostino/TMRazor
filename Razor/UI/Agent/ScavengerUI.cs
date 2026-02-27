using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard scavengerItemsCard;
        private RazorCard scavengerConfigCard;

        private void InitializeScavengerTab2()
        {
            // ─────────────────── Card: Item List (left) ────────────────────────
            scavengerItemsCard = new RazorCard
            {
                Name = "scavengerItemsCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.escavenger.Text") ?? "Scavenger"),
                Location = new Point(10, 10),
                Size = new Size(430, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // List selector label
            var lblList = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label22.Text") ?? "List:",
                Location = new Point(10, 34),
                Size = new Size(32, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            scavengerListSelect.Location = new Point(46, 30);
            scavengerListSelect.Size = new Size(155, 22);
            scavengerListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            scavengerButtonAddList.Text = LanguageHelper.GetString("MainForm.scavengerButtonAddList.Text") ?? "Add";
            scavengerButtonAddList.Location = new Point(207, 30);
            scavengerButtonAddList.Size = new Size(55, 22);

            scavengerButtonRemoveList.Text = LanguageHelper.GetString("MainForm.scavengerButtonRemoveList.Text") ?? "Remove";
            scavengerButtonRemoveList.Location = new Point(267, 30);
            scavengerButtonRemoveList.Size = new Size(62, 22);

            scavengerButtonClone.Text = LanguageHelper.GetString("MainForm.scavengerButtonClone.Text") ?? "Clone";
            scavengerButtonClone.Location = new Point(334, 30);
            scavengerButtonClone.Size = new Size(58, 22);
            if (scavengerButtonClone is RazorButton rbClone)
                rbClone.OverrideCustomColor = RazorTheme.Colors.Success;

            // Separator below list toolbar
            var sepToolbar = new Panel
            {
                Location = new Point(10, 56),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // DataGridView — apply dark theme
            scavengerdataGridView.Location = new Point(10, 63);
            scavengerdataGridView.Size = new Size(398, 194);
            scavengerdataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scavengerdataGridView.BackgroundColor = RazorTheme.Colors.CurrentCard;
            scavengerdataGridView.GridColor = Color.FromArgb(50, 70, 90);
            scavengerdataGridView.BorderStyle = BorderStyle.None;
            scavengerdataGridView.EnableHeadersVisualStyles = false;
            scavengerdataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            scavengerdataGridView.DefaultCellStyle.BackColor = RazorTheme.Colors.CurrentCard;
            scavengerdataGridView.DefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            scavengerdataGridView.DefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(9F);
            scavengerdataGridView.DefaultCellStyle.SelectionBackColor = RazorTheme.Colors.Primary;
            scavengerdataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            scavengerdataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 37, 53);
            scavengerdataGridView.ColumnHeadersDefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            scavengerdataGridView.ColumnHeadersDefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold);

            // Separator above action buttons
            var sepActions = new Panel
            {
                Location = new Point(10, 262),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Action buttons pinned to card bottom
            scavengerButtonAddTarget.Text = LanguageHelper.GetString("MainForm.scavengerButtonAddTarget.Text") ?? "Add Item";
            scavengerButtonAddTarget.Location = new Point(10, 268);
            scavengerButtonAddTarget.Size = new Size(115, 28);
            scavengerButtonAddTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            scavengerButtonEditProps.Text = LanguageHelper.GetString("MainForm.scavengerButtonEditProps.Text") ?? "Edit Props";
            scavengerButtonEditProps.Location = new Point(130, 268);
            scavengerButtonEditProps.Size = new Size(115, 28);
            scavengerButtonEditProps.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            scavengerItemsCard.Controls.AddRange(new Control[]
            {
                lblList, scavengerListSelect,
                scavengerButtonAddList, scavengerButtonRemoveList, scavengerButtonClone,
                sepToolbar,
                scavengerdataGridView,
                sepActions,
                scavengerButtonAddTarget, scavengerButtonEditProps
            });

            // ─────────────────── Card: Config + Log (right) ────────────────────
            scavengerConfigCard = new RazorCard
            {
                Name = "scavengerConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("MainForm.scavengerConfigCard.Text") ?? "Settings"),
                Location = new Point(450, 10),
                Size = new Size(214, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Toggles
            scavengerCheckBox.Text = LanguageHelper.GetString("MainForm.scavengerCheckBox.Text") ?? "Enable scavenger";
            scavengerCheckBox.Location = new Point(10, 30);
            scavengerCheckBox.Size = new Size(187, 22);
            scavengerCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            scavengerautostartCheckBox.Text = LanguageHelper.GetString("MainForm.scavengerautostartCheckBox.Text") ?? "Autostart OnLogin";
            scavengerautostartCheckBox.Location = new Point(10, 57);
            scavengerautostartCheckBox.Size = new Size(187, 22);
            scavengerautostartCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            var sep1 = new Panel
            {
                Location = new Point(10, 83),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Bag section
            var lblBag = new Label
            {
                Text = LanguageHelper.GetString("MainForm.groupBox41.Text") ?? "Scavenger Bag",
                Location = new Point(10, 89),
                Size = new Size(90, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            scavengerContainerLabel.Location = new Point(104, 87);
            scavengerContainerLabel.Size = new Size(93, 18);
            scavengerContainerLabel.ForeColor = RazorTheme.Colors.CurrentTextSecondary;
            scavengerContainerLabel.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Italic);
            scavengerContainerLabel.BackColor = Color.Transparent;

            scavengerButtonSetContainer.Text = LanguageHelper.GetString("MainForm.scavengerButtonSetContainer.Text") ?? "Set Bag";
            scavengerButtonSetContainer.Location = new Point(10, 107);
            scavengerButtonSetContainer.Size = new Size(187, 26);

            var sep2 = new Panel
            {
                Location = new Point(10, 137),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Delay field
            var lblDelay = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label23.Text") ?? "Delay (ms):",
                Location = new Point(10, 146),
                Size = new Size(90, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            scavengerDragDelay.Location = new Point(137, 143);
            scavengerDragDelay.Size = new Size(50, 22);
            scavengerDragDelay.BackColor = RazorTheme.Colors.CurrentCard;
            scavengerDragDelay.ForeColor = RazorTheme.Colors.CurrentText;
            scavengerDragDelay.Font = RazorTheme.Fonts.DisplayFont(9F);
            scavengerDragDelay.BorderStyle = BorderStyle.FixedSingle;

            // Max Range field
            var lblRange = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label61.Text") ?? "Max Range:",
                Location = new Point(10, 172),
                Size = new Size(90, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            scavengerRange.Location = new Point(137, 169);
            scavengerRange.Size = new Size(50, 22);
            scavengerRange.BackColor = RazorTheme.Colors.CurrentCard;
            scavengerRange.ForeColor = RazorTheme.Colors.CurrentText;
            scavengerRange.Font = RazorTheme.Fonts.DisplayFont(9F);
            scavengerRange.BorderStyle = BorderStyle.FixedSingle;

            var sep3 = new Panel
            {
                Location = new Point(10, 196),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Activity Log section
            var lblLog = new Label
            {
                Text = LanguageHelper.GetString("MainForm.groupBox12.Text") ?? "Scavenger Log",
                Location = new Point(10, 202),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            scavengerLogBox.Location = new Point(10, 218);
            scavengerLogBox.Size = new Size(187, 90);
            scavengerLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            scavengerLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            scavengerLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            scavengerLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            scavengerLogBox.BorderStyle = BorderStyle.None;

            scavengerConfigCard.Controls.AddRange(new Control[]
            {
                scavengerCheckBox, scavengerautostartCheckBox,
                sep1,
                lblBag, scavengerContainerLabel, scavengerButtonSetContainer,
                sep2,
                lblDelay, scavengerDragDelay,
                lblRange, scavengerRange,
                sep3,
                lblLog, scavengerLogBox
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            escavenger.Controls.Clear();
            escavenger.Controls.AddRange(new Control[] { scavengerItemsCard, scavengerConfigCard });
            escavenger.BackColor = RazorTheme.Colors.BackgroundDark;
        }
    }
}
