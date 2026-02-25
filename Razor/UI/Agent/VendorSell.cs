using RazorEnhanced;
using RazorEnhanced.UI;
using System;
using System.Windows.Forms;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        internal Label SellBagLabel { get { return sellBagLabel; } }
        internal CheckBox SellCheckBox { get { return sellEnableCheckBox; } }
        internal ListBox SellLogBox { get { return sellLogBox; } }
        internal ComboBox SellListSelect { get { return sellListSelect; } }
        internal DataGridView VendorSellGridView { get { return vendorsellGridView; } }

        private void sellListSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            SellAgent.UpdateListParam(sellListSelect.Text);

            if (sellListSelect.Focused && sellListSelect.Text != String.Empty)
            {
                Settings.SellAgent.ListUpdate(sellListSelect.Text, RazorEnhanced.SellAgent.SellBag, true, sellEnableCheckBox.Checked);
                SellAgent.AddLog(LanguageHelper.GetString("MsgSellAgentListChanged") + sellListSelect.Text);
            }

            SellAgent.InitGrid();
        }

        private void sellAddList_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 5;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(5).Show();
        }

        private void sellCloneListButton_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 14;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(14).Show();
        }

        private void sellRemoveList_Click(object sender, EventArgs e)
        {
            if (sellListSelect.Text != String.Empty)
            {
                var dialogResult = RazorEnhanced.UI.RE_MessageBox.Show(LanguageHelper.GetString("MsgDeleteVendorSellListTitle"),
                    string.Format(LanguageHelper.GetString("MsgDeleteVendorSellListText"), sellListSelect.Text),
                    ok: LanguageHelper.GetString("MsgYes"), no: LanguageHelper.GetString("MsgNo"), cancel: null, backColor: null);
                if (dialogResult == DialogResult.Yes)
                {
                    RazorEnhanced.SellAgent.AddLog(LanguageHelper.GetString("MsgSellAgentListRemoved") + sellListSelect.Text);
                    RazorEnhanced.SellAgent.RemoveList(sellListSelect.Text);
                }
            }
        }

        private void sellAddTarget_Click(object sender, EventArgs e)
        {
            if (sellListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(SellAgentItemTarget_Callback));
            else
                RazorEnhanced.SellAgent.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void SellAgentItemTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item sellItem = Assistant.World.FindItem(serial);
            if (sellItem != null && sellItem.Serial.IsItem)
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgSellAgentItemAdded") + sellItem.ToString(), false);
                RazorEnhanced.SellAgent.AddLog(LanguageHelper.GetString("MsgSellAgentItemAdded") + sellItem.ToString());
                this.Invoke((MethodInvoker)delegate { RazorEnhanced.SellAgent.AddItemToList(sellItem.Name, sellItem.TypeID, 999, sellItem.Hue); });
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidTarget"), false);
                RazorEnhanced.SellAgent.AddLog(LanguageHelper.GetString("MsgInvalidTarget"));
            }
        }

        private void sellEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (World.Player == null)  // offline
            {
                if (sellEnableCheckBox.Checked)
                {
                    sellEnableCheckBox.Checked = false;
                    SellAgent.AddLog(LanguageHelper.GetString("MsgNotLoggedIn"));
                }
                return;
            }

            if (sellListSelect.Text == String.Empty) // Nessuna lista
            {
                if (sellEnableCheckBox.Checked)
                {
                    sellEnableCheckBox.Checked = false;
                    SellAgent.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
                }
                return;
            }

            if (sellEnableCheckBox.Checked)
            {
                Assistant.Item bag = Assistant.World.FindItem(SellAgent.SellBag);

                if (bag != null && (!bag.IsLootableTarget))
                {
                    SellAgent.AddLog(LanguageHelper.GetString("MsgInvalidOrNotAccessibleContainer"));
                    if (showagentmessageCheckBox.Checked)
                        Misc.SendMessage(LanguageHelper.GetString("MsgInvalidOrNotAccessibleContainer"), false);
                    sellEnableCheckBox.Checked = false;
                }
                else
                {
                    sellListSelect.Enabled = false;
                    sellAddListButton.Enabled = false;
                    sellRemoveListButton.Enabled = false;
                    sellCloneListButton.Enabled = false;
                    SellAgent.AddLog(LanguageHelper.GetString("MsgApplyItemList") + sellListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"));
                    if (showagentmessageCheckBox.Checked)
                        Misc.SendMessage(LanguageHelper.GetString("MsgApplyItemList") + sellListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"), false);
                    SellAgent.EnableSellFilter();
                }
            }
            else
            {
                sellListSelect.Enabled = true;
                sellAddListButton.Enabled = true;
                sellRemoveListButton.Enabled = true;
                sellCloneListButton.Enabled = true;
                if (sellListSelect.Text != String.Empty)
                {
                    RazorEnhanced.SellAgent.AddLog(LanguageHelper.GetString("MsgRemoveItemList") + sellListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"));
                    if (showagentmessageCheckBox.Checked)
                        RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgRemoveItemList") + sellListSelect.SelectedItem.ToString() + LanguageHelper.GetString("MsgFilterOk"), false);
                }
            }
        }


        private void sellSetBag_Click(object sender, EventArgs e)
        {
            SellAgentSetBag();
        }

        internal void SellAgentSetBag()
        {
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgSelectSellBag"), false);

            if (sellListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(sellBagTarget_Callback));
            else
                RazorEnhanced.SellAgent.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void sellBagTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item sellBag = Assistant.World.FindItem(serial);

            if (sellBag == null)
                return;

            if (sellBag != null && sellBag.Serial.IsItem && sellBag.IsLootableTarget)
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgContainerSetTo") + sellBag.ToString(), false);
                SellAgent.AddLog(LanguageHelper.GetString("MsgContainerSetTo") + sellBag.ToString());
                SellAgent.SellBag = (int)sellBag.Serial.Value;
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgInvalidContainerSetBackpack"), false);
                SellAgent.AddLog(LanguageHelper.GetString("MsgInvalidContainerSetBackpack"));
                SellAgent.SellBag = (int)World.Player.Backpack.Serial.Value;
            }

            this.Invoke((MethodInvoker)delegate
            {
                RazorEnhanced.Settings.SellAgent.ListUpdate(sellListSelect.Text, serial, true, sellEnableCheckBox.Checked);
                RazorEnhanced.SellAgent.RefreshLists();
            });
        }

        private void vendorsellGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = vendorsellGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

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
            RazorEnhanced.SellAgent.CopyTable();
        }

        private void vendorsellGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[0].Value = false;
            e.Row.Cells[1].Value = LanguageHelper.GetString("MsgNewItem");
            e.Row.Cells[2].Value = "0x0000";
            e.Row.Cells[3].Value = 9999;
            e.Row.Cells[4].Value = "0x0000";
        }
    }
}
