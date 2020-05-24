namespace GitUI.HelperDialogs
{
    partial class FormResetAnotherBranch
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
            if (disposing && (components != null))
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
            this.BranchInfo = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.commitSummaryUserControl = new GitUI.UserControls.CommitSummaryUserControl();
            this.ForceReset = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelResetBranchWarning = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpnlWarning = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tlpnlWarning.SuspendLayout();
            this.SuspendLayout();
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BranchInfo.Location = new System.Drawing.Point(6, 39);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(509, 13);
            this.BranchInfo.TabIndex = 0;
            this.BranchInfo.Text = "Reset local &branch:";
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(320, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(91, 25);
            this.Ok.TabIndex = 3;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(417, 3);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(91, 25);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // commitSummaryUserControl
            // 
            this.commitSummaryUserControl.AutoSize = true;
            this.commitSummaryUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitSummaryUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitSummaryUserControl.Location = new System.Drawing.Point(4, 80);
            this.commitSummaryUserControl.Margin = new System.Windows.Forms.Padding(1);
            this.commitSummaryUserControl.MinimumSize = new System.Drawing.Size(493, 150);
            this.commitSummaryUserControl.Name = "commitSummaryUserControl";
            this.commitSummaryUserControl.Revision = null;
            this.commitSummaryUserControl.Size = new System.Drawing.Size(513, 150);
            this.commitSummaryUserControl.TabIndex = 0;
            // 
            // ForceReset
            // 
            this.ForceReset.AutoSize = true;
            this.ForceReset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ForceReset.Location = new System.Drawing.Point(6, 234);
            this.ForceReset.Name = "ForceReset";
            this.ForceReset.Size = new System.Drawing.Size(509, 17);
            this.ForceReset.TabIndex = 2;
            this.ForceReset.Text = "&Force reset for a non-fast-forward reset";
            this.ForceReset.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::GitUI.Properties.Images.Warning;
            this.pictureBox1.InitialImage = global::GitUI.Properties.Images.Warning;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // labelResetBranchWarning
            // 
            this.labelResetBranchWarning.ForeColor = System.Drawing.Color.Black;
            this.labelResetBranchWarning.Location = new System.Drawing.Point(25, 0);
            this.labelResetBranchWarning.Name = "labelResetBranchWarning";
            this.labelResetBranchWarning.Size = new System.Drawing.Size(250, 20);
            this.labelResetBranchWarning.TabIndex = 0;
            this.labelResetBranchWarning.Text = "You can only reset a branch safely if there is a direct path from it to selected " +
    "revision.\r\nForcing a branch to reset if it has not been merged might leave some " +
    "commits unreachable.";
            // 
            // Branches
            // 
            this.Branches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Branches.Location = new System.Drawing.Point(11, 55);
            this.Branches.Margin = new System.Windows.Forms.Padding(8, 3, 8, 3);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(499, 21);
            this.Branches.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.BranchInfo, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Branches, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.ForceReset, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.commitSummaryUserControl, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.tlpnlWarning, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(519, 292);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.Cancel);
            this.flowLayoutPanel1.Controls.Add(this.Ok);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 256);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(511, 31);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // tlpnlWarning
            // 
            this.tlpnlWarning.AutoSize = true;
            this.tlpnlWarning.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlWarning.ColumnCount = 2;
            this.tlpnlWarning.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlWarning.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlWarning.Controls.Add(this.pictureBox1, 0, 0);
            this.tlpnlWarning.Controls.Add(this.labelResetBranchWarning, 1, 0);
            this.tlpnlWarning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlWarning.Location = new System.Drawing.Point(5, 5);
            this.tlpnlWarning.Margin = new System.Windows.Forms.Padding(2);
            this.tlpnlWarning.Name = "tlpnlWarning";
            this.tlpnlWarning.RowCount = 1;
            this.tlpnlWarning.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlWarning.Size = new System.Drawing.Size(511, 22);
            this.tlpnlWarning.TabIndex = 4;
            // 
            // FormResetAnotherBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(535, 381);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormResetAnotherBranch";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reset branch";
            this.Load += new System.EventHandler(this.FormResetAnotherBranch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tlpnlWarning.ResumeLayout(false);
            this.tlpnlWarning.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl;
        private System.Windows.Forms.CheckBox ForceReset;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelResetBranchWarning;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tlpnlWarning;
    }
}
