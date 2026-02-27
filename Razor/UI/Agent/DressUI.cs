using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard dressItemsCard;
        private RazorCard dressConfigCard;

        private void InitializeDressTab2()
        {
            // ─────────────────── Card: Item List (left) ────────────────────────
            dressItemsCard = new RazorCard
            {
                Name = "dressItemsCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.dressItemsCard.Text") ?? "Dress / Arm"),
                Location = new Point(10, 10),
                Size = new Size(430, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // List selector label
            var lblList = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label28.Text") ?? "List:",
                Location = new Point(10, 34),
                Size = new Size(32, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            dressListSelect.Location = new Point(46, 30);
            dressListSelect.Size = new Size(155, 22);
            dressListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            dressAddListB.Text = LanguageHelper.GetString("MainForm.dressAddListB.Text") ?? "Add";
            dressAddListB.Location = new Point(207, 30);
            dressAddListB.Size = new Size(55, 22);

            dressRemoveListB.Text = LanguageHelper.GetString("MainForm.dressRemoveListB.Text") ?? "Remove";
            dressRemoveListB.Location = new Point(267, 30);
            dressRemoveListB.Size = new Size(62, 22);

            // Separator below list toolbar
            var sepToolbar = new Panel
            {
                Location = new Point(10, 56),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // ListView — apply dark theme
            dressListView.Location = new Point(10, 63);
            dressListView.Size = new Size(398, 159);
            dressListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dressListView.BackColor = RazorTheme.Colors.CurrentCard;
            dressListView.ForeColor = RazorTheme.Colors.CurrentText;
            dressListView.Font = RazorTheme.Fonts.DisplayFont(9F);
            dressListView.BorderStyle = BorderStyle.None;

            // Column header translations
            columnHeader24.Text = LanguageHelper.GetString("MainForm.columnHeader24.Text") ?? "X";
            columnHeader25.Text = LanguageHelper.GetString("MainForm.columnHeader25.Text") ?? "Layer";
            columnHeader26.Text = LanguageHelper.GetString("MainForm.columnHeader26.Text") ?? "Name";
            columnHeader27.Text = LanguageHelper.GetString("MainForm.columnHeader27.Text") ?? "Serial";

            // Separator above action buttons
            var sepActions = new Panel
            {
                Location = new Point(10, 227),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Action buttons row 1
            dressReadB.Text = LanguageHelper.GetString("MainForm.dressReadB.Text") ?? "Read Current";
            dressReadB.Location = new Point(10, 233);
            dressReadB.Size = new Size(122, 26);
            dressReadB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            dressAddTargetB.Text = LanguageHelper.GetString("MainForm.dressAddTargetB.Text") ?? "Add Target";
            dressAddTargetB.Location = new Point(141, 233);
            dressAddTargetB.Size = new Size(122, 26);
            dressAddTargetB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            dressAddManualB.Text = LanguageHelper.GetString("MainForm.dressAddManualB.Text") ?? "Add Clear Layer";
            dressAddManualB.Location = new Point(272, 233);
            dressAddManualB.Size = new Size(140, 26);
            dressAddManualB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // Action buttons row 2
            dressRemoveB.Text = LanguageHelper.GetString("MainForm.dressRemoveB.Text") ?? "Remove";
            dressRemoveB.Location = new Point(10, 264);
            dressRemoveB.Size = new Size(122, 26);
            dressRemoveB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            dressClearListB.Text = LanguageHelper.GetString("MainForm.dressClearListB.Text") ?? "Clear List";
            dressClearListB.Location = new Point(141, 264);
            dressClearListB.Size = new Size(122, 26);
            dressClearListB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            dressItemsCard.Controls.AddRange(new Control[]
            {
                lblList, dressListSelect,
                dressAddListB, dressRemoveListB,
                sepToolbar,
                dressListView,
                sepActions,
                dressReadB, dressAddTargetB, dressAddManualB,
                dressRemoveB, dressClearListB
            });

            // ─────────────────── Card: Config + Log (right) ────────────────────
            dressConfigCard = new RazorCard
            {
                Name = "dressConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("MainForm.dressConfigCard.Text") ?? "Settings"),
                Location = new Point(450, 10),
                Size = new Size(214, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Execute buttons: Dress / Undress / Stop
            dressExecuteButton.BackgroundImage = null;
            dressExecuteButton.Text = "\xE768  " + (LanguageHelper.GetString("MainForm.dressExecuteButton.Text") ?? "Dress");
            dressExecuteButton.FlatStyle = FlatStyle.Flat;
            dressExecuteButton.FlatAppearance.BorderSize = 0;
            dressExecuteButton.BackColor = RazorTheme.Colors.Success;
            dressExecuteButton.ForeColor = Color.White;
            dressExecuteButton.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            dressExecuteButton.Location = new Point(10, 30);
            dressExecuteButton.Size = new Size(57, 30);

            undressExecuteButton.BackgroundImage = null;
            undressExecuteButton.Text = "\xE8C4  " + (LanguageHelper.GetString("MainForm.undressExecuteButton.Text") ?? "Undress");
            undressExecuteButton.FlatStyle = FlatStyle.Flat;
            undressExecuteButton.FlatAppearance.BorderSize = 0;
            undressExecuteButton.BackColor = RazorTheme.Colors.Primary;
            undressExecuteButton.ForeColor = Color.White;
            undressExecuteButton.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            undressExecuteButton.Location = new Point(71, 30);
            undressExecuteButton.Size = new Size(67, 30);

            dressStopButton.BackgroundImage = null;
            dressStopButton.Text = "\xE71A  " + (LanguageHelper.GetString("MainForm.dressStopButton.Text") ?? "Stop");
            dressStopButton.FlatStyle = FlatStyle.Flat;
            dressStopButton.FlatAppearance.BorderSize = 0;
            dressStopButton.BackColor = RazorTheme.Colors.Danger;
            dressStopButton.ForeColor = Color.White;
            dressStopButton.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            dressStopButton.Location = new Point(142, 30);
            dressStopButton.Size = new Size(55, 30);

            var sep1 = new Panel
            {
                Location = new Point(10, 64),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Drag delay
            var lblDelay = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label29.Text") ?? "Drag Delay (ms):",
                Location = new Point(10, 72),
                Size = new Size(120, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            dressDragDelay.Location = new Point(137, 70);
            dressDragDelay.Size = new Size(50, 22);
            dressDragDelay.BackColor = RazorTheme.Colors.CurrentCard;
            dressDragDelay.ForeColor = RazorTheme.Colors.CurrentText;
            dressDragDelay.Font = RazorTheme.Fonts.DisplayFont(9F);
            dressDragDelay.BorderStyle = BorderStyle.FixedSingle;

            var sep2 = new Panel
            {
                Location = new Point(10, 96),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Toggles
            dressConflictCheckB.Text = LanguageHelper.GetString("MainForm.dressConflictCheckB.Text") ?? "Remove Conflict Item";
            dressConflictCheckB.Location = new Point(10, 102);
            dressConflictCheckB.Size = new Size(187, 22);
            dressConflictCheckB.Font = RazorTheme.Fonts.DisplayFont(9F);

            useUo3D.Text = LanguageHelper.GetString("MainForm.useUo3D.Text") ?? "Use UO3D Equip/Unequip";
            useUo3D.Location = new Point(10, 129);
            useUo3D.Size = new Size(187, 22);
            useUo3D.Font = RazorTheme.Fonts.DisplayFont(9F);

            var sep3 = new Panel
            {
                Location = new Point(10, 155),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Undress Bag section
            var lblBag = new Label
            {
                Text = LanguageHelper.GetString("MainForm.dressSetBagB.Text") ?? "Undress Bag",
                Location = new Point(10, 161),
                Size = new Size(90, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            dressBagLabel.Location = new Point(104, 159);
            dressBagLabel.Size = new Size(93, 18);
            dressBagLabel.ForeColor = RazorTheme.Colors.CurrentTextSecondary;
            dressBagLabel.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Italic);
            dressBagLabel.BackColor = Color.Transparent;

            dressSetBagB.Text = LanguageHelper.GetString("MainForm.dressSetBagB.Text") ?? "Undress Bag";
            dressSetBagB.Location = new Point(10, 177);
            dressSetBagB.Size = new Size(187, 26);

            var sep4 = new Panel
            {
                Location = new Point(10, 207),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Log section
            var lblLog = new Label
            {
                Text = LanguageHelper.GetString("MainForm.dressConfigCard.Log") ?? "Dress Log",
                Location = new Point(10, 213),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            dressLogBox.Location = new Point(10, 229);
            dressLogBox.Size = new Size(187, 81);
            dressLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dressLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            dressLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            dressLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            dressLogBox.BorderStyle = BorderStyle.None;

            dressConfigCard.Controls.AddRange(new Control[]
            {
                dressExecuteButton, undressExecuteButton, dressStopButton,
                sep1,
                lblDelay, dressDragDelay,
                sep2,
                dressConflictCheckB, useUo3D,
                sep3,
                lblBag, dressBagLabel, dressSetBagB,
                sep4,
                lblLog, dressLogBox
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            Dress.Controls.Clear();
            Dress.Controls.AddRange(new Control[] { dressItemsCard, dressConfigCard });
            Dress.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }
    }
}
