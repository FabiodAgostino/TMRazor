using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard vendorsellItemsCard;
        private RazorCard vendorsellConfigCard;

        private void InitializeVendorSellTab2()
        {
            // ─────────────────── Card: Item List (left) ────────────────────────
            vendorsellItemsCard = new RazorCard
            {
                Name = "vendorsellItemsCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.vendorsellItemsCard.Text") ?? "Vendite Vendor"),
                Location = new Point(10, 10),
                Size = new Size(430, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblList = new Label
            {
                Text = LanguageHelper.GetString("sellListLabel") ?? "List:",
                Location = new Point(10, 34),
                Size = new Size(32, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            sellListSelect.Location = new Point(46, 30);
            sellListSelect.Size = new Size(155, 22);
            sellListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            sellAddListButton.Text = LanguageHelper.GetString("sellAddListButton") ?? "Add";
            sellAddListButton.Location = new Point(207, 30);
            sellAddListButton.Size = new Size(55, 22);

            sellRemoveListButton.Text = LanguageHelper.GetString("sellRemoveListButton") ?? "Remove";
            sellRemoveListButton.Location = new Point(267, 30);
            sellRemoveListButton.Size = new Size(62, 22);

            sellCloneListButton.Text = LanguageHelper.GetString("sellCloneListButton") ?? "Clone";
            sellCloneListButton.Location = new Point(334, 30);
            sellCloneListButton.Size = new Size(58, 22);
            if (sellCloneListButton is RazorButton rbClone)
                rbClone.OverrideCustomColor = RazorTheme.Colors.Success;

            var sepToolbar = new Panel
            {
                Location = new Point(10, 56),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            vendorsellGridView.Location = new Point(10, 63);
            vendorsellGridView.Size = new Size(398, 194);
            vendorsellGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            vendorsellGridView.BackgroundColor = RazorTheme.Colors.CurrentCard;
            vendorsellGridView.GridColor = Color.FromArgb(50, 70, 90);
            vendorsellGridView.BorderStyle = BorderStyle.None;
            vendorsellGridView.EnableHeadersVisualStyles = false;
            vendorsellGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            vendorsellGridView.DefaultCellStyle.BackColor = RazorTheme.Colors.CurrentCard;
            vendorsellGridView.DefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            vendorsellGridView.DefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(9F);
            vendorsellGridView.DefaultCellStyle.SelectionBackColor = RazorTheme.Colors.Primary;
            vendorsellGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            vendorsellGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 37, 53);
            vendorsellGridView.ColumnHeadersDefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            vendorsellGridView.ColumnHeadersDefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold);

            VendorSellX.HeaderText = LanguageHelper.GetString("vendorSellDataGridX") ?? "X";
            VendorSellItemName.HeaderText = LanguageHelper.GetString("vendorSellDataGridItemName") ?? "Item Name";
            VendorSellGraphics.HeaderText = LanguageHelper.GetString("vendorSellDataGridGraphics") ?? "Graphics";
            VendorSellAmount.HeaderText = LanguageHelper.GetString("vendorSellDataGridAmount") ?? "Amount";
            VendorSellColor.HeaderText = LanguageHelper.GetString("vendorSellDataGridColor") ?? "Color";

            var sepActions = new Panel
            {
                Location = new Point(10, 262),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            sellAddTargerButton.Text = LanguageHelper.GetString("sellAddItemButton") ?? "Add Item";
            sellAddTargerButton.Location = new Point(10, 268);
            sellAddTargerButton.Size = new Size(115, 28);
            sellAddTargerButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            vendorsellItemsCard.Controls.AddRange(new Control[]
            {
                lblList, sellListSelect,
                sellAddListButton, sellRemoveListButton, sellCloneListButton,
                sepToolbar,
                vendorsellGridView,
                sepActions,
                sellAddTargerButton
            });

            // ─────────────────── Card: Config + Log (right) ────────────────────
            vendorsellConfigCard = new RazorCard
            {
                Name = "vendorsellConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("sellSettingsCard") ?? "Settings"),
                Location = new Point(450, 10),
                Size = new Size(214, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            sellEnableCheckBox.Text = LanguageHelper.GetString("sellEnableCheckBox") ?? "Enable Sell List";
            sellEnableCheckBox.Location = new Point(10, 30);
            sellEnableCheckBox.Size = new Size(187, 22);
            sellEnableCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            var sep1 = new Panel
            {
                Location = new Point(10, 56),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            var lblBag = new Label
            {
                Text = LanguageHelper.GetString("sellBagLabel") ?? "Sell Bag",
                Location = new Point(10, 62),
                Size = new Size(80, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            sellBagLabel.Location = new Point(94, 60);
            sellBagLabel.Size = new Size(93, 18);
            sellBagLabel.ForeColor = RazorTheme.Colors.CurrentTextSecondary;
            sellBagLabel.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Italic);
            sellBagLabel.BackColor = Color.Transparent;

            sellSetBagButton.Text = LanguageHelper.GetString("sellSetBagButton") ?? "Set Bag";
            sellSetBagButton.Location = new Point(10, 80);
            sellSetBagButton.Size = new Size(187, 26);

            var sep2 = new Panel
            {
                Location = new Point(10, 110),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            var lblLog = new Label
            {
                Text = LanguageHelper.GetString("sellLogLabel") ?? "Sell Log",
                Location = new Point(10, 116),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            sellLogBox.Location = new Point(10, 132);
            sellLogBox.Size = new Size(187, 176);
            sellLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            sellLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            sellLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            sellLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            sellLogBox.BorderStyle = BorderStyle.None;

            vendorsellConfigCard.Controls.AddRange(new Control[]
            {
                sellEnableCheckBox,
                sep1,
                lblBag, sellBagLabel, sellSetBagButton,
                sep2,
                lblLog, sellLogBox
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            VendorSell.Controls.Clear();
            VendorSell.Controls.AddRange(new Control[] { vendorsellItemsCard, vendorsellConfigCard });
            VendorSell.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }
    }
}
