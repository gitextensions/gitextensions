namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

partial class GitConfigAdvancedSettingsPage
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tlpnlMain = new TableLayoutPanel();
        lblPullFfOnly = new Label();
        lblPullRebase = new Label();
        lblFetchPrune = new Label();
        lblMergeAutoStash = new Label();
        lblRebaseAutostash = new Label();
        lblRebaseAutosquash = new Label();
        lblUpdateRefs = new Label();
        lblReReReEnabled = new Label();
        lblReReReAutoUpdate = new Label();
        lblStatusShowUntrackedFiles = new Label();
        tlpnlMain.SuspendLayout();
        SuspendLayout();
        // 
        // tlpnlMain
        // 
        tlpnlMain.AutoSize = true;
        tlpnlMain.ColumnCount = 3;
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.Controls.Add(lblPullFfOnly, 0, 0);
        tlpnlMain.Controls.Add(lblPullRebase, 0, 1);
        tlpnlMain.Controls.Add(lblFetchPrune, 0, 2);
        tlpnlMain.Controls.Add(lblMergeAutoStash, 0, 3);
        tlpnlMain.Controls.Add(lblRebaseAutostash, 0, 4);
        tlpnlMain.Controls.Add(lblRebaseAutosquash, 0, 5);
        tlpnlMain.Controls.Add(lblUpdateRefs, 0, 6);
        tlpnlMain.Controls.Add(lblReReReEnabled, 0, 7);
        tlpnlMain.Controls.Add(lblReReReAutoUpdate, 0, 8);
        tlpnlMain.Controls.Add(lblStatusShowUntrackedFiles, 0, 9);
        tlpnlMain.Dock = DockStyle.Fill;
        tlpnlMain.Location = new Point(0, 0);
        tlpnlMain.Name = "tlpnlMain";
        tlpnlMain.RowCount = 11;
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpnlMain.Size = new Size(1439, 516);
        tlpnlMain.TabIndex = 0;
        // 
        // lblPullFfOnly
        // 
        lblPullFfOnly.Anchor = AnchorStyles.Left;
        lblPullFfOnly.AutoSize = true;
        lblPullFfOnly.Name = "lblPullFfOnly";
        lblPullFfOnly.Text = "Only allow fast-forward when pulling";
        // 
        // lblPullRebase
        // 
        lblPullRebase.Anchor = AnchorStyles.Left;
        lblPullRebase.AutoSize = true;
        lblPullRebase.Name = "lblPullRebase";
        lblPullRebase.Text = "Rebase local branch when pulling (instead of merge)";
        // 
        // lblFetchPrune
        // 
        lblFetchPrune.Anchor = AnchorStyles.Left;
        lblFetchPrune.AutoSize = true;
        lblFetchPrune.Name = "lblFetchPrune";
        lblFetchPrune.Text = "Prune remote branches during fetch";
        // 
        // lblMergeAutoStash
        // 
        lblMergeAutoStash.Anchor = AnchorStyles.Left;
        lblMergeAutoStash.AutoSize = true;
        lblMergeAutoStash.Name = "lblMergeAutoStash";
        lblMergeAutoStash.Text = "Automatically stash before doing a merge";
        // 
        // lblRebaseAutostash
        // 
        lblRebaseAutostash.Anchor = AnchorStyles.Left;
        lblRebaseAutostash.AutoSize = true;
        lblRebaseAutostash.Name = "lblRebaseAutostash";
        lblRebaseAutostash.Text = "Automatically stash before doing a rebase";
        // 
        // lblRebaseAutosquash
        // 
        lblRebaseAutosquash.Anchor = AnchorStyles.Left;
        lblRebaseAutosquash.AutoSize = true;
        lblRebaseAutosquash.Name = "lblRebaseAutosquash";
        lblRebaseAutosquash.Text = "Automatically squash commits when doing an interactive rebase";
        // 
        // lblUpdateRefs
        // 
        lblUpdateRefs.Anchor = AnchorStyles.Left;
        lblUpdateRefs.AutoSize = true;
        lblUpdateRefs.Name = "lblUpdateRefs";
        lblUpdateRefs.Text = "Rebase also dependent branches";
        // 
        // lblReReReEnabled
        // 
        lblReReReEnabled.Anchor = AnchorStyles.Left;
        lblReReReEnabled.AutoSize = true;
        lblReReReEnabled.Name = "lblReReReEnabled";
        lblReReReEnabled.Text = "Reuse recorded resolution of conflicted merges";
        // 
        // lblReReReAutoUpdate
        // 
        lblReReReAutoUpdate.Anchor = AnchorStyles.Left;
        lblReReReAutoUpdate.AutoSize = true;
        lblReReReAutoUpdate.Name = "lblReReReAutoUpdate";
        lblReReReAutoUpdate.Text = "Automatically apply recorded resolution of conflicted merges";
        // 
        // lblStatusShowUntrackedFiles
        // 
        lblStatusShowUntrackedFiles.Anchor = AnchorStyles.Left;
        lblStatusShowUntrackedFiles.AutoSize = true;
        lblStatusShowUntrackedFiles.Name = "lblStatusShowUntrackedFiles";
        lblStatusShowUntrackedFiles.Text = "Show untracked files";
        // 
        // GitConfigAdvancedSettingsPage
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(tlpnlMain);
        Name = "GitConfigAdvancedSettingsPage";
        Size = new Size(1439, 516);
        Text = "Advanced";
        tlpnlMain.ResumeLayout(false);
        tlpnlMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel tlpnlMain;
    private Label lblPullFfOnly;
    private Label lblPullRebase;
    private Label lblFetchPrune;
    private Label lblMergeAutoStash;
    private Label lblRebaseAutostash;
    private Label lblRebaseAutosquash;
    private Label lblUpdateRefs;
    private Label lblReReReEnabled;
    private Label lblReReReAutoUpdate;
    private Label lblStatusShowUntrackedFiles;
}
