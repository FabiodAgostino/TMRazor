using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard restockItemsCard;
        private RazorCard restockConfigCard;

        private void InitializeRestockTab2()
        {
            // ─────────────────── Card: Item List (left) ────────────────────────
            restockItemsCard = new RazorCard
            {
                Name = "restockItemsCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.restock.Text") ?? "Restock"),
                Location = new Point(10, 10),
                Size = new Size(430, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblList = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label7.Text") ?? "List:",
                Location = new Point(10, 34),
                Size = new Size(32, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            restockListSelect.Location = new Point(46, 30);
            restockListSelect.Size = new Size(155, 22);
            restockListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            restockAddListB.Text = LanguageHelper.GetString("restockAddListButton") ?? "Add";
            restockAddListB.Location = new Point(207, 30);
            restockAddListB.Size = new Size(65, 22);

            restockRemoveListB.Text = LanguageHelper.GetString("restockRemoveListButton") ?? "Remove";
            restockRemoveListB.Location = new Point(278, 30);
            restockRemoveListB.Size = new Size(62, 22);

            restockCloneListB.Text = LanguageHelper.GetString("restockCloneListButton") ?? "Clone";
            restockCloneListB.Location = new Point(346, 30);
            restockCloneListB.Size = new Size(58, 22);
            if (restockCloneListB is RazorButton rbClone)
                rbClone.OverrideCustomColor = RazorTheme.Colors.Success;

            var sepToolbar = new Panel
            {
                Location = new Point(10, 56),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            restockdataGridView.Location = new Point(10, 63);
            restockdataGridView.Size = new Size(398, 194);
            restockdataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            restockdataGridView.BackgroundColor = RazorTheme.Colors.CurrentCard;
            restockdataGridView.GridColor = Color.FromArgb(50, 70, 90);
            restockdataGridView.BorderStyle = BorderStyle.None;
            restockdataGridView.EnableHeadersVisualStyles = false;
            restockdataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            restockdataGridView.DefaultCellStyle.BackColor = RazorTheme.Colors.CurrentCard;
            restockdataGridView.DefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            restockdataGridView.DefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(9F);
            restockdataGridView.DefaultCellStyle.SelectionBackColor = RazorTheme.Colors.Primary;
            restockdataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            restockdataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 37, 53);
            restockdataGridView.ColumnHeadersDefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            restockdataGridView.ColumnHeadersDefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold);

            dataGridViewCheckBoxColumn3.HeaderText = LanguageHelper.GetString("restockDataGridX") ?? "X";
            dataGridViewTextBoxColumn9.HeaderText = LanguageHelper.GetString("restockDataGridItemName") ?? "Item Name";
            dataGridViewTextBoxColumn10.HeaderText = LanguageHelper.GetString("restockDataGridGraphics") ?? "Graphics";
            dataGridViewTextBoxColumn11.HeaderText = LanguageHelper.GetString("restockDataGridColor") ?? "Color";
            dataGridViewTextBoxColumn12.HeaderText = LanguageHelper.GetString("restockDataGridLimit") ?? "Limit";

            var sepActions = new Panel
            {
                Location = new Point(10, 262),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            restockAddTargetButton.Text = LanguageHelper.GetString("restockAddItemButton") ?? "Add Item";
            restockAddTargetButton.Location = new Point(10, 268);
            restockAddTargetButton.Size = new Size(115, 28);
            restockAddTargetButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            restockItemsCard.Controls.AddRange(new Control[]
            {
                lblList, restockListSelect,
                restockAddListB, restockRemoveListB, restockCloneListB,
                sepToolbar,
                restockdataGridView,
                sepActions,
                restockAddTargetButton
            });

            // ─────────────────── Card: Config + Log (right) ────────────────────
            restockConfigCard = new RazorCard
            {
                Name = "restockConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("restockConfigCard") ?? "Settings"),
                Location = new Point(450, 10),
                Size = new Size(214, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Bags section
            var lblBags = new Label
            {
                Text = LanguageHelper.GetString("restockBagsLabel") ?? "Restock Bags",
                Location = new Point(10, 28),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            var lblSource = new Label
            {
                Text = LanguageHelper.GetString("restockSourceLabel") ?? "Source:",
                Location = new Point(10, 46),
                Size = new Size(56, 18),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            restockSourceLabel.Location = new Point(68, 46);
            restockSourceLabel.Size = new Size(129, 18);
            restockSourceLabel.ForeColor = RazorTheme.Colors.CurrentTextSecondary;
            restockSourceLabel.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Italic);
            restockSourceLabel.BackColor = Color.Transparent;

            restockSetSourceButton.Text = LanguageHelper.GetString("restockSetSourceButton") ?? "Set Source";
            restockSetSourceButton.Location = new Point(10, 66);
            restockSetSourceButton.Size = new Size(187, 26);

            var sep1 = new Panel
            {
                Location = new Point(10, 96),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            var lblDest = new Label
            {
                Text = LanguageHelper.GetString("restockDestinationLabel") ?? "Destination:",
                Location = new Point(10, 102),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            restockDestinationLabel.Location = new Point(10, 118);
            restockDestinationLabel.Size = new Size(187, 18);
            restockDestinationLabel.ForeColor = RazorTheme.Colors.CurrentTextSecondary;
            restockDestinationLabel.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Italic);
            restockDestinationLabel.BackColor = Color.Transparent;

            restockSetDestinationButton.Text = LanguageHelper.GetString("restockSetDestinationButton") ?? "Set Destination";
            restockSetDestinationButton.Location = new Point(10, 138);
            restockSetDestinationButton.Size = new Size(187, 26);

            var sep2 = new Panel
            {
                Location = new Point(10, 168),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Execute + Stop buttons
            restockExecuteButton.BackgroundImage = null;
            restockExecuteButton.Text = (LanguageHelper.GetString("restockExecuteButton") ?? "Execute");
            restockExecuteButton.FlatStyle = FlatStyle.Flat;
            restockExecuteButton.FlatAppearance.BorderSize = 0;
            restockExecuteButton.BackColor = RazorTheme.Colors.Success;
            restockExecuteButton.ForeColor = Color.White;
            restockExecuteButton.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            restockExecuteButton.Location = new Point(10, 176);
            restockExecuteButton.Size = new Size(87, 30);

            restockStopButton.BackgroundImage = null;
            restockStopButton.Text = (LanguageHelper.GetString("restockStopButton") ?? "Stop");
            restockStopButton.FlatStyle = FlatStyle.Flat;
            restockStopButton.FlatAppearance.BorderSize = 0;
            restockStopButton.BackColor = RazorTheme.Colors.Danger;
            restockStopButton.ForeColor = Color.White;
            restockStopButton.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            restockStopButton.Location = new Point(107, 176);
            restockStopButton.Size = new Size(80, 30);

            var sep3 = new Panel
            {
                Location = new Point(10, 210),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Delay row
            var lblDelay = new Label
            {
                Text = LanguageHelper.GetString("restockDragDelayLabel") ?? "Drag Item Delay (ms)",
                Location = new Point(10, 216),
                Size = new Size(125, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            restockDragDelay.Location = new Point(140, 214);
            restockDragDelay.Size = new Size(47, 22);
            restockDragDelay.BackColor = RazorTheme.Colors.CurrentCard;
            restockDragDelay.ForeColor = RazorTheme.Colors.CurrentText;
            restockDragDelay.Font = RazorTheme.Fonts.DisplayFont(9F);
            restockDragDelay.BorderStyle = BorderStyle.FixedSingle;

            var sep4 = new Panel
            {
                Location = new Point(10, 240),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            var lblLog = new Label
            {
                Text = LanguageHelper.GetString("restockLogLabel") ?? "Restock Log",
                Location = new Point(10, 246),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            restockLogBox.Location = new Point(10, 262);
            restockLogBox.Size = new Size(187, 48);
            restockLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            restockLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            restockLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            restockLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            restockLogBox.BorderStyle = BorderStyle.None;

            restockConfigCard.Controls.AddRange(new Control[]
            {
                lblBags,
                lblSource, restockSourceLabel, restockSetSourceButton,
                sep1,
                lblDest, restockDestinationLabel, restockSetDestinationButton,
                sep2,
                restockExecuteButton, restockStopButton,
                sep3,
                lblDelay, restockDragDelay,
                sep4,
                lblLog, restockLogBox
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            restock.Controls.Clear();
            restock.Controls.AddRange(new Control[] { restockItemsCard, restockConfigCard });
            restock.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }
    }
}
