using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard friendsListCard;
        private RazorCard friendsConfigCard;

        private void InitializeFriendsTab2()
        {
            // ─────────────────── Card: Lists (left) ────────────────────────────
            friendsListCard = new RazorCard
            {
                Name = "friendsListCard",
                Text = "\xE716  " + (LanguageHelper.GetString("MainForm.friends.Text") ?? "Amici"),
                Location = new Point(10, 10),
                Size = new Size(266, 330),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            // Theme the SplitContainer and inner ListViews
            splitContainer1.BackColor = RazorTheme.Colors.CurrentCard;
            splitContainer1.Panel1.BackColor = RazorTheme.Colors.CurrentCard;
            splitContainer1.Panel2.BackColor = RazorTheme.Colors.CurrentCard;
            splitContainer1.Location = new Point(0, 22);
            splitContainer1.Size = new Size(250, 278);
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            friendlistView.BackColor = RazorTheme.Colors.CurrentCard;
            friendlistView.ForeColor = RazorTheme.Colors.CurrentText;
            friendlistView.Font = RazorTheme.Fonts.DisplayFont(9F);
            friendlistView.BorderStyle = BorderStyle.None;
            friendlistView.Dock = DockStyle.Fill;

            friendguildListView.BackColor = RazorTheme.Colors.CurrentCard;
            friendguildListView.ForeColor = RazorTheme.Colors.CurrentText;
            friendguildListView.Font = RazorTheme.Fonts.DisplayFont(9F);
            friendguildListView.BorderStyle = BorderStyle.None;
            friendguildListView.Dock = DockStyle.Fill;

            friendsListCard.Controls.Add(splitContainer1);

            // ─────────────────── Card: Config (right) ──────────────────────────
            friendsConfigCard = new RazorCard
            {
                Name = "friendsConfigCard",
                Text = "\xE712  " + (LanguageHelper.GetString("MainForm.friendsConfigCard.Text") ?? "Settings"),
                Location = new Point(284, 10),
                Size = new Size(374, 330),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // ── List selector row ──
            var lblList = new Label
            {
                Text = LanguageHelper.GetString("MainForm.labelfriend.Text") ?? "Friend List:",
                Location = new Point(10, 34),
                Size = new Size(60, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            friendListSelect.Location = new Point(74, 30);
            friendListSelect.Size = new Size(140, 22);
            friendListSelect.Font = RazorTheme.Fonts.DisplayFont(9F);

            friendButtonAddList.Text = LanguageHelper.GetString("MainForm.friendButtonAddList.Text") ?? "Add";
            friendButtonAddList.Location = new Point(220, 30);
            friendButtonAddList.Size = new Size(65, 22);

            friendButtonRemoveList.Text = LanguageHelper.GetString("MainForm.friendButtonRemoveList.Text") ?? "Remove";
            friendButtonRemoveList.Location = new Point(291, 30);
            friendButtonRemoveList.Size = new Size(65, 22);

            var sepList = new Panel
            {
                Location = new Point(10, 56),
                Size = new Size(346, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // ── Toggles ──
            friendPartyCheckBox.Text = LanguageHelper.GetString("MainForm.friendPartyCheckBox.Text") ?? "Autoaccept party from Friends";
            friendPartyCheckBox.Location = new Point(10, 62);
            friendPartyCheckBox.AutoSize = false;
            friendPartyCheckBox.Size = new Size(346, 22);
            friendPartyCheckBox.Padding = new Padding(0, 0, 10, 0);
            friendPartyCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);
            friendPartyCheckBox.TextAlign = ContentAlignment.MiddleLeft;
            friendPartyCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            friendAttackCheckBox.Text = LanguageHelper.GetString("MainForm.friendAttackCheckBox.Text") ?? "Prevent attacking friends in warmode";
            friendAttackCheckBox.Location = new Point(10, 88);
            friendAttackCheckBox.AutoSize = false;
            friendAttackCheckBox.Size = new Size(346, 22);
            friendAttackCheckBox.Padding = new Padding(0, 0, 10, 0);
            friendAttackCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);
            friendAttackCheckBox.TextAlign = ContentAlignment.MiddleLeft;
            friendAttackCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            friendIncludePartyCheckBox.Text = LanguageHelper.GetString("MainForm.friendIncludePartyCheckBox.Text") ?? "Include party member in Friend List";
            friendIncludePartyCheckBox.Location = new Point(10, 114);
            friendIncludePartyCheckBox.AutoSize = false;
            friendIncludePartyCheckBox.Size = new Size(346, 22);
            friendIncludePartyCheckBox.Padding = new Padding(0, 0, 10, 0);
            friendIncludePartyCheckBox.Font = RazorTheme.Fonts.DisplayFont(9F);
            friendIncludePartyCheckBox.TextAlign = ContentAlignment.MiddleLeft;
            friendIncludePartyCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            var sepToggles = new Panel
            {
                Location = new Point(10, 140),
                Size = new Size(346, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };

            // ── User Friend section ──
            var lblUserFriend = new Label
            {
                Text = LanguageHelper.GetString("MainForm.friendGroupBox.Text") ?? "Amici Utente",
                Location = new Point(10, 146),
                Size = new Size(100, 14),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            friendAddButton.Text = LanguageHelper.GetString("MainForm.friendAddButton.Text") ?? "Aggiungi Manuale";
            friendAddButton.Location = new Point(10, 162);
            friendAddButton.Size = new Size(125, 24);

            friendAddTargetButton.Text = LanguageHelper.GetString("MainForm.friendAddTargetButton.Text") ?? "Aggiungi Bersaglio";
            friendAddTargetButton.Location = new Point(141, 162);
            friendAddTargetButton.Size = new Size(125, 24);

            friendRemoveButton.Text = LanguageHelper.GetString("MainForm.friendRemoveButton.Text") ?? "Rimuovi";
            if (friendRemoveButton is RazorButton rbFriendRem)
                rbFriendRem.OverrideCustomColor = RazorTheme.Colors.Danger;
            friendRemoveButton.Location = new Point(272, 162);
            friendRemoveButton.Size = new Size(78, 24);

                        var sepUser = new Panel
                        {
                            Location = new Point(10, 190),
                            Size = new Size(346, 1),
                            BackColor = Color.FromArgb(50, 255, 255, 255)
                        };
            
                        // ── Guild Friend section ──
                        var lblGuild = new Label
                        {
                            Text = LanguageHelper.GetString("MainForm.groupBox34.Text") ?? "Amici Gilda",
                            Location = new Point(10, 196),
                            Size = new Size(100, 14),
                            ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                            Font = RazorTheme.Fonts.DisplayFont(8.5F, FontStyle.Bold),
                            BackColor = Color.Transparent
                        };
            
                        FriendGuildAddButton.Text = LanguageHelper.GetString("MainForm.FriendGuildAddButton.Text") ?? "Aggiungi Manuale";
                        FriendGuildAddButton.Location = new Point(10, 212);
                        FriendGuildAddButton.Size = new Size(155, 24);
            
                        FriendGuildRemoveButton.Text = LanguageHelper.GetString("MainForm.FriendGuildRemoveButton.Text") ?? "Rimuovi";
                        if (FriendGuildRemoveButton is RazorButton rbGuildRem)
                            rbGuildRem.OverrideCustomColor = RazorTheme.Colors.Danger;
                        FriendGuildRemoveButton.Location = new Point(171, 212);
                        FriendGuildRemoveButton.Size = new Size(100, 24);
            
                        var sepGuild = new Panel
                        {
                            Location = new Point(10, 240),
                            Size = new Size(346, 1),
                            BackColor = Color.FromArgb(50, 255, 255, 255)
                        };
            
                        // ── Log ──
                        var lblLog = new Label
                        {
                            Text = LanguageHelper.GetString("MainForm.friendloggroupBox.Text") ?? "Log Amici",
                            Location = new Point(10, 246),
                            Size = new Size(187, 14),
                            ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                            Font = RazorTheme.Fonts.DisplayFont(8.5F),
                            BackColor = Color.Transparent
                        };
            
                        friendLogBox.Location = new Point(10, 262);
                        friendLogBox.Size = new Size(346, 58);
                        friendLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        friendLogBox.BackColor = RazorTheme.Colors.CurrentCard;
                        friendLogBox.ForeColor = RazorTheme.Colors.CurrentText;
                        friendLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
                        friendLogBox.BorderStyle = BorderStyle.None;
            
                        friendsConfigCard.Controls.AddRange(new Control[]
                        {
                            lblList, friendListSelect,
                            friendButtonAddList, friendButtonRemoveList,
                            sepList,
                            friendPartyCheckBox, friendAttackCheckBox, friendIncludePartyCheckBox,
                            sepToggles,
                            lblUserFriend,
                            friendAddButton, friendAddTargetButton, friendRemoveButton,
                            sepUser,
                            lblGuild,
                            FriendGuildAddButton, FriendGuildRemoveButton,
                            sepGuild,
                            lblLog, friendLogBox
                        });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            friends.Controls.Clear();
            friends.Controls.AddRange(new Control[] { friendsListCard, friendsConfigCard });
            friends.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }
    }
}
