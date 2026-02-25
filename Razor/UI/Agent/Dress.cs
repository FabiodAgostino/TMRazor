using RazorEnhanced;
using RazorEnhanced.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        internal CheckBox DressCheckBox { get { return dressConflictCheckB; } }
        internal CheckBox DressUseUo3D { get { return useUo3D; } }
        internal ListView DressListView { get { return dressListView; } }
        internal ListBox DressLogBox { get { return dressLogBox; } }
        internal RazorAgentNumOnlyTextBox DressDragDelay { get { return dressDragDelay; } }
        internal ComboBox DressListSelect { get { return dressListSelect; } }
        internal Label DressBagLabel { get { return dressBagLabel; } }

        internal Button DressExecuteButton { get { return dressExecuteButton; } }
        internal Button UnDressExecuteButton { get { return undressExecuteButton; } }
        internal Button DressStopButton { get { return dressStopButton; } }

        private void dressListSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            RazorEnhanced.Dress.UpdateListParam(dressListSelect.Text);

            if (dressListSelect.Focused && dressListSelect.Text != String.Empty)
            {
                Settings.Dress.ListUpdate(dressListSelect.Text, RazorEnhanced.Dress.DressDelay, RazorEnhanced.Dress.DressBag, RazorEnhanced.Dress.DressConflict, RazorEnhanced.Dress.DressUseUO3D, true);
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgDressListChanged") + dressListSelect.Text);
            }

            RazorEnhanced.Dress.InitGrid();
        }

        private void dressAddListB_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedAgentAddList af)
                {
                    af.AgentID = 6;
                    af.Focus();
                    return;
                }
            }
            new EnhancedAgentAddList(6).Show();
        }

        private void dressRemoveListB_Click(object sender, EventArgs e)
        {
            if (dressListSelect.Text != String.Empty)
            {
                var dialogResult = RazorEnhanced.UI.RE_MessageBox.Show(LanguageHelper.GetString("MsgDeleteDressListTitle"),
                    string.Format(LanguageHelper.GetString("MsgDeleteDressListText"), dressListSelect.Text),
                    ok: LanguageHelper.GetString("MsgYes"), no: LanguageHelper.GetString("MsgNo"), cancel: null, backColor: null);
                if (dialogResult == DialogResult.Yes)
                {
                    RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgDressListRemoved") + dressListSelect.Text);
                    RazorEnhanced.Dress.DressBag = 0;
                    RazorEnhanced.Dress.DressDelay = 100;
                    RazorEnhanced.Dress.DressConflict = false;
                    RazorEnhanced.Dress.DressUseUO3D = false;
                    RazorEnhanced.Dress.RemoveList(dressListSelect.Text);
                    HotKey.Init();
                }
            }
        }

        private void dressDragDelay_Leave(object sender, EventArgs e)
        {
            if (dressDragDelay.Text == String.Empty)
                dressDragDelay.Text = "100";

            RazorEnhanced.Dress.DressDelay = Convert.ToInt32(dressDragDelay.Text);

            RazorEnhanced.Settings.Dress.ListUpdate(dressListSelect.Text, RazorEnhanced.Dress.DressDelay, RazorEnhanced.Dress.DressBag, RazorEnhanced.Dress.DressConflict, RazorEnhanced.Dress.DressUseUO3D, true);
            RazorEnhanced.Dress.RefreshLists();
        }

        private void dressConflictCheckB_CheckedChanged(object sender, EventArgs e)
        {
            if (dressConflictCheckB.Focused)
            {
                RazorEnhanced.Dress.DressConflict = dressConflictCheckB.Checked;
                RazorEnhanced.Settings.Dress.ListUpdate(dressListSelect.Text, RazorEnhanced.Dress.DressDelay, RazorEnhanced.Dress.DressBag, RazorEnhanced.Dress.DressConflict, RazorEnhanced.Dress.DressUseUO3D, true);
                RazorEnhanced.Dress.RefreshLists();
            }
        }
        private void dressUseUO3d_CheckedChanged(object sender, EventArgs e)
        {
            if (useUo3D.Focused)
            {
                RazorEnhanced.Dress.DressUseUO3D = useUo3D.Checked;
                RazorEnhanced.Settings.Dress.ListUpdate(dressListSelect.Text, RazorEnhanced.Dress.DressDelay, RazorEnhanced.Dress.DressBag, RazorEnhanced.Dress.DressConflict, RazorEnhanced.Dress.DressUseUO3D, true);
                RazorEnhanced.Dress.RefreshLists();
            }
        }

        private void dressReadB_Click(object sender, EventArgs e)
        {
            if (dressListSelect.Text != String.Empty)
                RazorEnhanced.Dress.ReadPlayerDress();
            else
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void dressSetBagB_Click(object sender, EventArgs e)
        {
            if (dressListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(DressItemContainerTarget_Callback));
            else
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void DressItemContainerTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item dressBag = Assistant.World.FindItem(serial);

            if (dressBag == null)
                return;

            if (dressBag != null && dressBag.Serial.IsItem && dressBag.IsContainer)
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgUndressContainerSet") + dressBag.ToString(), false);
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgUndressContainerSet") + dressBag.ToString());
                RazorEnhanced.Dress.DressBag = (int)dressBag.Serial.Value;
            }
            else
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidUndressContainer"), false);
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgInvalidUndressContainer"));
                RazorEnhanced.Dress.DressBag = (int)World.Player.Backpack.Serial.Value;
            }

            this.Invoke((MethodInvoker)delegate
            {
                RazorEnhanced.Settings.Dress.ListUpdate(dressListSelect.Text, RazorEnhanced.Dress.DressDelay, RazorEnhanced.Dress.DressBag, RazorEnhanced.Dress.DressConflict, RazorEnhanced.Dress.DressUseUO3D, true);
                RazorEnhanced.Dress.RefreshLists();
            });
        }

        private void dressRemoveB_Click(object sender, EventArgs e)
        {
            if (dressListSelect.Text != String.Empty)
            {
                if (dressListView.SelectedItems.Count == 1)
                {
                    int index = dressListView.SelectedItems[0].Index;
                    string selection = dressListSelect.Text;

                    if (RazorEnhanced.Settings.Dress.ListExists(selection))
                    {
                        List<Dress.DressItemNew> items = Settings.Dress.ItemsRead(selection);
                        if (index <= items.Count - 1)
                        {
                            RazorEnhanced.Settings.Dress.ItemDelete(selection, items[index]);
                            RazorEnhanced.Dress.InitGrid();
                        }
                    }
                }
            }
            else
                RazorEnhanced.AutoLoot.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void dressClearListB_Click(object sender, EventArgs e)
        {
            if (dressListSelect.Text != String.Empty)
            {
                string selection = dressListSelect.Text;
                RazorEnhanced.Settings.Dress.ClearList(selection);
                RazorEnhanced.Dress.InitGrid();
            }
            else
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void dresslistView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (dressListView.FocusedItem != null)
            {
                ListViewItem item = e.Item;
                RazorEnhanced.Dress.UpdateSelectedItems(item.Index);
            }
        }

        private void dressAddTargetB_Click(object sender, EventArgs e)
        {
            if (dressListSelect.Text != String.Empty)
                Targeting.OneTimeTarget(new Targeting.TargetResponseCallback(DressItemTarget_Callback));
            else
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void DressItemTarget_Callback(bool loc, Assistant.Serial serial, Assistant.Point3D pt, ushort itemid)
        {
            Assistant.Item dressItem = Assistant.World.FindItem(serial);
            if (dressItem != null && dressItem.Serial.IsItem)
                this.Invoke((MethodInvoker)delegate { RazorEnhanced.Dress.AddItemByTarger(dressItem); });
            else
            {
                if (showagentmessageCheckBox.Checked)
                    RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgInvalidTarget"), false);
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgInvalidTarget"));
            }
        }

        private void dressAddManualB_Click(object sender, EventArgs e)
        {
            if (dressListSelect.Text != String.Empty)
            {
                EnhancedDressAddUndressLayer ManualAddLayer = new()
                {
                    TopMost = true
                };
                ManualAddLayer.Show();
            }
            else
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
        }

        private void razorButton10_Click(object sender, EventArgs e)
        {
            UndressStart();
        }

        internal void UndressStart()
        {
            if (World.Player == null) // non loggato
            {
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgNotLoggedIn"));
                UndressFinishWork();
                return;
            }

            if (dressListSelect.Text == String.Empty)
            {
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
                UndressFinishWork();
                return;
            }

            UndressStartWork();
            RazorEnhanced.Dress.UndressStart();
            RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgUndressEngineStart"));

            if (showagentmessageCheckBox.Checked)
                RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgUndressEngineStartMsg"), false);
        }

        private delegate void UndressFinishWorkCallback();

        internal void UndressFinishWork()
        {
            if (dressConflictCheckB.InvokeRequired ||
                useUo3D.InvokeRequired ||
                dressExecuteButton.InvokeRequired ||
                undressExecuteButton.InvokeRequired ||
                dressAddListB.InvokeRequired ||
                dressRemoveListB.InvokeRequired ||
                dressStopButton.InvokeRequired ||
                organizerDragDelay.InvokeRequired)
            {
                UndressFinishWorkCallback d = new(UndressFinishWork);
                this.Invoke(d, null);
            }
            else
            {
                dressStopButton.Enabled = false;
                dressConflictCheckB.Enabled = true;
                useUo3D.Enabled = true;
                dressExecuteButton.Enabled = true;
                undressExecuteButton.Enabled = true;
                dressAddListB.Enabled = true;
                dressRemoveListB.Enabled = true;
                dressDragDelay.Enabled = true;
            }
        }

        private delegate void UndressStartWorkCallback();

        internal void UndressStartWork()
        {
            if (dressConflictCheckB.InvokeRequired ||
                useUo3D.InvokeRequired ||
                dressExecuteButton.InvokeRequired ||
                undressExecuteButton.InvokeRequired ||
                dressAddListB.InvokeRequired ||
                dressRemoveListB.InvokeRequired ||
                dressStopButton.InvokeRequired ||
                organizerDragDelay.InvokeRequired)
            {
                UndressStartWorkCallback d = new(UndressStartWork);
                this.Invoke(d, null);
            }
            else
            {
                dressStopButton.Enabled = true;
                dressConflictCheckB.Enabled = false;
                useUo3D.Enabled = false;
                dressExecuteButton.Enabled = false;
                undressExecuteButton.Enabled = false;
                dressAddListB.Enabled = false;
                dressRemoveListB.Enabled = false;
                dressDragDelay.Enabled = false;
            }
        }

        private void dressExecuteButton_Click(object sender, EventArgs e)
        {
            DressStart();
        }

        internal void DressStart()
        {
            if (World.Player == null) // non loggato
            {
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgNotLoggedIn"));
                UndressFinishWork();
                return;
            }

            if (dressListSelect.Text == String.Empty)
            {
                RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgItemListNotSelected"));
                UndressFinishWork();
                return;
            }

            UndressStartWork();
            RazorEnhanced.Dress.DressStart();
            RazorEnhanced.Organizer.AddLog(LanguageHelper.GetString("MsgDressEngineStart"));

            if (showagentmessageCheckBox.Checked)
                RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgDressEngineStartMsg"), false);
        }

        private void dressStopButton_Click(object sender, EventArgs e)
        {
            DressStop();
        }

        internal void DressStop()
        {
            RazorEnhanced.Dress.ForceStop();

            RazorEnhanced.Dress.AddLog(LanguageHelper.GetString("MsgDressEngineForceStop"));
            if (showagentmessageCheckBox.Checked)
                RazorEnhanced.Misc.SendMessage(LanguageHelper.GetString("MsgDressEngineForceStopMsg"), false);
            UndressFinishWork();
        }
    }
}
