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
            Branches = new GitUI.BranchComboBox();
            tlpnlMain = new TableLayoutPanel();
            DeleteRemote = new CheckBox();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tlpnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tlpnlMain);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(394, 73);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Delete);
            ControlsPanel.Location = new Point(0, 73);
            ControlsPanel.Size = new Size(394, 41);
            // 
            // Delete
            // 
            Delete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Delete.AutoSize = true;
            Delete.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Delete.Enabled = false;
            Delete.ForeColor = SystemColors.ControlText;
            Delete.Location = new Point(306, 8);
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
            Branches.BranchesToSelect = null;
            Branches.Dock = DockStyle.Fill;
            Branches.Location = new Point(95, 0);
            Branches.Margin = new Padding(0);
            Branches.Name = "Branches";
            Branches.Size = new Size(286, 28);
            Branches.TabIndex = 1;
            // 
            // tlpnlMain
            // 
            tlpnlMain.ColumnCount = 2;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(labelSelectBranches, 0, 0);
            tlpnlMain.Controls.Add(Branches, 1, 0);
            tlpnlMain.Controls.Add(DeleteRemote, 1, 1);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(9, 9);
            tlpnlMain.Margin = new Padding(0);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 3;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.Size = new Size(376, 55);
            tlpnlMain.TabIndex = 0;
            // 
            // DeleteRemote
            // 
            DeleteRemote.AutoSize = true;
            DeleteRemote.Dock = DockStyle.Fill;
            DeleteRemote.Location = new Point(98, 31);
            DeleteRemote.Name = "DeleteRemote";
            DeleteRemote.Size = new Size(280, 19);
            DeleteRemote.TabIndex = 3;
            DeleteRemote.Text = "Delete branch(es) from &remote repository";
            DeleteRemote.UseVisualStyleBackColor = true;
            DeleteRemote.CheckedChanged += DeleteRemote_CheckedChanged;
            // 
            // FormDeleteRemoteBranch
            // 
            AcceptButton = Delete;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(394, 114);
            HelpButton = true;
            ManualSectionAnchorName = "delete-branch";
            ManualSectionSubfolder = "branches";
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(410, 129);
            Name = "FormDeleteRemoteBranch";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Delete branch";
            MainPanel.ResumeLayout(false);
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
    }
}
