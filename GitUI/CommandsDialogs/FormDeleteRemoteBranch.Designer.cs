namespace GitUI.CommandsDialogs
{
    partial class FormDeleteRemoteBranch
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
            Delete = new Button();
            labelSelectBranches = new Label();
            Branches = new BranchComboBox();
            DeleteLocalTrackingBranch = new CheckBox();
            tlpnlMain = new TableLayoutPanel();
            DeleteRemote = new CheckBox();
            _NO_TRANSLATE_labelLocalTrackingBranches = new Label();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tlpnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.AutoSize = true;
            MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MainPanel.Controls.Add(tlpnlMain);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(403, 102);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Delete);
            ControlsPanel.Location = new Point(0, 102);
            ControlsPanel.Size = new Size(403, 41);
            // 
            // Delete
            // 
            Delete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Delete.AutoSize = true;
            Delete.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Delete.Enabled = false;
            Delete.ForeColor = SystemColors.ControlText;
            Delete.Location = new Point(315, 8);
            Delete.MinimumSize = new Size(75, 23);
            Delete.Name = "Delete";
            Delete.Size = new Size(75, 25);
            Delete.TabIndex = 2;
            Delete.Text = "&Delete";
            Delete.UseVisualStyleBackColor = true;
            Delete.Click += Delete_Click;
            // 
            // labelSelectBranches
            // 
            labelSelectBranches.AutoSize = true;
            labelSelectBranches.Dock = DockStyle.Fill;
            labelSelectBranches.ForeColor = SystemColors.ControlText;
            labelSelectBranches.Location = new Point(3, 0);
            labelSelectBranches.Name = "labelSelectBranches";
            labelSelectBranches.Size = new Size(89, 28);
            labelSelectBranches.TabIndex = 0;
            labelSelectBranches.Text = "Select &branches";
            labelSelectBranches.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Branches
            // 
            Branches.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Branches.BranchesToSelect = null;
            Branches.Location = new Point(95, 0);
            Branches.Margin = new Padding(0);
            Branches.Name = "Branches";
            Branches.Size = new Size(290, 28);
            Branches.TabIndex = 1;
            Branches.SelectedValueChanged += Branches_SelectedValueChanged;
            // 
            // DeleteLocalTrackingBranch
            // 
            DeleteLocalTrackingBranch.AutoSize = true;
            DeleteLocalTrackingBranch.Dock = DockStyle.Fill;
            DeleteLocalTrackingBranch.Location = new Point(98, 56);
            DeleteLocalTrackingBranch.Name = "DeleteLocalTrackingBranch";
            DeleteLocalTrackingBranch.Size = new Size(284, 19);
            DeleteLocalTrackingBranch.TabIndex = 3;
            DeleteLocalTrackingBranch.Text = "Delete &local tracking branch (if available)";
            DeleteLocalTrackingBranch.UseVisualStyleBackColor = true;
            DeleteLocalTrackingBranch.CheckedChanged += DeleteRemote_CheckedChanged;
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 2;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlMain.Controls.Add(labelSelectBranches, 0, 0);
            tlpnlMain.Controls.Add(Branches, 1, 0);
            tlpnlMain.Controls.Add(DeleteRemote, 1, 1);
            tlpnlMain.Controls.Add(DeleteLocalTrackingBranch, 1, 2);
            tlpnlMain.Controls.Add(_NO_TRANSLATE_labelLocalTrackingBranches, 1, 3);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(9, 9);
            tlpnlMain.Margin = new Padding(0);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.Size = new Size(385, 80);
            tlpnlMain.TabIndex = 0;
            // 
            // DeleteRemote
            // 
            DeleteRemote.AutoSize = true;
            DeleteRemote.Dock = DockStyle.Fill;
            DeleteRemote.Location = new Point(98, 31);
            DeleteRemote.Name = "DeleteRemote";
            DeleteRemote.Size = new Size(284, 19);
            DeleteRemote.TabIndex = 2;
            DeleteRemote.Text = "Delete branch(es) from &remote repository";
            DeleteRemote.UseVisualStyleBackColor = true;
            DeleteRemote.CheckedChanged += DeleteRemote_CheckedChanged;
            // 
            // _NO_TRANSLATE_labelLocalTrackingBranches
            // 
            _NO_TRANSLATE_labelLocalTrackingBranches.AutoSize = true;
            _NO_TRANSLATE_labelLocalTrackingBranches.Location = new Point(98, 78);
            _NO_TRANSLATE_labelLocalTrackingBranches.Name = "_NO_TRANSLATE_labelLocalTrackingBranches";
            _NO_TRANSLATE_labelLocalTrackingBranches.Size = new Size(184, 15);
            _NO_TRANSLATE_labelLocalTrackingBranches.TabIndex = 4;
            _NO_TRANSLATE_labelLocalTrackingBranches.Text = "local tracking deletion candidates";
            // 
            // FormDeleteRemoteBranch
            // 
            AcceptButton = Delete;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            ClientSize = new Size(403, 139);
            HelpButton = true;
            ManualSectionAnchorName = "delete-branch";
            ManualSectionSubfolder = "branches";
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(403, 102);
            Name = "FormDeleteRemoteBranch";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Delete branch";
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Delete;
        private Label labelSelectBranches;
        private CheckBox DeleteRemote;
        private BranchComboBox Branches;
        private TableLayoutPanel tlpnlMain;
        private CheckBox DeleteLocalTrackingBranch;
        private Label _NO_TRANSLATE_labelLocalTrackingBranches;
    }
}
