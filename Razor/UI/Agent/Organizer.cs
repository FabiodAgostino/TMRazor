using RazorEnhanced;
using RazorEnhanced.UI;
using System;
using System.Windows.Forms;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        internal RazorAgentNumOnlyTextBox OrganizerDragDelay { get { return organizerDragDelay; } }
        internal Label OrganizerSourceLabel { get { return organizerSourceLabel; } }
        internal TextBox OrganizerDestinationLabel { get { return organizerDestination; } }
        internal ListBox OrganizerLogBox { get { return organizerLogBox; } }
        internal DataGridView OrganizerDataGridView { get { return organizerdataGridView; } }
        internal ComboBox OrganizerListSelect { get { return organizerListSelect; } }
        internal Button OrganizerExecute { get { return organizerExecuteButton; } }
        internal Button OrganizerStop { get { return organizerStopButton; } }

        private void organizerAddList_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 3;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(3).Show();
        }

        private void organizerCloneListB_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 12;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(12).Show();
        }

        private void organizerRemoveList_Click(object sender, EventArgs e)
        {
            if (organizerListSelect.Text != String.Empty)
            {
                DialogResult dialogResult = RazorEnhanced.UI.RE_MessageBox.Show(LanguageHelper.GetString("MsgDeleteOrganizerListTitle"),
                   LanguageHelper.GetString("MsgDeleteOrganizerListText") + organizerListSelect.Text,
                    ok: LanguageHelper.GetString("MsgOk"), no: LanguageHelper.GetString("MsgNo"), cancel: null, backColor: null);
                if (dialogResult == DialogResult.Yes)
                {
                    RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgOrganizerListRemoved") + organizerListSelect.Text);
                    RazorEnhanced.Organizer.OrganizerSource = 0;
                    RazorEnhanced.Organizer.OrganizerDestination = 0;
                    RazorEnhanced.Organizer.OrganizerDelay = 100;
                    RazorEnhanced.Organizer.RemoveList(organizerListSelect.Text);
                }
            }
        }

        private void organizerSetSource_Click(object sender, EventArgs e)
        {
            OrganizerSetSource();
        }

        internal void OrganizerSetSource()
        {
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgSelectSourceContainer"), false);

            if (organizerListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(OrganizerSourceContainerTarget_Callback));
            else
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }
        private bool AcceptibleOrganizerTarget(Assistant.Item organizerBag)
        {
            if (organizerBag.TypeID == 0x2259)
                return true;

            return organizerBag.Serial.IsItem && organizerBag.IsContainer;
        }

        private void OrganizerSourceContainerTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item organizerBag = Assistant.World.FindItem((Assistant.Serial)((uint)serial));
            if (organizerBag == null)
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidSourceContainer"), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgInvalidSourceContainer"));
                RazorEnhanced.Organizer.OrganizerSource = (int)World.Player.Backpack.Serial.Value;
                return;
            }

            if (organizerBag != null && AcceptibleOrganizerTarget(organizerBag))
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgSourceContainerSet") + organizerBag.ToString(), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgSourceContainerSet") + organizerBag.ToString());
                RazorEnhanced.Organizer.OrganizerSource = (int)organizerBag.Serial.Value;
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidSourceContainer"), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgInvalidSourceContainer"));
                RazorEnhanced.Organizer.OrganizerSource = (int)World.Player.Backpack.Serial.Value;
            }

            this.Invoke((MethodInvoker)delegate
            {
                RazorEnhanced.Settings.Organizer.ListUpdate(organizerListSelect.Text, RazorEnhanced.Organizer.OrganizerDelay, serial, RazorEnhanced.Organizer.OrganizerDestination, true);
                RazorEnhanced.Organizer.RefreshLists();
            });
        }

        private void organizerSetDestination_Click(object sender, EventArgs e)
        {
            OrganizerSetDestination();
        }

        internal void OrganizerSetDestination()
        {
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgSelectDestinationContainer"), false);

            if (organizerListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(OrganizerDestinationContainerTarget_Callback));
            else
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void OrganizerDestinationContainerTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item organizerBag = Assistant.World.FindItem((Assistant.Serial)((uint)serial));

            if (organizerBag == null)
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidDestinationContainer"), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgInvalidDestinationContainer"));
                RazorEnhanced.Organizer.OrganizerDestination = (int)World.Player.Backpack.Serial.Value;
                return;
            }

            if (organizerBag != null && AcceptibleOrganizerTarget(organizerBag))
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgDestinationContainerSet") + organizerBag.ToString(), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgDestinationContainerSet") + organizerBag.ToString());
                RazorEnhanced.Organizer.OrganizerDestination = (int)organizerBag.Serial.Value;
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidDestinationContainer"), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgInvalidDestinationContainer"));
                RazorEnhanced.Organizer.OrganizerDestination = (int)World.Player.Backpack.Serial.Value;
            }

            this.Invoke((MethodInvoker)delegate
            {
                RazorEnhanced.Settings.Organizer.ListUpdate(organizerListSelect.Text, RazorEnhanced.Organizer.OrganizerDelay, RazorEnhanced.Organizer.OrganizerSource, serial, true);
                RazorEnhanced.Organizer.RefreshLists();
            });
        }

        private void organizerListSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            Organizer.UpdateListParam(organizerListSelect.Text);

            if (organizerListSelect.Focused && organizerListSelect.Text != String.Empty)
            {
                Settings.Organizer.ListUpdate(organizerListSelect.Text, RazorEnhanced.Organizer.OrganizerDelay, RazorEnhanced.Organizer.OrganizerSource, RazorEnhanced.Organizer.OrganizerDestination, true);
                Organizer.AddLog(LanguageHelper.GetString("MsgOrganizerListChanged") + organizerListSelect.Text);
            }

            Organizer.InitGrid();
        }

        private void organizerAddTarget_Click(object sender, EventArgs e)
        {
            OrganizerAddItem();
        }

        internal void OrganizerAddItem()
        {
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgSelectOrganizerItem"), false);

            if (organizerListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(OrganizerItemTarget_Callback));
            else
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void OrganizerItemTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item organizerItem = Assistant.World.FindItem(serial);
            if (organizerItem != null && organizerItem.Serial.IsItem)
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgOrganizerItemAdded") + organizerItem.ToString(), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgOrganizerItemAdded") + organizerItem.ToString());
                this.Invoke((MethodInvoker)delegate { RazorEnhanced.Organizer.AddItemToList(organizerItem.Name, organizerItem.TypeID, organizerItem.Hue); });
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidTarget"), false);
                RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgInvalidTarget"));
            }
        }
        private void organizerDragDelay_Leave(object sender, EventArgs e)
        {
            if (organizerDragDelay.Text == String.Empty)
                organizerDragDelay.Text = "100";

            Organizer.OrganizerDelay = Convert.ToInt32(organizerDragDelay.Text);

            Settings.Organizer.ListUpdate(organizerListSelect.Text, RazorEnhanced.Organizer.OrganizerDelay, RazorEnhanced.Organizer.OrganizerSource, RazorEnhanced.Organizer.OrganizerDestination, true);
            Organizer.RefreshLists();
        }
        private void organizerDestination_Leave(object sender, EventArgs e)
        {
            if (organizerDestination.Text == String.Empty)
                organizerDestination.Text = "0";

            int newDest = 0;
            if (organizerDestination.Text.StartsWith("bank", StringComparison.CurrentCultureIgnoreCase))
            {
                if (Player.Bank == null)
                {
                    RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgMustOpenBankFirst"));
                    return;
                }
                else
                    newDest = Player.Bank.Serial;
            }
            else if (organizerDestination.Text.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!int.TryParse(organizerDestination.Text.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out newDest))
                {
                    RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgInvalidDestination"));
                    return;
                }
            }
            else
            {
                if (!int.TryParse(organizerDestination.Text.Substring(2), System.Globalization.NumberStyles.Number, null, out newDest))
                {
                    RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgInvalidDestination"));
                    return;
                }
            }

            Organizer.OrganizerDestination = newDest;

            Settings.Organizer.ListUpdate(organizerListSelect.Text, RazorEnhanced.Organizer.OrganizerDelay, RazorEnhanced.Organizer.OrganizerSource, RazorEnhanced.Organizer.OrganizerDestination, true);
            Organizer.RefreshLists();
        }

        private void organizerExecute_Click(object sender, EventArgs e)
        {
            OrganizerStartExec();
        }

        internal void OrganizerStartExec()
        {
            if (World.Player == null)  // offline
            {
                Organizer.AddLog(LanguageHelper.GetString("MsgNotLoggedIn"));
                return;
            }

            if (organizerListSelect.Text == String.Empty) // Nessuna lista
            {
                Organizer.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
                return;
            }

            Organizer.Start();
            Organizer.AddLog(LanguageHelper.GetString("MsgOrganizerEngineStart"));
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgOrganizerEngineStartMsg"), false);

            OrganizerStartWork();
        }

        private void organizerStop_Click(object sender, EventArgs e)
        {
            OrganizerStopExec();
        }

        internal void OrganizerStopExec()
        {
            Organizer.ForceStop();

            Organizer.AddLog(LanguageHelper.GetString("MsgOrganizerEngineForceStop"));
            if (showagentmessageCheckBox.Checked)
                Misc.SendMessage(LanguageHelper.GetString("MsgOrganizerEngineForceStopMsg"), false);
            OrganizerFinishWork();
        }

        private delegate void OrganizerStartWorkCallback();

        internal void OrganizerStartWork()
        {
            if (organizerStopButton.InvokeRequired ||
                organizerExecuteButton.InvokeRequired ||
                organizerListSelect.InvokeRequired ||
                organizerAddListB.InvokeRequired ||
                organizerRemoveListB.InvokeRequired ||
                organizerDragDelay.InvokeRequired)
            {
                OrganizerStartWorkCallback d = new(OrganizerStartWork);
                this.Invoke(d, null);
            }
            else
            {
                organizerStopButton.Enabled = true;
                organizerExecuteButton.Enabled = false;
                organizerListSelect.Enabled = false;
                organizerAddListB.Enabled = false;
                organizerRemoveListB.Enabled = false;
                organizerCloneListB.Enabled = false;
                organizerDragDelay.Enabled = false;
            }
        }

        private delegate void OrganizerFinishWorkCallback();

        internal void OrganizerFinishWork()
        {
            if (organizerStopButton.InvokeRequired ||
                organizerExecuteButton.InvokeRequired ||
                organizerListSelect.InvokeRequired ||
                organizerAddListB.InvokeRequired ||
                organizerRemoveListB.InvokeRequired ||
                organizerDragDelay.InvokeRequired)
            {
                OrganizerFinishWorkCallback d = new(OrganizerFinishWork);
                this.Invoke(d, null);
            }
            else
            {
                organizerStopButton.Enabled = false;
                organizerExecuteButton.Enabled = true;
                organizerListSelect.Enabled = true;
                organizerAddListB.Enabled = true;
                organizerRemoveListB.Enabled = true;
                organizerCloneListB.Enabled = true;
                organizerDragDelay.Enabled = true;
            }
        }

        private void organizerdataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = organizerdataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if (e.ColumnIndex == 3)
            {
                cell.Value = Utility.FormatDatagridColorCell(cell);
            }
            else if (e.ColumnIndex == 4)
            {
                cell.Value = Utility.FormatDatagridAmountCell(cell, true);
            }
            else if (e.ColumnIndex == 2)
            {
                cell.Value = Utility.FormatDatagridItemIDCell(cell);
            }
            RazorEnhanced.Organizer.CopyTable();
        }

        private void organizerdataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[0].Value = false;
            e.Row.Cells[1].Value = LanguageHelper.GetString("MsgNewItem");
            e.Row.Cells[2].Value = "0x0000";
            e.Row.Cells[3].Value = "0x0000";
            e.Row.Cells[4].Value = "1";
        }
    }
}
