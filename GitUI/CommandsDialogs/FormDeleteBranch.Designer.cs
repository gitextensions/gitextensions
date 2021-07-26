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
            this.Delete = new System.Windows.Forms.Button();
            this.labelSelectBranches = new System.Windows.Forms.Label();
            this.Branches = new GitUI.BranchComboBox();
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.MainPanel.SuspendLayout();
            this.tlpnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.tlpnlMain);
            this.MainPanel.Padding = new System.Windows.Forms.Padding(9);
            this.MainPanel.Size = new System.Drawing.Size(394, 58);
            // 
            // Delete
            // 
            this.Delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Delete.AutoSize = true;
            this.Delete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Delete.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Delete.Location = new System.Drawing.Point(310, 65);
            this.Delete.MinimumSize = new System.Drawing.Size(75, 23);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 23);
            this.Delete.TabIndex = 2;
            this.Delete.Text = "&Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // labelSelectBranches
            // 
            this.labelSelectBranches.AutoSize = true;
            this.labelSelectBranches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSelectBranches.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelSelectBranches.Location = new System.Drawing.Point(3, 0);
            this.labelSelectBranches.Name = "labelSelectBranches";
            this.labelSelectBranches.Size = new System.Drawing.Size(84, 28);
            this.labelSelectBranches.TabIndex = 0;
            this.labelSelectBranches.Text = "Select &branches";
            this.labelSelectBranches.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Branches
            // 
            this.Branches.BranchesToSelect = null;
            this.Branches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Branches.Location = new System.Drawing.Point(90, 0);
            this.Branches.Margin = new System.Windows.Forms.Padding(0);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(286, 28);
            this.Branches.TabIndex = 1;
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.ColumnCount = 2;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlMain.Controls.Add(this.labelSelectBranches, 0, 0);
            this.tlpnlMain.Controls.Add(this.Branches, 1, 0);
            this.tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlMain.Location = new System.Drawing.Point(9, 9);
            this.tlpnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 2;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlMain.Size = new System.Drawing.Size(376, 40);
            this.tlpnlMain.TabIndex = 0;
            // 
            // FormDeleteBranch
            // 
            this.AcceptButton = this.Delete;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(394, 90);
            this.Controls.Add(this.Delete);
            this.HelpButton = true;
            this.ManualSectionAnchorName = "delete-branch";
            this.ManualSectionSubfolder = "branches";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(410, 129);
            this.Name = "FormDeleteBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Delete branch";
            this.Controls.SetChildIndex(this.Delete, 0);
            this.Controls.SetChildIndex(this.MainPanel, 0);
            this.MainPanel.ResumeLayout(false);
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Label labelSelectBranches;
        private BranchComboBox Branches;
        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
    }
}
