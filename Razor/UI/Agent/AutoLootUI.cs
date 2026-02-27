using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard autolootItemsCard;
        private RazorCard autolootConfigCard;

        private void InitializeAutolootTab2()
        {
            // ─────────────────── Card: Item List (left) ────────────────────────
            autolootItemsCard = new RazorCard
            {
                Name = "autolootItemsCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.eautoloot.Text") ?? "Autoloot"),
                Location = new Point(10, 10),
                Size = new Size(430, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // List selector label
            var lblList = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label20.Text") ?? "List:",
                Location = new Point(10, 34),
                Size = new Size(32, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            autolootListSelect.Location = new Point(46, 30);
            autolootListSelect.Size = new Size(155, 22);
            autolootListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            autolootButtonAddList.Text = LanguageHelper.GetString("MainForm.autolootButtonAddList.Text") ?? "Add";
            autolootButtonAddList.Location = new Point(207, 30);
            autolootButtonAddList.Size = new Size(55, 22);

            autoLootButtonRemoveList.Text = LanguageHelper.GetString("MainForm.autoLootButtonRemoveList.Text") ?? "Remove";
            autoLootButtonRemoveList.Location = new Point(267, 30);
            autoLootButtonRemoveList.Size = new Size(62, 22);

            autoLootButtonListClone.Text = LanguageHelper.GetString("MainForm.autoLootButtonListClone.Text") ?? "Clone";
            autoLootButtonListClone.Location = new Point(334, 30);
            autoLootButtonListClone.Size = new Size(58, 22);
            if (autoLootButtonListClone is RazorButton rbClone)
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
            autolootdataGridView.Location = new Point(10, 63);
            autolootdataGridView.Size = new Size(398, 194);
            autolootdataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            autolootdataGridView.BackgroundColor = RazorTheme.Colors.CurrentCard;
            autolootdataGridView.GridColor = Color.FromArgb(50, 70, 90);
            autolootdataGridView.BorderStyle = BorderStyle.None;
            autolootdataGridView.EnableHeadersVisualStyles = false;
            autolootdataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            autolootdataGridView.DefaultCellStyle.BackColor = RazorTheme.Colors.CurrentCard;
            autolootdataGridView.DefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            autolootdataGridView.DefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(9F);
            autolootdataGridView.DefaultCellStyle.SelectionBackColor = RazorTheme.Colors.Primary;
            autolootdataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            autolootdataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 37, 53);
            autolootdataGridView.ColumnHeadersDefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            autolootdataGridView.ColumnHeadersDefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold);

            // Column Headers translations
            AutolootColumnX.HeaderText = LanguageHelper.GetString("MainForm.AutolootColumnX.HeaderText") ?? "X";
            AutolootColumnItemName.HeaderText = LanguageHelper.GetString("MainForm.AutolootColumnItemName.HeaderText") ?? "Item Name";
            AutolootColumnItemID.HeaderText = LanguageHelper.GetString("MainForm.AutolootColumnItemID.HeaderText") ?? "Graphics";
            AutolootColumnColor.HeaderText = LanguageHelper.GetString("MainForm.AutolootColumnColor.HeaderText") ?? "Color";
            AutolootColumnProps.HeaderText = LanguageHelper.GetString("MainForm.AutolootColumnProps.HeaderText") ?? "Props";

            // Separator above action buttons
            var sepActions = new Panel
            {
                Location = new Point(10, 262),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Action buttons pinned to card bottom
            autolootAddItemBTarget.Text = LanguageHelper.GetString("MainForm.autolootAddItemBTarget.Text") ?? "Add Item";
            autolootAddItemBTarget.Location = new Point(10, 268);
            autolootAddItemBTarget.Size = new Size(115, 28);
            autolootAddItemBTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            autolootItemPropsB.Text = LanguageHelper.GetString("MainForm.autolootItemPropsB.Text") ?? "Edit Props";
            autolootItemPropsB.Location = new Point(130, 268);
            autolootItemPropsB.Size = new Size(115, 28);
            autolootItemPropsB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            autolootItemsCard.Controls.AddRange(new Control[]
            {
                lblList, autolootListSelect,
                autolootButtonAddList, autoLootButtonRemoveList, autoLootButtonListClone,
                sepToolbar,
                autolootdataGridView,
                sepActions,
                autolootAddItemBTarget, autolootItemPropsB
            });

            // ─────────────────── Card: Config + Log (right) ────────────────────
            autolootConfigCard = new RazorCard
            {
                Name = "autolootConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("MainForm.autolootConfigCard.Text") ?? "Settings"),
                Location = new Point(450, 10),
                Size = new Size(214, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Toggles
            autoLootCheckBox.Text = LanguageHelper.GetString("MainForm.autoLootCheckBox.Text") ?? "Enable autoloot";
            autoLootCheckBox.Location = new Point(10, 30);
            autoLootCheckBox.Size = new Size(187, 22);
            autoLootCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            autoLootnoopenCheckBox.Text = LanguageHelper.GetString("MainForm.autoLootnoopenCheckBox.Text") ?? "No Open Corpse";
            autoLootnoopenCheckBox.Location = new Point(10, 57);
            autoLootnoopenCheckBox.Size = new Size(187, 22);
            autoLootnoopenCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            autolootautostartCheckBox.Text = LanguageHelper.GetString("MainForm.autolootautostartCheckBox.Text") ?? "Autostart OnLogin";
            autolootautostartCheckBox.Location = new Point(10, 84);
            autolootautostartCheckBox.Size = new Size(187, 22);
            autolootautostartCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            allowHiddenLooting.Text = LanguageHelper.GetString("MainForm.allowHiddenLooting.Text") ?? "Allow looting while hidden";
            allowHiddenLooting.Location = new Point(10, 111);
            allowHiddenLooting.Size = new Size(187, 22);
            allowHiddenLooting.Font = RazorTheme.Fonts.DisplayFont(9F);

            var sep1 = new Panel
            {
                Location = new Point(10, 137),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Loot Bag section
            var lblBag = new Label
            {
                Text = LanguageHelper.GetString("MainForm.groupBox14.Text") ?? "AutoLoot Bag",
                Location = new Point(10, 143),
                Size = new Size(80, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            autolootContainerLabel.Location = new Point(94, 141);
            autolootContainerLabel.Size = new Size(93, 18);
            autolootContainerLabel.ForeColor = RazorTheme.Colors.CurrentTextSecondary;
            autolootContainerLabel.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Italic);
            autolootContainerLabel.BackColor = Color.Transparent;

            autolootContainerButton.Text = LanguageHelper.GetString("MainForm.autolootContainerButton.Text") ?? "Set Bag";
            autolootContainerButton.Location = new Point(10, 161);
            autolootContainerButton.Size = new Size(187, 26);

            var sep2 = new Panel
            {
                Location = new Point(10, 191),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Delay field
            var lblDelay = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label21.Text") ?? "Delay (ms):",
                Location = new Point(10, 200),
                Size = new Size(90, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            autoLootTextBoxDelay.Location = new Point(137, 197);
            autoLootTextBoxDelay.Size = new Size(50, 22);
            autoLootTextBoxDelay.BackColor = RazorTheme.Colors.CurrentCard;
            autoLootTextBoxDelay.ForeColor = RazorTheme.Colors.CurrentText;
            autoLootTextBoxDelay.Font = RazorTheme.Fonts.DisplayFont(9F);
            autoLootTextBoxDelay.BorderStyle = BorderStyle.FixedSingle;

            // Max Range field
            var lblRange = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label60.Text") ?? "Max Range:",
                Location = new Point(10, 226),
                Size = new Size(90, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            autoLootTextBoxMaxRange.Location = new Point(137, 223);
            autoLootTextBoxMaxRange.Size = new Size(50, 22);
            autoLootTextBoxMaxRange.BackColor = RazorTheme.Colors.CurrentCard;
            autoLootTextBoxMaxRange.ForeColor = RazorTheme.Colors.CurrentText;
            autoLootTextBoxMaxRange.Font = RazorTheme.Fonts.DisplayFont(9F);
            autoLootTextBoxMaxRange.BorderStyle = BorderStyle.FixedSingle;

            var sep3 = new Panel
            {
                Location = new Point(10, 250),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Activity Log section
            var lblLog = new Label
            {
                Text = LanguageHelper.GetString("MainForm.groupBox13.Text") ?? "Autoloot Log",
                Location = new Point(10, 256),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            autolootLogBox.Location = new Point(10, 272);
            autolootLogBox.Size = new Size(187, 38);
            autolootLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            autolootLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            autolootLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            autolootLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            autolootLogBox.BorderStyle = BorderStyle.None;

            autolootConfigCard.Controls.AddRange(new Control[]
            {
                autoLootCheckBox, autoLootnoopenCheckBox, autolootautostartCheckBox, allowHiddenLooting,
                sep1,
                lblBag, autolootContainerLabel, autolootContainerButton,
                sep2,
                lblDelay, autoLootTextBoxDelay,
                lblRange, autoLootTextBoxMaxRange,
                sep3,
                lblLog, autolootLogBox
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            eautoloot.Controls.Clear();
            eautoloot.Controls.AddRange(new Control[] { autolootItemsCard, autolootConfigCard });
            eautoloot.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }
    }
}
