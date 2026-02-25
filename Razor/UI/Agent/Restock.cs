using RazorEnhanced;
using RazorEnhanced.UI;
using System;
using System.Windows.Forms;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        internal RazorAgentNumOnlyTextBox RestockDragDelay { get { return restockDragDelay; } }
        internal Label RestockSourceLabel { get { return restockSourceLabel; } }
        internal Label RestockDestinationLabel { get { return restockDestinationLabel; } }
        internal ListBox RestockLogBox { get { return restockLogBox; } }
        internal DataGridView RestockDataGridView { get { return restockdataGridView; } }
        internal ComboBox RestockListSelect { get { return restockListSelect; } }
        internal Button RestockExecute { get { return restockExecuteButton; } }
        internal Button RestockStop { get { return restockStopButton; } }

        private void restockListSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            Restock.UpdateListParam(restockListSelect.Text);

            if (restockListSelect.Focused && restockListSelect.Text != String.Empty)
            {
                RazorEnhanced.Restock.AddLog(LanguageHelper.GetString("MsgRestockListChanged") + restockListSelect.Text);
                Settings.Restock.ListUpdate(restockListSelect.Text, Restock.RestockDelay, Restock.RestockSource, Restock.RestockDestination, true);
            }

            RazorEnhanced.Restock.InitGrid();
        }

        private void restockAddListB_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 8;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(8).Show();
        }

        private void restockCloneListB_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 17;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(17).Show();
        }

        private void restockRemoveListB_Click(object sender, EventArgs e)
        {
            if (restockListSelect.Text != String.Empty)
            {
                var dialogResult = RazorEnhanced.UI.RE_MessageBox.Show(LanguageHelper.GetString("MsgDeleteRestockListTitle"),
                    string.Format(LanguageHelper.GetString("MsgDeleteRestockListText"), restockListSelect.Text),
                    ok: LanguageHelper.GetString("MsgYes"), no: LanguageHelper.GetString("MsgNo"), cancel: null, backColor: null);
                if (dialogResult == DialogResult.Yes)
                {
                    Restock.AddLog(LanguageHelper.GetString("MsgRestockListRemoved") + restockListSelect.Text);
                    Restock.RestockSource = 0;
                    Restock.RestockDestination = 0;
                    Restock.RestockDelay = 100;
                    Restock.RemoveList(restockListSelect.Text);
                }
            }
        }

        private void restockSetSourceButton_Click(object sender, EventArgs e)
        {
            RestockSetSource();
        }

        internal void RestockSetSource()
        {
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgSelectRestockSourceBag"), false);

            if (restockListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(RestockSourceContainerTarget_Callback));
            else
                Restock.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void RestockSourceContainerTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item restockBag = Assistant.World.FindItem((Assistant.Serial)((uint)serial));
            if (restockBag == null)
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgInvalidSourceContainer"), false);
                Restock.AddLog(LanguageHelper.GetString("MsgInvalidSourceContainer"));
                Restock.RestockSource = (int)World.Player.Backpack.Serial.Value;
                return;
            }

            if (restockBag != null && restockBag.Serial.IsItem && restockBag.IsContainer)
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgSourceContainerSet") + restockBag.ToString(), false);
                Restock.AddLog(LanguageHelper.GetString("MsgSourceContainerSet") + restockBag.ToString());
                Restock.RestockSource = (int)restockBag.Serial.Value;
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgInvalidSourceContainer"), false);
                Restock.AddLog(LanguageHelper.GetString("MsgInvalidSourceContainer"));
                Restock.RestockSource = (int)World.Player.Backpack.Serial.Value;
            }

            this.Invoke((MethodInvoker)delegate
            {
                Settings.Restock.ListUpdate(restockListSelect.Text, Restock.RestockDelay, serial, Restock.RestockDestination, true);
                Restock.RefreshLists();
            });
        }

        private void restockSetDestinationButton_Click(object sender, EventArgs e)
        {
            RestockSetDestination();
        }

        internal void RestockSetDestination()
        {
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgSelectRestockDestinationBag"), false);

            if (restockListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(RestockDestinationContainerTarget_Callback));
            else
                Restock.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void RestockDestinationContainerTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item restockBag = Assistant.World.FindItem((Assistant.Serial)((uint)serial));
            if (restockBag == null)
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgInvalidDestinationContainer"), false);
                Restock.AddLog(LanguageHelper.GetString("MsgInvalidDestinationContainer"));
                Restock.RestockDestination = (int)World.Player.Backpack.Serial.Value;
                return;
            }

            if (restockBag != null && restockBag.Serial.IsItem && restockBag.IsContainer)
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgDestinationContainerSet") + restockBag.ToString(), false);
                Restock.AddLog(LanguageHelper.GetString("MsgDestinationContainerSet") + restockBag.ToString());
                Restock.RestockDestination = (int)restockBag.Serial.Value;
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    Misc.SendMessage(LanguageHelper.GetString("MsgInvalidDestinationContainer"), false);
                Restock.AddLog(LanguageHelper.GetString("MsgInvalidDestinationContainer"));
                Restock.RestockDestination = (int)World.Player.Backpack.Serial.Value;
            }

            this.Invoke((MethodInvoker)delegate
            {
                Settings.Restock.ListUpdate(restockListSelect.Text, Restock.RestockDelay, Restock.RestockSource, serial, true);
                Restock.RefreshLists();
            });
        }

        private void restockDragDelay_Leave(object sender, EventArgs e)
        {
            if (restockDragDelay.Text == String.Empty)
                restockDragDelay.Text = "100";

            Restock.RestockDelay = Convert.ToInt32(restockDragDelay.Text);

            Settings.Restock.ListUpdate(restockListSelect.Text, Restock.RestockDelay, Restock.RestockSource, Restock.RestockDestination, true);
            Restock.RefreshLists();
        }

        private void restockExecuteButton_Click(object sender, EventArgs e)
        {
            RestockStartExec();
        }

        internal void RestockStartExec()
        {
            if (World.Player == null)  // offline
            {
                Restock.AddLog(LanguageHelper.GetString("MsgNotLoggedIn"));
                return;
            }

            if (restockListSelect.Text == String.Empty) // Nessuna lista
            {
                Restock.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
                return;
            }

            Restock.Start();
            Restock.AddLog(LanguageHelper.GetString("MsgRestockEngineStart"));
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgRestockEngineStartMsg"), false);
            RestockStartWork();
        }

        private void restockStopButton_Click(object sender, EventArgs e)
        {
            RestockStopExec();
        }

        internal void RestockStopExec()
        {
            RazorEnhanced.Restock.ForceStop();

            RazorEnhanced.Restock.AddLog(LanguageHelper.GetString("MsgRestockEngineForceStop"));
            if (showagentmessageCheckBox.Checked)
                RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgRestockEngineForceStopMsg"), false);
            RestockFinishWork();
        }

        private delegate void RestockFinishWorkCallback();

        internal void RestockFinishWork()
        {
            if (restockStopButton.InvokeRequired ||
                restockExecuteButton.InvokeRequired ||
                restockListSelect.InvokeRequired ||
                restockAddListB.InvokeRequired ||
                restockRemoveListB.InvokeRequired ||
                restockDragDelay.InvokeRequired)
            {
                RestockFinishWorkCallback d = new(RestockFinishWork);
                this.Invoke(d, null);
            }
            else
            {
                restockStopButton.Enabled = false;
                restockExecuteButton.Enabled = true;
                restockListSelect.Enabled = true;
                restockAddListB.Enabled = true;
                restockRemoveListB.Enabled = true;
                restockDragDelay.Enabled = true;
            }
        }

        private delegate void RestockStartWorkCallback();

        internal void RestockStartWork()
        {
            if (restockStopButton.InvokeRequired ||
                restockExecuteButton.InvokeRequired ||
                restockListSelect.InvokeRequired ||
                restockAddListB.InvokeRequired ||
                restockRemoveListB.InvokeRequired ||
                restockDragDelay.InvokeRequired)
            {
                RestockStartWorkCallback d = new(RestockStartWork);
                this.Invoke(d, null);
            }
            else
            {
                restockStopButton.Enabled = true;
                restockExecuteButton.Enabled = false;
                restockListSelect.Enabled = false;
                restockAddListB.Enabled = false;
                restockRemoveListB.Enabled = false;
                restockDragDelay.Enabled = false;
            }
        }

        private void restockAddTargetButton_Click(object sender, EventArgs e)
        {
            RestockAddItem();
        }

        internal void RestockAddItem()
        {
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgSelectRestockItem"), false);

            if (restockListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(RestockItemTarget_Callback));
            else
                RazorEnhanced.Restock.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void RestockItemTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item restockItem = Assistant.World.FindItem(serial);
            if (restockItem != null && restockItem.Serial.IsItem)
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgRestockItemAdded") + restockItem.ToString(), false);
                RazorEnhanced.Restock.AddLog(LanguageHelper.GetString("MsgRestockItemAdded") + restockItem.ToString());
                this.Invoke((MethodInvoker)delegate { RazorEnhanced.Restock.AddItemToList(restockItem.Name, restockItem.TypeID, restockItem.Hue); });
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidTarget"), false);
                RazorEnhanced.Restock.AddLog(LanguageHelper.GetString("MsgInvalidTarget"));
            }
        }

        private void restockdataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = restockdataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (e.ColumnIndex == 3)
            {
                cell.Value = Utility.FormatDatagridColorCell(cell);
            }
            else if (e.ColumnIndex == 4)
            {
                cell.Value = Utility.FormatDatagridAmountCell(cell, false);
            }
            else if (e.ColumnIndex == 2)
            {
                cell.Value = Utility.FormatDatagridItemIDCell(cell);
            }
            RazorEnhanced.Restock.CopyTable();
        }

        private void restockdataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[0].Value = false;
            e.Row.Cells[1].Value = LanguageHelper.GetString("MsgNewItem");
            e.Row.Cells[2].Value = "0x0000";
            e.Row.Cells[3].Value = "0x0000";
            e.Row.Cells[4].Value = "1";
        }
    }
}
