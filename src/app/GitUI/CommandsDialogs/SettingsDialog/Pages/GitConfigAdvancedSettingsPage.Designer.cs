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
        chkPullFfOnly = new CheckBox();
        chkPullRebase = new CheckBox();
        chkFetchPrune = new CheckBox();
        chkMergeAutoStash = new CheckBox();
        chkRebaseAutostash = new CheckBox();
        chkRebaseAutosquash = new CheckBox();
        chkUpdateRefs = new CheckBox();
        chkReReReEnabled = new CheckBox();
        chkReReReAutoUpdate = new CheckBox();
        chkStatusShowUntrackedFiles = new CheckBox();
        tlpnlMain.SuspendLayout();
        SuspendLayout();
        // 
        // tlpnlMain
        // 
        tlpnlMain.AutoSize = true;
        tlpnlMain.ColumnCount = 1;
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.Controls.Add(chkPullFfOnly, 0, 0);
        tlpnlMain.Controls.Add(chkPullRebase, 0, 1);
        tlpnlMain.Controls.Add(chkFetchPrune, 0, 2);
        tlpnlMain.Controls.Add(chkMergeAutoStash, 0, 3);
        tlpnlMain.Controls.Add(chkRebaseAutostash, 0, 4);
        tlpnlMain.Controls.Add(chkRebaseAutosquash, 0, 5);
        tlpnlMain.Controls.Add(chkUpdateRefs, 0, 6);
        tlpnlMain.Controls.Add(chkReReReEnabled, 0, 7);
        tlpnlMain.Controls.Add(chkReReReAutoUpdate, 0, 8);
        tlpnlMain.Controls.Add(chkStatusShowUntrackedFiles, 0, 9);
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
        // chkPullFfOnly
        // 
        chkPullFfOnly.AutoSize = true;
        chkPullFfOnly.Location = new Point(3, 3);
        chkPullFfOnly.Name = "chkPullFfOnly";
        chkPullFfOnly.Size = new Size(222, 19);
        chkPullFfOnly.TabIndex = 9;
        chkPullFfOnly.Text = "Only allow fast-forward when pulling";
        chkPullFfOnly.ThreeState = true;
        chkPullFfOnly.UseVisualStyleBackColor = true;
        // 
        // chkPullRebase
        // 
        chkPullRebase.AutoSize = true;
        chkPullRebase.Location = new Point(3, 28);
        chkPullRebase.Name = "chkPullRebase";
        chkPullRebase.Size = new Size(303, 19);
        chkPullRebase.TabIndex = 1;
        chkPullRebase.Text = "Rebase local branch when pulling (instead of merge)";
        chkPullRebase.ThreeState = true;
        chkPullRebase.UseVisualStyleBackColor = true;
        // 
        // chkFetchPrune
        // 
        chkFetchPrune.AutoSize = true;
        chkFetchPrune.Location = new Point(3, 53);
        chkFetchPrune.Name = "chkFetchPrune";
        chkFetchPrune.Size = new Size(217, 19);
        chkFetchPrune.TabIndex = 2;
        chkFetchPrune.Text = "Prune remote branches during fetch";
        chkFetchPrune.ThreeState = true;
        chkFetchPrune.UseVisualStyleBackColor = true;
        // 
        // chkMergeAutoStash
        // 
        chkMergeAutoStash.AutoSize = true;
        chkMergeAutoStash.Location = new Point(3, 78);
        chkMergeAutoStash.Name = "chkMergeAutoStash";
        chkMergeAutoStash.Size = new Size(247, 19);
        chkMergeAutoStash.TabIndex = 3;
        chkMergeAutoStash.Text = "Automatically stash before doing a merge";
        chkMergeAutoStash.ThreeState = true;
        chkMergeAutoStash.UseVisualStyleBackColor = true;
        // 
        // chkRebaseAutostash
        // 
        chkRebaseAutostash.AutoSize = true;
        chkRebaseAutostash.Location = new Point(3, 103);
        chkRebaseAutostash.Name = "chkRebaseAutostash";
        chkRebaseAutostash.Size = new Size(247, 19);
        chkRebaseAutostash.TabIndex = 4;
        chkRebaseAutostash.Text = "Automatically stash before doing a rebase";
        chkRebaseAutostash.ThreeState = true;
        chkRebaseAutostash.UseVisualStyleBackColor = true;
        // 
        // chkRebaseAutosquash
        // 
        chkRebaseAutosquash.AutoSize = true;
        chkRebaseAutosquash.Location = new Point(3, 128);
        chkRebaseAutosquash.Name = "chkRebaseAutosquash";
        chkRebaseAutosquash.Size = new Size(367, 19);
        chkRebaseAutosquash.TabIndex = 5;
        chkRebaseAutosquash.Text = "Automatically squash commits when doing an interactive rebase";
        chkRebaseAutosquash.ThreeState = true;
        chkRebaseAutosquash.UseVisualStyleBackColor = true;
        // 
        // chkUpdateRefs
        // 
        chkUpdateRefs.AutoSize = true;
        chkUpdateRefs.Location = new Point(3, 153);
        chkUpdateRefs.Name = "chkUpdateRefs";
        chkUpdateRefs.Size = new Size(198, 19);
        chkUpdateRefs.TabIndex = 6;
        chkUpdateRefs.Text = "Rebase also dependent branches";
        chkUpdateRefs.ThreeState = true;
        chkUpdateRefs.UseVisualStyleBackColor = true;
        // 
        // chkReReReEnabled
        // 
        chkReReReEnabled.AutoSize = true;
        chkReReReEnabled.Location = new Point(3, 178);
        chkReReReEnabled.Name = "chkReReReEnabled";
        chkReReReEnabled.Size = new Size(275, 19);
        chkReReReEnabled.TabIndex = 7;
        chkReReReEnabled.Text = "Reuse recorded resolution of conflicted merges";
        chkReReReEnabled.ThreeState = true;
        chkReReReEnabled.UseVisualStyleBackColor = true;
        // 
        // chkReReReAutoUpdate
        // 
        chkReReReAutoUpdate.AutoSize = true;
        chkReReReAutoUpdate.Location = new Point(3, 203);
        chkReReReAutoUpdate.Name = "chkReReReAutoUpdate";
        chkReReReAutoUpdate.Size = new Size(350, 19);
        chkReReReAutoUpdate.TabIndex = 8;
        chkReReReAutoUpdate.Text = "Automatically apply recorded resolution of conflicted merges";
        chkReReReAutoUpdate.ThreeState = true;
        chkReReReAutoUpdate.UseVisualStyleBackColor = true;
        // 
        // chkStatusShowUntrackedFiles
        // 
        chkStatusShowUntrackedFiles.AutoSize = true;
        chkStatusShowUntrackedFiles.Location = new Point(3, 228);
        chkStatusShowUntrackedFiles.Name = "chkStatusShowUntrackedFiles";
        chkStatusShowUntrackedFiles.Size = new Size(135, 19);
        chkStatusShowUntrackedFiles.TabIndex = 9;
        chkStatusShowUntrackedFiles.Text = "Show untracked files";
        chkStatusShowUntrackedFiles.ThreeState = true;
        chkStatusShowUntrackedFiles.UseVisualStyleBackColor = true;
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
    private CheckBox chkPullFfOnly;
    private CheckBox chkRebaseAutostash;
    private CheckBox chkFetchPrune;
    private CheckBox chkPullRebase;
    private CheckBox chkRebaseAutosquash;
    private CheckBox chkUpdateRefs;
    private CheckBox chkMergeAutoStash;
    private CheckBox chkReReReEnabled;
    private CheckBox chkReReReAutoUpdate;
    private CheckBox chkStatusShowUntrackedFiles;
}
