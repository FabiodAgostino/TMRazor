using System;
using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private RazorCard skillListCard;
        private RazorCard skillActionsCard;

        private void InitializeSkillsTab2()
        {
            // ─────────────────── Card: Elenco Abilità ─────────────────────────
            skillListCard = new RazorCard
            {
                Name = "skillListCard",
                Text = "\xE825  " + (LanguageHelper.GetString("MainForm.skillsTab.Text") ?? "Skills"),
                Location = new Point(10, 10),
                Size = new Size(460, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            skillList = new ListView
            {
                Name = "skillList",
                Location = new Point(10, 30),
                Size = new Size(428, 290),
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                AutoArrange = false,
                BackColor = RazorTheme.Colors.CurrentCard,
                ForeColor = RazorTheme.Colors.CurrentText,
                Font = RazorTheme.Fonts.DisplayFont(9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            skillHDRName = new ColumnHeader
            {
                Text = LanguageHelper.GetString("MainForm.skillHDRName.Text") ?? "Skill Name",
                Width = 218
            };
            skillHDRvalue = new ColumnHeader
            {
                Text = LanguageHelper.GetString("MainForm.skillHDRvalue.Text") ?? "Value",
                Width = 52
            };
            skillHDRbase = new ColumnHeader
            {
                Text = LanguageHelper.GetString("MainForm.skillHDRbase.Text") ?? "Base",
                Width = 52
            };
            skillHDRdelta = new ColumnHeader
            {
                Text = LanguageHelper.GetString("MainForm.skillHDRdelta.Text") ?? "+/-",
                Width = 44
            };
            skillHDRcap = new ColumnHeader
            {
                Text = LanguageHelper.GetString("MainForm.skillHDRcap.Text") ?? "Cap",
                Width = 52
            };
            skillHDRlock = new ColumnHeader
            {
                Text = LanguageHelper.GetString("MainForm.skillHDRlock.Text") ?? "Lock",
                Width = 40
            };

            skillList.Columns.AddRange(new ColumnHeader[]
            {
                skillHDRName, skillHDRvalue, skillHDRbase,
                skillHDRdelta, skillHDRcap, skillHDRlock
            });
            skillList.ColumnClick += new ColumnClickEventHandler(OnSkillColClick);
            skillList.MouseDown += new MouseEventHandler(skillList_MouseDown);

            skillListCard.Controls.Add(skillList);

            // ─────────────────── Card: Azioni ──────────────────────────────────
            skillActionsCard = new RazorCard
            {
                Name = "skillActionsCard",
                Text = "\xE8C4  " + (LanguageHelper.GetString("MainForm.skillActionsCard.Text") ?? "Actions"),
                Location = new Point(480, 10),
                Size = new Size(188, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            const int btnX = 10;
            const int btnW = 160;
            int y = 30;

            // Reset Delta
            resetDelta = new RazorButton
            {
                Name = "resetDelta",
                Text = LanguageHelper.GetString("MainForm.resetDelta.Text") ?? "Reset  +/-",
                Location = new Point(btnX, y),
                Size = new Size(btnW, 30)
            };
            resetDelta.Click += new EventHandler(OnResetSkillDelta);
            y += 38;

            Panel sep1 = SkillsMakeSeparator(btnX, y, btnW);
            y += 8;

            // Copia Selezionati
            skillCopySel = new RazorButton
            {
                Name = "skillCopySel",
                Text = LanguageHelper.GetString("MainForm.skillCopySel.Text") ?? "Copy Selected",
                Location = new Point(btnX, y),
                Size = new Size(btnW, 30)
            };
            skillCopySel.Click += new EventHandler(skillCopySel_Click);
            y += 38;

            // Copia Tutto
            skillCopyAll = new RazorButton
            {
                Name = "skillCopyAll",
                Text = LanguageHelper.GetString("MainForm.skillCopyAll.Text") ?? "Copy All",
                Location = new Point(btnX, y),
                Size = new Size(btnW, 30)
            };
            skillCopyAll.Click += new EventHandler(skillCopyAll_Click);
            y += 38;

            Panel sep2 = SkillsMakeSeparator(btnX, y, btnW);
            y += 8;

            // Mostra Variazioni toggle
            dispDelta = new RazorToggle
            {
                Name = "dispDelta",
                Text = LanguageHelper.GetString("MainForm.dispDelta.Text") ?? "Display Changes",
                Location = new Point(btnX, y),
                Size = new Size(btnW, 22)
            };
            dispDelta.CheckedChanged += new EventHandler(dispDelta_CheckedChanged);
            y += 30;

            Panel sep3 = SkillsMakeSeparator(btnX, y, btnW);
            y += 10;

            // Totale Base label + textbox
            label1 = new Label
            {
                Name = "label1",
                Text = LanguageHelper.GetString("MainForm.label1.Text") ?? "Base Total:",
                Location = new Point(btnX, y + 3),
                Size = new Size(88, 16),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };

            baseTotal = new RazorTextBox
            {
                Name = "baseTotal",
                Location = new Point(btnX + 90, y),
                Size = new Size(62, 22),
                ReadOnly = true
            };
            y += 32;

            Panel sep4 = SkillsMakeSeparator(btnX, y, btnW);
            y += 10;

            // Etichetta "Imposta Tutti i Blocchi"
            var lblSetLocks = new Label
            {
                Text = LanguageHelper.GetString("MainForm.skillSetLocksLabel.Text") ?? "Set All Locks:",
                Location = new Point(btnX, y + 2),
                Size = new Size(btnW, 16),
                ForeColor = RazorTheme.Colors.CurrentTextSecondary,
                Font = RazorTheme.Fonts.DisplayFont(8.5F),
                BackColor = Color.Transparent
            };
            y += 22;

            // Combo blocco
            locks = new RazorComboBox
            {
                Name = "locks",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(btnX, y),
                Size = new Size(btnW, 23)
            };
            locks.Items.AddRange(new object[] { "Up", "Down", "Locked" });
            y += 30;

            // Pulsante imposta blocchi
            setlocks = new RazorButton
            {
                Name = "setlocks",
                Text = LanguageHelper.GetString("MainForm.setlocks.Text") ?? "Set all locks",
                Location = new Point(btnX, y),
                Size = new Size(btnW, 30)
            };
            setlocks.Click += new EventHandler(OnSetSkillLocks);

            skillActionsCard.Controls.AddRange(new Control[]
            {
                resetDelta,
                sep1,
                skillCopySel, skillCopyAll,
                sep2,
                dispDelta,
                sep3,
                label1, baseTotal,
                sep4,
                lblSetLocks, locks, setlocks
            });

            // ─────────────────── Assembla Tab ──────────────────────────────────
            skillsTab.Controls.Clear();
            skillsTab.Controls.AddRange(new Control[] { skillListCard, skillActionsCard });
            skillsTab.BackColor = RazorTheme.Colors.BackgroundDark;

            Language.LoadControlNames(this);
        }

        private static Panel SkillsMakeSeparator(int x, int y, int width)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 1),
                BackColor = Color.FromArgb(50, 255, 255, 255)
            };
        }
    }
}
