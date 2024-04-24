using GitUI.UserControls.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class AdvancedSettingsPage
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedSettingsPage));
            tableLayoutPanel2 = new TableLayoutPanel();
            grpUpdates = new GroupBox();
            tableLayoutPanel6 = new TableLayoutPanel();
            chkCheckForUpdates = new CheckBox();
            chkCheckForRCVersions = new GitUI.UserControls.Settings.SettingsCheckBox();
            grpCommit = new GroupBox();
            tableLayoutPanel5 = new TableLayoutPanel();
            chkCommitAndPushForcedWhenAmend = new CheckBox();
            CheckoutGB = new GroupBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            chkAlwaysShowCheckoutDlg = new CheckBox();
            chkUseLocalChangesAction = new CheckBox();
            GeneralGB = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            chkAlwaysShowAdvOpt = new CheckBox();
            chkDontSHowHelpImages = new CheckBox();
            chkConsoleEmulator = new GitUI.UserControls.Settings.SettingsCheckBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            chkAutoNormaliseBranchName = new GitUI.UserControls.Settings.SettingsCheckBox();
            label1 = new Label();
            cboAutoNormaliseSymbol = new ComboBox();
            tableLayoutPanel2.SuspendLayout();
            grpUpdates.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            grpCommit.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            CheckoutGB.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            GeneralGB.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(grpUpdates, 0, 4);
            tableLayoutPanel2.Controls.Add(grpCommit, 0, 2);
            tableLayoutPanel2.Controls.Add(CheckoutGB, 0, 0);
            tableLayoutPanel2.Controls.Add(GeneralGB, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(8, 8);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(1600, 753);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // grpUpdates
            // 
            grpUpdates.AutoSize = true;
            grpUpdates.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpUpdates.Controls.Add(tableLayoutPanel6);
            grpUpdates.Dock = DockStyle.Top;
            grpUpdates.Location = new Point(3, 399);
            grpUpdates.Name = "grpUpdates";
            grpUpdates.Padding = new Padding(8);
            grpUpdates.Size = new Size(1448, 82);
            grpUpdates.TabIndex = 5;
            grpUpdates.TabStop = false;
            grpUpdates.Text = "Updates";
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.AutoSize = true;
            tableLayoutPanel6.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel6.ColumnCount = 1;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.Controls.Add(chkCheckForUpdates, 0, 2);
            tableLayoutPanel6.Controls.Add(chkCheckForRCVersions, 0, 4);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(8, 24);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 6;
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.RowStyles.Add(new RowStyle());
            tableLayoutPanel6.Size = new Size(1432, 50);
            tableLayoutPanel6.TabIndex = 1;
            // 
            // chkCheckForUpdates
            // 
            chkCheckForUpdates.AutoSize = true;
            chkCheckForUpdates.Checked = true;
            chkCheckForUpdates.CheckState = CheckState.Checked;
            chkCheckForUpdates.Dock = DockStyle.Fill;
            chkCheckForUpdates.Location = new Point(3, 3);
            chkCheckForUpdates.Name = "chkCheckForUpdates";
            chkCheckForUpdates.Size = new Size(1426, 19);
            chkCheckForUpdates.TabIndex = 2;
            chkCheckForUpdates.Text = "Check for updates weekly";
            chkCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // chkCheckForRCVersions
            // 
            chkCheckForRCVersions.AutoSize = true;
            chkCheckForRCVersions.Dock = DockStyle.Fill;
            chkCheckForRCVersions.Location = new Point(3, 28);
            chkCheckForRCVersions.ManualSectionAnchorName = "updates-check-for-release-candidate-versions";
            chkCheckForRCVersions.Name = "chkCheckForRCVersions";
            chkCheckForRCVersions.Size = new Size(1426, 19);
            chkCheckForRCVersions.TabIndex = 3;
            chkCheckForRCVersions.Text = "Check for release candidate versions";
            chkCheckForRCVersions.ToolTipText = resources.GetString("chkCheckForRCVersions.ToolTip");
            // 
            // grpCommit
            // 
            grpCommit.AutoSize = true;
            grpCommit.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpCommit.Controls.Add(tableLayoutPanel5);
            grpCommit.Dock = DockStyle.Top;
            grpCommit.Location = new Point(3, 276);
            grpCommit.Name = "grpCommit";
            grpCommit.Padding = new Padding(8);
            grpCommit.Size = new Size(1594, 53);
            grpCommit.TabIndex = 3;
            grpCommit.TabStop = false;
            grpCommit.Text = "Commit";
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.AutoSize = true;
            tableLayoutPanel5.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Controls.Add(chkCommitAndPushForcedWhenAmend, 0, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(8, 22);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 2;
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.Size = new Size(1578, 23);
            tableLayoutPanel5.TabIndex = 0;
            // 
            // chkCommitAndPushForcedWhenAmend
            // 
            chkCommitAndPushForcedWhenAmend.AutoSize = true;
            chkCommitAndPushForcedWhenAmend.Dock = DockStyle.Fill;
            chkCommitAndPushForcedWhenAmend.Location = new Point(3, 3);
            chkCommitAndPushForcedWhenAmend.Name = "chkCommitAndPushForcedWhenAmend";
            chkCommitAndPushForcedWhenAmend.Size = new Size(1572, 17);
            chkCommitAndPushForcedWhenAmend.TabIndex = 0;
            chkCommitAndPushForcedWhenAmend.Text = "Push forced with lease when Commit && Push action is performed with Amend option " +
    "checked";
            chkCommitAndPushForcedWhenAmend.UseVisualStyleBackColor = true;
            // 
            // CheckoutGB
            // 
            CheckoutGB.AutoSize = true;
            CheckoutGB.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CheckoutGB.Controls.Add(tableLayoutPanel3);
            CheckoutGB.Dock = DockStyle.Top;
            CheckoutGB.Location = new Point(3, 3);
            CheckoutGB.Name = "CheckoutGB";
            CheckoutGB.Padding = new Padding(8);
            CheckoutGB.Size = new Size(1594, 89);
            CheckoutGB.TabIndex = 0;
            CheckoutGB.TabStop = false;
            CheckoutGB.Text = "Checkout";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(chkAlwaysShowCheckoutDlg, 0, 0);
            tableLayoutPanel3.Controls.Add(chkUseLocalChangesAction, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(8, 22);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(1578, 59);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // chkAlwaysShowCheckoutDlg
            // 
            chkAlwaysShowCheckoutDlg.AutoSize = true;
            chkAlwaysShowCheckoutDlg.Dock = DockStyle.Fill;
            chkAlwaysShowCheckoutDlg.Location = new Point(3, 3);
            chkAlwaysShowCheckoutDlg.Name = "chkAlwaysShowCheckoutDlg";
            chkAlwaysShowCheckoutDlg.Size = new Size(1572, 17);
            chkAlwaysShowCheckoutDlg.TabIndex = 0;
            chkAlwaysShowCheckoutDlg.Text = "Always show checkout dialog";
            chkAlwaysShowCheckoutDlg.UseVisualStyleBackColor = true;
            // 
            // chkUseLocalChangesAction
            // 
            chkUseLocalChangesAction.AutoSize = true;
            chkUseLocalChangesAction.CheckAlign = ContentAlignment.TopLeft;
            chkUseLocalChangesAction.Dock = DockStyle.Fill;
            chkUseLocalChangesAction.Location = new Point(3, 26);
            chkUseLocalChangesAction.Name = "chkUseLocalChangesAction";
            chkUseLocalChangesAction.Size = new Size(1572, 30);
            chkUseLocalChangesAction.TabIndex = 1;
            chkUseLocalChangesAction.Text = "Use last chosen \"local changes\" action as default action.\r\nThis action will be pe" +
    "rformed without warning while checking out branch.";
            chkUseLocalChangesAction.UseVisualStyleBackColor = true;
            // 
            // GeneralGB
            // 
            GeneralGB.AutoSize = true;
            GeneralGB.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            GeneralGB.Controls.Add(tableLayoutPanel1);
            GeneralGB.Dock = DockStyle.Fill;
            GeneralGB.Location = new Point(3, 98);
            GeneralGB.Name = "GeneralGB";
            GeneralGB.Padding = new Padding(8);
            GeneralGB.Size = new Size(1594, 172);
            GeneralGB.TabIndex = 1;
            GeneralGB.TabStop = false;
            GeneralGB.Text = "General";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(chkAlwaysShowAdvOpt, 0, 1);
            tableLayoutPanel1.Controls.Add(chkDontSHowHelpImages, 0, 0);
            tableLayoutPanel1.Controls.Add(chkConsoleEmulator, 0, 4);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 0, 5);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(8, 22);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1578, 142);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // chkAlwaysShowAdvOpt
            // 
            chkAlwaysShowAdvOpt.AutoSize = true;
            chkAlwaysShowAdvOpt.Dock = DockStyle.Fill;
            chkAlwaysShowAdvOpt.Location = new Point(3, 26);
            chkAlwaysShowAdvOpt.Name = "chkAlwaysShowAdvOpt";
            chkAlwaysShowAdvOpt.Size = new Size(1572, 17);
            chkAlwaysShowAdvOpt.TabIndex = 1;
            chkAlwaysShowAdvOpt.Text = "Always show advanced options";
            chkAlwaysShowAdvOpt.UseVisualStyleBackColor = true;
            // 
            // chkDontSHowHelpImages
            // 
            chkDontSHowHelpImages.AutoSize = true;
            chkDontSHowHelpImages.Dock = DockStyle.Fill;
            chkDontSHowHelpImages.Location = new Point(3, 3);
            chkDontSHowHelpImages.Name = "chkDontSHowHelpImages";
            chkDontSHowHelpImages.Size = new Size(1572, 17);
            chkDontSHowHelpImages.TabIndex = 0;
            chkDontSHowHelpImages.Text = "Don\'t show help images";
            chkDontSHowHelpImages.UseVisualStyleBackColor = true;
            // 
            // chkConsoleEmulator
            // 
            chkConsoleEmulator.AutoSize = true;
            chkConsoleEmulator.Dock = DockStyle.Fill;
            chkConsoleEmulator.Location = new Point(3, 72);
            chkConsoleEmulator.ManualSectionAnchorName = "general-use-console-emulator-for-console-output-in-command-dialogs";
            chkConsoleEmulator.Name = "chkConsoleEmulator";
            chkConsoleEmulator.Size = new Size(1572, 17);
            chkConsoleEmulator.TabIndex = 3;
            chkConsoleEmulator.Text = "Use Console Emulator for console output in command dialogs";
            chkConsoleEmulator.ToolTipText = resources.GetString("chkConsoleEmulator.ToolTip");
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.AutoSize = true;
            tableLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.Controls.Add(chkAutoNormaliseBranchName, 0, 0);
            tableLayoutPanel4.Controls.Add(label1, 0, 1);
            tableLayoutPanel4.Controls.Add(cboAutoNormaliseSymbol, 1, 1);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(0, 92);
            tableLayoutPanel4.Margin = new Padding(0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(1578, 50);
            tableLayoutPanel4.TabIndex = 5;
            // 
            // chkAutoNormaliseBranchName
            // 
            chkAutoNormaliseBranchName.AutoSize = true;
            tableLayoutPanel4.SetColumnSpan(chkAutoNormaliseBranchName, 2);
            chkAutoNormaliseBranchName.Dock = DockStyle.Fill;
            chkAutoNormaliseBranchName.Location = new Point(3, 3);
            chkAutoNormaliseBranchName.ManualSectionAnchorName = "general-auto-normalise-branch-name";
            chkAutoNormaliseBranchName.Name = "chkAutoNormaliseBranchName";
            chkAutoNormaliseBranchName.Size = new Size(1572, 17);
            chkAutoNormaliseBranchName.TabIndex = 0;
            chkAutoNormaliseBranchName.Text = "Auto normalise branch name";
            chkAutoNormaliseBranchName.ToolTipText =
                "Controls whether branch name should be automatically normalised as per git branch" +
                " naming rules.\r\nIf enabled, any illegal symbols will be replaced with the replac" +
                "ement symbol of your choice.";
            chkAutoNormaliseBranchName.CheckedChanged += chkAutoNormaliseBranchName_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 23);
            label1.Name = "label1";
            label1.Size = new Size(78, 27);
            label1.TabIndex = 1;
            label1.Text = "Symbol to use:";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboAutoNormaliseSymbol
            // 
            cboAutoNormaliseSymbol.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAutoNormaliseSymbol.Enabled = false;
            cboAutoNormaliseSymbol.FormattingEnabled = true;
            cboAutoNormaliseSymbol.Location = new Point(87, 26);
            cboAutoNormaliseSymbol.Name = "cboAutoNormaliseSymbol";
            cboAutoNormaliseSymbol.Size = new Size(42, 21);
            cboAutoNormaliseSymbol.TabIndex = 2;
            // 
            // AdvancedSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tableLayoutPanel2);
            Name = "AdvancedSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1616, 769);
            Text = "Advanced";
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            grpUpdates.ResumeLayout(false);
            grpUpdates.PerformLayout();
            tableLayoutPanel6.ResumeLayout(false);
            tableLayoutPanel6.PerformLayout();
            grpCommit.ResumeLayout(false);
            grpCommit.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            CheckoutGB.ResumeLayout(false);
            CheckoutGB.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            GeneralGB.ResumeLayout(false);
            GeneralGB.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel2;
        private GroupBox CheckoutGB;
        private GroupBox GeneralGB;
        private TableLayoutPanel tableLayoutPanel1;
        private CheckBox chkDontSHowHelpImages;
        private TableLayoutPanel tableLayoutPanel3;
        private CheckBox chkAlwaysShowCheckoutDlg;
        private CheckBox chkUseLocalChangesAction;
        private CheckBox chkAlwaysShowAdvOpt;
        private SettingsCheckBox chkConsoleEmulator;
        private TableLayoutPanel tableLayoutPanel4;
        private SettingsCheckBox chkAutoNormaliseBranchName;
        private Label label1;
        private ComboBox cboAutoNormaliseSymbol;
        private GroupBox grpCommit;
        private TableLayoutPanel tableLayoutPanel5;
        private CheckBox chkCommitAndPushForcedWhenAmend;
        private GroupBox grpUpdates;
        private TableLayoutPanel tableLayoutPanel6;
        private CheckBox chkCheckForUpdates;
        private SettingsCheckBox chkCheckForRCVersions;
    }
}
