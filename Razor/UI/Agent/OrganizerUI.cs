using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard organizerItemsCard;
        private RazorCard organizerConfigCard;

        private void InitializeOrganizerTab2()
        {
            // ─────────────────── Card: Item List (left) ────────────────────────
            organizerItemsCard = new RazorCard
            {
                Name = "organizerItemsCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.organizer.Text") ?? "Organizer"),
                Location = new Point(10, 10),
                Size = new Size(430, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // List selector label
            var lblList = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label24.Text") ?? "Organizer List:",
                Location = new Point(10, 34),
                Size = new Size(32, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            organizerListSelect.Location = new Point(46, 30);
            organizerListSelect.Size = new Size(155, 22);
            organizerListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            organizerAddListB.Text = LanguageHelper.GetString("MainForm.organizerAddListB.Text") ?? "Add";
            organizerAddListB.Location = new Point(207, 30);
            organizerAddListB.Size = new Size(55, 22);

            organizerRemoveListB.Text = LanguageHelper.GetString("MainForm.organizerRemoveListB.Text") ?? "Remove";
            organizerRemoveListB.Location = new Point(267, 30);
            organizerRemoveListB.Size = new Size(62, 22);

            organizerCloneListB.Text = LanguageHelper.GetString("MainForm.organizerCloneListB.Text") ?? "Clone";
            organizerCloneListB.Location = new Point(334, 30);
            organizerCloneListB.Size = new Size(58, 22);
            if (organizerCloneListB is RazorButton rbClone)
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
            organizerdataGridView.Location = new Point(10, 63);
            organizerdataGridView.Size = new Size(398, 194);
            organizerdataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            organizerdataGridView.BackgroundColor = RazorTheme.Colors.CurrentCard;
            organizerdataGridView.GridColor = Color.FromArgb(50, 70, 90);
            organizerdataGridView.BorderStyle = BorderStyle.None;
            organizerdataGridView.EnableHeadersVisualStyles = false;
            organizerdataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            organizerdataGridView.DefaultCellStyle.BackColor = RazorTheme.Colors.CurrentCard;
            organizerdataGridView.DefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            organizerdataGridView.DefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(9F);
            organizerdataGridView.DefaultCellStyle.SelectionBackColor = RazorTheme.Colors.Primary;
            organizerdataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            organizerdataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 37, 53);
            organizerdataGridView.ColumnHeadersDefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            organizerdataGridView.ColumnHeadersDefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold);

            // Column header translations
            dataGridViewCheckBoxColumn2.HeaderText = LanguageHelper.GetString("MainForm.OrganizerColumnX.HeaderText") ?? "X";
            dataGridViewTextBoxColumn5.HeaderText = LanguageHelper.GetString("MainForm.OrganizerColumnItemName.HeaderText") ?? "Item Name";
            dataGridViewTextBoxColumn6.HeaderText = LanguageHelper.GetString("MainForm.OrganizerColumnGraphics.HeaderText") ?? "Graphics";
            dataGridViewTextBoxColumn8.HeaderText = LanguageHelper.GetString("MainForm.OrganizerColumnColor.HeaderText") ?? "Color";
            dataGridViewTextBoxColumn7.HeaderText = LanguageHelper.GetString("MainForm.OrganizerColumnAmount.HeaderText") ?? "Amount";

            // Separator above action buttons
            var sepActions = new Panel
            {
                Location = new Point(10, 262),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Add Item button pinned to card bottom
            organizerAddTargetB.Text = LanguageHelper.GetString("MainForm.organizerAddTargetB.Text") ?? "Add Item";
            organizerAddTargetB.Location = new Point(10, 268);
            organizerAddTargetB.Size = new Size(115, 28);
            organizerAddTargetB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            organizerItemsCard.Controls.AddRange(new Control[]
            {
                lblList, organizerListSelect,
                organizerAddListB, organizerRemoveListB, organizerCloneListB,
                sepToolbar,
                organizerdataGridView,
                sepActions,
                organizerAddTargetB
            });

            // ─────────────────── Card: Config + Log (right) ────────────────────
            organizerConfigCard = new RazorCard
            {
                Name = "organizerConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("MainForm.organizerConfigCard.Text") ?? "Settings"),
                Location = new Point(450, 10),
                Size = new Size(214, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Bags section label
            var lblBags = new Label
            {
                Text = LanguageHelper.GetString("MainForm.groupBox11.Text") ?? "Organizer Bags",
                Location = new Point(10, 28),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            // Source row
            var lblSource = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label56.Text") ?? "Source:",
                Location = new Point(10, 46),
                Size = new Size(56, 18),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            organizerSourceLabel.Location = new Point(68, 46);
            organizerSourceLabel.Size = new Size(129, 18);
            organizerSourceLabel.ForeColor = RazorTheme.Colors.CurrentTextSecondary;
            organizerSourceLabel.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Italic);
            organizerSourceLabel.BackColor = Color.Transparent;

            organizerSetSourceB.Text = LanguageHelper.GetString("MainForm.organizerSetSourceB.Text") ?? "Set Source";
            organizerSetSourceB.Location = new Point(10, 66);
            organizerSetSourceB.Size = new Size(187, 26);

            // Separator
            var sep1 = new Panel
            {
                Location = new Point(10, 96),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Destination row
            var lblDest = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label57.Text") ?? "Destination:",
                Location = new Point(10, 102),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            organizerDestination.Location = new Point(10, 118);
            organizerDestination.Size = new Size(187, 22);
            organizerDestination.BackColor = RazorTheme.Colors.CurrentCard;
            organizerDestination.ForeColor = RazorTheme.Colors.CurrentText;
            organizerDestination.Font = RazorTheme.Fonts.DisplayFont(9F);
            organizerDestination.BorderStyle = BorderStyle.FixedSingle;

            organizerSetDestinationB.Text = LanguageHelper.GetString("MainForm.organizerSetDestinationB.Text") ?? "Set Dest";
            organizerSetDestinationB.Location = new Point(10, 142);
            organizerSetDestinationB.Size = new Size(187, 26);

            // Separator
            var sep2 = new Panel
            {
                Location = new Point(10, 172),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Delay row
            var lblDelay = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label27.Text") ?? "Drag Item Delay (ms)",
                Location = new Point(10, 178),
                Size = new Size(125, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            organizerDragDelay.Location = new Point(140, 176);
            organizerDragDelay.Size = new Size(47, 22);
            organizerDragDelay.BackColor = RazorTheme.Colors.CurrentCard;
            organizerDragDelay.ForeColor = RazorTheme.Colors.CurrentText;
            organizerDragDelay.Font = RazorTheme.Fonts.DisplayFont(9F);
            organizerDragDelay.BorderStyle = BorderStyle.FixedSingle;

            // Separator
            var sep3 = new Panel
            {
                Location = new Point(10, 202),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Execute + Stop buttons — styled flat with text
            organizerExecuteButton.BackgroundImage = null;
            organizerExecuteButton.Text = "\xE768  " + (LanguageHelper.GetString("MainForm.organizerExecuteButton.Text") ?? "Avvia");
            organizerExecuteButton.FlatStyle = FlatStyle.Flat;
            organizerExecuteButton.FlatAppearance.BorderSize = 0;
            organizerExecuteButton.BackColor = RazorTheme.Colors.Success;
            organizerExecuteButton.ForeColor = Color.White;
            organizerExecuteButton.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            organizerExecuteButton.Location = new Point(10, 210);
            organizerExecuteButton.Size = new Size(87, 30);

            organizerStopButton.BackgroundImage = null;
            organizerStopButton.Text = "\xE71A  " + (LanguageHelper.GetString("MainForm.organizerStopButton.Text") ?? "Ferma");
            organizerStopButton.FlatStyle = FlatStyle.Flat;
            organizerStopButton.FlatAppearance.BorderSize = 0;
            organizerStopButton.BackColor = RazorTheme.Colors.Danger;
            organizerStopButton.ForeColor = Color.White;
            organizerStopButton.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            organizerStopButton.Location = new Point(107, 210);
            organizerStopButton.Size = new Size(80, 30);

            // Separator
            var sep4 = new Panel
            {
                Location = new Point(10, 244),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Log section
            var lblLog = new Label
            {
                Text = LanguageHelper.GetString("MainForm.groupBox16.Text") ?? "Organizer Log",
                Location = new Point(10, 250),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            organizerLogBox.Location = new Point(10, 266);
            organizerLogBox.Size = new Size(187, 44);
            organizerLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            organizerLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            organizerLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            organizerLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            organizerLogBox.BorderStyle = BorderStyle.None;

            organizerConfigCard.Controls.AddRange(new Control[]
            {
                lblBags,
                lblSource, organizerSourceLabel, organizerSetSourceB,
                sep1,
                lblDest, organizerDestination, organizerSetDestinationB,
                sep2,
                lblDelay, organizerDragDelay,
                sep3,
                organizerExecuteButton, organizerStopButton,
                sep4,
                lblLog, organizerLogBox
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            organizer.Controls.Clear();
            organizer.Controls.AddRange(new Control[] { organizerItemsCard, organizerConfigCard });
            organizer.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }
    }
}
