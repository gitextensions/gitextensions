namespace GitUI.CommandsDialogs
{
    partial class FormDeleteBranch
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
            labelWarning = new Label();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tlpnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tlpnlMain);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(412, 50);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Delete);
            ControlsPanel.Controls.Add(labelWarning);
            ControlsPanel.Location = new Point(0, 50);
            ControlsPanel.Size = new Size(412, 41);
            // 
            // Delete
            // 
            Delete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Delete.AutoSize = true;
            Delete.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Delete.ForeColor = SystemColors.ControlText;
            Delete.Location = new Point(324, 8);
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
            Branches.Size = new Size(299, 28);
            Branches.TabIndex = 1;
            Branches.SelectedValueChanged += Branches_SelectedValueChanged;
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 2;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(labelSelectBranches, 0, 0);
            tlpnlMain.Controls.Add(Branches, 1, 0);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(9, 9);
            tlpnlMain.Margin = new Padding(0);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 2;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.Size = new Size(394, 32);
            tlpnlMain.TabIndex = 0;
            // 
            // labelWarning
            // 
            labelWarning.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            labelWarning.AutoSize = true;
            labelWarning.ForeColor = Color.Red;
            labelWarning.Location = new Point(272, 5);
            labelWarning.Name = "labelWarning";
            labelWarning.Size = new Size(46, 31);
            labelWarning.TabIndex = 3;
            labelWarning.Text = "             ";
            labelWarning.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FormDeleteBranch
            // 
            AcceptButton = Delete;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(412, 91);
            HelpButton = true;
            ManualSectionAnchorName = "delete-branch";
            ManualSectionSubfolder = "branches";
            MaximizeBox = false;
            MaximumSize = new Size(950, 130);
            MinimizeBox = false;
            MinimumSize = new Size(420, 130);
            Name = "FormDeleteBranch";
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
        private BranchComboBox Branches;
        private TableLayoutPanel tlpnlMain;
        private Label labelWarning;
    }
}
