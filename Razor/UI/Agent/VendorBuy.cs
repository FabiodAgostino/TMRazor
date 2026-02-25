using RazorEnhanced;
using RazorEnhanced.UI;
using System;
using System.Windows.Forms;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        internal CheckBox BuyCheckBox { get { return buyEnableCheckBox; } }
        internal ListBox BuyLogBox { get { return buyLogBox; } }
        internal ComboBox BuyListSelect { get { return buyListSelect; } }
        internal CheckBox BuyCompareNameCheckBox { get { return buyCompareNameCheckBox; } }
        internal CheckBox BuyCompleteCheckBox { get { return buyToCompleteAmount; } }
        internal CheckBox BuyEnableCheckBox { get { return buyEnableCheckBox; } }

        internal DataGridView VendorBuyDataGridView { get { return vendorbuydataGridView; } }

        private void buyListSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (buyListSelect.Text != String.Empty)
            {
                BuyAgent.UpdateListParam(buyListSelect.Text);

                if (buyListSelect.Focused)
                {
                    Settings.BuyAgent.ListUpdate(buyListSelect.Text, RazorEnhanced.BuyAgent.CompareName, true, buyToCompleteAmount.Checked, buyEnableCheckBox.Checked);

                    BuyAgent.AddLog(LanguageHelper.GetString("MsgBuyAgentListChanged") + buyListSelect.Text);
                }
            }

            RazorEnhanced.BuyAgent.InitGrid();
        }

        private void buyAddList_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 4;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(4).Show();
        }

        private void buyCloneButton_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 13;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(13).Show();
        }
        private void buyRemoveList_Click(object sender, EventArgs e)
        {
            if (buyListSelect.Text != String.Empty)
            {
                var dialogResult = RazorEnhanced.UI.RE_MessageBox.Show(LanguageHelper.GetString("MsgDeleteVendorBuyListTitle"),
                    string.Format(LanguageHelper.GetString("MsgDeleteVendorBuyListText"), buyListSelect.Text),
                    ok: LanguageHelper.GetString("MsgYes"), no: LanguageHelper.GetString("MsgNo"), cancel: null, backColor: null);
                if (dialogResult == DialogResult.Yes)
                {
                    RazorEnhanced.BuyAgent.AddLog(LanguageHelper.GetString("MsgBuyAgentListRemoved") + buyListSelect.Text);
                    RazorEnhanced.BuyAgent.RemoveList(buyListSelect.Text);
                }
            }
        }

        private void buyAddTarget_Click(object sender, EventArgs e)
        {
            if (buyListSelect.Text != String.Empty)
            {
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(BuyAgentItemTarget_Callback));
            }
            else
                RazorEnhanced.BuyAgent.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void BuyAgentItemTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item buyItem = Assistant.World.FindItem(serial);
            if (buyItem != null && buyItem.Serial.IsItem)
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgBuyAgentItemAdded") + buyItem.ToString(), false);
                BuyAgent.AddLog(LanguageHelper.GetString("MsgBuyAgentItemAdded") + buyItem.ToString());
                this.Invoke((MethodInvoker)delegate { RazorEnhanced.BuyAgent.AddItemToList(buyItem.Name, buyItem.TypeID, 999, buyItem.Hue); });
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgInvalidTarget"), false);
                BuyAgent.AddLog(LanguageHelper.GetString("MsgInvalidTarget"));
            }
        }

        private void buyEnableCheckB_CheckedChanged(object sender, EventArgs e)
        {
            if (World.Player == null)  // offline
            {
                buyEnableCheckBox.Checked = false;
                BuyAgent.AddLog(LanguageHelper.GetString("MsgNotLoggedIn"));
                return;
            }

            if (buyListSelect.Text == String.Empty) // Nessuna lista
            {
                buyEnableCheckBox.Checked = false;
                BuyAgent.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
                return;
            }

            if (buyEnableCheckBox.Checked)
            {
                buyListSelect.Enabled = false;
                buyAddListButton.Enabled = false;
                buyRemoveListButton.Enabled = false;
                buyCloneButton.Enabled = false;
                BuyAgent.AddLog(LanguageHelper.GetString("MsgApplyItemList") + buyListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"));
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgApplyItemList") + buyListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"), false);
                BuyAgent.EnableBuyFilter();
            }
            else
            {
                buyListSelect.Enabled = true;
                buyAddListButton.Enabled = true;
                buyRemoveListButton.Enabled = true;
                buyCloneButton.Enabled = true;
                BuyAgent.AddLog(LanguageHelper.GetString("MsgRemoveItemList") + buyListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"));
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgRemoveItemList") + buyListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"), false);
            }
        }

        private void vendorbuydataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = vendorbuydataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (e.ColumnIndex == 4)
            {
                cell.Value = Utility.FormatDatagridColorCell(cell);
            }
            else if (e.ColumnIndex == 3)
            {
                cell.Value = Utility.FormatDatagridAmountCell(cell, false);
            }
            else if (e.ColumnIndex == 2)
            {
                cell.Value = Utility.FormatDatagridItemIDCell(cell);
            }
            RazorEnhanced.BuyAgent.CopyTable();
        }

        private void vendorbuydataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[0].Value = false;
            e.Row.Cells[1].Value = LanguageHelper.GetString("MsgNewItem");
            e.Row.Cells[2].Value = "0x0000";
            e.Row.Cells[3].Value = 999;
            e.Row.Cells[4].Value = "0x0000";
        }

        private void buyCompareNameCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (buyCompareNameCheckBox.Focused)
            {
                Settings.BuyAgent.ListUpdate(buyListSelect.Text, buyCompareNameCheckBox.Checked, true, buyToCompleteAmount.Checked, buyEnableCheckBox.Checked);
                BuyAgent.CompareName = buyCompareNameCheckBox.Checked;
            }
        }
        private void buyComplete_CheckedChanged(object sender, EventArgs e)
        {
            if (BuyCompleteCheckBox.Focused)
            {
                Settings.BuyAgent.ListUpdate(buyListSelect.Text, buyCompareNameCheckBox.Checked, true, buyToCompleteAmount.Checked, buyEnableCheckBox.Checked);
                BuyAgent.CompleteAmount = buyToCompleteAmount.Checked;
            }
        }
    }
}
