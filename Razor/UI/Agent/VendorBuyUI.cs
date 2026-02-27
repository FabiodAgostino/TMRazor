using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard vendorbuyItemsCard;
        private RazorCard vendorbuyConfigCard;

        private void InitializeVendorBuyTab2()
        {
            // ─────────────────── Card: Item List (left) ────────────────────────
            vendorbuyItemsCard = new RazorCard
            {
                Name = "vendorbuyItemsCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.vendorbuyItemsCard.Text") ?? "Vendor Buy"),
                Location = new Point(10, 10),
                Size = new Size(430, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // List selector label
            var lblList = new Label
            {
                Text = LanguageHelper.GetString("MainForm.label25.Text") ?? "List:",
                Location = new Point(10, 34),
                Size = new Size(32, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            buyListSelect.Location = new Point(46, 30);
            buyListSelect.Size = new Size(155, 22);
            buyListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            buyAddListButton.Text = LanguageHelper.GetString("MainForm.buyAddListButton.Text") ?? "Add";
            buyAddListButton.Location = new Point(207, 30);
            buyAddListButton.Size = new Size(55, 22);

            buyRemoveListButton.Text = LanguageHelper.GetString("MainForm.buyRemoveListButton.Text") ?? "Remove";
            buyRemoveListButton.Location = new Point(267, 30);
            buyRemoveListButton.Size = new Size(62, 22);

            buyCloneButton.Text = LanguageHelper.GetString("MainForm.buyCloneButton.Text") ?? "Clone";
            buyCloneButton.Location = new Point(334, 30);
            buyCloneButton.Size = new Size(58, 22);
            if (buyCloneButton is RazorButton rbClone)
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
            vendorbuydataGridView.Location = new Point(10, 63);
            vendorbuydataGridView.Size = new Size(398, 194);
            vendorbuydataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            vendorbuydataGridView.BackgroundColor = RazorTheme.Colors.CurrentCard;
            vendorbuydataGridView.GridColor = Color.FromArgb(50, 70, 90);
            vendorbuydataGridView.BorderStyle = BorderStyle.None;
            vendorbuydataGridView.EnableHeadersVisualStyles = false;
            vendorbuydataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            vendorbuydataGridView.DefaultCellStyle.BackColor = RazorTheme.Colors.CurrentCard;
            vendorbuydataGridView.DefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            vendorbuydataGridView.DefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(9F);
            vendorbuydataGridView.DefaultCellStyle.SelectionBackColor = RazorTheme.Colors.Primary;
            vendorbuydataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
            vendorbuydataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 37, 53);
            vendorbuydataGridView.ColumnHeadersDefaultCellStyle.ForeColor = RazorTheme.Colors.CurrentText;
            vendorbuydataGridView.ColumnHeadersDefaultCellStyle.Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold);

            // Column header translations
            dataGridViewCheckBoxColumn1.HeaderText = LanguageHelper.GetString("MainForm.vendorbuyColumnX.HeaderText") ?? "X";
            dataGridViewTextBoxColumn1.HeaderText = LanguageHelper.GetString("MainForm.vendorbuyColumnName.HeaderText") ?? "Item Name";
            dataGridViewTextBoxColumn2.HeaderText = LanguageHelper.GetString("MainForm.vendorbuyColumnGraphics.HeaderText") ?? "Graphics";
            dataGridViewTextBoxColumn3.HeaderText = LanguageHelper.GetString("MainForm.vendorbuyColumnAmount.HeaderText") ?? "Amount";
            dataGridViewTextBoxColumn4.HeaderText = LanguageHelper.GetString("MainForm.vendorbuyColumnColor.HeaderText") ?? "Color";

            // Separator above action button
            var sepActions = new Panel
            {
                Location = new Point(10, 262),
                Size = new Size(398, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Action button pinned to card bottom
            buyAddTargetB.Text = LanguageHelper.GetString("MainForm.buyAddTargetB.Text") ?? "Add Item";
            buyAddTargetB.Location = new Point(10, 268);
            buyAddTargetB.Size = new Size(115, 28);
            buyAddTargetB.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            vendorbuyItemsCard.Controls.AddRange(new Control[]
            {
                lblList, buyListSelect,
                buyAddListButton, buyRemoveListButton, buyCloneButton,
                sepToolbar,
                vendorbuydataGridView,
                sepActions,
                buyAddTargetB
            });

            // ─────────────────── Card: Config + Log (right) ────────────────────
            vendorbuyConfigCard = new RazorCard
            {
                Name = "vendorbuyConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("MainForm.vendorbuyConfigCard.Text") ?? "Settings"),
                Location = new Point(450, 10),
                Size = new Size(214, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            // Toggles
            buyEnableCheckBox.Text = LanguageHelper.GetString("MainForm.buyEnableCheckBox.Text") ?? "Enable Buy List";
            buyEnableCheckBox.Location = new Point(10, 30);
            buyEnableCheckBox.Size = new Size(187, 22);
            buyEnableCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            buyCompareNameCheckBox.Text = LanguageHelper.GetString("MainForm.buyCompareNameCheckBox.Text") ?? "Compare Item Name";
            buyCompareNameCheckBox.Location = new Point(10, 57);
            buyCompareNameCheckBox.Size = new Size(187, 22);
            buyCompareNameCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            buyToCompleteAmount.Text = LanguageHelper.GetString("MainForm.buyToCompleteAmount.Text") ?? "Restock";
            buyToCompleteAmount.Location = new Point(10, 84);
            buyToCompleteAmount.Size = new Size(187, 22);
            buyToCompleteAmount.Font = RazorTheme.Fonts.DisplayFont(9F);

            var sep1 = new Panel
            {
                Location = new Point(10, 110),
                Size = new Size(187, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // Activity Log section
            var lblLog = new Label
            {
                Text = LanguageHelper.GetString("MainForm.groupBox18.Text") ?? "Buy Log",
                Location = new Point(10, 116),
                Size = new Size(187, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            buyLogBox.Location = new Point(10, 132);
            buyLogBox.Size = new Size(187, 176);
            buyLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            buyLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            buyLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            buyLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            buyLogBox.BorderStyle = BorderStyle.None;

            vendorbuyConfigCard.Controls.AddRange(new Control[]
            {
                buyEnableCheckBox, buyCompareNameCheckBox, buyToCompleteAmount,
                sep1,
                lblLog, buyLogBox
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            VendorBuy.Controls.Clear();
            VendorBuy.Controls.AddRange(new Control[] { vendorbuyItemsCard, vendorbuyConfigCard });
            VendorBuy.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }
    }
}
