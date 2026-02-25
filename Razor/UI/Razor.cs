using AutoUpdaterDotNET;
using JsonData;
using RazorEnhanced;
using RazorEnhanced.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Assistant.UI.Controls;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        #region Class Variables
        private System.Windows.Forms.TabControl tabs;
        private Assistant.UI.Controls.RazorCard groupBox1;
        private System.Windows.Forms.CheckedListBox filters;
        private System.Windows.Forms.ColumnHeader skillHDRName;
        private System.Windows.Forms.ColumnHeader skillHDRvalue;
        private System.Windows.Forms.ColumnHeader skillHDRbase;
        private System.Windows.Forms.ColumnHeader skillHDRdelta;
        private RazorButton resetDelta;
        private RazorButton setlocks;
        private RazorComboBox locks;
        private System.Windows.Forms.ListView skillList;
        private System.Windows.Forms.ColumnHeader skillHDRcap;
        private RazorToggle alwaysTop;
        private System.Windows.Forms.Label label1;
        private RazorTextBox baseTotal;
        private System.Windows.Forms.TabPage targettingTab;
        private System.Windows.Forms.TabPage advancedTab;
        private RazorButton skillCopySel;
        private RazorButton skillCopyAll;
        private System.Windows.Forms.TabPage generalTab;
        private System.Windows.Forms.TabPage toolbarTab;
        private System.Windows.Forms.TabPage skillsTab;
        private System.Windows.Forms.TabPage moreOptTab;
        private RazorToggle chkForceSpeechHue;
        private System.Windows.Forms.Label label3;
        private RazorTextBox txtSpellFormat;
        private RazorToggle chkForceSpellHue;
        private System.Windows.Forms.Label opacityLabel;
        private System.Windows.Forms.TrackBar opacity;
        private RazorToggle dispDelta;
        private RazorToggle openCorpses;
        private RazorTextBox corpseRange;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage screenshotTab;
        private System.Windows.Forms.TabPage statusTab;
        private System.Windows.Forms.TabPage technicalTab;
        private RazorToggle spamFilter;
        private System.Windows.Forms.PictureBox screenPrev;
        private System.Windows.Forms.ListBox screensList;
        private RazorButton setScnPath;
        private System.Windows.Forms.RadioButton radioFull;
        private System.Windows.Forms.RadioButton radioUO;
        private RazorToggle screenAutoCap;
        private RazorTextBox screenPath;
        private RazorButton capNow;
        private RazorToggle dispTime;
        private System.Windows.Forms.ColumnHeader skillHDRlock;
        private System.ComponentModel.IContainer components;
        private RazorToggle queueTargets;
        private System.Windows.Forms.RadioButton systray;
        private System.Windows.Forms.RadioButton taskbar;
        private System.Windows.Forms.Label label11;
        private RazorToggle autoStackRes;
        private RazorButton setMsgHue;
        private RazorButton setWarnHue;
        private RazorButton setSpeechHue;
        private RazorButton setBeneHue;
        private RazorButton setHarmHue;
        private RazorButton setNeuHue;
        private System.Windows.Forms.Label lblWarnHue;
        private System.Windows.Forms.Label lblMsgHue;
        private System.Windows.Forms.Label lblBeneHue;
        private System.Windows.Forms.Label lblHarmHue;
        private System.Windows.Forms.Label lblNeuHue;
        private RazorToggle incomingCorpse;
        private RazorToggle incomingMob;
        private System.Windows.Forms.TabPage enhancedFilterTab;
        private RazorToggle filterSnoop;
        private RazorToggle smartCPU;
        private RazorButton setLTHilight;
        private RazorToggle lthilight;
        private RazorToggle blockDis;
        private System.Windows.Forms.Label label12;
        private RazorComboBox imgFmt;
        internal System.Windows.Forms.ToolTip m_Tip;
        private System.Windows.Forms.TabPage MacrosTab;

        #endregion Class Variables

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);
        private System.Windows.Forms.CheckBox preAOSstatbar;
        private System.Windows.Forms.ComboBox clientPrio;
        private System.Windows.Forms.Label label9;
        private Label morefilterLabel;
        private Label labelStatus;
        private RazorButton razorButtonWiki;
        private RazorButton razorButtonWebsite;
        private RazorButton razorButtonSource;
        private readonly List<RazorEnhanced.Organizer.OrganizerItem> organizerItemList = new();
        private readonly List<RazorEnhanced.SellAgent.SellAgentItem> sellItemList = new();
        private readonly List<RazorEnhanced.BuyAgent.BuyAgentItem> buyItemList = new();
        private readonly List<RazorEnhanced.Dress.DressItemNew> dressItemList = new();
        private TabPage EnhancedAgent;
        private RazorTabControl tabControl1;
        private TabPage eautoloot;
        private Assistant.UI.Controls.RazorCard groupBox13;
        private ListBox autolootLogBox;
        private Label autolootContainerLabel;
        private RazorButton autolootItemPropsB;
        private RazorButton autolootAddItemBTarget;
        private RazorButton autolootContainerButton;
        private RazorToggle autoLootCheckBox;
        private TabPage escavenger;
        private Label label21;
        private RazorButton autoLootButtonRemoveList;
        private RazorButton autolootButtonAddList;
        private RazorComboBox autolootListSelect;
        private Label label20;
        private RazorButton scavengerButtonRemoveList;
        private RazorButton scavengerButtonAddList;
        private RazorComboBox scavengerListSelect;
        private Label label22;
        private Assistant.UI.Controls.RazorCard groupBox12;
        private ListBox scavengerLogBox;
        private Label label23;
        private RazorAgentNumOnlyTextBox scavengerDragDelay;
        private Label scavengerContainerLabel;
        private RazorButton scavengerButtonSetContainer;
        private RazorToggle scavengerCheckBox;
        private RazorButton scavengerButtonEditProps;
        private RazorButton scavengerButtonAddTarget;
        private TabPage organizer;
        private Assistant.UI.Controls.RazorCard groupBox16;
        private ListBox organizerLogBox;
        private Label label27;
        private RazorAgentNumOnlyTextBox organizerDragDelay;
        private RazorButton organizerSetDestinationB;
        private Label organizerSourceLabel;
        private RazorButton organizerAddTargetB;
        private RazorButton organizerSetSourceB;
        private RazorButton organizerRemoveListB;
        private RazorButton organizerAddListB;
        private RazorComboBox organizerListSelect;
        private Label label24;
        private TabPage VendorBuy;
        private TabPage VendorSell;
        private RazorButton buyAddTargetB;
        private Assistant.UI.Controls.RazorCard groupBox18;
        private ListBox buyLogBox;
        private RazorToggle buyEnableCheckBox;
        private RazorButton buyRemoveListButton;
        private RazorButton buyAddListButton;
        private RazorComboBox buyListSelect;
        private Label label25;
        private RazorButton sellAddTargerButton;
        private Assistant.UI.Controls.RazorCard groupBox20;
        private ListBox sellLogBox;
        private RazorToggle sellEnableCheckBox;
        private RazorButton sellRemoveListButton;
        private RazorButton sellAddListButton;
        private RazorComboBox sellListSelect;
        private Label label26;
        private Label sellBagLabel;
        private RazorButton sellSetBagButton;
        private TabPage Dress;
        private RazorToggle dressConflictCheckB;
        private Label dressBagLabel;
        private RazorButton dressSetBagB;
        private RazorButton undressExecuteButton;
        private RazorButton dressExecuteButton;
        private Assistant.UI.Controls.RazorCard groupBox22;
        private RazorButton dressAddTargetB;
        private RazorButton dressAddManualB;
        private RazorButton dressRemoveB;
        private RazorButton dressClearListB;
        private RazorButton dressReadB;
        private Label label29;
        private RazorAgentNumOnlyTextBox dressDragDelay;
        private Assistant.UI.Controls.RazorCard groupBox21;
        private ListBox dressLogBox;
        private ListView dressListView;
        private ColumnHeader columnHeader24;
        private ColumnHeader columnHeader25;
        private ColumnHeader columnHeader26;
        private ColumnHeader columnHeader27;
        private RazorButton dressRemoveListB;
        private RazorButton dressAddListB;
        private RazorComboBox dressListSelect;
        private Label label28;
        private NotifyIcon m_NotifyIcon;
        private OpenFileDialog openFileDialogscript;
        private System.Timers.Timer m_SystemTimer;
        private RazorButton dressStopButton;
        private System.Windows.Forms.Timer timerupdatestatus;
        private TabPage friends;
        private Assistant.UI.Controls.RazorCard friendloggroupBox;
        private ListBox friendLogBox;
        private RazorToggle friendIncludePartyCheckBox;
        private RazorToggle friendAttackCheckBox;
        private RazorToggle friendPartyCheckBox;
        private ColumnHeader columnHeader28;
        private ColumnHeader columnHeader29;
        private ColumnHeader columnHeader30;
        private RazorButton friendButtonRemoveList;
        private RazorButton friendButtonAddList;
        private RazorComboBox friendListSelect;
        private Label labelfriend;
        private Assistant.UI.Controls.RazorCard friendGroupBox;
        private RazorButton friendAddTargetButton;
        private RazorButton friendRemoveButton;
        private RazorButton friendAddButton;
        private TabPage restock;
        private Assistant.UI.Controls.RazorCard groupBox2;
        private ListBox restockLogBox;
        private Label label13;
        private RazorAgentNumOnlyTextBox restockDragDelay;
        private Label restockDestinationLabel;
        private RazorButton restockSetDestinationButton;
        private Label restockSourceLabel;
        private RazorButton restockAddTargetButton;
        private RazorButton restockSetSourceButton;
        private RazorButton restockRemoveListB;
        private RazorButton restockAddListB;
        private RazorComboBox restockListSelect;
        private Label label7;
        private TabPage bandageheal;
        private Assistant.UI.Controls.RazorCard BandageHealSettingsBox;
        private RazorToggle bandagehealcountdownCheckBox;
        private RazorToggle bandagehealhiddedCheckBox;
        private RazorToggle bandagehealmortalCheckBox;
        private RazorToggle bandagehealpoisonCheckBox;
        private Label label33;
        private RazorAgentNumOnlyTextBox bandagehealhpTextBox;
        private Label label32;
        private RazorToggle bandagehealdexformulaCheckBox;
        private RazorAgentNumHexTextBox bandagehealcustomcolorTextBox;
        private Label label30;
        private RazorAgentNumHexTextBox bandagehealcustomIDTextBox;
        private Label label19;
        private RazorToggle bandagehealcustomCheckBox;
        private Label bandagehealtargetLabel;
        private Label label15;
        private RazorButton bandagehealsettargetButton;
        private RazorComboBox bandagehealtargetComboBox;
        private Label label14;
        private RazorToggle bandagehealenableCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox5;
        private ListBox bandagehealLogBox;
        private RazorToggle rememberPwds;
        private RazorToggle gameSize;
        private RazorTextBox forceSizeX;
        private RazorTextBox forceSizeY;
        private RazorToggle chkStealth;
        private RazorToggle autoOpenDoors;
        private RazorToggle spellUnequip;
        private RazorToggle potionEquip;
        private Label label17;
        private RazorComboBox msglvl;
        private RazorToggle actionStatusMsg;
        private RazorToggle QueueActions;
        private RazorTextBox txtObjDelay;
        private Label label5;
        private Label label6;
        private RazorToggle smartLT;
        private RazorToggle rangeCheckLT;
        private RazorTextBox ltRange;
        private Label label8;
        private RazorToggle showtargtext;
        private RazorToggle showHealthOH;
        private RazorTextBox healthFmt;
        private Label label10;
        private RazorToggle chkPartyOverhead;
        private RazorButton openToolBarButton;
        private Assistant.UI.Controls.RazorCard groupBox25;
        private RazorToggle lockToolBarCheckBox;
        private RazorToggle autoopenToolBarCheckBox;
        private Label locationToolBarLabel;
        private RazorButton closeToolBarButton;
        private Assistant.UI.Controls.RazorCard groupBox26;
        private Label label38;
        private RazorTextBox toolboxcountNameTextBox;
        private Label label37;
        private RazorButton toolboxcountClearButton;
        private RazorButton toolboxcountTargetButton;
        private RazorAgentNumOnlyTextBox toolboxcountWarningTextBox;
        private Label label36;
        private RazorToggle toolboxcountHueWarningCheckBox;
        private RazorTextBox toolboxcountHueTextBox;
        private Label label35;
        private RazorAgentNumHexTextBox toolboxcountGraphTextBox;
        private Label label18;
        private RazorComboBox toolboxcountComboBox;
        private TabPage enhancedHotKeytabPage;
        private TreeView hotkeytreeView;
        private RazorHotKeyTextBox hotkeytextbox;
        private Assistant.UI.Controls.RazorCard groupBox27;
        private RazorButton hotkeyClearButton;
        private RazorButton hotkeySetButton;
        private Label label39;
        private Assistant.UI.Controls.RazorCard groupBox28;
        private RazorButton hotkeyMasterClearButton;
        private RazorButton hotkeyMasterSetButton;
        private Label label42;
        private Label hotkeyKeyMasterLabel;
        private RazorHotKeyTextBox hotkeyKeyMasterTextBox;
        private Label hotkeyStatusLabel;
        private RazorToggle hotkeypassCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox8;
        private RazorButton hotkeyMDisableButton;
        private RazorButton hotkeyMEnableButton;
        private Assistant.UI.Controls.RazorCard groupBox29;
        private Assistant.UI.Controls.RazorCard opacityGroupBox;
        private RazorButton profilesDeleteButton;
        private RazorButton profilesAddButton;
        private RazorComboBox profilesComboBox;
        private RazorButton profilesCloneButton;
        private RazorButton profilesRenameButton;
        private RazorButton profilesUnlinkButton;
        private RazorButton profilesLinkButton;
        private Label profilelinklabel;

        private bool m_CanClose = true;
        private RazorToggle showlauncher;
        private Assistant.UI.Controls.RazorCard groupBox4;
        private Label label43;
        private RazorComboBox toolboxsizeComboBox;
        private Label label41;
        private RazorToggle showfollowerToolBarCheckBox;
        private RazorToggle showweightToolBarCheckBox;
        private RazorToggle showmanaToolBarCheckBox;
        private RazorToggle showstaminaToolBarCheckBox;
        private RazorToggle showhitsToolBarCheckBox;
        private RazorComboBox toolboxstyleComboBox;
        private Label label2;
        private Label toolbarslot_label;
        private RazorButton FriendGuildRemoveButton;
        private RazorButton FriendGuildAddButton;
        private ColumnHeader columnHeader63;
        private ColumnHeader columnHeader64;
        private RazorToggle MINfriendCheckBox;
        private RazorToggle COMfriendCheckBox;
        private RazorToggle TBfriendCheckBox;
        private RazorToggle SLfriendCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox34;
        private Assistant.UI.Controls.RazorCard groupBox33;
        private RazorButton toolbarremoveslotButton;
        private RazorButton toolbaraddslotButton;
        private RazorToggle autoLootnoopenCheckBox;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private Assistant.UI.Controls.RazorCard groupBox37;
        private RazorButton gridhslotremove_button;
        private RazorButton gridhslotadd_button;
        private Label gridhslot_textbox;
        private Label label53;
        private RazorButton gridvslotremove_button;
        private RazorButton gridvslotadd_button;
        private Label gridvslot_textbox;
        private Label label49;
        private Assistant.UI.Controls.RazorCard groupBox36;
        private RazorComboBox gridborder_ComboBox;
        private Label label44;
        private RazorComboBox gridspell_ComboBox;
        private Label label52;
        private RazorComboBox gridgroup_ComboBox;
        private Label label51;
        private Label label45;
        private RazorComboBox gridslot_ComboBox;
        private Assistant.UI.Controls.RazorCard groupBox35;
        private RazorToggle gridlock_CheckBox;
        private RazorToggle gridopenlogin_CheckBox;
        private Label gridlocation_label;
        private RazorButton gridclose_button;
        private RazorButton gridopen_button;
        private Assistant.UI.Controls.RazorCard groupBox39;
        private TrackBar toolbar_trackBar;
        private Label toolbar_opacity_label;
        private Assistant.UI.Controls.RazorCard groupBox38;
        private TrackBar spellgrid_trackBar;
        private Label spellgrid_opacity_label;
        private TabControl toolbarstab;
        private Label labelHotride;
        private RazorAgentNumOnlyTextBox bandagehealmaxrangeTextBox;
        private Label label46;
        private RazorAgentNumOnlyTextBox bandagehealdelayTextBox;
        private Label label31;
        private RazorButton openchangelogButton;
        private RazorButton discordrazorButton;
        private DataGridView vendorsellGridView;
        private ContextMenuStrip datagridMenuStrip;
        private ToolStripMenuItem deleteRowToolStripMenuItem;
        private Label label50;
        private Assistant.UI.Controls.RazorCard groupBox19;
        private DataGridView scavengerdataGridView;
        private Assistant.UI.Controls.RazorCard groupBox41;
        private Label label54;
        private DataGridView autolootdataGridView;
        private Assistant.UI.Controls.RazorCard groupBox14;
        private Label label55;
        private DataGridView vendorbuydataGridView;
        private DataGridView organizerdataGridView;
        private System.Windows.Forms.Button organizerStopButton;
        private Assistant.UI.Controls.RazorCard groupBox11;
        private Label label57;
        private Label label56;
        private System.Windows.Forms.Button organizerExecuteButton;
        private DataGridView restockdataGridView;
        private Assistant.UI.Controls.RazorCard groupBox3;
        private Label label59;
        private Label label58;
        private System.Windows.Forms.Button restockExecuteButton;
        private System.Windows.Forms.Button restockStopButton;
        private Label label60;
        private RazorAgentNumOnlyTextBox autoLootTextBoxMaxRange;
        private Label label61;
        private RazorAgentNumOnlyTextBox scavengerRange;
        private System.Windows.Forms.CheckBox hiddedAutoOpenDoors;
        private System.Windows.Forms.CheckBox nosearchpouches;
        private System.Windows.Forms.CheckBox autosearchcontainers;
        private TabPage videoTab;
        private TextBox videoPathTextBox;
        private System.Windows.Forms.Button videoPathButton;
        private ListBox videolistBox;
        private Assistant.UI.Controls.RazorCard groupBox40;
        private Assistant.UI.Controls.RazorCard videosettinggroupBox;
        private System.Windows.Forms.Button videorecbutton;
        private System.Windows.Forms.Button videostopbutton;
        private Label label62;
        private Assistant.UI.Controls.RazorCard groupBox15;
        private Label videoRecStatuslabel;
        private Label label64;
        private Label label63;
        private Accord.Controls.VideoSourcePlayer videoSourcePlayer;
        private ComboBox gridscript_ComboBox;
        private Label label65;
        private TabPage DPStabPage;
        private System.Windows.Forms.Button DPSMeterStopButton;
        private System.Windows.Forms.Button DPSMeterStartButton;
        private System.Windows.Forms.Button DPSMeterClearButton;
        private Label DPSMeterStatusLabel;
        private Label label67;
        private System.Windows.Forms.Button DPSMeterPauseButton;
        private DataGridView DpsMeterGridView;
        private Assistant.UI.Controls.RazorCard filtergroup;
        private System.Windows.Forms.Button DPSMeterApplyFilterButton;
        private TextBox DPSmetername;
        private Label label70;
        private RazorAgentNumHexTextBox DPSmeterserial;
        private Label label69;
        private Label label68;
        private RazorAgentNumOnlyTextBox DPSmetermaxdamage;
        private Label label66;
        private RazorAgentNumOnlyTextBox DPSmetermindamage;
        private System.Windows.Forms.Button DPSMeterClearFilterButton;
        private System.Drawing.Point windowspt;

        // GumpInspector Flag
        internal bool GumpInspectorEnable = false;
        private ContextMenuStrip scriptgridMenuStrip;
        private ToolStripMenuItem modifyToolStripMenuItem;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem removeToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem moveUpToolStripMenuItem;
        private ToolStripMenuItem moveDownToolStripMenuItem;
        private ToolStripMenuItem moveToToolStripMenuItem;
        private ToolStripMenuItem flagsToolStripMenuItem;
        private ToolStripMenuItem loopModeToolStripMenuItem;
        private ToolStripMenuItem preloadToolStripMenuItem;
        private ToolStripMenuItem waitBeforeInterruptToolStripMenuItem;
        private ToolStripMenuItem autoStartAtLoginToolStripMenuItem;
        private ToolStripMenuItem playToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.Timer timertitlestatusbar;
        private System.Windows.Forms.CheckBox chknorunStealth;
        private System.Windows.Forms.CheckBox filterPoison;
        private Label label71;
        private Assistant.UI.Controls.RazorCard groupBox17;
        private Label label72;
        private TextBox enhancedmappathTextBox;
        private System.Windows.Forms.Button setpathmapbutton;
        private OpenFileDialog openmaplocation;
        private System.Windows.Forms.CheckBox autolootautostartCheckBox;
        private System.Windows.Forms.CheckBox scavengerautostartCheckBox;
        private System.Windows.Forms.CheckBox filterNPC;
        private System.Windows.Forms.Button autoLootButtonListClone;
        private System.Windows.Forms.Button scavengerButtonClone;
        private System.Windows.Forms.Button organizerCloneListB;
        private System.Windows.Forms.Button buyCloneButton;
        private System.Windows.Forms.Button sellCloneListButton;
        private System.Windows.Forms.Button restockCloneListB;
        private Assistant.UI.Controls.RazorCard groupBox43;
        private System.Windows.Forms.Button targetaddButton;
        private TextBox targetaddTextBox;
        private ListBox targetlistBox;
        private Assistant.UI.Controls.RazorCard groupBox45;
        private DataGridView targethueGridView;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn21;
        private System.Windows.Forms.CheckBox targetcoloCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox44;
        private DataGridView targetbodydataGridView;
        private System.Windows.Forms.CheckBox targetbodyCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox46;
        private Assistant.UI.Controls.RazorCard groupBox47;
        private RadioButton paralizedBoth;
        private RadioButton paralizedOff;
        private RadioButton paralizedOn;
        private Assistant.UI.Controls.RazorCard groupBox49;
        private RadioButton friendOn;
        private RadioButton friendBoth;
        private RadioButton friendOff;
        private Assistant.UI.Controls.RazorCard groupBox50;
        private RadioButton warmodeOn;
        private RadioButton warmodeBoth;
        private RadioButton warmodeOff;
        private Assistant.UI.Controls.RazorCard groupBox51;
        private RadioButton ghostOn;
        private RadioButton ghostBoth;
        private RadioButton ghostOff;
        private Assistant.UI.Controls.RazorCard groupBox52;
        private RadioButton humanOn;
        private RadioButton humanOff;
        private RadioButton humanBoth;
        private Assistant.UI.Controls.RazorCard groupBox53;
        private RadioButton blessedOn;
        private RadioButton blessedOff;
        private RadioButton blessedBoth;
        private Assistant.UI.Controls.RazorCard groupBox54;
        private RadioButton poisonedOn;
        private RadioButton poisonedOff;
        private RadioButton poisonedBoth;
        private Assistant.UI.Controls.RazorCard groupBox48;
        private Label label73;
        private Label label74;
        private TextBox targetRangeMaxTextBox;
        private Label label75;
        private TextBox targetRangeMinTextBox;
        private Assistant.UI.Controls.RazorCard groupBox55;
        private TextBox targetNameTextBox;
        private Assistant.UI.Controls.RazorCard groupBox57;
        private Assistant.UI.Controls.RazorCard groupBox56;
        private ComboBox targetSelectorComboBox;
        private System.Windows.Forms.CheckBox targetBlueCheckBox;
        private System.Windows.Forms.CheckBox targetYellowCheckBox;
        private System.Windows.Forms.CheckBox targetRedCheckBox;
        private System.Windows.Forms.CheckBox targetOrangeCheckBox;
        private System.Windows.Forms.CheckBox targetCriminalCheckBox;
        private System.Windows.Forms.CheckBox targetGreyCheckBox;
        private System.Windows.Forms.CheckBox targetGreenCheckBox;
        private System.Windows.Forms.Button targetsaveButton;
        private System.Windows.Forms.Button targetTestButton;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn20;
        private System.Windows.Forms.Button targetremoveButton;
        private Label label76;
        private System.Windows.Forms.Button targetChoseHue;
        private System.Windows.Forms.Button targetChoseBody;
        private System.Windows.Forms.CheckBox bandagehealAutostartCheckBox;
        private System.Windows.Forms.CheckBox bandagehealusetarget;
        private System.Windows.Forms.CheckBox bandagehealusetext;
        private TextBox bandagehealusetextSelfContent;
        private TextBox bandagehealusetextContent;
        private Label label77;
        private Label label78;
        private System.Windows.Forms.Button advertisementLink;
        private System.Windows.Forms.Button advertisementDiscordLink;
        private PictureBox advertisement;
        private System.Windows.Forms.CheckBox allowHiddenLooting;
        private System.Windows.Forms.CheckBox druidClericPackets;
        private System.Windows.Forms.CheckBox buyCompareNameCheckBox;
        private System.Windows.Forms.CheckBox showtitheToolBarCheckBox;
        private Label label79;
        private ComboBox videoCodecComboBox;
        private TextBox videoFPSTextBox;
        private DataGridViewCheckBoxColumn AutolootColumnX;
        private DataGridViewTextBoxColumn AutolootColumnItemName;
        private DataGridViewTextBoxColumn AutolootColumnItemID;
        private DataGridViewTextBoxColumn AutolootColumnColor;
        private DataGridViewTextBoxColumn LootBagColumnID;
        private DataGridViewTextBoxColumn AutolootColumnProps;
        private DataGridViewCheckBoxColumn ScavengerX;
        private DataGridViewTextBoxColumn ScavengerItemName;
        private DataGridViewTextBoxColumn ScavenerGraphics;
        private DataGridViewTextBoxColumn ScavengerColor;
        private DataGridViewTextBoxColumn ScavengerProp;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewCheckBoxColumn VendorSellX;
        private DataGridViewTextBoxColumn VendorSellItemName;
        private DataGridViewTextBoxColumn VendorSellGraphics;
        private DataGridViewTextBoxColumn VendorSellAmount;
        private DataGridViewTextBoxColumn VendorSellColor;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private CheckBox bandagehealTimeWithBufCheckBox;
        private RazorTabControl FilterPages;
        private RazorTabControl AdvancedPages;
        private RazorTabControl TechnicalPages;
        private TabPage MiscFilterPage;
        private Assistant.UI.Controls.RazorCard uomodgroupbox;
        private System.Windows.Forms.CheckBox uomodpaperdollCheckBox;
        private System.Windows.Forms.CheckBox uomodglobalsoundCheckBox;
        private System.Windows.Forms.CheckBox uomodFPSCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox32;
        private RazorAgentNumOnlyTextBox remountedelay;
        private RazorAgentNumOnlyTextBox remountdelay;
        private Label label48;
        private Label label40;
        private Label remountseriallabel;
        private Label label47;
        private System.Windows.Forms.Button remountsetbutton;
        private System.Windows.Forms.CheckBox remountcheckbox;
        private Assistant.UI.Controls.RazorCard groupBox24;
        private System.Windows.Forms.CheckBox colorflagsselfHighlightCheckBox;
        private System.Windows.Forms.CheckBox showagentmessageCheckBox;
        private System.Windows.Forms.CheckBox showmessagefieldCheckBox;
        private System.Windows.Forms.CheckBox colorflagsHighlightCheckBox;
        private System.Windows.Forms.CheckBox blockchivalryhealCheckBox;
        private System.Windows.Forms.CheckBox blockbighealCheckBox;
        private System.Windows.Forms.CheckBox blockminihealCheckBox;
        private System.Windows.Forms.CheckBox blockhealpoisonCheckBox;
        private System.Windows.Forms.CheckBox showheadtargetCheckBox;
        private System.Windows.Forms.CheckBox blockpartyinviteCheckBox;
        private System.Windows.Forms.CheckBox blocktraderequestCheckBox;
        private System.Windows.Forms.CheckBox highlighttargetCheckBox;
        private System.Windows.Forms.CheckBox flagsHighlightCheckBox;
        private System.Windows.Forms.CheckBox showstaticfieldCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox23;
        private DataGridView graphfilterdatagrid;
        private System.Windows.Forms.CheckBox mobfilterCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox10;
        private Assistant.UI.Controls.RazorCard overrideGroupBox;
        private Assistant.UI.Controls.RazorCard queueGroupBox;
        private Assistant.UI.Controls.RazorCard showmobileGroupBox;
        private Assistant.UI.Controls.RazorCard spellspotionsGroupBox;
        private Assistant.UI.Controls.RazorCard preaosstatusGroupBox;
        private Assistant.UI.Controls.RazorCard containeruseGroupBox;
        private Assistant.UI.Controls.RazorCard razormessagesGroupBox;
        private Assistant.UI.Controls.RazorCard stealthGroupBox;
        private Assistant.UI.Controls.RazorCard miscellaneousGroupBox;
        private Assistant.UI.Controls.RazorCard targetGroupBox;
        private Label autocarverbladeLabel;
        private Label label34;
        private System.Windows.Forms.Button autocarverrazorButton;
        private System.Windows.Forms.CheckBox autocarverCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox9;
        private Label bonebladeLabel;
        private Label label16;
        private System.Windows.Forms.Button boneCutterrazorButton;
        private System.Windows.Forms.CheckBox bonecutterCheckBox;
        private TabPage JournalFilterPage;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn18;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn19;
        private DataGridView journalfilterdatagrid;
        private ComboBox spellgridstyleComboBox;
        private Label label80;
        private System.Windows.Forms.Button setSpellBarOrigin;
        private DataGridViewTextBoxColumn journalFilterText;
        private CheckBox buyToCompleteAmount;
        private Button ChkForUpdate;
        private TabPage AllScripts;
        private RazorTabControl AllScriptsTab;
        private TabPage pythonScriptingTab;
        private RazorToggle scriptshowStartStopCheckBox;
        private Assistant.UI.Controls.RazorCard groupBox42;
        private RazorTextBox scriptSearchTextBox;
        private Assistant.UI.Controls.RazorCard scriptOperationsBox;
        private RazorButton buttonScriptTo;
        private RazorButton buttonScriptEditorNew;
        private RazorButton buttonScriptRefresh;
        private RazorButton buttonAddScript;
        private RazorButton buttonRemoveScript;
        private RazorButton buttonScriptDown;
        private RazorTextBox textBoxDelay;
        private RazorButton buttonScriptUp;
        private RazorButton buttonScriptEditor;
        private RazorButton buttonScriptStop;
        private RazorButton buttonScriptPlay;
        private Assistant.UI.Controls.RazorCard groupBox30;
        private RazorToggle scriptautostartcheckbox;
        private RazorToggle scriptwaitmodecheckbox;
        private RazorToggle scriptloopmodecheckbox;
        private RazorToggle scripterrorlogCheckBox;
        private RazorToggle showscriptmessageCheckBox;
        private ScriptListView pyScriptListView;
        private ColumnHeader filename;
        private ColumnHeader status;
        private ColumnHeader loop;
        private ColumnHeader autostart;
        private ColumnHeader wait;
        private ColumnHeader hotkey;
        private ColumnHeader heypass;
        private ColumnHeader index;
        private ColumnHeader fullFilePath;
        private Assistant.UI.Controls.RazorCard scriptControlBox;
        private TabPage uosScriptingTab;
        private TabPage csScriptingTab;
        private ScriptListView uosScriptListView;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private ColumnHeader columnHeader6;
        private ColumnHeader columnHeader7;
        private ColumnHeader columnHeader8;
        private ColumnHeader columnHeader9;
        private ScriptListView csScriptListView;
        private ColumnHeader columnHeader10;
        private ColumnHeader columnHeader11;
        private ColumnHeader columnHeader12;
        private ColumnHeader columnHeader13;
        private ColumnHeader columnHeader14;
        private ColumnHeader columnHeader15;
        private ColumnHeader columnHeader16;
        private ColumnHeader columnHeader17;
        private ColumnHeader columnHeader18;
        private RazorButton InspectContextButton;
        private RazorButton InspectGumpsButton;
        private RazorTextBox organizerDestination;
        private ListView friendlistView;
        private ListView friendguildListView;
        private SplitContainer splitContainer1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private RazorAgentNumOnlyTextBox autoLootTextBoxDelay;
        private RazorToggle bandageHealIgnoreCount;
        private RazorToggle scriptPacketLogCheckBox;
        private RazorToggle autoScriptReload;
        private Assistant.UI.Controls.RazorCard DmgDsplyGroup;
        private RazorToggle limitDamageDisplayEnable;
        private RazorAgentNumOnlyTextBox minDmgShown;
        private Label label81;
        private RazorToggle scriptpreload;
        private ColumnHeader preload;
        private ColumnHeader columnHeader19;
        private ColumnHeader columnHeader20;
        private CheckBox remoteControl;
        private CheckBox useUo3D;

        internal MainForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            RazorTheme.ApplyThemeToForm(this);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                this.AutoScaleMode = AutoScaleMode.Font;

            this.MaximizeBox = false;
            this.MinimizeBox = true;

            LanguageHelper.CurrentLanguage = Shards.allShards.Language ?? "it";
            LanguageHelper.TranslateForm(this);
            InitializeSidebar();

            m_NotifyIcon.ContextMenu =
                new ContextMenu(new MenuItem[]
                {
                    new( "Show Razor", new EventHandler( DoShowMe ) ),
                    new( "Hide Razor", new EventHandler( HideMe ) ),
                    new( "-" ),
                    new( "Toggle Razor Visibility", new EventHandler( ToggleVisible ) ),
                    new( "-" ),
                    new( "Close Razor && UO", new EventHandler( OnClose ) ),
                });
            m_NotifyIcon.ContextMenu.MenuItems[0].DefaultItem = true;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.friendlistView = new System.Windows.Forms.ListView();
            this.columnHeader28 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader29 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader30 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.friendguildListView = new System.Windows.Forms.ListView();
            this.columnHeader63 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader64 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabs = new System.Windows.Forms.TabControl();
            this.generalTab = new System.Windows.Forms.TabPage();
            this.label79 = new System.Windows.Forms.Label();
            this.openchangelogButton = new RazorButton();
            this.showlauncher = new RazorToggle();
            this.groupBox29 = new Assistant.UI.Controls.RazorCard();
            this.opacityGroupBox = new Assistant.UI.Controls.RazorCard();
            this.profilesCloneButton = new RazorButton();
            this.profilesRenameButton = new RazorButton();
            this.profilesUnlinkButton = new RazorButton();
            this.profilesLinkButton = new RazorButton();
            this.profilelinklabel = new System.Windows.Forms.Label();
            this.profilesDeleteButton = new RazorButton();
            this.profilesAddButton = new RazorButton();
            this.profilesComboBox = new RazorComboBox();
            this.forceSizeY = new RazorTextBox();
            this.forceSizeX = new RazorTextBox();
            this.gameSize = new RazorToggle();
            this.rememberPwds = new RazorToggle();
            this.clientPrio = new RazorComboBox();
            this.systray = new System.Windows.Forms.RadioButton();
            this.taskbar = new System.Windows.Forms.RadioButton();
            this.smartCPU = new RazorToggle();
            this.label11 = new System.Windows.Forms.Label();
            this.opacity = new System.Windows.Forms.TrackBar();
            this.alwaysTop = new RazorToggle();
            this.groupBox1 = new Assistant.UI.Controls.RazorCard();
            this.filters = new System.Windows.Forms.CheckedListBox();
            this.opacityLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.morefilterLabel = new System.Windows.Forms.Label();
            this.moreOptTab = new System.Windows.Forms.TabPage();
            this.remoteControl = new RazorToggle();
            this.druidClericPackets = new RazorToggle();
            this.allowHiddenLooting = new RazorToggle();
            this.filterNPC = new RazorToggle();
            this.groupBox17 = new Assistant.UI.Controls.RazorCard();
            this.setpathmapbutton = new RazorButton();
            this.label72 = new System.Windows.Forms.Label();
            this.enhancedmappathTextBox = new RazorTextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lblHarmHue = new System.Windows.Forms.Label();
            this.lblNeuHue = new System.Windows.Forms.Label();
            this.lblBeneHue = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblWarnHue = new System.Windows.Forms.Label();
            this.lblMsgHue = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.filterPoison = new RazorToggle();
            this.chknorunStealth = new RazorToggle();
            this.nosearchpouches = new RazorToggle();
            this.autosearchcontainers = new RazorToggle();
            this.hiddedAutoOpenDoors = new RazorToggle();
            this.chkPartyOverhead = new RazorToggle();
            this.healthFmt = new RazorTextBox();
            this.showHealthOH = new RazorToggle();
            this.showtargtext = new RazorToggle();
            this.ltRange = new RazorTextBox();
            this.rangeCheckLT = new RazorToggle();
            this.smartLT = new RazorToggle();
            this.txtObjDelay = new RazorTextBox();
            this.QueueActions = new RazorToggle();
            this.actionStatusMsg = new RazorToggle();
            this.msglvl = new RazorComboBox();
            this.potionEquip = new RazorToggle();
            this.spellUnequip = new RazorToggle();
            this.autoOpenDoors = new RazorToggle();
            this.chkStealth = new RazorToggle();
            this.preAOSstatbar = new RazorToggle();
            this.setLTHilight = new RazorButton();
            this.lthilight = new RazorToggle();
            this.filterSnoop = new RazorToggle();
            this.corpseRange = new RazorTextBox();
            this.incomingCorpse = new RazorToggle();
            this.incomingMob = new RazorToggle();
            this.setHarmHue = new RazorButton();
            this.setNeuHue = new RazorButton();
            this.setBeneHue = new RazorButton();
            this.setSpeechHue = new RazorButton();
            this.setWarnHue = new RazorButton();
            this.setMsgHue = new RazorButton();
            this.autoStackRes = new RazorToggle();
            this.queueTargets = new RazorToggle();
            this.spamFilter = new RazorToggle();
            this.openCorpses = new RazorToggle();
            this.blockDis = new RazorToggle();
            this.txtSpellFormat = new RazorTextBox();
            this.chkForceSpellHue = new RazorToggle();
            this.chkForceSpeechHue = new RazorToggle();
            this.enhancedFilterTab = new System.Windows.Forms.TabPage();
            this.FilterPages = new RazorTabControl();
            this.AdvancedPages = new RazorTabControl();
            this.TechnicalPages = new RazorTabControl();
            this.MiscFilterPage = new System.Windows.Forms.TabPage();
            this.DmgDsplyGroup = new Assistant.UI.Controls.RazorCard();
            this.minDmgShown = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.label81 = new System.Windows.Forms.Label();
            this.limitDamageDisplayEnable = new RazorToggle();
            this.uomodgroupbox = new Assistant.UI.Controls.RazorCard();
            this.uomodpaperdollCheckBox = new RazorToggle();
            this.uomodglobalsoundCheckBox = new RazorToggle();
            this.uomodFPSCheckBox = new RazorToggle();
            this.groupBox32 = new Assistant.UI.Controls.RazorCard();
            this.remountedelay = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.remountdelay = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.label48 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.remountseriallabel = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.remountsetbutton = new RazorButton();
            this.remountcheckbox = new RazorToggle();
            this.groupBox24 = new Assistant.UI.Controls.RazorCard();
            this.colorflagsselfHighlightCheckBox = new RazorToggle();
            this.showagentmessageCheckBox = new RazorToggle();
            this.showmessagefieldCheckBox = new RazorToggle();
            this.colorflagsHighlightCheckBox = new RazorToggle();
            this.blockchivalryhealCheckBox = new RazorToggle();
            this.blockbighealCheckBox = new RazorToggle();
            this.blockminihealCheckBox = new RazorToggle();
            this.blockhealpoisonCheckBox = new RazorToggle();
            this.showheadtargetCheckBox = new RazorToggle();
            this.blockpartyinviteCheckBox = new RazorToggle();
            this.blocktraderequestCheckBox = new RazorToggle();
            this.highlighttargetCheckBox = new RazorToggle();
            this.flagsHighlightCheckBox = new RazorToggle();
            this.showstaticfieldCheckBox = new RazorToggle();
            this.groupBox23 = new Assistant.UI.Controls.RazorCard();
            this.graphfilterdatagrid = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn4 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn18 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mobfilterCheckBox = new RazorToggle();
            this.groupBox10 = new Assistant.UI.Controls.RazorCard();
            this.overrideGroupBox = new Assistant.UI.Controls.RazorCard();
            this.queueGroupBox = new Assistant.UI.Controls.RazorCard();
            this.showmobileGroupBox = new Assistant.UI.Controls.RazorCard();
            this.spellspotionsGroupBox = new Assistant.UI.Controls.RazorCard();
            this.preaosstatusGroupBox = new Assistant.UI.Controls.RazorCard();
            this.containeruseGroupBox = new Assistant.UI.Controls.RazorCard();
            this.razormessagesGroupBox = new Assistant.UI.Controls.RazorCard();
            this.stealthGroupBox = new Assistant.UI.Controls.RazorCard();
            this.miscellaneousGroupBox = new Assistant.UI.Controls.RazorCard();
            this.targetGroupBox = new Assistant.UI.Controls.RazorCard();
            this.autocarverbladeLabel = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.autocarverrazorButton = new RazorButton();
            this.autocarverCheckBox = new RazorToggle();
            this.groupBox9 = new Assistant.UI.Controls.RazorCard();
            this.bonebladeLabel = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.boneCutterrazorButton = new RazorButton();
            this.bonecutterCheckBox = new RazorToggle();
            this.JournalFilterPage = new System.Windows.Forms.TabPage();
            this.journalfilterdatagrid = new System.Windows.Forms.DataGridView();
            this.journalFilterText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.datagridMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AllScripts = new System.Windows.Forms.TabPage();
            this.scriptControlBox = new Assistant.UI.Controls.RazorCard();
            this.autoScriptReload = new RazorToggle();
            this.scriptPacketLogCheckBox = new RazorToggle();
            this.InspectGumpsButton = new RazorButton();
            this.InspectContextButton = new RazorButton();
            this.scriptshowStartStopCheckBox = new RazorToggle();
            this.groupBox30 = new Assistant.UI.Controls.RazorCard();
            this.scriptpreload = new RazorToggle();
            this.scriptautostartcheckbox = new RazorToggle();
            this.scriptwaitmodecheckbox = new RazorToggle();
            this.scriptloopmodecheckbox = new RazorToggle();
            this.groupBox42 = new Assistant.UI.Controls.RazorCard();
            this.scriptSearchTextBox = new RazorTextBox();
            this.scripterrorlogCheckBox = new RazorToggle();
            this.scriptOperationsBox = new Assistant.UI.Controls.RazorCard();
            this.buttonScriptTo = new RazorButton();
            this.buttonScriptEditorNew = new RazorButton();
            this.buttonScriptRefresh = new RazorButton();
            this.buttonAddScript = new RazorButton();
            this.buttonRemoveScript = new RazorButton();
            this.buttonScriptDown = new RazorButton();
            this.textBoxDelay = new RazorTextBox();
            this.buttonScriptUp = new RazorButton();
            this.buttonScriptEditor = new RazorButton();
            this.buttonScriptStop = new RazorButton();
            this.buttonScriptPlay = new RazorButton();
            this.showscriptmessageCheckBox = new RazorToggle();
            this.AllScriptsTab = new RazorTabControl();
            this.pythonScriptingTab = new System.Windows.Forms.TabPage();
            this.pyScriptListView = new RazorEnhanced.UI.ScriptListView();
            this.filename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.loop = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.autostart = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.wait = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hotkey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.heypass = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.preload = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fullFilePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.uosScriptingTab = new System.Windows.Forms.TabPage();
            this.uosScriptListView = new RazorEnhanced.UI.ScriptListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.csScriptingTab = new System.Windows.Forms.TabPage();
            this.csScriptListView = new RazorEnhanced.UI.ScriptListView();
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EnhancedAgent = new System.Windows.Forms.TabPage();
            this.tabControl1 = new RazorTabControl();
            this.eautoloot = new System.Windows.Forms.TabPage();
            this.autoLootButtonListClone = new RazorButton();
            this.autolootautostartCheckBox = new RazorToggle();
            this.label60 = new System.Windows.Forms.Label();
            this.autoLootTextBoxMaxRange = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.autolootItemPropsB = new RazorButton();
            this.groupBox14 = new Assistant.UI.Controls.RazorCard();
            this.label55 = new System.Windows.Forms.Label();
            this.autolootContainerLabel = new System.Windows.Forms.Label();
            this.autolootContainerButton = new RazorButton();
            this.autolootAddItemBTarget = new RazorButton();
            this.autolootdataGridView = new System.Windows.Forms.DataGridView();
            this.AutolootColumnX = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.AutolootColumnItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AutolootColumnItemID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AutolootColumnColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LootBagColumnID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AutolootColumnProps = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.autoLootnoopenCheckBox = new RazorToggle();
            this.label21 = new System.Windows.Forms.Label();
            this.autoLootTextBoxDelay = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.autoLootButtonRemoveList = new RazorButton();
            this.autolootButtonAddList = new RazorButton();
            this.autolootListSelect = new RazorComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.groupBox13 = new Assistant.UI.Controls.RazorCard();
            this.autolootLogBox = new System.Windows.Forms.ListBox();
            this.autoLootCheckBox = new RazorToggle();
            this.escavenger = new System.Windows.Forms.TabPage();
            this.scavengerButtonClone = new RazorButton();
            this.scavengerautostartCheckBox = new RazorToggle();
            this.label61 = new System.Windows.Forms.Label();
            this.groupBox41 = new Assistant.UI.Controls.RazorCard();
            this.label54 = new System.Windows.Forms.Label();
            this.scavengerContainerLabel = new System.Windows.Forms.Label();
            this.scavengerButtonSetContainer = new RazorButton();
            this.scavengerdataGridView = new System.Windows.Forms.DataGridView();
            this.ScavengerX = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ScavengerItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ScavenerGraphics = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ScavengerColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ScavengerProp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox12 = new Assistant.UI.Controls.RazorCard();
            this.scavengerLogBox = new System.Windows.Forms.ListBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.scavengerButtonEditProps = new RazorButton();
            this.scavengerButtonAddTarget = new RazorButton();
            this.scavengerCheckBox = new RazorToggle();
            this.scavengerButtonRemoveList = new RazorButton();
            this.scavengerButtonAddList = new RazorButton();
            this.scavengerListSelect = new RazorComboBox();
            this.scavengerRange = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.scavengerDragDelay = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.organizer = new System.Windows.Forms.TabPage();
            this.organizerCloneListB = new RazorButton();
            this.organizerExecuteButton = new RazorButton();
            this.organizerStopButton = new RazorButton();
            this.groupBox11 = new Assistant.UI.Controls.RazorCard();
            this.organizerDestination = new RazorTextBox();
            this.label57 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.organizerSetSourceB = new RazorButton();
            this.organizerSetDestinationB = new RazorButton();
            this.organizerSourceLabel = new System.Windows.Forms.Label();
            this.organizerdataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox16 = new Assistant.UI.Controls.RazorCard();
            this.organizerLogBox = new System.Windows.Forms.ListBox();
            this.label27 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.organizerAddTargetB = new RazorButton();
            this.organizerRemoveListB = new RazorButton();
            this.organizerAddListB = new RazorButton();
            this.organizerListSelect = new RazorComboBox();
            this.organizerDragDelay = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.VendorBuy = new System.Windows.Forms.TabPage();
            this.buyToCompleteAmount = new RazorToggle();
            this.buyLogBox = new System.Windows.Forms.ListBox();
            this.buyCompareNameCheckBox = new RazorToggle();
            this.buyCloneButton = new RazorButton();
            this.vendorbuydataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox18 = new Assistant.UI.Controls.RazorCard();
            this.label25 = new System.Windows.Forms.Label();
            this.buyAddTargetB = new RazorButton();
            this.buyEnableCheckBox = new RazorToggle();
            this.buyRemoveListButton = new RazorButton();
            this.buyAddListButton = new RazorButton();
            this.buyListSelect = new RazorComboBox();
            this.VendorSell = new System.Windows.Forms.TabPage();
            this.sellCloneListButton = new RazorButton();
            this.groupBox19 = new Assistant.UI.Controls.RazorCard();
            this.sellSetBagButton = new RazorButton();
            this.label50 = new System.Windows.Forms.Label();
            this.sellBagLabel = new System.Windows.Forms.Label();
            this.vendorsellGridView = new System.Windows.Forms.DataGridView();
            this.VendorSellX = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.VendorSellItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VendorSellGraphics = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VendorSellAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VendorSellColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox20 = new Assistant.UI.Controls.RazorCard();
            this.sellLogBox = new System.Windows.Forms.ListBox();
            this.label26 = new System.Windows.Forms.Label();
            this.sellAddTargerButton = new RazorButton();
            this.sellEnableCheckBox = new RazorToggle();
            this.sellRemoveListButton = new RazorButton();
            this.sellAddListButton = new RazorButton();
            this.sellListSelect = new RazorComboBox();
            this.Dress = new System.Windows.Forms.TabPage();
            this.useUo3D = new RazorToggle();
            this.dressStopButton = new RazorButton();
            this.dressConflictCheckB = new RazorToggle();
            this.dressBagLabel = new System.Windows.Forms.Label();
            this.groupBox22 = new Assistant.UI.Controls.RazorCard();
            this.dressAddTargetB = new RazorButton();
            this.dressAddManualB = new RazorButton();
            this.dressRemoveB = new RazorButton();
            this.dressClearListB = new RazorButton();
            this.dressReadB = new RazorButton();
            this.label29 = new System.Windows.Forms.Label();
            this.groupBox21 = new Assistant.UI.Controls.RazorCard();
            this.dressLogBox = new System.Windows.Forms.ListBox();
            this.dressSetBagB = new RazorButton();
            this.undressExecuteButton = new RazorButton();
            this.dressExecuteButton = new RazorButton();
            this.dressDragDelay = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.dressListView = new System.Windows.Forms.ListView();
            this.columnHeader24 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader25 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader26 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader27 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label28 = new System.Windows.Forms.Label();
            this.dressRemoveListB = new RazorButton();
            this.dressAddListB = new RazorButton();
            this.dressListSelect = new RazorComboBox();
            this.friends = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox34 = new Assistant.UI.Controls.RazorCard();
            this.FriendGuildAddButton = new RazorButton();
            this.FriendGuildRemoveButton = new RazorButton();
            this.groupBox33 = new Assistant.UI.Controls.RazorCard();
            this.MINfriendCheckBox = new RazorToggle();
            this.SLfriendCheckBox = new RazorToggle();
            this.TBfriendCheckBox = new RazorToggle();
            this.COMfriendCheckBox = new RazorToggle();
            this.friendGroupBox = new Assistant.UI.Controls.RazorCard();
            this.friendAddTargetButton = new RazorButton();
            this.friendRemoveButton = new RazorButton();
            this.friendAddButton = new RazorButton();
            this.friendloggroupBox = new Assistant.UI.Controls.RazorCard();
            this.friendLogBox = new System.Windows.Forms.ListBox();
            this.friendIncludePartyCheckBox = new RazorToggle();
            this.friendAttackCheckBox = new RazorToggle();
            this.friendPartyCheckBox = new RazorToggle();
            this.labelfriend = new System.Windows.Forms.Label();
            this.friendButtonRemoveList = new RazorButton();
            this.friendButtonAddList = new RazorButton();
            this.friendListSelect = new RazorComboBox();
            this.restock = new System.Windows.Forms.TabPage();
            this.restockCloneListB = new RazorButton();
            this.restockExecuteButton = new RazorButton();
            this.restockStopButton = new RazorButton();
            this.groupBox3 = new Assistant.UI.Controls.RazorCard();
            this.label59 = new System.Windows.Forms.Label();
            this.label58 = new System.Windows.Forms.Label();
            this.restockSetSourceButton = new RazorButton();
            this.restockSourceLabel = new System.Windows.Forms.Label();
            this.restockDestinationLabel = new System.Windows.Forms.Label();
            this.restockSetDestinationButton = new RazorButton();
            this.restockdataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox2 = new Assistant.UI.Controls.RazorCard();
            this.restockLogBox = new System.Windows.Forms.ListBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.restockAddTargetButton = new RazorButton();
            this.restockRemoveListB = new RazorButton();
            this.restockAddListB = new RazorButton();
            this.restockListSelect = new RazorComboBox();
            this.restockDragDelay = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.bandageheal = new System.Windows.Forms.TabPage();
            this.BandageHealSettingsBox = new Assistant.UI.Controls.RazorCard();
            this.bandagehealAutostartCheckBox = new RazorToggle();
            this.bandageHealIgnoreCount = new RazorToggle();
            this.bandagehealTimeWithBufCheckBox = new RazorToggle();
            this.label78 = new System.Windows.Forms.Label();
            this.bandagehealenableCheckBox = new RazorToggle();
            this.label77 = new System.Windows.Forms.Label();
            this.bandagehealusetextContent = new RazorTextBox();
            this.bandagehealusetextSelfContent = new RazorTextBox();
            this.bandagehealusetext = new RazorToggle();
            this.bandagehealusetarget = new RazorToggle();
            this.bandagehealmaxrangeTextBox = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.bandagehealcountdownCheckBox = new RazorToggle();
            this.bandagehealhiddedCheckBox = new RazorToggle();
            this.bandagehealmortalCheckBox = new RazorToggle();
            this.bandagehealpoisonCheckBox = new RazorToggle();
            this.label33 = new System.Windows.Forms.Label();
            this.bandagehealhpTextBox = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.bandagehealdelayTextBox = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.bandagehealdexformulaCheckBox = new RazorToggle();
            this.bandagehealcustomcolorTextBox = new RazorEnhanced.UI.RazorAgentNumHexTextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.bandagehealcustomIDTextBox = new RazorEnhanced.UI.RazorAgentNumHexTextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.bandagehealcustomCheckBox = new RazorToggle();
            this.bandagehealtargetLabel = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.bandagehealsettargetButton = new RazorButton();
            this.bandagehealtargetComboBox = new RazorComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.groupBox5 = new Assistant.UI.Controls.RazorCard();
            this.bandagehealLogBox = new System.Windows.Forms.ListBox();
            this.toolbarTab = new System.Windows.Forms.TabPage();
            this.toolbarstab = new RazorTabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox39 = new Assistant.UI.Controls.RazorCard();
            this.toolbar_trackBar = new System.Windows.Forms.TrackBar();
            this.toolbar_opacity_label = new System.Windows.Forms.Label();
            this.groupBox25 = new Assistant.UI.Controls.RazorCard();
            this.lockToolBarCheckBox = new RazorToggle();
            this.autoopenToolBarCheckBox = new RazorToggle();
            this.locationToolBarLabel = new System.Windows.Forms.Label();
            this.closeToolBarButton = new RazorButton();
            this.openToolBarButton = new RazorButton();
            this.groupBox4 = new Assistant.UI.Controls.RazorCard();
            this.showtitheToolBarCheckBox = new RazorToggle();
            this.toolbarremoveslotButton = new RazorButton();
            this.toolbaraddslotButton = new RazorButton();
            this.toolbarslot_label = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.toolboxsizeComboBox = new RazorComboBox();
            this.label41 = new System.Windows.Forms.Label();
            this.showfollowerToolBarCheckBox = new RazorToggle();
            this.showweightToolBarCheckBox = new RazorToggle();
            this.showmanaToolBarCheckBox = new RazorToggle();
            this.showstaminaToolBarCheckBox = new RazorToggle();
            this.showhitsToolBarCheckBox = new RazorToggle();
            this.toolboxstyleComboBox = new RazorComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox26 = new Assistant.UI.Controls.RazorCard();
            this.label38 = new System.Windows.Forms.Label();
            this.toolboxcountNameTextBox = new RazorTextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.toolboxcountClearButton = new RazorButton();
            this.toolboxcountTargetButton = new RazorButton();
            this.toolboxcountWarningTextBox = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.label36 = new System.Windows.Forms.Label();
            this.toolboxcountHueWarningCheckBox = new RazorToggle();
            this.toolboxcountHueTextBox = new RazorTextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.toolboxcountGraphTextBox = new RazorEnhanced.UI.RazorAgentNumHexTextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.toolboxcountComboBox = new RazorComboBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox38 = new Assistant.UI.Controls.RazorCard();
            this.spellgrid_trackBar = new System.Windows.Forms.TrackBar();
            this.spellgrid_opacity_label = new System.Windows.Forms.Label();
            this.groupBox37 = new Assistant.UI.Controls.RazorCard();
            this.spellgridstyleComboBox = new RazorComboBox();
            this.label80 = new System.Windows.Forms.Label();
            this.gridhslotremove_button = new RazorButton();
            this.gridhslotadd_button = new RazorButton();
            this.gridhslot_textbox = new System.Windows.Forms.Label();
            this.label53 = new System.Windows.Forms.Label();
            this.gridvslotremove_button = new RazorButton();
            this.gridvslotadd_button = new RazorButton();
            this.gridvslot_textbox = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.groupBox36 = new Assistant.UI.Controls.RazorCard();
            this.gridscript_ComboBox = new RazorComboBox();
            this.label65 = new System.Windows.Forms.Label();
            this.gridborder_ComboBox = new RazorComboBox();
            this.label44 = new System.Windows.Forms.Label();
            this.gridspell_ComboBox = new RazorComboBox();
            this.label52 = new System.Windows.Forms.Label();
            this.gridgroup_ComboBox = new RazorComboBox();
            this.label51 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.gridslot_ComboBox = new RazorComboBox();
            this.groupBox35 = new Assistant.UI.Controls.RazorCard();
            this.setSpellBarOrigin = new RazorButton();
            this.gridlock_CheckBox = new RazorToggle();
            this.gridopenlogin_CheckBox = new RazorToggle();
            this.gridlocation_label = new System.Windows.Forms.Label();
            this.gridclose_button = new RazorButton();
            this.gridopen_button = new RazorButton();
            this.targettingTab = new System.Windows.Forms.TabPage();
            this.advancedTab = new System.Windows.Forms.TabPage();
            this.targetTestButton = new RazorButton();
            this.targetsaveButton = new RazorButton();
            this.groupBox57 = new Assistant.UI.Controls.RazorCard();
            this.targetYellowCheckBox = new RazorToggle();
            this.targetRedCheckBox = new RazorToggle();
            this.targetOrangeCheckBox = new RazorToggle();
            this.targetCriminalCheckBox = new RazorToggle();
            this.targetGreyCheckBox = new RazorToggle();
            this.targetGreenCheckBox = new RazorToggle();
            this.targetBlueCheckBox = new RazorToggle();
            this.groupBox56 = new Assistant.UI.Controls.RazorCard();
            this.targetSelectorComboBox = new RazorComboBox();
            this.groupBox55 = new Assistant.UI.Controls.RazorCard();
            this.targetNameTextBox = new RazorTextBox();
            this.groupBox48 = new Assistant.UI.Controls.RazorCard();
            this.label73 = new System.Windows.Forms.Label();
            this.label74 = new System.Windows.Forms.Label();
            this.targetRangeMaxTextBox = new RazorTextBox();
            this.label75 = new System.Windows.Forms.Label();
            this.targetRangeMinTextBox = new RazorTextBox();
            this.groupBox46 = new Assistant.UI.Controls.RazorCard();
            this.groupBox47 = new Assistant.UI.Controls.RazorCard();
            this.paralizedBoth = new System.Windows.Forms.RadioButton();
            this.paralizedOff = new System.Windows.Forms.RadioButton();
            this.paralizedOn = new System.Windows.Forms.RadioButton();
            this.groupBox49 = new Assistant.UI.Controls.RazorCard();
            this.friendOn = new System.Windows.Forms.RadioButton();
            this.friendBoth = new System.Windows.Forms.RadioButton();
            this.friendOff = new System.Windows.Forms.RadioButton();
            this.groupBox50 = new Assistant.UI.Controls.RazorCard();
            this.warmodeOn = new System.Windows.Forms.RadioButton();
            this.warmodeBoth = new System.Windows.Forms.RadioButton();
            this.warmodeOff = new System.Windows.Forms.RadioButton();
            this.groupBox51 = new Assistant.UI.Controls.RazorCard();
            this.ghostOn = new System.Windows.Forms.RadioButton();
            this.ghostBoth = new System.Windows.Forms.RadioButton();
            this.ghostOff = new System.Windows.Forms.RadioButton();
            this.groupBox52 = new Assistant.UI.Controls.RazorCard();
            this.humanOn = new System.Windows.Forms.RadioButton();
            this.humanOff = new System.Windows.Forms.RadioButton();
            this.humanBoth = new System.Windows.Forms.RadioButton();
            this.groupBox53 = new Assistant.UI.Controls.RazorCard();
            this.blessedOn = new System.Windows.Forms.RadioButton();
            this.blessedOff = new System.Windows.Forms.RadioButton();
            this.blessedBoth = new System.Windows.Forms.RadioButton();
            this.groupBox54 = new Assistant.UI.Controls.RazorCard();
            this.poisonedOn = new System.Windows.Forms.RadioButton();
            this.poisonedOff = new System.Windows.Forms.RadioButton();
            this.poisonedBoth = new System.Windows.Forms.RadioButton();
            this.groupBox45 = new Assistant.UI.Controls.RazorCard();
            this.targetChoseHue = new RazorButton();
            this.targethueGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.targetcoloCheckBox = new RazorToggle();
            this.groupBox44 = new Assistant.UI.Controls.RazorCard();
            this.targetChoseBody = new RazorButton();
            this.targetbodydataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn20 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.targetbodyCheckBox = new RazorToggle();
            this.groupBox43 = new Assistant.UI.Controls.RazorCard();
            this.label76 = new System.Windows.Forms.Label();
            this.targetremoveButton = new RazorButton();
            this.targetaddButton = new RazorButton();
            this.targetaddTextBox = new RazorTextBox();
            this.targetlistBox = new System.Windows.Forms.ListBox();
            this.skillsTab = new System.Windows.Forms.TabPage();
            this.dispDelta = new RazorToggle();
            this.skillCopyAll = new RazorButton();
            this.skillCopySel = new RazorButton();
            this.baseTotal = new RazorTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.locks = new RazorComboBox();
            this.setlocks = new RazorButton();
            this.resetDelta = new RazorButton();
            this.skillList = new System.Windows.Forms.ListView();
            this.skillHDRName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.skillHDRvalue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.skillHDRbase = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.skillHDRdelta = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.skillHDRcap = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.skillHDRlock = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.enhancedHotKeytabPage = new System.Windows.Forms.TabPage();
            this.groupBox8 = new Assistant.UI.Controls.RazorCard();
            this.hotkeyMasterClearButton = new RazorButton();
            this.hotkeyKeyMasterTextBox = new RazorEnhanced.UI.RazorHotKeyTextBox();
            this.hotkeyMasterSetButton = new RazorButton();
            this.label42 = new System.Windows.Forms.Label();
            this.groupBox28 = new Assistant.UI.Controls.RazorCard();
            this.hotkeyMDisableButton = new RazorButton();
            this.hotkeyMEnableButton = new RazorButton();
            this.hotkeyKeyMasterLabel = new System.Windows.Forms.Label();
            this.hotkeyStatusLabel = new System.Windows.Forms.Label();
            this.groupBox27 = new Assistant.UI.Controls.RazorCard();
            this.hotkeypassCheckBox = new RazorToggle();
            this.hotkeyClearButton = new RazorButton();
            this.hotkeySetButton = new RazorButton();
            this.label39 = new System.Windows.Forms.Label();
            this.hotkeytextbox = new RazorEnhanced.UI.RazorHotKeyTextBox();
            this.hotkeytreeView = new System.Windows.Forms.TreeView();
            this.screenshotTab = new System.Windows.Forms.TabPage();
            this.imgFmt = new RazorComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.capNow = new RazorButton();
            this.screenPath = new RazorTextBox();
            this.radioUO = new System.Windows.Forms.RadioButton();
            this.radioFull = new System.Windows.Forms.RadioButton();
            this.screenAutoCap = new RazorToggle();
            this.setScnPath = new RazorButton();
            this.screensList = new System.Windows.Forms.ListBox();
            this.screenPrev = new System.Windows.Forms.PictureBox();
            this.dispTime = new RazorToggle();
            this.videoTab = new System.Windows.Forms.TabPage();
            this.videoRecStatuslabel = new System.Windows.Forms.Label();
            this.label64 = new System.Windows.Forms.Label();
            this.groupBox40 = new Assistant.UI.Controls.RazorCard();
            this.videoSourcePlayer = new Accord.Controls.VideoSourcePlayer();
            this.videosettinggroupBox = new Assistant.UI.Controls.RazorCard();
            this.videoCodecComboBox = new RazorComboBox();
            this.label63 = new System.Windows.Forms.Label();
            this.label62 = new System.Windows.Forms.Label();
            this.videoFPSTextBox = new RazorTextBox();
            this.videorecbutton = new RazorButton();
            this.videostopbutton = new RazorButton();
            this.groupBox15 = new Assistant.UI.Controls.RazorCard();
            this.videolistBox = new System.Windows.Forms.ListBox();
            this.videoPathButton = new RazorButton();
            this.videoPathTextBox = new RazorTextBox();
            this.DPStabPage = new System.Windows.Forms.TabPage();
            this.filtergroup = new Assistant.UI.Controls.RazorCard();
            this.DPSMeterClearFilterButton = new RazorButton();
            this.DPSMeterApplyFilterButton = new RazorButton();
            this.DPSmetername = new RazorTextBox();
            this.label70 = new System.Windows.Forms.Label();
            this.DPSmeterserial = new RazorEnhanced.UI.RazorAgentNumHexTextBox();
            this.label69 = new System.Windows.Forms.Label();
            this.label68 = new System.Windows.Forms.Label();
            this.DPSmetermaxdamage = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.label66 = new System.Windows.Forms.Label();
            this.DPSmetermindamage = new RazorEnhanced.UI.RazorAgentNumOnlyTextBox();
            this.DpsMeterGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DPSMeterStatusLabel = new System.Windows.Forms.Label();
            this.label67 = new System.Windows.Forms.Label();
            this.DPSMeterPauseButton = new RazorButton();
            this.DPSMeterStopButton = new RazorButton();
            this.DPSMeterStartButton = new RazorButton();

            this.MacrosTab = new System.Windows.Forms.TabPage();

            this.DPSMeterClearButton = new RazorButton();
            this.statusTab = new System.Windows.Forms.TabPage();
            this.technicalTab = new System.Windows.Forms.TabPage();
            this.ChkForUpdate = new RazorButton();
            this.advertisementLink = new RazorButton();
            this.advertisementDiscordLink = new RazorButton();
            this.advertisement = new System.Windows.Forms.PictureBox();
            this.label71 = new System.Windows.Forms.Label();
            this.labelHotride = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.discordrazorButton = new RazorButton();
            this.razorButtonWiki = new RazorButton();
            this.razorButtonSource = new RazorButton();
            this.razorButtonWebsite = new RazorButton();
            this.m_NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.openFileDialogscript = new System.Windows.Forms.OpenFileDialog();
            this.timerupdatestatus = new System.Windows.Forms.Timer(this.components);
            this.scriptgridMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.modifyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loopModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.waitBeforeInterruptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoStartAtLoginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timertitlestatusbar = new System.Windows.Forms.Timer(this.components);
            this.openmaplocation = new System.Windows.Forms.OpenFileDialog();
            this.m_Tip = new System.Windows.Forms.ToolTip(this.components);
            this.MacrosTab.SuspendLayout();
            this.tabs.SuspendLayout();
            this.generalTab.SuspendLayout();
            this.technicalTab.SuspendLayout();
            this.groupBox29.SuspendLayout();
            this.opacityGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.opacity)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.moreOptTab.SuspendLayout();
            this.groupBox17.SuspendLayout();
            this.enhancedFilterTab.SuspendLayout();
            this.FilterPages.SuspendLayout();
            this.AdvancedPages.SuspendLayout();
            this.TechnicalPages.SuspendLayout();
            this.MiscFilterPage.SuspendLayout();
            this.DmgDsplyGroup.SuspendLayout();
            this.uomodgroupbox.SuspendLayout();
            this.groupBox32.SuspendLayout();
            this.groupBox24.SuspendLayout();
            this.groupBox23.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphfilterdatagrid)).BeginInit();
            this.groupBox10.SuspendLayout();
            this.overrideGroupBox.SuspendLayout();
            this.queueGroupBox.SuspendLayout();
            this.showmobileGroupBox.SuspendLayout();
            this.spellspotionsGroupBox.SuspendLayout();
            this.preaosstatusGroupBox.SuspendLayout();
            this.containeruseGroupBox.SuspendLayout();
            this.razormessagesGroupBox.SuspendLayout();
            this.stealthGroupBox.SuspendLayout();
            this.miscellaneousGroupBox.SuspendLayout();
            this.targetGroupBox.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.JournalFilterPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.journalfilterdatagrid)).BeginInit();
            this.datagridMenuStrip.SuspendLayout();
            this.AllScripts.SuspendLayout();
            this.scriptControlBox.SuspendLayout();
            this.groupBox30.SuspendLayout();
            this.groupBox42.SuspendLayout();
            this.scriptOperationsBox.SuspendLayout();
            this.AllScriptsTab.SuspendLayout();
            this.pythonScriptingTab.SuspendLayout();
            this.uosScriptingTab.SuspendLayout();
            this.csScriptingTab.SuspendLayout();
            this.EnhancedAgent.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.eautoloot.SuspendLayout();
            this.groupBox14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autolootdataGridView)).BeginInit();
            this.groupBox13.SuspendLayout();
            this.escavenger.SuspendLayout();
            this.groupBox41.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scavengerdataGridView)).BeginInit();
            this.groupBox12.SuspendLayout();
            this.organizer.SuspendLayout();
            this.groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.organizerdataGridView)).BeginInit();
            this.groupBox16.SuspendLayout();
            this.VendorBuy.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vendorbuydataGridView)).BeginInit();
            this.VendorSell.SuspendLayout();
            this.groupBox19.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vendorsellGridView)).BeginInit();
            this.groupBox20.SuspendLayout();
            this.Dress.SuspendLayout();
            this.groupBox22.SuspendLayout();
            this.groupBox21.SuspendLayout();
            this.friends.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox34.SuspendLayout();
            this.groupBox33.SuspendLayout();
            this.friendGroupBox.SuspendLayout();
            this.friendloggroupBox.SuspendLayout();
            this.restock.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.restockdataGridView)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.bandageheal.SuspendLayout();
            this.BandageHealSettingsBox.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.toolbarTab.SuspendLayout();
            this.toolbarstab.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox39.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toolbar_trackBar)).BeginInit();
            this.groupBox25.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox26.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox38.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spellgrid_trackBar)).BeginInit();
            this.groupBox37.SuspendLayout();
            this.groupBox36.SuspendLayout();
            this.groupBox35.SuspendLayout();
            this.targettingTab.SuspendLayout();
            this.advancedTab.SuspendLayout();
            this.groupBox57.SuspendLayout();
            this.groupBox56.SuspendLayout();
            this.groupBox55.SuspendLayout();
            this.groupBox48.SuspendLayout();
            this.groupBox46.SuspendLayout();
            this.groupBox47.SuspendLayout();
            this.groupBox49.SuspendLayout();
            this.groupBox50.SuspendLayout();
            this.groupBox51.SuspendLayout();
            this.groupBox52.SuspendLayout();
            this.groupBox53.SuspendLayout();
            this.groupBox54.SuspendLayout();
            this.groupBox45.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targethueGridView)).BeginInit();
            this.groupBox44.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetbodydataGridView)).BeginInit();
            this.groupBox43.SuspendLayout();
            this.skillsTab.SuspendLayout();
            this.enhancedHotKeytabPage.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox28.SuspendLayout();
            this.groupBox27.SuspendLayout();
            this.screenshotTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.screenPrev)).BeginInit();
            this.videoTab.SuspendLayout();
            this.groupBox40.SuspendLayout();
            this.videosettinggroupBox.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.DPStabPage.SuspendLayout();
            this.filtergroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DpsMeterGridView)).BeginInit();
            this.statusTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.advertisement)).BeginInit();
            this.scriptgridMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // friendlistView
            // 
            this.friendlistView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.friendlistView.CheckBoxes = true;
            this.friendlistView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader28,
            this.columnHeader29,
            this.columnHeader30});
            this.friendlistView.FullRowSelect = true;
            this.friendlistView.GridLines = true;
            this.friendlistView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.friendlistView.HideSelection = false;
            this.friendlistView.LabelWrap = false;
            this.friendlistView.Location = new System.Drawing.Point(3, 3);
            this.friendlistView.MinimumSize = new System.Drawing.Size(10, 10);
            this.friendlistView.MultiSelect = false;
            this.friendlistView.Name = "friendlistView";
            this.friendlistView.Size = new System.Drawing.Size(243, 130);
            this.friendlistView.TabIndex = 64;
            this.friendlistView.UseCompatibleStateImageBehavior = false;
            this.friendlistView.View = System.Windows.Forms.View.Details;
            this.friendlistView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.friendlistView_PlayerChecked);
            // 
            // columnHeader28
            // 
            this.columnHeader28.Text = "X";
            this.columnHeader28.Width = 30;
            // 
            // columnHeader29
            // 
            this.columnHeader29.Text = "Name";
            this.columnHeader29.Width = 230;
            // 
            // columnHeader30
            // 
            this.columnHeader30.Text = "Serial";
            this.columnHeader30.Width = 100;
            // 
            // friendguildListView
            // 
            this.friendguildListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.friendguildListView.CheckBoxes = true;
            this.friendguildListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader63,
            this.columnHeader64});
            this.friendguildListView.FullRowSelect = true;
            this.friendguildListView.GridLines = true;
            this.friendguildListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.friendguildListView.HideSelection = false;
            this.friendguildListView.LabelWrap = false;
            this.friendguildListView.Location = new System.Drawing.Point(3, 3);
            this.friendguildListView.MinimumSize = new System.Drawing.Size(10, 10);
            this.friendguildListView.MultiSelect = false;
            this.friendguildListView.Name = "friendguildListView";
            this.friendguildListView.Size = new System.Drawing.Size(243, 141);
            this.friendguildListView.TabIndex = 77;
            this.friendguildListView.UseCompatibleStateImageBehavior = false;
            this.friendguildListView.View = System.Windows.Forms.View.Details;
            this.friendguildListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.friendGuildListView_Checked);
            // 
            // columnHeader63
            // 
            this.columnHeader63.Text = "X";
            this.columnHeader63.Width = 30;
            // 
            // columnHeader64
            // 
            this.columnHeader64.Text = "Guild";
            this.columnHeader64.Width = 330;
            // 
            // tabs
            // 
            this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabs.Controls.Add(this.moreOptTab);
            this.tabs.Controls.Add(this.enhancedFilterTab);
            this.tabs.Controls.Add(this.AllScripts);
            this.tabs.Controls.Add(this.MacrosTab);  // NEW LINE - Macros tab
            this.tabs.Controls.Add(this.EnhancedAgent);
            this.tabs.Controls.Add(this.toolbarTab);
            this.tabs.Controls.Add(this.skillsTab);
            this.tabs.Controls.Add(this.enhancedHotKeytabPage);
            this.tabs.Controls.Add(this.advancedTab);
            this.tabs.Controls.Add(this.technicalTab);
            this.tabs.ItemSize = new System.Drawing.Size(0, 1);
            this.tabs.Location = new System.Drawing.Point(200, 0);
            this.tabs.Multiline = true;
            this.tabs.Name = "tabs";
            this.tabs.Padding = new System.Drawing.Point(0, 0);
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(486, 423);
            this.tabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabs.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabs.TabIndex = 0;
            this.tabs.SelectedIndexChanged += new System.EventHandler(this.tabs_IndexChanged);
            #region Options Tab
            #region Tab Page
            // 
            // moreOptTab
            // 
            this.moreOptTab.Controls.Add(this.druidClericPackets);
            this.moreOptTab.Controls.Add(this.groupBox17);
            this.moreOptTab.Controls.Add(this.label10);
            this.moreOptTab.Controls.Add(this.label5);
            this.moreOptTab.Controls.Add(this.label6);
            this.moreOptTab.Controls.Add(this.label17);
            this.moreOptTab.Controls.Add(this.lblHarmHue);
            this.moreOptTab.Controls.Add(this.lblNeuHue);
            this.moreOptTab.Controls.Add(this.lblBeneHue);
            this.moreOptTab.Controls.Add(this.label4);
            this.moreOptTab.Controls.Add(this.lblWarnHue);
            this.moreOptTab.Controls.Add(this.lblMsgHue);
            this.moreOptTab.Controls.Add(this.txtSpellFormat);
            this.moreOptTab.Controls.Add(this.label3);
            this.moreOptTab.Controls.Add(this.chknorunStealth);
            this.moreOptTab.Controls.Add(this.nosearchpouches);
            this.moreOptTab.Controls.Add(this.autosearchcontainers);
            this.moreOptTab.Controls.Add(this.hiddedAutoOpenDoors);
            this.moreOptTab.Controls.Add(this.chkPartyOverhead);
            this.moreOptTab.Controls.Add(this.healthFmt);
            this.moreOptTab.Controls.Add(this.showHealthOH);
            this.moreOptTab.Controls.Add(this.showtargtext);
            this.moreOptTab.Controls.Add(this.ltRange);
            this.moreOptTab.Controls.Add(this.rangeCheckLT);
            this.moreOptTab.Controls.Add(this.label8);
            this.moreOptTab.Controls.Add(this.smartLT);
            this.moreOptTab.Controls.Add(this.txtObjDelay);
            this.moreOptTab.Controls.Add(this.QueueActions);
            this.moreOptTab.Controls.Add(this.actionStatusMsg);
            this.moreOptTab.Controls.Add(this.msglvl);
            this.moreOptTab.Controls.Add(this.potionEquip);
            this.moreOptTab.Controls.Add(this.spellUnequip);
            this.moreOptTab.Controls.Add(this.autoOpenDoors);
            this.moreOptTab.Controls.Add(this.chkStealth);
            this.moreOptTab.Controls.Add(this.preAOSstatbar);
            this.moreOptTab.Controls.Add(this.setLTHilight);
            this.moreOptTab.Controls.Add(this.lthilight);
            this.moreOptTab.Controls.Add(this.corpseRange);
            this.moreOptTab.Controls.Add(this.incomingCorpse);
            this.moreOptTab.Controls.Add(this.incomingMob);
            this.moreOptTab.Controls.Add(this.setHarmHue);
            this.moreOptTab.Controls.Add(this.setNeuHue);
            this.moreOptTab.Controls.Add(this.setNeuHue);
            this.moreOptTab.Controls.Add(this.setBeneHue);
            this.moreOptTab.Controls.Add(this.setSpeechHue);
            this.moreOptTab.Controls.Add(this.setWarnHue);
            this.moreOptTab.Controls.Add(this.setMsgHue);
            this.moreOptTab.Controls.Add(this.autoStackRes);
            this.moreOptTab.Controls.Add(this.queueTargets);
            this.moreOptTab.Controls.Add(this.openCorpses);
            this.moreOptTab.Controls.Add(this.blockDis);
            this.moreOptTab.Controls.Add(this.chkForceSpellHue);
            this.moreOptTab.Controls.Add(this.chkForceSpeechHue);
            this.moreOptTab.Controls.Add(this.overrideGroupBox);
            this.moreOptTab.Controls.Add(this.queueGroupBox);
            this.moreOptTab.Controls.Add(this.targetGroupBox);
            this.moreOptTab.Controls.Add(this.showmobileGroupBox);
            this.moreOptTab.Controls.Add(this.spellspotionsGroupBox);
            this.moreOptTab.Controls.Add(this.containeruseGroupBox);
            this.moreOptTab.Controls.Add(this.preaosstatusGroupBox);
            this.moreOptTab.Controls.Add(this.stealthGroupBox);
            this.moreOptTab.Controls.Add(this.miscellaneousGroupBox);
            this.moreOptTab.Controls.Add(this.razormessagesGroupBox);
            this.moreOptTab.Location = new System.Drawing.Point(4, 54);
            this.moreOptTab.Name = "moreOptTab";
            this.moreOptTab.Size = new System.Drawing.Size(678, 365);
            this.moreOptTab.TabIndex = 5;
            this.moreOptTab.Text = "Options";
            #endregion
            #region Overrides
            // 
            // overrideGroupBox
            // 
            this.overrideGroupBox.Location = new System.Drawing.Point(2, 5);
            this.overrideGroupBox.Name = "overrideGroupBox";
            this.overrideGroupBox.Size = new System.Drawing.Size(200, 220);
            this.overrideGroupBox.TabIndex = 80;
            this.overrideGroupBox.TabStop = false;
            this.overrideGroupBox.Text = "Overrides";
            // 
            // lblMsgHue
            // 
            this.lblMsgHue.Location = new System.Drawing.Point(7, 19);
            this.lblMsgHue.Name = "lblMsgHue";
            this.lblMsgHue.Size = new System.Drawing.Size(139, 17);
            this.lblMsgHue.TabIndex = 15;
            this.lblMsgHue.Text = "TM Razor Message Hue";
            // 
            // setMsgHue
            // 
            this.setMsgHue.Location = new System.Drawing.Point(152, 18);
            this.setMsgHue.Name = "setMsgHue";
            this.setMsgHue.Size = new System.Drawing.Size(32, 19);
            this.setMsgHue.TabIndex = 38;
            this.setMsgHue.Text = "Set";
            this.setMsgHue.Click += new System.EventHandler(this.setMsgHue_Click);
            // 
            // lblWarnHue
            // 
            this.lblWarnHue.Location = new System.Drawing.Point(7, 44);
            this.lblWarnHue.Name = "lblWarnHue";
            this.lblWarnHue.Size = new System.Drawing.Size(139, 16);
            this.lblWarnHue.TabIndex = 16;
            this.lblWarnHue.Text = "Warning Message Hue";
            // 
            // setWarnHue
            // 
            this.setWarnHue.Location = new System.Drawing.Point(152, 42);
            this.setWarnHue.Name = "setWarnHue";
            this.setWarnHue.Size = new System.Drawing.Size(32, 20);
            this.setWarnHue.TabIndex = 39;
            this.setWarnHue.Text = "Set";
            this.setWarnHue.Click += new System.EventHandler(this.setWarnHue_Click);
            // 
            // chkForceSpeechHue
            // 
            this.chkForceSpeechHue.Location = new System.Drawing.Point(7, 69);
            this.chkForceSpeechHue.Name = "chkForceSpeechHue";
            this.chkForceSpeechHue.Size = new System.Drawing.Size(139, 22);
            this.chkForceSpeechHue.TabIndex = 0;
            this.chkForceSpeechHue.Text = "Override Speech Hue";
            this.chkForceSpeechHue.CheckedChanged += new System.EventHandler(this.chkForceSpeechHue_CheckedChanged);
            // 
            // setSpeechHue
            // 
            this.setSpeechHue.Location = new System.Drawing.Point(152, 66);
            this.setSpeechHue.Name = "setSpeechHue";
            this.setSpeechHue.Size = new System.Drawing.Size(32, 20);
            this.setSpeechHue.TabIndex = 40;
            this.setSpeechHue.Text = "Set";
            this.setSpeechHue.Click += new System.EventHandler(this.setSpeechHue_Click);
            // 
            // lthilight
            // 
            this.lthilight.Location = new System.Drawing.Point(7, 93);
            this.lthilight.Name = "lthilight";
            this.lthilight.Size = new System.Drawing.Size(139, 22);
            this.lthilight.TabIndex = 50;
            this.lthilight.Text = "Last Target Highlight:";
            this.lthilight.CheckedChanged += new System.EventHandler(this.lthilight_CheckedChanged);
            // 
            // setLTHilight
            // 
            this.setLTHilight.Location = new System.Drawing.Point(152, 90);
            this.setLTHilight.Name = "setLTHilight";
            this.setLTHilight.Size = new System.Drawing.Size(32, 20);
            this.setLTHilight.TabIndex = 51;
            this.setLTHilight.Text = "Set";
            this.setLTHilight.Click += new System.EventHandler(this.setLTHilight_Click);
            // 
            // chkForceSpellHue
            // 
            this.chkForceSpellHue.Location = new System.Drawing.Point(7, 117);
            this.chkForceSpellHue.Name = "chkForceSpellHue";
            this.chkForceSpellHue.Size = new System.Drawing.Size(139, 22);
            this.chkForceSpellHue.TabIndex = 2;
            this.chkForceSpellHue.Text = "Override Spell Hues:";
            this.chkForceSpellHue.CheckedChanged += new System.EventHandler(this.chkForceSpellHue_CheckedChanged);
            // 
            // lblBeneHue
            // 
            this.lblBeneHue.Location = new System.Drawing.Point(17, 142);
            this.lblBeneHue.Name = "lblBeneHue";
            this.lblBeneHue.Size = new System.Drawing.Size(55, 14);
            this.lblBeneHue.TabIndex = 44;
            this.lblBeneHue.Text = "Beneficial";
            // 
            // lblHarmHue
            // 
            this.lblHarmHue.Location = new System.Drawing.Point(77, 142);
            this.lblHarmHue.Name = "lblHarmHue";
            this.lblHarmHue.Size = new System.Drawing.Size(45, 14);
            this.lblHarmHue.TabIndex = 46;
            this.lblHarmHue.Text = "Harmful";
            // 
            // lblNeuHue
            // 
            this.lblNeuHue.Location = new System.Drawing.Point(135, 142);
            this.lblNeuHue.Name = "lblNeuHue";
            this.lblNeuHue.Size = new System.Drawing.Size(42, 14);
            this.lblNeuHue.TabIndex = 45;
            this.lblNeuHue.Text = "Neutral";
            // 
            // setBeneHue
            // 
            this.setBeneHue.Location = new System.Drawing.Point(28, 159);
            this.setBeneHue.Name = "setBeneHue";
            this.setBeneHue.Size = new System.Drawing.Size(33, 20);
            this.setBeneHue.TabIndex = 41;
            this.setBeneHue.Text = "Set";
            this.setBeneHue.Click += new System.EventHandler(this.setBeneHue_Click);
            // 
            // setHarmHue
            // 
            this.setHarmHue.Enabled = false;
            this.setHarmHue.Location = new System.Drawing.Point(83, 159);
            this.setHarmHue.Name = "setHarmHue";
            this.setHarmHue.Size = new System.Drawing.Size(32, 20);
            this.setHarmHue.TabIndex = 42;
            this.setHarmHue.Text = "Set";
            this.setHarmHue.Click += new System.EventHandler(this.setHarmHue_Click);
            // 
            // setNeuHue
            // 
            this.setNeuHue.Enabled = false;
            this.setNeuHue.Location = new System.Drawing.Point(140, 159);
            this.setNeuHue.Name = "setNeuHue";
            this.setNeuHue.Size = new System.Drawing.Size(31, 20);
            this.setNeuHue.TabIndex = 43;
            this.setNeuHue.Text = "Set";
            this.setNeuHue.Click += new System.EventHandler(this.setNeuHue_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 193);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Spell Format:";
            // 
            // txtSpellFormat
            // 
            this.txtSpellFormat.BackColor = System.Drawing.Color.White;
            this.txtSpellFormat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSpellFormat.Location = new System.Drawing.Point(86, 191);
            this.txtSpellFormat.Name = "txtSpellFormat";
            this.txtSpellFormat.Size = new System.Drawing.Size(106, 20);
            this.txtSpellFormat.TabIndex = 5;
            this.txtSpellFormat.TextChanged += new System.EventHandler(this.txtSpellFormat_TextChanged);
            #endregion
            #region Targets Group
            // 
            // targetGroupBox
            // 
            this.targetGroupBox.Location = new System.Drawing.Point(210, 5);
            this.targetGroupBox.Name = "targetGroupBox";
            this.targetGroupBox.Size = new System.Drawing.Size(210, 105);
            this.targetGroupBox.TabIndex = 80;
            this.targetGroupBox.TabStop = false;
            this.targetGroupBox.Text = "Targets";
            // 
            // smartLT
            // 
            this.smartLT.Location = new System.Drawing.Point(214, 20);
            this.smartLT.Name = "smartLT";
            this.smartLT.Size = new System.Drawing.Size(185, 22);
            this.smartLT.TabIndex = 52;
            this.smartLT.Text = "Use smart last target";
            this.smartLT.CheckedChanged += new System.EventHandler(this.smartLT_CheckedChanged);
            // 
            // rangeCheckLT
            // 
            this.rangeCheckLT.Location = new System.Drawing.Point(214, 38);
            this.rangeCheckLT.Name = "rangeCheckLT";
            this.rangeCheckLT.Size = new System.Drawing.Size(185, 22);
            this.rangeCheckLT.TabIndex = 40;
            this.rangeCheckLT.Text = "Range check Last Target";
            this.rangeCheckLT.CheckedChanged += new System.EventHandler(this.rangeCheckLT_CheckedChanged);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(230, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 18);
            this.label8.TabIndex = 72;
            this.label8.Text = "Tiles:";
            // 
            // ltRange
            // 
            this.ltRange.BackColor = System.Drawing.Color.White;
            this.ltRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ltRange.Location = new System.Drawing.Point(265, 59);
            this.ltRange.Name = "ltRange";
            this.ltRange.Size = new System.Drawing.Size(21, 20);
            this.ltRange.TabIndex = 41;
            this.ltRange.TextChanged += new System.EventHandler(this.ltRange_TextChanged);
            // 
            // showtargtext
            // 
            this.showtargtext.Location = new System.Drawing.Point(214, 81);
            this.showtargtext.Name = "showtargtext";
            this.showtargtext.Size = new System.Drawing.Size(190, 22);
            this.showtargtext.TabIndex = 53;
            this.showtargtext.Text = "Show target flag on single click";
            this.showtargtext.CheckedChanged += new System.EventHandler(this.showtargtext_CheckedChanged);
            #endregion
            #region Queues Group
            // 
            // queueGroupBox
            // 
            this.queueGroupBox.Location = new System.Drawing.Point(426, 5);
            this.queueGroupBox.Name = "queueGroupBox";
            this.queueGroupBox.Size = new System.Drawing.Size(230, 105);
            this.queueGroupBox.TabIndex = 80;
            this.queueGroupBox.TabStop = false;
            this.queueGroupBox.Text = "Queues";
            // 
            // actionStatusMsg
            // 
            this.actionStatusMsg.Location = new System.Drawing.Point(436, 20);
            this.actionStatusMsg.Name = "actionStatusMsg";
            this.actionStatusMsg.Size = new System.Drawing.Size(212, 22);
            this.actionStatusMsg.TabIndex = 38;
            this.actionStatusMsg.Text = "Show Action-Queue status messages";
            this.actionStatusMsg.CheckedChanged += new System.EventHandler(this.actionStatusMsg_CheckedChanged);
            // 
            // QueueActions
            // 
            this.QueueActions.Location = new System.Drawing.Point(436, 38);
            this.QueueActions.Name = "QueueActions";
            this.QueueActions.Size = new System.Drawing.Size(202, 22);
            this.QueueActions.TabIndex = 34;
            this.QueueActions.Text = "Auto-Queue Object Delay actions ";
            this.QueueActions.CheckedChanged += new System.EventHandler(this.QueueActions_CheckedChanged);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(452, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 18);
            this.label5.TabIndex = 70;
            this.label5.Text = "Object delay:";
            // 
            // txtObjDelay
            // 
            this.txtObjDelay.BackColor = System.Drawing.Color.White;
            this.txtObjDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtObjDelay.Location = new System.Drawing.Point(530, 59);
            this.txtObjDelay.Name = "txtObjDelay";
            this.txtObjDelay.Size = new System.Drawing.Size(31, 20);
            this.txtObjDelay.TabIndex = 37;
            this.txtObjDelay.TextChanged += new System.EventHandler(this.txtObjDelay_TextChanged);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(568, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 18);
            this.label6.TabIndex = 71;
            this.label6.Text = "ms";
            // 
            // queueTargets
            // 
            this.queueTargets.Location = new System.Drawing.Point(436, 81);
            this.queueTargets.Name = "queueTargets";
            this.queueTargets.Size = new System.Drawing.Size(218, 22);
            this.queueTargets.TabIndex = 34;
            this.queueTargets.Text = "Queue Last Target and Target Self";
            this.queueTargets.CheckedChanged += new System.EventHandler(this.queueTargets_CheckedChanged);
            #endregion
            #region Containers Group
            // 
            // containeruseGroupBox
            // 
            this.containeruseGroupBox.Location = new System.Drawing.Point(210, 110);
            this.containeruseGroupBox.Name = "containeruseGroupBox";
            this.containeruseGroupBox.Size = new System.Drawing.Size(210, 85);
            this.containeruseGroupBox.TabIndex = 80;
            this.containeruseGroupBox.TabStop = false;
            this.containeruseGroupBox.Text = "Containers";
            // 
            // openCorpses
            // 
            this.openCorpses.Location = new System.Drawing.Point(214, 126);
            this.openCorpses.Name = "openCorpses";
            this.openCorpses.Size = new System.Drawing.Size(156, 22);
            this.openCorpses.TabIndex = 22;
            this.openCorpses.Text = "Open new corpses within";
            this.openCorpses.CheckedChanged += new System.EventHandler(this.openCorpses_CheckedChanged);
            // 
            // corpseRange
            // 
            this.corpseRange.BackColor = System.Drawing.Color.White;
            this.corpseRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.corpseRange.Location = new System.Drawing.Point(365, 126);
            this.corpseRange.Name = "corpseRange";
            this.corpseRange.Size = new System.Drawing.Size(22, 20);
            this.corpseRange.TabIndex = 23;
            this.corpseRange.TextChanged += new System.EventHandler(this.corpseRange_TextChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(389, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 22);
            this.label4.TabIndex = 22;
            this.label4.Text = "tiles";
            // 
            // nosearchpouches
            // 
            this.nosearchpouches.Location = new System.Drawing.Point(233, 166);
            this.nosearchpouches.Name = "nosearchpouches";
            this.nosearchpouches.Size = new System.Drawing.Size(185, 22);
            this.nosearchpouches.TabIndex = 77;
            this.nosearchpouches.Text = "Ignore pouches";
            this.nosearchpouches.CheckedChanged += new System.EventHandler(this.nosearchpouches_CheckedChanged);
            // 
            // autosearchcontainers
            // 
            this.autosearchcontainers.Location = new System.Drawing.Point(214, 146);
            this.autosearchcontainers.Name = "autosearchcontainers";
            this.autosearchcontainers.Size = new System.Drawing.Size(204, 22);
            this.autosearchcontainers.TabIndex = 76;
            this.autosearchcontainers.Text = "Auto search new containers";
            this.autosearchcontainers.CheckedChanged += new System.EventHandler(this.autosearchcontainers_CheckedChanged);
            #endregion
            #region Visual Group
            // 
            // showmobileGroupBox
            // 
            this.showmobileGroupBox.Location = new System.Drawing.Point(426, 110);
            this.showmobileGroupBox.Name = "showmobileGroupBox";
            this.showmobileGroupBox.Size = new System.Drawing.Size(230, 125);
            this.showmobileGroupBox.TabIndex = 80;
            this.showmobileGroupBox.TabStop = false;
            this.showmobileGroupBox.Text = "Visual";
            // 
            // incomingMob
            // 
            this.incomingMob.Location = new System.Drawing.Point(436, 125);
            this.incomingMob.Name = "incomingMob";
            this.incomingMob.Size = new System.Drawing.Size(200, 22);
            this.incomingMob.TabIndex = 47;
            this.incomingMob.Text = "Show Names of New/Inc Mobiles";
            this.incomingMob.CheckedChanged += new System.EventHandler(this.incomingMob_CheckedChanged);
            // 
            // incomingCorpse
            // 
            this.incomingCorpse.Location = new System.Drawing.Point(436, 146);
            this.incomingCorpse.Name = "incomingCorpse";
            this.incomingCorpse.Size = new System.Drawing.Size(200, 22);
            this.incomingCorpse.TabIndex = 48;
            this.incomingCorpse.Text = "Show Names of New/Inc Corpses";
            this.incomingCorpse.CheckedChanged += new System.EventHandler(this.incomingCorpse_CheckedChanged);
            // 
            // showHealthOH
            // 
            this.showHealthOH.Location = new System.Drawing.Point(436, 164);
            this.showHealthOH.Name = "showHealthOH";
            this.showHealthOH.Size = new System.Drawing.Size(202, 22);
            this.showHealthOH.TabIndex = 69;
            this.showHealthOH.Text = "Show health above Mobiles";
            this.showHealthOH.CheckedChanged += new System.EventHandler(this.showHealthOH_CheckedChanged);
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(453, 189);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(80, 17);
            this.label10.TabIndex = 73;
            this.label10.Text = "Health Format:";
            // 
            // healthFmt
            // 
            this.healthFmt.BackColor = System.Drawing.Color.White;
            this.healthFmt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.healthFmt.Location = new System.Drawing.Point(537, 186);
            this.healthFmt.Name = "healthFmt";
            this.healthFmt.Size = new System.Drawing.Size(112, 20);
            this.healthFmt.TabIndex = 71;
            this.healthFmt.TextChanged += new System.EventHandler(this.healthFmt_TextChanged);
            // 
            // chkPartyOverhead
            // 
            this.chkPartyOverhead.Location = new System.Drawing.Point(436, 207);
            this.chkPartyOverhead.Name = "chkPartyOverhead";
            this.chkPartyOverhead.Size = new System.Drawing.Size(206, 21);
            this.chkPartyOverhead.TabIndex = 72;
            this.chkPartyOverhead.Text = "Show party members mana/stam";
            this.chkPartyOverhead.CheckedChanged += new System.EventHandler(this.chkPartyOverhead_CheckedChanged);
            #endregion
            #region Stealth
            // 
            // stealthGroupBox
            // 
            this.stealthGroupBox.Location = new System.Drawing.Point(210, 195);
            this.stealthGroupBox.Name = "stealthGroupBox";
            this.stealthGroupBox.Size = new System.Drawing.Size(210, 65);
            this.stealthGroupBox.TabIndex = 80;
            this.stealthGroupBox.TabStop = false;
            this.stealthGroupBox.Text = "Stealth";
            // 
            // chkStealth
            // 
            this.chkStealth.Location = new System.Drawing.Point(214, 210);
            this.chkStealth.Name = "chkStealth";
            this.chkStealth.Size = new System.Drawing.Size(190, 22);
            this.chkStealth.TabIndex = 12;
            this.chkStealth.Text = "Count stealth steps";
            this.chkStealth.CheckedChanged += new System.EventHandler(this.chkStealth_CheckedChanged);
            // 
            // chknorunStealth
            // 
            this.chknorunStealth.Location = new System.Drawing.Point(214, 230);
            this.chknorunStealth.Name = "chknorunStealth";
            this.chknorunStealth.Size = new System.Drawing.Size(190, 22);
            this.chknorunStealth.TabIndex = 78;
            this.chknorunStealth.Text = "Block run if Stealthed";
            this.chknorunStealth.CheckedChanged += new System.EventHandler(this.chknorunStealth_CheckedChanged);
            #endregion
            #region Miscellaneous Group
            // 
            // miscellaneousGroupBox
            // 
            this.miscellaneousGroupBox.Location = new System.Drawing.Point(210, 260);
            this.miscellaneousGroupBox.Name = "miscellaneousGroupBox";
            this.miscellaneousGroupBox.Size = new System.Drawing.Size(210, 100);
            this.miscellaneousGroupBox.TabIndex = 80;
            this.miscellaneousGroupBox.TabStop = false;
            this.miscellaneousGroupBox.Text = "Miscellaneous";
            // 
            // blockDis
            // 
            this.blockDis.Location = new System.Drawing.Point(214, 275);
            this.blockDis.Name = "blockDis";
            this.blockDis.Size = new System.Drawing.Size(184, 22);
            this.blockDis.TabIndex = 55;
            this.blockDis.Text = "Block dismount in war mode";
            this.blockDis.CheckedChanged += new System.EventHandler(this.blockDis_CheckedChanged);
            // 
            // autoStackRes
            // 
            this.autoStackRes.Location = new System.Drawing.Point(214, 295);
            this.autoStackRes.Name = "autoStackRes";
            this.autoStackRes.Size = new System.Drawing.Size(198, 22);
            this.autoStackRes.TabIndex = 35;
            this.autoStackRes.Text = "Auto-Stack Ore/Fish/Logs at Feet";
            this.autoStackRes.CheckedChanged += new System.EventHandler(this.autoStackRes_CheckedChanged);
            // 
            // autoOpenDoors
            // 
            this.autoOpenDoors.Location = new System.Drawing.Point(214, 315);
            this.autoOpenDoors.Name = "autoOpenDoors";
            this.autoOpenDoors.Size = new System.Drawing.Size(190, 22);
            this.autoOpenDoors.TabIndex = 59;
            this.autoOpenDoors.Text = "Automatically open doors";
            this.autoOpenDoors.CheckedChanged += new System.EventHandler(this.autoOpenDoors_CheckedChanged);
            // 
            // hiddedAutoOpenDoors
            // 
            this.hiddedAutoOpenDoors.Location = new System.Drawing.Point(232, 335);
            this.hiddedAutoOpenDoors.Name = "hiddedAutoOpenDoors";
            this.hiddedAutoOpenDoors.Size = new System.Drawing.Size(170, 22);
            this.hiddedAutoOpenDoors.TabIndex = 74;
            this.hiddedAutoOpenDoors.Text = "Disable if hidden";
            this.hiddedAutoOpenDoors.CheckedChanged += new System.EventHandler(this.hiddedAutoOpenDoors_CheckedChanged);
            #endregion
            #region Spell / Potion Group
            // 
            // spellspotionsGroupBox
            // 
            this.spellspotionsGroupBox.Location = new System.Drawing.Point(426, 235);
            this.spellspotionsGroupBox.Name = "spellspotionsGroupBox";
            this.spellspotionsGroupBox.Size = new System.Drawing.Size(230, 80);
            this.spellspotionsGroupBox.TabIndex = 80;
            this.spellspotionsGroupBox.TabStop = false;
            this.spellspotionsGroupBox.Text = "Spells / Potions";
            // 
            // potionEquip
            // 
            this.potionEquip.Location = new System.Drawing.Point(436, 252);
            this.potionEquip.Name = "potionEquip";
            this.potionEquip.Size = new System.Drawing.Size(214, 22);
            this.potionEquip.TabIndex = 67;
            this.potionEquip.Text = "Auto Un/Re-equip hands for potions";
            this.potionEquip.CheckedChanged += new System.EventHandler(this.potionEquip_CheckedChanged);
            // 
            // spellUnequip
            // 
            this.spellUnequip.Location = new System.Drawing.Point(436, 272);
            this.spellUnequip.Name = "spellUnequip";
            this.spellUnequip.Size = new System.Drawing.Size(214, 22);
            this.spellUnequip.TabIndex = 39;
            this.spellUnequip.Text = "Auto Unequip hands before casting";
            this.spellUnequip.CheckedChanged += new System.EventHandler(this.spellUnequip_CheckedChanged);
            // 
            // druidClericPackets
            // 
            this.druidClericPackets.AutoSize = true;
            this.druidClericPackets.Location = new System.Drawing.Point(436, 292);
            this.druidClericPackets.Name = "druidClericPackets";
            this.druidClericPackets.Size = new System.Drawing.Size(192, 18);
            this.druidClericPackets.TabIndex = 83;
            this.druidClericPackets.Text = "Use packets for Druid/Cleric spells";
            this.druidClericPackets.UseVisualStyleBackColor = true;
            this.druidClericPackets.CheckedChanged += new System.EventHandler(this.druidClericPackets_CheckedChanged);
            #endregion
            #region Status Gump Group
            // 
            // preaosstatusGroupBox
            // 
            this.preaosstatusGroupBox.Location = new System.Drawing.Point(426, 315);
            this.preaosstatusGroupBox.Name = "spellspotionsGroupBox";
            this.preaosstatusGroupBox.Size = new System.Drawing.Size(230, 45);
            this.preaosstatusGroupBox.TabIndex = 80;
            this.preaosstatusGroupBox.TabStop = false;
            this.preaosstatusGroupBox.Text = "Status Window";
            // 
            // preAOSstatbar
            // 
            this.preAOSstatbar.Location = new System.Drawing.Point(436, 332);
            this.preAOSstatbar.Name = "preAOSstatbar";
            this.preAOSstatbar.Size = new System.Drawing.Size(190, 22);
            this.preAOSstatbar.TabIndex = 57;
            this.preAOSstatbar.Text = "Use Pre-AOS status window";
            this.preAOSstatbar.CheckedChanged += new System.EventHandler(this.preAOSstatbar_CheckedChanged);
            #endregion
            #region Map Integration Group
            // 
            // groupBox17
            // 
            this.groupBox17.Controls.Add(this.setpathmapbutton);
            this.groupBox17.Controls.Add(this.label72);
            this.groupBox17.Controls.Add(this.enhancedmappathTextBox);
            this.groupBox17.Location = new System.Drawing.Point(2, 302);
            this.groupBox17.Name = "groupBox17";
            this.groupBox17.Size = new System.Drawing.Size(200, 58);
            this.groupBox17.TabIndex = 80;
            this.groupBox17.TabStop = false;
            this.groupBox17.Text = "Map Integration";
            // 
            // setpathmapbutton
            // 
            this.setpathmapbutton.BackgroundImage = global::Assistant.Properties.Resources.document_open_7;
            this.setpathmapbutton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.setpathmapbutton.Location = new System.Drawing.Point(177, 22);
            this.setpathmapbutton.Name = "setpathmapbutton";
            this.setpathmapbutton.Size = new System.Drawing.Size(20, 20);
            this.setpathmapbutton.TabIndex = 10;
            this.setpathmapbutton.UseVisualStyleBackColor = true;
            this.setpathmapbutton.Click += new System.EventHandler(this.setpathmapbutton_Click);
            // 
            // label72
            // 
            this.label72.AutoSize = true;
            this.label72.Location = new System.Drawing.Point(6, 25);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(31, 14);
            this.label72.TabIndex = 1;
            this.label72.Text = "Path:";
            // 
            // enhancedmappathTextBox
            // 
            this.enhancedmappathTextBox.Location = new System.Drawing.Point(41, 22);
            this.enhancedmappathTextBox.Name = "enhancedmappathTextBox";
            this.enhancedmappathTextBox.ReadOnly = true;
            this.enhancedmappathTextBox.Size = new System.Drawing.Size(130, 20);
            this.enhancedmappathTextBox.TabIndex = 0;
            #endregion
            #region Razor Messages Group
            // 
            // razormessagesGroupBox
            // 
            this.razormessagesGroupBox.Location = new System.Drawing.Point(2, 225);
            this.razormessagesGroupBox.Name = "razormessagesGroupBox";
            this.razormessagesGroupBox.Size = new System.Drawing.Size(200, 75);
            this.razormessagesGroupBox.TabIndex = 80;
            this.razormessagesGroupBox.TabStop = false;
            this.razormessagesGroupBox.Text = "Warnings and Errors";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(7, 258);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(92, 18);
            this.label17.TabIndex = 68;
            this.label17.Text = "TM Razor messages:";
            // 
            // msglvl
            // 
            this.msglvl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.msglvl.Font = new System.Drawing.Font("Arial", 9F);
            this.msglvl.Items.AddRange(new object[] {
            "Show All",
            "Warnings & Errors",
            "Errors Only",
            "None"});
            this.msglvl.Location = new System.Drawing.Point(106, 254);
            this.msglvl.Name = "msglvl";
            this.msglvl.Size = new System.Drawing.Size(88, 23);
            this.msglvl.TabIndex = 69;
            this.msglvl.SelectedIndexChanged += new System.EventHandler(this.msglvl_SelectedIndexChanged);
            #endregion
            #endregion
            #region Filters Tab 
            #region Tab Page
            // 
            // enhancedFilterTab
            // 
            this.enhancedFilterTab.Controls.Add(this.FilterPages);
            this.enhancedFilterTab.Location = new System.Drawing.Point(4, 54);
            this.enhancedFilterTab.Name = "enhancedFilterTab";
            this.enhancedFilterTab.Size = new System.Drawing.Size(678, 365);
            this.enhancedFilterTab.TabIndex = 10;
            this.enhancedFilterTab.Text = "Filters";
            this.enhancedFilterTab.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            // 
            // FilterPages
            // 
            this.FilterPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterPages.Controls.Add(this.JournalFilterPage);
            this.FilterPages.Controls.Add(this.targettingTab);
            this.FilterPages.Controls.Add(this.MiscFilterPage);
            this.FilterPages.Location = new System.Drawing.Point(-2, 2);
            this.FilterPages.Name = "FilterPages";
            this.FilterPages.SelectedIndex = 0;
            this.FilterPages.Size = new System.Drawing.Size(657, 371);
            this.FilterPages.TabIndex = 0;
            this.FilterPages.TabIndex = 0;
            #endregion
            #region Virtual Tab 
            #region Filters Group
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.filters);
            this.groupBox1.Location = new System.Drawing.Point(3, 0);
            //this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 355);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // filters
            // 
            this.filters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filters.CheckOnClick = true;
            this.filters.IntegralHeight = false;
            this.filters.Location = new System.Drawing.Point(6, 85);
            this.filters.Name = "filters";
            this.filters.Size = new System.Drawing.Size(162, 261);
            this.filters.TabIndex = 0;
            this.filters.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnFilterCheck);
            // 
            // morefilterLabel
            // 
            this.morefilterLabel.Location = new System.Drawing.Point(8, 67); //251 189
            this.morefilterLabel.Name = "morefilterLabel";
            this.morefilterLabel.Size = new System.Drawing.Size(100, 15);
            this.morefilterLabel.TabIndex = 59;
            this.morefilterLabel.Text = "More Filters:";
            // 
            // filterNPC
            // 
            this.filterNPC.Location = new System.Drawing.Point(12, 48);
            this.filterNPC.Name = "filterNPC";
            this.filterNPC.Size = new System.Drawing.Size(158, 17);
            this.filterNPC.TabIndex = 81;
            this.filterNPC.Text = "Monster Messages";
            this.m_Tip.SetToolTip(this.filterNPC, "Orc / Lizardmen / Ratmen");
            this.filterNPC.CheckedChanged += new System.EventHandler(this.filterNPC_CheckedChanged);
            // 
            // filterPoison
            // 
            this.filterPoison.Location = new System.Drawing.Point(12, 31);
            this.filterPoison.Name = "filterPoison";
            this.filterPoison.Size = new System.Drawing.Size(158, 17);
            this.filterPoison.TabIndex = 79;
            this.filterPoison.Text = "Poison Messages";
            this.filterPoison.CheckedChanged += new System.EventHandler(this.filterPoison_CheckedChanged);
            // 
            // filterSnoop
            // 
            this.filterSnoop.Location = new System.Drawing.Point(12, 14);
            this.filterSnoop.Name = "filterSnoop";
            this.filterSnoop.Size = new System.Drawing.Size(158, 17);
            this.filterSnoop.TabIndex = 49;
            this.filterSnoop.Text = "Snooping Messages";
            this.filterSnoop.CheckedChanged += new System.EventHandler(this.filterSnoop_CheckedChanged);
            // 
            // spamFilter
            // 
            this.spamFilter.Location = new System.Drawing.Point(12, -3);
            this.spamFilter.Name = "spamFilter";
            this.spamFilter.Size = new System.Drawing.Size(158, 17);
            this.spamFilter.TabIndex = 2;
            this.spamFilter.Text = "Repeating Sys Messages";
            this.spamFilter.CheckedChanged += new System.EventHandler(this.spamFilter_CheckedChanged);
            #endregion
            #region Juounal Group
            // 
            // JournalFilterPage
            // 
            this.JournalFilterPage.Controls.Add(this.journalfilterdatagrid);
            this.JournalFilterPage.Controls.Add(this.spamFilter);
            this.JournalFilterPage.Controls.Add(this.filterSnoop);
            this.JournalFilterPage.Controls.Add(this.filterPoison);
            this.JournalFilterPage.Controls.Add(this.filterNPC);
            this.JournalFilterPage.Controls.Add(this.morefilterLabel);
            this.JournalFilterPage.Controls.Add(this.groupBox1);
            this.JournalFilterPage.Location = new System.Drawing.Point(4, 22);
            this.JournalFilterPage.Name = "JournalFilterPage";
            //this.JournalFilterPage.Padding = new System.Windows.Forms.Padding(3);
            this.JournalFilterPage.Size = new System.Drawing.Size(678, 365);
            this.JournalFilterPage.TabIndex = 1;
            this.JournalFilterPage.Text = "Virtual";
            this.JournalFilterPage.UseVisualStyleBackColor = true;
            // 
            // journalfilterdatagrid
            // 
            this.journalfilterdatagrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.journalfilterdatagrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.journalfilterdatagrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.journalFilterText});
            this.journalfilterdatagrid.Location = new System.Drawing.Point(196, 4);
            this.journalfilterdatagrid.Name = "journalfilterdatagrid";
            this.journalfilterdatagrid.RowHeadersVisible = false;
            this.journalfilterdatagrid.RowHeadersWidth = 62;
            this.journalfilterdatagrid.RowTemplate.Height = 28;
            this.journalfilterdatagrid.Size = new System.Drawing.Size(471, 350);
            this.journalfilterdatagrid.TabIndex = 0;
            this.journalfilterdatagrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.journalfilterdatagrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.journalfilterdatagrid_CellEndEdit);
            this.journalfilterdatagrid.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.journalfilterdatagrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.journalfilterdatagrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.journalfilterdatagrid.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.journalfilterdatagrid_DefaultValuesNeeded);
            this.journalfilterdatagrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.journalfilterdatagrid.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.journalfilterdatagrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.journalfilterdatagrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // journalFilterText
            // 
            this.journalFilterText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.journalFilterText.ContextMenuStrip = this.datagridMenuStrip;
            this.journalFilterText.HeaderText = "Journal Filter Text";
            this.journalFilterText.MinimumWidth = 8;
            this.journalFilterText.Name = "journalFilterText";
            // 
            // datagridMenuStrip
            // 
            this.datagridMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.datagridMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteRowToolStripMenuItem});
            this.datagridMenuStrip.Name = "datagridMenuStrip";
            this.datagridMenuStrip.Size = new System.Drawing.Size(134, 26);
            this.datagridMenuStrip.Click += new System.EventHandler(this.datagridMenuStrip_Click);
            // 
            // deleteRowToolStripMenuItem
            // 
            this.deleteRowToolStripMenuItem.Name = "deleteRowToolStripMenuItem";
            this.deleteRowToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.deleteRowToolStripMenuItem.Text = "Delete Row";
            #endregion
            #endregion
            #region Targetting Tab
            // 
            // targettingTab
            // 
            this.targettingTab.Controls.Add(this.targetTestButton);
            this.targettingTab.Controls.Add(this.targetsaveButton);
            this.targettingTab.Controls.Add(this.groupBox57);
            this.targettingTab.Controls.Add(this.groupBox56);
            this.targettingTab.Controls.Add(this.groupBox55);
            this.targettingTab.Controls.Add(this.groupBox48);
            this.targettingTab.Controls.Add(this.groupBox46);
            this.targettingTab.Controls.Add(this.groupBox45);
            this.targettingTab.Controls.Add(this.groupBox44);
            this.targettingTab.Controls.Add(this.groupBox43);
            this.targettingTab.Location = new System.Drawing.Point(4, 54);
            this.targettingTab.Name = "targettingTab";
            this.targettingTab.Size = new System.Drawing.Size(678, 365);
            this.targettingTab.TabIndex = 3;
            this.targettingTab.Text = "Targetting";
            #region Shortcut Group
            // 
            // groupBox43
            // 
            this.groupBox43.Controls.Add(this.label76);
            this.groupBox43.Controls.Add(this.targetremoveButton);
            this.groupBox43.Controls.Add(this.targetaddButton);
            this.groupBox43.Controls.Add(this.targetaddTextBox);
            this.groupBox43.Controls.Add(this.targetlistBox);
            this.groupBox43.Location = new System.Drawing.Point(8, 6);
            this.groupBox43.Name = "groupBox43";
            this.groupBox43.Size = new System.Drawing.Size(126, 355);
            this.groupBox43.TabIndex = 49;
            this.groupBox43.TabStop = false;
            this.groupBox43.Text = "Shortcut";
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Location = new System.Drawing.Point(6, 22);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(37, 14);
            this.label76.TabIndex = 5;
            this.label76.Text = "Name:";
            // 
            // targetremoveButton
            // 
            this.targetremoveButton.Location = new System.Drawing.Point(7, 45);
            this.targetremoveButton.Name = "targetremoveButton";
            this.targetremoveButton.Size = new System.Drawing.Size(55, 23);
            this.targetremoveButton.TabIndex = 5;
            this.targetremoveButton.Text = "Remove";
            this.targetremoveButton.UseVisualStyleBackColor = true;
            this.targetremoveButton.Click += new System.EventHandler(this.targetremoveButton_Click);
            // 
            // targetaddButton
            // 
            this.targetaddButton.Location = new System.Drawing.Point(65, 45);
            this.targetaddButton.Name = "targetaddButton";
            this.targetaddButton.Size = new System.Drawing.Size(55, 23);
            this.targetaddButton.TabIndex = 4;
            this.targetaddButton.Text = "Add";
            this.targetaddButton.UseVisualStyleBackColor = true;
            this.targetaddButton.Click += new System.EventHandler(this.targetaddButton_Click);
            // 
            // targetaddTextBox
            // 
            this.targetaddTextBox.Location = new System.Drawing.Point(44, 19);
            this.targetaddTextBox.Name = "targetaddTextBox";
            this.targetaddTextBox.Size = new System.Drawing.Size(76, 20);
            this.targetaddTextBox.TabIndex = 1;
            // 
            // targetlistBox
            // 
            this.targetlistBox.FormattingEnabled = true;
            this.targetlistBox.ItemHeight = 14;
            this.targetlistBox.Location = new System.Drawing.Point(7, 72);
            this.targetlistBox.Name = "targetlistBox";
            this.targetlistBox.Size = new System.Drawing.Size(113, 130);
            this.targetlistBox.TabIndex = 0;
            this.targetlistBox.SelectedIndexChanged += new System.EventHandler(this.targetlistBox_SelectedIndexChanged);
            #endregion
            #region Body Filter Group
            // 
            // groupBox44
            // 
            this.groupBox44.Controls.Add(this.targetChoseBody);
            this.groupBox44.Controls.Add(this.targetbodydataGridView);
            this.groupBox44.Controls.Add(this.targetbodyCheckBox);
            this.groupBox44.Location = new System.Drawing.Point(140, 6);
            this.groupBox44.Name = "groupBox44";
            this.groupBox44.Size = new System.Drawing.Size(111, 313);
            this.groupBox44.TabIndex = 50;
            this.groupBox44.TabStop = false;
            this.groupBox44.Text = "Body Filter";
            // 
            // targetChoseBody
            // 
            this.targetChoseBody.Location = new System.Drawing.Point(7, 284);
            this.targetChoseBody.Name = "targetChoseBody";
            this.targetChoseBody.Size = new System.Drawing.Size(95, 23);
            this.targetChoseBody.TabIndex = 58;
            this.targetChoseBody.Text = "Target Body ID";
            this.targetChoseBody.UseVisualStyleBackColor = true;
            this.targetChoseBody.Click += new System.EventHandler(this.targetChoseBody_Click);
            // 
            // targetbodydataGridView
            // 
            this.targetbodydataGridView.AllowDrop = true;
            this.targetbodydataGridView.AllowUserToResizeRows = false;
            this.targetbodydataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.targetbodydataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn20});
            this.targetbodydataGridView.Location = new System.Drawing.Point(7, 43);
            this.targetbodydataGridView.Name = "targetbodydataGridView";
            this.targetbodydataGridView.RowHeadersVisible = false;
            this.targetbodydataGridView.RowHeadersWidth = 62;
            this.targetbodydataGridView.Size = new System.Drawing.Size(95, 233);
            this.targetbodydataGridView.TabIndex = 70;
            this.targetbodydataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.targetbodydataGridView_CellEndEdit);
            this.targetbodydataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.targetbodydataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.targetbodydataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.targetbodydataGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.targetfilter_DefaultValuesNeeded);
            this.targetbodydataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.targetbodydataGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.targetbodydataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.targetbodydataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // dataGridViewTextBoxColumn20
            // 
            this.dataGridViewTextBoxColumn20.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn20.HeaderText = "Body ID";
            this.dataGridViewTextBoxColumn20.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn20.Name = "dataGridViewTextBoxColumn20";
            this.dataGridViewTextBoxColumn20.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn20.Width = 75;
            // 
            // targetbodyCheckBox
            // 
            this.targetbodyCheckBox.AutoSize = true;
            this.targetbodyCheckBox.Location = new System.Drawing.Point(7, 20);
            this.targetbodyCheckBox.Name = "targetbodyCheckBox";
            this.targetbodyCheckBox.Size = new System.Drawing.Size(58, 18);
            this.targetbodyCheckBox.TabIndex = 0;
            this.targetbodyCheckBox.Text = "Enable";
            this.targetbodyCheckBox.UseVisualStyleBackColor = true;
            this.targetbodyCheckBox.CheckedChanged += new System.EventHandler(this.targetbodyCheckBox_CheckedChanged);
            #endregion
            #region Color Filter Group
            // 
            // groupBox45
            // 
            this.groupBox45.Controls.Add(this.targetChoseHue);
            this.groupBox45.Controls.Add(this.targethueGridView);
            this.groupBox45.Controls.Add(this.targetcoloCheckBox);
            this.groupBox45.Location = new System.Drawing.Point(257, 6);
            this.groupBox45.Name = "groupBox45";
            this.groupBox45.Size = new System.Drawing.Size(111, 313);
            this.groupBox45.TabIndex = 51;
            this.groupBox45.TabStop = false;
            this.groupBox45.Text = "Color Filter";
            // 
            // targetChoseHue
            // 
            this.targetChoseHue.Location = new System.Drawing.Point(7, 284);
            this.targetChoseHue.Name = "targetChoseHue";
            this.targetChoseHue.Size = new System.Drawing.Size(95, 23);
            this.targetChoseHue.TabIndex = 71;
            this.targetChoseHue.Text = "Target Hue ID";
            this.targetChoseHue.UseVisualStyleBackColor = true;
            this.targetChoseHue.Click += new System.EventHandler(this.targetChoseHue_Click);
            // 
            // targethueGridView
            // 
            this.targethueGridView.AllowDrop = true;
            this.targethueGridView.AllowUserToResizeRows = false;
            this.targethueGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.targethueGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn21});
            this.targethueGridView.Location = new System.Drawing.Point(7, 43);
            this.targethueGridView.Name = "targethueGridView";
            this.targethueGridView.RowHeadersVisible = false;
            this.targethueGridView.RowHeadersWidth = 62;
            this.targethueGridView.Size = new System.Drawing.Size(95, 233);
            this.targethueGridView.TabIndex = 70;
            this.targethueGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.targethueGridView_CellEndEdit);
            this.targethueGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_MouseDown);
            this.targethueGridView.CellMouseMove += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_MouseMove);
            this.targethueGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.targethueGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.targethueGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.targethueGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.targetfilter_DefaultValuesNeeded);
            this.targethueGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.targethueGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            // 
            // dataGridViewTextBoxColumn21
            // 
            this.dataGridViewTextBoxColumn21.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn21.HeaderText = "Hue";
            this.dataGridViewTextBoxColumn21.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn21.Name = "dataGridViewTextBoxColumn21";
            this.dataGridViewTextBoxColumn21.Width = 75;
            // 
            // targetcoloCheckBox
            // 
            this.targetcoloCheckBox.AutoSize = true;
            this.targetcoloCheckBox.Location = new System.Drawing.Point(7, 20);
            this.targetcoloCheckBox.Name = "targetcoloCheckBox";
            this.targetcoloCheckBox.Size = new System.Drawing.Size(58, 18);
            this.targetcoloCheckBox.TabIndex = 0;
            this.targetcoloCheckBox.Text = "Enable";
            this.targetcoloCheckBox.UseVisualStyleBackColor = true;
            this.targetcoloCheckBox.CheckedChanged += new System.EventHandler(this.targetcoloCheckBox_CheckedChanged);
            #endregion
            #region Range Group  
            // 
            // groupBox48
            // 
            this.groupBox48.Controls.Add(this.label73);
            this.groupBox48.Controls.Add(this.label74);
            this.groupBox48.Controls.Add(this.targetRangeMaxTextBox);
            this.groupBox48.Controls.Add(this.label75);
            this.groupBox48.Controls.Add(this.targetRangeMinTextBox);
            this.groupBox48.Location = new System.Drawing.Point(374, 6);
            this.groupBox48.Name = "groupBox48";
            this.groupBox48.Size = new System.Drawing.Size(101, 94);
            this.groupBox48.TabIndex = 53;
            this.groupBox48.TabStop = false;
            this.groupBox48.Text = "Range";
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Location = new System.Drawing.Point(9, 77);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(88, 14);
            this.label73.TabIndex = 4;
            this.label73.Text = "Set -1 for no limit";
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Location = new System.Drawing.Point(9, 48);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(30, 14);
            this.label74.TabIndex = 3;
            this.label74.Text = "Max:";
            // 
            // targetRangeMaxTextBox
            // 
            this.targetRangeMaxTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetRangeMaxTextBox.BackColor = System.Drawing.Color.White;
            this.targetRangeMaxTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetRangeMaxTextBox.Location = new System.Drawing.Point(43, 45);
            this.targetRangeMaxTextBox.Name = "targetRangeMaxTextBox";
            this.targetRangeMaxTextBox.Size = new System.Drawing.Size(47, 20);
            this.targetRangeMaxTextBox.TabIndex = 2;
            this.targetRangeMaxTextBox.Text = "-1";
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Location = new System.Drawing.Point(9, 22);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(26, 14);
            this.label75.TabIndex = 1;
            this.label75.Text = "Min:";
            // 
            // targetRangeMinTextBox
            // 
            this.targetRangeMinTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetRangeMinTextBox.BackColor = System.Drawing.Color.White;
            this.targetRangeMinTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetRangeMinTextBox.Location = new System.Drawing.Point(43, 19);
            this.targetRangeMinTextBox.Name = "targetRangeMinTextBox";
            this.targetRangeMinTextBox.Size = new System.Drawing.Size(47, 20);
            this.targetRangeMinTextBox.TabIndex = 0;
            this.targetRangeMinTextBox.Text = "-1";
            #endregion
            #region Notoriety Color Group
            // 
            // groupBox57
            // 
            this.groupBox57.Controls.Add(this.targetYellowCheckBox);
            this.groupBox57.Controls.Add(this.targetRedCheckBox);
            this.groupBox57.Controls.Add(this.targetOrangeCheckBox);
            this.groupBox57.Controls.Add(this.targetCriminalCheckBox);
            this.groupBox57.Controls.Add(this.targetGreyCheckBox);
            this.groupBox57.Controls.Add(this.targetGreenCheckBox);
            this.groupBox57.Controls.Add(this.targetBlueCheckBox);
            this.groupBox57.Location = new System.Drawing.Point(375, 110);
            this.groupBox57.Name = "groupBox57";
            this.groupBox57.Size = new System.Drawing.Size(100, 157);
            this.groupBox57.TabIndex = 56;
            this.groupBox57.TabStop = false;
            this.groupBox57.Text = "Notoriety Color";
            // 
            // targetYellowCheckBox
            // 
            this.targetYellowCheckBox.AutoSize = true;
            this.targetYellowCheckBox.ForeColor = System.Drawing.Color.DarkSalmon;
            this.targetYellowCheckBox.Location = new System.Drawing.Point(6, 134);
            this.targetYellowCheckBox.Name = "targetYellowCheckBox";
            this.targetYellowCheckBox.Size = new System.Drawing.Size(59, 18);
            this.targetYellowCheckBox.TabIndex = 77;
            this.targetYellowCheckBox.Text = "Yellow";
            this.targetYellowCheckBox.UseVisualStyleBackColor = true;
            // 
            // targetRedCheckBox
            // 
            this.targetRedCheckBox.AutoSize = true;
            this.targetRedCheckBox.ForeColor = System.Drawing.Color.Red;
            this.targetRedCheckBox.Location = new System.Drawing.Point(6, 114);
            this.targetRedCheckBox.Name = "targetRedCheckBox";
            this.targetRedCheckBox.Size = new System.Drawing.Size(45, 18);
            this.targetRedCheckBox.TabIndex = 76;
            this.targetRedCheckBox.Text = "Red";
            this.targetRedCheckBox.UseVisualStyleBackColor = true;
            // 
            // targetOrangeCheckBox
            // 
            this.targetOrangeCheckBox.AutoSize = true;
            this.targetOrangeCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.targetOrangeCheckBox.Location = new System.Drawing.Point(6, 95);
            this.targetOrangeCheckBox.Name = "targetOrangeCheckBox";
            this.targetOrangeCheckBox.Size = new System.Drawing.Size(62, 18);
            this.targetOrangeCheckBox.TabIndex = 75;
            this.targetOrangeCheckBox.Text = "Orange";
            this.targetOrangeCheckBox.UseVisualStyleBackColor = true;
            // 
            // targetCriminalCheckBox
            // 
            this.targetCriminalCheckBox.AutoSize = true;
            this.targetCriminalCheckBox.ForeColor = System.Drawing.Color.DimGray;
            this.targetCriminalCheckBox.Location = new System.Drawing.Point(6, 76);
            this.targetCriminalCheckBox.Name = "targetCriminalCheckBox";
            this.targetCriminalCheckBox.Size = new System.Drawing.Size(91, 18);
            this.targetCriminalCheckBox.TabIndex = 74;
            this.targetCriminalCheckBox.Text = "Grey (Aggro)";
            this.targetCriminalCheckBox.UseVisualStyleBackColor = true;
            // 
            // targetGreyCheckBox
            // 
            this.targetGreyCheckBox.AutoSize = true;
            this.targetGreyCheckBox.ForeColor = System.Drawing.Color.DimGray;
            this.targetGreyCheckBox.Location = new System.Drawing.Point(6, 57);
            this.targetGreyCheckBox.Name = "targetGreyCheckBox";
            this.targetGreyCheckBox.Size = new System.Drawing.Size(50, 18);
            this.targetGreyCheckBox.TabIndex = 73;
            this.targetGreyCheckBox.Text = "Grey";
            this.targetGreyCheckBox.UseVisualStyleBackColor = true;
            // 
            // targetGreenCheckBox
            // 
            this.targetGreenCheckBox.AutoSize = true;
            this.targetGreenCheckBox.ForeColor = System.Drawing.Color.Green;
            this.targetGreenCheckBox.Location = new System.Drawing.Point(6, 38);
            this.targetGreenCheckBox.Name = "targetGreenCheckBox";
            this.targetGreenCheckBox.Size = new System.Drawing.Size(56, 18);
            this.targetGreenCheckBox.TabIndex = 72;
            this.targetGreenCheckBox.Text = "Green";
            this.targetGreenCheckBox.UseVisualStyleBackColor = true;
            // 
            // targetBlueCheckBox
            // 
            this.targetBlueCheckBox.AutoSize = true;
            this.targetBlueCheckBox.ForeColor = System.Drawing.Color.Blue;
            this.targetBlueCheckBox.Location = new System.Drawing.Point(6, 19);
            this.targetBlueCheckBox.Name = "targetBlueCheckBox";
            this.targetBlueCheckBox.Size = new System.Drawing.Size(47, 18);
            this.targetBlueCheckBox.TabIndex = 71;
            this.targetBlueCheckBox.Text = "Blue";
            this.targetBlueCheckBox.UseVisualStyleBackColor = true;
            #endregion
            #region Flags Group
            // 
            // groupBox46
            // 
            this.groupBox46.Controls.Add(this.groupBox47);
            this.groupBox46.Controls.Add(this.groupBox49);
            this.groupBox46.Controls.Add(this.groupBox50);
            this.groupBox46.Controls.Add(this.groupBox51);
            this.groupBox46.Controls.Add(this.groupBox52);
            this.groupBox46.Controls.Add(this.groupBox53);
            this.groupBox46.Controls.Add(this.groupBox54);
            this.groupBox46.Location = new System.Drawing.Point(481, 6);
            this.groupBox46.Name = "groupBox46";
            this.groupBox46.Size = new System.Drawing.Size(180, 311);
            this.groupBox46.TabIndex = 52;
            this.groupBox46.TabStop = false;
            this.groupBox46.Text = "Flags";
            // 
            // groupBox47
            // 
            this.groupBox47.Controls.Add(this.paralizedBoth);
            this.groupBox47.Controls.Add(this.paralizedOff);
            this.groupBox47.Controls.Add(this.paralizedOn);
            this.groupBox47.Location = new System.Drawing.Point(6, 263);
            this.groupBox47.Name = "groupBox47";
            this.groupBox47.Size = new System.Drawing.Size(163, 37);
            this.groupBox47.TabIndex = 97;
            this.groupBox47.TabStop = false;
            this.groupBox47.Text = "Paralized";
            // 
            // paralizedBoth
            // 
            this.paralizedBoth.Checked = true;
            this.paralizedBoth.Location = new System.Drawing.Point(107, 15);
            this.paralizedBoth.Name = "paralizedBoth";
            this.paralizedBoth.Size = new System.Drawing.Size(50, 20);
            this.paralizedBoth.TabIndex = 59;
            this.paralizedBoth.TabStop = true;
            this.paralizedBoth.Text = "Both";
            // 
            // paralizedOff
            // 
            this.paralizedOff.Location = new System.Drawing.Point(60, 15);
            this.paralizedOff.Name = "paralizedOff";
            this.paralizedOff.Size = new System.Drawing.Size(41, 20);
            this.paralizedOff.TabIndex = 58;
            this.paralizedOff.Text = "No";
            // 
            // paralizedOn
            // 
            this.paralizedOn.Location = new System.Drawing.Point(6, 15);
            this.paralizedOn.Name = "paralizedOn";
            this.paralizedOn.Size = new System.Drawing.Size(48, 20);
            this.paralizedOn.TabIndex = 57;
            this.paralizedOn.Text = "Yes";
            // 
            // groupBox49
            // 
            this.groupBox49.Controls.Add(this.friendOn);
            this.groupBox49.Controls.Add(this.friendBoth);
            this.groupBox49.Controls.Add(this.friendOff);
            this.groupBox49.Location = new System.Drawing.Point(6, 222);
            this.groupBox49.Name = "groupBox49";
            this.groupBox49.Size = new System.Drawing.Size(163, 37);
            this.groupBox49.TabIndex = 95;
            this.groupBox49.TabStop = false;
            this.groupBox49.Text = "Friend";
            // 
            // friendOn
            // 
            this.friendOn.Location = new System.Drawing.Point(6, 15);
            this.friendOn.Name = "friendOn";
            this.friendOn.Size = new System.Drawing.Size(48, 20);
            this.friendOn.TabIndex = 65;
            this.friendOn.Text = "Yes";
            // 
            // friendBoth
            // 
            this.friendBoth.Checked = true;
            this.friendBoth.Location = new System.Drawing.Point(107, 15);
            this.friendBoth.Name = "friendBoth";
            this.friendBoth.Size = new System.Drawing.Size(50, 20);
            this.friendBoth.TabIndex = 67;
            this.friendBoth.TabStop = true;
            this.friendBoth.Text = "Both";
            // 
            // friendOff
            // 
            this.friendOff.Location = new System.Drawing.Point(60, 14);
            this.friendOff.Name = "friendOff";
            this.friendOff.Size = new System.Drawing.Size(41, 20);
            this.friendOff.TabIndex = 66;
            this.friendOff.Text = "No";
            // 
            // groupBox50
            // 
            this.groupBox50.Controls.Add(this.warmodeOn);
            this.groupBox50.Controls.Add(this.warmodeBoth);
            this.groupBox50.Controls.Add(this.warmodeOff);
            this.groupBox50.Location = new System.Drawing.Point(6, 179);
            this.groupBox50.Name = "groupBox50";
            this.groupBox50.Size = new System.Drawing.Size(163, 37);
            this.groupBox50.TabIndex = 94;
            this.groupBox50.TabStop = false;
            this.groupBox50.Text = "Warmode";
            // 
            // warmodeOn
            // 
            this.warmodeOn.Location = new System.Drawing.Point(6, 15);
            this.warmodeOn.Name = "warmodeOn";
            this.warmodeOn.Size = new System.Drawing.Size(48, 20);
            this.warmodeOn.TabIndex = 69;
            this.warmodeOn.Text = "Yes";
            // 
            // warmodeBoth
            // 
            this.warmodeBoth.Checked = true;
            this.warmodeBoth.Location = new System.Drawing.Point(107, 15);
            this.warmodeBoth.Name = "warmodeBoth";
            this.warmodeBoth.Size = new System.Drawing.Size(50, 20);
            this.warmodeBoth.TabIndex = 71;
            this.warmodeBoth.TabStop = true;
            this.warmodeBoth.Text = "Both";
            // 
            // warmodeOff
            // 
            this.warmodeOff.Location = new System.Drawing.Point(60, 15);
            this.warmodeOff.Name = "warmodeOff";
            this.warmodeOff.Size = new System.Drawing.Size(41, 20);
            this.warmodeOff.TabIndex = 70;
            this.warmodeOff.Text = "No";
            // 
            // groupBox51
            // 
            this.groupBox51.Controls.Add(this.ghostOn);
            this.groupBox51.Controls.Add(this.ghostBoth);
            this.groupBox51.Controls.Add(this.ghostOff);
            this.groupBox51.Location = new System.Drawing.Point(6, 139);
            this.groupBox51.Name = "groupBox51";
            this.groupBox51.Size = new System.Drawing.Size(163, 37);
            this.groupBox51.TabIndex = 92;
            this.groupBox51.TabStop = false;
            this.groupBox51.Text = "Ghost";
            // 
            // ghostOn
            // 
            this.ghostOn.Location = new System.Drawing.Point(6, 14);
            this.ghostOn.Name = "ghostOn";
            this.ghostOn.Size = new System.Drawing.Size(48, 20);
            this.ghostOn.TabIndex = 73;
            this.ghostOn.Text = "Yes";
            // 
            // ghostBoth
            // 
            this.ghostBoth.Checked = true;
            this.ghostBoth.Location = new System.Drawing.Point(107, 14);
            this.ghostBoth.Name = "ghostBoth";
            this.ghostBoth.Size = new System.Drawing.Size(50, 20);
            this.ghostBoth.TabIndex = 75;
            this.ghostBoth.TabStop = true;
            this.ghostBoth.Text = "Both";
            // 
            // ghostOff
            // 
            this.ghostOff.Location = new System.Drawing.Point(60, 14);
            this.ghostOff.Name = "ghostOff";
            this.ghostOff.Size = new System.Drawing.Size(41, 20);
            this.ghostOff.TabIndex = 74;
            this.ghostOff.Text = "No";
            // 
            // groupBox52
            // 
            this.groupBox52.Controls.Add(this.humanOn);
            this.groupBox52.Controls.Add(this.humanOff);
            this.groupBox52.Controls.Add(this.humanBoth);
            this.groupBox52.Location = new System.Drawing.Point(6, 98);
            this.groupBox52.Name = "groupBox52";
            this.groupBox52.Size = new System.Drawing.Size(163, 37);
            this.groupBox52.TabIndex = 91;
            this.groupBox52.TabStop = false;
            this.groupBox52.Text = "Human";
            // 
            // humanOn
            // 
            this.humanOn.Location = new System.Drawing.Point(6, 15);
            this.humanOn.Name = "humanOn";
            this.humanOn.Size = new System.Drawing.Size(48, 20);
            this.humanOn.TabIndex = 77;
            this.humanOn.Text = "Yes";
            // 
            // humanOff
            // 
            this.humanOff.Location = new System.Drawing.Point(60, 15);
            this.humanOff.Name = "humanOff";
            this.humanOff.Size = new System.Drawing.Size(41, 20);
            this.humanOff.TabIndex = 78;
            this.humanOff.Text = "No";
            // 
            // humanBoth
            // 
            this.humanBoth.Checked = true;
            this.humanBoth.Location = new System.Drawing.Point(107, 15);
            this.humanBoth.Name = "humanBoth";
            this.humanBoth.Size = new System.Drawing.Size(50, 20);
            this.humanBoth.TabIndex = 79;
            this.humanBoth.TabStop = true;
            this.humanBoth.Text = "Both";
            // 
            // groupBox53
            // 
            this.groupBox53.Controls.Add(this.blessedOn);
            this.groupBox53.Controls.Add(this.blessedOff);
            this.groupBox53.Controls.Add(this.blessedBoth);
            this.groupBox53.Location = new System.Drawing.Point(6, 58);
            this.groupBox53.Name = "groupBox53";
            this.groupBox53.Size = new System.Drawing.Size(163, 37);
            this.groupBox53.TabIndex = 90;
            this.groupBox53.TabStop = false;
            this.groupBox53.Text = "Yellow Hits";
            // 
            // blessedOn
            // 
            this.blessedOn.Location = new System.Drawing.Point(6, 14);
            this.blessedOn.Name = "blessedOn";
            this.blessedOn.Size = new System.Drawing.Size(48, 20);
            this.blessedOn.TabIndex = 81;
            this.blessedOn.Text = "Yes";
            // 
            // blessedOff
            // 
            this.blessedOff.Location = new System.Drawing.Point(60, 14);
            this.blessedOff.Name = "blessedOff";
            this.blessedOff.Size = new System.Drawing.Size(41, 20);
            this.blessedOff.TabIndex = 82;
            this.blessedOff.Text = "No";
            // 
            // blessedBoth
            // 
            this.blessedBoth.Checked = true;
            this.blessedBoth.Location = new System.Drawing.Point(107, 14);
            this.blessedBoth.Name = "blessedBoth";
            this.blessedBoth.Size = new System.Drawing.Size(50, 20);
            this.blessedBoth.TabIndex = 83;
            this.blessedBoth.TabStop = true;
            this.blessedBoth.Text = "Both";
            // 
            // groupBox54
            // 
            this.groupBox54.Controls.Add(this.poisonedOn);
            this.groupBox54.Controls.Add(this.poisonedOff);
            this.groupBox54.Controls.Add(this.poisonedBoth);
            this.groupBox54.Location = new System.Drawing.Point(6, 19);
            this.groupBox54.Name = "groupBox54";
            this.groupBox54.Size = new System.Drawing.Size(163, 37);
            this.groupBox54.TabIndex = 89;
            this.groupBox54.TabStop = false;
            this.groupBox54.Text = "Poisoned";
            // 
            // poisonedOn
            // 
            this.poisonedOn.Location = new System.Drawing.Point(6, 13);
            this.poisonedOn.Name = "poisonedOn";
            this.poisonedOn.Size = new System.Drawing.Size(48, 20);
            this.poisonedOn.TabIndex = 85;
            this.poisonedOn.Text = "Yes";
            // 
            // poisonedOff
            // 
            this.poisonedOff.Location = new System.Drawing.Point(60, 13);
            this.poisonedOff.Name = "poisonedOff";
            this.poisonedOff.Size = new System.Drawing.Size(41, 20);
            this.poisonedOff.TabIndex = 86;
            this.poisonedOff.Text = "No";
            // 
            // poisonedBoth
            // 
            this.poisonedBoth.Checked = true;
            this.poisonedBoth.Location = new System.Drawing.Point(107, 13);
            this.poisonedBoth.Name = "poisonedBoth";
            this.poisonedBoth.Size = new System.Drawing.Size(50, 20);
            this.poisonedBoth.TabIndex = 87;
            this.poisonedBoth.TabStop = true;
            this.poisonedBoth.Text = "Both";
            #endregion
            #region Selector Group
            // 
            // groupBox56
            // 
            this.groupBox56.Controls.Add(this.targetSelectorComboBox);
            this.groupBox56.Location = new System.Drawing.Point(140, 321);
            this.groupBox56.Name = "groupBox56";
            this.groupBox56.Size = new System.Drawing.Size(200, 40);
            this.groupBox56.TabIndex = 55;
            this.groupBox56.TabStop = false;
            this.groupBox56.Text = "Selector";
            // 
            // targetSelectorComboBox
            // 
            this.targetSelectorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.targetSelectorComboBox.FormattingEnabled = true;
            this.targetSelectorComboBox.Location = new System.Drawing.Point(8, 14);
            this.targetSelectorComboBox.Name = "targetSelectorComboBox";
            this.targetSelectorComboBox.Size = new System.Drawing.Size(182, 22);
            this.targetSelectorComboBox.TabIndex = 11;
            // 
            #endregion
            #region Target By Name Group
            // groupBox55
            // 
            this.groupBox55.Controls.Add(this.targetNameTextBox);
            this.groupBox55.Location = new System.Drawing.Point(346, 321);
            this.groupBox55.Name = "groupBox55";
            this.groupBox55.Size = new System.Drawing.Size(180, 40);
            this.groupBox55.TabIndex = 54;
            this.groupBox55.TabStop = false;
            this.groupBox55.Text = "Target (Char/Mob) Name";
            // 
            // targetNameTextBox
            // 
            this.targetNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetNameTextBox.BackColor = System.Drawing.Color.White;
            this.targetNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.targetNameTextBox.Location = new System.Drawing.Point(10, 15);
            this.targetNameTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.targetNameTextBox.Name = "targetNameTextBox";
            this.targetNameTextBox.Size = new System.Drawing.Size(162, 20);
            this.targetNameTextBox.TabIndex = 13;
            #endregion
            // 
            // targetTestButton
            // 
            this.targetTestButton.Location = new System.Drawing.Point(375, 274);
            this.targetTestButton.Name = "targetTestButton";
            this.targetTestButton.Size = new System.Drawing.Size(100, 23);
            this.targetTestButton.TabIndex = 57;
            this.targetTestButton.Text = "Test Target";
            this.targetTestButton.UseVisualStyleBackColor = true;
            this.targetTestButton.Click += new System.EventHandler(this.targetTestButton_Click);
            // 
            // targetsaveButton
            // 
            this.targetsaveButton.Location = new System.Drawing.Point(556, 333);
            this.targetsaveButton.Name = "targetsaveButton";
            this.targetsaveButton.Size = new System.Drawing.Size(88, 23);
            this.targetsaveButton.TabIndex = 5;
            this.targetsaveButton.Text = "Save Change";
            this.targetsaveButton.UseVisualStyleBackColor = true;
            this.targetsaveButton.Click += new System.EventHandler(this.targetsaveButton_Click);
            #endregion
            #region Misc Tab
            // 
            // MiscFilterPage
            // 
            this.MiscFilterPage.Controls.Add(this.DmgDsplyGroup);
            this.MiscFilterPage.Controls.Add(this.uomodgroupbox);
            this.MiscFilterPage.Controls.Add(this.groupBox32);
            this.MiscFilterPage.Controls.Add(this.groupBox24);
            this.MiscFilterPage.Controls.Add(this.groupBox23);
            this.MiscFilterPage.Controls.Add(this.groupBox10);
            this.MiscFilterPage.Controls.Add(this.groupBox9);
            this.MiscFilterPage.Location = new System.Drawing.Point(4, 23);
            this.MiscFilterPage.Name = "MiscFilterPage";
            this.MiscFilterPage.Padding = new System.Windows.Forms.Padding(3);
            this.MiscFilterPage.Size = new System.Drawing.Size(649, 344);
            this.MiscFilterPage.TabIndex = 0;
            this.MiscFilterPage.Text = "Misc";
            this.MiscFilterPage.UseVisualStyleBackColor = true;
            #region Misc Group
            // 
            // groupBox24
            // 
            this.groupBox24.Controls.Add(this.colorflagsselfHighlightCheckBox);
            this.groupBox24.Controls.Add(this.showagentmessageCheckBox);
            this.groupBox24.Controls.Add(this.showmessagefieldCheckBox);
            this.groupBox24.Controls.Add(this.colorflagsHighlightCheckBox);
            this.groupBox24.Controls.Add(this.blockchivalryhealCheckBox);
            this.groupBox24.Controls.Add(this.blockbighealCheckBox);
            this.groupBox24.Controls.Add(this.blockminihealCheckBox);
            this.groupBox24.Controls.Add(this.blockhealpoisonCheckBox);
            this.groupBox24.Controls.Add(this.showheadtargetCheckBox);
            this.groupBox24.Controls.Add(this.blockpartyinviteCheckBox);
            this.groupBox24.Controls.Add(this.blocktraderequestCheckBox);
            this.groupBox24.Controls.Add(this.highlighttargetCheckBox);
            this.groupBox24.Controls.Add(this.flagsHighlightCheckBox);
            this.groupBox24.Controls.Add(this.showstaticfieldCheckBox);
            this.groupBox24.Location = new System.Drawing.Point(10, 0);
            this.groupBox24.Name = "groupBox24";
            this.groupBox24.Size = new System.Drawing.Size(178, 337);
            this.groupBox24.TabIndex = 73;
            this.groupBox24.TabStop = false;
            this.groupBox24.Text = "Misc";
            // 
            // colorflagsselfHighlightCheckBox
            // 
            this.colorflagsselfHighlightCheckBox.Location = new System.Drawing.Point(6, 86);
            this.colorflagsselfHighlightCheckBox.Name = "colorflagsselfHighlightCheckBox";
            this.colorflagsselfHighlightCheckBox.Size = new System.Drawing.Size(145, 22);
            this.colorflagsselfHighlightCheckBox.TabIndex = 71;
            this.colorflagsselfHighlightCheckBox.Text = "Color Flag Self Highlight";
            this.colorflagsselfHighlightCheckBox.CheckedChanged += new System.EventHandler(this.colorflagsselfHighlightCheckBox_CheckedChanged);
            // 
            // showagentmessageCheckBox
            // 
            this.showagentmessageCheckBox.Location = new System.Drawing.Point(6, 313);
            this.showagentmessageCheckBox.Name = "showagentmessageCheckBox";
            this.showagentmessageCheckBox.Size = new System.Drawing.Size(166, 22);
            this.showagentmessageCheckBox.TabIndex = 70;
            this.showagentmessageCheckBox.Text = "Show Agent Message";
            this.showagentmessageCheckBox.CheckedChanged += new System.EventHandler(this.showagentmessageCheckBox_CheckedChanged);
            // 
            // showmessagefieldCheckBox
            // 
            this.showmessagefieldCheckBox.Location = new System.Drawing.Point(6, 131);
            this.showmessagefieldCheckBox.Name = "showmessagefieldCheckBox";
            this.showmessagefieldCheckBox.Size = new System.Drawing.Size(157, 22);
            this.showmessagefieldCheckBox.TabIndex = 69;
            this.showmessagefieldCheckBox.Text = "Show Filed Message";
            this.showmessagefieldCheckBox.CheckedChanged += new System.EventHandler(this.showmessagefieldCheckBox_CheckedChanged);
            // 
            // colorflagsHighlightCheckBox
            // 
            this.colorflagsHighlightCheckBox.Location = new System.Drawing.Point(6, 63);
            this.colorflagsHighlightCheckBox.Name = "colorflagsHighlightCheckBox";
            this.colorflagsHighlightCheckBox.Size = new System.Drawing.Size(145, 22);
            this.colorflagsHighlightCheckBox.TabIndex = 68;
            this.colorflagsHighlightCheckBox.Text = "Color Flag Highlight";
            this.colorflagsHighlightCheckBox.CheckedChanged += new System.EventHandler(this.colorflagsHighlightCheckBox_CheckedChanged);
            // 
            // blockchivalryhealCheckBox
            // 
            this.blockchivalryhealCheckBox.Location = new System.Drawing.Point(6, 290);
            this.blockchivalryhealCheckBox.Name = "blockchivalryhealCheckBox";
            this.blockchivalryhealCheckBox.Size = new System.Drawing.Size(166, 22);
            this.blockchivalryhealCheckBox.TabIndex = 67;
            this.blockchivalryhealCheckBox.Text = "Block ChivaHeal if no need";
            this.blockchivalryhealCheckBox.CheckedChanged += new System.EventHandler(this.blockchivalryhealCheckBox_CheckedChanged);
            // 
            // blockbighealCheckBox
            // 
            this.blockbighealCheckBox.Location = new System.Drawing.Point(6, 267);
            this.blockbighealCheckBox.Name = "blockbighealCheckBox";
            this.blockbighealCheckBox.Size = new System.Drawing.Size(157, 22);
            this.blockbighealCheckBox.TabIndex = 66;
            this.blockbighealCheckBox.Text = "Block BigHeal if no need";
            this.blockbighealCheckBox.CheckedChanged += new System.EventHandler(this.blockbighealCheckBox_CheckedChanged);
            // 
            // blockminihealCheckBox
            // 
            this.blockminihealCheckBox.Location = new System.Drawing.Point(6, 244);
            this.blockminihealCheckBox.Name = "blockminihealCheckBox";
            this.blockminihealCheckBox.Size = new System.Drawing.Size(157, 22);
            this.blockminihealCheckBox.TabIndex = 65;
            this.blockminihealCheckBox.Text = "Block MiniHeal if no need";
            this.blockminihealCheckBox.CheckedChanged += new System.EventHandler(this.blockminihealCheckBox_CheckedChanged);
            // 
            // blockhealpoisonCheckBox
            // 
            this.blockhealpoisonCheckBox.Location = new System.Drawing.Point(6, 221);
            this.blockhealpoisonCheckBox.Name = "blockhealpoisonCheckBox";
            this.blockhealpoisonCheckBox.Size = new System.Drawing.Size(166, 22);
            this.blockhealpoisonCheckBox.TabIndex = 64;
            this.blockhealpoisonCheckBox.Text = "Block Heal if Poison/Mortal";
            this.blockhealpoisonCheckBox.CheckedChanged += new System.EventHandler(this.blockhealpoisonCheckBox_CheckedChanged);
            // 
            // showheadtargetCheckBox
            // 
            this.showheadtargetCheckBox.Location = new System.Drawing.Point(6, 198);
            this.showheadtargetCheckBox.Name = "showheadtargetCheckBox";
            this.showheadtargetCheckBox.Size = new System.Drawing.Size(141, 22);
            this.showheadtargetCheckBox.TabIndex = 63;
            this.showheadtargetCheckBox.Text = "Show Target on Head";
            this.showheadtargetCheckBox.CheckedChanged += new System.EventHandler(this.showheadtargetCheckBox_CheckedChanged);
            // 
            // blockpartyinviteCheckBox
            // 
            this.blockpartyinviteCheckBox.Location = new System.Drawing.Point(6, 175);
            this.blockpartyinviteCheckBox.Name = "blockpartyinviteCheckBox";
            this.blockpartyinviteCheckBox.Size = new System.Drawing.Size(141, 22);
            this.blockpartyinviteCheckBox.TabIndex = 62;
            this.blockpartyinviteCheckBox.Text = "Block Party Invite";
            this.blockpartyinviteCheckBox.CheckedChanged += new System.EventHandler(this.blockpartyinviteCheckBox_CheckedChanged);
            // 
            // blocktraderequestCheckBox
            // 
            this.blocktraderequestCheckBox.Location = new System.Drawing.Point(6, 153);
            this.blocktraderequestCheckBox.Name = "blocktraderequestCheckBox";
            this.blocktraderequestCheckBox.Size = new System.Drawing.Size(141, 22);
            this.blocktraderequestCheckBox.TabIndex = 61;
            this.blocktraderequestCheckBox.Text = "Block Trade Request";
            this.blocktraderequestCheckBox.CheckedChanged += new System.EventHandler(this.blocktraderequestCheckBox_CheckedChanged);
            // 
            // highlighttargetCheckBox
            // 
            this.highlighttargetCheckBox.Location = new System.Drawing.Point(6, 19);
            this.highlighttargetCheckBox.Name = "highlighttargetCheckBox";
            this.highlighttargetCheckBox.Size = new System.Drawing.Size(145, 22);
            this.highlighttargetCheckBox.TabIndex = 58;
            this.highlighttargetCheckBox.Text = "Text Current Target";
            this.highlighttargetCheckBox.CheckedChanged += new System.EventHandler(this.highlighttargetCheckBox_CheckedChanged);
            // 
            // flagsHighlightCheckBox
            // 
            this.flagsHighlightCheckBox.Location = new System.Drawing.Point(6, 41);
            this.flagsHighlightCheckBox.Name = "flagsHighlightCheckBox";
            this.flagsHighlightCheckBox.Size = new System.Drawing.Size(132, 22);
            this.flagsHighlightCheckBox.TabIndex = 59;
            this.flagsHighlightCheckBox.Text = "Text Flags Highlight";
            this.flagsHighlightCheckBox.CheckedChanged += new System.EventHandler(this.flagsHighlightCheckBox_CheckedChanged);
            // 
            // showstaticfieldCheckBox
            // 
            this.showstaticfieldCheckBox.Location = new System.Drawing.Point(6, 108);
            this.showstaticfieldCheckBox.Name = "showstaticfieldCheckBox";
            this.showstaticfieldCheckBox.Size = new System.Drawing.Size(118, 22);
            this.showstaticfieldCheckBox.TabIndex = 60;
            this.showstaticfieldCheckBox.Text = "Show Static Field";
            this.showstaticfieldCheckBox.CheckedChanged += new System.EventHandler(this.showstaticfieldCheckBox_CheckedChanged);
            #endregion
            #region Mobile Graphics Change Filter Group
            // 
            // groupBox23
            // 
            this.groupBox23.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox23.Controls.Add(this.graphfilterdatagrid);
            this.groupBox23.Controls.Add(this.mobfilterCheckBox);
            this.groupBox23.Location = new System.Drawing.Point(194, 0);
            this.groupBox23.Name = "groupBox23";
            this.groupBox23.Size = new System.Drawing.Size(268, 194);
            this.groupBox23.TabIndex = 72;
            this.groupBox23.TabStop = false;
            this.groupBox23.Text = "Mobile Graphics Change Filter";
            // 
            // graphfilterdatagrid
            // 
            this.graphfilterdatagrid.AllowDrop = true;
            this.graphfilterdatagrid.AllowUserToResizeRows = false;
            this.graphfilterdatagrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphfilterdatagrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.graphfilterdatagrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn4,
            this.dataGridViewTextBoxColumn16,
            this.dataGridViewTextBoxColumn17,
            this.dataGridViewTextBoxColumn18,
            this.dataGridViewTextBoxColumn19});
            this.graphfilterdatagrid.Location = new System.Drawing.Point(6, 47);
            this.graphfilterdatagrid.Name = "graphfilterdatagrid";
            this.graphfilterdatagrid.RowHeadersVisible = false;
            this.graphfilterdatagrid.RowHeadersWidth = 62;
            this.graphfilterdatagrid.Size = new System.Drawing.Size(256, 134);
            this.graphfilterdatagrid.TabIndex = 69;
            this.graphfilterdatagrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.graphfilterdatagrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.graphfilterdatagrid_CellEndEdit);
            this.graphfilterdatagrid.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.graphfilterdatagrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.graphfilterdatagrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.graphfilterdatagrid.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.graphfilterdatagrid_DefaultValuesNeeded);
            this.graphfilterdatagrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.graphfilterdatagrid.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.graphfilterdatagrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.graphfilterdatagrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // dataGridViewCheckBoxColumn4
            // 
            this.dataGridViewCheckBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewCheckBoxColumn4.FalseValue = "False";
            this.dataGridViewCheckBoxColumn4.HeaderText = "X";
            this.dataGridViewCheckBoxColumn4.IndeterminateValue = "False";
            this.dataGridViewCheckBoxColumn4.MinimumWidth = 30;
            this.dataGridViewCheckBoxColumn4.Name = "dataGridViewCheckBoxColumn4";
            this.dataGridViewCheckBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewCheckBoxColumn4.TrueValue = "True";
            this.dataGridViewCheckBoxColumn4.Width = 30;
            // 
            // dataGridViewTextBoxColumn16
            // 
            this.dataGridViewTextBoxColumn16.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn16.FillWeight = 25F;
            this.dataGridViewTextBoxColumn16.HeaderText = "Old Graphic";
            this.dataGridViewTextBoxColumn16.MinimumWidth = 133;
            this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
            // 
            // dataGridViewTextBoxColumn17
            // 
            this.dataGridViewTextBoxColumn17.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn17.FillWeight = 25F;
            this.dataGridViewTextBoxColumn17.HeaderText = "New Graphic";
            this.dataGridViewTextBoxColumn17.MinimumWidth = 133;
            this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
            this.dataGridViewTextBoxColumn17.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridViewTextBoxColumn18
            // 
            this.dataGridViewTextBoxColumn18.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn18.FillWeight = 25F;
            this.dataGridViewTextBoxColumn18.HeaderText = "New Color";
            this.dataGridViewTextBoxColumn18.MinimumWidth = 133;
            this.dataGridViewTextBoxColumn18.Name = "dataGridViewTextBoxColumn18";
            this.dataGridViewTextBoxColumn18.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridViewTextBoxColumn19
            // 
            this.dataGridViewTextBoxColumn19.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn19.FillWeight = 25F;
            this.dataGridViewTextBoxColumn19.HeaderText = "Props";
            this.dataGridViewTextBoxColumn19.MinimumWidth = 150;
            this.dataGridViewTextBoxColumn19.Name = "dataGridViewTextBoxColumn19";
            this.dataGridViewTextBoxColumn19.Visible = false;
            // 
            // mobfilterCheckBox
            // 
            this.mobfilterCheckBox.Location = new System.Drawing.Point(6, 19);
            this.mobfilterCheckBox.Name = "mobfilterCheckBox";
            this.mobfilterCheckBox.Size = new System.Drawing.Size(79, 22);
            this.mobfilterCheckBox.TabIndex = 61;
            this.mobfilterCheckBox.Text = "Enable";
            this.mobfilterCheckBox.CheckedChanged += new System.EventHandler(this.mobfilterCheckBox_CheckedChanged);
            #endregion
            #region UO Mod Client Group
            // 
            // uomodgroupbox
            // 
            this.uomodgroupbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.uomodgroupbox.Controls.Add(this.uomodpaperdollCheckBox);
            this.uomodgroupbox.Controls.Add(this.uomodglobalsoundCheckBox);
            this.uomodgroupbox.Controls.Add(this.uomodFPSCheckBox);
            this.uomodgroupbox.Location = new System.Drawing.Point(194, 201);
            this.uomodgroupbox.Name = "uomodgroupbox";
            this.uomodgroupbox.Size = new System.Drawing.Size(234, 67);
            this.uomodgroupbox.TabIndex = 75;
            this.uomodgroupbox.TabStop = false;
            this.uomodgroupbox.Text = "UoMod (Client > 7.0.0.0)";
            // 
            // uomodpaperdollCheckBox
            // 
            this.uomodpaperdollCheckBox.Location = new System.Drawing.Point(111, 15);
            this.uomodpaperdollCheckBox.Name = "uomodpaperdollCheckBox";
            this.uomodpaperdollCheckBox.Size = new System.Drawing.Size(111, 22);
            this.uomodpaperdollCheckBox.TabIndex = 61;
            this.uomodpaperdollCheckBox.Text = "Show Paperdoll Slot";
            this.uomodpaperdollCheckBox.CheckedChanged += new System.EventHandler(this.uomodpaperdollCheckBox_CheckedChanged);
            // 
            // uomodglobalsoundCheckBox
            // 
            this.uomodglobalsoundCheckBox.Location = new System.Drawing.Point(6, 38);
            this.uomodglobalsoundCheckBox.Name = "uomodglobalsoundCheckBox";
            this.uomodglobalsoundCheckBox.Size = new System.Drawing.Size(99, 22);
            this.uomodglobalsoundCheckBox.TabIndex = 60;
            this.uomodglobalsoundCheckBox.Text = "Global Sound";
            this.uomodglobalsoundCheckBox.CheckedChanged += new System.EventHandler(this.uomodglobalsoundCheckBox_CheckedChanged);
            // 
            // uomodFPSCheckBox
            // 
            this.uomodFPSCheckBox.Location = new System.Drawing.Point(6, 15);
            this.uomodFPSCheckBox.Name = "uomodFPSCheckBox";
            this.uomodFPSCheckBox.Size = new System.Drawing.Size(99, 22);
            this.uomodFPSCheckBox.TabIndex = 59;
            this.uomodFPSCheckBox.Text = "Increase FPS";
            this.uomodFPSCheckBox.CheckedChanged += new System.EventHandler(this.uomodFPSCheckBox_CheckedChanged);
            #endregion
            #region Auto Carver Group
            // 
            // groupBox10
            // 
            this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox10.Controls.Add(this.autocarverbladeLabel);
            this.groupBox10.Controls.Add(this.label34);
            this.groupBox10.Controls.Add(this.autocarverrazorButton);
            this.groupBox10.Controls.Add(this.autocarverCheckBox);
            this.groupBox10.Location = new System.Drawing.Point(477, 0);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(166, 80);
            this.groupBox10.TabIndex = 71;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Auto Carver";
            // 
            // autocarverbladeLabel
            // 
            this.autocarverbladeLabel.AutoSize = true;
            this.autocarverbladeLabel.Location = new System.Drawing.Point(78, 48);
            this.autocarverbladeLabel.Name = "autocarverbladeLabel";
            this.autocarverbladeLabel.Size = new System.Drawing.Size(67, 14);
            this.autocarverbladeLabel.TabIndex = 64;
            this.autocarverbladeLabel.Text = "0x00000000";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(6, 48);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(67, 14);
            this.label34.TabIndex = 63;
            this.label34.Text = "Blade Serial:";
            // 
            // autocarverrazorButton
            // 
            this.autocarverrazorButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autocarverrazorButton.Location = new System.Drawing.Point(74, 18);
            this.autocarverrazorButton.Name = "autocarverrazorButton";
            this.autocarverrazorButton.Size = new System.Drawing.Size(75, 21);
            this.autocarverrazorButton.TabIndex = 62;
            this.autocarverrazorButton.Text = "Set Blade";
            this.autocarverrazorButton.UseVisualStyleBackColor = true;
            this.autocarverrazorButton.Click += new System.EventHandler(this.autocarverrazorButton_Click);
            // 
            // autocarverCheckBox
            // 
            this.autocarverCheckBox.Location = new System.Drawing.Point(6, 19);
            this.autocarverCheckBox.Name = "autocarverCheckBox";
            this.autocarverCheckBox.Size = new System.Drawing.Size(79, 22);
            this.autocarverCheckBox.TabIndex = 61;
            this.autocarverCheckBox.Text = "Enable";
            this.autocarverCheckBox.CheckedChanged += new System.EventHandler(this.autocarverCheckBox_CheckedChanged);
            #endregion
            #region Bone Cutter Group
            // 
            // groupBox9
            // 
            this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox9.Controls.Add(this.bonebladeLabel);
            this.groupBox9.Controls.Add(this.label16);
            this.groupBox9.Controls.Add(this.boneCutterrazorButton);
            this.groupBox9.Controls.Add(this.bonecutterCheckBox);
            this.groupBox9.Location = new System.Drawing.Point(477, 86);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(166, 80);
            this.groupBox9.TabIndex = 70;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Bone Cutter";
            // 
            // bonebladeLabel
            // 
            this.bonebladeLabel.AutoSize = true;
            this.bonebladeLabel.Location = new System.Drawing.Point(78, 48);
            this.bonebladeLabel.Name = "bonebladeLabel";
            this.bonebladeLabel.Size = new System.Drawing.Size(67, 14);
            this.bonebladeLabel.TabIndex = 64;
            this.bonebladeLabel.Text = "0x00000000";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 48);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(67, 14);
            this.label16.TabIndex = 63;
            this.label16.Text = "Blade Serial:";
            // 
            // boneCutterrazorButton
            // 
            this.boneCutterrazorButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.boneCutterrazorButton.Location = new System.Drawing.Point(74, 19);
            this.boneCutterrazorButton.Name = "boneCutterrazorButton";
            this.boneCutterrazorButton.Size = new System.Drawing.Size(75, 21);
            this.boneCutterrazorButton.TabIndex = 62;
            this.boneCutterrazorButton.Text = "Set Blade";
            this.boneCutterrazorButton.UseVisualStyleBackColor = true;
            this.boneCutterrazorButton.Click += new System.EventHandler(this.boneCutterrazorButton_Click);
            // 
            // bonecutterCheckBox
            // 
            this.bonecutterCheckBox.Location = new System.Drawing.Point(6, 19);
            this.bonecutterCheckBox.Name = "bonecutterCheckBox";
            this.bonecutterCheckBox.Size = new System.Drawing.Size(79, 22);
            this.bonecutterCheckBox.TabIndex = 61;
            this.bonecutterCheckBox.Text = "Enable";
            this.bonecutterCheckBox.CheckedChanged += new System.EventHandler(this.bonecutterCheckBox_CheckedChanged);
            #endregion
            #region Auto Remount Group
            // 
            // groupBox32
            // 
            this.groupBox32.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox32.Controls.Add(this.remountedelay);
            this.groupBox32.Controls.Add(this.remountdelay);
            this.groupBox32.Controls.Add(this.label48);
            this.groupBox32.Controls.Add(this.label40);
            this.groupBox32.Controls.Add(this.remountseriallabel);
            this.groupBox32.Controls.Add(this.label47);
            this.groupBox32.Controls.Add(this.remountsetbutton);
            this.groupBox32.Controls.Add(this.remountcheckbox);
            this.groupBox32.Location = new System.Drawing.Point(478, 172);
            this.groupBox32.Name = "groupBox32";
            this.groupBox32.Size = new System.Drawing.Size(165, 118);
            this.groupBox32.TabIndex = 74;
            this.groupBox32.TabStop = false;
            this.groupBox32.Text = "Auto Remount";
            // 
            // remountedelay
            // 
            this.remountedelay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remountedelay.BackColor = System.Drawing.Color.White;
            this.remountedelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.remountedelay.Location = new System.Drawing.Point(93, 89);
            this.remountedelay.Name = "remountedelay";
            this.remountedelay.Size = new System.Drawing.Size(58, 20);
            this.remountedelay.TabIndex = 68;
            this.remountedelay.Leave += new System.EventHandler(this.remountedelay_Leave);
            // 
            // remountdelay
            // 
            this.remountdelay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remountdelay.BackColor = System.Drawing.Color.White;
            this.remountdelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.remountdelay.Location = new System.Drawing.Point(93, 64);
            this.remountdelay.Name = "remountdelay";
            this.remountdelay.Size = new System.Drawing.Size(58, 20);
            this.remountdelay.TabIndex = 67;
            this.remountdelay.Leave += new System.EventHandler(this.remountdelay_Leave);
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(6, 91);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(79, 14);
            this.label48.TabIndex = 66;
            this.label48.Text = "Ethereal Delay:";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(6, 70);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(69, 14);
            this.label40.TabIndex = 65;
            this.label40.Text = "Mount Delay:";
            // 
            // remountseriallabel
            // 
            this.remountseriallabel.AutoSize = true;
            this.remountseriallabel.Location = new System.Drawing.Point(90, 48);
            this.remountseriallabel.Name = "remountseriallabel";
            this.remountseriallabel.Size = new System.Drawing.Size(67, 14);
            this.remountseriallabel.TabIndex = 64;
            this.remountseriallabel.Text = "0x00000000";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(6, 48);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(69, 14);
            this.label47.TabIndex = 63;
            this.label47.Text = "Mount Serial:";
            // 
            // remountsetbutton
            // 
            this.remountsetbutton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.remountsetbutton.Location = new System.Drawing.Point(73, 19);
            this.remountsetbutton.Name = "remountsetbutton";
            this.remountsetbutton.Size = new System.Drawing.Size(75, 21);
            this.remountsetbutton.TabIndex = 62;
            this.remountsetbutton.Text = "Set Mount";
            this.remountsetbutton.UseVisualStyleBackColor = true;
            this.remountsetbutton.Click += new System.EventHandler(this.remountsetbutton_Click);
            // 
            // remountcheckbox
            // 
            this.remountcheckbox.Location = new System.Drawing.Point(6, 19);
            this.remountcheckbox.Name = "remountcheckbox";
            this.remountcheckbox.Size = new System.Drawing.Size(62, 22);
            this.remountcheckbox.TabIndex = 61;
            this.remountcheckbox.Text = "Enable";
            this.remountcheckbox.CheckedChanged += new System.EventHandler(this.remountcheckbox_CheckedChanged);
            #endregion
            #region Damage Display Group
            // 
            // DmgDsplyGroup
            // 
            this.DmgDsplyGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DmgDsplyGroup.Controls.Add(this.minDmgShown);
            this.DmgDsplyGroup.Controls.Add(this.label81);
            this.DmgDsplyGroup.Controls.Add(this.limitDamageDisplayEnable);
            this.DmgDsplyGroup.Location = new System.Drawing.Point(478, 297);
            this.DmgDsplyGroup.Name = "DmgDsplyGroup";
            this.DmgDsplyGroup.Size = new System.Drawing.Size(165, 91);
            this.DmgDsplyGroup.TabIndex = 76;
            this.DmgDsplyGroup.TabStop = false;
            this.DmgDsplyGroup.Text = "Damage Display";
            // 
            // minDmgShown
            // 
            this.minDmgShown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minDmgShown.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.minDmgShown.Location = new System.Drawing.Point(93, 42);
            this.minDmgShown.Name = "minDmgShown";
            this.minDmgShown.Size = new System.Drawing.Size(58, 20);
            this.minDmgShown.TabIndex = 2;
            this.minDmgShown.Leave += new System.EventHandler(this.minDmgShown_Leave);
            // 
            // label81
            // 
            this.label81.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label81.AutoSize = true;
            this.label81.Location = new System.Drawing.Point(9, 44);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(50, 14);
            this.label81.TabIndex = 1;
            this.label81.Text = "Min Dmg:";
            // 
            // limitDamageDisplayEnable
            // 
            this.limitDamageDisplayEnable.AutoSize = true;
            this.limitDamageDisplayEnable.Location = new System.Drawing.Point(9, 20);
            this.limitDamageDisplayEnable.Name = "limitDamageDisplayEnable";
            this.limitDamageDisplayEnable.Size = new System.Drawing.Size(135, 18);
            this.limitDamageDisplayEnable.TabIndex = 0;
            this.limitDamageDisplayEnable.Text = "Suppress Dmg Display";
            this.limitDamageDisplayEnable.UseVisualStyleBackColor = true;
            this.limitDamageDisplayEnable.CheckedChanged += new System.EventHandler(this.DmgDisplayLimitCheckBox_CheckedChanged);
            #endregion

            #endregion
            #endregion
            #region Scripts Tab
            // 
            // AllScripts
            // 
            this.AllScripts.Controls.Add(this.scriptControlBox);
            this.AllScripts.Controls.Add(this.AllScriptsTab);
            this.AllScripts.Location = new System.Drawing.Point(4, 54);
            this.AllScripts.Name = "AllScripts";
            this.AllScripts.Size = new System.Drawing.Size(678, 365);
            this.AllScripts.TabIndex = 19;
            this.AllScripts.Text = "Scripting";
            this.AllScripts.UseVisualStyleBackColor = true;
            // 
            // AllScriptsTab
            // 
            this.AllScriptsTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AllScriptsTab.Controls.Add(this.pythonScriptingTab);
            this.AllScriptsTab.Controls.Add(this.uosScriptingTab);
            this.AllScriptsTab.Controls.Add(this.csScriptingTab);
            this.AllScriptsTab.Location = new System.Drawing.Point(-2, 0);
            this.AllScriptsTab.Name = "AllScriptsTab";
            this.AllScriptsTab.SelectedIndex = 0;
            this.AllScriptsTab.Size = new System.Drawing.Size(477, 290);
            this.AllScriptsTab.TabIndex = 0;
            #region Python Scripting Tab
            // 
            // pythonScriptingTab
            // 
            this.pythonScriptingTab.BackColor = System.Drawing.SystemColors.Control;
            this.pythonScriptingTab.Controls.Add(this.pyScriptListView);
            this.pythonScriptingTab.Location = new System.Drawing.Point(4, 23);
            this.pythonScriptingTab.Name = "pythonScriptingTab";
            this.pythonScriptingTab.Padding = new System.Windows.Forms.Padding(3);
            this.pythonScriptingTab.Size = new System.Drawing.Size(469, 263);
            this.pythonScriptingTab.TabIndex = 13;
            this.pythonScriptingTab.Text = "Python";
            // 
            // pyScriptListView
            // 
            this.pyScriptListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pyScriptListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.filename,
            this.status,
            this.loop,
            this.autostart,
            this.wait,
            this.hotkey,
            this.heypass,
            this.index,
            this.preload,
            this.fullFilePath});
            this.pyScriptListView.FullRowSelect = true;
            this.pyScriptListView.GridLines = true;
            this.pyScriptListView.HideSelection = false;
            this.pyScriptListView.LabelWrap = false;
            this.pyScriptListView.Location = new System.Drawing.Point(4, -2);
            this.pyScriptListView.MultiSelect = false;
            this.pyScriptListView.Name = "pyScriptListView";
            this.pyScriptListView.ShowItemToolTips = true;
            this.pyScriptListView.Size = new System.Drawing.Size(466, 255);
            this.pyScriptListView.TabIndex = 48;
            this.pyScriptListView.UseCompatibleStateImageBehavior = false;
            this.pyScriptListView.View = System.Windows.Forms.View.Details;
            this.pyScriptListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.scriptlistView_ColumnClick);
            this.pyScriptListView.SelectedIndexChanged += new System.EventHandler(this.scriptlistView_SelectedIndexChanged);
            this.pyScriptListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scriptlistView_MouseClick);
            this.pyScriptListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.scriptlistView_MouseDoubleClick);
            // 
            // filename
            // 
            this.filename.DisplayIndex = 1;
            this.filename.Tag = "filename";
            this.filename.Text = "Filename";
            this.filename.Width = 350;
            // 
            // status
            // 
            this.status.DisplayIndex = 2;
            this.status.Tag = "status";
            this.status.Text = "Status";
            this.status.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.status.Width = 80;
            // 
            // loop
            // 
            this.loop.DisplayIndex = 3;
            this.loop.Tag = "loop";
            this.loop.Text = "Loop";
            this.loop.Width = 50;
            // 
            // autostart
            // 
            this.autostart.DisplayIndex = 6;
            this.autostart.Tag = "autostart";
            this.autostart.Text = "A.S.";
            this.autostart.Width = 55;
            // 
            // wait
            // 
            this.wait.DisplayIndex = 5;
            this.wait.Tag = "wait";
            this.wait.Text = "Wait";
            this.wait.Width = 40;
            // 
            // hotkey
            // 
            this.hotkey.DisplayIndex = 7;
            this.hotkey.Tag = "hotkey";
            this.hotkey.Text = "Hot Keys";
            this.hotkey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.hotkey.Width = 80;
            // 
            // heypass
            // 
            this.heypass.DisplayIndex = 8;
            this.heypass.Tag = "keypass";
            this.heypass.Text = "KeyPass";
            this.heypass.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.heypass.Width = 80;
            // 
            // index
            // 
            this.index.DisplayIndex = 0;
            this.index.Tag = "index";
            this.index.Text = "#";
            this.index.Width = 40;
            // 
            // preload
            // 
            this.preload.DisplayIndex = 4;
            this.preload.Tag = "preload";
            this.preload.Text = "Preload";
            this.preload.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.preload.Width = 50;
            // 
            // fullFilePath
            // 
            this.fullFilePath.Tag = "fullFilePath";
            this.fullFilePath.Text = "";
            this.fullFilePath.Width = 0;
            #endregion
            #region UOS Scripting Tab
            // 
            // uosScriptingTab
            // 
            this.uosScriptingTab.Controls.Add(this.uosScriptListView);
            this.uosScriptingTab.Location = new System.Drawing.Point(4, 22);
            this.uosScriptingTab.Name = "uosScriptingTab";
            this.uosScriptingTab.Size = new System.Drawing.Size(469, 264);
            this.uosScriptingTab.TabIndex = 14;
            this.uosScriptingTab.Text = "UOS";
            this.uosScriptingTab.UseVisualStyleBackColor = true;
            // 
            // uosScriptListView
            // 
            this.uosScriptListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uosScriptListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader19,
            this.columnHeader9});
            this.uosScriptListView.FullRowSelect = true;
            this.uosScriptListView.GridLines = true;
            this.uosScriptListView.HideSelection = false;
            this.uosScriptListView.LabelWrap = false;
            this.uosScriptListView.Location = new System.Drawing.Point(2, 1);
            this.uosScriptListView.MultiSelect = false;
            this.uosScriptListView.Name = "uosScriptListView";
            this.uosScriptListView.ShowItemToolTips = true;
            this.uosScriptListView.Size = new System.Drawing.Size(463, 310);
            this.uosScriptListView.TabIndex = 49;
            this.uosScriptListView.UseCompatibleStateImageBehavior = false;
            this.uosScriptListView.View = System.Windows.Forms.View.Details;
            this.uosScriptListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.scriptlistView_ColumnClick);
            this.uosScriptListView.SelectedIndexChanged += new System.EventHandler(this.scriptlistView_SelectedIndexChanged);
            this.uosScriptListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scriptlistView_MouseClick);
            this.uosScriptListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.scriptlistView_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.DisplayIndex = 1;
            this.columnHeader1.Text = "Filename";
            this.columnHeader1.Width = 350;
            // 
            // columnHeader2
            // 
            this.columnHeader2.DisplayIndex = 2;
            this.columnHeader2.Text = "Status";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            this.columnHeader3.DisplayIndex = 3;
            this.columnHeader3.Text = "Loop";
            this.columnHeader3.Width = 50;
            // 
            // columnHeader4
            // 
            this.columnHeader4.DisplayIndex = 6;
            this.columnHeader4.Text = "A.S.";
            this.columnHeader4.Width = 55;
            // 
            // columnHeader5
            // 
            this.columnHeader5.DisplayIndex = 5;
            this.columnHeader5.Text = "Wait";
            this.columnHeader5.Width = 40;
            // 
            // columnHeader6
            // 
            this.columnHeader6.DisplayIndex = 7;
            this.columnHeader6.Text = "Hot Keys";
            this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader6.Width = 80;
            // 
            // columnHeader7
            // 
            this.columnHeader7.DisplayIndex = 8;
            this.columnHeader7.Text = "KeyPass";
            this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader7.Width = 80;
            // 
            // columnHeader8
            // 
            this.columnHeader8.DisplayIndex = 0;
            this.columnHeader8.Text = "#";
            this.columnHeader8.Width = 40;
            // 
            // columnHeader19
            // 
            this.columnHeader19.DisplayIndex = 4;
            this.columnHeader19.Text = "Preload";
            this.columnHeader19.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader19.Width = 0;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "";
            this.columnHeader9.Width = 0;
            #endregion
            #region C# Scripting Tab
            // 
            // csScriptingTab
            // 
            this.csScriptingTab.Controls.Add(this.csScriptListView);
            this.csScriptingTab.Location = new System.Drawing.Point(4, 22);
            this.csScriptingTab.Name = "csScriptingTab";
            this.csScriptingTab.Size = new System.Drawing.Size(469, 264);
            this.csScriptingTab.TabIndex = 15;
            this.csScriptingTab.Text = "C#";
            this.csScriptingTab.UseVisualStyleBackColor = true;
            // 
            // csScriptListView
            // 
            this.csScriptListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.csScriptListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader17,
            this.columnHeader20,
            this.columnHeader18});
            this.csScriptListView.FullRowSelect = true;
            this.csScriptListView.GridLines = true;
            this.csScriptListView.HideSelection = false;
            this.csScriptListView.LabelWrap = false;
            this.csScriptListView.Location = new System.Drawing.Point(2, 1);
            this.csScriptListView.MultiSelect = false;
            this.csScriptListView.Name = "csScriptListView";
            this.csScriptListView.ShowItemToolTips = true;
            this.csScriptListView.Size = new System.Drawing.Size(463, 310);
            this.csScriptListView.TabIndex = 49;
            this.csScriptListView.UseCompatibleStateImageBehavior = false;
            this.csScriptListView.View = System.Windows.Forms.View.Details;
            this.csScriptListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.scriptlistView_ColumnClick);
            this.csScriptListView.SelectedIndexChanged += new System.EventHandler(this.scriptlistView_SelectedIndexChanged);
            this.csScriptListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scriptlistView_MouseClick);
            this.csScriptListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.scriptlistView_MouseDoubleClick);
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Filename";
            this.columnHeader10.Width = 350;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Status";
            this.columnHeader11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader11.Width = 80;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Loop";
            this.columnHeader12.Width = 50;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "A.S.";
            this.columnHeader13.Width = 55;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Wait";
            this.columnHeader14.Width = 40;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "Hot Keys";
            this.columnHeader15.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader15.Width = 80;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "KeyPass";
            this.columnHeader16.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader16.Width = 80;
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "#";
            this.columnHeader17.Width = 40;
            // 
            // columnHeader20
            // 
            this.columnHeader20.Text = "Preload";
            this.columnHeader20.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader20.Width = 50;
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "";
            this.columnHeader18.Width = 0;
            #endregion
            #region Script Control Groups
            // 
            // scriptControlBox
            // 
            this.scriptControlBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptControlBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.scriptControlBox.Controls.Add(this.InspectGumpsButton);
            this.scriptControlBox.Controls.Add(this.InspectContextButton);
            this.scriptControlBox.Controls.Add(this.groupBox30);
            this.scriptControlBox.Controls.Add(this.scriptOperationsBox);
            this.scriptControlBox.Controls.Add(this.groupBox42);
            this.scriptControlBox.Controls.Add(this.scripterrorlogCheckBox);
            this.scriptControlBox.Controls.Add(this.showscriptmessageCheckBox);
            this.scriptControlBox.Controls.Add(this.scriptshowStartStopCheckBox);
            this.scriptControlBox.Controls.Add(this.scriptPacketLogCheckBox);
            this.scriptControlBox.Controls.Add(this.autoScriptReload);
            this.scriptControlBox.Location = new System.Drawing.Point(478, 2);
            this.scriptControlBox.Name = "scriptControlBox";
            this.scriptControlBox.Size = new System.Drawing.Size(202, 465);
            this.scriptControlBox.TabIndex = 1;
            this.scriptControlBox.TabStop = false;
            this.scriptControlBox.Text = "\uE713  Control Panel";
            // 
            // InspectContextButton
            // 
            this.InspectContextButton.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F);
            this.InspectContextButton.Location = new System.Drawing.Point(12, 32);
            this.InspectContextButton.Name = "InspectContextButton";
            this.InspectContextButton.Size = new System.Drawing.Size(83, 26);
            this.InspectContextButton.TabIndex = 77;
            this.InspectContextButton.Text = "\uE721 Inspect";
            this.m_Tip.SetToolTip(this.InspectContextButton, "Inspect Object/Mobile");
            this.InspectContextButton.Click += new System.EventHandler(this.InspectContext_Click);
            // 
            // InspectGumpsButton
            // 
            this.InspectGumpsButton.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F);
            this.InspectGumpsButton.Location = new System.Drawing.Point(100, 32);
            this.InspectGumpsButton.Name = "InspectGumpsButton";
            this.InspectGumpsButton.Size = new System.Drawing.Size(83, 26);
            this.InspectGumpsButton.TabIndex = 78;
            this.InspectGumpsButton.Text = "\uE9F9 Gump";
            this.m_Tip.SetToolTip(this.InspectGumpsButton, "Inspect Last Gump");
            this.InspectGumpsButton.Click += new System.EventHandler(this.InspectGump_Click);
            // 
            // groupBox30
            // 
            this.groupBox30.Controls.Add(this.scriptloopmodecheckbox);
            this.groupBox30.Controls.Add(this.scriptwaitmodecheckbox);
            this.groupBox30.Controls.Add(this.scriptautostartcheckbox);
            this.groupBox30.Controls.Add(this.scriptpreload);
            this.groupBox30.Location = new System.Drawing.Point(8, 65);
            this.groupBox30.Name = "groupBox30";
            this.groupBox30.Size = new System.Drawing.Size(182, 95);
            this.groupBox30.TabIndex = 49;
            this.groupBox30.TabStop = false;
            this.groupBox30.Text = "\uE946  Script Info";
            // 
            // scriptloopmodecheckbox
            // 
            this.scriptloopmodecheckbox.Location = new System.Drawing.Point(10, 26);
            this.scriptloopmodecheckbox.Name = "scriptloopmodecheckbox";
            this.scriptloopmodecheckbox.Size = new System.Drawing.Size(80, 20);
            this.scriptloopmodecheckbox.TabIndex = 49;
            this.scriptloopmodecheckbox.Text = "Loop";
            this.scriptloopmodecheckbox.CheckedChanged += new System.EventHandler(this.scriptloopmodecheckbox_CheckedChanged);
            // 
            // scriptwaitmodecheckbox
            // 
            this.scriptwaitmodecheckbox.Location = new System.Drawing.Point(95, 26);
            this.scriptwaitmodecheckbox.Name = "scriptwaitmodecheckbox";
            this.scriptwaitmodecheckbox.Size = new System.Drawing.Size(80, 20);
            this.scriptwaitmodecheckbox.TabIndex = 50;
            this.scriptwaitmodecheckbox.Text = "Wait";
            this.scriptwaitmodecheckbox.CheckedChanged += new System.EventHandler(this.scriptwaitmodecheckbox_CheckedChanged);
            // 
            // scriptautostartcheckbox
            // 
            this.scriptautostartcheckbox.Location = new System.Drawing.Point(10, 48);
            this.scriptautostartcheckbox.Name = "scriptautostartcheckbox";
            this.scriptautostartcheckbox.Size = new System.Drawing.Size(80, 20);
            this.scriptautostartcheckbox.TabIndex = 51;
            this.scriptautostartcheckbox.Text = "AutoStart";
            this.scriptautostartcheckbox.CheckedChanged += new System.EventHandler(this.scriptautostartcheckbox_CheckedChanged);
            // 
            // scriptpreload
            // 
            this.scriptpreload.Location = new System.Drawing.Point(95, 48);
            this.scriptpreload.Name = "scriptpreload";
            this.scriptpreload.Size = new System.Drawing.Size(80, 20);
            this.scriptpreload.TabIndex = 52;
            this.scriptpreload.Text = "Preload";
            this.scriptpreload.CheckedChanged += new System.EventHandler(this.scriptpreloadcheckbox_CheckedChanged);
            // 
            // scriptOperationsBox
            // 
            this.scriptOperationsBox.Controls.Add(this.buttonScriptPlay);
            this.scriptOperationsBox.Controls.Add(this.buttonScriptStop);
            this.scriptOperationsBox.Controls.Add(this.buttonScriptRefresh);
            this.scriptOperationsBox.Controls.Add(this.buttonAddScript);
            this.scriptOperationsBox.Controls.Add(this.buttonRemoveScript);
            this.scriptOperationsBox.Controls.Add(this.buttonScriptUp);
            this.scriptOperationsBox.Controls.Add(this.buttonScriptDown);
            this.scriptOperationsBox.Controls.Add(this.buttonScriptTo);
            this.scriptOperationsBox.Controls.Add(this.buttonScriptEditorNew);
            this.scriptOperationsBox.Controls.Add(this.buttonScriptEditor);
            this.scriptOperationsBox.Controls.Add(this.textBoxDelay);
            this.scriptOperationsBox.Location = new System.Drawing.Point(8, 165);
            this.scriptOperationsBox.Name = "scriptOperationsBox";
            this.scriptOperationsBox.Size = new System.Drawing.Size(182, 155);
            this.scriptOperationsBox.TabIndex = 50;
            this.scriptOperationsBox.TabStop = false;
            this.scriptOperationsBox.Text = "\uE70F  Operations";
            // 
            // buttonScriptPlay
            // 
            this.buttonScriptPlay.OverrideCustomColor = RazorTheme.Colors.Success;
            this.buttonScriptPlay.Font = new System.Drawing.Font("Segoe MDL2 Assets", 12F, System.Drawing.FontStyle.Bold);
            this.buttonScriptPlay.Location = new System.Drawing.Point(10, 30);
            this.buttonScriptPlay.Name = "buttonScriptPlay";
            this.buttonScriptPlay.Size = new System.Drawing.Size(50, 26);
            this.buttonScriptPlay.TabIndex = 21;
            this.buttonScriptPlay.Text = "\uE768";
            this.m_Tip.SetToolTip(this.buttonScriptPlay, "Play Script");
            this.buttonScriptPlay.Click += new System.EventHandler(this.buttonScriptPlay_Click);
            // 
            // buttonScriptStop
            // 
            this.buttonScriptStop.OverrideCustomColor = RazorTheme.Colors.Danger;
            this.buttonScriptStop.Font = new System.Drawing.Font("Segoe MDL2 Assets", 12F, System.Drawing.FontStyle.Bold);
            this.buttonScriptStop.Location = new System.Drawing.Point(65, 30);
            this.buttonScriptStop.Name = "buttonScriptStop";
            this.buttonScriptStop.Size = new System.Drawing.Size(50, 26);
            this.buttonScriptStop.TabIndex = 22;
            this.buttonScriptStop.Text = "\uE71A";
            this.m_Tip.SetToolTip(this.buttonScriptStop, "Stop Script");
            this.buttonScriptStop.Click += new System.EventHandler(this.buttonScriptStop_Click);
            // 
            // buttonScriptRefresh
            // 
            this.buttonScriptRefresh.Font = new System.Drawing.Font("Segoe MDL2 Assets", 12F, System.Drawing.FontStyle.Bold);
            this.buttonScriptRefresh.Location = new System.Drawing.Point(120, 30);
            this.buttonScriptRefresh.Name = "buttonScriptRefresh";
            this.buttonScriptRefresh.Size = new System.Drawing.Size(50, 26);
            this.buttonScriptRefresh.TabIndex = 73;
            this.buttonScriptRefresh.Text = "\uE72C";
            this.m_Tip.SetToolTip(this.buttonScriptRefresh, "Refresh Scripts");
            this.buttonScriptRefresh.Click += new System.EventHandler(this.buttonScriptRefresh_Click);
            // 
            // buttonAddScript
            // 
            this.buttonAddScript.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9F, System.Drawing.FontStyle.Bold);
            this.buttonAddScript.Location = new System.Drawing.Point(10, 60);
            this.buttonAddScript.Name = "buttonAddScript";
            this.buttonAddScript.Size = new System.Drawing.Size(78, 24);
            this.buttonAddScript.TabIndex = 14;
            this.buttonAddScript.Text = "\uE710 Add";
            this.buttonAddScript.Click += new System.EventHandler(this.buttonScriptAdd_Click);
            // 
            // buttonRemoveScript
            // 
            this.buttonRemoveScript.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9F, System.Drawing.FontStyle.Bold);
            this.buttonRemoveScript.Location = new System.Drawing.Point(92, 60);
            this.buttonRemoveScript.Name = "buttonRemoveScript";
            this.buttonRemoveScript.Size = new System.Drawing.Size(78, 24);
            this.buttonRemoveScript.TabIndex = 15;
            this.buttonRemoveScript.Text = "\uE738 Rem";
            this.buttonRemoveScript.Click += new System.EventHandler(this.buttonScriptRemove_Click);
            // 
            // buttonScriptUp
            // 
            this.buttonScriptUp.OverrideCustomColor = RazorTheme.Colors.Success;
            this.buttonScriptUp.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F, System.Drawing.FontStyle.Bold);
            this.buttonScriptUp.Location = new System.Drawing.Point(10, 88);
            this.buttonScriptUp.Name = "buttonScriptUp";
            this.buttonScriptUp.Size = new System.Drawing.Size(50, 24);
            this.buttonScriptUp.TabIndex = 18;
            this.buttonScriptUp.Text = "\uE74A";
            this.m_Tip.SetToolTip(this.buttonScriptUp, "Move Up");
            this.buttonScriptUp.Click += new System.EventHandler(this.buttonScriptUp_Click);
            // 
            // buttonScriptDown
            // 
            this.buttonScriptDown.OverrideCustomColor = RazorTheme.Colors.Warning;
            this.buttonScriptDown.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F, System.Drawing.FontStyle.Bold);
            this.buttonScriptDown.Location = new System.Drawing.Point(65, 88);
            this.buttonScriptDown.Name = "buttonScriptDown";
            this.buttonScriptDown.Size = new System.Drawing.Size(50, 24);
            this.buttonScriptDown.TabIndex = 17;
            this.buttonScriptDown.Text = "\uE74B";
            this.m_Tip.SetToolTip(this.buttonScriptDown, "Move Down");
            this.buttonScriptDown.Click += new System.EventHandler(this.buttonScriptDown_Click);
            // 
            // buttonScriptTo
            // 
            this.buttonScriptTo.Font = new System.Drawing.Font("Segoe MDL2 Assets", 10F, System.Drawing.FontStyle.Bold);
            this.buttonScriptTo.Location = new System.Drawing.Point(120, 88);
            this.buttonScriptTo.Name = "buttonScriptTo";
            this.buttonScriptTo.Size = new System.Drawing.Size(50, 24);
            this.buttonScriptTo.TabIndex = 75;
            this.buttonScriptTo.Text = "\uE8DE";
            this.m_Tip.SetToolTip(this.buttonScriptTo, "Move To...");
            this.buttonScriptTo.Click += new System.EventHandler(this.buttonScriptTo_Click);
            // 
            // buttonScriptEditorNew
            // 
            this.buttonScriptEditorNew.OverrideCustomColor = RazorTheme.Colors.Success;
            this.buttonScriptEditorNew.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9F, System.Drawing.FontStyle.Bold);
            this.buttonScriptEditorNew.Location = new System.Drawing.Point(10, 116);
            this.buttonScriptEditorNew.Name = "buttonScriptEditorNew";
            this.buttonScriptEditorNew.Size = new System.Drawing.Size(78, 24);
            this.buttonScriptEditorNew.TabIndex = 74;
            this.buttonScriptEditorNew.Text = "\uE7C3 New";
            this.buttonScriptEditorNew.Click += new System.EventHandler(this.buttonScriptEditorNew_Click);
            // 
            // buttonScriptEditor
            // 
            this.buttonScriptEditor.OverrideCustomColor = RazorTheme.Colors.Warning;
            this.buttonScriptEditor.Font = new System.Drawing.Font("Segoe MDL2 Assets", 9F, System.Drawing.FontStyle.Bold);
            this.buttonScriptEditor.Location = new System.Drawing.Point(92, 116);
            this.buttonScriptEditor.Name = "buttonScriptEditor";
            this.buttonScriptEditor.Size = new System.Drawing.Size(78, 24);
            this.buttonScriptEditor.TabIndex = 20;
            this.buttonScriptEditor.Text = "\uE70F Edit";
            this.buttonScriptEditor.Click += new System.EventHandler(this.buttonOpenEditor_Click);
            // 
            // textBoxDelay
            // 
            this.textBoxDelay.Location = new System.Drawing.Point(116, 204);
            this.textBoxDelay.Name = "textBoxDelay";
            this.textBoxDelay.Size = new System.Drawing.Size(42, 20);
            this.textBoxDelay.TabIndex = 23;
            this.textBoxDelay.Text = "100";
            this.textBoxDelay.Visible = false;
            // 
            // groupBox42
            // 
            this.groupBox42.Controls.Add(this.scriptSearchTextBox);
            this.groupBox42.Location = new System.Drawing.Point(8, 325);
            this.groupBox42.Name = "groupBox42";
            this.groupBox42.Size = new System.Drawing.Size(182, 45);
            this.groupBox42.TabIndex = 75;
            this.groupBox42.TabStop = false;
            this.groupBox42.Text = "\uE721  Search";
            // 
            // scriptSearchTextBox
            // 
            this.scriptSearchTextBox.Location = new System.Drawing.Point(10, 22);
            this.scriptSearchTextBox.Name = "scriptSearchTextBox";
            this.scriptSearchTextBox.Size = new System.Drawing.Size(160, 20);
            this.scriptSearchTextBox.TabIndex = 0;
            this.scriptSearchTextBox.TextChanged += new System.EventHandler(this.scriptSearchTextBox_TextChanged);
            // 
            // scripterrorlogCheckBox
            // 
            this.scripterrorlogCheckBox.Location = new System.Drawing.Point(12, 375);
            this.scripterrorlogCheckBox.Name = "scripterrorlogCheckBox";
            this.scripterrorlogCheckBox.Size = new System.Drawing.Size(175, 20);
            this.scripterrorlogCheckBox.TabIndex = 74;
            this.scripterrorlogCheckBox.Text = "Log Script Error";
            this.scripterrorlogCheckBox.CheckedChanged += new System.EventHandler(this.scripterrorlogCheckBox_CheckedChanged);
            // 
            // showscriptmessageCheckBox
            // 
            this.showscriptmessageCheckBox.Location = new System.Drawing.Point(12, 395);
            this.showscriptmessageCheckBox.Name = "showscriptmessageCheckBox";
            this.showscriptmessageCheckBox.Size = new System.Drawing.Size(175, 20);
            this.showscriptmessageCheckBox.TabIndex = 72;
            this.showscriptmessageCheckBox.Text = "Show Script Error Msg";
            this.showscriptmessageCheckBox.CheckedChanged += new System.EventHandler(this.showscriptmessageCheckBox_CheckedChanged);
            // 
            // scriptshowStartStopCheckBox
            // 
            this.scriptshowStartStopCheckBox.Location = new System.Drawing.Point(12, 415);
            this.scriptshowStartStopCheckBox.Name = "scriptshowStartStopCheckBox";
            this.scriptshowStartStopCheckBox.Size = new System.Drawing.Size(175, 20);
            this.scriptshowStartStopCheckBox.TabIndex = 76;
            this.scriptshowStartStopCheckBox.Text = "Show Start/Stop Msg";
            this.scriptshowStartStopCheckBox.CheckedChanged += new System.EventHandler(this.scriptshowStartStopCheckBox_CheckedChanged);
            // 
            // scriptPacketLogCheckBox
            // 
            this.scriptPacketLogCheckBox.Location = new System.Drawing.Point(12, 435);
            this.scriptPacketLogCheckBox.Name = "scriptPacketLogCheckBox";
            this.scriptPacketLogCheckBox.Size = new System.Drawing.Size(175, 20);
            this.scriptPacketLogCheckBox.TabIndex = 79;
            this.scriptPacketLogCheckBox.Text = "Enable Packet Logging";
            this.scriptPacketLogCheckBox.CheckStateChanged += new System.EventHandler(this.scriptPacketLogCheckBox_CheckStateChanged);
            // 
            // autoScriptReload
            // 
            this.autoScriptReload.Location = new System.Drawing.Point(12, 455);
            this.autoScriptReload.Name = "autoScriptReload";
            this.autoScriptReload.Size = new System.Drawing.Size(175, 20);
            this.autoScriptReload.TabIndex = 80;
            this.autoScriptReload.Text = "Auto Script Reload";
            this.m_Tip.SetToolTip(this.autoScriptReload, "Automatically reload scripts modified externally");
            this.autoScriptReload.CheckedChanged += new System.EventHandler(this.autoScriptReload_CheckedChanged);
            #endregion
            #endregion
            #region Macros Tab
            // 
            // MacrosTab
            // 
            this.MacrosTab.Location = new System.Drawing.Point(4, 29);
            this.MacrosTab.Name = "MacrosTab";
            this.MacrosTab.Padding = new System.Windows.Forms.Padding(3);
            this.MacrosTab.Size = new System.Drawing.Size(678, 390);
            this.MacrosTab.TabIndex = 16;
            this.MacrosTab.Text = "Macros";
            this.MacrosTab.UseVisualStyleBackColor = true;
            // Initialize Macro Tab UI
            InitializeMacroTab();
            #endregion
            #region Agents Tab
            // 
            // EnhancedAgent
            // 
            this.EnhancedAgent.Controls.Add(this.tabControl1);
            this.EnhancedAgent.Location = new System.Drawing.Point(4, 54);
            this.EnhancedAgent.Name = "EnhancedAgent";
            this.EnhancedAgent.Padding = new System.Windows.Forms.Padding(3);
            this.EnhancedAgent.Size = new System.Drawing.Size(678, 365);
            this.EnhancedAgent.TabIndex = 14;
            this.EnhancedAgent.Text = "Agents";
            this.EnhancedAgent.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.eautoloot);
            this.tabControl1.Controls.Add(this.escavenger);
            this.tabControl1.Controls.Add(this.organizer);
            this.tabControl1.Controls.Add(this.VendorBuy);
            this.tabControl1.Controls.Add(this.VendorSell);
            this.tabControl1.Controls.Add(this.Dress);
            this.tabControl1.Controls.Add(this.friends);
            this.tabControl1.Controls.Add(this.restock);
            this.tabControl1.Controls.Add(this.bandageheal);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(685, 373);
            this.tabControl1.TabIndex = 0;
            #region Auto Loot Tab
            // 
            // eautoloot
            // 
            this.eautoloot.Controls.Add(this.autoLootButtonListClone);
            this.eautoloot.Controls.Add(this.autolootautostartCheckBox);
            this.eautoloot.Controls.Add(this.label60);
            this.eautoloot.Controls.Add(this.autoLootTextBoxMaxRange);
            this.eautoloot.Controls.Add(this.autolootItemPropsB);
            this.eautoloot.Controls.Add(this.groupBox14);
            this.eautoloot.Controls.Add(this.autolootAddItemBTarget);
            this.eautoloot.Controls.Add(this.autolootdataGridView);
            this.eautoloot.Controls.Add(this.autoLootnoopenCheckBox);
            this.eautoloot.Controls.Add(this.label21);
            this.eautoloot.Controls.Add(this.autoLootTextBoxDelay);
            this.eautoloot.Controls.Add(this.autoLootButtonRemoveList);
            this.eautoloot.Controls.Add(this.autolootButtonAddList);
            this.eautoloot.Controls.Add(this.autolootListSelect);
            this.eautoloot.Controls.Add(this.allowHiddenLooting);
            this.eautoloot.Controls.Add(this.label20);
            this.eautoloot.Controls.Add(this.groupBox13);
            this.eautoloot.Controls.Add(this.autoLootCheckBox);
            this.eautoloot.Location = new System.Drawing.Point(4, 23);
            this.eautoloot.Name = "eautoloot";
            this.eautoloot.Padding = new System.Windows.Forms.Padding(3);
            this.eautoloot.Size = new System.Drawing.Size(677, 346);
            this.eautoloot.TabIndex = 0;
            this.eautoloot.Text = "Autoloot";
            this.eautoloot.UseVisualStyleBackColor = true;
            // 
            // allowHiddenLooting
            // 
            this.allowHiddenLooting.Location = new System.Drawing.Point(510, 12);
            this.allowHiddenLooting.Name = "allowHiddenLooting";
            this.allowHiddenLooting.Size = new System.Drawing.Size(190, 22);
            this.allowHiddenLooting.TabIndex = 82;
            this.allowHiddenLooting.Text = "Allow looting while hidden";
            this.allowHiddenLooting.CheckedChanged += new System.EventHandler(this.hiddenLooting_CheckedChanged);
            // 
            // autoLootButtonListClone
            // 
            this.autoLootButtonListClone.Location = new System.Drawing.Point(424, 12);
            this.autoLootButtonListClone.Name = "autoLootButtonListClone";
            this.autoLootButtonListClone.Size = new System.Drawing.Size(70, 21);
            this.autoLootButtonListClone.TabIndex = 67;
            this.autoLootButtonListClone.Text = "Clone";
            this.autoLootButtonListClone.Click += new System.EventHandler(this.autoLootButtonListClone_Click);
            // 
            // autolootautostartCheckBox
            // 
            this.autolootautostartCheckBox.Location = new System.Drawing.Point(275, 73);
            this.autolootautostartCheckBox.Name = "autolootautostartCheckBox";
            this.autolootautostartCheckBox.Size = new System.Drawing.Size(126, 22);
            this.autolootautostartCheckBox.TabIndex = 66;
            this.autolootautostartCheckBox.Text = "Autostart OnLogin";
            this.autolootautostartCheckBox.CheckedChanged += new System.EventHandler(this.autolootautostartCheckBox_CheckedChanged);
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Location = new System.Drawing.Point(464, 68);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(61, 14);
            this.label60.TabIndex = 65;
            this.label60.Text = "Max Range";
            // 
            // autoLootTextBoxMaxRange
            // 
            this.autoLootTextBoxMaxRange.BackColor = System.Drawing.Color.White;
            this.autoLootTextBoxMaxRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.autoLootTextBoxMaxRange.Location = new System.Drawing.Point(411, 65);
            this.autoLootTextBoxMaxRange.Name = "autoLootTextBoxMaxRange";
            this.autoLootTextBoxMaxRange.Size = new System.Drawing.Size(45, 20);
            this.autoLootTextBoxMaxRange.TabIndex = 64;
            this.autoLootTextBoxMaxRange.Leave += new System.EventHandler(this.autoLootTextBoxMaxRange_Leave);
            // 
            // autolootItemPropsB
            // 
            this.autolootItemPropsB.Location = new System.Drawing.Point(540, 66);
            this.autolootItemPropsB.Name = "autolootItemPropsB";
            this.autolootItemPropsB.Size = new System.Drawing.Size(90, 21);
            this.autolootItemPropsB.TabIndex = 49;
            this.autolootItemPropsB.Text = "Edit Props";
            this.autolootItemPropsB.Click += new System.EventHandler(this.autoLootItemProps_Click);
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.label55);
            this.groupBox14.Controls.Add(this.autolootContainerLabel);
            this.groupBox14.Controls.Add(this.autolootContainerButton);
            this.groupBox14.Location = new System.Drawing.Point(9, 42);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(252, 47);
            this.groupBox14.TabIndex = 63;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "AutoLoot Bag";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(6, 21);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(37, 14);
            this.label55.TabIndex = 90;
            this.label55.Text = "Serial:";
            // 
            // autolootContainerLabel
            // 
            this.autolootContainerLabel.Location = new System.Drawing.Point(48, 21);
            this.autolootContainerLabel.Name = "autolootContainerLabel";
            this.autolootContainerLabel.Size = new System.Drawing.Size(82, 19);
            this.autolootContainerLabel.TabIndex = 50;
            this.autolootContainerLabel.Text = "0x00000000";
            // 
            // autolootContainerButton
            // 
            this.autolootContainerButton.Location = new System.Drawing.Point(157, 16);
            this.autolootContainerButton.Name = "autolootContainerButton";
            this.autolootContainerButton.Size = new System.Drawing.Size(89, 21);
            this.autolootContainerButton.TabIndex = 49;
            this.autolootContainerButton.Text = "Set Bag";
            this.autolootContainerButton.Click += new System.EventHandler(this.autolootContainerButton_Click);
            // 
            // autolootAddItemBTarget
            // 
            this.autolootAddItemBTarget.Location = new System.Drawing.Point(540, 39);
            this.autolootAddItemBTarget.Name = "autolootAddItemBTarget";
            this.autolootAddItemBTarget.Size = new System.Drawing.Size(90, 21);
            this.autolootAddItemBTarget.TabIndex = 47;
            this.autolootAddItemBTarget.Text = "Add Item";
            this.autolootAddItemBTarget.Click += new System.EventHandler(this.autoLootAddItemTarget_Click);
            // 
            // autolootdataGridView
            // 
            this.autolootdataGridView.AllowDrop = true;
            this.autolootdataGridView.AllowUserToResizeRows = false;
            this.autolootdataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.autolootdataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.autolootdataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.autolootdataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.AutolootColumnX,
            this.AutolootColumnItemName,
            this.AutolootColumnItemID,
            this.AutolootColumnColor,
            this.LootBagColumnID,
            this.AutolootColumnProps});
            this.autolootdataGridView.Location = new System.Drawing.Point(9, 95);
            this.autolootdataGridView.Name = "autolootdataGridView";
            this.autolootdataGridView.RowHeadersVisible = false;
            this.autolootdataGridView.RowHeadersWidth = 62;
            this.autolootdataGridView.Size = new System.Drawing.Size(410, 223);
            this.autolootdataGridView.TabIndex = 62;
            this.autolootdataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.autolootdataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.autolootdataGridView_CellEndEdit);
            this.autolootdataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.autolootdataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.autolootdataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.autolootdataGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.autolootdataGridView_DefaultValuesNeeded);
            this.autolootdataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.autolootdataGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.autolootdataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.autolootdataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // AutolootColumnX
            // 
            this.AutolootColumnX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.AutolootColumnX.FalseValue = "False";
            this.AutolootColumnX.FillWeight = 1F;
            this.AutolootColumnX.Frozen = true;
            this.AutolootColumnX.HeaderText = "X";
            this.AutolootColumnX.IndeterminateValue = "False";
            this.AutolootColumnX.MinimumWidth = 30;
            this.AutolootColumnX.Name = "AutolootColumnX";
            this.AutolootColumnX.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.AutolootColumnX.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.AutolootColumnX.TrueValue = "True";
            this.AutolootColumnX.Width = 30;
            // 
            // AutolootColumnItemName
            // 
            this.AutolootColumnItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.AutolootColumnItemName.FillWeight = 69F;
            this.AutolootColumnItemName.HeaderText = "Item Name";
            this.AutolootColumnItemName.MinimumWidth = 50;
            this.AutolootColumnItemName.Name = "AutolootColumnItemName";
            // 
            // AutolootColumnItemID
            // 
            this.AutolootColumnItemID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            this.AutolootColumnItemID.FillWeight = 10F;
            this.AutolootColumnItemID.HeaderText = "Graphics";
            this.AutolootColumnItemID.MinimumWidth = 80;
            this.AutolootColumnItemID.Name = "AutolootColumnItemID";
            this.AutolootColumnItemID.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.AutolootColumnItemID.Width = 80;
            // 
            // AutolootColumnColor
            // 
            this.AutolootColumnColor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            this.AutolootColumnColor.FillWeight = 10F;
            this.AutolootColumnColor.HeaderText = "Color";
            this.AutolootColumnColor.MinimumWidth = 80;
            this.AutolootColumnColor.Name = "AutolootColumnColor";
            this.AutolootColumnColor.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.AutolootColumnColor.Width = 80;
            // 
            // LootBagColumnID
            // 
            this.LootBagColumnID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            this.LootBagColumnID.FillWeight = 10F;
            this.LootBagColumnID.HeaderText = "Bag";
            this.LootBagColumnID.MaxInputLength = 65535;
            this.LootBagColumnID.MinimumWidth = 80;
            this.LootBagColumnID.Name = "LootBagColumnID";
            this.LootBagColumnID.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.LootBagColumnID.Width = 80;
            // 
            // AutolootColumnProps
            // 
            this.AutolootColumnProps.HeaderText = "Props";
            this.AutolootColumnProps.MinimumWidth = 8;
            this.AutolootColumnProps.Name = "AutolootColumnProps";
            this.AutolootColumnProps.Visible = false;
            // 
            // autoLootnoopenCheckBox
            // 
            this.autoLootnoopenCheckBox.Location = new System.Drawing.Point(275, 55);
            this.autoLootnoopenCheckBox.Name = "autoLootnoopenCheckBox";
            this.autoLootnoopenCheckBox.Size = new System.Drawing.Size(126, 22);
            this.autoLootnoopenCheckBox.TabIndex = 61;
            this.autoLootnoopenCheckBox.Text = "No Open Corpse";
            this.autoLootnoopenCheckBox.CheckedChanged += new System.EventHandler(this.autoLootnoopenCheckBox_CheckedChanged);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(464, 44);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(59, 14);
            this.label21.TabIndex = 59;
            this.label21.Text = "Delay (ms)";
            // 
            // autoLootTextBoxDelay
            // 
            this.autoLootTextBoxDelay.BackColor = System.Drawing.Color.White;
            this.autoLootTextBoxDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.autoLootTextBoxDelay.Location = new System.Drawing.Point(411, 41);
            this.autoLootTextBoxDelay.Name = "autoLootTextBoxDelay";
            this.autoLootTextBoxDelay.Size = new System.Drawing.Size(45, 20);
            this.autoLootTextBoxDelay.TabIndex = 58;
            this.autoLootTextBoxDelay.Leave += new System.EventHandler(this.autoLootTextBoxDelay_Leave);
            // 
            // autoLootButtonRemoveList
            // 
            this.autoLootButtonRemoveList.Location = new System.Drawing.Point(347, 12);
            this.autoLootButtonRemoveList.Name = "autoLootButtonRemoveList";
            this.autoLootButtonRemoveList.Size = new System.Drawing.Size(71, 21);
            this.autoLootButtonRemoveList.TabIndex = 57;
            this.autoLootButtonRemoveList.Text = "Remove";
            this.autoLootButtonRemoveList.Click += new System.EventHandler(this.autoLootButtonRemoveList_Click);
            // 
            // autolootButtonAddList
            // 
            this.autolootButtonAddList.Location = new System.Drawing.Point(270, 12);
            this.autolootButtonAddList.Name = "autolootButtonAddList";
            this.autolootButtonAddList.Size = new System.Drawing.Size(71, 21);
            this.autolootButtonAddList.TabIndex = 56;
            this.autolootButtonAddList.Text = "Add";
            this.autolootButtonAddList.Click += new System.EventHandler(this.autoLootButtonAddList_Click);
            // 
            // autolootListSelect
            // 
            this.autolootListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.autolootListSelect.FormattingEnabled = true;
            this.autolootListSelect.Location = new System.Drawing.Point(78, 12);
            this.autolootListSelect.Name = "autolootListSelect";
            this.autolootListSelect.Size = new System.Drawing.Size(183, 22);
            this.autolootListSelect.TabIndex = 55;
            this.autolootListSelect.SelectedIndexChanged += new System.EventHandler(this.autoLootListSelect_SelectedIndexChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(6, 18);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(70, 14);
            this.label20.TabIndex = 54;
            this.label20.Text = "Autoloot List:";
            // 
            // groupBox13
            // 
            this.groupBox13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox13.Controls.Add(this.autolootLogBox);
            this.groupBox13.Location = new System.Drawing.Point(425, 94);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(243, 224);
            this.groupBox13.TabIndex = 53;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "Autoloot Log";
            // 
            // autolootLogBox
            // 
            this.autolootLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.autolootLogBox.FormattingEnabled = true;
            this.autolootLogBox.ItemHeight = 14;
            this.autolootLogBox.Location = new System.Drawing.Point(6, 19);
            this.autolootLogBox.Name = "autolootLogBox";
            this.autolootLogBox.Size = new System.Drawing.Size(232, 4);
            this.autolootLogBox.TabIndex = 0;
            // 
            // autoLootCheckBox
            // 
            this.autoLootCheckBox.Location = new System.Drawing.Point(275, 37);
            this.autoLootCheckBox.Name = "autoLootCheckBox";
            this.autoLootCheckBox.Size = new System.Drawing.Size(126, 22);
            this.autoLootCheckBox.TabIndex = 48;
            this.autoLootCheckBox.Text = "Enable autoloot";
            this.autoLootCheckBox.CheckedChanged += new System.EventHandler(this.autoLootEnable_CheckedChanged);
            #endregion
            #region Scavenger Tab
            // 
            // escavenger
            // 
            this.escavenger.Controls.Add(this.scavengerButtonClone);
            this.escavenger.Controls.Add(this.scavengerautostartCheckBox);
            this.escavenger.Controls.Add(this.label61);
            this.escavenger.Controls.Add(this.groupBox41);
            this.escavenger.Controls.Add(this.scavengerdataGridView);
            this.escavenger.Controls.Add(this.groupBox12);
            this.escavenger.Controls.Add(this.label23);
            this.escavenger.Controls.Add(this.label22);
            this.escavenger.Controls.Add(this.scavengerButtonEditProps);
            this.escavenger.Controls.Add(this.scavengerButtonAddTarget);
            this.escavenger.Controls.Add(this.scavengerCheckBox);
            this.escavenger.Controls.Add(this.scavengerButtonRemoveList);
            this.escavenger.Controls.Add(this.scavengerButtonAddList);
            this.escavenger.Controls.Add(this.scavengerListSelect);
            this.escavenger.Controls.Add(this.scavengerRange);
            this.escavenger.Controls.Add(this.scavengerDragDelay);
            this.escavenger.Location = new System.Drawing.Point(4, 22);
            this.escavenger.Name = "escavenger";
            this.escavenger.Padding = new System.Windows.Forms.Padding(3);
            this.escavenger.Size = new System.Drawing.Size(677, 347);
            this.escavenger.TabIndex = 1;
            this.escavenger.Text = "Scavenger";
            this.escavenger.UseVisualStyleBackColor = true;
            // 
            // scavengerButtonClone
            // 
            this.scavengerButtonClone.Location = new System.Drawing.Point(423, 11);
            this.scavengerButtonClone.Name = "scavengerButtonClone";
            this.scavengerButtonClone.Size = new System.Drawing.Size(68, 21);
            this.scavengerButtonClone.TabIndex = 77;
            this.scavengerButtonClone.Text = "Clone";
            this.scavengerButtonClone.Click += new System.EventHandler(this.scavengerButtonClone_Click);
            // 
            // scavengerautostartCheckBox
            // 
            this.scavengerautostartCheckBox.Location = new System.Drawing.Point(275, 67);
            this.scavengerautostartCheckBox.Name = "scavengerautostartCheckBox";
            this.scavengerautostartCheckBox.Size = new System.Drawing.Size(126, 22);
            this.scavengerautostartCheckBox.TabIndex = 76;
            this.scavengerautostartCheckBox.Text = "Autostart OnLogin";
            this.scavengerautostartCheckBox.CheckedChanged += new System.EventHandler(this.scavengerautostartCheckBox_CheckedChanged);
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Location = new System.Drawing.Point(469, 71);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(61, 14);
            this.label61.TabIndex = 75;
            this.label61.Text = "Max Range";
            // 
            // groupBox41
            // 
            this.groupBox41.Controls.Add(this.label54);
            this.groupBox41.Controls.Add(this.scavengerContainerLabel);
            this.groupBox41.Controls.Add(this.scavengerButtonSetContainer);
            this.groupBox41.Location = new System.Drawing.Point(9, 42);
            this.groupBox41.Name = "groupBox41";
            this.groupBox41.Size = new System.Drawing.Size(257, 47);
            this.groupBox41.TabIndex = 73;
            this.groupBox41.TabStop = false;
            this.groupBox41.Text = "Scavenger Bag";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(6, 21);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(37, 14);
            this.label54.TabIndex = 89;
            this.label54.Text = "Serial:";
            // 
            // scavengerContainerLabel
            // 
            this.scavengerContainerLabel.Location = new System.Drawing.Point(48, 21);
            this.scavengerContainerLabel.Name = "scavengerContainerLabel";
            this.scavengerContainerLabel.Size = new System.Drawing.Size(82, 19);
            this.scavengerContainerLabel.TabIndex = 67;
            this.scavengerContainerLabel.Text = "0x00000000";
            // 
            // scavengerButtonSetContainer
            // 
            this.scavengerButtonSetContainer.Location = new System.Drawing.Point(161, 16);
            this.scavengerButtonSetContainer.Name = "scavengerButtonSetContainer";
            this.scavengerButtonSetContainer.Size = new System.Drawing.Size(90, 21);
            this.scavengerButtonSetContainer.TabIndex = 66;
            this.scavengerButtonSetContainer.Text = "Set Bag";
            this.scavengerButtonSetContainer.Click += new System.EventHandler(this.scavengerSetContainer_Click);
            // 
            // scavengerdataGridView
            // 
            this.scavengerdataGridView.AllowDrop = true;
            this.scavengerdataGridView.AllowUserToResizeRows = false;
            this.scavengerdataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scavengerdataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.scavengerdataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.scavengerdataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ScavengerX,
            this.ScavengerItemName,
            this.ScavenerGraphics,
            this.ScavengerColor,
            this.ScavengerProp});
            this.scavengerdataGridView.Location = new System.Drawing.Point(9, 95);
            this.scavengerdataGridView.Name = "scavengerdataGridView";
            this.scavengerdataGridView.RowHeadersVisible = false;
            this.scavengerdataGridView.RowHeadersWidth = 62;
            this.scavengerdataGridView.Size = new System.Drawing.Size(374, 218);
            this.scavengerdataGridView.TabIndex = 72;
            this.scavengerdataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.scavengerdataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.scavengerdataGridView_CellEndEdit);
            this.scavengerdataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.scavengerdataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.scavengerdataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.scavengerdataGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.scavengerdataGridView_DefaultValuesNeeded);
            this.scavengerdataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.scavengerdataGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.scavengerdataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.scavengerdataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // ScavengerX
            // 
            this.ScavengerX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ScavengerX.FalseValue = "False";
            this.ScavengerX.FillWeight = 1F;
            this.ScavengerX.HeaderText = "X";
            this.ScavengerX.IndeterminateValue = "False";
            this.ScavengerX.MinimumWidth = 10;
            this.ScavengerX.Name = "ScavengerX";
            this.ScavengerX.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ScavengerX.TrueValue = "True";
            this.ScavengerX.Width = 21;
            // 
            // ScavengerItemName
            // 
            this.ScavengerItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ScavengerItemName.FillWeight = 80F;
            this.ScavengerItemName.HeaderText = "Item Name";
            this.ScavengerItemName.MinimumWidth = 8;
            this.ScavengerItemName.Name = "ScavengerItemName";
            // 
            // ScavenerGraphics
            // 
            this.ScavenerGraphics.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ScavenerGraphics.FillWeight = 10F;
            this.ScavenerGraphics.HeaderText = "Graphics";
            this.ScavenerGraphics.MinimumWidth = 8;
            this.ScavenerGraphics.Name = "ScavenerGraphics";
            this.ScavenerGraphics.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ScavenerGraphics.Width = 76;
            // 
            // ScavengerColor
            // 
            this.ScavengerColor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ScavengerColor.FillWeight = 10F;
            this.ScavengerColor.HeaderText = "Color";
            this.ScavengerColor.MinimumWidth = 8;
            this.ScavengerColor.Name = "ScavengerColor";
            this.ScavengerColor.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ScavengerColor.Width = 57;
            // 
            // ScavengerProp
            // 
            this.ScavengerProp.HeaderText = "Prop";
            this.ScavengerProp.MinimumWidth = 8;
            this.ScavengerProp.Name = "ScavengerProp";
            this.ScavengerProp.Visible = false;
            // 
            // groupBox12
            // 
            this.groupBox12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox12.Controls.Add(this.scavengerLogBox);
            this.groupBox12.Location = new System.Drawing.Point(389, 94);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(279, 219);
            this.groupBox12.TabIndex = 70;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Scavenger Log";
            // 
            // scavengerLogBox
            // 
            this.scavengerLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scavengerLogBox.FormattingEnabled = true;
            this.scavengerLogBox.ItemHeight = 14;
            this.scavengerLogBox.Location = new System.Drawing.Point(7, 18);
            this.scavengerLogBox.Name = "scavengerLogBox";
            this.scavengerLogBox.Size = new System.Drawing.Size(265, 74);
            this.scavengerLogBox.TabIndex = 0;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(469, 45);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(59, 14);
            this.label23.TabIndex = 69;
            this.label23.Text = "Delay (ms)";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(6, 18);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(83, 14);
            this.label22.TabIndex = 60;
            this.label22.Text = "Scavenger List:";
            // 
            // scavengerButtonEditProps
            // 
            this.scavengerButtonEditProps.Location = new System.Drawing.Point(540, 63);
            this.scavengerButtonEditProps.Name = "scavengerButtonEditProps";
            this.scavengerButtonEditProps.Size = new System.Drawing.Size(90, 21);
            this.scavengerButtonEditProps.TabIndex = 49;
            this.scavengerButtonEditProps.Text = "Edit Props";
            this.scavengerButtonEditProps.Click += new System.EventHandler(this.scavengerEditProps_Click);
            // 
            // scavengerButtonAddTarget
            // 
            this.scavengerButtonAddTarget.Location = new System.Drawing.Point(540, 39);
            this.scavengerButtonAddTarget.Name = "scavengerButtonAddTarget";
            this.scavengerButtonAddTarget.Size = new System.Drawing.Size(90, 21);
            this.scavengerButtonAddTarget.TabIndex = 47;
            this.scavengerButtonAddTarget.Text = "Add Item";
            this.scavengerButtonAddTarget.Click += new System.EventHandler(this.scavengerAddItemTarget_Click);
            // 
            // scavengerCheckBox
            // 
            this.scavengerCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.scavengerCheckBox.Location = new System.Drawing.Point(275, 47);
            this.scavengerCheckBox.Name = "scavengerCheckBox";
            this.scavengerCheckBox.Size = new System.Drawing.Size(115, 22);
            this.scavengerCheckBox.TabIndex = 65;
            this.scavengerCheckBox.Text = "Enable scavenger";
            this.scavengerCheckBox.CheckedChanged += new System.EventHandler(this.scavengerEnableCheck_CheckedChanged);
            // 
            // scavengerButtonRemoveList
            // 
            this.scavengerButtonRemoveList.Location = new System.Drawing.Point(349, 12);
            this.scavengerButtonRemoveList.Name = "scavengerButtonRemoveList";
            this.scavengerButtonRemoveList.Size = new System.Drawing.Size(68, 21);
            this.scavengerButtonRemoveList.TabIndex = 63;
            this.scavengerButtonRemoveList.Text = "Remove";
            this.scavengerButtonRemoveList.Click += new System.EventHandler(this.scavengerRemoveList_Click);
            // 
            // scavengerButtonAddList
            // 
            this.scavengerButtonAddList.Location = new System.Drawing.Point(275, 12);
            this.scavengerButtonAddList.Name = "scavengerButtonAddList";
            this.scavengerButtonAddList.Size = new System.Drawing.Size(68, 21);
            this.scavengerButtonAddList.TabIndex = 62;
            this.scavengerButtonAddList.Text = "Add";
            this.scavengerButtonAddList.Click += new System.EventHandler(this.scavengerAddList_Click);
            // 
            // scavengerListSelect
            // 
            this.scavengerListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scavengerListSelect.FormattingEnabled = true;
            this.scavengerListSelect.Location = new System.Drawing.Point(91, 12);
            this.scavengerListSelect.Name = "scavengerListSelect";
            this.scavengerListSelect.Size = new System.Drawing.Size(175, 22);
            this.scavengerListSelect.TabIndex = 61;
            this.scavengerListSelect.SelectedIndexChanged += new System.EventHandler(this.scavengertListSelect_SelectedIndexChanged);
            // 
            // scavengerRange
            // 
            this.scavengerRange.BackColor = System.Drawing.Color.White;
            this.scavengerRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scavengerRange.Location = new System.Drawing.Point(416, 68);
            this.scavengerRange.Name = "scavengerRange";
            this.scavengerRange.Size = new System.Drawing.Size(45, 20);
            this.scavengerRange.TabIndex = 74;
            this.scavengerRange.Leave += new System.EventHandler(this.scavengerRange_Leave);
            // 
            // scavengerDragDelay
            // 
            this.scavengerDragDelay.BackColor = System.Drawing.Color.White;
            this.scavengerDragDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scavengerDragDelay.Location = new System.Drawing.Point(416, 42);
            this.scavengerDragDelay.Name = "scavengerDragDelay";
            this.scavengerDragDelay.Size = new System.Drawing.Size(45, 20);
            this.scavengerDragDelay.TabIndex = 68;
            this.scavengerDragDelay.Leave += new System.EventHandler(this.scavengerDragDelay_Leave);
            #endregion
            #region Organizer Tab
            // 
            // organizer
            // 
            this.organizer.Controls.Add(this.organizerCloneListB);
            this.organizer.Controls.Add(this.organizerExecuteButton);
            this.organizer.Controls.Add(this.organizerStopButton);
            this.organizer.Controls.Add(this.groupBox11);
            this.organizer.Controls.Add(this.organizerdataGridView);
            this.organizer.Controls.Add(this.groupBox16);
            this.organizer.Controls.Add(this.label27);
            this.organizer.Controls.Add(this.label24);
            this.organizer.Controls.Add(this.organizerAddTargetB);
            this.organizer.Controls.Add(this.organizerRemoveListB);
            this.organizer.Controls.Add(this.organizerAddListB);
            this.organizer.Controls.Add(this.organizerListSelect);
            this.organizer.Controls.Add(this.organizerDragDelay);
            this.organizer.Location = new System.Drawing.Point(4, 22);
            this.organizer.Name = "organizer";
            this.organizer.Padding = new System.Windows.Forms.Padding(3);
            this.organizer.Size = new System.Drawing.Size(677, 347);
            this.organizer.TabIndex = 2;
            this.organizer.Text = "Organizer";
            this.organizer.UseVisualStyleBackColor = true;
            // 
            // organizerCloneListB
            // 
            this.organizerCloneListB.Location = new System.Drawing.Point(425, 12);
            this.organizerCloneListB.Name = "organizerCloneListB";
            this.organizerCloneListB.Size = new System.Drawing.Size(70, 21);
            this.organizerCloneListB.TabIndex = 92;
            this.organizerCloneListB.Text = "Clone";
            this.organizerCloneListB.Click += new System.EventHandler(this.organizerCloneListB_Click);
            // 
            // organizerExecuteButton
            // 
            this.organizerExecuteButton.BackgroundImage = global::Assistant.Properties.Resources.playagent;
            this.organizerExecuteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.organizerExecuteButton.FlatAppearance.BorderSize = 0;
            this.organizerExecuteButton.Location = new System.Drawing.Point(283, 58);
            this.organizerExecuteButton.Name = "organizerExecuteButton";
            this.organizerExecuteButton.Size = new System.Drawing.Size(30, 30);
            this.organizerExecuteButton.TabIndex = 91;
            this.organizerExecuteButton.UseVisualStyleBackColor = true;
            this.organizerExecuteButton.Click += new System.EventHandler(this.organizerExecute_Click);
            // 
            // organizerStopButton
            // 
            this.organizerStopButton.BackgroundImage = global::Assistant.Properties.Resources.stopagent;
            this.organizerStopButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.organizerStopButton.FlatAppearance.BorderSize = 0;
            this.organizerStopButton.Location = new System.Drawing.Point(319, 58);
            this.organizerStopButton.Name = "organizerStopButton";
            this.organizerStopButton.Size = new System.Drawing.Size(30, 30);
            this.organizerStopButton.TabIndex = 1;
            this.organizerStopButton.UseVisualStyleBackColor = true;
            this.organizerStopButton.Click += new System.EventHandler(this.organizerStop_Click);
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.organizerDestination);
            this.groupBox11.Controls.Add(this.label57);
            this.groupBox11.Controls.Add(this.label56);
            this.groupBox11.Controls.Add(this.organizerSetSourceB);
            this.groupBox11.Controls.Add(this.organizerSetDestinationB);
            this.groupBox11.Controls.Add(this.organizerSourceLabel);
            this.groupBox11.Location = new System.Drawing.Point(9, 42);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(252, 65);
            this.groupBox11.TabIndex = 90;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Organizer Bags";
            // 
            // organizerDestination
            // 
            this.organizerDestination.Location = new System.Drawing.Point(78, 37);
            this.organizerDestination.Name = "organizerDestination";
            this.organizerDestination.Size = new System.Drawing.Size(74, 20);
            this.organizerDestination.TabIndex = 92;
            this.organizerDestination.Leave += new System.EventHandler(this.organizerDestination_Leave);
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Location = new System.Drawing.Point(6, 41);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(63, 14);
            this.label57.TabIndex = 91;
            this.label57.Text = "Destination:";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(6, 17);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(45, 14);
            this.label56.TabIndex = 90;
            this.label56.Text = "Source:";
            // 
            // organizerSetSourceB
            // 
            this.organizerSetSourceB.Location = new System.Drawing.Point(156, 12);
            this.organizerSetSourceB.Name = "organizerSetSourceB";
            this.organizerSetSourceB.Size = new System.Drawing.Size(90, 21);
            this.organizerSetSourceB.TabIndex = 66;
            this.organizerSetSourceB.Text = "Set Source";
            this.organizerSetSourceB.Click += new System.EventHandler(this.organizerSetSource_Click);
            // 
            // organizerSetDestinationB
            // 
            this.organizerSetDestinationB.Location = new System.Drawing.Point(156, 37);
            this.organizerSetDestinationB.Name = "organizerSetDestinationB";
            this.organizerSetDestinationB.Size = new System.Drawing.Size(90, 21);
            this.organizerSetDestinationB.TabIndex = 69;
            this.organizerSetDestinationB.Text = "Set Dest";
            this.organizerSetDestinationB.Click += new System.EventHandler(this.organizerSetDestination_Click);
            // 
            // organizerSourceLabel
            // 
            this.organizerSourceLabel.Location = new System.Drawing.Point(75, 17);
            this.organizerSourceLabel.Name = "organizerSourceLabel";
            this.organizerSourceLabel.Size = new System.Drawing.Size(82, 19);
            this.organizerSourceLabel.TabIndex = 67;
            this.organizerSourceLabel.Text = "0x00000000";
            // 
            // organizerdataGridView
            // 
            this.organizerdataGridView.AllowDrop = true;
            this.organizerdataGridView.AllowUserToResizeRows = false;
            this.organizerdataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.organizerdataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.organizerdataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn2,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn8,
            this.dataGridViewTextBoxColumn7});
            this.organizerdataGridView.Location = new System.Drawing.Point(9, 113);
            this.organizerdataGridView.Name = "organizerdataGridView";
            this.organizerdataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.organizerdataGridView.RowHeadersVisible = false;
            this.organizerdataGridView.RowHeadersWidth = 62;
            this.organizerdataGridView.Size = new System.Drawing.Size(376, 200);
            this.organizerdataGridView.TabIndex = 89;
            this.organizerdataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.organizerdataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.organizerdataGridView_CellEndEdit);
            this.organizerdataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.organizerdataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.organizerdataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.organizerdataGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.organizerdataGridView_DefaultValuesNeeded);
            this.organizerdataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.organizerdataGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.organizerdataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.organizerdataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            this.dataGridViewCheckBoxColumn2.FalseValue = "False";
            this.dataGridViewCheckBoxColumn2.FillWeight = 1F;
            this.dataGridViewCheckBoxColumn2.HeaderText = "X";
            this.dataGridViewCheckBoxColumn2.IndeterminateValue = "False";
            this.dataGridViewCheckBoxColumn2.MinimumWidth = 20;
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            this.dataGridViewCheckBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn2.ToolTipText = "Check This for enable item in list";
            this.dataGridViewCheckBoxColumn2.TrueValue = "True";
            this.dataGridViewCheckBoxColumn2.Width = 21;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn5.FillWeight = 70F;
            this.dataGridViewTextBoxColumn5.HeaderText = "Item Name";
            this.dataGridViewTextBoxColumn5.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn5.ToolTipText = "Here the item name";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn6.FillWeight = 10F;
            this.dataGridViewTextBoxColumn6.HeaderText = "Graphics";
            this.dataGridViewTextBoxColumn6.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn6.ToolTipText = "Here Graphics item ID";
            this.dataGridViewTextBoxColumn6.Width = 76;
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn8.FillWeight = 10F;
            this.dataGridViewTextBoxColumn8.HeaderText = "Color";
            this.dataGridViewTextBoxColumn8.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn8.ToolTipText = "Here item color, use -1 for all color";
            this.dataGridViewTextBoxColumn8.Width = 57;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn7.FillWeight = 10F;
            this.dataGridViewTextBoxColumn7.HeaderText = "Amount";
            this.dataGridViewTextBoxColumn7.MinimumWidth = 20;
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn7.ToolTipText = "Here amount to move, use -1 for all item";
            this.dataGridViewTextBoxColumn7.Width = 69;
            // 
            // groupBox16
            // 
            this.groupBox16.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox16.Controls.Add(this.organizerLogBox);
            this.groupBox16.Location = new System.Drawing.Point(391, 84);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Size = new System.Drawing.Size(278, 231);
            this.groupBox16.TabIndex = 73;
            this.groupBox16.TabStop = false;
            this.groupBox16.Text = "Organizer Log";
            // 
            // organizerLogBox
            // 
            this.organizerLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.organizerLogBox.FormattingEnabled = true;
            this.organizerLogBox.ItemHeight = 14;
            this.organizerLogBox.Location = new System.Drawing.Point(6, 19);
            this.organizerLogBox.Name = "organizerLogBox";
            this.organizerLogBox.Size = new System.Drawing.Size(265, 74);
            this.organizerLogBox.TabIndex = 0;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(425, 55);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(107, 14);
            this.label27.TabIndex = 72;
            this.label27.Text = "Drag Item Delay (ms)";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(6, 18);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(78, 14);
            this.label24.TabIndex = 60;
            this.label24.Text = "Organizer List:";
            // 
            // organizerAddTargetB
            // 
            this.organizerAddTargetB.Location = new System.Drawing.Point(540, 39);
            this.organizerAddTargetB.Name = "organizerAddTargetB";
            this.organizerAddTargetB.Size = new System.Drawing.Size(90, 20);
            this.organizerAddTargetB.TabIndex = 47;
            this.organizerAddTargetB.Text = "Add Item";
            this.organizerAddTargetB.Click += new System.EventHandler(this.organizerAddTarget_Click);
            // 
            // organizerRemoveListB
            // 
            this.organizerRemoveListB.Location = new System.Drawing.Point(349, 12);
            this.organizerRemoveListB.Name = "organizerRemoveListB";
            this.organizerRemoveListB.Size = new System.Drawing.Size(70, 21);
            this.organizerRemoveListB.TabIndex = 63;
            this.organizerRemoveListB.Text = "Remove";
            this.organizerRemoveListB.Click += new System.EventHandler(this.organizerRemoveList_Click);
            // 
            // organizerAddListB
            // 
            this.organizerAddListB.Location = new System.Drawing.Point(273, 12);
            this.organizerAddListB.Name = "organizerAddListB";
            this.organizerAddListB.Size = new System.Drawing.Size(70, 21);
            this.organizerAddListB.TabIndex = 62;
            this.organizerAddListB.Text = "Add";
            this.organizerAddListB.Click += new System.EventHandler(this.organizerAddList_Click);
            // 
            // organizerListSelect
            // 
            this.organizerListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.organizerListSelect.FormattingEnabled = true;
            this.organizerListSelect.Location = new System.Drawing.Point(89, 12);
            this.organizerListSelect.Name = "organizerListSelect";
            this.organizerListSelect.Size = new System.Drawing.Size(172, 22);
            this.organizerListSelect.TabIndex = 61;
            this.organizerListSelect.SelectedIndexChanged += new System.EventHandler(this.organizerListSelect_SelectedIndexChanged);
            // 
            // organizerDragDelay
            // 
            this.organizerDragDelay.BackColor = System.Drawing.Color.White;
            this.organizerDragDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.organizerDragDelay.Location = new System.Drawing.Point(379, 52);
            this.organizerDragDelay.Name = "organizerDragDelay";
            this.organizerDragDelay.Size = new System.Drawing.Size(45, 20);
            this.organizerDragDelay.TabIndex = 71;
            this.organizerDragDelay.Leave += new System.EventHandler(this.organizerDragDelay_Leave);
            #endregion
            #region Vendor Buy Tab
            // 
            // VendorBuy
            // 
            this.VendorBuy.Controls.Add(this.buyToCompleteAmount);
            this.VendorBuy.Controls.Add(this.buyLogBox);
            this.VendorBuy.Controls.Add(this.buyCompareNameCheckBox);
            this.VendorBuy.Controls.Add(this.buyCloneButton);
            this.VendorBuy.Controls.Add(this.vendorbuydataGridView);
            this.VendorBuy.Controls.Add(this.groupBox18);
            this.VendorBuy.Controls.Add(this.label25);
            this.VendorBuy.Controls.Add(this.buyAddTargetB);
            this.VendorBuy.Controls.Add(this.buyEnableCheckBox);
            this.VendorBuy.Controls.Add(this.buyRemoveListButton);
            this.VendorBuy.Controls.Add(this.buyAddListButton);
            this.VendorBuy.Controls.Add(this.buyListSelect);
            this.VendorBuy.Location = new System.Drawing.Point(4, 22);
            this.VendorBuy.Name = "VendorBuy";
            this.VendorBuy.Padding = new System.Windows.Forms.Padding(3);
            this.VendorBuy.Size = new System.Drawing.Size(677, 347);
            this.VendorBuy.TabIndex = 3;
            this.VendorBuy.Text = "Vendor Buy";
            this.VendorBuy.UseVisualStyleBackColor = true;
            // 
            // buyToCompleteAmount
            // 
            this.buyToCompleteAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buyToCompleteAmount.AutoSize = true;
            this.buyToCompleteAmount.Location = new System.Drawing.Point(391, 86);
            this.buyToCompleteAmount.Name = "buyToCompleteAmount";
            this.buyToCompleteAmount.Size = new System.Drawing.Size(65, 18);
            this.buyToCompleteAmount.TabIndex = 91;
            this.buyToCompleteAmount.Tag = "VendorBuyRestockTag";
            this.buyToCompleteAmount.Text = "Restock";
            this.m_Tip.SetToolTip(this.buyToCompleteAmount, "Buys enough items to refill your backpack to the specified amount");
            this.buyToCompleteAmount.CheckedChanged += new System.EventHandler(this.buyComplete_CheckedChanged);
            // 
            // buyLogBox
            // 
            this.buyLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buyLogBox.FormattingEnabled = true;
            this.buyLogBox.ItemHeight = 14;
            this.buyLogBox.Location = new System.Drawing.Point(399, 177);
            this.buyLogBox.Name = "buyLogBox";
            this.buyLogBox.Size = new System.Drawing.Size(266, 32);
            this.buyLogBox.TabIndex = 0;
            // 
            // buyCompareNameCheckBox
            // 
            this.buyCompareNameCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buyCompareNameCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buyCompareNameCheckBox.Location = new System.Drawing.Point(392, 65);
            this.buyCompareNameCheckBox.Name = "buyCompareNameCheckBox";
            this.buyCompareNameCheckBox.Size = new System.Drawing.Size(135, 22);
            this.buyCompareNameCheckBox.TabIndex = 90;
            this.buyCompareNameCheckBox.Text = "Compare Item Name";
            this.buyCompareNameCheckBox.CheckedChanged += new System.EventHandler(this.buyCompareNameCheckBox_CheckedChanged);
            // 
            // buyCloneButton
            // 
            this.buyCloneButton.Location = new System.Drawing.Point(419, 12);
            this.buyCloneButton.Name = "buyCloneButton";
            this.buyCloneButton.Size = new System.Drawing.Size(67, 21);
            this.buyCloneButton.TabIndex = 89;
            this.buyCloneButton.Text = "Clone";
            this.buyCloneButton.Click += new System.EventHandler(this.buyCloneButton_Click);
            // 
            // vendorbuydataGridView
            // 
            this.vendorbuydataGridView.AllowDrop = true;
            this.vendorbuydataGridView.AllowUserToResizeRows = false;
            this.vendorbuydataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vendorbuydataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.vendorbuydataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.vendorbuydataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1,
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.vendorbuydataGridView.Location = new System.Drawing.Point(6, 54);
            this.vendorbuydataGridView.Name = "vendorbuydataGridView";
            this.vendorbuydataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.vendorbuydataGridView.RowHeadersVisible = false;
            this.vendorbuydataGridView.RowHeadersWidth = 62;
            this.vendorbuydataGridView.Size = new System.Drawing.Size(375, 254);
            this.vendorbuydataGridView.TabIndex = 88;
            this.vendorbuydataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.vendorbuydataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.vendorbuydataGridView_CellEndEdit);
            this.vendorbuydataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.vendorbuydataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.vendorbuydataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.vendorbuydataGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.vendorbuydataGridView_DefaultValuesNeeded);
            this.vendorbuydataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.vendorbuydataGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.vendorbuydataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.vendorbuydataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewCheckBoxColumn1.FalseValue = "False";
            this.dataGridViewCheckBoxColumn1.FillWeight = 1F;
            this.dataGridViewCheckBoxColumn1.HeaderText = "X";
            this.dataGridViewCheckBoxColumn1.IndeterminateValue = "False";
            this.dataGridViewCheckBoxColumn1.MinimumWidth = 8;
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn1.ToolTipText = "Check This for enable item in list";
            this.dataGridViewCheckBoxColumn1.TrueValue = "True";
            this.dataGridViewCheckBoxColumn1.Width = 21;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.FillWeight = 70F;
            this.dataGridViewTextBoxColumn1.HeaderText = "Item Name";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn1.ToolTipText = "Here the item name";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn2.FillWeight = 10F;
            this.dataGridViewTextBoxColumn2.HeaderText = "Graphics";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn2.ToolTipText = "Here Graphics item ID";
            this.dataGridViewTextBoxColumn2.Width = 76;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn3.FillWeight = 10F;
            this.dataGridViewTextBoxColumn3.HeaderText = "Amount";
            this.dataGridViewTextBoxColumn3.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn3.ToolTipText = "Here Item Amount to sell";
            this.dataGridViewTextBoxColumn3.Width = 69;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn4.FillWeight = 10F;
            this.dataGridViewTextBoxColumn4.HeaderText = "Color";
            this.dataGridViewTextBoxColumn4.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn4.ToolTipText = "Here item color, use -1 for all color";
            this.dataGridViewTextBoxColumn4.Width = 57;
            // 
            // groupBox18
            // 
            this.groupBox18.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox18.Location = new System.Drawing.Point(391, 163);
            this.groupBox18.Name = "groupBox18";
            this.groupBox18.Size = new System.Drawing.Size(278, 145);
            this.groupBox18.TabIndex = 73;
            this.groupBox18.TabStop = false;
            this.groupBox18.Text = "Buy Log";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(3, 18);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(67, 14);
            this.label25.TabIndex = 66;
            this.label25.Text = "Vendor Buy:";
            // 
            // buyAddTargetB
            // 
            this.buyAddTargetB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buyAddTargetB.Location = new System.Drawing.Point(581, 39);
            this.buyAddTargetB.Name = "buyAddTargetB";
            this.buyAddTargetB.Size = new System.Drawing.Size(90, 21);
            this.buyAddTargetB.TabIndex = 45;
            this.buyAddTargetB.Text = "Add Item";
            this.buyAddTargetB.Click += new System.EventHandler(this.buyAddTarget_Click);
            // 
            // buyEnableCheckBox
            // 
            this.buyEnableCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buyEnableCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buyEnableCheckBox.Location = new System.Drawing.Point(392, 44);
            this.buyEnableCheckBox.Name = "buyEnableCheckBox";
            this.buyEnableCheckBox.Size = new System.Drawing.Size(135, 22);
            this.buyEnableCheckBox.TabIndex = 72;
            this.buyEnableCheckBox.Text = "Enable Buy List";
            this.buyEnableCheckBox.CheckedChanged += new System.EventHandler(this.buyEnableCheckB_CheckedChanged);
            // 
            // buyRemoveListButton
            // 
            this.buyRemoveListButton.Location = new System.Drawing.Point(346, 12);
            this.buyRemoveListButton.Name = "buyRemoveListButton";
            this.buyRemoveListButton.Size = new System.Drawing.Size(67, 21);
            this.buyRemoveListButton.TabIndex = 69;
            this.buyRemoveListButton.Text = "Remove";
            this.buyRemoveListButton.Click += new System.EventHandler(this.buyRemoveList_Click);
            // 
            // buyAddListButton
            // 
            this.buyAddListButton.Location = new System.Drawing.Point(273, 12);
            this.buyAddListButton.Name = "buyAddListButton";
            this.buyAddListButton.Size = new System.Drawing.Size(67, 21);
            this.buyAddListButton.TabIndex = 68;
            this.buyAddListButton.Text = "Add";
            this.buyAddListButton.Click += new System.EventHandler(this.buyAddList_Click);
            // 
            // buyListSelect
            // 
            this.buyListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buyListSelect.FormattingEnabled = true;
            this.buyListSelect.Location = new System.Drawing.Point(78, 12);
            this.buyListSelect.Name = "buyListSelect";
            this.buyListSelect.Size = new System.Drawing.Size(183, 22);
            this.buyListSelect.TabIndex = 67;
            this.buyListSelect.SelectedIndexChanged += new System.EventHandler(this.buyListSelect_SelectedIndexChanged);
            #endregion
            #region Vendor Sell Tab
            // 
            // VendorSell
            // 
            this.VendorSell.Controls.Add(this.sellCloneListButton);
            this.VendorSell.Controls.Add(this.groupBox19);
            this.VendorSell.Controls.Add(this.vendorsellGridView);
            this.VendorSell.Controls.Add(this.groupBox20);
            this.VendorSell.Controls.Add(this.label26);
            this.VendorSell.Controls.Add(this.sellAddTargerButton);
            this.VendorSell.Controls.Add(this.sellEnableCheckBox);
            this.VendorSell.Controls.Add(this.sellRemoveListButton);
            this.VendorSell.Controls.Add(this.sellAddListButton);
            this.VendorSell.Controls.Add(this.sellListSelect);
            this.VendorSell.Location = new System.Drawing.Point(4, 22);
            this.VendorSell.Name = "VendorSell";
            this.VendorSell.Padding = new System.Windows.Forms.Padding(3);
            this.VendorSell.Size = new System.Drawing.Size(677, 347);
            this.VendorSell.TabIndex = 4;
            this.VendorSell.Text = "Vendor Sell";
            this.VendorSell.UseVisualStyleBackColor = true;
            // 
            // sellCloneListButton
            // 
            this.sellCloneListButton.Location = new System.Drawing.Point(419, 12);
            this.sellCloneListButton.Name = "sellCloneListButton";
            this.sellCloneListButton.Size = new System.Drawing.Size(67, 21);
            this.sellCloneListButton.TabIndex = 90;
            this.sellCloneListButton.Text = "Clone";
            this.sellCloneListButton.Click += new System.EventHandler(this.sellCloneListButton_Click);
            // 
            // groupBox19
            // 
            this.groupBox19.Controls.Add(this.sellSetBagButton);
            this.groupBox19.Controls.Add(this.label50);
            this.groupBox19.Controls.Add(this.sellBagLabel);
            this.groupBox19.Location = new System.Drawing.Point(6, 42);
            this.groupBox19.Name = "groupBox19";
            this.groupBox19.Size = new System.Drawing.Size(255, 42);
            this.groupBox19.TabIndex = 89;
            this.groupBox19.TabStop = false;
            this.groupBox19.Text = "Sell Bag";
            // 
            // sellSetBagButton
            // 
            this.sellSetBagButton.Location = new System.Drawing.Point(157, 14);
            this.sellSetBagButton.Name = "sellSetBagButton";
            this.sellSetBagButton.Size = new System.Drawing.Size(90, 21);
            this.sellSetBagButton.TabIndex = 85;
            this.sellSetBagButton.Text = "Set Bag";
            this.sellSetBagButton.Click += new System.EventHandler(this.sellSetBag_Click);
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(6, 19);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(37, 14);
            this.label50.TabIndex = 88;
            this.label50.Text = "Serial:";
            // 
            // sellBagLabel
            // 
            this.sellBagLabel.Location = new System.Drawing.Point(47, 19);
            this.sellBagLabel.Name = "sellBagLabel";
            this.sellBagLabel.Size = new System.Drawing.Size(72, 19);
            this.sellBagLabel.TabIndex = 86;
            this.sellBagLabel.Text = "0x00000000";
            // 
            // vendorsellGridView
            // 
            this.vendorsellGridView.AllowDrop = true;
            this.vendorsellGridView.AllowUserToResizeRows = false;
            this.vendorsellGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vendorsellGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.vendorsellGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.vendorsellGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.VendorSellX,
            this.VendorSellItemName,
            this.VendorSellGraphics,
            this.VendorSellAmount,
            this.VendorSellColor});
            this.vendorsellGridView.Location = new System.Drawing.Point(6, 90);
            this.vendorsellGridView.Name = "vendorsellGridView";
            this.vendorsellGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.vendorsellGridView.RowHeadersVisible = false;
            this.vendorsellGridView.RowHeadersWidth = 62;
            this.vendorsellGridView.Size = new System.Drawing.Size(375, 218);
            this.vendorsellGridView.TabIndex = 87;
            this.vendorsellGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.vendorsellGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.vendorsellGridView_CellEndEdit);
            this.vendorsellGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.vendorsellGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.vendorsellGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.vendorsellGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.vendorsellGridView_DefaultValuesNeeded);
            this.vendorsellGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.vendorsellGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.vendorsellGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.vendorsellGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // VendorSellX
            // 
            this.VendorSellX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.VendorSellX.FalseValue = "False";
            this.VendorSellX.FillWeight = 1F;
            this.VendorSellX.HeaderText = "X";
            this.VendorSellX.IndeterminateValue = "False";
            this.VendorSellX.MinimumWidth = 8;
            this.VendorSellX.Name = "VendorSellX";
            this.VendorSellX.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.VendorSellX.ToolTipText = "Check This for enable item in list";
            this.VendorSellX.TrueValue = "True";
            this.VendorSellX.Width = 21;
            // 
            // VendorSellItemName
            // 
            this.VendorSellItemName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.VendorSellItemName.FillWeight = 70F;
            this.VendorSellItemName.HeaderText = "Item Name";
            this.VendorSellItemName.MinimumWidth = 8;
            this.VendorSellItemName.Name = "VendorSellItemName";
            this.VendorSellItemName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.VendorSellItemName.ToolTipText = "Here the item name";
            // 
            // VendorSellGraphics
            // 
            this.VendorSellGraphics.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.VendorSellGraphics.FillWeight = 10F;
            this.VendorSellGraphics.HeaderText = "Graphics";
            this.VendorSellGraphics.MinimumWidth = 8;
            this.VendorSellGraphics.Name = "VendorSellGraphics";
            this.VendorSellGraphics.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.VendorSellGraphics.ToolTipText = "Here Graphics item ID";
            this.VendorSellGraphics.Width = 76;
            // 
            // VendorSellAmount
            // 
            this.VendorSellAmount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.VendorSellAmount.FillWeight = 10F;
            this.VendorSellAmount.HeaderText = "Amount";
            this.VendorSellAmount.MinimumWidth = 8;
            this.VendorSellAmount.Name = "VendorSellAmount";
            this.VendorSellAmount.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.VendorSellAmount.ToolTipText = "Here Item Amount to sell";
            this.VendorSellAmount.Width = 69;
            // 
            // VendorSellColor
            // 
            this.VendorSellColor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.VendorSellColor.FillWeight = 10F;
            this.VendorSellColor.HeaderText = "Color";
            this.VendorSellColor.MinimumWidth = 8;
            this.VendorSellColor.Name = "VendorSellColor";
            this.VendorSellColor.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.VendorSellColor.ToolTipText = "Here item color, use -1 for all color";
            this.VendorSellColor.Width = 57;
            // 
            // groupBox20
            // 
            this.groupBox20.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox20.Controls.Add(this.sellLogBox);
            this.groupBox20.Location = new System.Drawing.Point(391, 72);
            this.groupBox20.Name = "groupBox20";
            this.groupBox20.Size = new System.Drawing.Size(278, 236);
            this.groupBox20.TabIndex = 83;
            this.groupBox20.TabStop = false;
            this.groupBox20.Text = "Sell Log";
            // 
            // sellLogBox
            // 
            this.sellLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sellLogBox.FormattingEnabled = true;
            this.sellLogBox.ItemHeight = 14;
            this.sellLogBox.Location = new System.Drawing.Point(7, 18);
            this.sellLogBox.Name = "sellLogBox";
            this.sellLogBox.Size = new System.Drawing.Size(265, 102);
            this.sellLogBox.TabIndex = 0;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(3, 18);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(65, 14);
            this.label26.TabIndex = 77;
            this.label26.Text = "Vendor Sell:";
            // 
            // sellAddTargerButton
            // 
            this.sellAddTargerButton.Location = new System.Drawing.Point(540, 39);
            this.sellAddTargerButton.Name = "sellAddTargerButton";
            this.sellAddTargerButton.Size = new System.Drawing.Size(90, 21);
            this.sellAddTargerButton.TabIndex = 47;
            this.sellAddTargerButton.Text = "Add Item";
            this.sellAddTargerButton.Click += new System.EventHandler(this.sellAddTarget_Click);
            // 
            // sellEnableCheckBox
            // 
            this.sellEnableCheckBox.Location = new System.Drawing.Point(273, 49);
            this.sellEnableCheckBox.Name = "sellEnableCheckBox";
            this.sellEnableCheckBox.Size = new System.Drawing.Size(105, 22);
            this.sellEnableCheckBox.TabIndex = 82;
            this.sellEnableCheckBox.Text = "Enable Sell List";
            this.sellEnableCheckBox.CheckedChanged += new System.EventHandler(this.sellEnableCheck_CheckedChanged);
            // 
            // sellRemoveListButton
            // 
            this.sellRemoveListButton.Location = new System.Drawing.Point(346, 12);
            this.sellRemoveListButton.Name = "sellRemoveListButton";
            this.sellRemoveListButton.Size = new System.Drawing.Size(67, 21);
            this.sellRemoveListButton.TabIndex = 80;
            this.sellRemoveListButton.Text = "Remove";
            this.sellRemoveListButton.Click += new System.EventHandler(this.sellRemoveList_Click);
            // 
            // sellAddListButton
            // 
            this.sellAddListButton.Location = new System.Drawing.Point(273, 12);
            this.sellAddListButton.Name = "sellAddListButton";
            this.sellAddListButton.Size = new System.Drawing.Size(67, 21);
            this.sellAddListButton.TabIndex = 79;
            this.sellAddListButton.Text = "Add";
            this.sellAddListButton.Click += new System.EventHandler(this.sellAddList_Click);
            // 
            // sellListSelect
            // 
            this.sellListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sellListSelect.FormattingEnabled = true;
            this.sellListSelect.Location = new System.Drawing.Point(78, 12);
            this.sellListSelect.Name = "sellListSelect";
            this.sellListSelect.Size = new System.Drawing.Size(183, 22);
            this.sellListSelect.TabIndex = 78;
            this.sellListSelect.SelectedIndexChanged += new System.EventHandler(this.sellListSelect_SelectedIndexChanged);
            #endregion
            #region Dress Arm Tab
            // 
            // Dress
            // 
            this.Dress.Controls.Add(this.useUo3D);
            this.Dress.Controls.Add(this.dressStopButton);
            this.Dress.Controls.Add(this.dressConflictCheckB);
            this.Dress.Controls.Add(this.dressBagLabel);
            this.Dress.Controls.Add(this.groupBox22);
            this.Dress.Controls.Add(this.label29);
            this.Dress.Controls.Add(this.groupBox21);
            this.Dress.Controls.Add(this.dressSetBagB);
            this.Dress.Controls.Add(this.undressExecuteButton);
            this.Dress.Controls.Add(this.dressExecuteButton);
            this.Dress.Controls.Add(this.dressDragDelay);
            this.Dress.Controls.Add(this.dressListView);
            this.Dress.Controls.Add(this.label28);
            this.Dress.Controls.Add(this.dressRemoveListB);
            this.Dress.Controls.Add(this.dressAddListB);
            this.Dress.Controls.Add(this.dressListSelect);
            this.Dress.Location = new System.Drawing.Point(4, 22);
            this.Dress.Name = "Dress";
            this.Dress.Padding = new System.Windows.Forms.Padding(3);
            this.Dress.Size = new System.Drawing.Size(677, 347);
            this.Dress.TabIndex = 5;
            this.Dress.Text = "Dress / Arm";
            this.Dress.UseVisualStyleBackColor = true;
            // 
            // useUo3D
            // 
            this.useUo3D.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.useUo3D.Location = new System.Drawing.Point(450, 84);
            this.useUo3D.Name = "useUo3D";
            this.useUo3D.Size = new System.Drawing.Size(185, 36);
            this.useUo3D.TabIndex = 92;
            this.useUo3D.Text = "Use UO3D Equip and UnEquip";
            this.useUo3D.CheckedChanged += new System.EventHandler(this.dressUseUO3d_CheckedChanged);
            // 
            // dressStopButton
            // 
            this.dressStopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dressStopButton.Location = new System.Drawing.Point(439, 58);
            this.dressStopButton.Name = "dressStopButton";
            this.dressStopButton.Size = new System.Drawing.Size(62, 20);
            this.dressStopButton.TabIndex = 91;
            this.dressStopButton.Text = "Stop";
            this.dressStopButton.Click += new System.EventHandler(this.dressStopButton_Click);
            // 
            // dressConflictCheckB
            // 
            this.dressConflictCheckB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dressConflictCheckB.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.dressConflictCheckB.Location = new System.Drawing.Point(305, 84);
            this.dressConflictCheckB.Name = "dressConflictCheckB";
            this.dressConflictCheckB.Size = new System.Drawing.Size(128, 36);
            this.dressConflictCheckB.TabIndex = 90;
            this.dressConflictCheckB.Text = "Remove Conflict Item";
            this.dressConflictCheckB.CheckedChanged += new System.EventHandler(this.dressConflictCheckB_CheckedChanged);
            // 
            // dressBagLabel
            // 
            this.dressBagLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dressBagLabel.Location = new System.Drawing.Point(578, 161);
            this.dressBagLabel.Name = "dressBagLabel";
            this.dressBagLabel.Size = new System.Drawing.Size(81, 19);
            this.dressBagLabel.TabIndex = 89;
            this.dressBagLabel.Text = "0x00000000";
            // 
            // groupBox22
            // 
            this.groupBox22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox22.Controls.Add(this.dressAddTargetB);
            this.groupBox22.Controls.Add(this.dressAddManualB);
            this.groupBox22.Controls.Add(this.dressRemoveB);
            this.groupBox22.Controls.Add(this.dressClearListB);
            this.groupBox22.Controls.Add(this.dressReadB);
            this.groupBox22.Location = new System.Drawing.Point(560, 183);
            this.groupBox22.Name = "groupBox22";
            this.groupBox22.Size = new System.Drawing.Size(100, 149);
            this.groupBox22.TabIndex = 85;
            this.groupBox22.TabStop = false;
            this.groupBox22.Text = "Item List";
            // 
            // dressAddTargetB
            // 
            this.dressAddTargetB.Location = new System.Drawing.Point(5, 68);
            this.dressAddTargetB.Name = "dressAddTargetB";
            this.dressAddTargetB.Size = new System.Drawing.Size(90, 20);
            this.dressAddTargetB.TabIndex = 48;
            this.dressAddTargetB.Text = "Add Target";
            this.dressAddTargetB.Click += new System.EventHandler(this.dressAddTargetB_Click);
            // 
            // dressAddManualB
            // 
            this.dressAddManualB.Location = new System.Drawing.Point(5, 43);
            this.dressAddManualB.Name = "dressAddManualB";
            this.dressAddManualB.Size = new System.Drawing.Size(90, 20);
            this.dressAddManualB.TabIndex = 47;
            this.dressAddManualB.Text = "Add Clear Layer";
            this.dressAddManualB.Click += new System.EventHandler(this.dressAddManualB_Click);
            // 
            // dressRemoveB
            // 
            this.dressRemoveB.Location = new System.Drawing.Point(5, 94);
            this.dressRemoveB.Name = "dressRemoveB";
            this.dressRemoveB.Size = new System.Drawing.Size(90, 20);
            this.dressRemoveB.TabIndex = 46;
            this.dressRemoveB.Text = "Remove";
            this.dressRemoveB.Click += new System.EventHandler(this.dressRemoveB_Click);
            // 
            // dressClearListB
            // 
            this.dressClearListB.Location = new System.Drawing.Point(5, 120);
            this.dressClearListB.Name = "dressClearListB";
            this.dressClearListB.Size = new System.Drawing.Size(90, 20);
            this.dressClearListB.TabIndex = 111;
            this.dressClearListB.Text = "ClearList";
            this.dressClearListB.Click += new System.EventHandler(this.dressClearListB_Click);
            // 
            // dressReadB
            // 
            this.dressReadB.Location = new System.Drawing.Point(5, 18);
            this.dressReadB.Name = "dressReadB";
            this.dressReadB.Size = new System.Drawing.Size(90, 20);
            this.dressReadB.TabIndex = 45;
            this.dressReadB.Text = "Read Current";
            this.dressReadB.Click += new System.EventHandler(this.dressReadB_Click);
            // 
            // label29
            // 
            this.label29.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(555, 61);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(107, 14);
            this.label29.TabIndex = 76;
            this.label29.Text = "Drag Item Delay (ms)";
            // 
            // groupBox21
            // 
            this.groupBox21.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox21.Controls.Add(this.dressLogBox);
            this.groupBox21.Location = new System.Drawing.Point(306, 134);
            this.groupBox21.Name = "groupBox21";
            this.groupBox21.Size = new System.Drawing.Size(257, 177);
            this.groupBox21.TabIndex = 74;
            this.groupBox21.TabStop = false;
            this.groupBox21.Text = "Organizer Log";
            // 
            // dressLogBox
            // 
            this.dressLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dressLogBox.FormattingEnabled = true;
            this.dressLogBox.ItemHeight = 14;
            this.dressLogBox.Location = new System.Drawing.Point(3, 13);
            this.dressLogBox.Name = "dressLogBox";
            this.dressLogBox.Size = new System.Drawing.Size(250, 46);
            this.dressLogBox.TabIndex = 0;
            // 
            // dressSetBagB
            // 
            this.dressSetBagB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dressSetBagB.Location = new System.Drawing.Point(565, 134);
            this.dressSetBagB.Name = "dressSetBagB";
            this.dressSetBagB.Size = new System.Drawing.Size(88, 20);
            this.dressSetBagB.TabIndex = 88;
            this.dressSetBagB.Text = "Undress Bag";
            this.dressSetBagB.Click += new System.EventHandler(this.dressSetBagB_Click);
            // 
            // undressExecuteButton
            // 
            this.undressExecuteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.undressExecuteButton.Location = new System.Drawing.Point(373, 58);
            this.undressExecuteButton.Name = "undressExecuteButton";
            this.undressExecuteButton.Size = new System.Drawing.Size(62, 20);
            this.undressExecuteButton.TabIndex = 87;
            this.undressExecuteButton.Text = "Undres";
            this.undressExecuteButton.Click += new System.EventHandler(this.razorButton10_Click);
            // 
            // dressExecuteButton
            // 
            this.dressExecuteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dressExecuteButton.Location = new System.Drawing.Point(306, 58);
            this.dressExecuteButton.Name = "dressExecuteButton";
            this.dressExecuteButton.Size = new System.Drawing.Size(62, 20);
            this.dressExecuteButton.TabIndex = 86;
            this.dressExecuteButton.Text = "Dress";
            this.dressExecuteButton.Click += new System.EventHandler(this.dressExecuteButton_Click);
            // 
            // dressDragDelay
            // 
            this.dressDragDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dressDragDelay.BackColor = System.Drawing.Color.White;
            this.dressDragDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dressDragDelay.Location = new System.Drawing.Point(508, 58);
            this.dressDragDelay.Name = "dressDragDelay";
            this.dressDragDelay.Size = new System.Drawing.Size(45, 20);
            this.dressDragDelay.TabIndex = 75;
            this.dressDragDelay.Leave += new System.EventHandler(this.dressDragDelay_Leave);
            // 
            // dressListView
            // 
            this.dressListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dressListView.AutoArrange = false;
            this.dressListView.CheckBoxes = true;
            this.dressListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader24,
            this.columnHeader25,
            this.columnHeader26,
            this.columnHeader27});
            this.dressListView.FullRowSelect = true;
            this.dressListView.GridLines = true;
            this.dressListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.dressListView.HideSelection = false;
            this.dressListView.LabelWrap = false;
            this.dressListView.Location = new System.Drawing.Point(6, 51);
            this.dressListView.MultiSelect = false;
            this.dressListView.Name = "dressListView";
            this.dressListView.Size = new System.Drawing.Size(297, 266);
            this.dressListView.TabIndex = 64;
            this.dressListView.UseCompatibleStateImageBehavior = false;
            this.dressListView.View = System.Windows.Forms.View.Details;
            this.dressListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.dresslistView_ItemChecked);
            // 
            // columnHeader24
            // 
            this.columnHeader24.Text = "X";
            this.columnHeader24.Width = 30;
            // 
            // columnHeader25
            // 
            this.columnHeader25.Text = "Layer";
            this.columnHeader25.Width = 100;
            // 
            // columnHeader26
            // 
            this.columnHeader26.Text = "Name";
            this.columnHeader26.Width = 190;
            // 
            // columnHeader27
            // 
            this.columnHeader27.Text = "Serial";
            this.columnHeader27.Width = 120;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(6, 18);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(59, 14);
            this.label28.TabIndex = 60;
            this.label28.Text = "Dress List:";
            // 
            // dressRemoveListB
            // 
            this.dressRemoveListB.Location = new System.Drawing.Point(366, 12);
            this.dressRemoveListB.Name = "dressRemoveListB";
            this.dressRemoveListB.Size = new System.Drawing.Size(90, 21);
            this.dressRemoveListB.TabIndex = 63;
            this.dressRemoveListB.Text = "Remove";
            this.dressRemoveListB.Click += new System.EventHandler(this.dressRemoveListB_Click);
            // 
            // dressAddListB
            // 
            this.dressAddListB.Location = new System.Drawing.Point(270, 12);
            this.dressAddListB.Name = "dressAddListB";
            this.dressAddListB.Size = new System.Drawing.Size(90, 21);
            this.dressAddListB.TabIndex = 62;
            this.dressAddListB.Text = "Add";
            this.dressAddListB.Click += new System.EventHandler(this.dressAddListB_Click);
            // 
            // dressListSelect
            // 
            this.dressListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dressListSelect.FormattingEnabled = true;
            this.dressListSelect.Location = new System.Drawing.Point(78, 12);
            this.dressListSelect.Name = "dressListSelect";
            this.dressListSelect.Size = new System.Drawing.Size(183, 22);
            this.dressListSelect.TabIndex = 61;
            this.dressListSelect.SelectedIndexChanged += new System.EventHandler(this.dressListSelect_SelectedIndexChanged);
            #endregion
            #region Friends Tab
            // 
            // friends
            // 
            this.friends.Controls.Add(this.splitContainer1);
            this.friends.Controls.Add(this.groupBox34);
            this.friends.Controls.Add(this.groupBox33);
            this.friends.Controls.Add(this.friendGroupBox);
            this.friends.Controls.Add(this.friendloggroupBox);
            this.friends.Controls.Add(this.friendIncludePartyCheckBox);
            this.friends.Controls.Add(this.friendAttackCheckBox);
            this.friends.Controls.Add(this.friendPartyCheckBox);
            this.friends.Controls.Add(this.labelfriend);
            this.friends.Controls.Add(this.friendButtonRemoveList);
            this.friends.Controls.Add(this.friendButtonAddList);
            this.friends.Controls.Add(this.friendListSelect);
            this.friends.Location = new System.Drawing.Point(4, 22);
            this.friends.Name = "friends";
            this.friends.Padding = new System.Windows.Forms.Padding(3);
            this.friends.Size = new System.Drawing.Size(677, 347);
            this.friends.TabIndex = 6;
            this.friends.Text = "Friends";
            this.friends.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(9, 41);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.friendlistView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.friendguildListView);
            this.splitContainer1.Size = new System.Drawing.Size(249, 285);
            this.splitContainer1.SplitterDistance = 135;
            this.splitContainer1.TabIndex = 83;
            // 
            // groupBox34
            // 
            this.groupBox34.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox34.Controls.Add(this.FriendGuildAddButton);
            this.groupBox34.Controls.Add(this.FriendGuildRemoveButton);
            this.groupBox34.Location = new System.Drawing.Point(565, 230);
            this.groupBox34.Name = "groupBox34";
            this.groupBox34.Size = new System.Drawing.Size(106, 75);
            this.groupBox34.TabIndex = 82;
            this.groupBox34.TabStop = false;
            this.groupBox34.Text = "Guild Friend";
            // 
            // FriendGuildAddButton
            // 
            this.FriendGuildAddButton.Location = new System.Drawing.Point(9, 19);
            this.FriendGuildAddButton.Name = "FriendGuildAddButton";
            this.FriendGuildAddButton.Size = new System.Drawing.Size(90, 20);
            this.FriendGuildAddButton.TabIndex = 80;
            this.FriendGuildAddButton.Text = "Add Manual";
            this.FriendGuildAddButton.Click += new System.EventHandler(this.FriendGuildAddButton_Click);
            // 
            // FriendGuildRemoveButton
            // 
            this.FriendGuildRemoveButton.Location = new System.Drawing.Point(9, 45);
            this.FriendGuildRemoveButton.Name = "FriendGuildRemoveButton";
            this.FriendGuildRemoveButton.Size = new System.Drawing.Size(90, 20);
            this.FriendGuildRemoveButton.TabIndex = 81;
            this.FriendGuildRemoveButton.Text = "Remove";
            this.FriendGuildRemoveButton.Click += new System.EventHandler(this.FriendGuildRemoveButton_Click);
            // 
            // groupBox33
            // 
            this.groupBox33.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox33.Controls.Add(this.MINfriendCheckBox);
            this.groupBox33.Controls.Add(this.SLfriendCheckBox);
            this.groupBox33.Controls.Add(this.TBfriendCheckBox);
            this.groupBox33.Controls.Add(this.COMfriendCheckBox);
            this.groupBox33.Location = new System.Drawing.Point(565, 147);
            this.groupBox33.Name = "groupBox33";
            this.groupBox33.Size = new System.Drawing.Size(106, 77);
            this.groupBox33.TabIndex = 78;
            this.groupBox33.TabStop = false;
            this.groupBox33.Text = "Faction Friend";
            // 
            // MINfriendCheckBox
            // 
            this.MINfriendCheckBox.Location = new System.Drawing.Point(50, 47);
            this.MINfriendCheckBox.Name = "MINfriendCheckBox";
            this.MINfriendCheckBox.Size = new System.Drawing.Size(47, 22);
            this.MINfriendCheckBox.TabIndex = 81;
            this.MINfriendCheckBox.Text = "MIN";
            this.MINfriendCheckBox.CheckedChanged += new System.EventHandler(this.MINfriendCheckBox_CheckedChanged);
            // 
            // SLfriendCheckBox
            // 
            this.SLfriendCheckBox.Location = new System.Drawing.Point(5, 20);
            this.SLfriendCheckBox.Name = "SLfriendCheckBox";
            this.SLfriendCheckBox.Size = new System.Drawing.Size(41, 22);
            this.SLfriendCheckBox.TabIndex = 78;
            this.SLfriendCheckBox.Text = "SL";
            this.SLfriendCheckBox.CheckedChanged += new System.EventHandler(this.SLfriendCheckBox_CheckedChanged);
            // 
            // TBfriendCheckBox
            // 
            this.TBfriendCheckBox.Location = new System.Drawing.Point(5, 47);
            this.TBfriendCheckBox.Name = "TBfriendCheckBox";
            this.TBfriendCheckBox.Size = new System.Drawing.Size(41, 22);
            this.TBfriendCheckBox.TabIndex = 79;
            this.TBfriendCheckBox.Text = "TB";
            this.TBfriendCheckBox.CheckedChanged += new System.EventHandler(this.TBfriendCheckBox_CheckedChanged);
            // 
            // COMfriendCheckBox
            // 
            this.COMfriendCheckBox.Location = new System.Drawing.Point(50, 20);
            this.COMfriendCheckBox.Name = "COMfriendCheckBox";
            this.COMfriendCheckBox.Size = new System.Drawing.Size(50, 22);
            this.COMfriendCheckBox.TabIndex = 80;
            this.COMfriendCheckBox.Text = "CoM";
            this.COMfriendCheckBox.CheckedChanged += new System.EventHandler(this.COMfriendCheckBox_CheckedChanged);
            // 
            // friendGroupBox
            // 
            this.friendGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.friendGroupBox.Controls.Add(this.friendAddTargetButton);
            this.friendGroupBox.Controls.Add(this.friendRemoveButton);
            this.friendGroupBox.Controls.Add(this.friendAddButton);
            this.friendGroupBox.Location = new System.Drawing.Point(565, 41);
            this.friendGroupBox.Name = "friendGroupBox";
            this.friendGroupBox.Size = new System.Drawing.Size(106, 100);
            this.friendGroupBox.TabIndex = 76;
            this.friendGroupBox.TabStop = false;
            this.friendGroupBox.Text = "User Friend";
            // 
            // friendAddTargetButton
            // 
            this.friendAddTargetButton.Location = new System.Drawing.Point(9, 46);
            this.friendAddTargetButton.Name = "friendAddTargetButton";
            this.friendAddTargetButton.Size = new System.Drawing.Size(90, 20);
            this.friendAddTargetButton.TabIndex = 50;
            this.friendAddTargetButton.Text = "Add Target";
            this.friendAddTargetButton.Click += new System.EventHandler(this.friendAddTargetButton_Click);
            // 
            // friendRemoveButton
            // 
            this.friendRemoveButton.Location = new System.Drawing.Point(9, 72);
            this.friendRemoveButton.Name = "friendRemoveButton";
            this.friendRemoveButton.Size = new System.Drawing.Size(90, 20);
            this.friendRemoveButton.TabIndex = 49;
            this.friendRemoveButton.Text = "Remove";
            this.friendRemoveButton.Click += new System.EventHandler(this.friendRemoveButton_Click);
            // 
            // friendAddButton
            // 
            this.friendAddButton.Location = new System.Drawing.Point(9, 21);
            this.friendAddButton.Name = "friendAddButton";
            this.friendAddButton.Size = new System.Drawing.Size(90, 20);
            this.friendAddButton.TabIndex = 48;
            this.friendAddButton.Text = "Add Manual";
            this.friendAddButton.Click += new System.EventHandler(this.friendAddButton_Click);
            // 
            // friendloggroupBox
            // 
            this.friendloggroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.friendloggroupBox.Controls.Add(this.friendLogBox);
            this.friendloggroupBox.Location = new System.Drawing.Point(288, 131);
            this.friendloggroupBox.Name = "friendloggroupBox";
            this.friendloggroupBox.Size = new System.Drawing.Size(271, 204);
            this.friendloggroupBox.TabIndex = 74;
            this.friendloggroupBox.TabStop = false;
            this.friendloggroupBox.Text = "Friend Log";
            // 
            // friendLogBox
            // 
            this.friendLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.friendLogBox.FormattingEnabled = true;
            this.friendLogBox.ItemHeight = 14;
            this.friendLogBox.Location = new System.Drawing.Point(6, 19);
            this.friendLogBox.Name = "friendLogBox";
            this.friendLogBox.Size = new System.Drawing.Size(259, 74);
            this.friendLogBox.TabIndex = 0;
            // 
            // friendIncludePartyCheckBox
            // 
            this.friendIncludePartyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.friendIncludePartyCheckBox.Location = new System.Drawing.Point(288, 101);
            this.friendIncludePartyCheckBox.Name = "friendIncludePartyCheckBox";
            this.friendIncludePartyCheckBox.Size = new System.Drawing.Size(235, 22);
            this.friendIncludePartyCheckBox.TabIndex = 68;
            this.friendIncludePartyCheckBox.Text = "Include party member in Friend List";
            this.friendIncludePartyCheckBox.CheckedChanged += new System.EventHandler(this.friendIncludePartyCheckBox_CheckedChanged);
            // 
            // friendAttackCheckBox
            // 
            this.friendAttackCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.friendAttackCheckBox.Location = new System.Drawing.Point(288, 76);
            this.friendAttackCheckBox.Name = "friendAttackCheckBox";
            this.friendAttackCheckBox.Size = new System.Drawing.Size(241, 22);
            this.friendAttackCheckBox.TabIndex = 67;
            this.friendAttackCheckBox.Text = "Prevent attacking friends in warmode";
            this.friendAttackCheckBox.CheckedChanged += new System.EventHandler(this.friendAttackCheckBox_CheckedChanged);
            // 
            // friendPartyCheckBox
            // 
            this.friendPartyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.friendPartyCheckBox.Location = new System.Drawing.Point(288, 51);
            this.friendPartyCheckBox.Name = "friendPartyCheckBox";
            this.friendPartyCheckBox.Size = new System.Drawing.Size(241, 22);
            this.friendPartyCheckBox.TabIndex = 66;
            this.friendPartyCheckBox.Text = "Autoaccept party from Friends";
            this.friendPartyCheckBox.CheckedChanged += new System.EventHandler(this.friendPartyCheckBox_CheckedChanged);
            // 
            // labelfriend
            // 
            this.labelfriend.AutoSize = true;
            this.labelfriend.Location = new System.Drawing.Point(6, 18);
            this.labelfriend.Name = "labelfriend";
            this.labelfriend.Size = new System.Drawing.Size(60, 14);
            this.labelfriend.TabIndex = 60;
            this.labelfriend.Text = "Friend List:";
            // 
            // friendButtonRemoveList
            // 
            this.friendButtonRemoveList.Location = new System.Drawing.Point(366, 12);
            this.friendButtonRemoveList.Name = "friendButtonRemoveList";
            this.friendButtonRemoveList.Size = new System.Drawing.Size(90, 21);
            this.friendButtonRemoveList.TabIndex = 63;
            this.friendButtonRemoveList.Text = "Remove";
            this.friendButtonRemoveList.Click += new System.EventHandler(this.friendButtonRemoveList_Click);
            // 
            // friendButtonAddList
            // 
            this.friendButtonAddList.Location = new System.Drawing.Point(270, 12);
            this.friendButtonAddList.Name = "friendButtonAddList";
            this.friendButtonAddList.Size = new System.Drawing.Size(90, 21);
            this.friendButtonAddList.TabIndex = 62;
            this.friendButtonAddList.Text = "Add";
            this.friendButtonAddList.Click += new System.EventHandler(this.friendButtonAddList_Click);
            // 
            // friendListSelect
            // 
            this.friendListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.friendListSelect.FormattingEnabled = true;
            this.friendListSelect.Location = new System.Drawing.Point(78, 12);
            this.friendListSelect.Name = "friendListSelect";
            this.friendListSelect.Size = new System.Drawing.Size(183, 22);
            this.friendListSelect.TabIndex = 61;
            this.friendListSelect.SelectedIndexChanged += new System.EventHandler(this.friendListSelect_SelectedIndexChanged);
            #endregion
            #region Restock Tab
            // 
            // restock
            // 
            this.restock.Controls.Add(this.restockCloneListB);
            this.restock.Controls.Add(this.restockExecuteButton);
            this.restock.Controls.Add(this.restockStopButton);
            this.restock.Controls.Add(this.groupBox3);
            this.restock.Controls.Add(this.restockdataGridView);
            this.restock.Controls.Add(this.groupBox2);
            this.restock.Controls.Add(this.label13);
            this.restock.Controls.Add(this.label7);
            this.restock.Controls.Add(this.restockAddTargetButton);
            this.restock.Controls.Add(this.restockRemoveListB);
            this.restock.Controls.Add(this.restockAddListB);
            this.restock.Controls.Add(this.restockListSelect);
            this.restock.Controls.Add(this.restockDragDelay);
            this.restock.Location = new System.Drawing.Point(4, 22);
            this.restock.Name = "restock";
            this.restock.Padding = new System.Windows.Forms.Padding(3);
            this.restock.Size = new System.Drawing.Size(677, 347);
            this.restock.TabIndex = 7;
            this.restock.Text = "Restock";
            this.restock.UseVisualStyleBackColor = true;
            // 
            // restockCloneListB
            // 
            this.restockCloneListB.Location = new System.Drawing.Point(416, 11);
            this.restockCloneListB.Name = "restockCloneListB";
            this.restockCloneListB.Size = new System.Drawing.Size(67, 21);
            this.restockCloneListB.TabIndex = 94;
            this.restockCloneListB.Text = "Clone";
            this.restockCloneListB.Click += new System.EventHandler(this.restockCloneListB_Click);
            // 
            // restockExecuteButton
            // 
            this.restockExecuteButton.BackgroundImage = global::Assistant.Properties.Resources.playagent;
            this.restockExecuteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.restockExecuteButton.FlatAppearance.BorderSize = 0;
            this.restockExecuteButton.Location = new System.Drawing.Point(283, 58);
            this.restockExecuteButton.Name = "restockExecuteButton";
            this.restockExecuteButton.Size = new System.Drawing.Size(30, 30);
            this.restockExecuteButton.TabIndex = 93;
            this.restockExecuteButton.UseVisualStyleBackColor = true;
            this.restockExecuteButton.Click += new System.EventHandler(this.restockExecuteButton_Click);
            // 
            // restockStopButton
            // 
            this.restockStopButton.BackgroundImage = global::Assistant.Properties.Resources.stopagent;
            this.restockStopButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.restockStopButton.FlatAppearance.BorderSize = 0;
            this.restockStopButton.Location = new System.Drawing.Point(319, 58);
            this.restockStopButton.Name = "restockStopButton";
            this.restockStopButton.Size = new System.Drawing.Size(30, 30);
            this.restockStopButton.TabIndex = 92;
            this.restockStopButton.UseVisualStyleBackColor = true;
            this.restockStopButton.Click += new System.EventHandler(this.restockStopButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label59);
            this.groupBox3.Controls.Add(this.label58);
            this.groupBox3.Controls.Add(this.restockSetSourceButton);
            this.groupBox3.Controls.Add(this.restockSourceLabel);
            this.groupBox3.Controls.Add(this.restockDestinationLabel);
            this.groupBox3.Controls.Add(this.restockSetDestinationButton);
            this.groupBox3.Location = new System.Drawing.Point(9, 42);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(252, 65);
            this.groupBox3.TabIndex = 91;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Restock Bags";
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Location = new System.Drawing.Point(6, 42);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(63, 14);
            this.label59.TabIndex = 92;
            this.label59.Text = "Destination:";
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Location = new System.Drawing.Point(6, 18);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(45, 14);
            this.label58.TabIndex = 91;
            this.label58.Text = "Source:";
            // 
            // restockSetSourceButton
            // 
            this.restockSetSourceButton.Location = new System.Drawing.Point(156, 12);
            this.restockSetSourceButton.Name = "restockSetSourceButton";
            this.restockSetSourceButton.Size = new System.Drawing.Size(90, 21);
            this.restockSetSourceButton.TabIndex = 76;
            this.restockSetSourceButton.Text = "Set Bag";
            this.restockSetSourceButton.Click += new System.EventHandler(this.restockSetSourceButton_Click);
            // 
            // restockSourceLabel
            // 
            this.restockSourceLabel.Location = new System.Drawing.Point(71, 18);
            this.restockSourceLabel.Name = "restockSourceLabel";
            this.restockSourceLabel.Size = new System.Drawing.Size(82, 19);
            this.restockSourceLabel.TabIndex = 77;
            this.restockSourceLabel.Text = "0x00000000";
            // 
            // restockDestinationLabel
            // 
            this.restockDestinationLabel.Location = new System.Drawing.Point(71, 42);
            this.restockDestinationLabel.Name = "restockDestinationLabel";
            this.restockDestinationLabel.Size = new System.Drawing.Size(82, 19);
            this.restockDestinationLabel.TabIndex = 80;
            this.restockDestinationLabel.Text = "0x00000000";
            // 
            // restockSetDestinationButton
            // 
            this.restockSetDestinationButton.Location = new System.Drawing.Point(156, 38);
            this.restockSetDestinationButton.Name = "restockSetDestinationButton";
            this.restockSetDestinationButton.Size = new System.Drawing.Size(90, 21);
            this.restockSetDestinationButton.TabIndex = 79;
            this.restockSetDestinationButton.Text = "Set Bag";
            this.restockSetDestinationButton.Click += new System.EventHandler(this.restockSetDestinationButton_Click);
            // 
            // restockdataGridView
            // 
            this.restockdataGridView.AllowDrop = true;
            this.restockdataGridView.AllowUserToResizeRows = false;
            this.restockdataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.restockdataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.restockdataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.restockdataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn3,
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10,
            this.dataGridViewTextBoxColumn11,
            this.dataGridViewTextBoxColumn12});
            this.restockdataGridView.Location = new System.Drawing.Point(9, 113);
            this.restockdataGridView.Name = "restockdataGridView";
            this.restockdataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.restockdataGridView.RowHeadersVisible = false;
            this.restockdataGridView.RowHeadersWidth = 62;
            this.restockdataGridView.Size = new System.Drawing.Size(376, 200);
            this.restockdataGridView.TabIndex = 90;
            this.restockdataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridView_CellContentClick);
            this.restockdataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.restockdataGridView_CellEndEdit);
            this.restockdataGridView.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_CellMouseUp);
            this.restockdataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.GridView_CurrentCellDirtyStateChanged);
            this.restockdataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.GridView_DataError);
            this.restockdataGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.restockdataGridView_DefaultValuesNeeded);
            this.restockdataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.GridView_DragDrop);
            this.restockdataGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.GridView_DragOver);
            this.restockdataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseDown);
            this.restockdataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridView_MouseMove);
            // 
            // dataGridViewCheckBoxColumn3
            // 
            this.dataGridViewCheckBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewCheckBoxColumn3.FalseValue = "False";
            this.dataGridViewCheckBoxColumn3.FillWeight = 1F;
            this.dataGridViewCheckBoxColumn3.HeaderText = "X";
            this.dataGridViewCheckBoxColumn3.IndeterminateValue = "False";
            this.dataGridViewCheckBoxColumn3.MinimumWidth = 10;
            this.dataGridViewCheckBoxColumn3.Name = "dataGridViewCheckBoxColumn3";
            this.dataGridViewCheckBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn3.ToolTipText = "Check This for enable item in list";
            this.dataGridViewCheckBoxColumn3.TrueValue = "True";
            this.dataGridViewCheckBoxColumn3.Width = 21;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn9.FillWeight = 70F;
            this.dataGridViewTextBoxColumn9.HeaderText = "Item Name";
            this.dataGridViewTextBoxColumn9.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn9.ToolTipText = "Here the item name";
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn10.FillWeight = 10F;
            this.dataGridViewTextBoxColumn10.HeaderText = "Graphics";
            this.dataGridViewTextBoxColumn10.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            this.dataGridViewTextBoxColumn10.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn10.ToolTipText = "Here Graphics item ID";
            this.dataGridViewTextBoxColumn10.Width = 80;
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn11.FillWeight = 10F;
            this.dataGridViewTextBoxColumn11.HeaderText = "Color";
            this.dataGridViewTextBoxColumn11.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            this.dataGridViewTextBoxColumn11.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn11.ToolTipText = "Here item color, use -1 for all color";
            this.dataGridViewTextBoxColumn11.Width = 57;
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn12.FillWeight = 10F;
            this.dataGridViewTextBoxColumn12.HeaderText = "Limit";
            this.dataGridViewTextBoxColumn12.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            this.dataGridViewTextBoxColumn12.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn12.ToolTipText = "Here amount limit to move";
            this.dataGridViewTextBoxColumn12.Width = 53;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.restockLogBox);
            this.groupBox2.Location = new System.Drawing.Point(391, 84);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(278, 231);
            this.groupBox2.TabIndex = 83;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Restock Log";
            // 
            // restockLogBox
            // 
            this.restockLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.restockLogBox.FormattingEnabled = true;
            this.restockLogBox.ItemHeight = 14;
            this.restockLogBox.Location = new System.Drawing.Point(7, 18);
            this.restockLogBox.Name = "restockLogBox";
            this.restockLogBox.Size = new System.Drawing.Size(265, 74);
            this.restockLogBox.TabIndex = 0;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(438, 54);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(107, 14);
            this.label13.TabIndex = 82;
            this.label13.Text = "Drag Item Delay (ms)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(69, 14);
            this.label7.TabIndex = 66;
            this.label7.Text = "Restock List:";
            // 
            // restockAddTargetButton
            // 
            this.restockAddTargetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.restockAddTargetButton.Location = new System.Drawing.Point(581, 39);
            this.restockAddTargetButton.Name = "restockAddTargetButton";
            this.restockAddTargetButton.Size = new System.Drawing.Size(90, 21);
            this.restockAddTargetButton.TabIndex = 47;
            this.restockAddTargetButton.Text = "Add Item";
            this.restockAddTargetButton.Click += new System.EventHandler(this.restockAddTargetButton_Click);
            // 
            // restockRemoveListB
            // 
            this.restockRemoveListB.Location = new System.Drawing.Point(343, 12);
            this.restockRemoveListB.Name = "restockRemoveListB";
            this.restockRemoveListB.Size = new System.Drawing.Size(67, 21);
            this.restockRemoveListB.TabIndex = 69;
            this.restockRemoveListB.Text = "Remove";
            this.restockRemoveListB.Click += new System.EventHandler(this.restockRemoveListB_Click);
            // 
            // restockAddListB
            // 
            this.restockAddListB.Location = new System.Drawing.Point(270, 12);
            this.restockAddListB.Name = "restockAddListB";
            this.restockAddListB.Size = new System.Drawing.Size(67, 21);
            this.restockAddListB.TabIndex = 68;
            this.restockAddListB.Text = "Add";
            this.restockAddListB.Click += new System.EventHandler(this.restockAddListB_Click);
            // 
            // restockListSelect
            // 
            this.restockListSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.restockListSelect.FormattingEnabled = true;
            this.restockListSelect.Location = new System.Drawing.Point(78, 12);
            this.restockListSelect.Name = "restockListSelect";
            this.restockListSelect.Size = new System.Drawing.Size(183, 22);
            this.restockListSelect.TabIndex = 67;
            this.restockListSelect.SelectedIndexChanged += new System.EventHandler(this.restockListSelect_SelectedIndexChanged);
            // 
            // restockDragDelay
            // 
            this.restockDragDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.restockDragDelay.BackColor = System.Drawing.Color.White;
            this.restockDragDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.restockDragDelay.Location = new System.Drawing.Point(391, 51);
            this.restockDragDelay.Name = "restockDragDelay";
            this.restockDragDelay.Size = new System.Drawing.Size(45, 20);
            this.restockDragDelay.TabIndex = 81;
            this.restockDragDelay.Leave += new System.EventHandler(this.restockDragDelay_Leave);
            #endregion
            #region Bandage Heal Tab
            // 
            // bandageheal
            // 
            this.bandageheal.Controls.Add(this.BandageHealSettingsBox);
            this.bandageheal.Controls.Add(this.groupBox5);
            this.bandageheal.Location = new System.Drawing.Point(4, 22);
            this.bandageheal.Name = "bandageheal";
            this.bandageheal.Padding = new System.Windows.Forms.Padding(3);
            this.bandageheal.Size = new System.Drawing.Size(677, 347);
            this.bandageheal.TabIndex = 8;
            this.bandageheal.Text = "Bandage Heal";
            this.bandageheal.UseVisualStyleBackColor = true;
            // 
            // BandageHealSettingsBox
            // 
            this.BandageHealSettingsBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealAutostartCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.bandageHealIgnoreCount);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealTimeWithBufCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.label78);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealenableCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.label77);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealusetextContent);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealusetextSelfContent);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealusetext);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealusetarget);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealmaxrangeTextBox);
            this.BandageHealSettingsBox.Controls.Add(this.label46);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealcountdownCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealhiddedCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealmortalCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealpoisonCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.label33);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealhpTextBox);
            this.BandageHealSettingsBox.Controls.Add(this.label32);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealdelayTextBox);
            this.BandageHealSettingsBox.Controls.Add(this.label31);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealdexformulaCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealcustomcolorTextBox);
            this.BandageHealSettingsBox.Controls.Add(this.label30);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealcustomIDTextBox);
            this.BandageHealSettingsBox.Controls.Add(this.label19);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealcustomCheckBox);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealtargetLabel);
            this.BandageHealSettingsBox.Controls.Add(this.label15);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealsettargetButton);
            this.BandageHealSettingsBox.Controls.Add(this.bandagehealtargetComboBox);
            this.BandageHealSettingsBox.Controls.Add(this.label14);
            this.BandageHealSettingsBox.Location = new System.Drawing.Point(325, 24);
            this.BandageHealSettingsBox.Name = "BandageHealSettingsBox";
            this.BandageHealSettingsBox.Size = new System.Drawing.Size(346, 332);
            this.BandageHealSettingsBox.TabIndex = 74;
            this.BandageHealSettingsBox.TabStop = false;
            this.BandageHealSettingsBox.Text = "Settings";
            // 
            // bandagehealAutostartCheckBox
            // 
            this.bandagehealAutostartCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealAutostartCheckBox.Location = new System.Drawing.Point(221, 19);
            this.bandagehealAutostartCheckBox.Name = "bandagehealAutostartCheckBox";
            this.bandagehealAutostartCheckBox.Size = new System.Drawing.Size(118, 22);
            this.bandagehealAutostartCheckBox.TabIndex = 75;
            this.bandagehealAutostartCheckBox.Text = "Autostart OnLogin";
            this.bandagehealAutostartCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealAutostartCheckBox_CheckedChanged);
            // 
            // bandageHealIgnoreCount
            // 
            this.bandageHealIgnoreCount.AutoSize = true;
            this.bandageHealIgnoreCount.Location = new System.Drawing.Point(210, 304);
            this.bandageHealIgnoreCount.Name = "bandageHealIgnoreCount";
            this.bandageHealIgnoreCount.Size = new System.Drawing.Size(87, 18);
            this.bandageHealIgnoreCount.TabIndex = 98;
            this.bandageHealIgnoreCount.Text = "Ignore Count";
            this.bandageHealIgnoreCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.bandageHealIgnoreCount.UseVisualStyleBackColor = true;
            this.bandageHealIgnoreCount.CheckedChanged += new System.EventHandler(this.bandagehealignorecount_CheckedChanged);
            // 
            // bandagehealTimeWithBufCheckBox
            // 
            this.bandagehealTimeWithBufCheckBox.AutoSize = true;
            this.bandagehealTimeWithBufCheckBox.Location = new System.Drawing.Point(180, 142);
            this.bandagehealTimeWithBufCheckBox.Name = "bandagehealTimeWithBufCheckBox";
            this.bandagehealTimeWithBufCheckBox.Size = new System.Drawing.Size(96, 18);
            this.bandagehealTimeWithBufCheckBox.TabIndex = 97;
            this.bandagehealTimeWithBufCheckBox.Text = "Time with Buff";
            this.bandagehealTimeWithBufCheckBox.UseVisualStyleBackColor = true;
            this.bandagehealTimeWithBufCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealBufControlled_CheckedChanged);
            // 
            // label78
            // 
            this.label78.AutoSize = true;
            this.label78.Location = new System.Drawing.Point(207, 285);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(37, 14);
            this.label78.TabIndex = 96;
            this.label78.Text = "Other:";
            // 
            // bandagehealenableCheckBox
            // 
            this.bandagehealenableCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealenableCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealenableCheckBox.Location = new System.Drawing.Point(10, 19);
            this.bandagehealenableCheckBox.Name = "bandagehealenableCheckBox";
            this.bandagehealenableCheckBox.Size = new System.Drawing.Size(146, 22);
            this.bandagehealenableCheckBox.TabIndex = 73;
            this.bandagehealenableCheckBox.Text = "Enable Bandage Heal";
            this.bandagehealenableCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealenableCheckBox_CheckedChanged);
            // 
            // label77
            // 
            this.label77.AutoSize = true;
            this.label77.Location = new System.Drawing.Point(207, 260);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(29, 14);
            this.label77.TabIndex = 95;
            this.label77.Text = "Self:";
            // 
            // bandagehealusetextContent
            // 
            this.bandagehealusetextContent.Enabled = false;
            this.bandagehealusetextContent.Location = new System.Drawing.Point(254, 282);
            this.bandagehealusetextContent.Name = "bandagehealusetextContent";
            this.bandagehealusetextContent.Size = new System.Drawing.Size(82, 20);
            this.bandagehealusetextContent.TabIndex = 94;
            this.bandagehealusetextContent.Text = "[band";
            this.bandagehealusetextContent.Leave += new System.EventHandler(this.bandagehealusetext_Content_Leave);
            // 
            // bandagehealusetextSelfContent
            // 
            this.bandagehealusetextSelfContent.Enabled = false;
            this.bandagehealusetextSelfContent.Location = new System.Drawing.Point(254, 256);
            this.bandagehealusetextSelfContent.Name = "bandagehealusetextSelfContent";
            this.bandagehealusetextSelfContent.Size = new System.Drawing.Size(82, 20);
            this.bandagehealusetextSelfContent.TabIndex = 94;
            this.bandagehealusetextSelfContent.Text = "[bandself";
            this.bandagehealusetextSelfContent.Leave += new System.EventHandler(this.bandagehealusetextSelf_Content_Leave);
            // 
            // bandagehealusetext
            // 
            this.bandagehealusetext.AutoSize = true;
            this.bandagehealusetext.Location = new System.Drawing.Point(184, 230);
            this.bandagehealusetext.Name = "bandagehealusetext";
            this.bandagehealusetext.Size = new System.Drawing.Size(133, 18);
            this.bandagehealusetext.TabIndex = 93;
            this.bandagehealusetext.Text = "Send text for self heal";
            this.bandagehealusetext.UseVisualStyleBackColor = true;
            this.bandagehealusetext.CheckedChanged += new System.EventHandler(this.bandagehealusetext_CheckedChanged);
            // 
            // bandagehealusetarget
            // 
            this.bandagehealusetarget.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealusetarget.Location = new System.Drawing.Point(184, 199);
            this.bandagehealusetarget.Name = "bandagehealusetarget";
            this.bandagehealusetarget.Size = new System.Drawing.Size(155, 22);
            this.bandagehealusetarget.TabIndex = 92;
            this.bandagehealusetarget.Text = "Use Normal Target";
            this.bandagehealusetarget.CheckedChanged += new System.EventHandler(this.bandagehealusetarget_CheckedChanged);
            // 
            // bandagehealmaxrangeTextBox
            // 
            this.bandagehealmaxrangeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealmaxrangeTextBox.BackColor = System.Drawing.Color.White;
            this.bandagehealmaxrangeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bandagehealmaxrangeTextBox.Location = new System.Drawing.Point(77, 307);
            this.bandagehealmaxrangeTextBox.Name = "bandagehealmaxrangeTextBox";
            this.bandagehealmaxrangeTextBox.Size = new System.Drawing.Size(29, 20);
            this.bandagehealmaxrangeTextBox.TabIndex = 91;
            this.bandagehealmaxrangeTextBox.Leave += new System.EventHandler(this.bandagehealmaxrangeTextBox_Leave);
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(7, 310);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(64, 14);
            this.label46.TabIndex = 90;
            this.label46.Text = "Max Range:";
            // 
            // bandagehealcountdownCheckBox
            // 
            this.bandagehealcountdownCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealcountdownCheckBox.Location = new System.Drawing.Point(10, 283);
            this.bandagehealcountdownCheckBox.Name = "bandagehealcountdownCheckBox";
            this.bandagehealcountdownCheckBox.Size = new System.Drawing.Size(155, 22);
            this.bandagehealcountdownCheckBox.TabIndex = 89;
            this.bandagehealcountdownCheckBox.Text = "Show Heal Countdown";
            this.bandagehealcountdownCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealcountdownCheckBox_CheckedChanged);
            // 
            // bandagehealhiddedCheckBox
            // 
            this.bandagehealhiddedCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealhiddedCheckBox.Location = new System.Drawing.Point(10, 255);
            this.bandagehealhiddedCheckBox.Name = "bandagehealhiddedCheckBox";
            this.bandagehealhiddedCheckBox.Size = new System.Drawing.Size(155, 22);
            this.bandagehealhiddedCheckBox.TabIndex = 88;
            this.bandagehealhiddedCheckBox.Text = "Block heal if Hidden";
            this.bandagehealhiddedCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealhiddedCheckBox_CheckedChanged);
            // 
            // bandagehealmortalCheckBox
            // 
            this.bandagehealmortalCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealmortalCheckBox.Location = new System.Drawing.Point(10, 227);
            this.bandagehealmortalCheckBox.Name = "bandagehealmortalCheckBox";
            this.bandagehealmortalCheckBox.Size = new System.Drawing.Size(155, 22);
            this.bandagehealmortalCheckBox.TabIndex = 87;
            this.bandagehealmortalCheckBox.Text = "Block heal if Mortalled";
            this.bandagehealmortalCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealmortalCheckBox_CheckedChanged);
            // 
            // bandagehealpoisonCheckBox
            // 
            this.bandagehealpoisonCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealpoisonCheckBox.Location = new System.Drawing.Point(10, 199);
            this.bandagehealpoisonCheckBox.Name = "bandagehealpoisonCheckBox";
            this.bandagehealpoisonCheckBox.Size = new System.Drawing.Size(155, 22);
            this.bandagehealpoisonCheckBox.TabIndex = 86;
            this.bandagehealpoisonCheckBox.Text = "Block heal if Poisoned";
            this.bandagehealpoisonCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealpoisonCheckBox_CheckedChanged);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(135, 176);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(37, 14);
            this.label33.TabIndex = 85;
            this.label33.Text = "% hits";
            // 
            // bandagehealhpTextBox
            // 
            this.bandagehealhpTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealhpTextBox.BackColor = System.Drawing.Color.White;
            this.bandagehealhpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bandagehealhpTextBox.Location = new System.Drawing.Point(76, 173);
            this.bandagehealhpTextBox.Name = "bandagehealhpTextBox";
            this.bandagehealhpTextBox.Size = new System.Drawing.Size(53, 20);
            this.bandagehealhpTextBox.TabIndex = 84;
            this.bandagehealhpTextBox.Leave += new System.EventHandler(this.bandagehealhpTextBox_Leave);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(7, 175);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(67, 14);
            this.label32.TabIndex = 83;
            this.label32.Text = "Start Below:";
            // 
            // bandagehealdelayTextBox
            // 
            this.bandagehealdelayTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealdelayTextBox.BackColor = System.Drawing.Color.White;
            this.bandagehealdelayTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bandagehealdelayTextBox.Location = new System.Drawing.Point(283, 171);
            this.bandagehealdelayTextBox.Name = "bandagehealdelayTextBox";
            this.bandagehealdelayTextBox.Size = new System.Drawing.Size(53, 20);
            this.bandagehealdelayTextBox.TabIndex = 82;
            this.bandagehealdelayTextBox.Leave += new System.EventHandler(this.bandagehealdelayTextBox_Leave);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(202, 174);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(76, 14);
            this.label31.TabIndex = 81;
            this.label31.Text = "Custom Delay:";
            // 
            // bandagehealdexformulaCheckBox
            // 
            this.bandagehealdexformulaCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealdexformulaCheckBox.Location = new System.Drawing.Point(10, 139);
            this.bandagehealdexformulaCheckBox.Name = "bandagehealdexformulaCheckBox";
            this.bandagehealdexformulaCheckBox.Size = new System.Drawing.Size(129, 22);
            this.bandagehealdexformulaCheckBox.TabIndex = 80;
            this.bandagehealdexformulaCheckBox.Text = "Use DEX formula delay";
            this.bandagehealdexformulaCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealdexformulaCheckBox_CheckedChanged);
            // 
            // bandagehealcustomcolorTextBox
            // 
            this.bandagehealcustomcolorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealcustomcolorTextBox.BackColor = System.Drawing.Color.White;
            this.bandagehealcustomcolorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bandagehealcustomcolorTextBox.Enabled = false;
            this.bandagehealcustomcolorTextBox.Location = new System.Drawing.Point(286, 113);
            this.bandagehealcustomcolorTextBox.Name = "bandagehealcustomcolorTextBox";
            this.bandagehealcustomcolorTextBox.Size = new System.Drawing.Size(53, 20);
            this.bandagehealcustomcolorTextBox.TabIndex = 79;
            this.bandagehealcustomcolorTextBox.Leave += new System.EventHandler(this.bandagehealcustomcolorTextBox_Leave);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(246, 116);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(35, 14);
            this.label30.TabIndex = 78;
            this.label30.Text = "Color:";
            // 
            // bandagehealcustomIDTextBox
            // 
            this.bandagehealcustomIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealcustomIDTextBox.BackColor = System.Drawing.Color.White;
            this.bandagehealcustomIDTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bandagehealcustomIDTextBox.Enabled = false;
            this.bandagehealcustomIDTextBox.Location = new System.Drawing.Point(180, 112);
            this.bandagehealcustomIDTextBox.Name = "bandagehealcustomIDTextBox";
            this.bandagehealcustomIDTextBox.Size = new System.Drawing.Size(53, 20);
            this.bandagehealcustomIDTextBox.TabIndex = 77;
            this.bandagehealcustomIDTextBox.Leave += new System.EventHandler(this.bandagehealcustomIDTextBox_Leave);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(153, 115);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(19, 14);
            this.label19.TabIndex = 76;
            this.label19.Text = "ID:";
            // 
            // bandagehealcustomCheckBox
            // 
            this.bandagehealcustomCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bandagehealcustomCheckBox.Location = new System.Drawing.Point(10, 111);
            this.bandagehealcustomCheckBox.Name = "bandagehealcustomCheckBox";
            this.bandagehealcustomCheckBox.Size = new System.Drawing.Size(137, 22);
            this.bandagehealcustomCheckBox.TabIndex = 75;
            this.bandagehealcustomCheckBox.Text = "Use Custom Bandage";
            this.bandagehealcustomCheckBox.CheckedChanged += new System.EventHandler(this.bandagehealcustomCheckBox_CheckedChanged);
            // 
            // bandagehealtargetLabel
            // 
            this.bandagehealtargetLabel.AutoSize = true;
            this.bandagehealtargetLabel.Location = new System.Drawing.Point(73, 86);
            this.bandagehealtargetLabel.Name = "bandagehealtargetLabel";
            this.bandagehealtargetLabel.Size = new System.Drawing.Size(95, 14);
            this.bandagehealtargetLabel.TabIndex = 4;
            this.bandagehealtargetLabel.Text = "Null (0x00000000)";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 86);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(40, 14);
            this.label15.TabIndex = 3;
            this.label15.Text = "Target:";
            // 
            // bandagehealsettargetButton
            // 
            this.bandagehealsettargetButton.Location = new System.Drawing.Point(213, 51);
            this.bandagehealsettargetButton.Name = "bandagehealsettargetButton";
            this.bandagehealsettargetButton.Size = new System.Drawing.Size(75, 23);
            this.bandagehealsettargetButton.TabIndex = 2;
            this.bandagehealsettargetButton.Text = "Set Target";
            this.bandagehealsettargetButton.UseVisualStyleBackColor = true;
            this.bandagehealsettargetButton.Click += new System.EventHandler(this.bandagehealsettargetButton_Click);
            // 
            // bandagehealtargetComboBox
            // 
            this.bandagehealtargetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bandagehealtargetComboBox.FormattingEnabled = true;
            this.bandagehealtargetComboBox.Location = new System.Drawing.Point(76, 52);
            this.bandagehealtargetComboBox.Name = "bandagehealtargetComboBox";
            this.bandagehealtargetComboBox.Size = new System.Drawing.Size(121, 22);
            this.bandagehealtargetComboBox.TabIndex = 1;
            this.bandagehealtargetComboBox.SelectedIndexChanged += new System.EventHandler(this.bandagehealtargetComboBox_SelectedIndexChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 57);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(64, 14);
            this.label14.TabIndex = 0;
            this.label14.Text = "Heal Target:";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.bandagehealLogBox);
            this.groupBox5.Location = new System.Drawing.Point(6, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(297, 313);
            this.groupBox5.TabIndex = 54;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Bandage Heal Log";
            // 
            // bandagehealLogBox
            // 
            this.bandagehealLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bandagehealLogBox.FormattingEnabled = true;
            this.bandagehealLogBox.ItemHeight = 14;
            this.bandagehealLogBox.Location = new System.Drawing.Point(7, 18);
            this.bandagehealLogBox.Name = "bandagehealLogBox";
            this.bandagehealLogBox.Size = new System.Drawing.Size(283, 270);
            this.bandagehealLogBox.TabIndex = 0;
            #endregion
            #endregion
            #region Toolbars Tab
            // 
            // toolbarTab
            // 
            this.toolbarTab.Controls.Add(this.toolbarstab);
            this.toolbarTab.Location = new System.Drawing.Point(4, 54);
            this.toolbarTab.Name = "toolbarTab";
            this.toolbarTab.Size = new System.Drawing.Size(678, 365);
            this.toolbarTab.TabIndex = 1;
            this.toolbarTab.Text = "Toolbars";
            // 
            // toolbarstab
            // 
            this.toolbarstab.Controls.Add(this.tabPage2);
            this.toolbarstab.Controls.Add(this.tabPage3);
            this.toolbarstab.Location = new System.Drawing.Point(3, 3);
            this.toolbarstab.Name = "toolbarstab";
            this.toolbarstab.SelectedIndex = 0;
            this.toolbarstab.Size = new System.Drawing.Size(660, 363);
            this.toolbarstab.TabIndex = 62;
            #region Counter Tab
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.groupBox39);
            this.tabPage2.Controls.Add(this.groupBox25);
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Controls.Add(this.groupBox26);
            this.tabPage2.Location = new System.Drawing.Point(4, 23);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(652, 336);
            this.tabPage2.TabIndex = 0;
            this.tabPage2.Text = LanguageHelper.GetString("MainForm.tabPage2.Text");
            // 
            // groupBox39
            // 
            this.groupBox39.Controls.Add(this.toolbar_trackBar);
            this.groupBox39.Controls.Add(this.toolbar_opacity_label);
            this.groupBox39.Location = new System.Drawing.Point(351, 183);
            this.groupBox39.Name = "groupBox39";
            this.groupBox39.Size = new System.Drawing.Size(295, 50);
            this.groupBox39.TabIndex = 65;
            this.groupBox39.TabStop = false;
            this.groupBox39.Text = "Opacity";
            // 
            // toolbar_trackBar
            // 
            this.toolbar_trackBar.AutoSize = false;
            this.toolbar_trackBar.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.toolbar_trackBar.Location = new System.Drawing.Point(54, 24);
            this.toolbar_trackBar.Maximum = 100;
            this.toolbar_trackBar.Minimum = 10;
            this.toolbar_trackBar.Name = "toolbar_trackBar";
            this.toolbar_trackBar.Size = new System.Drawing.Size(235, 16);
            this.toolbar_trackBar.TabIndex = 62;
            this.toolbar_trackBar.TickFrequency = 0;
            this.toolbar_trackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.toolbar_trackBar.Value = 100;
            this.toolbar_trackBar.Scroll += new System.EventHandler(this.toolbar_trackBar_Scroll);
            // 
            // toolbar_opacity_label
            // 
            this.toolbar_opacity_label.Location = new System.Drawing.Point(8, 24);
            this.toolbar_opacity_label.Name = "toolbar_opacity_label";
            this.toolbar_opacity_label.Size = new System.Drawing.Size(40, 16);
            this.toolbar_opacity_label.TabIndex = 63;
            this.toolbar_opacity_label.Text = "100%";
            // 
            // groupBox25
            // 
            this.groupBox25.Controls.Add(this.lockToolBarCheckBox);
            this.groupBox25.Controls.Add(this.autoopenToolBarCheckBox);
            this.groupBox25.Controls.Add(this.locationToolBarLabel);
            this.groupBox25.Controls.Add(this.closeToolBarButton);
            this.groupBox25.Controls.Add(this.openToolBarButton);
            this.groupBox25.Location = new System.Drawing.Point(6, 6);
            this.groupBox25.Name = "groupBox25";
            this.groupBox25.Size = new System.Drawing.Size(121, 159);
            this.groupBox25.TabIndex = 59;
            this.groupBox25.TabStop = false;
            this.groupBox25.Text = "General";
            // 
            // lockToolBarCheckBox
            // 
            this.lockToolBarCheckBox.Location = new System.Drawing.Point(6, 71);
            this.lockToolBarCheckBox.Name = "lockToolBarCheckBox";
            this.lockToolBarCheckBox.Size = new System.Drawing.Size(99, 22);
            this.lockToolBarCheckBox.TabIndex = 63;
            this.lockToolBarCheckBox.Text = "Lock ToolBar";
            this.lockToolBarCheckBox.CheckedChanged += new System.EventHandler(this.lockToolBarCheckBox_CheckedChanged);
            // 
            // autoopenToolBarCheckBox
            // 
            this.autoopenToolBarCheckBox.Location = new System.Drawing.Point(6, 93);
            this.autoopenToolBarCheckBox.Name = "autoopenToolBarCheckBox";
            this.autoopenToolBarCheckBox.Size = new System.Drawing.Size(112, 22);
            this.autoopenToolBarCheckBox.TabIndex = 62;
            this.autoopenToolBarCheckBox.Text = "Open On Login";
            this.autoopenToolBarCheckBox.CheckedChanged += new System.EventHandler(this.autoopenToolBarCheckBox_CheckedChanged);
            // 
            // locationToolBarLabel
            // 
            this.locationToolBarLabel.AutoSize = true;
            this.locationToolBarLabel.Location = new System.Drawing.Point(6, 118);
            this.locationToolBarLabel.Name = "locationToolBarLabel";
            this.locationToolBarLabel.Size = new System.Drawing.Size(42, 14);
            this.locationToolBarLabel.TabIndex = 61;
            this.locationToolBarLabel.Text = "X:0 Y:0";
            // 
            // closeToolBarButton
            // 
            this.closeToolBarButton.Location = new System.Drawing.Point(6, 45);
            this.closeToolBarButton.Name = "closeToolBarButton";
            this.closeToolBarButton.Size = new System.Drawing.Size(90, 21);
            this.closeToolBarButton.TabIndex = 59;
            this.closeToolBarButton.Text = "Close";
            this.closeToolBarButton.Click += new System.EventHandler(this.closeToolBarButton_Click);
            // 
            // openToolBarButton
            // 
            this.openToolBarButton.Location = new System.Drawing.Point(6, 19);
            this.openToolBarButton.Name = "openToolBarButton";
            this.openToolBarButton.Size = new System.Drawing.Size(90, 21);
            this.openToolBarButton.TabIndex = 58;
            this.openToolBarButton.Text = "Open";
            this.openToolBarButton.Click += new System.EventHandler(this.openToolBarButton_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.showtitheToolBarCheckBox);
            this.groupBox4.Controls.Add(this.toolbarremoveslotButton);
            this.groupBox4.Controls.Add(this.toolbaraddslotButton);
            this.groupBox4.Controls.Add(this.toolbarslot_label);
            this.groupBox4.Controls.Add(this.label43);
            this.groupBox4.Controls.Add(this.toolboxsizeComboBox);
            this.groupBox4.Controls.Add(this.label41);
            this.groupBox4.Controls.Add(this.showfollowerToolBarCheckBox);
            this.groupBox4.Controls.Add(this.showweightToolBarCheckBox);
            this.groupBox4.Controls.Add(this.showmanaToolBarCheckBox);
            this.groupBox4.Controls.Add(this.showstaminaToolBarCheckBox);
            this.groupBox4.Controls.Add(this.showhitsToolBarCheckBox);
            this.groupBox4.Controls.Add(this.toolboxstyleComboBox);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Location = new System.Drawing.Point(351, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(295, 168);
            this.groupBox4.TabIndex = 61;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Layout";
            // 
            // showtitheToolBarCheckBox
            // 
            this.showtitheToolBarCheckBox.AutoSize = true;
            this.showtitheToolBarCheckBox.Location = new System.Drawing.Point(181, 145);
            this.showtitheToolBarCheckBox.Name = "showtitheToolBarCheckBox";
            this.showtitheToolBarCheckBox.Size = new System.Drawing.Size(81, 18);
            this.showtitheToolBarCheckBox.TabIndex = 80;
            this.showtitheToolBarCheckBox.Text = "Show Tithe";
            this.showtitheToolBarCheckBox.UseVisualStyleBackColor = true;
            this.showtitheToolBarCheckBox.CheckedChanged += new System.EventHandler(this.showtitheToolBarCheckBox_CheckedChanged);
            // 
            // toolbarremoveslotButton
            // 
            this.toolbarremoveslotButton.Location = new System.Drawing.Point(86, 85);
            this.toolbarremoveslotButton.Name = "toolbarremoveslotButton";
            this.toolbarremoveslotButton.Size = new System.Drawing.Size(20, 20);
            this.toolbarremoveslotButton.TabIndex = 79;
            this.toolbarremoveslotButton.Text = "-";
            this.toolbarremoveslotButton.Click += new System.EventHandler(this.toolbarremoveslotButton_Click);
            // 
            // toolbaraddslotButton
            // 
            this.toolbaraddslotButton.Location = new System.Drawing.Point(61, 85);
            this.toolbaraddslotButton.Name = "toolbaraddslotButton";
            this.toolbaraddslotButton.Size = new System.Drawing.Size(20, 20);
            this.toolbaraddslotButton.TabIndex = 71;
            this.toolbaraddslotButton.Text = "+";
            this.toolbaraddslotButton.Click += new System.EventHandler(this.toolbaraddslotButton_Click);
            // 
            // toolbarslot_label
            // 
            this.toolbarslot_label.AutoSize = true;
            this.toolbarslot_label.Location = new System.Drawing.Point(42, 89);
            this.toolbarslot_label.Name = "toolbarslot_label";
            this.toolbarslot_label.Size = new System.Drawing.Size(13, 14);
            this.toolbarslot_label.TabIndex = 78;
            this.toolbarslot_label.Text = "0";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(6, 89);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(34, 14);
            this.label43.TabIndex = 71;
            this.label43.Text = "Slots:";
            // 
            // toolboxsizeComboBox
            // 
            this.toolboxsizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolboxsizeComboBox.FormattingEnabled = true;
            this.toolboxsizeComboBox.Location = new System.Drawing.Point(45, 52);
            this.toolboxsizeComboBox.Name = "toolboxsizeComboBox";
            this.toolboxsizeComboBox.Size = new System.Drawing.Size(112, 22);
            this.toolboxsizeComboBox.TabIndex = 77;
            this.toolboxsizeComboBox.SelectedIndexChanged += new System.EventHandler(this.toolboxsizeComboBox_SelectedIndexChanged);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(6, 58);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(31, 14);
            this.label41.TabIndex = 76;
            this.label41.Text = "Size:";
            // 
            // showfollowerToolBarCheckBox
            // 
            this.showfollowerToolBarCheckBox.Location = new System.Drawing.Point(181, 121);
            this.showfollowerToolBarCheckBox.Name = "showfollowerToolBarCheckBox";
            this.showfollowerToolBarCheckBox.Size = new System.Drawing.Size(99, 22);
            this.showfollowerToolBarCheckBox.TabIndex = 75;
            this.showfollowerToolBarCheckBox.Text = "Show Follower";
            this.showfollowerToolBarCheckBox.CheckedChanged += new System.EventHandler(this.showfollowerToolBarCheckBox_CheckedChanged);
            // 
            // showweightToolBarCheckBox
            // 
            this.showweightToolBarCheckBox.Location = new System.Drawing.Point(181, 95);
            this.showweightToolBarCheckBox.Name = "showweightToolBarCheckBox";
            this.showweightToolBarCheckBox.Size = new System.Drawing.Size(99, 22);
            this.showweightToolBarCheckBox.TabIndex = 74;
            this.showweightToolBarCheckBox.Text = "Show Weight";
            this.showweightToolBarCheckBox.CheckedChanged += new System.EventHandler(this.showweightToolBarCheckBox_CheckedChanged);
            // 
            // showmanaToolBarCheckBox
            // 
            this.showmanaToolBarCheckBox.Location = new System.Drawing.Point(181, 69);
            this.showmanaToolBarCheckBox.Name = "showmanaToolBarCheckBox";
            this.showmanaToolBarCheckBox.Size = new System.Drawing.Size(99, 22);
            this.showmanaToolBarCheckBox.TabIndex = 73;
            this.showmanaToolBarCheckBox.Text = "Show Mana";
            this.showmanaToolBarCheckBox.CheckedChanged += new System.EventHandler(this.showmanaToolBarCheckBox_CheckedChanged);
            // 
            // showstaminaToolBarCheckBox
            // 
            this.showstaminaToolBarCheckBox.Location = new System.Drawing.Point(181, 43);
            this.showstaminaToolBarCheckBox.Name = "showstaminaToolBarCheckBox";
            this.showstaminaToolBarCheckBox.Size = new System.Drawing.Size(99, 22);
            this.showstaminaToolBarCheckBox.TabIndex = 72;
            this.showstaminaToolBarCheckBox.Text = "Show Stamina";
            this.showstaminaToolBarCheckBox.CheckedChanged += new System.EventHandler(this.showstaminaToolBarCheckBox_CheckedChanged);
            // 
            // showhitsToolBarCheckBox
            // 
            this.showhitsToolBarCheckBox.Location = new System.Drawing.Point(181, 17);
            this.showhitsToolBarCheckBox.Name = "showhitsToolBarCheckBox";
            this.showhitsToolBarCheckBox.Size = new System.Drawing.Size(99, 22);
            this.showhitsToolBarCheckBox.TabIndex = 64;
            this.showhitsToolBarCheckBox.Text = "Show Hits";
            this.showhitsToolBarCheckBox.CheckedChanged += new System.EventHandler(this.showhitsToolBarCheckBox_CheckedChanged);
            // 
            // toolboxstyleComboBox
            // 
            this.toolboxstyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolboxstyleComboBox.FormattingEnabled = true;
            this.toolboxstyleComboBox.Location = new System.Drawing.Point(45, 19);
            this.toolboxstyleComboBox.Name = "toolboxstyleComboBox";
            this.toolboxstyleComboBox.Size = new System.Drawing.Size(112, 22);
            this.toolboxstyleComboBox.TabIndex = 71;
            this.toolboxstyleComboBox.SelectedIndexChanged += new System.EventHandler(this.toolboxstyleComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 14);
            this.label2.TabIndex = 0;
            this.label2.Text = "Style:";
            // 
            // groupBox26
            // 
            this.groupBox26.Controls.Add(this.label38);
            this.groupBox26.Controls.Add(this.toolboxcountNameTextBox);
            this.groupBox26.Controls.Add(this.label37);
            this.groupBox26.Controls.Add(this.toolboxcountClearButton);
            this.groupBox26.Controls.Add(this.toolboxcountTargetButton);
            this.groupBox26.Controls.Add(this.toolboxcountWarningTextBox);
            this.groupBox26.Controls.Add(this.label36);
            this.groupBox26.Controls.Add(this.toolboxcountHueWarningCheckBox);
            this.groupBox26.Controls.Add(this.toolboxcountHueTextBox);
            this.groupBox26.Controls.Add(this.label35);
            this.groupBox26.Controls.Add(this.toolboxcountGraphTextBox);
            this.groupBox26.Controls.Add(this.label18);
            this.groupBox26.Controls.Add(this.toolboxcountComboBox);
            this.groupBox26.Location = new System.Drawing.Point(130, 6);
            this.groupBox26.Name = "groupBox26";
            this.groupBox26.Size = new System.Drawing.Size(214, 203);
            this.groupBox26.TabIndex = 60;
            this.groupBox26.TabStop = false;
            this.groupBox26.Text = "Item Count";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(131, 102);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(47, 14);
            this.label38.TabIndex = 70;
            this.label38.Text = "-1 for all";
            // 
            // toolboxcountNameTextBox
            // 
            this.toolboxcountNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolboxcountNameTextBox.BackColor = System.Drawing.Color.White;
            this.toolboxcountNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolboxcountNameTextBox.Location = new System.Drawing.Point(64, 49);
            this.toolboxcountNameTextBox.Name = "toolboxcountNameTextBox";
            this.toolboxcountNameTextBox.Size = new System.Drawing.Size(144, 20);
            this.toolboxcountNameTextBox.TabIndex = 69;
            this.toolboxcountNameTextBox.TextChanged += new System.EventHandler(this.toolboxcountNameTextBox_TextChanged);
            this.toolboxcountNameTextBox.Leave += new System.EventHandler(this.toolboxcountNameTextBox_Leave);
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(6, 52);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(37, 14);
            this.label37.TabIndex = 68;
            this.label37.Text = "Name:";
            // 
            // toolboxcountClearButton
            // 
            this.toolboxcountClearButton.Location = new System.Drawing.Point(131, 177);
            this.toolboxcountClearButton.Name = "toolboxcountClearButton";
            this.toolboxcountClearButton.Size = new System.Drawing.Size(77, 21);
            this.toolboxcountClearButton.TabIndex = 67;
            this.toolboxcountClearButton.Text = "Clear Slot";
            this.toolboxcountClearButton.Click += new System.EventHandler(this.toolboxcountClearButton_Click);
            // 
            // toolboxcountTargetButton
            // 
            this.toolboxcountTargetButton.Location = new System.Drawing.Point(9, 177);
            this.toolboxcountTargetButton.Name = "toolboxcountTargetButton";
            this.toolboxcountTargetButton.Size = new System.Drawing.Size(77, 21);
            this.toolboxcountTargetButton.TabIndex = 64;
            this.toolboxcountTargetButton.Text = "Get Data";
            this.toolboxcountTargetButton.Click += new System.EventHandler(this.toolboxcountTargetButton_Click);
            // 
            // toolboxcountWarningTextBox
            // 
            this.toolboxcountWarningTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolboxcountWarningTextBox.BackColor = System.Drawing.Color.White;
            this.toolboxcountWarningTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolboxcountWarningTextBox.Location = new System.Drawing.Point(64, 148);
            this.toolboxcountWarningTextBox.Name = "toolboxcountWarningTextBox";
            this.toolboxcountWarningTextBox.Size = new System.Drawing.Size(61, 20);
            this.toolboxcountWarningTextBox.TabIndex = 66;
            this.toolboxcountWarningTextBox.TextChanged += new System.EventHandler(this.toolboxcountWarningTextBox_TextChanged);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(6, 151);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(50, 14);
            this.label36.TabIndex = 65;
            this.label36.Text = "Warning:";
            // 
            // toolboxcountHueWarningCheckBox
            // 
            this.toolboxcountHueWarningCheckBox.Location = new System.Drawing.Point(9, 125);
            this.toolboxcountHueWarningCheckBox.Name = "toolboxcountHueWarningCheckBox";
            this.toolboxcountHueWarningCheckBox.Size = new System.Drawing.Size(99, 22);
            this.toolboxcountHueWarningCheckBox.TabIndex = 64;
            this.toolboxcountHueWarningCheckBox.Text = "Show Warning";
            this.toolboxcountHueWarningCheckBox.CheckedChanged += new System.EventHandler(this.toolboxcountHueWarningCheckBox_CheckedChanged);
            // 
            // toolboxcountHueTextBox
            // 
            this.toolboxcountHueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolboxcountHueTextBox.BackColor = System.Drawing.Color.White;
            this.toolboxcountHueTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolboxcountHueTextBox.Location = new System.Drawing.Point(64, 99);
            this.toolboxcountHueTextBox.Name = "toolboxcountHueTextBox";
            this.toolboxcountHueTextBox.Size = new System.Drawing.Size(61, 20);
            this.toolboxcountHueTextBox.TabIndex = 4;
            this.toolboxcountHueTextBox.TextChanged += new System.EventHandler(this.toolboxcountHueTextBox_TextChanged);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(6, 102);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(35, 14);
            this.label35.TabIndex = 3;
            this.label35.Text = "Color:";
            // 
            // toolboxcountGraphTextBox
            // 
            this.toolboxcountGraphTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolboxcountGraphTextBox.BackColor = System.Drawing.Color.White;
            this.toolboxcountGraphTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolboxcountGraphTextBox.Location = new System.Drawing.Point(64, 73);
            this.toolboxcountGraphTextBox.Name = "toolboxcountGraphTextBox";
            this.toolboxcountGraphTextBox.Size = new System.Drawing.Size(61, 20);
            this.toolboxcountGraphTextBox.TabIndex = 2;
            this.toolboxcountGraphTextBox.TextChanged += new System.EventHandler(this.toolboxcountGraphTextBox_TextChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 76);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(54, 14);
            this.label18.TabIndex = 1;
            this.label18.Text = "Graphics:";
            // 
            // toolboxcountComboBox
            // 
            this.toolboxcountComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolboxcountComboBox.FormattingEnabled = true;
            this.toolboxcountComboBox.Location = new System.Drawing.Point(6, 19);
            this.toolboxcountComboBox.Name = "toolboxcountComboBox";
            this.toolboxcountComboBox.Size = new System.Drawing.Size(202, 22);
            this.toolboxcountComboBox.TabIndex = 0;
            this.toolboxcountComboBox.SelectedIndexChanged += new System.EventHandler(this.toolboxcountComboBox_SelectedIndexChanged);
            #endregion
            #region Spell Grid Tab
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.groupBox38);
            this.tabPage3.Controls.Add(this.groupBox37);
            this.tabPage3.Controls.Add(this.groupBox36);
            this.tabPage3.Controls.Add(this.groupBox35);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(652, 337);
            this.tabPage3.TabIndex = 1;
            this.tabPage3.Text = LanguageHelper.GetString("MainForm.tabPage3.Text");
            // 
            // groupBox38
            // 
            this.groupBox38.Controls.Add(this.spellgrid_trackBar);
            this.groupBox38.Controls.Add(this.spellgrid_opacity_label);
            this.groupBox38.Location = new System.Drawing.Point(427, 135);
            this.groupBox38.Name = "groupBox38";
            this.groupBox38.Size = new System.Drawing.Size(219, 48);
            this.groupBox38.TabIndex = 66;
            this.groupBox38.TabStop = false;
            this.groupBox38.Text = "Opacity";
            // 
            // spellgrid_trackBar
            // 
            this.spellgrid_trackBar.AutoSize = false;
            this.spellgrid_trackBar.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.spellgrid_trackBar.Location = new System.Drawing.Point(43, 20);
            this.spellgrid_trackBar.Maximum = 100;
            this.spellgrid_trackBar.Minimum = 10;
            this.spellgrid_trackBar.Name = "spellgrid_trackBar";
            this.spellgrid_trackBar.Size = new System.Drawing.Size(170, 16);
            this.spellgrid_trackBar.TabIndex = 62;
            this.spellgrid_trackBar.TickFrequency = 0;
            this.spellgrid_trackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.spellgrid_trackBar.Value = 100;
            this.spellgrid_trackBar.Scroll += new System.EventHandler(this.spellgrid_trackBar_Scroll);
            // 
            // spellgrid_opacity_label
            // 
            this.spellgrid_opacity_label.Location = new System.Drawing.Point(6, 20);
            this.spellgrid_opacity_label.Name = "spellgrid_opacity_label";
            this.spellgrid_opacity_label.Size = new System.Drawing.Size(36, 16);
            this.spellgrid_opacity_label.TabIndex = 63;
            this.spellgrid_opacity_label.Text = "100%";
            // 
            // groupBox37
            // 
            this.groupBox37.Controls.Add(this.spellgridstyleComboBox);
            this.groupBox37.Controls.Add(this.label80);
            this.groupBox37.Controls.Add(this.gridhslotremove_button);
            this.groupBox37.Controls.Add(this.gridhslotadd_button);
            this.groupBox37.Controls.Add(this.gridhslot_textbox);
            this.groupBox37.Controls.Add(this.label53);
            this.groupBox37.Controls.Add(this.gridvslotremove_button);
            this.groupBox37.Controls.Add(this.gridvslotadd_button);
            this.groupBox37.Controls.Add(this.gridvslot_textbox);
            this.groupBox37.Controls.Add(this.label49);
            this.groupBox37.Location = new System.Drawing.Point(427, 6);
            this.groupBox37.Name = "groupBox37";
            this.groupBox37.Size = new System.Drawing.Size(172, 115);
            this.groupBox37.TabIndex = 65;
            this.groupBox37.TabStop = false;
            this.groupBox37.Text = "Layout";
            // 
            // spellgridstyleComboBox
            // 
            this.spellgridstyleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spellgridstyleComboBox.FormattingEnabled = true;
            this.spellgridstyleComboBox.Items.AddRange(new object[] {
            "Window",
            "Gump"});
            this.spellgridstyleComboBox.Location = new System.Drawing.Point(51, 18);
            this.spellgridstyleComboBox.Name = "spellgridstyleComboBox";
            this.spellgridstyleComboBox.Size = new System.Drawing.Size(111, 22);
            this.spellgridstyleComboBox.TabIndex = 85;
            this.spellgridstyleComboBox.SelectedIndexChanged += new System.EventHandler(this.spellgridstyleComboBox_SelectedIndexChanged);
            // 
            // label80
            // 
            this.label80.AutoSize = true;
            this.label80.Location = new System.Drawing.Point(12, 24);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(34, 14);
            this.label80.TabIndex = 84;
            this.label80.Text = "Style:";
            // 
            // gridhslotremove_button
            // 
            this.gridhslotremove_button.Location = new System.Drawing.Point(90, 86);
            this.gridhslotremove_button.Name = "gridhslotremove_button";
            this.gridhslotremove_button.Size = new System.Drawing.Size(20, 20);
            this.gridhslotremove_button.TabIndex = 83;
            this.gridhslotremove_button.Text = "-";
            this.gridhslotremove_button.Click += new System.EventHandler(this.gridhslotremove_button_Click);
            // 
            // gridhslotadd_button
            // 
            this.gridhslotadd_button.Location = new System.Drawing.Point(65, 86);
            this.gridhslotadd_button.Name = "gridhslotadd_button";
            this.gridhslotadd_button.Size = new System.Drawing.Size(20, 20);
            this.gridhslotadd_button.TabIndex = 80;
            this.gridhslotadd_button.Text = "+";
            this.gridhslotadd_button.Click += new System.EventHandler(this.gridhslotadd_button_Click);
            // 
            // gridhslot_textbox
            // 
            this.gridhslot_textbox.AutoSize = true;
            this.gridhslot_textbox.Location = new System.Drawing.Point(48, 90);
            this.gridhslot_textbox.Name = "gridhslot_textbox";
            this.gridhslot_textbox.Size = new System.Drawing.Size(13, 14);
            this.gridhslot_textbox.TabIndex = 82;
            this.gridhslot_textbox.Text = "0";
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(6, 90);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(44, 14);
            this.label53.TabIndex = 81;
            this.label53.Text = "Slots H:";
            // 
            // gridvslotremove_button
            // 
            this.gridvslotremove_button.Location = new System.Drawing.Point(90, 60);
            this.gridvslotremove_button.Name = "gridvslotremove_button";
            this.gridvslotremove_button.Size = new System.Drawing.Size(20, 20);
            this.gridvslotremove_button.TabIndex = 79;
            this.gridvslotremove_button.Text = "-";
            this.gridvslotremove_button.Click += new System.EventHandler(this.gridvslotremove_button_Click);
            // 
            // gridvslotadd_button
            // 
            this.gridvslotadd_button.Location = new System.Drawing.Point(65, 60);
            this.gridvslotadd_button.Name = "gridvslotadd_button";
            this.gridvslotadd_button.Size = new System.Drawing.Size(20, 20);
            this.gridvslotadd_button.TabIndex = 71;
            this.gridvslotadd_button.Text = "+";
            this.gridvslotadd_button.Click += new System.EventHandler(this.gridvslotadd_button_Click);
            // 
            // gridvslot_textbox
            // 
            this.gridvslot_textbox.AutoSize = true;
            this.gridvslot_textbox.Location = new System.Drawing.Point(48, 64);
            this.gridvslot_textbox.Name = "gridvslot_textbox";
            this.gridvslot_textbox.Size = new System.Drawing.Size(13, 14);
            this.gridvslot_textbox.TabIndex = 78;
            this.gridvslot_textbox.Text = "0";
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(6, 64);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(45, 14);
            this.label49.TabIndex = 71;
            this.label49.Text = "Slots V:";
            // 
            // groupBox36
            // 
            this.groupBox36.Controls.Add(this.gridscript_ComboBox);
            this.groupBox36.Controls.Add(this.label65);
            this.groupBox36.Controls.Add(this.gridborder_ComboBox);
            this.groupBox36.Controls.Add(this.label44);
            this.groupBox36.Controls.Add(this.gridspell_ComboBox);
            this.groupBox36.Controls.Add(this.label52);
            this.groupBox36.Controls.Add(this.gridgroup_ComboBox);
            this.groupBox36.Controls.Add(this.label51);
            this.groupBox36.Controls.Add(this.label45);
            this.groupBox36.Controls.Add(this.gridslot_ComboBox);
            this.groupBox36.Location = new System.Drawing.Point(133, 6);
            this.groupBox36.Name = "groupBox36";
            this.groupBox36.Size = new System.Drawing.Size(288, 177);
            this.groupBox36.TabIndex = 64;
            this.groupBox36.TabStop = false;
            this.groupBox36.Text = "Grid Item";
            // 
            // gridscript_ComboBox
            // 
            this.gridscript_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gridscript_ComboBox.FormattingEnabled = true;
            this.gridscript_ComboBox.Location = new System.Drawing.Point(75, 136);
            this.gridscript_ComboBox.Name = "gridscript_ComboBox";
            this.gridscript_ComboBox.Size = new System.Drawing.Size(202, 22);
            this.gridscript_ComboBox.TabIndex = 78;
            this.gridscript_ComboBox.SelectedIndexChanged += new System.EventHandler(this.gridscript_ComboBox_SelectedIndexChanged);
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.Location = new System.Drawing.Point(6, 141);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(35, 14);
            this.label65.TabIndex = 77;
            this.label65.Text = "Script";
            // 
            // gridborder_ComboBox
            // 
            this.gridborder_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gridborder_ComboBox.FormattingEnabled = true;
            this.gridborder_ComboBox.Location = new System.Drawing.Point(75, 107);
            this.gridborder_ComboBox.Name = "gridborder_ComboBox";
            this.gridborder_ComboBox.Size = new System.Drawing.Size(202, 22);
            this.gridborder_ComboBox.TabIndex = 76;
            this.gridborder_ComboBox.SelectedIndexChanged += new System.EventHandler(this.gridborder_ComboBox_SelectedIndexChanged);
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(6, 112);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(46, 14);
            this.label44.TabIndex = 75;
            this.label44.Text = "Border: ";
            // 
            // gridspell_ComboBox
            // 
            this.gridspell_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gridspell_ComboBox.FormattingEnabled = true;
            this.gridspell_ComboBox.Location = new System.Drawing.Point(75, 77);
            this.gridspell_ComboBox.Name = "gridspell_ComboBox";
            this.gridspell_ComboBox.Size = new System.Drawing.Size(202, 22);
            this.gridspell_ComboBox.TabIndex = 74;
            this.gridspell_ComboBox.SelectedIndexChanged += new System.EventHandler(this.gridspell_ComboBox_SelectedIndexChanged);
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Location = new System.Drawing.Point(6, 82);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(67, 14);
            this.label52.TabIndex = 73;
            this.label52.Text = "Abilitie/Spell:";
            // 
            // gridgroup_ComboBox
            // 
            this.gridgroup_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gridgroup_ComboBox.FormattingEnabled = true;
            this.gridgroup_ComboBox.Location = new System.Drawing.Point(75, 47);
            this.gridgroup_ComboBox.Name = "gridgroup_ComboBox";
            this.gridgroup_ComboBox.Size = new System.Drawing.Size(202, 22);
            this.gridgroup_ComboBox.TabIndex = 72;
            this.gridgroup_ComboBox.SelectedIndexChanged += new System.EventHandler(this.gridgroup_ComboBox_SelectedIndexChanged);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(6, 23);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(28, 14);
            this.label51.TabIndex = 71;
            this.label51.Text = "Slot:";
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(6, 52);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(40, 14);
            this.label45.TabIndex = 68;
            this.label45.Text = "Group:";
            // 
            // gridslot_ComboBox
            // 
            this.gridslot_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gridslot_ComboBox.FormattingEnabled = true;
            this.gridslot_ComboBox.Location = new System.Drawing.Point(75, 17);
            this.gridslot_ComboBox.Name = "gridslot_ComboBox";
            this.gridslot_ComboBox.Size = new System.Drawing.Size(202, 22);
            this.gridslot_ComboBox.TabIndex = 0;
            this.gridslot_ComboBox.SelectedIndexChanged += new System.EventHandler(this.gridslot_ComboBox_SelectedIndexChanged);
            // 
            // groupBox35
            // 
            this.groupBox35.Controls.Add(this.setSpellBarOrigin);
            this.groupBox35.Controls.Add(this.gridlock_CheckBox);
            this.groupBox35.Controls.Add(this.gridopenlogin_CheckBox);
            this.groupBox35.Controls.Add(this.gridlocation_label);
            this.groupBox35.Controls.Add(this.gridclose_button);
            this.groupBox35.Controls.Add(this.gridopen_button);
            this.groupBox35.Location = new System.Drawing.Point(6, 6);
            this.groupBox35.Name = "groupBox35";
            this.groupBox35.Size = new System.Drawing.Size(121, 177);
            this.groupBox35.TabIndex = 63;
            this.groupBox35.TabStop = false;
            this.groupBox35.Text = "General";
            // 
            // setSpellBarOrigin
            // 
            this.setSpellBarOrigin.Location = new System.Drawing.Point(6, 141);
            this.setSpellBarOrigin.Name = "setSpellBarOrigin";
            this.setSpellBarOrigin.Size = new System.Drawing.Size(90, 20);
            this.setSpellBarOrigin.TabIndex = 64;
            this.setSpellBarOrigin.Text = "Set Origin";
            this.setSpellBarOrigin.UseVisualStyleBackColor = true;
            this.setSpellBarOrigin.Click += new System.EventHandler(this.spellGridSetOrigin);
            // 
            // gridlock_CheckBox
            // 
            this.gridlock_CheckBox.Location = new System.Drawing.Point(6, 71);
            this.gridlock_CheckBox.Name = "gridlock_CheckBox";
            this.gridlock_CheckBox.Size = new System.Drawing.Size(99, 22);
            this.gridlock_CheckBox.TabIndex = 63;
            this.gridlock_CheckBox.Text = "Lock Grid";
            this.gridlock_CheckBox.CheckedChanged += new System.EventHandler(this.gridlock_CheckBox_CheckedChanged);
            // 
            // gridopenlogin_CheckBox
            // 
            this.gridopenlogin_CheckBox.Location = new System.Drawing.Point(6, 93);
            this.gridopenlogin_CheckBox.Name = "gridopenlogin_CheckBox";
            this.gridopenlogin_CheckBox.Size = new System.Drawing.Size(112, 22);
            this.gridopenlogin_CheckBox.TabIndex = 62;
            this.gridopenlogin_CheckBox.Text = "Open On Login";
            this.gridopenlogin_CheckBox.CheckedChanged += new System.EventHandler(this.gridopenlogin_CheckBox_CheckedChanged);
            // 
            // gridlocation_label
            // 
            this.gridlocation_label.AutoSize = true;
            this.gridlocation_label.Location = new System.Drawing.Point(6, 118);
            this.gridlocation_label.Name = "gridlocation_label";
            this.gridlocation_label.Size = new System.Drawing.Size(42, 14);
            this.gridlocation_label.TabIndex = 61;
            this.gridlocation_label.Text = "X:0 Y:0";
            // 
            // gridclose_button
            // 
            this.gridclose_button.Location = new System.Drawing.Point(6, 45);
            this.gridclose_button.Name = "gridclose_button";
            this.gridclose_button.Size = new System.Drawing.Size(90, 21);
            this.gridclose_button.TabIndex = 59;
            this.gridclose_button.Text = "Close";
            this.gridclose_button.Click += new System.EventHandler(this.gridclose_button_Click);
            // 
            // gridopen_button
            // 
            this.gridopen_button.Location = new System.Drawing.Point(6, 19);
            this.gridopen_button.Name = "gridopen_button";
            this.gridopen_button.Size = new System.Drawing.Size(90, 21);
            this.gridopen_button.TabIndex = 58;
            this.gridopen_button.Text = "Open";
            this.gridopen_button.Click += new System.EventHandler(this.gridopen_button_Click);
            #endregion
            #endregion
            #region Skills Tab
            // 
            // skillsTab
            // 
            this.skillsTab.Controls.Add(this.dispDelta);
            this.skillsTab.Controls.Add(this.skillCopyAll);
            this.skillsTab.Controls.Add(this.skillCopySel);
            this.skillsTab.Controls.Add(this.baseTotal);
            this.skillsTab.Controls.Add(this.label1);
            this.skillsTab.Controls.Add(this.locks);
            this.skillsTab.Controls.Add(this.setlocks);
            this.skillsTab.Controls.Add(this.resetDelta);
            this.skillsTab.Controls.Add(this.skillList);
            this.skillsTab.Location = new System.Drawing.Point(4, 54);
            this.skillsTab.Name = "skillsTab";
            this.skillsTab.Size = new System.Drawing.Size(678, 365);
            this.skillsTab.TabIndex = 2;
            this.skillsTab.Text = "Skills";
            // 
            // dispDelta
            // 
            this.dispDelta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dispDelta.Location = new System.Drawing.Point(530, 144);
            this.dispDelta.Name = "dispDelta";
            this.dispDelta.Size = new System.Drawing.Size(113, 22);
            this.dispDelta.TabIndex = 11;
            this.dispDelta.Text = "Display Changes";
            this.dispDelta.CheckedChanged += new System.EventHandler(this.dispDelta_CheckedChanged);
            // 
            // skillCopyAll
            // 
            this.skillCopyAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.skillCopyAll.Location = new System.Drawing.Point(530, 109);
            this.skillCopyAll.Name = "skillCopyAll";
            this.skillCopyAll.Size = new System.Drawing.Size(132, 20);
            this.skillCopyAll.TabIndex = 9;
            this.skillCopyAll.Text = "Copy All";
            this.skillCopyAll.Click += new System.EventHandler(this.skillCopyAll_Click);
            // 
            // skillCopySel
            // 
            this.skillCopySel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.skillCopySel.Location = new System.Drawing.Point(530, 81);
            this.skillCopySel.Name = "skillCopySel";
            this.skillCopySel.Size = new System.Drawing.Size(132, 21);
            this.skillCopySel.TabIndex = 8;
            this.skillCopySel.Text = "Copy Selected";
            this.skillCopySel.Click += new System.EventHandler(this.skillCopySel_Click);
            // 
            // baseTotal
            // 
            this.baseTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.baseTotal.Location = new System.Drawing.Point(600, 175);
            this.baseTotal.Name = "baseTotal";
            this.baseTotal.ReadOnly = true;
            this.baseTotal.Size = new System.Drawing.Size(43, 20);
            this.baseTotal.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(530, 179);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Base Total:";
            // 
            // locks
            // 
            this.locks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.locks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.locks.Font = new System.Drawing.Font("Arial", 9F);
            this.locks.Items.AddRange(new object[] {
            "Up",
            "Down",
            "Locked"});
            this.locks.Location = new System.Drawing.Point(612, 45);
            this.locks.Name = "locks";
            this.locks.Size = new System.Drawing.Size(50, 23);
            this.locks.TabIndex = 5;
            // 
            // setlocks
            // 
            this.setlocks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.setlocks.Location = new System.Drawing.Point(530, 46);
            this.setlocks.Name = "setlocks";
            this.setlocks.Size = new System.Drawing.Size(76, 20);
            this.setlocks.TabIndex = 4;
            this.setlocks.Text = "Set all locks:";
            this.setlocks.Click += new System.EventHandler(this.OnSetSkillLocks);
            // 
            // resetDelta
            // 
            this.resetDelta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetDelta.Location = new System.Drawing.Point(530, 13);
            this.resetDelta.Name = "resetDelta";
            this.resetDelta.Size = new System.Drawing.Size(132, 20);
            this.resetDelta.TabIndex = 3;
            this.resetDelta.Text = "Reset  +/-";
            this.resetDelta.Click += new System.EventHandler(this.OnResetSkillDelta);
            // 
            // skillList
            // 
            this.skillList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.skillList.AutoArrange = false;
            this.skillList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.skillHDRName,
            this.skillHDRvalue,
            this.skillHDRbase,
            this.skillHDRdelta,
            this.skillHDRcap,
            this.skillHDRlock});
            this.skillList.FullRowSelect = true;
            this.skillList.HideSelection = false;
            this.skillList.Location = new System.Drawing.Point(7, 13);
            this.skillList.Name = "skillList";
            this.skillList.Size = new System.Drawing.Size(505, 366);
            this.skillList.TabIndex = 1;
            this.skillList.UseCompatibleStateImageBehavior = false;
            this.skillList.View = System.Windows.Forms.View.Details;
            this.skillList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnSkillColClick);
            this.skillList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.skillList_MouseDown);
            // 
            // skillHDRName
            // 
            this.skillHDRName.Text = "Skill Name";
            this.skillHDRName.Width = 252;
            // 
            // skillHDRvalue
            // 
            this.skillHDRvalue.Text = "Value";
            this.skillHDRvalue.Width = 100;
            // 
            // skillHDRbase
            // 
            this.skillHDRbase.Text = "Base";
            this.skillHDRbase.Width = 100;
            // 
            // skillHDRdelta
            // 
            this.skillHDRdelta.Text = "+/-";
            this.skillHDRdelta.Width = 40;
            // 
            // skillHDRcap
            // 
            this.skillHDRcap.Text = "Cap";
            this.skillHDRcap.Width = 100;
            // 
            // skillHDRlock
            // 
            this.skillHDRlock.Text = "Lock";
            this.skillHDRlock.Width = 80;
            // Initialize Skills Tab UI (modern RazorCard layout)
            InitializeSkillsTab();
            #endregion
            #region Hot Keys Tab
            // 
            // enhancedHotKeytabPage
            // 
            this.enhancedHotKeytabPage.Controls.Add(this.groupBox8);
            this.enhancedHotKeytabPage.Controls.Add(this.groupBox28);
            this.enhancedHotKeytabPage.Controls.Add(this.groupBox27);
            this.enhancedHotKeytabPage.Controls.Add(this.hotkeytreeView);
            this.enhancedHotKeytabPage.Location = new System.Drawing.Point(4, 54);
            this.enhancedHotKeytabPage.Name = "enhancedHotKeytabPage";
            this.enhancedHotKeytabPage.Padding = new System.Windows.Forms.Padding(3);
            this.enhancedHotKeytabPage.Size = new System.Drawing.Size(678, 365);
            this.enhancedHotKeytabPage.TabIndex = 15;
            this.enhancedHotKeytabPage.Text = "Hot Keys";
            this.enhancedHotKeytabPage.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox8.Controls.Add(this.hotkeyMasterClearButton);
            this.groupBox8.Controls.Add(this.hotkeyKeyMasterTextBox);
            this.groupBox8.Controls.Add(this.hotkeyMasterSetButton);
            this.groupBox8.Controls.Add(this.label42);
            this.groupBox8.Location = new System.Drawing.Point(515, 105);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(157, 84);
            this.groupBox8.TabIndex = 4;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Master Key";
            // 
            // hotkeyMasterClearButton
            // 
            this.hotkeyMasterClearButton.Location = new System.Drawing.Point(92, 50);
            this.hotkeyMasterClearButton.Name = "hotkeyMasterClearButton";
            this.hotkeyMasterClearButton.Size = new System.Drawing.Size(53, 23);
            this.hotkeyMasterClearButton.TabIndex = 5;
            this.hotkeyMasterClearButton.Text = "Clear";
            this.hotkeyMasterClearButton.UseVisualStyleBackColor = true;
            this.hotkeyMasterClearButton.Click += new System.EventHandler(this.hotkeyMasterClearButton_Click);
            // 
            // hotkeyKeyMasterTextBox
            // 
            this.hotkeyKeyMasterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hotkeyKeyMasterTextBox.BackColor = System.Drawing.Color.White;
            this.hotkeyKeyMasterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hotkeyKeyMasterTextBox.Location = new System.Drawing.Point(41, 19);
            this.hotkeyKeyMasterTextBox.Name = "hotkeyKeyMasterTextBox";
            this.hotkeyKeyMasterTextBox.ReadOnly = true;
            this.hotkeyKeyMasterTextBox.Size = new System.Drawing.Size(104, 20);
            this.hotkeyKeyMasterTextBox.TabIndex = 5;
            this.hotkeyKeyMasterTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HotKey_MouseDown);
            this.hotkeyKeyMasterTextBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.HotKey_MouseRoll);
            // 
            // hotkeyMasterSetButton
            // 
            this.hotkeyMasterSetButton.Location = new System.Drawing.Point(10, 50);
            this.hotkeyMasterSetButton.Name = "hotkeyMasterSetButton";
            this.hotkeyMasterSetButton.Size = new System.Drawing.Size(53, 23);
            this.hotkeyMasterSetButton.TabIndex = 7;
            this.hotkeyMasterSetButton.Text = "Set";
            this.hotkeyMasterSetButton.UseVisualStyleBackColor = true;
            this.hotkeyMasterSetButton.Click += new System.EventHandler(this.hotkeyMasterSetButton_Click);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(7, 22);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(29, 14);
            this.label42.TabIndex = 6;
            this.label42.Text = "Key:";
            // 
            // groupBox28
            // 
            this.groupBox28.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox28.Controls.Add(this.hotkeyMDisableButton);
            this.groupBox28.Controls.Add(this.hotkeyMEnableButton);
            this.groupBox28.Controls.Add(this.hotkeyKeyMasterLabel);
            this.groupBox28.Controls.Add(this.hotkeyStatusLabel);
            this.groupBox28.Location = new System.Drawing.Point(515, 7);
            this.groupBox28.Name = "groupBox28";
            this.groupBox28.Size = new System.Drawing.Size(157, 92);
            this.groupBox28.TabIndex = 3;
            this.groupBox28.TabStop = false;
            this.groupBox28.Text = "General";
            // 
            // hotkeyMDisableButton
            // 
            this.hotkeyMDisableButton.Location = new System.Drawing.Point(92, 58);
            this.hotkeyMDisableButton.Name = "hotkeyMDisableButton";
            this.hotkeyMDisableButton.Size = new System.Drawing.Size(53, 23);
            this.hotkeyMDisableButton.TabIndex = 8;
            this.hotkeyMDisableButton.Text = "Disable";
            this.hotkeyMDisableButton.UseVisualStyleBackColor = true;
            this.hotkeyMDisableButton.Click += new System.EventHandler(this.hotkeyDisableButton_Click);
            // 
            // hotkeyMEnableButton
            // 
            this.hotkeyMEnableButton.Location = new System.Drawing.Point(10, 58);
            this.hotkeyMEnableButton.Name = "hotkeyMEnableButton";
            this.hotkeyMEnableButton.Size = new System.Drawing.Size(53, 23);
            this.hotkeyMEnableButton.TabIndex = 9;
            this.hotkeyMEnableButton.Text = "Enable";
            this.hotkeyMEnableButton.UseVisualStyleBackColor = true;
            this.hotkeyMEnableButton.Click += new System.EventHandler(this.hotkeyEnableButton_Click);
            // 
            // hotkeyKeyMasterLabel
            // 
            this.hotkeyKeyMasterLabel.AutoSize = true;
            this.hotkeyKeyMasterLabel.Location = new System.Drawing.Point(7, 38);
            this.hotkeyKeyMasterLabel.Name = "hotkeyKeyMasterLabel";
            this.hotkeyKeyMasterLabel.Size = new System.Drawing.Size(98, 14);
            this.hotkeyKeyMasterLabel.TabIndex = 4;
            this.hotkeyKeyMasterLabel.Text = LanguageHelper.GetString("MainForm.hotkeyKeyMaster.None");
            // 
            // hotkeyStatusLabel
            // 
            this.hotkeyStatusLabel.AutoSize = true;
            this.hotkeyStatusLabel.Location = new System.Drawing.Point(7, 16);
            this.hotkeyStatusLabel.Name = "hotkeyStatusLabel";
            this.hotkeyStatusLabel.Size = new System.Drawing.Size(82, 14);
            this.hotkeyStatusLabel.TabIndex = 3;
            this.hotkeyStatusLabel.Text = LanguageHelper.GetString("MainForm.hotkeyStatus.Enabled");
            // 
            // groupBox27
            // 
            this.groupBox27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox27.Controls.Add(this.hotkeypassCheckBox);
            this.groupBox27.Controls.Add(this.hotkeyClearButton);
            this.groupBox27.Controls.Add(this.hotkeySetButton);
            this.groupBox27.Controls.Add(this.label39);
            this.groupBox27.Controls.Add(this.hotkeytextbox);
            this.groupBox27.Location = new System.Drawing.Point(515, 195);
            this.groupBox27.Name = "groupBox27";
            this.groupBox27.Size = new System.Drawing.Size(157, 107);
            this.groupBox27.TabIndex = 2;
            this.groupBox27.TabStop = false;
            this.groupBox27.Text = "Modify Key";
            // 
            // hotkeypassCheckBox
            // 
            this.hotkeypassCheckBox.Location = new System.Drawing.Point(10, 43);
            this.hotkeypassCheckBox.Name = "hotkeypassCheckBox";
            this.hotkeypassCheckBox.Size = new System.Drawing.Size(103, 22);
            this.hotkeypassCheckBox.TabIndex = 49;
            this.hotkeypassCheckBox.Text = "Pass Key to UO";
            this.hotkeypassCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hotkeyClearButton
            // 
            this.hotkeyClearButton.Location = new System.Drawing.Point(92, 71);
            this.hotkeyClearButton.Name = "hotkeyClearButton";
            this.hotkeyClearButton.Size = new System.Drawing.Size(53, 23);
            this.hotkeyClearButton.TabIndex = 4;
            this.hotkeyClearButton.Text = "Clear";
            this.hotkeyClearButton.UseVisualStyleBackColor = true;
            this.hotkeyClearButton.Click += new System.EventHandler(this.hotkeyClearButton_Click);
            // 
            // hotkeySetButton
            // 
            this.hotkeySetButton.Location = new System.Drawing.Point(10, 71);
            this.hotkeySetButton.Name = "hotkeySetButton";
            this.hotkeySetButton.Size = new System.Drawing.Size(53, 23);
            this.hotkeySetButton.TabIndex = 3;
            this.hotkeySetButton.Text = "Set";
            this.hotkeySetButton.UseVisualStyleBackColor = true;
            this.hotkeySetButton.Click += new System.EventHandler(this.hotkeySetButton_Click);
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(7, 20);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(29, 14);
            this.label39.TabIndex = 2;
            this.label39.Text = "Key:";
            // 
            // hotkeytextbox
            // 
            this.hotkeytextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hotkeytextbox.BackColor = System.Drawing.Color.White;
            this.hotkeytextbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hotkeytextbox.Location = new System.Drawing.Point(41, 17);
            this.hotkeytextbox.Name = "hotkeytextbox";
            this.hotkeytextbox.ReadOnly = true;
            this.hotkeytextbox.Size = new System.Drawing.Size(104, 20);
            this.hotkeytextbox.TabIndex = 1;
            this.hotkeytextbox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HotKey_MouseDown);
            this.hotkeytextbox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.HotKey_MouseRoll);
            // 
            // hotkeytreeView
            // 
            this.hotkeytreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hotkeytreeView.Location = new System.Drawing.Point(10, 7);
            this.hotkeytreeView.Name = "hotkeytreeView";
            this.hotkeytreeView.Size = new System.Drawing.Size(502, 358);
            this.hotkeytreeView.TabIndex = 0;
            this.hotkeytreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.hotkeytreeView_AfterSelect);
            #endregion
            #region Technical Tab
            // 
            // technicalTab
            // 
            this.technicalTab.Controls.Add(this.TechnicalPages);
            //this.technicalTab.Controls.Add(this.statusTab);
            this.technicalTab.Location = new System.Drawing.Point(4, 54);
            this.technicalTab.Name = "technicalTab";
            this.technicalTab.Size = new System.Drawing.Size(678, 365);
            this.technicalTab.TabIndex = 0;
            this.technicalTab.Text = "Technical";
            // 
            // TechnicalPages
            // 
            this.TechnicalPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TechnicalPages.Controls.Add(this.generalTab);
            this.TechnicalPages.Controls.Add(this.statusTab);
            this.TechnicalPages.Location = new System.Drawing.Point(-2, 2);
            this.TechnicalPages.Name = "TechnicalPages";
            this.TechnicalPages.SelectedIndex = 0;
            this.TechnicalPages.Size = new System.Drawing.Size(657, 371);
            this.TechnicalPages.TabIndex = 0;
            #region Razor Settings Tab
            // 
            // generalTab
            // 
            this.generalTab.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.generalTab.Controls.Add(this.label79);
            this.generalTab.Controls.Add(this.openchangelogButton);
            this.generalTab.Controls.Add(this.showlauncher);
            this.generalTab.Controls.Add(this.groupBox29);
            this.generalTab.Controls.Add(this.forceSizeY);
            this.generalTab.Controls.Add(this.forceSizeX);
            this.generalTab.Controls.Add(this.gameSize);
            this.generalTab.Controls.Add(this.rememberPwds);
            this.generalTab.Controls.Add(this.clientPrio);
            this.generalTab.Controls.Add(this.systray);
            this.generalTab.Controls.Add(this.taskbar);
            this.generalTab.Controls.Add(this.smartCPU);
            this.generalTab.Controls.Add(this.label11);
            this.generalTab.Controls.Add(this.opacityGroupBox);
            this.generalTab.Controls.Add(this.alwaysTop);
            //this.generalTab.Controls.Add(this.groupBox1);
            //this.generalTab.Controls.Add(this.opacityLabel);
            this.generalTab.Controls.Add(this.label9);
            this.generalTab.Controls.Add(this.remoteControl);
            this.generalTab.Location = new System.Drawing.Point(4, 22);
            this.generalTab.Name = "generalTab";
            this.generalTab.Size = new System.Drawing.Size(678, 345);
            this.generalTab.TabIndex = 0;
            this.generalTab.Text = "TM Razor Settings";
            // 
            // label79
            // 
            this.label79.Location = new System.Drawing.Point(429, 49);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(12, 18);
            this.label79.TabIndex = 70;
            this.label79.Text = "X";
            this.label79.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // openchangelogButton
            // 
            this.openchangelogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openchangelogButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openchangelogButton.Location = new System.Drawing.Point(496, 9);
            this.openchangelogButton.Name = "openchangelogButton";
            this.openchangelogButton.Size = new System.Drawing.Size(176, 22);
            this.openchangelogButton.TabIndex = 68;
            this.openchangelogButton.Text = "Changelog";
            this.openchangelogButton.UseVisualStyleBackColor = true;
            this.openchangelogButton.Click += new System.EventHandler(this.openchangelogButton_Click);
            // 
            // showlauncher
            // 
            this.showlauncher.Location = new System.Drawing.Point(6, 74);
            this.showlauncher.Name = "showlauncher";
            this.showlauncher.Size = new System.Drawing.Size(148, 22);
            this.showlauncher.TabIndex = 67;
            this.showlauncher.Text = "Show Launcher Window";
            this.showlauncher.CheckedChanged += new System.EventHandler(this.showlauncher_CheckedChanged);
            // 
            // groupBox29
            // 
            this.groupBox29.Controls.Add(this.profilesCloneButton);
            this.groupBox29.Controls.Add(this.profilesRenameButton);
            this.groupBox29.Controls.Add(this.profilesUnlinkButton);
            this.groupBox29.Controls.Add(this.profilesLinkButton);
            this.groupBox29.Controls.Add(this.profilelinklabel);
            this.groupBox29.Controls.Add(this.profilesDeleteButton);
            this.groupBox29.Controls.Add(this.profilesAddButton);
            this.groupBox29.Controls.Add(this.profilesComboBox);
            this.groupBox29.Location = new System.Drawing.Point(6, 125);
            this.groupBox29.Name = "groupBox29";
            this.groupBox29.Size = new System.Drawing.Size(390, 98);
            this.groupBox29.TabIndex = 66;
            this.groupBox29.TabStop = false;
            this.groupBox29.Text = "Profiles";
            // 
            // profilesCloneButton
            // 
            this.profilesCloneButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profilesCloneButton.Location = new System.Drawing.Point(321, 44);
            this.profilesCloneButton.Name = "profilesCloneButton";
            this.profilesCloneButton.Size = new System.Drawing.Size(63, 21);
            this.profilesCloneButton.TabIndex = 9;
            this.profilesCloneButton.Text = "Clone";
            this.profilesCloneButton.Click += new System.EventHandler(this.profilesCloneButton_Click);
            // 
            // profilesRenameButton
            // 
            this.profilesRenameButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profilesRenameButton.Location = new System.Drawing.Point(252, 44);
            this.profilesRenameButton.Name = "profilesRenameButton";
            this.profilesRenameButton.Size = new System.Drawing.Size(63, 21);
            this.profilesRenameButton.TabIndex = 8;
            this.profilesRenameButton.Text = "Rename";
            this.profilesRenameButton.Click += new System.EventHandler(this.profilesRenameButton_Click);
            // 
            // profilesUnlinkButton
            // 
            this.profilesUnlinkButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profilesUnlinkButton.Location = new System.Drawing.Point(75, 70);
            this.profilesUnlinkButton.Name = "profilesUnlinkButton";
            this.profilesUnlinkButton.Size = new System.Drawing.Size(63, 21);
            this.profilesUnlinkButton.TabIndex = 7;
            this.profilesUnlinkButton.Text = "UnLink";
            this.profilesUnlinkButton.Click += new System.EventHandler(this.profilesUnlinkButton_Click);
            // 
            // profilesLinkButton
            // 
            this.profilesLinkButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profilesLinkButton.Location = new System.Drawing.Point(6, 70);
            this.profilesLinkButton.Name = "profilesLinkButton";
            this.profilesLinkButton.Size = new System.Drawing.Size(63, 21);
            this.profilesLinkButton.TabIndex = 6;
            this.profilesLinkButton.Text = "Link";
            this.profilesLinkButton.Click += new System.EventHandler(this.profilesLinkButton_Click);
            // 
            // profilelinklabel
            // 
            this.profilelinklabel.AutoSize = true;
            this.profilelinklabel.Location = new System.Drawing.Point(7, 50);
            this.profilelinklabel.Name = "profilelinklabel";
            this.profilelinklabel.Size = new System.Drawing.Size(81, 14);
            this.profilelinklabel.TabIndex = 5;
            this.profilelinklabel.Text = "Linked to: None";
            // 
            // profilesDeleteButton
            // 
            this.profilesDeleteButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profilesDeleteButton.Location = new System.Drawing.Point(321, 18);
            this.profilesDeleteButton.Name = "profilesDeleteButton";
            this.profilesDeleteButton.Size = new System.Drawing.Size(63, 21);
            this.profilesDeleteButton.TabIndex = 4;
            this.profilesDeleteButton.Text = "Delete";
            this.profilesDeleteButton.Click += new System.EventHandler(this.profilesDeleteButton_Click);
            // 
            // profilesAddButton
            // 
            this.profilesAddButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.profilesAddButton.Location = new System.Drawing.Point(252, 18);
            this.profilesAddButton.Name = "profilesAddButton";
            this.profilesAddButton.Size = new System.Drawing.Size(63, 21);
            this.profilesAddButton.TabIndex = 3;
            this.profilesAddButton.Text = "Add";
            this.profilesAddButton.Click += new System.EventHandler(this.profilesAddButton_Click);
            // 
            // profilesComboBox
            // 
            this.profilesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.profilesComboBox.FormattingEnabled = true;
            this.profilesComboBox.Location = new System.Drawing.Point(6, 19);
            this.profilesComboBox.Name = "profilesComboBox";
            this.profilesComboBox.Size = new System.Drawing.Size(240, 22);
            this.profilesComboBox.TabIndex = 0;
            this.profilesComboBox.SelectedIndexChanged += new System.EventHandler(this.profilesComboBox_SelectedIndexChanged);
            // 
            // forceSizeY
            // 
            this.forceSizeY.BackColor = System.Drawing.Color.White;
            this.forceSizeY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.forceSizeY.Location = new System.Drawing.Point(444, 49);
            this.forceSizeY.Name = "forceSizeY";
            this.forceSizeY.Size = new System.Drawing.Size(50, 20);
            this.forceSizeY.TabIndex = 64;
            this.forceSizeY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.forceSizeY.TextChanged += new System.EventHandler(this.forceSizeY_TextChanged);
            // 
            // forceSizeX
            // 
            this.forceSizeX.BackColor = System.Drawing.Color.White;
            this.forceSizeX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.forceSizeX.Location = new System.Drawing.Point(375, 49);
            this.forceSizeX.Name = "forceSizeX";
            this.forceSizeX.Size = new System.Drawing.Size(50, 20);
            this.forceSizeX.TabIndex = 63;
            this.forceSizeX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.forceSizeX.TextChanged += new System.EventHandler(this.forceSizeX_TextChanged);
            // 
            // gameSize
            // 
            this.gameSize.Location = new System.Drawing.Point(253, 49);
            this.gameSize.Name = "gameSize";
            this.gameSize.Size = new System.Drawing.Size(114, 22);
            this.gameSize.TabIndex = 65;
            this.gameSize.Text = "Force Game Size:";
            this.gameSize.CheckedChanged += new System.EventHandler(this.gameSize_CheckedChanged);
            // 
            // rememberPwds
            // 
            this.rememberPwds.Location = new System.Drawing.Point(6, 49);
            this.rememberPwds.Name = "rememberPwds";
            this.rememberPwds.Size = new System.Drawing.Size(190, 22);
            this.rememberPwds.TabIndex = 54;
            this.rememberPwds.Text = "Remember passwords ";
            this.rememberPwds.CheckedChanged += new System.EventHandler(this.rememberPwds_CheckedChanged);
            // 
            // clientPrio
            // 
            this.clientPrio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clientPrio.Font = new System.Drawing.Font("Arial", 9F);
            this.clientPrio.Items.AddRange(new object[] {
            "Idle",
            "BelowNormal",
            "Normal",
            "AboveNormal",
            "High",
            "Realtime"});
            this.clientPrio.Location = new System.Drawing.Point(118, 96); //363, 186
            this.clientPrio.Name = "clientPrio";
            this.clientPrio.Size = new System.Drawing.Size(88, 23);
            this.clientPrio.TabIndex = 60;
            this.clientPrio.SelectedIndexChanged += new System.EventHandler(this.clientPrio_SelectedIndexChanged);
            // 
            // systray
            // 
            this.systray.Location = new System.Drawing.Point(365, 74);
            this.systray.Name = "systray";
            this.systray.Size = new System.Drawing.Size(99, 20);
            this.systray.TabIndex = 35;
            this.systray.Text = "System Tray";
            this.systray.CheckedChanged += new System.EventHandler(this.systray_CheckedChanged);
            // 
            // taskbar
            // 
            this.taskbar.Location = new System.Drawing.Point(301, 74);
            this.taskbar.Name = "taskbar";
            this.taskbar.Size = new System.Drawing.Size(66, 20);
            this.taskbar.TabIndex = 34;
            this.taskbar.Text = "Taskbar";
            this.taskbar.CheckedChanged += new System.EventHandler(this.taskbar_CheckedChanged);
            // 
            // smartCPU
            // 
            this.smartCPU.Location = new System.Drawing.Point(6, 24);
            this.smartCPU.Name = "smartCPU";
            this.smartCPU.Size = new System.Drawing.Size(241, 22);
            this.smartCPU.TabIndex = 53;
            this.smartCPU.Text = "Use smart CPU usage reduction";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(251, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(50, 15);
            this.label11.TabIndex = 33;
            this.label11.Text = "Show in:";
            // 
            // opacityGroupBox
            // 
            this.opacityGroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.opacityGroupBox.AutoSize = false;
            this.opacityGroupBox.Controls.Add(this.opacity);
            this.opacityGroupBox.Controls.Add(this.opacityLabel);
            this.opacityGroupBox.Location = new System.Drawing.Point(6, 225);
            this.opacityGroupBox.Name = "opacityGroupBox";
            this.opacityGroupBox.Size = new System.Drawing.Size(650, 50);
            this.opacityGroupBox.TabIndex = 66;
            this.opacityGroupBox.TabStop = false;
            // 
            // opacity
            // 
            this.opacityGroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.opacity.AutoSize = false;
            this.opacity.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.opacity.Location = new System.Drawing.Point(4, 25);
            this.opacity.Maximum = 100;
            this.opacity.Minimum = 10;
            this.opacity.Name = "opacity";
            this.opacity.Size = new System.Drawing.Size(642, 16);
            this.opacity.TabIndex = 22;
            this.opacity.TickFrequency = 0;
            this.opacity.TickStyle = System.Windows.Forms.TickStyle.None;
            this.opacity.Value = 100;
            this.opacity.Scroll += new System.EventHandler(this.opacity_Scroll);
            // 
            // alwaysTop
            // 
            this.alwaysTop.Location = new System.Drawing.Point(253, 24);
            this.alwaysTop.Name = "alwaysTop";
            this.alwaysTop.Size = new System.Drawing.Size(241, 22);
            this.alwaysTop.TabIndex = 3;
            this.alwaysTop.Text = "Use Smart Always on Top";
            this.alwaysTop.CheckedChanged += new System.EventHandler(this.alwaysTop_CheckedChanged);
            // 
            // opacityLabel
            // 
            this.opacityLabel.Location = new System.Drawing.Point(2, 0);
            this.opacityLabel.Name = "opacityLabel";
            this.opacityLabel.Size = new System.Drawing.Size(78, 16);
            this.opacityLabel.TabIndex = 23;
            this.opacityLabel.Text = "Opacity: 100%";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(6, 99); //251 189
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(114, 19);
            this.label9.TabIndex = 59;
            this.label9.Text = "Default Client Priority:";
            // 
            // remoteControl
            // 
            this.remoteControl.AutoSize = true;
            this.remoteControl.Location = new System.Drawing.Point(253, 99);
            this.remoteControl.Name = "remoteControl";
            this.remoteControl.Size = new System.Drawing.Size(134, 18);
            this.remoteControl.TabIndex = 84;
            this.remoteControl.Text = "Enable Remote Control";
            this.remoteControl.UseVisualStyleBackColor = true;
            this.remoteControl.CheckedChanged += new System.EventHandler(this.remoteControl_CheckedChanged);
            #endregion
            #region Help Status Tab
            // 
            // statusTab
            // 
            this.statusTab.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.statusTab.Controls.Add(this.ChkForUpdate);
            this.statusTab.Controls.Add(this.advertisementLink);
            this.statusTab.Controls.Add(this.advertisementDiscordLink);
            this.statusTab.Controls.Add(this.discordrazorButton);
            this.statusTab.Controls.Add(this.razorButtonWiki);
            this.statusTab.Controls.Add(this.razorButtonSource);
            this.statusTab.Controls.Add(this.razorButtonWebsite);
            this.statusTab.Controls.Add(this.labelStatus);
            this.statusTab.Controls.Add(this.advertisement);
            this.statusTab.Location = new System.Drawing.Point(4, 54);
            this.statusTab.Name = "statusTab";
            this.statusTab.Size = new System.Drawing.Size(678, 365);
            this.statusTab.TabIndex = 9;
            this.statusTab.Text = "Help / Status";
            // 
            // ChkForUpdate
            // 
            this.ChkForUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkForUpdate.Location = new System.Drawing.Point(496, 9);
            this.ChkForUpdate.Name = "ChkForUpdate";
            this.ChkForUpdate.Size = new System.Drawing.Size(176, 22);
            this.ChkForUpdate.TabIndex = 13;
            this.ChkForUpdate.Text = "Check For Updates";
            this.ChkForUpdate.UseVisualStyleBackColor = true;
            this.ChkForUpdate.Click += new System.EventHandler(this.chkForUpdate_Click);
            // 
            // advertisementLink
            // 
            this.advertisementLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.advertisementLink.Location = new System.Drawing.Point(187, 350);
            this.advertisementLink.Name = "advertisementLink";
            this.advertisementLink.Size = new System.Drawing.Size(176, 22);
            this.advertisementLink.TabIndex = 12;
            this.advertisementLink.Text = "UO Eventine";
            this.advertisementLink.UseVisualStyleBackColor = true;
            this.advertisementLink.Click += new System.EventHandler(this.advertisement_Click);
            // 
            // advertisementDiscordLink
            // 
            this.advertisementDiscordLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.advertisementDiscordLink.Location = new System.Drawing.Point(368, 350);
            this.advertisementDiscordLink.Name = "advertisementDiscordLink";
            this.advertisementDiscordLink.Size = new System.Drawing.Size(176, 22);
            this.advertisementDiscordLink.TabIndex = 12;
            this.advertisementDiscordLink.Text = "UO Eventine Discord";
            this.advertisementDiscordLink.UseVisualStyleBackColor = true;
            this.advertisementDiscordLink.Click += new System.EventHandler(this.advertisementDiscord_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelStatus.Location = new System.Drawing.Point(496, 32);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(176, 325);
            this.labelStatus.TabIndex = 1;
            // 
            // discordrazorButton
            // 
            this.discordrazorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.discordrazorButton.Location = new System.Drawing.Point(368, 325);
            this.discordrazorButton.Name = "discordrazorButton";
            this.discordrazorButton.Size = new System.Drawing.Size(176, 22);
            this.discordrazorButton.TabIndex = 12;
            this.discordrazorButton.Text = "TM Razor Discord";
            this.discordrazorButton.UseVisualStyleBackColor = true;
            this.discordrazorButton.Click += new System.EventHandler(this.discordrazorButton_Click);
            // 
            // razorButtonWiki
            // 
            this.razorButtonWiki.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.razorButtonWiki.Location = new System.Drawing.Point(187, 325);
            this.razorButtonWiki.Name = "razorButtonWiki";
            this.razorButtonWiki.Size = new System.Drawing.Size(176, 22);
            this.razorButtonWiki.TabIndex = 12;
            this.razorButtonWiki.Text = "TM Razor wiki";
            this.razorButtonWiki.UseVisualStyleBackColor = true;
            this.razorButtonWiki.Click += new System.EventHandler(this.razorButtonWiki_Click);
            // 
            // razorButtonSource
            // 
            this.razorButtonSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.razorButtonSource.Location = new System.Drawing.Point(6, 350);
            this.razorButtonSource.Name = "razorButtonSource";
            this.razorButtonSource.Size = new System.Drawing.Size(176, 22);
            this.razorButtonSource.TabIndex = 12;
            this.razorButtonSource.Text = "TM Razor Source";
            this.razorButtonSource.UseVisualStyleBackColor = true;
            this.razorButtonSource.Click += new System.EventHandler(this.razorButtonSource_Click);
            // 
            // razorButtonWebsite
            // 
            this.razorButtonWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.razorButtonWebsite.Location = new System.Drawing.Point(6, 325);
            this.razorButtonWebsite.Name = "razorButtonWebsite";
            this.razorButtonWebsite.Size = new System.Drawing.Size(176, 22);
            this.razorButtonWebsite.TabIndex = 12;
            this.razorButtonWebsite.Text = "TM Razor Website";
            this.razorButtonWebsite.UseVisualStyleBackColor = true;
            this.razorButtonWebsite.Click += new System.EventHandler(this.razorButtonWebsite_Click);
            // 
            // advertisement
            // 
            this.advertisement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.advertisement.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.advertisement.Image = ((System.Drawing.Image)(resources.GetObject("advertisement.Image")));
            this.advertisement.InitialImage = ((System.Drawing.Image)(resources.GetObject("advertisement.InitialImage")));
            this.advertisement.Location = new System.Drawing.Point(3, 3);
            this.advertisement.Name = "advertisement";
            this.advertisement.Size = new System.Drawing.Size(487, 255);
            this.advertisement.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.advertisement.TabIndex = 11;
            this.advertisement.TabStop = false;
            #endregion
            #endregion
            #region Advanced Tab
            // 
            // advancedTab   
            // 
            this.advancedTab.Controls.Add(this.AdvancedPages);
            this.advancedTab.Location = new System.Drawing.Point(4, 54);
            this.advancedTab.Name = "advancedTab";
            this.advancedTab.Size = new System.Drawing.Size(678, 365);
            this.advancedTab.TabIndex = 0;
            this.advancedTab.Text = "Advanced";
            // 
            // AdvancedPages
            // 
            this.AdvancedPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AdvancedPages.Controls.Add(this.screenshotTab);
            this.AdvancedPages.Controls.Add(this.videoTab);
            this.AdvancedPages.Controls.Add(this.DPStabPage);
            this.AdvancedPages.Location = new System.Drawing.Point(-2, 2);
            this.AdvancedPages.Name = "AdvancedPages";
            this.AdvancedPages.SelectedIndex = 0;
            this.AdvancedPages.Size = new System.Drawing.Size(657, 371);
            this.AdvancedPages.TabIndex = 0;
            #region Screen Shot Tab
            // 
            // screenshotTab
            // 
            this.screenshotTab.Controls.Add(this.imgFmt);
            this.screenshotTab.Controls.Add(this.label12);
            this.screenshotTab.Controls.Add(this.capNow);
            this.screenshotTab.Controls.Add(this.screenPath);
            this.screenshotTab.Controls.Add(this.radioUO);
            this.screenshotTab.Controls.Add(this.radioFull);
            this.screenshotTab.Controls.Add(this.screenAutoCap);
            this.screenshotTab.Controls.Add(this.setScnPath);
            this.screenshotTab.Controls.Add(this.screensList);
            this.screenshotTab.Controls.Add(this.screenPrev);
            this.screenshotTab.Controls.Add(this.dispTime);
            this.screenshotTab.Location = new System.Drawing.Point(4, 54);
            this.screenshotTab.Name = "screenshotTab";
            this.screenshotTab.Size = new System.Drawing.Size(678, 365);
            this.screenshotTab.TabIndex = 8;
            this.screenshotTab.Text = "Screen Shots";
            // 
            // imgFmt
            // 
            this.imgFmt.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imgFmt.Font = new System.Drawing.Font("Arial", 9F);
            this.imgFmt.Items.AddRange(new object[] {
            "jpg",
            "png",
            "bmp",
            "gif",
            "tif",
            "wmf",
            "exif",
            "emf"});
            this.imgFmt.Location = new System.Drawing.Point(94, 202);
            this.imgFmt.Name = "imgFmt";
            this.imgFmt.Size = new System.Drawing.Size(71, 23);
            this.imgFmt.TabIndex = 11;
            this.imgFmt.SelectedIndexChanged += new System.EventHandler(this.imgFmt_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(8, 205);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(80, 20);
            this.label12.TabIndex = 10;
            this.label12.Text = "Image Format:";
            // 
            // capNow
            // 
            this.capNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capNow.Location = new System.Drawing.Point(333, 11);
            this.capNow.Name = "capNow";
            this.capNow.Size = new System.Drawing.Size(285, 20);
            this.capNow.TabIndex = 8;
            this.capNow.Text = "Take Screen Shot Now";
            this.capNow.Click += new System.EventHandler(this.capNow_Click);
            // 
            // screenPath
            // 
            this.screenPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.screenPath.BackColor = System.Drawing.Color.White;
            this.screenPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.screenPath.Location = new System.Drawing.Point(7, 14);
            this.screenPath.Name = "screenPath";
            this.screenPath.Size = new System.Drawing.Size(265, 20);
            this.screenPath.TabIndex = 7;
            this.screenPath.TextChanged += new System.EventHandler(this.screenPath_TextChanged);
            // 
            // radioUO
            // 
            this.radioUO.Location = new System.Drawing.Point(11, 228);
            this.radioUO.Name = "radioUO";
            this.radioUO.Size = new System.Drawing.Size(87, 20);
            this.radioUO.TabIndex = 6;
            this.radioUO.Text = "UO Only";
            this.radioUO.CheckedChanged += new System.EventHandler(this.radioUO_CheckedChanged);
            // 
            // radioFull
            // 
            this.radioFull.Location = new System.Drawing.Point(102, 228);
            this.radioFull.Name = "radioFull";
            this.radioFull.Size = new System.Drawing.Size(89, 20);
            this.radioFull.TabIndex = 5;
            this.radioFull.Text = "Full Screen";
            this.radioFull.CheckedChanged += new System.EventHandler(this.radioFull_CheckedChanged);
            // 
            // screenAutoCap
            // 
            this.screenAutoCap.Location = new System.Drawing.Point(11, 284);
            this.screenAutoCap.Name = "screenAutoCap";
            this.screenAutoCap.Size = new System.Drawing.Size(180, 22);
            this.screenAutoCap.TabIndex = 4;
            this.screenAutoCap.Text = "Auto Death Screen Capture";
            this.screenAutoCap.CheckedChanged += new System.EventHandler(this.screenAutoCap_CheckedChanged);
            // 
            // setScnPath
            // 
            this.setScnPath.Location = new System.Drawing.Point(208, 16);
            this.setScnPath.Name = "setScnPath";
            this.setScnPath.Size = new System.Drawing.Size(22, 17);
            this.setScnPath.TabIndex = 3;
            this.setScnPath.Text = "...";
            this.setScnPath.Click += new System.EventHandler(this.setScnPath_Click);
            // 
            // screensList
            // 
            this.screensList.IntegralHeight = false;
            this.screensList.ItemHeight = 14;
            this.screensList.Location = new System.Drawing.Point(7, 40);
            this.screensList.Name = "screensList";
            this.screensList.Size = new System.Drawing.Size(223, 147);
            this.screensList.Sorted = true;
            this.screensList.TabIndex = 1;
            this.screensList.SelectedIndexChanged += new System.EventHandler(this.screensList_SelectedIndexChanged);
            this.screensList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.screensList_MouseDown);
            // 
            // screenPrev
            // 
            this.screenPrev.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.screenPrev.Location = new System.Drawing.Point(246, 36);
            this.screenPrev.Name = "screenPrev";
            this.screenPrev.Size = new System.Drawing.Size(412, 322);
            this.screenPrev.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.screenPrev.TabIndex = 0;
            this.screenPrev.TabStop = false;
            this.screenPrev.Click += new System.EventHandler(this.screenPrev_Click);
            // 
            // dispTime
            // 
            this.dispTime.Location = new System.Drawing.Point(11, 256);
            this.dispTime.Name = "dispTime";
            this.dispTime.Size = new System.Drawing.Size(180, 22);
            this.dispTime.TabIndex = 9;
            this.dispTime.Text = "Include Timestamp on images";
            this.dispTime.CheckedChanged += new System.EventHandler(this.dispTime_CheckedChanged);
            #endregion
            #region Video Recorder Tab
            // 
            // videoTab
            // 
            this.videoTab.BackColor = System.Drawing.SystemColors.Control;
            this.videoTab.Controls.Add(this.videoRecStatuslabel);
            this.videoTab.Controls.Add(this.label64);
            this.videoTab.Controls.Add(this.groupBox40);
            this.videoTab.Controls.Add(this.videosettinggroupBox);
            this.videoTab.Controls.Add(this.videorecbutton);
            this.videoTab.Controls.Add(this.videostopbutton);
            this.videoTab.Controls.Add(this.groupBox15);
            this.videoTab.Location = new System.Drawing.Point(4, 54);
            this.videoTab.Name = "videoTab";
            this.videoTab.Padding = new System.Windows.Forms.Padding(3);
            this.videoTab.Size = new System.Drawing.Size(678, 365);
            this.videoTab.TabIndex = 16;
            this.videoTab.Text = "Video Recorder";
            // 
            // videoRecStatuslabel
            // 
            this.videoRecStatuslabel.AutoSize = true;
            this.videoRecStatuslabel.ForeColor = System.Drawing.Color.Green;
            this.videoRecStatuslabel.Location = new System.Drawing.Point(185, 334);
            this.videoRecStatuslabel.Name = "videoRecStatuslabel";
            this.videoRecStatuslabel.Size = new System.Drawing.Size(23, 14);
            this.videoRecStatuslabel.TabIndex = 95;
            this.videoRecStatuslabel.Text = "Idle";
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Location = new System.Drawing.Point(120, 334);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(63, 14);
            this.label64.TabIndex = 94;
            this.label64.Text = "Rec Status:";
            // 
            // groupBox40
            // 
            this.groupBox40.Controls.Add(this.videoSourcePlayer);
            this.groupBox40.Location = new System.Drawing.Point(259, 6);
            this.groupBox40.Name = "groupBox40";
            this.groupBox40.Size = new System.Drawing.Size(399, 352);
            this.groupBox40.TabIndex = 64;
            this.groupBox40.TabStop = false;
            this.groupBox40.Text = "Playback";
            // 
            // videoSourcePlayer
            // 
            this.videoSourcePlayer.Location = new System.Drawing.Point(7, 20);
            this.videoSourcePlayer.Name = "videoSourcePlayer";
            this.videoSourcePlayer.Size = new System.Drawing.Size(386, 298);
            this.videoSourcePlayer.TabIndex = 0;
            this.videoSourcePlayer.Text = "videoSourcePlayer";
            this.videoSourcePlayer.VideoSource = null;
            // 
            // videosettinggroupBox
            // 
            this.videosettinggroupBox.Controls.Add(this.videoCodecComboBox);
            this.videosettinggroupBox.Controls.Add(this.label63);
            this.videosettinggroupBox.Controls.Add(this.label62);
            this.videosettinggroupBox.Controls.Add(this.videoFPSTextBox);
            this.videosettinggroupBox.Location = new System.Drawing.Point(10, 250);
            this.videosettinggroupBox.Name = "videosettinggroupBox";
            this.videosettinggroupBox.Size = new System.Drawing.Size(243, 59);
            this.videosettinggroupBox.TabIndex = 63;
            this.videosettinggroupBox.TabStop = false;
            this.videosettinggroupBox.Text = "Video Settings";
            // 
            // videoCodecComboBox
            // 
            this.videoCodecComboBox.FormattingEnabled = true;
            this.videoCodecComboBox.Items.AddRange(new object[] {
            "Default",
            "MPEG4",
            "WMV1",
            "WMV2",
            "MSMPEG4v2",
            "MSMPEG4v3",
            "H263P",
            "FLV1",
            "MPEG2",
            "Raw",
            "FFV1",
            "FFVHUFF",
            "H264",
            "H265",
            "Theora",
            "VP8",
            "VP9"});
            this.videoCodecComboBox.Location = new System.Drawing.Point(122, 27);
            this.videoCodecComboBox.Name = "videoCodecComboBox";
            this.videoCodecComboBox.Size = new System.Drawing.Size(110, 22);
            this.videoCodecComboBox.TabIndex = 63;
            this.videoCodecComboBox.SelectedIndexChanged += new System.EventHandler(this.videoCodecComboBox_SelectedIndexChanged);
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Location = new System.Drawing.Point(81, 31);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(41, 14);
            this.label63.TabIndex = 62;
            this.label63.Text = "Codec:";
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Location = new System.Drawing.Point(7, 31);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(32, 14);
            this.label62.TabIndex = 61;
            this.label62.Text = "FPS: ";
            // 
            // videoFPSTextBox
            // 
            this.videoFPSTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoFPSTextBox.BackColor = System.Drawing.Color.White;
            this.videoFPSTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.videoFPSTextBox.Location = new System.Drawing.Point(42, 28);
            this.videoFPSTextBox.Name = "videoFPSTextBox";
            this.videoFPSTextBox.Size = new System.Drawing.Size(33, 20);
            this.videoFPSTextBox.TabIndex = 60;
            this.videoFPSTextBox.TextChanged += new System.EventHandler(this.videoFPSTextBox_TextChanged);
            // 
            // videorecbutton
            // 
            this.videorecbutton.BackgroundImage = global::Assistant.Properties.Resources.record;
            this.videorecbutton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.videorecbutton.FlatAppearance.BorderSize = 0;
            this.videorecbutton.Location = new System.Drawing.Point(43, 325);
            this.videorecbutton.Name = "videorecbutton";
            this.videorecbutton.Size = new System.Drawing.Size(30, 30);
            this.videorecbutton.TabIndex = 93;
            this.videorecbutton.UseVisualStyleBackColor = true;
            this.videorecbutton.Click += new System.EventHandler(this.videorecbutton_Click);
            // 
            // videostopbutton
            // 
            this.videostopbutton.BackgroundImage = global::Assistant.Properties.Resources.stopagent;
            this.videostopbutton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.videostopbutton.FlatAppearance.BorderSize = 0;
            this.videostopbutton.Location = new System.Drawing.Point(79, 325);
            this.videostopbutton.Name = "videostopbutton";
            this.videostopbutton.Size = new System.Drawing.Size(30, 30);
            this.videostopbutton.TabIndex = 92;
            this.videostopbutton.UseVisualStyleBackColor = true;
            this.videostopbutton.Click += new System.EventHandler(this.videostopbutton_Click);
            // 
            // groupBox15
            // 
            this.groupBox15.Controls.Add(this.videolistBox);
            this.groupBox15.Controls.Add(this.videoPathButton);
            this.groupBox15.Controls.Add(this.videoPathTextBox);
            this.groupBox15.Location = new System.Drawing.Point(8, 6);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(245, 238);
            this.groupBox15.TabIndex = 62;
            this.groupBox15.TabStop = false;
            this.groupBox15.Text = "File";
            // 
            // videolistBox
            // 
            this.videolistBox.IntegralHeight = false;
            this.videolistBox.ItemHeight = 14;
            this.videolistBox.Location = new System.Drawing.Point(11, 41);
            this.videolistBox.Name = "videolistBox";
            this.videolistBox.Size = new System.Drawing.Size(223, 183);
            this.videolistBox.Sorted = true;
            this.videolistBox.TabIndex = 8;
            this.videolistBox.SelectedIndexChanged += new System.EventHandler(this.videoList_SelectedIndexChanged);
            this.videolistBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.videoList_MouseDown);
            // 
            // videoPathButton
            // 
            this.videoPathButton.Location = new System.Drawing.Point(212, 17);
            this.videoPathButton.Name = "videoPathButton";
            this.videoPathButton.Size = new System.Drawing.Size(22, 17);
            this.videoPathButton.TabIndex = 9;
            this.videoPathButton.Text = "...";
            this.videoPathButton.Click += new System.EventHandler(this.videoPathButton_Click);
            // 
            // videoPathTextBox
            // 
            this.videoPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPathTextBox.BackColor = System.Drawing.Color.White;
            this.videoPathTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.videoPathTextBox.Location = new System.Drawing.Point(11, 15);
            this.videoPathTextBox.Name = "videoPathTextBox";
            this.videoPathTextBox.Size = new System.Drawing.Size(195, 20);
            this.videoPathTextBox.TabIndex = 10;
            #endregion
            #region DPS Meter Tab
            // 
            // DPStabPage
            // 
            this.DPStabPage.Controls.Add(this.filtergroup);
            this.DPStabPage.Controls.Add(this.DpsMeterGridView);
            this.DPStabPage.Controls.Add(this.DPSMeterStatusLabel);
            this.DPStabPage.Controls.Add(this.label67);
            this.DPStabPage.Controls.Add(this.DPSMeterPauseButton);
            this.DPStabPage.Controls.Add(this.DPSMeterStopButton);
            this.DPStabPage.Controls.Add(this.DPSMeterStartButton);
            this.DPStabPage.Controls.Add(this.DPSMeterClearButton);
            this.DPStabPage.Location = new System.Drawing.Point(4, 54);
            this.DPStabPage.Name = "DPStabPage";
            this.DPStabPage.Padding = new System.Windows.Forms.Padding(3);
            this.DPStabPage.Size = new System.Drawing.Size(678, 365);
            this.DPStabPage.TabIndex = 17;
            this.DPStabPage.Text = "DPS Meter";
            this.DPStabPage.UseVisualStyleBackColor = true;
            // 
            // filtergroup
            // 
            this.filtergroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.filtergroup.Controls.Add(this.DPSMeterClearFilterButton);
            this.filtergroup.Controls.Add(this.DPSMeterApplyFilterButton);
            this.filtergroup.Controls.Add(this.DPSmetername);
            this.filtergroup.Controls.Add(this.label70);
            this.filtergroup.Controls.Add(this.DPSmeterserial);
            this.filtergroup.Controls.Add(this.label69);
            this.filtergroup.Controls.Add(this.label68);
            this.filtergroup.Controls.Add(this.DPSmetermaxdamage);
            this.filtergroup.Controls.Add(this.label66);
            this.filtergroup.Controls.Add(this.DPSmetermindamage);
            this.filtergroup.Location = new System.Drawing.Point(385, 53);
            this.filtergroup.Name = "filtergroup";
            this.filtergroup.Size = new System.Drawing.Size(287, 167);
            this.filtergroup.TabIndex = 66;
            this.filtergroup.TabStop = false;
            this.filtergroup.Text = "Filter";
            // 
            // DPSMeterClearFilterButton
            // 
            this.DPSMeterClearFilterButton.Location = new System.Drawing.Point(144, 129);
            this.DPSMeterClearFilterButton.Name = "DPSMeterClearFilterButton";
            this.DPSMeterClearFilterButton.Size = new System.Drawing.Size(63, 21);
            this.DPSMeterClearFilterButton.TabIndex = 68;
            this.DPSMeterClearFilterButton.Text = "Clear";
            this.DPSMeterClearFilterButton.Click += new System.EventHandler(this.DPSMeterClearFilterButton_Click);
            // 
            // DPSMeterApplyFilterButton
            // 
            this.DPSMeterApplyFilterButton.Location = new System.Drawing.Point(213, 129);
            this.DPSMeterApplyFilterButton.Name = "DPSMeterApplyFilterButton";
            this.DPSMeterApplyFilterButton.Size = new System.Drawing.Size(63, 21);
            this.DPSMeterApplyFilterButton.TabIndex = 67;
            this.DPSMeterApplyFilterButton.Text = "Apply";
            this.DPSMeterApplyFilterButton.Click += new System.EventHandler(this.DPSMeterApplyFilterButton_Click);
            // 
            // DPSmetername
            // 
            this.DPSmetername.Location = new System.Drawing.Point(48, 93);
            this.DPSmetername.Name = "DPSmetername";
            this.DPSmetername.Size = new System.Drawing.Size(228, 20);
            this.DPSmetername.TabIndex = 8;
            // 
            // label70
            // 
            this.label70.AutoSize = true;
            this.label70.Location = new System.Drawing.Point(6, 96);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(37, 14);
            this.label70.TabIndex = 7;
            this.label70.Text = "Name:";
            // 
            // DPSmeterserial
            // 
            this.DPSmeterserial.Location = new System.Drawing.Point(48, 58);
            this.DPSmeterserial.Name = "DPSmeterserial";
            this.DPSmeterserial.Size = new System.Drawing.Size(100, 20);
            this.DPSmeterserial.TabIndex = 6;
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.Location = new System.Drawing.Point(6, 61);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(37, 14);
            this.label69.TabIndex = 5;
            this.label69.Text = "Serial:";
            // 
            // label68
            // 
            this.label68.AutoSize = true;
            this.label68.Location = new System.Drawing.Point(144, 26);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(72, 14);
            this.label68.TabIndex = 3;
            this.label68.Text = "Damage Max:";
            // 
            // DPSmetermaxdamage
            // 
            this.DPSmetermaxdamage.Location = new System.Drawing.Point(220, 23);
            this.DPSmetermaxdamage.Name = "DPSmetermaxdamage";
            this.DPSmetermaxdamage.Size = new System.Drawing.Size(56, 20);
            this.DPSmetermaxdamage.TabIndex = 2;
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Location = new System.Drawing.Point(6, 26);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(68, 14);
            this.label66.TabIndex = 1;
            this.label66.Text = "Damage Min:";
            // 
            // DPSmetermindamage
            // 
            this.DPSmetermindamage.Location = new System.Drawing.Point(82, 23);
            this.DPSmetermindamage.Name = "DPSmetermindamage";
            this.DPSmetermindamage.Size = new System.Drawing.Size(56, 20);
            this.DPSmetermindamage.TabIndex = 0;
            // 
            // DpsMeterGridView
            // 
            this.DpsMeterGridView.AllowDrop = true;
            this.DpsMeterGridView.AllowUserToResizeRows = false;
            this.DpsMeterGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DpsMeterGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DpsMeterGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn13,
            this.dataGridViewTextBoxColumn14,
            this.dataGridViewTextBoxColumn15});
            this.DpsMeterGridView.Location = new System.Drawing.Point(8, 6);
            this.DpsMeterGridView.Name = "DpsMeterGridView";
            this.DpsMeterGridView.RowHeadersVisible = false;
            this.DpsMeterGridView.RowHeadersWidth = 62;
            this.DpsMeterGridView.Size = new System.Drawing.Size(370, 370);
            this.DpsMeterGridView.TabIndex = 65;
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn13.FillWeight = 20F;
            this.dataGridViewTextBoxColumn13.Frozen = true;
            this.dataGridViewTextBoxColumn13.HeaderText = "Serial";
            this.dataGridViewTextBoxColumn13.MinimumWidth = 15;
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            this.dataGridViewTextBoxColumn13.ReadOnly = true;
            this.dataGridViewTextBoxColumn13.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn13.Width = 170;
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn14.FillWeight = 75F;
            this.dataGridViewTextBoxColumn14.HeaderText = "Name";
            this.dataGridViewTextBoxColumn14.MinimumWidth = 8;
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            this.dataGridViewTextBoxColumn14.ReadOnly = true;
            this.dataGridViewTextBoxColumn14.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn15.FillWeight = 32.83951F;
            this.dataGridViewTextBoxColumn15.HeaderText = "Damage";
            this.dataGridViewTextBoxColumn15.MinimumWidth = 15;
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            this.dataGridViewTextBoxColumn15.ReadOnly = true;
            this.dataGridViewTextBoxColumn15.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // DPSMeterStatusLabel
            // 
            this.DPSMeterStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DPSMeterStatusLabel.AutoSize = true;
            this.DPSMeterStatusLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DPSMeterStatusLabel.ForeColor = System.Drawing.Color.Green;
            this.DPSMeterStatusLabel.Location = new System.Drawing.Point(642, 331);
            this.DPSMeterStatusLabel.Name = "DPSMeterStatusLabel";
            this.DPSMeterStatusLabel.Size = new System.Drawing.Size(27, 14);
            this.DPSMeterStatusLabel.TabIndex = 64;
            this.DPSMeterStatusLabel.Text = "Idle";
            // 
            // label67
            // 
            this.label67.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label67.AutoSize = true;
            this.label67.Location = new System.Drawing.Point(602, 331);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(41, 14);
            this.label67.TabIndex = 63;
            this.label67.Text = "Status:";
            // 
            // DPSMeterPauseButton
            // 
            this.DPSMeterPauseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DPSMeterPauseButton.Location = new System.Drawing.Point(528, 17);
            this.DPSMeterPauseButton.Name = "DPSMeterPauseButton";
            this.DPSMeterPauseButton.Size = new System.Drawing.Size(64, 21);
            this.DPSMeterPauseButton.TabIndex = 61;
            this.DPSMeterPauseButton.Text = "Pause";
            this.DPSMeterPauseButton.Click += new System.EventHandler(this.DPSMeterPauseButton_Click);
            // 
            // DPSMeterStopButton
            // 
            this.DPSMeterStopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DPSMeterStopButton.Location = new System.Drawing.Point(460, 17);
            this.DPSMeterStopButton.Name = "DPSMeterStopButton";
            this.DPSMeterStopButton.Size = new System.Drawing.Size(62, 21);
            this.DPSMeterStopButton.TabIndex = 60;
            this.DPSMeterStopButton.Text = "Stop";
            this.DPSMeterStopButton.Click += new System.EventHandler(this.DPSMeterStopButton_Click);
            // 
            // DPSMeterStartButton
            // 
            this.DPSMeterStartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DPSMeterStartButton.Location = new System.Drawing.Point(390, 17);
            this.DPSMeterStartButton.Name = "DPSMeterStartButton";
            this.DPSMeterStartButton.Size = new System.Drawing.Size(63, 21);
            this.DPSMeterStartButton.TabIndex = 59;
            this.DPSMeterStartButton.Text = "Start";
            this.DPSMeterStartButton.Click += new System.EventHandler(this.DPSMeterStartButton_Click);
            // 
            // DPSMeterClearButton
            // 
            this.DPSMeterClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DPSMeterClearButton.Location = new System.Drawing.Point(597, 17);
            this.DPSMeterClearButton.Name = "DPSMeterClearButton";
            this.DPSMeterClearButton.Size = new System.Drawing.Size(63, 21);
            this.DPSMeterClearButton.TabIndex = 58;
            this.DPSMeterClearButton.Text = "Clear";
            this.DPSMeterClearButton.Click += new System.EventHandler(this.DPSMeterClearButton_Click);
            #endregion
            #endregion

            // 
            // label71
            // 
            this.label71.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label71.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label71.Location = new System.Drawing.Point(3, 345);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(650, 24);
            this.label71.TabIndex = 10;
            this.label71.Text = "Many thanks also for developer of UO.DLL and ULTIMA.DLL";
            // 
            // labelHotride
            // 
            this.labelHotride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelHotride.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHotride.Location = new System.Drawing.Point(3, 313);
            this.labelHotride.Name = "labelHotride";
            this.labelHotride.Size = new System.Drawing.Size(650, 45);
            this.labelHotride.TabIndex = 8;
            this.labelHotride.Text = "Many thanks to Hotride for his  FPS multiclient patch! Hotride is the author of O" +
    "penGL OrionUO Client project (you can point your browser to the link http://foru" +
    "m.orion-client.online for more info)";
            // 
            // m_NotifyIcon
            // 
            this.m_NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("m_NotifyIcon.Icon")));
            this.m_NotifyIcon.Text = "TM Razor";
            this.m_NotifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
            // 
            // openFileDialogscript
            // 
            this.openFileDialogscript.Filter = "Script Files|*.py;*.txt";
            this.openFileDialogscript.InitialDirectory = "C:\\Users\\credz\\AppData\\Local\\Microsoft\\VisualStudio\\16.0_1b6dbd30\\ProjectAssembli" +
    "es\\zro-nw7h01\\Scripts";
            this.openFileDialogscript.RestoreDirectory = true;
            // 
            // timerupdatestatus
            // 
            this.timerupdatestatus.Enabled = true;
            this.timerupdatestatus.Interval = 1000;
            this.timerupdatestatus.Tick += new System.EventHandler(this.timerupdatestatus_Tick);
            // 
            // scriptgridMenuStrip
            // 
            this.scriptgridMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.scriptgridMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modifyToolStripMenuItem,
            this.flagsToolStripMenuItem,
            this.playToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.scriptgridMenuStrip.Name = "scriptgridMenuStrip";
            this.scriptgridMenuStrip.Size = new System.Drawing.Size(113, 92);
            // 
            // modifyToolStripMenuItem
            // 
            this.modifyToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.openToolStripMenuItem,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveToToolStripMenuItem});
            this.modifyToolStripMenuItem.Name = "modifyToolStripMenuItem";
            this.modifyToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.modifyToolStripMenuItem.Text = "Modify";
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.moveUpToolStripMenuItem.Text = "Move Up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.moveDownToolStripMenuItem.Text = "Move Down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // moveToToolStripMenuItem
            // 
            this.moveToToolStripMenuItem.Name = "moveToToolStripMenuItem";
            this.moveToToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.moveToToolStripMenuItem.Text = "Move To";
            this.moveToToolStripMenuItem.Click += new System.EventHandler(this.moveToToolStripMenuItem_Click);
            // 
            // flagsToolStripMenuItem
            // 
            this.flagsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loopModeToolStripMenuItem,
            this.preloadToolStripMenuItem,
            this.waitBeforeInterruptToolStripMenuItem,
            this.autoStartAtLoginToolStripMenuItem});
            this.flagsToolStripMenuItem.Name = "flagsToolStripMenuItem";
            this.flagsToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.flagsToolStripMenuItem.Text = "Flags";
            // 
            // loopModeToolStripMenuItem
            // 
            this.loopModeToolStripMenuItem.Name = "loopModeToolStripMenuItem";
            this.loopModeToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.loopModeToolStripMenuItem.Text = "Loop Mode";
            this.loopModeToolStripMenuItem.Click += new System.EventHandler(this.loopModeToolStripMenuItem_Click);
            // 
            // preloadToolStripMenuItem
            // 
            this.preloadToolStripMenuItem.Name = "preloadToolStripMenuItem";
            this.preloadToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.preloadToolStripMenuItem.Text = "Preload";
            this.preloadToolStripMenuItem.Click += new System.EventHandler(this.preloadToolStripMenuItem_Click);
            // 
            // waitBeforeInterruptToolStripMenuItem
            // 
            this.waitBeforeInterruptToolStripMenuItem.Name = "waitBeforeInterruptToolStripMenuItem";
            this.waitBeforeInterruptToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.waitBeforeInterruptToolStripMenuItem.Text = "Wait Before Interrupt";
            this.waitBeforeInterruptToolStripMenuItem.Click += new System.EventHandler(this.waitBeforeInterruptToolStripMenuItem_Click);
            // 
            // autoStartAtLoginToolStripMenuItem
            // 
            this.autoStartAtLoginToolStripMenuItem.Name = "autoStartAtLoginToolStripMenuItem";
            this.autoStartAtLoginToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.autoStartAtLoginToolStripMenuItem.Text = "AutoStart at Login";
            this.autoStartAtLoginToolStripMenuItem.Click += new System.EventHandler(this.autoStartAtLoginToolStripMenuItem_Click);
            // 
            // playToolStripMenuItem
            // 
            this.playToolStripMenuItem.Name = "playToolStripMenuItem";
            this.playToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.playToolStripMenuItem.Text = "Play";
            this.playToolStripMenuItem.Click += new System.EventHandler(this.playToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // timertitlestatusbar
            // 
            this.timertitlestatusbar.Enabled = true;
            this.timertitlestatusbar.Interval = 200;
            this.timertitlestatusbar.Tick += new System.EventHandler(this.timertitlestatusbar_Tick);
            // 
            // openmaplocation
            // 
            this.openmaplocation.DefaultExt = "exe";
            this.openmaplocation.FileName = "EnhancedMap.exe";
            this.openmaplocation.Filter = "Executable Files|*.exe";
            this.openmaplocation.RestoreDirectory = true;
            this.openmaplocation.Title = "Select Enhanced Map";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(694, 431);
            this.Controls.Add(this.tabs);
            this.Font = new System.Drawing.Font("Arial", 8F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TM Razor {0}";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.LocationChanged += new System.EventHandler(this.MainForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.Move += new System.EventHandler(this.MainForm_Move);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.MacrosTab.ResumeLayout(false);
            this.MacrosTab.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.generalTab.ResumeLayout(false);
            this.generalTab.PerformLayout();
            this.technicalTab.ResumeLayout(false);
            this.technicalTab.PerformLayout();
            this.groupBox29.ResumeLayout(false);
            this.groupBox29.PerformLayout();
            this.opacityGroupBox.ResumeLayout(false);
            this.opacityGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.opacity)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.moreOptTab.ResumeLayout(false);
            this.moreOptTab.PerformLayout();
            this.groupBox17.ResumeLayout(false);
            this.groupBox17.PerformLayout();
            this.enhancedFilterTab.ResumeLayout(false);
            this.FilterPages.ResumeLayout(false);
            this.TechnicalPages.ResumeLayout(false);
            this.MiscFilterPage.ResumeLayout(false);
            this.DmgDsplyGroup.ResumeLayout(false);
            this.DmgDsplyGroup.PerformLayout();
            this.uomodgroupbox.ResumeLayout(false);
            this.groupBox32.ResumeLayout(false);
            this.groupBox32.PerformLayout();
            this.groupBox24.ResumeLayout(false);
            this.groupBox23.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.graphfilterdatagrid)).EndInit();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.overrideGroupBox.ResumeLayout(false);
            this.overrideGroupBox.PerformLayout();
            this.queueGroupBox.ResumeLayout(false);
            this.queueGroupBox.PerformLayout();
            this.showmobileGroupBox.ResumeLayout(false);
            this.showmobileGroupBox.PerformLayout();
            this.spellspotionsGroupBox.ResumeLayout(false);
            this.spellspotionsGroupBox.PerformLayout();
            this.preaosstatusGroupBox.ResumeLayout(false);
            this.preaosstatusGroupBox.PerformLayout();
            this.containeruseGroupBox.ResumeLayout(false);
            this.containeruseGroupBox.PerformLayout();
            this.razormessagesGroupBox.ResumeLayout(false);
            this.razormessagesGroupBox.PerformLayout();
            this.stealthGroupBox.ResumeLayout(false);
            this.stealthGroupBox.PerformLayout();
            this.miscellaneousGroupBox.ResumeLayout(false);
            this.miscellaneousGroupBox.PerformLayout();
            this.targetGroupBox.ResumeLayout(false);
            this.targetGroupBox.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.JournalFilterPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.journalfilterdatagrid)).EndInit();
            this.datagridMenuStrip.ResumeLayout(false);
            this.AllScripts.ResumeLayout(false);
            this.scriptControlBox.ResumeLayout(false);
            this.groupBox30.ResumeLayout(false);
            this.groupBox42.ResumeLayout(false);
            this.groupBox42.PerformLayout();
            this.scriptOperationsBox.ResumeLayout(false);
            this.scriptOperationsBox.PerformLayout();
            this.AllScriptsTab.ResumeLayout(false);
            this.pythonScriptingTab.ResumeLayout(false);
            this.uosScriptingTab.ResumeLayout(false);
            this.csScriptingTab.ResumeLayout(false);
            this.EnhancedAgent.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.eautoloot.ResumeLayout(false);
            this.eautoloot.PerformLayout();
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autolootdataGridView)).EndInit();
            this.groupBox13.ResumeLayout(false);
            this.escavenger.ResumeLayout(false);
            this.escavenger.PerformLayout();
            this.groupBox41.ResumeLayout(false);
            this.groupBox41.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scavengerdataGridView)).EndInit();
            this.groupBox12.ResumeLayout(false);
            this.organizer.ResumeLayout(false);
            this.organizer.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.organizerdataGridView)).EndInit();
            this.groupBox16.ResumeLayout(false);
            this.VendorBuy.ResumeLayout(false);
            this.VendorBuy.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vendorbuydataGridView)).EndInit();
            this.VendorSell.ResumeLayout(false);
            this.VendorSell.PerformLayout();
            this.groupBox19.ResumeLayout(false);
            this.groupBox19.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vendorsellGridView)).EndInit();
            this.groupBox20.ResumeLayout(false);
            this.Dress.ResumeLayout(false);
            this.Dress.PerformLayout();
            this.groupBox22.ResumeLayout(false);
            this.groupBox21.ResumeLayout(false);
            this.friends.ResumeLayout(false);
            this.friends.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox34.ResumeLayout(false);
            this.groupBox33.ResumeLayout(false);
            this.friendGroupBox.ResumeLayout(false);
            this.friendloggroupBox.ResumeLayout(false);
            this.restock.ResumeLayout(false);
            this.restock.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.restockdataGridView)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.bandageheal.ResumeLayout(false);
            this.BandageHealSettingsBox.ResumeLayout(false);
            this.BandageHealSettingsBox.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.toolbarTab.ResumeLayout(false);
            this.toolbarstab.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox39.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.toolbar_trackBar)).EndInit();
            this.groupBox25.ResumeLayout(false);
            this.groupBox25.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox26.ResumeLayout(false);
            this.groupBox26.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.groupBox38.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spellgrid_trackBar)).EndInit();
            this.groupBox37.ResumeLayout(false);
            this.groupBox37.PerformLayout();
            this.groupBox36.ResumeLayout(false);
            this.groupBox36.PerformLayout();
            this.groupBox35.ResumeLayout(false);
            this.groupBox35.PerformLayout();
            this.targettingTab.ResumeLayout(false);
            this.advancedTab.ResumeLayout(false);
            this.groupBox57.ResumeLayout(false);
            this.groupBox57.PerformLayout();
            this.groupBox56.ResumeLayout(false);
            this.groupBox55.ResumeLayout(false);
            this.groupBox55.PerformLayout();
            this.groupBox48.ResumeLayout(false);
            this.groupBox48.PerformLayout();
            this.groupBox46.ResumeLayout(false);
            this.groupBox47.ResumeLayout(false);
            this.groupBox49.ResumeLayout(false);
            this.groupBox50.ResumeLayout(false);
            this.groupBox51.ResumeLayout(false);
            this.groupBox52.ResumeLayout(false);
            this.groupBox53.ResumeLayout(false);
            this.groupBox54.ResumeLayout(false);
            this.groupBox45.ResumeLayout(false);
            this.groupBox45.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targethueGridView)).EndInit();
            this.groupBox44.ResumeLayout(false);
            this.groupBox44.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.targetbodydataGridView)).EndInit();
            this.groupBox43.ResumeLayout(false);
            this.groupBox43.PerformLayout();
            this.skillsTab.ResumeLayout(false);
            this.skillsTab.PerformLayout();
            this.enhancedHotKeytabPage.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox28.ResumeLayout(false);
            this.groupBox28.PerformLayout();
            this.groupBox27.ResumeLayout(false);
            this.groupBox27.PerformLayout();
            this.screenshotTab.ResumeLayout(false);
            this.screenshotTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.screenPrev)).EndInit();
            this.videoTab.ResumeLayout(false);
            this.videoTab.PerformLayout();
            this.groupBox40.ResumeLayout(false);
            this.videosettinggroupBox.ResumeLayout(false);
            this.videosettinggroupBox.PerformLayout();
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.DPStabPage.ResumeLayout(false);
            this.DPStabPage.PerformLayout();
            this.filtergroup.ResumeLayout(false);
            this.filtergroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DpsMeterGridView)).EndInit();
            this.statusTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.advertisement)).EndInit();
            this.scriptgridMenuStrip.ResumeLayout(false);

            this.ResumeLayout(false);

        }

        #endregion Windows Form Designer generated code
        private void InitializeMacroTab()
        {
            // Yes I know this is stupid, but the compiler couldnt find the method if I put it in the MacrosUI.cs file, so here we are
            InitializeMacroTab2();

        }

        private void InitializeSkillsTab()
        {
            InitializeSkillsTab2();
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == 1025)
            {
                msg.Result = (IntPtr)(Assistant.Client.Instance.OnMessage(this, (uint)msg.WParam.ToInt32(), msg.LParam.ToInt32()) ? 1 : 0);
                return;
            }
            if (msg.Msg >= 1224 && msg.Msg <= 1338)
            {
                msg.Result = (IntPtr)Assistant.UOAssist.OnUOAMessage(this, msg.Msg, msg.WParam.ToInt32(), msg.LParam.ToInt32());
                return;
            }
            base.WndProc(ref msg);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CP_NOCLOSE_BUTTON = 0x200;
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                m_CanClose = false;
                return myCp;
            }
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Assistant.Timer.Slice();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            // Upgrade?
            if (Properties.Settings.Default.F1Size.Width == 686)
            {
                Properties.Settings.Default.Upgrade();
            }

            this.Size = Properties.Settings.Default.F1Size;
            this.Location = Properties.Settings.Default.F1Location;
            this.WindowState = Properties.Settings.Default.F1State;

            m_SystemTimer = new System.Timers.Timer(5);
            m_SystemTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            Timer.SystemTimer = m_SystemTimer;

            this.Hide();

            bool st = RazorEnhanced.Settings.General.ReadBool("Systray");
            taskbar.Checked = this.ShowInTaskbar = !st;
            systray.Checked = m_NotifyIcon.Visible = st;

            UpdateTitle();

            if (!Assistant.Client.Instance.InstallHooks(this.Handle)) // WaitForInputIdle done here
            {
                m_CanClose = true;
                //SplashScreen.End();
                this.Close();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }

            InitConfig();

            this.Show();
            this.BringToFront();

            Engine.ActiveWindow = this;

            tabs_IndexChanged(this, null); // load first tab

            m_Tip.Active = true;
        }

        internal void LoadSettings()
        {
            // -------------- SCRIPTING --------------------
            //scriptTable = Settings.Dataset.Tables["SCRIPTING"];
            ReloadScriptTable();

            // ---------------- AUTOLOOT -----------------
            AutoLoot.RefreshLists();
            autolootautostartCheckBox.Checked = Settings.General.ReadBool("AutolootAutostartCheckBox");

            // ------------ SCAVENGER -------------------
            Scavenger.RefreshLists();
            scavengerautostartCheckBox.Checked = Settings.General.ReadBool("ScavengerAutostartCheckBox");

            // ---------------- ORGANIZER ----------------
            Organizer.RefreshLists();

            // ----------- SELL AGENT -----------------
            SellAgent.RefreshLists();

            // ------------------- BUY AGENT ----------------------
            BuyAgent.RefreshLists();

            // ------------------ DRESS AGENT -------------------------
            RazorEnhanced.Dress.RefreshLists();

            // ------------------ FRIEND -------------------------
            Friend.RefreshLists();

            // ------------------ RESTOCK -------------------------
            Restock.RefreshLists();

            // ------------------ BANDAGE HEAL --------------------
            BandageHeal.LoadSettings();

            // ------------------ ENHANCED FILTERS --------------------
            RazorEnhanced.Filters.LoadSettings();

            // ------------------ ENHANCED TOOLBAR --------------------
            RazorEnhanced.ToolBar.LoadSettings();
            toolbar_trackBar.Value = RazorEnhanced.Settings.General.ReadInt("ToolBarOpacity");

            // ------------------ ENHANCED SPELLGRID --------------------
            RazorEnhanced.SpellGrid.LoadSettings();
            spellgrid_trackBar.Value = RazorEnhanced.Settings.General.ReadInt("GridOpacity");

            // ------------------ TARGETS --------------------
            targetSelectorComboBox.DataSource = TargetGUI.Selectors;
            targetcoloCheckBox.Checked = targethueGridView.Enabled = false;
            targetbodyCheckBox.Checked = targetbodydataGridView.Enabled = false;
            JsonData.Filter.RefreshTargetShortCut(targetlistBox);
            if (targetlistBox.Items.Count > 0)
                EnableTargetGUI();
            else
                DisableTargetGUI();

            // ------------------ HOTKEY --------------------
            //HotKey.Init();

            // ------------------ DPS METER --------------------
            DPSMeterStopButton.Enabled = DPSMeterPauseButton.Enabled = false;
            DPSMeter.Clear();
            DpsMeterGridView.Rows.Clear();

            // ------------------ PARAMETRI GENERALI -------------------
            imgFmt.SelectedItem = RazorEnhanced.Settings.General.ReadString("ImageFormat");

            screenPath.Text = RazorEnhanced.Settings.General.ReadString("CapPath");
            radioUO.Checked = !(radioFull.Checked = RazorEnhanced.Settings.General.ReadBool("CapFullScreen"));
            imgFmt.SelectedItem = RazorEnhanced.Settings.General.ReadString("ImageFormat");
            dispTime.Checked = RazorEnhanced.Settings.General.ReadBool("CapTimeStamp");
            screenAutoCap.Checked = RazorEnhanced.Settings.General.ReadBool("AutoCap");
            Filters.Filter.Load();
            Filters.Filter.Draw(filters);
            if (Assistant.Client.IsOSI)
            {
                smartCPU.Checked = RazorEnhanced.Settings.General.ReadBool("SmartCPU");
            }
            else
            {
                smartCPU.Checked = false;
            }

            this.TopMost = alwaysTop.Checked = RazorEnhanced.Settings.General.ReadBool("AlwaysOnTop");
            rememberPwds.Checked = RazorEnhanced.Settings.General.ReadBool("RememberPwds");

            forceSizeX.Text = RazorEnhanced.Settings.General.ReadInt("ForceSizeX").ToString();
            forceSizeY.Text = RazorEnhanced.Settings.General.ReadInt("ForceSizeY").ToString();
            gameSize.Checked = RazorEnhanced.Settings.General.ReadBool("ForceSizeEnabled");
            if (!Assistant.Client.IsOSI)
            {
                DisableGameSize();
            }
            showlauncher.Checked = Shards.ShowLauncher;
            forceSizeX.Enabled = forceSizeY.Enabled = gameSize.Checked;
            taskbar.Checked = !(systray.Checked = RazorEnhanced.Settings.General.ReadBool("Systray"));
            clientPrio.SelectedItem = RazorEnhanced.Settings.General.ReadString("ClientPrio");
            opacity.AutoSize = false;
            opacity.Value = RazorEnhanced.Settings.General.ReadInt("Opacity");
            this.Opacity = opacity.Value / 100.0;
            opacityLabel.Text = String.Format("Opacity: {0}%", opacity.Value);
            msglvl.SelectedIndex = RazorEnhanced.Settings.General.ReadInt("MessageLevel");

            this.Location = new System.Drawing.Point(RazorEnhanced.Settings.General.ReadInt("WindowX"), RazorEnhanced.Settings.General.ReadInt("WindowY"));
            Assistant.Engine.MainWindowX = RazorEnhanced.Settings.General.ReadInt("WindowX");
            Assistant.Engine.MainWindowY = RazorEnhanced.Settings.General.ReadInt("WindowY");

            this.TopLevel = true;

            bool onScreen = false;
            foreach (Screen s in Screen.AllScreens)
            {
                if (s.Bounds.Contains(this.Location.X + this.Width, this.Location.Y + this.Height) ||
                    s.Bounds.Contains(this.Location.X - this.Width, this.Location.Y - this.Height))
                {
                    onScreen = true;
                    break;
                }
            }

            if (!onScreen)
                this.Location = Point.Empty;

            dispDelta.Checked = RazorEnhanced.Settings.General.ReadBool("DisplaySkillChanges");
            actionStatusMsg.Checked = RazorEnhanced.Settings.General.ReadBool("ActionStatusMsg");
            QueueActions.Checked = RazorEnhanced.Settings.General.ReadBool("QueueActions");
            txtObjDelay.Text = RazorEnhanced.Settings.General.ReadInt("ObjectDelay").ToString();
            smartLT.Checked = RazorEnhanced.Settings.General.ReadBool("SmartLastTarget");
            ltRange.Enabled = rangeCheckLT.Checked = RazorEnhanced.Settings.General.ReadBool("RangeCheckLT");
            ltRange.Text = RazorEnhanced.Settings.General.ReadInt("LTRange").ToString();
            showtargtext.Checked = RazorEnhanced.Settings.General.ReadBool("LastTargTextFlags");
            healthFmt.Enabled = showHealthOH.Checked = RazorEnhanced.Settings.General.ReadBool("ShowHealth");
            healthFmt.Text = RazorEnhanced.Settings.General.ReadString("HealthFmt");
            chkPartyOverhead.Checked = RazorEnhanced.Settings.General.ReadBool("ShowPartyStats");
            preAOSstatbar.Checked = RazorEnhanced.Settings.General.ReadBool("OldStatBar");
            queueTargets.Checked = RazorEnhanced.Settings.General.ReadBool("QueueTargets");
            blockDis.Checked = RazorEnhanced.Settings.General.ReadBool("BlockDismount");
            autoStackRes.Checked = RazorEnhanced.Settings.General.ReadBool("AutoStack");
            corpseRange.Enabled = openCorpses.Checked = RazorEnhanced.Settings.General.ReadBool("AutoOpenCorpses");
            corpseRange.Text = RazorEnhanced.Settings.General.ReadInt("CorpseRange").ToString();
            allowHiddenLooting.Checked = hiddedAutoOpenDoors.Enabled = RazorEnhanced.Settings.General.ReadBool("AutoOpenCorpses");
            allowHiddenLooting.Checked = RazorEnhanced.Settings.General.ReadBool("AllowHiddenLooting");

            spamFilter.Checked = RazorEnhanced.Settings.General.ReadBool("FilterSpam");
            filterSnoop.Checked = RazorEnhanced.Settings.General.ReadBool("FilterSnoopMsg");
            filterPoison.Checked = RazorEnhanced.Settings.General.ReadBool("FilterPoison");
            filterNPC.Checked = RazorEnhanced.Settings.General.ReadBool("FilterNPC");
            incomingMob.Checked = RazorEnhanced.Settings.General.ReadBool("ShowMobNames");
            incomingCorpse.Checked = RazorEnhanced.Settings.General.ReadBool("ShowCorpseNames");
            chkStealth.Checked = RazorEnhanced.Settings.General.ReadBool("CountStealthSteps");
            autoOpenDoors.Checked = hiddedAutoOpenDoors.Enabled = RazorEnhanced.Settings.General.ReadBool("AutoOpenDoors");
            hiddedAutoOpenDoors.Checked = RazorEnhanced.Settings.General.ReadBool("HiddedAutoOpenDoors");
            spellUnequip.Checked = RazorEnhanced.Settings.General.ReadBool("SpellUnequip");
            potionEquip.Checked = RazorEnhanced.Settings.General.ReadBool("PotionEquip");
            autosearchcontainers.Checked = RazorEnhanced.Settings.General.ReadBool("AutoSearch");
            nosearchpouches.Checked = RazorEnhanced.Settings.General.ReadBool("NoSearchPouches");
            druidClericPackets.Checked = RazorEnhanced.Settings.General.ReadBool("DruidClericPackets");
            remoteControl.Checked = RazorEnhanced.Settings.General.ReadBool("RemoteControl");
            chknorunStealth.Checked = RazorEnhanced.Settings.General.ReadBool("ChkNoRunStealth");
            enhancedmappathTextBox.Text = Settings.General.ReadString("EnhancedMapPath");
            autoScriptReload.Checked = RazorEnhanced.Settings.General.ReadBool("AutoScriptReload");

            chkForceSpeechHue.Checked = setSpeechHue.Enabled = RazorEnhanced.Settings.General.ReadBool("ForceSpeechHue");
            chkForceSpellHue.Checked = setBeneHue.Enabled = setNeuHue.Enabled = setHarmHue.Enabled = RazorEnhanced.Settings.General.ReadBool("ForceSpellHue");
            if (RazorEnhanced.Settings.General.ReadInt("LTHilight") != 0)
            {
                InitPreviewHue(lthilight, "LTHilight");
                lthilight.Checked = setLTHilight.Enabled = true;
            }
            else
            {
                lthilight.Checked = setLTHilight.Enabled = false;
            }
            InitPreviewHue(lblMsgHue, "SysColor");
            InitPreviewHue(lblWarnHue, "WarningColor");
            InitPreviewHue(chkForceSpeechHue, "SpeechHue");
            InitPreviewHue(lblBeneHue, "BeneficialSpellHue");
            InitPreviewHue(lblHarmHue, "HarmfulSpellHue");
            InitPreviewHue(lblNeuHue, "NeutralSpellHue");

            txtSpellFormat.Text = RazorEnhanced.Settings.General.ReadString("SpellFormat");

            // Script
            showscriptmessageCheckBox.Checked = Settings.General.ReadBool("ShowScriptMessageCheckBox");
            scripterrorlogCheckBox.Checked = Scripts.ScriptErrorLog = Settings.General.ReadBool("ScriptErrorLog");
            scriptshowStartStopCheckBox.Checked = Scripts.ScriptStartStopMessage = Settings.General.ReadBool("ScriptStartStopMessage");

            // UoMod
            if (Engine.ClientMajor >= 7) //&& Engine.ClientBuild < 49)
            {
                uomodFPSCheckBox.Checked = RazorEnhanced.Settings.General.ReadBool("UoModFPS");
                uomodpaperdollCheckBox.Checked = RazorEnhanced.Settings.General.ReadBool("UoModPaperdoll");
                uomodglobalsoundCheckBox.Checked = RazorEnhanced.Settings.General.ReadBool("UoModSound");
            }
            else
            {
                uomodFPSCheckBox.Enabled = false;
                uomodpaperdollCheckBox.Enabled = false;
                uomodglobalsoundCheckBox.Enabled = false;
            }

            limitDamageDisplayEnable.Checked = RazorEnhanced.Settings.General.ReadBool("LimitDamageDisplay");

            // Video Recorder
            videoPathTextBox.Text = Settings.General.ReadString("VideoPath");
            videoFPSTextBox.Text = Settings.General.ReadInt("VideoFPS").ToString();
            videoCodecComboBox.SelectedIndex = Settings.General.ReadInt("VideoFormat");
        }

        public void SetBandSelfState()
        {
            bandagehealusetextSelfContent.Enabled = bandagehealusetext.Checked;
            bandagehealusetextContent.Enabled = bandagehealusetext.Checked;
            BandageHealUseTarget.Enabled = !bandagehealusetext.Checked;
            bandageHealIgnoreCount.Enabled = bandagehealusetext.Checked;
        }

        internal void DisableGameSize()
        {
            this.gameSize.Text = "Unavailable / CUO";
            this.gameSize.Checked = false;
            this.gameSize.Enabled = false;
            this.forceSizeY.Enabled = false;
            this.forceSizeX.Enabled = false;
        }

        public void DisableSmartCpu()
        {
            this.smartCPU.Text = "Unavailable / CUO";
            this.smartCPU.Enabled = false;
            this.smartCPU.Checked = false;
        }

        private bool m_Initializing = false;
        internal bool Initializing
        {
            get { return m_Initializing; }
            set { m_Initializing = value; }
        }

        internal void InitConfig()
        {
            m_Initializing = true;
            LoadSettings();
            RazorEnhanced.Profiles.Refresh();

            // Init mappe ultima.dll
            //Ultima.Map.InitializeMap("Felucca");
            //Ultima.Map.InitializeMap("Trammel");
            //Ultima.Map.InitializeMap("Ilshenar");
            //Ultima.Map.InitializeMap("Malas");
            //Ultima.Map.InitializeMap("Tokuno");
            //Ultima.Map.InitializeMap("TerMur");

            m_Initializing = false;
        }

        public void removeVideoTab()
        {
            this.AdvancedPages.Controls.Remove(this.videoTab);
        }

        private void tabs_IndexChanged(object sender, System.EventArgs e)
        {
            if (tabs == null)
                return;
            else if (tabs.SelectedTab == skillsTab)
            {
                RedrawSkills();
            }
            else if (tabs.SelectedTab == technicalTab && TechnicalPages.SelectedTab == statusTab)
            {
                UpdateRazorStatus();
            }
            else if (tabs.SelectedTab == advancedTab && AdvancedPages.SelectedTab == screenshotTab)
            {
                ReloadScreenShotsList();
            }
            else if (tabs.SelectedTab == pythonScriptingTab)
            {
                UpdateScriptGrid();
            }
            
            else if (tabs.SelectedTab == advancedTab  && AdvancedPages.SelectedTab ==  videoTab)
            {
                if (!tabs.SelectedTab.Enabled)
                {
                }
                else
                    ReloadVideoList();
            }
        }

        private readonly Version m_Ver = System.Reflection.Assembly.GetCallingAssembly().GetName().Version;

        private uint m_OutPrev;
        private uint m_InPrev;

        private void UpdateRazorStatus()
        {
            if (!Assistant.Client.Instance.ClientRunning)
                Close();

            if ((tabs.SelectedTab != technicalTab || TechnicalPages.SelectedTab != statusTab))
                return;

            uint ps = m_OutPrev;
            uint pr = m_InPrev;
            m_OutPrev = Client.Instance.TotalDataOut(); // DLLImport.Razor.TotalOut();
            m_InPrev = Client.Instance.TotalDataIn(); //  DLLImport.Razor.TotalIn();

            int time = 0;
            if (Assistant.Client.Instance.ConnectionStart != DateTime.MinValue)
                time = (int)((DateTime.Now - Assistant.Client.Instance.ConnectionStart).TotalSeconds);

            string status = Language.Format(LocString.RazorStatus1,
                m_Ver,
                Utility.FormatSize(System.GC.GetTotalMemory(false)),
                Utility.FormatSize(m_OutPrev), Utility.FormatSize((m_OutPrev - ps)),
                Utility.FormatSize(m_InPrev), Utility.FormatSize((m_InPrev - pr)),
                Utility.FormatTime(time),
                World.Player != null ? (uint)World.Player.Serial : 0,
                World.Player != null && World.Player.Backpack != null ? (uint)World.Player.Backpack.Serial : 0,
                World.Items.Count,
                World.Mobiles.Count,
                ProtoControlServer.Instance.AssignedPort);

            if (World.Player != null)
                status += String.Format("\r\nCoordinates\r\nX: {0}\r\nY: {1}\r\nZ: {2}", World.Player.Position.X, World.Player.Position.Y, World.Player.Position.Z);

            labelStatus.Text = status;
        }


        internal bool CanClose
        {
            get
            {
                return m_CanClose;
            }
            set
            {
                m_CanClose = value;
            }
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!m_CanClose && Assistant.Client.Instance.ClientRunning)
            {
                e.Cancel = true;
            }
        }

        private void MainForm_Activated(object sender, System.EventArgs e)
        {
        }

        private void MainForm_Resize(object sender, System.EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && !this.ShowInTaskbar)
                this.Hide();
        }

        private void MainForm_Move(object sender, System.EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                windowspt = this.Location;
                Assistant.Engine.MainWindowX = this.Location.X;
                Assistant.Engine.MainWindowY = this.Location.Y;
            }
        }


        internal void UpdateTitle()
        {
            string str = LanguageHelper.GetString(this.Name + ".Text");
            if (str == null || str == String.Empty || str == this.Name + ".Text")
                str = "TM Razor {0}";
            if (str == null || str == String.Empty)
                str = "TM Razor {0}";

            str = String.Format(str, Engine.Version);
            if (World.Player != null)
                this.Text = String.Format("{1} ({2}) - {0}", str, World.Player.Name, World.ShardName);
            else
                this.Text = str;

            UpdateSystray();
        }

        internal void UpdateSystray()
        {
            if (m_NotifyIcon != null && m_NotifyIcon.Visible)
            {
                if (World.Player != null)
                    m_NotifyIcon.Text = String.Format("Razor Enhanced - {0} ({1})", World.Player.Name, World.ShardName);
                else
                    m_NotifyIcon.Text = "TM Razor";
            }
        }

        private void DoShowMe(object sender, System.EventArgs e)
        {
            ShowMe();
        }

        internal void ShowMe()
        {
            Client.BringToFront(this.Handle);
            if (RazorEnhanced.Settings.General.ReadBool("AlwaysOnTop"))
                this.TopMost = true;
            if (WindowState != FormWindowState.Normal)
                WindowState = FormWindowState.Normal;
        }

        private void HideMe(object sender, System.EventArgs e)
        {
            //this.WindowState = FormWindowState.Minimized;
            this.TopMost = false;
            this.SendToBack();
            this.Hide();
        }

        private void NotifyIcon_DoubleClick(object sender, System.EventArgs e)
        {
            ShowMe();
        }

        private void ToggleVisible(object sender, System.EventArgs e)
        {
            if (this.Visible)
                HideMe(sender, e);
            else
                ShowMe();
        }

        private void OnClose(object sender, System.EventArgs e)
        {
            m_CanClose = true;
            this.Close();
        }

        private void razorButtonWebsite_Click(object sender, EventArgs e)
        {
            ProcessStartInfo p = new("https://razorenhanced.github.io/");
            try
            {
                Process.Start(p);
            }
            catch { }
        }
        private void razorButtonWiki_Click(object sender, EventArgs e)
        {
            ProcessStartInfo p = new("https://razorenhanced.net/dokuwiki/");
            try
            {
                Process.Start(p);
            }
            catch { }
        }

        private void razorButtonSource_Click(object sender, EventArgs e)
        {
            ProcessStartInfo p = new("https://github.com/RazorEnhanced/RazorEnhanced");
            try
            {
                Process.Start(p);
            }
            catch { }
        }

        
        private void advertisement_Click(object sender, EventArgs e)
        {
            ProcessStartInfo p = new("https://www.uoeventine.com/");
            try
            {
                Process.Start(p);
            }
            catch { }
        }

        private void advertisementDiscord_Click(object sender, EventArgs e)
        {
            ProcessStartInfo p = new("https://discord.com/invite/vF9TXZW");
            try
            {
                Process.Start(p);
            }
            catch { }
        }


        private void discordrazorButton_Click(object sender, EventArgs e)
        {
            //ProcessStartInfo p = new ProcessStartInfo("https://discord.gg/P3Q7mKT");
            ProcessStartInfo p = new("https://discord.com/invite/ukQUK7cSPd");
            try
            {
                Process.Start(p);
            }
            catch { }
        }

        private void chkForUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Leave stuff thats already set up
                AutoUpdater.Start("https://raw.githubusercontent.com/RazorEnhanced/razorenhanced.github.io/main/RazorEnhancedAutoUpdater.xml");
            }
            catch
            {
            }
        }



        private void openchangelogButton_Click(object sender, EventArgs e)
        {
            EnhancedChangeLog changelogform = new()
            {
                TopMost = true
            };
            changelogform.Show();
        }

        // ----------------- FEATURE START -------------------

        public void UpdateControlLocks()
        {
            if (!Assistant.Client.Instance.AllowBit(FeatureBit.AutolootAgent))
            {
                autoLootCheckBox.Enabled = false;
                autoLootCheckBox.Checked = false;
                if (RazorEnhanced.AutoLoot.Status())
                    RazorEnhanced.AutoLoot.Stop();
            }
            else
            {
                if (!autoLootCheckBox.Enabled)
                    autoLootCheckBox.Enabled = true;
            }

            if (!Assistant.Client.Instance.AllowBit(FeatureBit.RangeCheckLT))
            {
                rangeCheckLT.Checked = false;
                rangeCheckLT.Enabled = false;
                Settings.General.WriteBoolNoSave("RangeCheckLT", false);
            }
            else
            {
                if (!rangeCheckLT.Enabled)
                    rangeCheckLT.Enabled = true;
            }


            /*  if (Client.AllowBit(FeatureBit.AutoOpenDoors))
                {
                    RazorEnhanced.AutoLoot.AddLog(Client.AllowBit(FeatureBit.LightFilter).ToString());

                    //RazorEnhanced.AutoLoot.AddLog(Client.AllowBit(FeatureBit.AutoOpenDoors).ToString());
                    autoOpenDoors.Checked = false;
                    autoOpenDoors.Enabled = false;
                    Settings.General.WriteBoolNoSave("AutoOpenDoors", false);
                }
                else
                {
                    if (!autoOpenDoors.Enabled)
                        autoOpenDoors.Enabled = true;
                }*/


            if (!Assistant.Client.Instance.AllowBit(FeatureBit.UnequipBeforeCast))
            {
                spellUnequip.Checked = false;
                spellUnequip.Enabled = false;
                Settings.General.WriteBoolNoSave("SpellUnequip", false);
            }
            else
            {
                if (!spellUnequip.Enabled)
                    spellUnequip.Enabled = true;
            }

            if (!Assistant.Client.Instance.AllowBit(FeatureBit.AutoPotionEquip))
            {
                potionEquip.Checked = false;
                potionEquip.Enabled = false;
                Settings.General.WriteBoolNoSave("PotionEquip", false);
            }
            else
            {
                if (!potionEquip.Enabled)
                    potionEquip.Enabled = true;
            }

            if (!Assistant.Client.Instance.AllowBit(FeatureBit.BlockHealPoisoned))
            {
                blockhealpoisonCheckBox.Checked = false;
                blockhealpoisonCheckBox.Enabled = false;
                Settings.General.WriteBoolNoSave("BlockHealPoison", false);
            }
            else
            {
                if (!blockhealpoisonCheckBox.Enabled)
                    blockhealpoisonCheckBox.Enabled = true;
            }

            /*  if (!Client.AllowBit(FeatureBit.SellAgent))
                {
                    sellEnableCheckBox.Enabled = false;
                    sellEnableCheckBox.Checked = false;
                }
                else
                {
                    if (!sellEnableCheckBox.Enabled)
                        sellEnableCheckBox.Enabled = true;
                }*/


            /*if (!Client.AllowBit(FeatureBit.BuyAgent))
            {
                buyEnableCheckBox.Enabled = false;
                buyEnableCheckBox.Checked = false;
            }
            else
            {
                if (!buyEnableCheckBox.Enabled)
                    buyEnableCheckBox.Enabled = true;
            }*/

            if (!Assistant.Client.Instance.AllowBit(FeatureBit.OverheadHealth))
            {
                chkPartyOverhead.Checked = false;
                chkPartyOverhead.Enabled = false;
                Settings.General.WriteBoolNoSave("ShowPartyStats", false);
            }
            else
            {
                if (!chkPartyOverhead.Enabled)
                    chkPartyOverhead.Enabled = true;
            }

            if (!Assistant.Client.Instance.AllowBit(FeatureBit.BoneCutterAgent))
            {
                bonecutterCheckBox.Checked = false;
                bonecutterCheckBox.Enabled = false;
                Settings.General.WriteBoolNoSave("BoneCutterCheckBox", false);
            }
            else
            {
                if (!bonecutterCheckBox.Enabled)
                    bonecutterCheckBox.Enabled = true;
            }

            if (!Assistant.Client.Instance.AllowBit(FeatureBit.AutoRemount))
            {
                remountcheckbox.Checked = false;
                remountcheckbox.Enabled = false;
                Settings.General.WriteBoolNoSave("RemountCheckbox", false);
            }
            else
            {
                if (!remountcheckbox.Enabled)
                    remountcheckbox.Enabled = true;
            }


            if (!Assistant.Client.Instance.AllowBit(FeatureBit.AutoBandage))
            {
                bandagehealenableCheckBox.Enabled = false;
                bandagehealenableCheckBox.Checked = false;
                if (RazorEnhanced.BandageHeal.Status())
                    RazorEnhanced.BandageHeal.Stop();
            }
            else
            {
                if (!bandagehealenableCheckBox.Enabled)
                    bandagehealenableCheckBox.Enabled = true;
            }
            if (!Assistant.Client.Instance.AllowBit(FeatureBit.FPSOverride))
            {
                UoMod.EnableDisable(false, (int)UoMod.PATCH_TYPE.PT_FPS);
                uomodFPSCheckBox.Enabled = false;
                uomodFPSCheckBox.Checked = false;
            }
            else
            {
                uomodFPSCheckBox.Enabled = true;
                uomodFPSCheckBox.Checked = RazorEnhanced.Settings.General.ReadBool("UoModFPS");
            }


        }
        // ----------------- FEATURE END -------------------

        // ----------------- UO MOD START -------------------
        private void uomodFPSCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (uomodFPSCheckBox.Focused)
            {
                if (uomodFPSCheckBox.Checked)
                {
                    if (Engine.ClientBuild > 49)
                        RazorEnhanced.UI.RE_MessageBox.Show("Warning",
                            "Enabling this option can make client unstable",
                            ok: "Accept", no: null, cancel: null, backColor: null);
                    UoMod.EnableDisable(true, (int)UoMod.PATCH_TYPE.PT_FPS);
                }
                else
                    UoMod.EnableDisable(false, (int)UoMod.PATCH_TYPE.PT_FPS);

                RazorEnhanced.Settings.General.WriteBool("UoModFPS", uomodFPSCheckBox.Checked);
            }
        }

        private void DmgDisplayLimitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (limitDamageDisplayEnable.Focused)
            {
                if (limitDamageDisplayEnable.Checked)
                {
                    minDmgShown.Enabled = true;
                }
                else
                {
                    minDmgShown.Enabled = false;
                }

                RazorEnhanced.Settings.General.WriteBool("LimitDamageDisplay", limitDamageDisplayEnable.Checked);
            }
        }

        private void uomodpaperdollCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (uomodpaperdollCheckBox.Focused)
            {
                if (uomodpaperdollCheckBox.Checked)
                {
                    if (Engine.ClientBuild > 49)
                        RazorEnhanced.UI.RE_MessageBox.Show("Warning",
                            "Enabling this option can make client unstable",
                            ok: "Accept", no: null, cancel: null, backColor: null);
                    UoMod.EnableDisable(true, (int)UoMod.PATCH_TYPE.PT_PAPERDOLL_SLOTS);
                }
                else
                    UoMod.EnableDisable(false, (int)UoMod.PATCH_TYPE.PT_PAPERDOLL_SLOTS);

                RazorEnhanced.Settings.General.WriteBool("UoModPaperdoll", uomodpaperdollCheckBox.Checked);
            }
        }

        private void uomodglobalsoundCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (uomodglobalsoundCheckBox.Focused)
            {
                if (uomodglobalsoundCheckBox.Checked)
                {
                    if (Engine.ClientBuild > 49)
                        RazorEnhanced.UI.RE_MessageBox.Show("Warning",
                            "Enabling this option can make client unstable",
                            ok: "Accept", no: null, cancel: null, backColor: null);
                    UoMod.EnableDisable(true, (int)UoMod.PATCH_TYPE.PT_GLOBAL_SOUND);
                }
                else
                    UoMod.EnableDisable(false, (int)UoMod.PATCH_TYPE.PT_GLOBAL_SOUND);

                RazorEnhanced.Settings.General.WriteBool("UoModSound", uomodglobalsoundCheckBox.Checked);
            }
        }

        private void paypalButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo p = new("https://github.com/sponsors/credzba");
            Process.Start(p);
        }


        private void hiddenLooting_CheckedChanged(object sender, EventArgs e)
        {
            if (allowHiddenLooting.Focused)
                RazorEnhanced.Settings.General.WriteBool("AllowHiddenLooting", allowHiddenLooting.Checked);

        }

        private void scriptlistView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ScriptGridOpen();
        }

        private void InspectGump_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is EnhancedGumpInspector af)
                {
                    af.Focus();
                    return;
                }
            }
            EnhancedGumpInspector ginspector = new();
            ginspector.FormClosed += new FormClosedEventHandler(Gumpinspector_close);
            ginspector.TopMost = true;
            ginspector.Show();
        }
        private static void Gumpinspector_close(object sender, EventArgs e)
        {
            Assistant.Engine.MainWindow.GumpInspectorEnable = false;
        }

        private void InspectContext_Click(object sender, EventArgs e)
        {
            Targeting.OneTimeTarget(true, new Targeting.TargetResponseCallback(Commands.GetInfoTarget_Callback));
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            MainForm_SaveWindowLocation();
        }

        private void MainForm_LocationChanged(object sender, EventArgs e)
        {
            MainForm_SaveWindowLocation();
        }

        internal void MainForm_SaveWindowLocation()
        {
            Properties.Settings.Default.F1State = this.WindowState;
            if (this.WindowState == FormWindowState.Normal)
            {
                // save location and size if the state is normal
                Properties.Settings.Default.F1Location = this.Location;
                Properties.Settings.Default.F1Size = this.Size;
            }
            else
            {
                // save the RestoreBounds if the form is minimized or maximized!
                Properties.Settings.Default.F1Location = this.RestoreBounds.Location;
                Properties.Settings.Default.F1Size = this.RestoreBounds.Size;
            }

            // don't forget to save the settings
            Properties.Settings.Default.Save();
        }

        private void scriptPacketLogCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            if (scriptPacketLogCheckBox.Checked)
            {
                var path = PacketLogger.SharedInstance.StartRecording(appendLogs: true);
                if (this.scriptshowStartStopCheckBox.Checked)
                {
                    Misc.SendMessage($"Packet Logger: START", 178);
                    Misc.SendMessage(path, 178);
                }
            }
            else
            {

                var path = PacketLogger.SharedInstance.StopRecording();
                if (this.scriptshowStartStopCheckBox.Checked)
                {
                    Misc.SendMessage($"Packet Logger: STOP", 138);
                    // Misc.SendMessage(path);
                }
            }
        }

        private System.Windows.Forms.Panel SidebarPanel;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarOptions;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarFilters;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarScripting;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarMacros;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarAgents;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarToolbars;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarSkills;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarHotkeys;
        private Assistant.UI.Controls.RazorSidebarTab btnSidebarAdvanced;
        private EventHandler _spellHueVisibilityHandler;

        private void InitializeSidebar()
        {
            SidebarPanel = new Panel {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(0)
            };
            this.Controls.Add(SidebarPanel);
            SidebarPanel.BringToFront();

            Panel footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark };

            // --- Language selector row ---
            Panel langRow = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = Color.Transparent, Padding = new Padding(0) };

            Label langLabel = new Label {
                Text = "\uE774", // Segoe MDL2 globe icon
                Font = new Font("Segoe MDL2 Assets", 11F),
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                AutoSize = false,
                Width = 24, Height = 50,
                Left = 14,
                TextAlign = ContentAlignment.MiddleCenter
            };

            ComboBox langCombo = new ComboBox {
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark,
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode,
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F),
                Width = 130,
                Height = 28,
                Left = 44,
                Top = 11,
                Cursor = Cursors.Hand
            };
            langCombo.Items.AddRange(new object[] { "English", "Italiano" });
            langCombo.SelectedIndex = (LanguageHelper.CurrentLanguage == "en") ? 0 : 1;
            langCombo.SelectedIndexChanged += (s, e) => {
                string newLang = langCombo.SelectedIndex == 0 ? "en" : "it";
                if (LanguageHelper.CurrentLanguage != newLang) {
                    LanguageHelper.CurrentLanguage = newLang;
                    Shards.allShards.Language = newLang;
                    // Translate standard WinForms controls (those with registered resource keys)
                    LanguageHelper.TranslateForm(this);
                    // Refresh dynamically-built sidebar button labels
                    btnSidebarOptions.Text  = LanguageHelper.GetString("MainForm.moreOptTab.Text");
                    btnSidebarFilters.Text  = LanguageHelper.GetString("MainForm.enhancedFilterTab.Text");
                    btnSidebarScripting.Text = LanguageHelper.GetString("MainForm.AllScripts.Text");
                    btnSidebarMacros.Text   = LanguageHelper.GetString("MainForm.MacrosTab.Text");
                    btnSidebarAgents.Text   = LanguageHelper.GetString("MainForm.EnhancedAgent.Text");
                    btnSidebarToolbars.Text = LanguageHelper.GetString("MainForm.toolbarTab.Text");
                    btnSidebarSkills.Text   = LanguageHelper.GetString("MainForm.skillsTab.Text");
                    btnSidebarHotkeys.Text  = LanguageHelper.GetString("MainForm.enhancedHotKeytabPage.Text");
                    btnSidebarAdvanced.Text = LanguageHelper.GetString("MainForm.advancedTab.Text");
                    // Rebuild the Options tab (cards/labels are created dynamically with GetString)
                    RebuildOptionsTab();
                    RebuildStatBarTab();
                    RebuildSpellGridTab();
                    RebuildHotKeysTab();
                    RazorEnhanced.HotKey.Init();
                    // Rebuild the Filters sub-tabs
                    RebuildVirtualFilterTab();
                    RebuildTargettingFilterTab();
                    RebuildMiscFilterTab();
                }
            };

            langRow.Controls.Add(langLabel);
            langRow.Controls.Add(langCombo);
            footerPanel.Controls.Add(langRow);

            // --- Version / status row ---
            Panel versionRow = new Panel { Height = 40, Dock = DockStyle.Bottom, BackColor = Color.Transparent };
            versionRow.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                using (Font f = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(8F)) { TextRenderer.DrawText(e.Graphics, "Version 1.0.0.2", f, new Rectangle(20, 0, 100, 40), Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark, TextFormatFlags.Left | TextFormatFlags.VerticalCenter); }
                using (SolidBrush b = new SolidBrush(Color.FromArgb(34, 197, 94))) { e.Graphics.FillEllipse(b, 175, 16, 8, 8); }
            };
            footerPanel.Controls.Add(versionRow);

            SidebarPanel.Controls.Add(footerPanel);

            Panel headerLogoPanel = new Panel { Dock = DockStyle.Top, Height = 80 };
            headerLogoPanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                
                if (this.Icon != null)
                {
                    e.Graphics.DrawIcon(this.Icon, new Rectangle(20, 24, 32, 32));
                }

                using (Font fontTitle = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(11F, FontStyle.Bold)) { TextRenderer.DrawText(e.Graphics, "TM Razor", fontTitle, new Rectangle(62, 25, 138, 30), Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter); }
            };
            SidebarPanel.Controls.Add(headerLogoPanel);
            headerLogoPanel.BringToFront();

            btnSidebarOptions = CreateSidebarButton(LanguageHelper.GetString("MainForm.moreOptTab.Text"), "\xE713", 0);
            btnSidebarFilters = CreateSidebarButton(LanguageHelper.GetString("MainForm.enhancedFilterTab.Text"), "\xE71C", 1);
            btnSidebarScripting = CreateSidebarButton(LanguageHelper.GetString("MainForm.AllScripts.Text"), "\xE943", 2);
            btnSidebarMacros = CreateSidebarButton(LanguageHelper.GetString("MainForm.MacrosTab.Text"), "\xE765", 3);
            btnSidebarAgents = CreateSidebarButton(LanguageHelper.GetString("MainForm.EnhancedAgent.Text"), "\xE716", 4);
            btnSidebarToolbars = CreateSidebarButton(LanguageHelper.GetString("MainForm.toolbarTab.Text"), "\xE8A5", 5);
            btnSidebarSkills = CreateSidebarButton(LanguageHelper.GetString("MainForm.skillsTab.Text"), "\xE825", 6);
            btnSidebarHotkeys = CreateSidebarButton(LanguageHelper.GetString("MainForm.enhancedHotKeytabPage.Text"), "\xE8A6", 7);
            btnSidebarAdvanced = CreateSidebarButton(LanguageHelper.GetString("MainForm.advancedTab.Text"), "\xE790", 8);

            btnSidebarOptions.IsActive = true;
            this.tabs.SelectedIndex = 0;
            this.toolbarTab.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.toolbarstab.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            RebuildOptionsTab();
            RebuildStatBarTab();
            RebuildSpellGridTab();
            RebuildHotKeysTab();
            RebuildVirtualFilterTab();
            RebuildTargettingFilterTab();
            RebuildMiscFilterTab();
        }

        private Assistant.UI.Controls.RazorSidebarTab CreateSidebarButton(string text, string icon, int index)
        {
            var btn = new Assistant.UI.Controls.RazorSidebarTab {
                Text = text,
                IconText = icon,
                Dock = DockStyle.Top,
                Height = 45
            };
            btn.Click += (s, e) => {
                foreach (Control c in SidebarPanel.Controls) {
                    if (c is Assistant.UI.Controls.RazorSidebarTab tab) {
                        tab.IsActive = false;
                    }
                }
                btn.IsActive = true;
                this.tabs.SelectedIndex = index;
            };
            SidebarPanel.Controls.Add(btn);
            btn.BringToFront();
            return btn;
        }

        // ─────────────────────────────────────────────────────────────────────
        // FILTER TAB REBUILDS
        // ─────────────────────────────────────────────────────────────────────

        private void RebuildVirtualFilterTab()
        {
            this.JournalFilterPage.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.JournalFilterPage.Controls.Clear();

            // ── Header ───────────────────────────────────────────────────────
            Panel headerPanel = new Panel {
                Dock = DockStyle.Top, Height = 90,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark
            };
            headerPanel.Controls.Add(new Label {
                Text = LanguageHelper.GetString("MainForm.JournalFilterPage.Text"),
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(18F, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(24, 16)
            });
            this.JournalFilterPage.Controls.Add(headerPanel);

            // ── Scrollable body ───────────────────────────────────────────────
            Panel scrollWrapper = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(0)
            };
            FlowLayoutPanel flow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(12, 8, 12, 8),
                WrapContents = true
            };
            flow.HandleCreated += (s2, e2) => {
                try { SetWindowTheme(flow.Handle, "DarkMode_Explorer", null); } catch { }
            };
            scrollWrapper.Controls.Add(flow);
            this.JournalFilterPage.Controls.Add(scrollWrapper);
            scrollWrapper.BringToFront();

            Color accent = ColorTranslator.FromHtml("#8B5CF6");
            int   cw     = 340;

            // ── Card 1 – Filtri Messaggi (toggle rows – uses RazorCard fine) ──
            var cMsgs = BuildCard("\uE965", LanguageHelper.GetString("MainForm.groupBox1.Text"), cw, accent);
            AddRow(cMsgs, this.spamFilter,   LanguageHelper.GetString("MainForm.spamFilter.Text"));
            AddRow(cMsgs, this.filterSnoop,  LanguageHelper.GetString("MainForm.filterSnoop.Text"));
            AddRow(cMsgs, this.filterPoison, LanguageHelper.GetString("MainForm.filterPoison.Text"));
            AddRow(cMsgs, this.filterNPC,    LanguageHelper.GetString("MainForm.filterNPC.Text"));
            FinalizeCard(cMsgs);

            // ── Pseudo-card 2 – Altri Filtri (plain Panel so scroll works) ───
            // RazorCard extends GroupBox which clips children; for scrollable
            // controls we use a self-painted Panel instead.
            const int listH = 200;
            var pMore = MakePseudoCard("\uE9B0  " + LanguageHelper.GetString("MainForm.morefilterLabel.Text"),
                                       cw, listH + 44, accent);
            this.filters.Parent      = pMore;
            this.filters.Location    = new Point(10, 38);
            this.filters.Width       = cw - 20;
            this.filters.Height      = listH;
            this.filters.BackColor   = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.filters.ForeColor   = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            this.filters.BorderStyle = BorderStyle.None;
            this.filters.IntegralHeight = false;

            // ── Pseudo-card 3 – Filtro Giornale (plain Panel so scroll works) ─
            const int gridH = 230;
            int       gridW = cw * 2 + 20; // fills two columns
            var pGrid = MakePseudoCard("\uE8A7  " + "Filtro Giornale", gridW, gridH + 44, accent);
            this.journalfilterdatagrid.Parent   = pGrid;
            this.journalfilterdatagrid.Location = new Point(10, 38);
            this.journalfilterdatagrid.Width    = gridW - 20;
            this.journalfilterdatagrid.Height   = gridH;
            this.journalfilterdatagrid.BackgroundColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.journalfilterdatagrid.GridColor       = Assistant.UI.Controls.RazorTheme.Colors.CardDark;
            this.journalfilterdatagrid.DefaultCellStyle.BackColor  = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.journalfilterdatagrid.DefaultCellStyle.ForeColor  = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            this.journalfilterdatagrid.ColumnHeadersDefaultCellStyle.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.journalfilterdatagrid.ColumnHeadersDefaultCellStyle.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
            this.journalfilterdatagrid.EnableHeadersVisualStyles = false;
            this.journalfilterdatagrid.ScrollBars = ScrollBars.Both;

            flow.Controls.AddRange(new Control[] { cMsgs, pMore, pGrid });
        }

        /// <summary>
        /// Creates a self-painted Panel that looks like a RazorCard (same background,
        /// rounded corners, coloured left bar, title) but extends Panel instead of
        /// GroupBox, so child scrollable controls (DataGridView, CheckedListBox …)
        /// are never clipped and their input events work correctly.
        /// </summary>
        private Panel MakePseudoCard(string iconAndTitle, int width, int height, Color accentColor)
        {
            var p = new Panel {
                Width     = width,
                Height    = height,
                Margin    = new Padding(10),
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.CardDark
            };
            // Store accent for paint
            p.Tag = accentColor;
            p.Paint += (s, e) => {
                var g     = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Color bg = Assistant.UI.Controls.RazorTheme.Colors.CardDark;
                Color ac = p.Tag is Color c ? c : ColorTranslator.FromHtml("#8B5CF6");

                // Rounded rect background
                var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                int r = 12;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath()) {
                    path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
                    path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
                    path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                    path.CloseFigure();
                    using (var brush = new SolidBrush(bg))
                        g.FillPath(brush, path);
                }
                // Accent bar
                using (var brush = new SolidBrush(ac))
                    g.FillRectangle(brush, 0, r + 8, 4, Math.Max(p.Height - r * 2 - 16, 12));

                // Title (icon + text)
                int sep = iconAndTitle.IndexOf("  ");
                string iconPart  = sep > 0 ? iconAndTitle.Substring(0, sep)      : "";
                string titlePart = sep > 0 ? iconAndTitle.Substring(sep + 2)     : iconAndTitle;
                Color fg = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
                using (var iFont = new Font("Segoe MDL2 Assets", 11f))
                using (var tFont = new Font(p.Font.FontFamily, 10f, FontStyle.Bold)) {
                    Size iconSz = TextRenderer.MeasureText(g, iconPart, iFont, new Size(40, 28), TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, iconPart,  iFont,  new Point(14, 10), fg, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, titlePart, tFont, new Point(14 + iconSz.Width + 4, 11), fg, TextFormatFlags.NoPadding);
                }
            };
            return p;
        }


        private void RebuildTargettingFilterTab()
        {
            this.targettingTab.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.targettingTab.Controls.Clear();

            // ── Header ───────────────────────────────────────────────────────
            Panel headerPanel = new Panel {
                Dock = DockStyle.Top, Height = 90,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark
            };
            headerPanel.Controls.Add(new Label {
                Text = LanguageHelper.GetString("MainForm.targettingTab.Text"),
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(18F, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(24, 16)
            });
            this.targettingTab.Controls.Add(headerPanel);

            // ── Scrollable body ───────────────────────────────────────────────
            Panel scrollWrapper = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(0)
            };
            FlowLayoutPanel flow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(12, 8, 12, 8),
                WrapContents = true
            };
            flow.HandleCreated += (s2, e2) => {
                try { SetWindowTheme(flow.Handle, "DarkMode_Explorer", null); } catch { }
            };
            scrollWrapper.Controls.Add(flow);
            this.targettingTab.Controls.Add(scrollWrapper);
            scrollWrapper.BringToFront();

            int cw = 340;
            Color accent = ColorTranslator.FromHtml("#8B5CF6"); // same as Options tab

            // ── Card: Shortcut ────────────────────────────────────────────────
            int scW = 210;
            var cShortcut = BuildCard("\uE8C1", LanguageHelper.GetString("MainForm.groupBox43.Text"), scW, accent);
            int curYs = cShortcut.Tag is int ts ? ts : 38;

            // Name label + textbox row
            var lblName = new Label { Text = LanguageHelper.GetString("MainForm.label76.Text"),
                Location = new Point(10, curYs - 6), Width = 50, Height = 26,
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F), ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft };
            cShortcut.Controls.Add(lblName);
            this.targetaddTextBox.Parent = cShortcut;
            this.targetaddTextBox.Location = new Point(60, curYs - 4);
            this.targetaddTextBox.Width = scW - 74;
            this.targetaddTextBox.Height = 22;
            this.targetaddTextBox.BackColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.targetaddTextBox.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            curYs += 30;

            // Add / Remove buttons row
            this.targetaddButton.Parent = cShortcut;
            this.targetaddButton.Location = new Point(10, curYs - 6);
            this.targetaddButton.Size = new Size(80, 26);
            this.targetaddButton.Text = LanguageHelper.GetString("MainForm.targetaddButton.Text");
            this.targetremoveButton.Parent = cShortcut;
            this.targetremoveButton.Location = new Point(100, curYs - 6);
            this.targetremoveButton.Size = new Size(80, 26);
            this.targetremoveButton.Text = LanguageHelper.GetString("MainForm.targetremoveButton.Text");
            curYs += 34;

            // List box
            this.targetlistBox.Parent = cShortcut;
            this.targetlistBox.Location = new Point(10, curYs - 6);
            this.targetlistBox.Width = scW - 24;
            this.targetlistBox.Height = 140;
            this.targetlistBox.BackColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.targetlistBox.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            this.targetlistBox.BorderStyle = BorderStyle.None;
            curYs += 146;
            cShortcut.Tag = curYs;
            FinalizeCard(cShortcut);

            // ── Card: Filtro Corpo ────────────────────────────────────────────
            var cBody = BuildCard("\uE7B5", LanguageHelper.GetString("MainForm.groupBox44.Text"), cw, accent);
            AddRow(cBody, this.targetbodyCheckBox, LanguageHelper.GetString("MainForm.targetbodyCheckBox.Text"));
            int curYb = cBody.Tag is int tb ? tb : 38;
            this.targetbodydataGridView.Parent = cBody;
            this.targetbodydataGridView.Location = new Point(10, curYb - 6);
            this.targetbodydataGridView.Width  = cw - 24;
            this.targetbodydataGridView.Height = 80;
            this.targetbodydataGridView.BackgroundColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.targetbodydataGridView.GridColor       = Assistant.UI.Controls.RazorTheme.Colors.CardDark;
            this.targetbodydataGridView.DefaultCellStyle.BackColor  = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.targetbodydataGridView.DefaultCellStyle.ForeColor  = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            this.targetbodydataGridView.ColumnHeadersDefaultCellStyle.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.targetbodydataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
            this.targetbodydataGridView.EnableHeadersVisualStyles = false;
            cBody.Tag = curYb + 86;

            // Choose body button
            this.targetChoseBody.Parent = cBody;
            int curYb2 = cBody.Tag is int tb2 ? tb2 : 38;
            this.targetChoseBody.Location = new Point(10, curYb2 - 6);
            this.targetChoseBody.Size = new Size(160, 28);
            this.targetChoseBody.Text = LanguageHelper.GetString("MainForm.targetChoseBody.Text");
            // Style as outline (secondary action)
            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.targetChoseBody);
            cBody.Tag = curYb2 + 36;
            FinalizeCard(cBody);

            // ── Card: Filtro Colore ───────────────────────────────────────────
            var cColor = BuildCard("\uE790", LanguageHelper.GetString("MainForm.groupBox45.Text"), cw, accent);
            AddRow(cColor, this.targetcoloCheckBox, LanguageHelper.GetString("MainForm.targetcoloCheckBox.Text"));
            int curYc = cColor.Tag is int tc ? tc : 38;

            this.targetChoseHue.Parent = cColor;
            this.targetChoseHue.Location = new Point(10, curYc - 6);
            this.targetChoseHue.Size = new Size(160, 28);
            this.targetChoseHue.Text = LanguageHelper.GetString("MainForm.targetChoseHue.Text");
            // Style as outline (secondary action)
            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.targetChoseHue);
            cColor.Tag = curYc + 36;
            FinalizeCard(cColor);

            // ── Card: Raggio ──────────────────────────────────────────────────
            var cRange = BuildCard("\uE81D", LanguageHelper.GetString("MainForm.groupBox48.Text"), cw, accent);
            AddIndentRow(cRange, this.targetRangeMinTextBox, LanguageHelper.GetString("MainForm.label75.Text"), 60);
            AddIndentRow(cRange, this.targetRangeMaxTextBox, LanguageHelper.GetString("MainForm.label74.Text"), 60);
            // hint label
            int curYr = cRange.Tag is int tr ? tr : 38;
            var lblHint = new Label {
                Text = LanguageHelper.GetString("MainForm.label73.Text"),
                Location = new Point(10, curYr - 6), Width = cw - 24, Height = 26,
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(8F),
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft
            };
            cRange.Controls.Add(lblHint);
            cRange.Tag = curYr + 26;
            FinalizeCard(cRange);

            // ── Card: Notorietà ───────────────────────────────────────────────
            var cNot = BuildCard("\uE902", LanguageHelper.GetString("MainForm.groupBox57.Text"), cw, accent);
            AddRow(cNot, this.targetBlueCheckBox,     LanguageHelper.GetString("MainForm.targetBlueCheckBox.Text"));
            AddRow(cNot, this.targetGreenCheckBox,    LanguageHelper.GetString("MainForm.targetGreenCheckBox.Text"));
            AddRow(cNot, this.targetGreyCheckBox,     LanguageHelper.GetString("MainForm.targetGreyCheckBox.Text"));
            AddRow(cNot, this.targetCriminalCheckBox, LanguageHelper.GetString("MainForm.targetCriminalCheckBox.Text"));
            AddRow(cNot, this.targetOrangeCheckBox,   LanguageHelper.GetString("MainForm.targetOrangeCheckBox.Text"));
            AddRow(cNot, this.targetRedCheckBox,      LanguageHelper.GetString("MainForm.targetRedCheckBox.Text"));
            AddRow(cNot, this.targetYellowCheckBox,   LanguageHelper.GetString("MainForm.targetYellowCheckBox.Text"));
            FinalizeCard(cNot);

            // ── Card: Flag ────────────────────────────────────────────────────
            var cFlags = BuildCard("\uE9D9", LanguageHelper.GetString("MainForm.groupBox46.Text"), cw, accent);

            // Helper: adds 3 radio buttons (Sì / No / Entrambi) horizontally on one row after a section label.
            Action<string, RadioButton, RadioButton, RadioButton> addFlagGroup = (groupLabel, rbOn, rbOff, rbBoth) => {
                AddSectionLabel(cFlags, groupLabel);
                if (rbOn == null && rbOff == null && rbBoth == null) return;
                int cardW3 = cFlags.Width > 0 ? cFlags.Width : 340;
                int curY3  = cFlags.Tag is int ct3 ? ct3 : 38;
                int rowH3  = 28;
                int colW3  = (cardW3 - 20) / 3;
                void PlaceRb(RadioButton rb, string lbl, int colX) {
                    if (rb == null) return;
                    rb.Parent    = cFlags;
                    rb.Text      = lbl;
                    rb.Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F);
                    rb.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
                    rb.BackColor = Color.Transparent;
                    rb.AutoSize  = false;
                    rb.Size      = new Size(colW3, rowH3);
                    rb.Location  = new Point(10 + colX, curY3 - 4);
                }
                PlaceRb(rbOn,   LanguageHelper.GetString("MainForm.paralizedOn.Text"),   0);
                PlaceRb(rbOff,  LanguageHelper.GetString("MainForm.paralizedOff.Text"),  colW3);
                PlaceRb(rbBoth, LanguageHelper.GetString("MainForm.paralizedBoth.Text"), colW3 * 2);
                cFlags.Tag = curY3 + rowH3 + 4;
            };

            addFlagGroup(LanguageHelper.GetString("MainForm.groupBox54.Text"), this.poisonedOn,  this.poisonedOff,  this.poisonedBoth);
            addFlagGroup(LanguageHelper.GetString("MainForm.groupBox53.Text"), this.blessedOn,   this.blessedOff,   this.blessedBoth);
            addFlagGroup(LanguageHelper.GetString("MainForm.groupBox52.Text"), this.humanOn,     this.humanOff,     this.humanBoth);
            addFlagGroup(LanguageHelper.GetString("MainForm.groupBox51.Text"), this.ghostOn,     this.ghostOff,     this.ghostBoth);
            addFlagGroup(LanguageHelper.GetString("MainForm.groupBox50.Text"), this.warmodeOn,   this.warmodeOff,   this.warmodeBoth);
            addFlagGroup(LanguageHelper.GetString("MainForm.groupBox49.Text"), this.friendOn,    this.friendOff,    this.friendBoth);
            addFlagGroup(LanguageHelper.GetString("MainForm.groupBox47.Text"), this.paralizedOn, this.paralizedOff, this.paralizedBoth);
            FinalizeCard(cFlags);


            // ── Card: Selettore + Bersaglio ───────────────────────────────────
            var cSel = BuildCard("\uE721", LanguageHelper.GetString("MainForm.groupBox56.Text"), cw, accent);
            AddIndentRow(cSel, this.targetSelectorComboBox, LanguageHelper.GetString("MainForm.groupBox56.Text"), 180);
            AddIndentRow(cSel, this.targetNameTextBox, LanguageHelper.GetString("MainForm.groupBox55.Text"), 180);

            // Test + Save buttons
            int curYsel = cSel.Tag is int tsel ? tsel : 38;
            this.targetTestButton.Parent = cSel;
            this.targetTestButton.Location = new Point(10, curYsel - 6);
            this.targetTestButton.Size = new Size(120, 26);
            this.targetTestButton.Text = LanguageHelper.GetString("MainForm.targetTestButton.Text");
            this.targetsaveButton.Parent = cSel;
            this.targetsaveButton.Location = new Point(140, curYsel - 6);
            this.targetsaveButton.Size = new Size(130, 26);
            this.targetsaveButton.Text = LanguageHelper.GetString("MainForm.targetsaveButton.Text");
            cSel.Tag = curYsel + 34;
            FinalizeCard(cSel);

            flow.Controls.AddRange(new Control[] {
                cShortcut, cBody, cColor, cRange, cNot, cFlags, cSel
            });
        }

        private void RebuildMiscFilterTab()
        {
            this.MiscFilterPage.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.MiscFilterPage.Controls.Clear();

            // ── Header ───────────────────────────────────────────────────────
            Panel headerPanel = new Panel {
                Dock = DockStyle.Top, Height = 90,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark
            };
            headerPanel.Controls.Add(new Label {
                Text = LanguageHelper.GetString("MainForm.MiscFilterPage.Text"),
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(18F, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(24, 16)
            });
            this.MiscFilterPage.Controls.Add(headerPanel);

            // ── Scrollable body ───────────────────────────────────────────────
            Panel scrollWrapper = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(0)
            };
            FlowLayoutPanel flow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(12, 8, 12, 8),
                WrapContents = true
            };
            flow.HandleCreated += (s2, e2) => {
                try { SetWindowTheme(flow.Handle, "DarkMode_Explorer", null); } catch { }
            };
            scrollWrapper.Controls.Add(flow);
            this.MiscFilterPage.Controls.Add(scrollWrapper);
            scrollWrapper.BringToFront();

            int cw = 340;
            Color accent = ColorTranslator.FromHtml("#8B5CF6"); // same as Options tab

            // ── Card: Varie (scrollable pseudo-card so it doesn't dominate layout) ──
            // Build as a pseudo-card so we can host a scrollable inner Panel.
            const int miscMaxH = 290;
            // Compute full content height (14 rows × 44px)
            int miscContentH = 14 * 44 + 20;
            var pMisc = MakePseudoCard("\uE115  " + LanguageHelper.GetString("MainForm.groupBox24.Text"),
                                       cw, miscMaxH, accent);
            // Scrollable inner panel
            var miscScrollPanel = new Panel {
                Location    = new Point(8, 38),
                Width       = cw - 16,
                Height      = miscMaxH - 46,
                AutoScroll  = true,
                BackColor   = Assistant.UI.Controls.RazorTheme.Colors.CardDark,
                BorderStyle = BorderStyle.None
            };
            miscScrollPanel.HandleCreated += (s2, e2) => {
                try { SetWindowTheme(miscScrollPanel.Handle, "DarkMode_Explorer", null); } catch { }
            };
            pMisc.Controls.Add(miscScrollPanel);
            // Build toggle rows inside the scroll panel using a temporary card-like shim
            // We use a Panel as a layout proxy (same approach as AddRow but targeting miscScrollPanel)
            int miscRowY = 2;
            int miscRowW = miscScrollPanel.Width - 4;
            int miscRowH = 40;
            Action<System.Windows.Forms.CheckBox, string> addMiscRow = (toggle, text) => {
                if (toggle == null) return;
                toggle.Text      = "";
                toggle.Size      = new Size(44, 22);
                toggle.Location  = new Point(miscRowW - 48, miscRowY + (miscRowH - 22) / 2);
                toggle.BackColor = Color.Transparent;
                toggle.Parent    = miscScrollPanel;
                var lbl = new Label {
                    Text      = text,
                    Location  = new Point(4, miscRowY),
                    Width     = miscRowW - 56,
                    Height    = miscRowH,
                    Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9.5F),
                    ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                miscScrollPanel.Controls.Add(lbl);
                toggle.BringToFront();
                // Subtle separator
                var line = new Panel {
                    Location  = new Point(2, miscRowY + miscRowH - 1),
                    Width     = miscRowW - 4,
                    Height    = 1,
                    BackColor = Color.FromArgb(30, 255, 255, 255)
                };
                miscScrollPanel.Controls.Add(line);
                miscRowY += miscRowH;
            };
            addMiscRow(this.highlighttargetCheckBox,         LanguageHelper.GetString("MainForm.highlighttargetCheckBox.Text"));
            addMiscRow(this.flagsHighlightCheckBox,          LanguageHelper.GetString("MainForm.flagsHighlightCheckBox.Text"));
            addMiscRow(this.colorflagsHighlightCheckBox,     LanguageHelper.GetString("MainForm.colorflagsHighlightCheckBox.Text"));
            addMiscRow(this.colorflagsselfHighlightCheckBox, LanguageHelper.GetString("MainForm.colorflagsselfHighlightCheckBox.Text"));
            addMiscRow(this.showstaticfieldCheckBox,         LanguageHelper.GetString("MainForm.showstaticfieldCheckBox.Text"));
            addMiscRow(this.showmessagefieldCheckBox,        LanguageHelper.GetString("MainForm.showmessagefieldCheckBox.Text"));
            addMiscRow(this.blocktraderequestCheckBox,       LanguageHelper.GetString("MainForm.blocktraderequestCheckBox.Text"));
            addMiscRow(this.blockpartyinviteCheckBox,        LanguageHelper.GetString("MainForm.blockpartyinviteCheckBox.Text"));
            addMiscRow(this.showheadtargetCheckBox,          LanguageHelper.GetString("MainForm.showheadtargetCheckBox.Text"));
            addMiscRow(this.blockhealpoisonCheckBox,         LanguageHelper.GetString("MainForm.blockhealpoisonCheckBox.Text"));
            addMiscRow(this.blockminihealCheckBox,           LanguageHelper.GetString("MainForm.blockminihealCheckBox.Text"));
            addMiscRow(this.blockbighealCheckBox,            LanguageHelper.GetString("MainForm.blockbighealCheckBox.Text"));
            addMiscRow(this.blockchivalryhealCheckBox,       LanguageHelper.GetString("MainForm.blockchivalryhealCheckBox.Text"));
            addMiscRow(this.showagentmessageCheckBox,        LanguageHelper.GetString("MainForm.showagentmessageCheckBox.Text"));
            var cMisc = pMisc; // alias for consistency with AddRange below

            // ── Card: Cambio Grafica Mobile ───────────────────────────────────
            var cMob = BuildCard("\uE8FD", LanguageHelper.GetString("MainForm.groupBox23.Text"), cw, accent);
            AddRow(cMob, this.mobfilterCheckBox, LanguageHelper.GetString("MainForm.mobfilterCheckBox.Text"));
            int curYm = cMob.Tag is int tm ? tm : 38;
            this.graphfilterdatagrid.Parent = cMob;
            this.graphfilterdatagrid.Location = new Point(10, curYm - 6);
            this.graphfilterdatagrid.Width  = cw - 24;
            this.graphfilterdatagrid.Height = 120;
            this.graphfilterdatagrid.BackgroundColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.graphfilterdatagrid.GridColor       = Assistant.UI.Controls.RazorTheme.Colors.CardDark;
            this.graphfilterdatagrid.DefaultCellStyle.BackColor  = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            this.graphfilterdatagrid.DefaultCellStyle.ForeColor  = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            this.graphfilterdatagrid.ColumnHeadersDefaultCellStyle.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.graphfilterdatagrid.ColumnHeadersDefaultCellStyle.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
            this.graphfilterdatagrid.EnableHeadersVisualStyles = false;
            cMob.Tag = curYm + 126;
            FinalizeCard(cMob);

            // ── Card: Auto Carver ─────────────────────────────────────────────
            var cCarver = BuildCard("\uE8B8", LanguageHelper.GetString("MainForm.groupBox10.Text"), cw, accent);
            AddRow(cCarver, this.autocarverCheckBox, LanguageHelper.GetString("MainForm.autocarverCheckBox.Text"));
            int curYcv = cCarver.Tag is int tcv ? tcv : 38;
            // serial label + set button row
            var lblCvSerial = new Label { Text = LanguageHelper.GetString("MainForm.label34.Text"),
                Location = new Point(10, curYcv - 6), Width = 90, Height = 32,
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F), ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft };
            cCarver.Controls.Add(lblCvSerial);
            this.autocarverbladeLabel.Parent = cCarver;
            this.autocarverbladeLabel.Location = new Point(100, curYcv - 3);
            this.autocarverbladeLabel.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
            this.autocarverrazorButton.Parent = cCarver;
            this.autocarverrazorButton.Location = new Point(cw - 100, curYcv - 5);
            this.autocarverrazorButton.Size = new Size(80, 26);
            this.autocarverrazorButton.Text = LanguageHelper.GetString("MainForm.autocarverrazorButton.Text");
            cCarver.Tag = curYcv + 36;
            FinalizeCard(cCarver);

            // ── Card: Taglia Ossa ─────────────────────────────────────────────
            var cBone = BuildCard("\uE8B8", LanguageHelper.GetString("MainForm.groupBox9.Text"), cw, accent);
            AddRow(cBone, this.bonecutterCheckBox, LanguageHelper.GetString("MainForm.bonecutterCheckBox.Text"));
            int curYbn = cBone.Tag is int tbn ? tbn : 38;
            var lblBnSerial = new Label { Text = LanguageHelper.GetString("MainForm.label16.Text"),
                Location = new Point(10, curYbn - 6), Width = 90, Height = 32,
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F), ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft };
            cBone.Controls.Add(lblBnSerial);
            this.bonebladeLabel.Parent = cBone;
            this.bonebladeLabel.Location = new Point(100, curYbn - 3);
            this.bonebladeLabel.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
            this.boneCutterrazorButton.Parent = cBone;
            this.boneCutterrazorButton.Location = new Point(cw - 100, curYbn - 5);
            this.boneCutterrazorButton.Size = new Size(80, 26);
            this.boneCutterrazorButton.Text = LanguageHelper.GetString("MainForm.boneCutterrazorButton.Text");
            cBone.Tag = curYbn + 36;
            FinalizeCard(cBone);

            // ── Card: Auto Rimonta ────────────────────────────────────────────
            var cRemount = BuildCard("\uE806", LanguageHelper.GetString("MainForm.groupBox32.Text"), cw, accent);
            AddRow(cRemount, this.remountcheckbox, LanguageHelper.GetString("MainForm.remountcheckbox.Text"));
            int curYrm = cRemount.Tag is int trm ? trm : 38;
            // Serial label + Set Mount button
            var lblRmSerial = new Label { Text = LanguageHelper.GetString("MainForm.label47.Text"),
                Location = new Point(10, curYrm - 6), Width = 90, Height = 32,
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F), ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft };
            cRemount.Controls.Add(lblRmSerial);
            this.remountseriallabel.Parent = cRemount;
            this.remountseriallabel.Location = new Point(100, curYrm - 3);
            this.remountseriallabel.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
            this.remountsetbutton.Parent = cRemount;
            this.remountsetbutton.Location = new Point(cw - 100, curYrm - 5);
            this.remountsetbutton.Size = new Size(80, 26);
            this.remountsetbutton.Text = LanguageHelper.GetString("MainForm.remountsetbutton.Text");
            cRemount.Tag = curYrm + 36;
            AddIndentRow(cRemount, this.remountdelay,  LanguageHelper.GetString("MainForm.label40.Text"), 60);
            AddIndentRow(cRemount, this.remountedelay, LanguageHelper.GetString("MainForm.label48.Text"), 60);
            FinalizeCard(cRemount);

            // ── Card: Visualizzazione Danno ───────────────────────────────────
            var cDmg = BuildCard("\uE9E9", LanguageHelper.GetString("MainForm.DmgDsplyGroup.Text"), cw, accent);
            AddRow(cDmg, this.limitDamageDisplayEnable, LanguageHelper.GetString("MainForm.limitDamageDisplayEnable.Text"));
            AddIndentRow(cDmg, this.minDmgShown, LanguageHelper.GetString("MainForm.label81.Text"), 60);
            FinalizeCard(cDmg);

            // ── Card: UoMod ───────────────────────────────────────────────────
            var cUoMod = BuildCard("\uE9B0", LanguageHelper.GetString("MainForm.uomodgroupbox.Text"), cw, accent);
            AddRow(cUoMod, this.uomodFPSCheckBox,        LanguageHelper.GetString("MainForm.uomodFPSCheckBox.Text"));
            AddRow(cUoMod, this.uomodpaperdollCheckBox,  LanguageHelper.GetString("MainForm.uomodpaperdollCheckBox.Text"));
            AddRow(cUoMod, this.uomodglobalsoundCheckBox,LanguageHelper.GetString("MainForm.uomodglobalsoundCheckBox.Text"));
            FinalizeCard(cUoMod);

            flow.Controls.AddRange(new Control[] {
                cMisc, cMob, cCarver, cBone, cRemount, cDmg, cUoMod
            });
        }

        private void RebuildOptionsTab()
        {
            this.moreOptTab.Text = LanguageHelper.GetString("MainForm.moreOptTab.Text");
            this.moreOptTab.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
            this.moreOptTab.Controls.Clear();

            // â”€â”€ Header â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            Panel headerPanel = new Panel {
                Dock = DockStyle.Top, Height = 90,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark
            };
            Label lblTitle = new Label {
                Text = this.moreOptTab.Text,
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(18F, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(24, 16)
            };
            Label lblSub = new Label {
                Text = LanguageHelper.GetString("MainForm.generalTab.Text"),
                Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(10F),
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                AutoSize = true, Location = new Point(24, 55)
            };
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSub);
            this.moreOptTab.Controls.Add(headerPanel);

            // ―― Scrollable body ─────────────────────────────────────────────────
            // Wrap FlowLayoutPanel in a dark Panel so the Win32 scrollbar
            // lies inside the dark area and doesn't show a white border.
            Panel scrollWrapper = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(0)
            };
            FlowLayoutPanel flow = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                Padding = new Padding(12, 8, 12, 8),
                WrapContents = true
            };
            // Apply dark scrollbar via UxTheme (Windows 10/11)
            flow.HandleCreated += (s2, e2) => {
                try { SetWindowTheme(flow.Handle, "DarkMode_Explorer", null); } catch { }
            };
            scrollWrapper.Controls.Add(flow);
            this.moreOptTab.Controls.Add(scrollWrapper);
            scrollWrapper.BringToFront();

            // Card width so two fit side-by-side with comfortable margin
            int cw = 340;
            Color accent = ColorTranslator.FromHtml("#8B5CF6");

            // ―― 1. Bersagli ──────────────────────────────────────────────────
            var cTargets = BuildCard(
                "\uE909", LanguageHelper.GetString("MainForm.targetGroupBox.Text"),
                cw, accent);
            AddRow(cTargets, this.smartLT,      LanguageHelper.GetString("MainForm.smartLT.Text"));
            AddRow(cTargets, this.rangeCheckLT, LanguageHelper.GetString("MainForm.rangeCheckLT.Text"));
            AddIndentRow(cTargets, this.ltRange, LanguageHelper.GetString("MainForm.label8.Text"), 60);
            AddRow(cTargets, this.showtargtext, LanguageHelper.GetString("MainForm.showtargtext.Text"));
            FinalizeCard(cTargets);

            // ―― 2. Code / Queue ──────────────────────────────────────────────────
            var cQueues = BuildCard(
                "\uE965", LanguageHelper.GetString("MainForm.queueGroupBox.Text"),
                cw, accent);
            AddRow(cQueues, this.actionStatusMsg, LanguageHelper.GetString("MainForm.actionStatusMsg.Text"));
            AddRow(cQueues, this.QueueActions,    LanguageHelper.GetString("MainForm.QueueActions.Text"));
            AddIndentRow(cQueues, this.txtObjDelay, LanguageHelper.GetString("MainForm.label5.Text"), 60);
            AddRow(cQueues, this.queueTargets,   LanguageHelper.GetString("MainForm.queueTargets.Text"));
            FinalizeCard(cQueues);

            // ―― 3. Visuale ──────────────────────────────────────────────────
            var cVisuals = BuildCard(
                "\uE7B3", LanguageHelper.GetString("MainForm.showmobileGroupBox.Text"),
                cw, accent);
            AddRow(cVisuals, this.incomingMob,    LanguageHelper.GetString("MainForm.incomingMob.Text"));
            AddRow(cVisuals, this.incomingCorpse, LanguageHelper.GetString("MainForm.incomingCorpse.Text"));
            AddRow(cVisuals, this.showHealthOH,   LanguageHelper.GetString("MainForm.showHealthOH.Text"));
            AddIndentRow(cVisuals, this.healthFmt, LanguageHelper.GetString("MainForm.label10.Text"), 120);
            AddRow(cVisuals, this.chkPartyOverhead, LanguageHelper.GetString("MainForm.chkPartyOverhead.Text"));
            AddSectionLabel(cVisuals, LanguageHelper.GetString("MainForm.preaosstatusGroupBox.Text"));
            AddRow(cVisuals, this.preAOSstatbar, LanguageHelper.GetString("MainForm.preAOSstatbar.Text"));
            FinalizeCard(cVisuals);

            // ―― 4. Overrides ──────────────────────────────────────────────────
            var cOverrides = BuildCard(
                "\uE932", LanguageHelper.GetString("MainForm.overrideGroupBox.Text"),
                cw, accent);
            // Message / Warn hue rows
            AddColorHueRow(cOverrides,
                this.lblMsgHue,  this.setMsgHue,
                LanguageHelper.GetString("MainForm.lblMsgHue.Text"));
            AddColorHueRow(cOverrides,
                this.lblWarnHue, this.setWarnHue,
                LanguageHelper.GetString("MainForm.lblWarnHue.Text"));
            AddSectionLabel(cOverrides, LanguageHelper.GetString("MainForm.miscellaneousGroupBox.Text"));
            AddRow(cOverrides, this.chkForceSpeechHue, LanguageHelper.GetString("MainForm.chkForceSpeechHue.Text"));
            AddRow(cOverrides, this.lthilight,          LanguageHelper.GetString("MainForm.lthilight.Text"));
            AddRow(cOverrides, this.chkForceSpellHue,   LanguageHelper.GetString("MainForm.chkForceSpellHue.Text"));
            // ── Spell hue rows – visible only when chkForceSpellHue is checked ──────
            int _compactTagY = cOverrides.Tag is int _cty ? _cty : 38;
            int _preCount    = cOverrides.Controls.Count;

            AddSectionLabel(cOverrides, LanguageHelper.GetString("MainForm.overrideGroupBox.Text") + " " + LanguageHelper.GetString("MainForm.spellspotionsGroupBox.Text"));
            AddSpellHueTriple(cOverrides,
                this.lblBeneHue, this.setBeneHue, LanguageHelper.GetString("MainForm.lblBeneHue.Text"), Color.FromArgb(100, 220, 120),
                this.lblHarmHue, this.setHarmHue, LanguageHelper.GetString("MainForm.lblHarmHue.Text"), Color.FromArgb(220, 100, 100),
                this.lblNeuHue,  this.setNeuHue,  LanguageHelper.GetString("MainForm.lblNeuHue.Text"),  Color.FromArgb(170, 170, 170));

            FinalizeCard(cOverrides);
            int _expandedH = cOverrides.Height;

            // Compute compact height (no spell-hue sub-section)
            cOverrides.Tag = _compactTagY;
            FinalizeCard(cOverrides);
            int _compactH  = cOverrides.Height;

            // Collect the controls that belong to the spell-hue sub-section
            var _spellHueCtrls = new System.Collections.Generic.List<Control>();
            for (int _i = _preCount; _i < cOverrides.Controls.Count; _i++)
                _spellHueCtrls.Add(cOverrides.Controls[_i]);

            // Apply initial visibility state
            bool _initShow = this.chkForceSpellHue?.Checked ?? false;
            foreach (var _c in _spellHueCtrls) _c.Visible = _initShow;
            cOverrides.Height = _initShow ? _expandedH : _compactH;

            // Re-register toggle handler (remove stale one from previous rebuild)
            if (_spellHueVisibilityHandler != null)
                this.chkForceSpellHue.CheckedChanged -= _spellHueVisibilityHandler;
            _spellHueVisibilityHandler = (s, ev) =>
            {
                bool _show = this.chkForceSpellHue?.Checked ?? false;
                foreach (var _c in _spellHueCtrls) _c.Visible = _show;
                cOverrides.Height = _show ? _expandedH : _compactH;
            };
            this.chkForceSpellHue.CheckedChanged += _spellHueVisibilityHandler;

            // ―― 5. Contenitori ──────────────────────────────────────────────────
            var cContainers = BuildCard(
                "\uE8B1", LanguageHelper.GetString("MainForm.containeruseGroupBox.Text"),
                cw, accent);
            AddRow(cContainers, this.openCorpses,         LanguageHelper.GetString("MainForm.openCorpses.Text"));
            AddIndentRow(cContainers, this.corpseRange,   LanguageHelper.GetString("MainForm.label4.Text"), 40);
            AddRow(cContainers, this.autosearchcontainers, LanguageHelper.GetString("MainForm.autosearchcontainers.Text"));
            AddRow(cContainers, this.nosearchpouches,     LanguageHelper.GetString("MainForm.nosearchpouches.Text"));
            FinalizeCard(cContainers);

            // ―― 6. Magie / Pozioni ──────────────────────────────────────────────────
            var cSpells = BuildCard(
                "\uE70B", LanguageHelper.GetString("MainForm.spellspotionsGroupBox.Text"),
                cw, accent);
            AddRow(cSpells, this.potionEquip,      LanguageHelper.GetString("MainForm.potionEquip.Text"));
            AddRow(cSpells, this.spellUnequip,     LanguageHelper.GetString("MainForm.spellUnequip.Text"));
            AddRow(cSpells, this.druidClericPackets, LanguageHelper.GetString("MainForm.druidClericPackets.Text"));
            FinalizeCard(cSpells);

            // ―― 7. Stealth ──────────────────────────────────────────────────
            var cStealth = BuildCard(
                "\uE7B8", LanguageHelper.GetString("MainForm.stealthGroupBox.Text"),
                cw, accent);
            AddRow(cStealth, this.chkStealth,      LanguageHelper.GetString("MainForm.chkStealth.Text"));
            AddRow(cStealth, this.chknorunStealth,  LanguageHelper.GetString("MainForm.chknorunStealth.Text"));
            FinalizeCard(cStealth);

            // ―― 8. Varie ──────────────────────────────────────────────────
            var cMisc = BuildCard(
                "\uE115", LanguageHelper.GetString("MainForm.miscellaneousGroupBox.Text"),
                cw, accent);
            AddRow(cMisc, this.blockDis,    LanguageHelper.GetString("MainForm.blockDis.Text"));
            AddRow(cMisc, this.autoStackRes, LanguageHelper.GetString("MainForm.autoStackRes.Text"));
            FinalizeCard(cMisc);

                        flow.Controls.AddRange(new Control[] {
                            cTargets, cQueues, cVisuals, cOverrides, cContainers, cSpells, cStealth, cMisc
                        });
                    }
            
                            private void RebuildStatBarTab()
                            {
                                this.tabPage2.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
                                this.tabPage2.Controls.Clear();
                    
                                // ―― Scrollable body ─────────────────────────────────────────────────
                                Panel scrollWrapper = new Panel
                                {
                                    Dock = DockStyle.Fill,
                                    BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                                    Padding = new Padding(0)
                                };
                                FlowLayoutPanel flow = new FlowLayoutPanel
                                {
                                    Dock = DockStyle.Fill,
                                    AutoScroll = true,
                                    BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                                    Padding = new Padding(12, 8, 12, 8),
                                    WrapContents = true
                                };
                                flow.HandleCreated += (s2, e2) =>
                                {
                                    try { SetWindowTheme(flow.Handle, "DarkMode_Explorer", null); } catch { }
                                };
                                scrollWrapper.Controls.Add(flow);
                                this.tabPage2.Controls.Add(scrollWrapper);
                                scrollWrapper.BringToFront();

                                int cw = 290; // due colonne: 2*(290+20)=620 ≤ 628px disponibili
                                Color accent = ColorTranslator.FromHtml("#8B5CF6");

                                // ―― 1. Generale ──────────────────────────────────────────────────
                                var cGeneral = BuildCard("\uE713", LanguageHelper.GetString("MainForm.groupBox25.Text"), cw, accent);
                                AddRow(cGeneral, this.lockToolBarCheckBox, LanguageHelper.GetString("MainForm.lockToolBarCheckBox.Text"));
                                AddRow(cGeneral, this.autoopenToolBarCheckBox, LanguageHelper.GetString("MainForm.autoopenToolBarCheckBox.Text"));
                    
                                int curY = (int)cGeneral.Tag;
                                this.locationToolBarLabel.Parent = cGeneral;
                                this.locationToolBarLabel.Location = new Point(12, curY + 6);
                                this.locationToolBarLabel.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
                                this.locationToolBarLabel.Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F);
                                this.locationToolBarLabel.AutoSize = true;
                    
                                this.openToolBarButton.Parent = cGeneral;
                                this.openToolBarButton.Location = new Point(cw - 185, curY + 2);
                                this.openToolBarButton.Size = new Size(80, 26);
                                this.openToolBarButton.Text = LanguageHelper.GetString("MainForm.openToolBarButton.Text");
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.openToolBarButton);
                    
                                this.closeToolBarButton.Parent = cGeneral;
                                this.closeToolBarButton.Location = new Point(cw - 95, curY + 2);
                                this.closeToolBarButton.Size = new Size(80, 26);
                                this.closeToolBarButton.Text = LanguageHelper.GetString("MainForm.closeToolBarButton.Text");
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.closeToolBarButton);
                    
                                cGeneral.Tag = curY + 44;
                                FinalizeCard(cGeneral);
                    
                                // ―― 2. Layout ────────────────────────────────────────────────────
                                var cLayout = BuildCard("\uE9B0", LanguageHelper.GetString("MainForm.groupBox4.Text"), cw, accent);
                                AddIndentRow(cLayout, this.toolboxstyleComboBox, LanguageHelper.GetString("MainForm.label2.Text"), 120);
                                AddIndentRow(cLayout, this.toolboxsizeComboBox, LanguageHelper.GetString("MainForm.label41.Text"), 120);
                    
                                curY = (int)cLayout.Tag;
                                var lblSlots = new Label
                                {
                                    Text = LanguageHelper.GetString("MainForm.label43.Text"),
                                    Location = new Point(20 - 8, curY - 6),
                                    Width = 100,
                                    Height = 36,
                                    Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F),
                                    ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                                    BackColor = Color.Transparent,
                                    TextAlign = ContentAlignment.MiddleLeft
                                };
                                cLayout.Controls.Add(lblSlots);
                    
                                this.toolbarslot_label.Parent = cLayout;
                                this.toolbarslot_label.Location = new Point(110, curY + 5);
                                this.toolbarslot_label.ForeColor = Color.White;
                                this.toolbarslot_label.AutoSize = true;
                    
                                this.toolbaraddslotButton.Parent = cLayout;
                                this.toolbaraddslotButton.Location = new Point(cw - 85, curY + 2);
                                this.toolbaraddslotButton.Size = new Size(30, 26);
                                this.toolbaraddslotButton.Text = "+";
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.toolbaraddslotButton);
                    
                                this.toolbarremoveslotButton.Parent = cLayout;
                                this.toolbarremoveslotButton.Location = new Point(cw - 50, curY + 2);
                                this.toolbarremoveslotButton.Size = new Size(30, 26);
                                this.toolbarremoveslotButton.Text = "-";
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.toolbarremoveslotButton);
                                cLayout.Tag = curY + 40;
                    
                                AddRow(cLayout, this.showhitsToolBarCheckBox, LanguageHelper.GetString("MainForm.showhitsToolBarCheckBox.Text"));
                                AddRow(cLayout, this.showstaminaToolBarCheckBox, LanguageHelper.GetString("MainForm.showstaminaToolBarCheckBox.Text"));
                                AddRow(cLayout, this.showmanaToolBarCheckBox, LanguageHelper.GetString("MainForm.showmanaToolBarCheckBox.Text"));
                                AddRow(cLayout, this.showweightToolBarCheckBox, LanguageHelper.GetString("MainForm.showweightToolBarCheckBox.Text"));
                                AddRow(cLayout, this.showfollowerToolBarCheckBox, LanguageHelper.GetString("MainForm.showfollowerToolBarCheckBox.Text"));
                                AddRow(cLayout, this.showtitheToolBarCheckBox, LanguageHelper.GetString("MainForm.showtitheToolBarCheckBox.Text"));
                                FinalizeCard(cLayout);
                    
                                // ―― 3. Conteggio Oggetti ─────────────────────────────────────────
                                var cCount = BuildCard("\uE8A1", LanguageHelper.GetString("MainForm.groupBox26.Text"), cw, accent);
                                AddIndentRow(cCount, this.toolboxcountComboBox, "Slot:", 120);
                                AddIndentRow(cCount, this.toolboxcountNameTextBox, LanguageHelper.GetString("MainForm.label37.Text"), 180);
                                AddIndentRow(cCount, this.toolboxcountGraphTextBox, LanguageHelper.GetString("MainForm.label18.Text"), 100);
                                AddIndentRow(cCount, this.toolboxcountHueTextBox, LanguageHelper.GetString("MainForm.label35.Text"), 100);
                                AddRow(cCount, this.toolboxcountHueWarningCheckBox, LanguageHelper.GetString("MainForm.toolboxcountHueWarningCheckBox.Text"));
                                AddIndentRow(cCount, this.toolboxcountWarningTextBox, LanguageHelper.GetString("MainForm.label36.Text"), 100);
                    
                                curY = (int)cCount.Tag;
                                this.toolboxcountTargetButton.Parent = cCount;
                                this.toolboxcountTargetButton.Location = new Point(20, curY + 5);
                                this.toolboxcountTargetButton.Size = new Size(120, 30);
                                this.toolboxcountTargetButton.Text = LanguageHelper.GetString("MainForm.toolboxcountTargetButton.Text");
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.toolboxcountTargetButton);
                    
                                this.toolboxcountClearButton.Parent = cCount;
                                this.toolboxcountClearButton.Location = new Point(cw - 140, curY + 5);
                                this.toolboxcountClearButton.Size = new Size(120, 30);
                                this.toolboxcountClearButton.Text = LanguageHelper.GetString("MainForm.toolboxcountClearButton.Text");
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.toolboxcountClearButton);
                                cCount.Tag = curY + 45;
                    
                                FinalizeCard(cCount);
                    
                                // ―― 4. OpacitÃ  ───────────────────────────────────────────────────
                                var cOpacity = BuildCard("\uE7B3", LanguageHelper.GetString("MainForm.groupBox39.Text"), cw, accent);
                                curY = (int)cOpacity.Tag;
                                this.toolbar_opacity_label.Parent = cOpacity;
                                this.toolbar_opacity_label.Location = new Point(20, curY + 5);
                                this.toolbar_opacity_label.ForeColor = Color.White;
                                this.toolbar_opacity_label.AutoSize = true;
                    
                                this.toolbar_trackBar.Parent = cOpacity;
                                this.toolbar_trackBar.Location = new Point(70, curY + 5);
                                this.toolbar_trackBar.Width = cw - 90;
                                this.toolbar_trackBar.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
                                cOpacity.Tag = curY + 44;
                                FinalizeCard(cOpacity);
                    
                                flow.Controls.AddRange(new Control[] { cGeneral, cLayout, cCount, cOpacity });
                            }
            
                            private void RebuildSpellGridTab()
                            {
                                this.tabPage3.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
                                this.tabPage3.Controls.Clear();
                    
                                // ―― Scrollable body ─────────────────────────────────────────────────
                                Panel scrollWrapper = new Panel
                                {
                                    Dock = DockStyle.Fill,
                                    BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                                    Padding = new Padding(0)
                                };
                                FlowLayoutPanel flow = new FlowLayoutPanel
                                {
                                    Dock = DockStyle.Fill,
                                    AutoScroll = true,
                                    BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                                    Padding = new Padding(12, 8, 12, 8),
                                    WrapContents = true
                                };
                                flow.HandleCreated += (s2, e2) =>
                                {
                                    try { SetWindowTheme(flow.Handle, "DarkMode_Explorer", null); } catch { }
                                };
                                scrollWrapper.Controls.Add(flow);
                                this.tabPage3.Controls.Add(scrollWrapper);
                                scrollWrapper.BringToFront();

                                int cw = 290; // due colonne: 2*(290+20)=620 ≤ 628px disponibili
                                Color accent = ColorTranslator.FromHtml("#8B5CF6");

                                // ―― 1. Generale ──────────────────────────────────────────────────
                                var cGeneral = BuildCard("\uE713", LanguageHelper.GetString("MainForm.groupBox35.Text"), cw, accent);
                                AddRow(cGeneral, this.gridlock_CheckBox, LanguageHelper.GetString("MainForm.gridlock_CheckBox.Text"));
                                AddRow(cGeneral, this.gridopenlogin_CheckBox, LanguageHelper.GetString("MainForm.gridopenlogin_CheckBox.Text"));
                    
                                int curY = (int)cGeneral.Tag;
                                this.gridlocation_label.Parent = cGeneral;
                                this.gridlocation_label.Location = new Point(12, curY + 6);
                                this.gridlocation_label.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
                                this.gridlocation_label.Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F);
                                this.gridlocation_label.AutoSize = true;
                    
                                this.gridopen_button.Parent = cGeneral;
                                this.gridopen_button.Location = new Point(cw - 185, curY + 2);
                                this.gridopen_button.Size = new Size(80, 26);
                                this.gridopen_button.Text = LanguageHelper.GetString("MainForm.gridopen_button.Text");
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.gridopen_button);
                    
                                this.gridclose_button.Parent = cGeneral;
                                this.gridclose_button.Location = new Point(cw - 95, curY + 2);
                                this.gridclose_button.Size = new Size(80, 26);
                                this.gridclose_button.Text = LanguageHelper.GetString("MainForm.gridclose_button.Text");
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.gridclose_button);
                    
                                curY += 35;
                                this.setSpellBarOrigin.Parent = cGeneral;
                                this.setSpellBarOrigin.Location = new Point(20, curY);
                                this.setSpellBarOrigin.Size = new Size(cw - 40, 26);
                                this.setSpellBarOrigin.Text = LanguageHelper.GetString("MainForm.setSpellBarOrigin.Text");
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.setSpellBarOrigin);
                    
                                cGeneral.Tag = curY + 40;
                                FinalizeCard(cGeneral);
                    
                                // ―― 2. Layout ────────────────────────────────────────────────────
                                var cLayout = BuildCard("\uE9B0", LanguageHelper.GetString("MainForm.groupBox37.Text"), cw, accent);
                                AddIndentRow(cLayout, this.spellgridstyleComboBox, LanguageHelper.GetString("MainForm.label80.Text"), 120);
                    
                                // Slots V
                                curY = (int)cLayout.Tag;
                                var lblSlotsV = new Label
                                {
                                    Text = LanguageHelper.GetString("MainForm.label49.Text"),
                                    Location = new Point(20 - 8, curY - 6),
                                    Width = 100,
                                    Height = 36,
                                    Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F),
                                    ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                                    BackColor = Color.Transparent,
                                    TextAlign = ContentAlignment.MiddleLeft
                                };
                                cLayout.Controls.Add(lblSlotsV);
                                this.gridvslot_textbox.Parent = cLayout;
                                this.gridvslot_textbox.Location = new Point(110, curY + 5);
                                this.gridvslot_textbox.ForeColor = Color.White;
                                this.gridvslot_textbox.AutoSize = true;
                                this.gridvslotadd_button.Parent = cLayout;
                                this.gridvslotadd_button.Location = new Point(cw - 85, curY + 2);
                                this.gridvslotadd_button.Size = new Size(30, 26);
                                this.gridvslotadd_button.Text = "+";
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.gridvslotadd_button);
                                this.gridvslotremove_button.Parent = cLayout;
                                this.gridvslotremove_button.Location = new Point(cw - 50, curY + 2);
                                this.gridvslotremove_button.Size = new Size(30, 26);
                                this.gridvslotremove_button.Text = "-";
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.gridvslotremove_button);
                                curY += 40;
                    
                                // Slots H
                                var lblSlotsH = new Label
                                {
                                    Text = LanguageHelper.GetString("MainForm.label53.Text"),
                                    Location = new Point(20 - 8, curY - 6),
                                    Width = 100,
                                    Height = 36,
                                    Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F),
                                    ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                                    BackColor = Color.Transparent,
                                    TextAlign = ContentAlignment.MiddleLeft
                                };
                                cLayout.Controls.Add(lblSlotsH);
                                this.gridhslot_textbox.Parent = cLayout;
                                this.gridhslot_textbox.Location = new Point(110, curY + 5);
                                this.gridhslot_textbox.ForeColor = Color.White;
                                this.gridhslot_textbox.AutoSize = true;
                                this.gridhslotadd_button.Parent = cLayout;
                                this.gridhslotadd_button.Location = new Point(cw - 85, curY + 2);
                                this.gridhslotadd_button.Size = new Size(30, 26);
                                this.gridhslotadd_button.Text = "+";
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.gridhslotadd_button);
                                this.gridhslotremove_button.Parent = cLayout;
                                this.gridhslotremove_button.Location = new Point(cw - 50, curY + 2);
                                this.gridhslotremove_button.Size = new Size(30, 26);
                                this.gridhslotremove_button.Text = "-";
                                StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.gridhslotremove_button);
                                cLayout.Tag = curY + 40;
                    
                                FinalizeCard(cLayout);
                    
                                // ―― 3. Elemento Griglia ──────────────────────────────────────────
                                var cItem = BuildCard("\uE8A1", LanguageHelper.GetString("MainForm.groupBox36.Text"), cw, accent);
                                AddIndentRow(cItem, this.gridslot_ComboBox, LanguageHelper.GetString("MainForm.label51.Text"), 180);
                                AddIndentRow(cItem, this.gridgroup_ComboBox, LanguageHelper.GetString("MainForm.label45.Text"), 180);
                                AddIndentRow(cItem, this.gridspell_ComboBox, LanguageHelper.GetString("MainForm.label52.Text"), 180);
                                AddIndentRow(cItem, this.gridborder_ComboBox, LanguageHelper.GetString("MainForm.label44.Text"), 180);
                                AddIndentRow(cItem, this.gridscript_ComboBox, LanguageHelper.GetString("MainForm.label65.Text"), 180);
                                FinalizeCard(cItem);
                    
                                // ―― 4. OpacitÃ  ───────────────────────────────────────────────────
                                var cOpacity = BuildCard("\uE7B3", LanguageHelper.GetString("MainForm.groupBox38.Text"), cw, accent);
                                curY = (int)cOpacity.Tag;
                                this.spellgrid_opacity_label.Parent = cOpacity;
                                this.spellgrid_opacity_label.Location = new Point(20, curY + 5);
                                this.spellgrid_opacity_label.ForeColor = Color.White;
                                this.spellgrid_opacity_label.AutoSize = true;
                    
                                this.spellgrid_trackBar.Parent = cOpacity;
                                this.spellgrid_trackBar.Location = new Point(70, curY + 5);
                                this.spellgrid_trackBar.Width = cw - 90;
                                this.spellgrid_trackBar.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
                                cOpacity.Tag = curY + 44;
                    
                                FinalizeCard(cOpacity);
                    
                                            flow.Controls.AddRange(new Control[] { cGeneral, cLayout, cItem, cOpacity });
                                        }
                                
                                        private void RebuildHotKeysTab()
                                        {
                                            this.enhancedHotKeytabPage.BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark;
                                            this.enhancedHotKeytabPage.Controls.Clear();
                                
                                            // ―― Scrollable body ─────────────────────────────────────────────────
                                            Panel scrollWrapper = new Panel
                                            {
                                                Dock = DockStyle.Fill,
                                                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                                                Padding = new Padding(0)
                                            };
                                            FlowLayoutPanel flow = new FlowLayoutPanel
                                            {
                                                Dock = DockStyle.Fill,
                                                AutoScroll = true,
                                                BackColor = Assistant.UI.Controls.RazorTheme.Colors.BackgroundDark,
                                                Padding = new Padding(12, 8, 12, 8),
                                                WrapContents = true
                                            };
                                            flow.HandleCreated += (s2, e2) =>
                                            {
                                                try { SetWindowTheme(flow.Handle, "DarkMode_Explorer", null); } catch { }
                                            };
                                            scrollWrapper.Controls.Add(flow);
                                            this.enhancedHotKeytabPage.Controls.Add(scrollWrapper);
                                            scrollWrapper.BringToFront();
                                
                                            int fullW = 642;
                                            int cw = 310;
                                            Color accent = ColorTranslator.FromHtml("#8B5CF6");
                                
                                            // ―― 1. TreeView Pseudo-Card ──────────────────────────────────────
                                            var pTree = MakePseudoCard("\uE8A6  " + LanguageHelper.GetString("MainForm.enhancedHotKeytabPage.Text"), fullW, 400, accent);
                                            this.hotkeytreeView.Parent = pTree;
                                            this.hotkeytreeView.Location = new Point(12, 40);
                                            this.hotkeytreeView.Width = fullW - 24;
                                            this.hotkeytreeView.Height = 400 - 52;
                                            this.hotkeytreeView.BackColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
                                            this.hotkeytreeView.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
                                            this.hotkeytreeView.BorderStyle = BorderStyle.None;
                                            // The theme usually applies Explorer style to treeviews, but let's ensure
                                            this.hotkeytreeView.HandleCreated += (s, e) => {
                                                try { SetWindowTheme(this.hotkeytreeView.Handle, "Explorer", null); } catch { }
                                            };
                                            flow.Controls.Add(pTree);
                                
                                            // ―― 2. Status Card ───────────────────────────────────────────────
                                            var cStatus = BuildCard("\uE953", LanguageHelper.GetString("MainForm.groupBox28.Text"), cw, accent);
                                            int curY = (int)cStatus.Tag;
                                
                                            this.hotkeyStatusLabel.Parent = cStatus;
                                            this.hotkeyStatusLabel.Location = new Point(12, curY + 4);
                                            this.hotkeyStatusLabel.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
                                            this.hotkeyStatusLabel.Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9.5F, FontStyle.Bold);
                                            this.hotkeyStatusLabel.AutoSize = true;
                                
                                            this.hotkeyKeyMasterLabel.Parent = cStatus;
                                            this.hotkeyKeyMasterLabel.Location = new Point(12, curY + 26);
                                            this.hotkeyKeyMasterLabel.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark;
                                            this.hotkeyKeyMasterLabel.Font = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F);
                                            this.hotkeyKeyMasterLabel.AutoSize = true;
                                
                                            curY += 55;
                                            this.hotkeyMEnableButton.Parent = cStatus;
                                            this.hotkeyMEnableButton.Location = new Point(20, curY);
                                            this.hotkeyMEnableButton.Size = new Size(cw / 2 - 30, 28);
                                            this.hotkeyMEnableButton.Text = LanguageHelper.GetString("MainForm.hotkeyMEnableButton.Text");
                                            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.hotkeyMEnableButton);
                                
                                            this.hotkeyMDisableButton.Parent = cStatus;
                                            this.hotkeyMDisableButton.Location = new Point(cw / 2 + 10, curY);
                                            this.hotkeyMDisableButton.Size = new Size(cw / 2 - 30, 28);
                                            this.hotkeyMDisableButton.Text = LanguageHelper.GetString("MainForm.hotkeyMDisableButton.Text");
                                            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.hotkeyMDisableButton);
                                
                                            cStatus.Tag = curY + 40;
                                            FinalizeCard(cStatus);
                                
                                            // ―― 3. Master Key Card ───────────────────────────────────────────
                                            var cMaster = BuildCard("\uE722", LanguageHelper.GetString("MainForm.groupBox8.Text"), cw, accent);
                                            AddIndentRow(cMaster, this.hotkeyKeyMasterTextBox, LanguageHelper.GetString("MainForm.label42.Text"), 140);
                                            
                                            curY = (int)cMaster.Tag;
                                            this.hotkeyMasterSetButton.Parent = cMaster;
                                            this.hotkeyMasterSetButton.Location = new Point(20, curY + 4);
                                            this.hotkeyMasterSetButton.Size = new Size(cw / 2 - 30, 28);
                                            this.hotkeyMasterSetButton.Text = LanguageHelper.GetString("MainForm.hotkeyMasterSetButton.Text");
                                            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.hotkeyMasterSetButton);
                                
                                            this.hotkeyMasterClearButton.Parent = cMaster;
                                            this.hotkeyMasterClearButton.Location = new Point(cw / 2 + 10, curY + 4);
                                            this.hotkeyMasterClearButton.Size = new Size(cw / 2 - 30, 28);
                                            this.hotkeyMasterClearButton.Text = LanguageHelper.GetString("MainForm.hotkeyMasterClearButton.Text");
                                            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.hotkeyMasterClearButton);
                                
                                            cMaster.Tag = curY + 42;
                                            FinalizeCard(cMaster);
                                
                                            // ―― 4. Modify Key Card ───────────────────────────────────────────
                                            var cModify = BuildCard("\uE70F", LanguageHelper.GetString("MainForm.groupBox27.Text"), cw, accent);
                                            AddIndentRow(cModify, this.hotkeytextbox, LanguageHelper.GetString("MainForm.label39.Text"), 140);
                                            AddRow(cModify, this.hotkeypassCheckBox, LanguageHelper.GetString("MainForm.hotkeypassCheckBox.Text"));
                                
                                            curY = (int)cModify.Tag;
                                            this.hotkeySetButton.Parent = cModify;
                                            this.hotkeySetButton.Location = new Point(20, curY + 4);
                                            this.hotkeySetButton.Size = new Size(cw / 2 - 30, 28);
                                            this.hotkeySetButton.Text = LanguageHelper.GetString("MainForm.hotkeySetButton.Text");
                                            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.hotkeySetButton);
                                
                                            this.hotkeyClearButton.Parent = cModify;
                                            this.hotkeyClearButton.Location = new Point(cw / 2 + 10, curY + 4);
                                            this.hotkeyClearButton.Size = new Size(cw / 2 - 30, 28);
                                            this.hotkeyClearButton.Text = LanguageHelper.GetString("MainForm.hotkeyClearButton.Text");
                                            StyleButtonAsOutline((Assistant.UI.Controls.RazorButton)this.hotkeyClearButton);
                                
                                                        cModify.Tag = curY + 42;
                                                        FinalizeCard(cModify);
                                            
                                                        flow.Controls.AddRange(new Control[] { pTree, cStatus, cMaster, cModify });
                                                    }
                                            
                                        // ── Card factory ─────────────────────────────────────────────────────
        // card.Tag = current Y (pre-shift). OnHandleCreated adds +8L/+6T to every
        // direct child, so we pre-subtract 8 from X and 6 from Y when positioning.
        private Assistant.UI.Controls.RazorCard BuildCard(
            string icon, string title, int width, Color borderColor)
        {
            var card = new Assistant.UI.Controls.RazorCard {
                Text        = icon + "  " + title,
                Width       = width,
                Height      = 60,
                Margin      = new Padding(10),
                BorderColor = borderColor
            };
            card.Tag = 38; // first row starts at actual Y=38 (pre-shift stored as 38)
            return card;
        }

        // Set card.Height based on tracked Y so the purple bar spans full height.
        private void FinalizeCard(Assistant.UI.Controls.RazorCard card)
        {
            int curY = card.Tag is int t ? t : 38;
            // After OnHandleCreated +6 shift, final content bottom = curY + 6
            card.Height = curY + 6 + 14;
        }

        // ── Toggle row ───────────────────────────────────────────────────────
        private void AddRow(Control card, System.Windows.Forms.CheckBox toggle, string text)
        {
            if (toggle == null) return;
            int cardW = card.Width > 0 ? card.Width : 340;
            int rowW  = cardW - 20;
            int rowH  = 44;
            int curY  = card.Tag is int t ? t : 38;

            toggle.Text      = "";
            toggle.Size      = new Size(44, 22);
            toggle.Location  = new Point(rowW - 48 - 8, curY + (rowH - 22) / 2 - 6);
            toggle.BackColor = Color.Transparent;
            toggle.Parent    = card;

            var lbl = new Label {
                Text      = text,
                Location  = new Point(12 - 8, curY - 6),
                Width     = rowW - 68,
                Height    = rowH,
                Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9.5F),
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lbl);
            toggle.BringToFront();
            card.Tag = curY + rowH;
        }

        // ── Indented text-box row ────────────────────────────────────────────
        private void AddIndentRow(Control card, Control txt, string label, int tbWidth)
        {
            if (txt == null) return;
            int cardW = card.Width > 0 ? card.Width : 340;
            int rowW  = cardW - 20;
            int rowH  = 36;
            int curY  = card.Tag is int t ? t : 38;

            var lbl = new Label {
                Text      = label,
                Location  = new Point(20 - 8, curY - 6),
                Width     = rowW - tbWidth - 30,
                Height    = rowH,
                Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F),
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lbl);

            txt.Width     = tbWidth;
            txt.Height    = 22;
            txt.Location  = new Point(rowW - tbWidth - 4 - 8, curY + (rowH - 22) / 2 - 6);
            txt.Parent    = card;
            txt.BackColor = Assistant.UI.Controls.RazorTheme.Colors.SurfaceDark;
            txt.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            txt.BringToFront();
            card.Tag = curY + rowH;
        }

        // ── Section sub-label + separator ───────────────────────────────────
        private void AddSectionLabel(Control card, string text)
        {
            int cardW = card.Width > 0 ? card.Width : 340;
            int rowW  = cardW - 20;
            int rowH  = 28;
            int curY  = card.Tag is int t ? t : 38;

            var sep = new Label {
                Location  = new Point(8 - 8, curY - 6),
                Width     = rowW, Height = rowH,
                BackColor = Color.Transparent,
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextSecondaryDark,
                Text      = "  " + text.ToUpper(),
                Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(7.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            int capturedRowW = rowW, capturedRowH = rowH;
            sep.Paint += (s, ev) => {
                ev.Graphics.DrawLine(
                    new System.Drawing.Pen(Color.FromArgb(55, 255, 255, 255), 1),
                    0, capturedRowH / 2, capturedRowW, capturedRowH / 2);
            };
            card.Controls.Add(sep);
            card.Tag = curY + rowH;
        }

        // ── Color hue row ────────────────────────────────────────────────────
        private void AddColorHueRow(Control card, Control swatch,
            Assistant.UI.Controls.RazorButton btn, string text)
        {
            if (swatch == null || btn == null) return;
            int cardW = card.Width > 0 ? card.Width : 340;
            int rowW  = cardW - 20;
            int rowH  = 44;
            int curY  = card.Tag is int t ? t : 38;

            var lbl = new Label {
                Text      = text,
                Location  = new Point(12 - 8, curY - 6),
                Width     = rowW - 110,
                Height    = rowH,
                Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9.5F),
                ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lbl);

            swatch.Parent   = card;
            swatch.Size     = new Size(26, 22);
            swatch.Location = new Point(rowW - 98 - 8, curY + (rowH - 22) / 2 - 6);
            if (swatch is Label swL) swL.BorderStyle = BorderStyle.FixedSingle;

            btn.Text     = "Set";
            btn.Parent   = card;
            btn.Size     = new Size(52, 26);
            btn.Location = new Point(rowW - 62 - 8, curY + (rowH - 26) / 2 - 6);
            card.Tag = curY + rowH;
        }

        // ── Spell hue triple (Bene / Harm / Neu) ─────────────────────────────
        private void AddSpellHueTriple(
            Control card,
            Control sw1, Assistant.UI.Controls.RazorButton b1, string t1, Color c1,
            Control sw2, Assistant.UI.Controls.RazorButton b2, string t2, Color c2,
            Control sw3, Assistant.UI.Controls.RazorButton b3, string t3, Color c3)
        {
            int cardW = card.Width > 0 ? card.Width : 340;
            int rowW  = cardW - 20;
            int rowH  = 76;
            int curY  = card.Tag is int t ? t : 38;
            int colW  = rowW / 3;

            void PlaceHue(Control sw, Assistant.UI.Controls.RazorButton btn,
                          string label, Color accent, int colX)
            {
                var lb = new Label {
                    Text      = label,
                    Location  = new Point(colX + 4 - 8, curY + 2 - 6),
                    Width     = colW - 8, Height = 18,
                    Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(8F),
                    ForeColor = accent,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lb);

                sw.Parent   = card;
                sw.Size     = new Size(colW - 20, 14);
                sw.Location = new Point(colX + 10 - 8, curY + 22 - 6);
                if (sw is Label swL2) swL2.BorderStyle = BorderStyle.FixedSingle;

                btn.Text     = "Set";
                btn.Parent   = card;
                btn.Size     = new Size(colW - 16, 20);
                btn.Location = new Point(colX + 8 - 8, curY + 42 - 6);
            }
            PlaceHue(sw1, b1, t1, c1, 0);
            PlaceHue(sw2, b2, t2, c2, colW);
            PlaceHue(sw3, b3, t3, c3, colW * 2);
            card.Tag = curY + rowH;
        }

        // ── Compact single spell-hue row (label + swatch + Set button) ────────────
        private void AddSpellHueRow(Control card, Control swatch,
            Assistant.UI.Controls.RazorButton btn, string label, Color accent)
        {
            if (swatch == null || btn == null) return;
            int cardW = card.Width > 0 ? card.Width : 340;
            int rowW  = cardW - 20;
            int rowH  = 34;
            int curY  = card.Tag is int t ? t : 38;

            var lbl = new Label {
                Text      = label,
                Location  = new Point(20 - 8, curY - 6),
                Width     = rowW - 90,
                Height    = rowH,
                Font      = Assistant.UI.Controls.RazorTheme.Fonts.DisplayFont(9F),
                ForeColor = accent,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lbl);

            swatch.Parent   = card;
            swatch.Size     = new Size(22, 16);
            swatch.Location = new Point(rowW - 76 - 8, curY + (rowH - 16) / 2 - 6);
            if (swatch is Label swL) swL.BorderStyle = BorderStyle.FixedSingle;

            btn.Text     = "Set";
            btn.Parent   = card;
            btn.Size     = new Size(46, 22);
            btn.Location = new Point(rowW - 50 - 8, curY + (rowH - 22) / 2 - 6);
            card.Tag = curY + rowH;
        }
        // ── Helper to style a RazorButton as an outline button (secondary action) ──
        private void StyleButtonAsOutline(Assistant.UI.Controls.RazorButton btn)
        {
            if (btn == null) return;
            btn.BackColor = Assistant.UI.Controls.RazorTheme.Colors.CardDark; // Dark background
            btn.ForeColor = Assistant.UI.Controls.RazorTheme.Colors.TextDarkMode;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Assistant.UI.Controls.RazorTheme.Colors.Primary;
            btn.FlatAppearance.BorderSize = 1;
        }

        // GetInner kept for API compat — always returns null (direct mode now)
        private FlowLayoutPanel GetInner(Control card) => null;

        // Legacy helpers  delegate to new row-based methods
        private void AddOptionToggle(System.Windows.Forms.CheckBox toggle, string defaultText, Control parent, int x, int y)
        {
            AddRow(parent, toggle, defaultText);
        }

        private void AddOptionTextBox(Assistant.UI.Controls.RazorTextBox txt, string prefixText, Control parent, int x, int y, int width)
        {
            AddIndentRow(parent, txt, prefixText, width);
        }

        private void AddOptionColorHue(Control lblHue, Assistant.UI.Controls.RazorButton btn, string text, Control parent, int x, int y)
        {
            AddColorHueRow(parent, lblHue, btn, text);
        }
    }
}

