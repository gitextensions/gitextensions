using System.ComponentModel;
using GitUI.UserControls.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

partial class FormBrowseRepoSettingsPage
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components is not null))
        {
            components.Dispose();
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
        chkShowGpgInformation = new SettingsCheckBox();
        chkShowConsoleTab = new SettingsCheckBox();
        cboTerminal = new ComboBox();
        lblDefaultShell = new Label();
        chkUseBrowseForFileHistory = new SettingsCheckBox();
        chkUseDiffViewerForBlame = new SettingsCheckBox();
        chkShowFindInCommitFilesGitGrep = new SettingsCheckBox();
        tlpnlMain = new TableLayoutPanel();
        groupBox1 = new GroupBox();
        tlpnlGeneral = new TableLayoutPanel();
        gbTabs = new GroupBox();
        tlpnlTabs = new TableLayoutPanel();
        chkShowOutputHistoryAsTab = new SettingsCheckBox();
        lblOutputHistoryDepth = new Label();
        _NO_TRANSLATE_OutputHistoryDepth = new NumericUpDown();
        tlpnlMain.SuspendLayout();
        groupBox1.SuspendLayout();
        tlpnlGeneral.SuspendLayout();
        gbTabs.SuspendLayout();
        tlpnlTabs.SuspendLayout();
        ((ISupportInitialize)_NO_TRANSLATE_OutputHistoryDepth).BeginInit();
        SuspendLayout();
        // 
        // chkShowGpgInformation
        // 
        chkShowGpgInformation.AutoSize = true;
        chkShowGpgInformation.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        chkShowGpgInformation.Checked = false;
        chkShowGpgInformation.Dock = DockStyle.Fill;
        chkShowGpgInformation.Location = new Point(4, 28);
        chkShowGpgInformation.ManualSectionAnchorName = "tabs-show-gpg-information";
        chkShowGpgInformation.Margin = new Padding(4, 3, 4, 3);
        chkShowGpgInformation.Name = "chkShowGpgInformation";
        chkShowGpgInformation.Size = new Size(277, 19);
        chkShowGpgInformation.TabIndex = 1;
        chkShowGpgInformation.Text = "Show GPG information";
        chkShowGpgInformation.ToolTipIcon = UserControls.Settings.ToolTipIcon.Information;
        chkShowGpgInformation.ToolTipText = "View help";
        // 
        // chkShowConsoleTab
        // 
        chkShowConsoleTab.AutoSize = true;
        chkShowConsoleTab.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        chkShowConsoleTab.Checked = false;
        chkShowConsoleTab.Dock = DockStyle.Fill;
        chkShowConsoleTab.Location = new Point(4, 3);
        chkShowConsoleTab.ManualSectionAnchorName = "general-show-blame-in-diff-view";
        chkShowConsoleTab.Margin = new Padding(4, 3, 4, 3);
        chkShowConsoleTab.Name = "chkShowConsoleTab";
        chkShowConsoleTab.Size = new Size(277, 19);
        chkShowConsoleTab.TabIndex = 0;
        chkShowConsoleTab.Text = "Show the Console tab";
        chkShowConsoleTab.ToolTipIcon = UserControls.Settings.ToolTipIcon.Information;
        chkShowConsoleTab.ToolTipText = "View help";
        // 
        // cboTerminal
        // 
        cboTerminal.DropDownStyle = ComboBoxStyle.DropDownList;
        cboTerminal.FormattingEnabled = true;
        cboTerminal.Location = new Point(260, 2);
        cboTerminal.Margin = new Padding(3, 2, 3, 2);
        cboTerminal.Name = "cboTerminal";
        cboTerminal.Size = new Size(262, 23);
        cboTerminal.TabIndex = 1;
        // 
        // lblDefaultShell
        // 
        lblDefaultShell.AutoSize = true;
        lblDefaultShell.Dock = DockStyle.Fill;
        lblDefaultShell.Location = new Point(3, 0);
        lblDefaultShell.Name = "lblDefaultShell";
        lblDefaultShell.Size = new Size(251, 27);
        lblDefaultShell.TabIndex = 0;
        lblDefaultShell.Text = "Default shell";
        lblDefaultShell.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // chkUseBrowseForFileHistory
        // 
        chkUseBrowseForFileHistory.AutoSize = true;
        chkUseBrowseForFileHistory.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        chkUseBrowseForFileHistory.Checked = false;
        chkUseBrowseForFileHistory.Dock = DockStyle.Fill;
        chkUseBrowseForFileHistory.Location = new Point(4, 30);
        chkUseBrowseForFileHistory.ManualSectionAnchorName = "general-show-file-history-in-the-main-window";
        chkUseBrowseForFileHistory.Margin = new Padding(4, 3, 4, 3);
        chkUseBrowseForFileHistory.Name = "chkUseBrowseForFileHistory";
        chkUseBrowseForFileHistory.Size = new Size(249, 19);
        chkUseBrowseForFileHistory.TabIndex = 2;
        chkUseBrowseForFileHistory.Text = "Show file history in the main window";
        chkUseBrowseForFileHistory.ToolTipIcon = UserControls.Settings.ToolTipIcon.Information;
        chkUseBrowseForFileHistory.ToolTipText = "View help";
        // 
        // chkUseDiffViewerForBlame
        // 
        chkUseDiffViewerForBlame.AutoSize = true;
        chkUseDiffViewerForBlame.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        chkUseDiffViewerForBlame.Checked = false;
        chkUseDiffViewerForBlame.Dock = DockStyle.Fill;
        chkUseDiffViewerForBlame.Location = new Point(4, 55);
        chkUseDiffViewerForBlame.ManualSectionAnchorName = "general-show-blame-in-diff-view";
        chkUseDiffViewerForBlame.Margin = new Padding(4, 3, 4, 3);
        chkUseDiffViewerForBlame.Name = "chkUseDiffViewerForBlame";
        chkUseDiffViewerForBlame.Size = new Size(249, 19);
        chkUseDiffViewerForBlame.TabIndex = 3;
        chkUseDiffViewerForBlame.Text = "Show blame in diff viewer";
        chkUseDiffViewerForBlame.ToolTipIcon = UserControls.Settings.ToolTipIcon.Information;
        chkUseDiffViewerForBlame.ToolTipText = "View help";
        // 
        // chkShowFindInCommitFilesGitGrep
        // 
        chkShowFindInCommitFilesGitGrep.AutoSize = true;
        chkShowFindInCommitFilesGitGrep.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        chkShowFindInCommitFilesGitGrep.Checked = false;
        chkShowFindInCommitFilesGitGrep.Dock = DockStyle.Fill;
        chkShowFindInCommitFilesGitGrep.Location = new Point(4, 80);
        chkShowFindInCommitFilesGitGrep.ManualSectionAnchorName = null;
        chkShowFindInCommitFilesGitGrep.Margin = new Padding(4, 3, 4, 3);
        chkShowFindInCommitFilesGitGrep.Name = "chkShowFindInCommitFilesGitGrep";
        chkShowFindInCommitFilesGitGrep.Size = new Size(249, 19);
        chkShowFindInCommitFilesGitGrep.TabIndex = 4;
        chkShowFindInCommitFilesGitGrep.Text = "Show 'Find in commit files using git-grep'";
        chkShowFindInCommitFilesGitGrep.ToolTipIcon = UserControls.Settings.ToolTipIcon.Information;
        chkShowFindInCommitFilesGitGrep.ToolTipText = null;
        // 
        // tlpnlMain
        // 
        tlpnlMain.ColumnCount = 1;
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.Controls.Add(groupBox1, 0, 0);
        tlpnlMain.Controls.Add(gbTabs, 0, 1);
        tlpnlMain.Dock = DockStyle.Fill;
        tlpnlMain.Location = new Point(8, 8);
        tlpnlMain.Name = "tlpnlMain";
        tlpnlMain.RowCount = 4;
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlMain.Size = new Size(1585, 642);
        tlpnlMain.TabIndex = 0;
        // 
        // groupBox1
        // 
        groupBox1.AutoSize = true;
        groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupBox1.Controls.Add(tlpnlGeneral);
        groupBox1.Dock = DockStyle.Fill;
        groupBox1.Location = new Point(3, 3);
        groupBox1.Name = "groupBox1";
        groupBox1.Padding = new Padding(8);
        groupBox1.Size = new Size(1579, 134);
        groupBox1.TabIndex = 0;
        groupBox1.TabStop = false;
        groupBox1.Text = "General";
        // 
        // tlpnlGeneral
        // 
        tlpnlGeneral.AutoSize = true;
        tlpnlGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlGeneral.ColumnCount = 2;
        tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
        tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
        tlpnlGeneral.Controls.Add(cboTerminal, 1, 0);
        tlpnlGeneral.Controls.Add(lblDefaultShell, 0, 0);
        tlpnlGeneral.Controls.Add(chkUseBrowseForFileHistory, 0, 1);
        tlpnlGeneral.Controls.Add(chkUseDiffViewerForBlame, 0, 2);
        tlpnlGeneral.Controls.Add(chkShowFindInCommitFilesGitGrep, 0, 3);
        tlpnlGeneral.Dock = DockStyle.Fill;
        tlpnlGeneral.Location = new Point(8, 24);
        tlpnlGeneral.Name = "tlpnlGeneral";
        tlpnlGeneral.RowCount = 4;
        tlpnlGeneral.RowStyles.Add(new RowStyle());
        tlpnlGeneral.RowStyles.Add(new RowStyle());
        tlpnlGeneral.RowStyles.Add(new RowStyle());
        tlpnlGeneral.RowStyles.Add(new RowStyle());
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlGeneral.Size = new Size(1563, 102);
        tlpnlGeneral.TabIndex = 0;
        // 
        // gbTabs
        // 
        gbTabs.AutoSize = true;
        gbTabs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        gbTabs.Controls.Add(tlpnlTabs);
        gbTabs.Dock = DockStyle.Fill;
        gbTabs.Location = new Point(3, 143);
        gbTabs.Name = "gbTabs";
        gbTabs.Padding = new Padding(8);
        gbTabs.Size = new Size(1579, 136);
        gbTabs.TabIndex = 1;
        gbTabs.TabStop = false;
        gbTabs.Text = "Tabs (restart required)";
        // 
        // tlpnlTabs
        // 
        tlpnlTabs.AutoSize = true;
        tlpnlTabs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlTabs.ColumnCount = 2;
        tlpnlTabs.ColumnStyles.Add(new ColumnStyle());
        tlpnlTabs.ColumnStyles.Add(new ColumnStyle());
        tlpnlTabs.Controls.Add(chkShowConsoleTab, 0, 0);
        tlpnlTabs.Controls.Add(chkShowGpgInformation, 0, 1);
        tlpnlTabs.Controls.Add(chkShowOutputHistoryAsTab, 0, 2);
        tlpnlTabs.Controls.Add(lblOutputHistoryDepth, 0, 3);
        tlpnlTabs.Controls.Add(_NO_TRANSLATE_OutputHistoryDepth, 1, 3);
        tlpnlTabs.Dock = DockStyle.Fill;
        tlpnlTabs.Location = new Point(8, 24);
        tlpnlTabs.Name = "tlpnlTabs";
        tlpnlTabs.RowCount = 4;
        tlpnlTabs.RowStyles.Add(new RowStyle());
        tlpnlTabs.RowStyles.Add(new RowStyle());
        tlpnlTabs.RowStyles.Add(new RowStyle());
        tlpnlTabs.RowStyles.Add(new RowStyle());
        tlpnlTabs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tlpnlTabs.Size = new Size(1563, 104);
        tlpnlTabs.TabIndex = 0;
        // 
        // chkShowOutputHistoryAsTab
        // 
        chkShowOutputHistoryAsTab.AutoSize = true;
        chkShowOutputHistoryAsTab.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        chkShowOutputHistoryAsTab.Checked = false;
        chkShowOutputHistoryAsTab.Location = new Point(4, 53);
        chkShowOutputHistoryAsTab.ManualSectionAnchorName = "show-output-history-as-tab-otherwise-as-panel";
        chkShowOutputHistoryAsTab.Margin = new Padding(4, 3, 4, 3);
        chkShowOutputHistoryAsTab.Name = "chkShowOutputHistoryAsTab";
        chkShowOutputHistoryAsTab.Size = new Size(277, 19);
        chkShowOutputHistoryAsTab.TabIndex = 8;
        chkShowOutputHistoryAsTab.Text = "Show output history as tab (otherwise as panel)";
        chkShowOutputHistoryAsTab.ToolTipIcon = UserControls.Settings.ToolTipIcon.Information;
        chkShowOutputHistoryAsTab.ToolTipText = null;
        // 
        // lblOutputHistoryDepth
        // 
        lblOutputHistoryDepth.AutoSize = true;
        lblOutputHistoryDepth.Dock = DockStyle.Fill;
        lblOutputHistoryDepth.Location = new Point(3, 75);
        lblOutputHistoryDepth.Name = "lblOutputHistoryDepth";
        lblOutputHistoryDepth.Size = new Size(279, 29);
        lblOutputHistoryDepth.TabIndex = 9;
        lblOutputHistoryDepth.Text = "Output history depth (0 to disable):";
        lblOutputHistoryDepth.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _NO_TRANSLATE_OutputHistoryDepth
        // 
        _NO_TRANSLATE_OutputHistoryDepth.Location = new Point(288, 78);
        _NO_TRANSLATE_OutputHistoryDepth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        _NO_TRANSLATE_OutputHistoryDepth.Name = "_NO_TRANSLATE_OutputHistoryDepth";
        _NO_TRANSLATE_OutputHistoryDepth.Size = new Size(48, 23);
        _NO_TRANSLATE_OutputHistoryDepth.TabIndex = 10;
        _NO_TRANSLATE_OutputHistoryDepth.Value = new decimal(new int[] { 1000, 0, 0, 0 });
        // 
        // FormBrowseRepoSettingsPage
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpnlMain);
        Name = "FormBrowseRepoSettingsPage";
        Padding = new Padding(8);
        Text = "Browse repository window";
        Size = new Size(1601, 658);
        tlpnlMain.ResumeLayout(false);
        tlpnlMain.PerformLayout();
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        tlpnlGeneral.ResumeLayout(false);
        tlpnlGeneral.PerformLayout();
        gbTabs.ResumeLayout(false);
        gbTabs.PerformLayout();
        tlpnlTabs.ResumeLayout(false);
        tlpnlTabs.PerformLayout();
        ((ISupportInitialize)_NO_TRANSLATE_OutputHistoryDepth).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private SettingsCheckBox chkShowGpgInformation;
    private SettingsCheckBox chkShowConsoleTab;
    private ComboBox cboTerminal;
    private Label lblDefaultShell;
    private SettingsCheckBox chkUseBrowseForFileHistory;
    private SettingsCheckBox chkUseDiffViewerForBlame;
    private SettingsCheckBox chkShowFindInCommitFilesGitGrep;
    private TableLayoutPanel tlpnlMain;
    private GroupBox groupBox1;
    private TableLayoutPanel tlpnlGeneral;
    private GroupBox gbTabs;
    private TableLayoutPanel tlpnlTabs;
    private SettingsCheckBox chkShowOutputHistoryAsTab;
    private Label lblOutputHistoryDepth;
    private NumericUpDown _NO_TRANSLATE_OutputHistoryDepth;
}
