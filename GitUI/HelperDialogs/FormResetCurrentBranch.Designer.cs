namespace GitUI.HelperDialogs
{
    partial class FormResetCurrentBranch
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
            System.Windows.Forms.GroupBox gbResetType;
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Soft = new System.Windows.Forms.RadioButton();
            this.Hard = new System.Windows.Forms.RadioButton();
            this.Mixed = new System.Windows.Forms.RadioButton();
            this._NO_TRANSLATE_BranchInfo = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            gbResetType = new System.Windows.Forms.GroupBox();
            gbResetType.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbResetType
            // 
            gbResetType.AutoSize = true;
            gbResetType.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            gbResetType.Controls.Add(this.tableLayoutPanel2);
            gbResetType.Dock = System.Windows.Forms.DockStyle.Fill;
            gbResetType.Location = new System.Drawing.Point(7, 253);
            gbResetType.Name = "gbResetType";
            gbResetType.Padding = new System.Windows.Forms.Padding(16);
            gbResetType.Size = new System.Drawing.Size(689, 231);
            gbResetType.TabIndex = 2;
            gbResetType.TabStop = false;
            gbResetType.Text = "Reset type";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.Soft, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Hard, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.Mixed, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(16, 35);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(657, 180);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // Soft
            // 
            this.Soft.AutoSize = true;
            this.Soft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.Soft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Soft.ForeColor = System.Drawing.Color.Black;
            this.Soft.Location = new System.Drawing.Point(4, 4);
            this.Soft.Margin = new System.Windows.Forms.Padding(4);
            this.Soft.Name = "Soft";
            this.Soft.Padding = new System.Windows.Forms.Padding(4);
            this.Soft.Size = new System.Drawing.Size(649, 51);
            this.Soft.TabIndex = 0;
            this.Soft.Text = "Soft: leave working directory and index untouched";
            this.Soft.UseVisualStyleBackColor = false;
            // 
            // Hard
            // 
            this.Hard.AutoSize = true;
            this.Hard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.Hard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Hard.ForeColor = System.Drawing.Color.Black;
            this.Hard.Location = new System.Drawing.Point(4, 122);
            this.Hard.Margin = new System.Windows.Forms.Padding(4);
            this.Hard.Name = "Hard";
            this.Hard.Padding = new System.Windows.Forms.Padding(4);
            this.Hard.Size = new System.Drawing.Size(649, 54);
            this.Hard.TabIndex = 2;
            this.Hard.Text = "Hard: reset working directory and index\r\n(discard ALL local changes, even uncommi" +
    "tted changes)";
            this.Hard.UseVisualStyleBackColor = false;
            // 
            // Mixed
            // 
            this.Mixed.AutoSize = true;
            this.Mixed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.Mixed.Checked = true;
            this.Mixed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Mixed.ForeColor = System.Drawing.Color.Black;
            this.Mixed.Location = new System.Drawing.Point(4, 63);
            this.Mixed.Margin = new System.Windows.Forms.Padding(4);
            this.Mixed.Name = "Mixed";
            this.Mixed.Padding = new System.Windows.Forms.Padding(4);
            this.Mixed.Size = new System.Drawing.Size(649, 51);
            this.Mixed.TabIndex = 1;
            this.Mixed.TabStop = true;
            this.Mixed.Text = "Mixed: leave working directory untouched, reset index";
            this.Mixed.UseVisualStyleBackColor = false;
            // 
            // _NO_TRANSLATE_BranchInfo
            // 
            this._NO_TRANSLATE_BranchInfo.AutoSize = true;
            this._NO_TRANSLATE_BranchInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_BranchInfo.Location = new System.Drawing.Point(8, 4);
            this._NO_TRANSLATE_BranchInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._NO_TRANSLATE_BranchInfo.Name = "_NO_TRANSLATE_BranchInfo";
            this._NO_TRANSLATE_BranchInfo.Size = new System.Drawing.Size(687, 20);
            this._NO_TRANSLATE_BranchInfo.TabIndex = 0;
            this._NO_TRANSLATE_BranchInfo.Text = "##Reset branch \'{0}\' to:";
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(408, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(136, 38);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(550, 3);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(136, 38);
            this.Cancel.TabIndex = 7;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitSummaryUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(7, 27);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(660, 220);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(689, 220);
            this.commitSummaryUserControl1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_BranchInfo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.commitSummaryUserControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(gbResetType, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(703, 546);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.Cancel);
            this.flowLayoutPanel1.Controls.Add(this.Ok);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(7, 490);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(689, 49);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // FormResetCurrentBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(719, 562);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormResetCurrentBranch";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reset current branch";
            this.Load += new System.EventHandler(this.FormResetCurrentBranch_Load);
            gbResetType.ResumeLayout(false);
            gbResetType.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _NO_TRANSLATE_BranchInfo;
        private System.Windows.Forms.RadioButton Hard;
        private System.Windows.Forms.RadioButton Mixed;
        private System.Windows.Forms.RadioButton Soft;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}