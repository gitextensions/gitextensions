namespace GitUI.CommandsDialogs
{
    partial class RevisionGpgInfoControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.commitSignPicture = new System.Windows.Forms.PictureBox();
            this.tagSignPicture = new System.Windows.Forms.PictureBox();
            this.txtTagGpgInfo = new System.Windows.Forms.TextBox();
            this.txtCommitGpgInfo = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.commitSignPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tagSignPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.commitSignPicture, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tagSignPicture, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtTagGpgInfo, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtCommitGpgInfo, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(476, 247);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // commitSignPicture
            // 
            this.commitSignPicture.Image = global::GitUI.Properties.Images.CommitSignatureOk;
            this.commitSignPicture.Location = new System.Drawing.Point(3, 3);
            this.commitSignPicture.Name = "commitSignPicture";
            this.commitSignPicture.Size = new System.Drawing.Size(32, 32);
            this.commitSignPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.commitSignPicture.TabIndex = 5;
            this.commitSignPicture.TabStop = false;
            // 
            // tagSignPicture
            // 
            this.tagSignPicture.Image = global::GitUI.Properties.Images.CommitSignatureOk;
            this.tagSignPicture.Location = new System.Drawing.Point(3, 126);
            this.tagSignPicture.Name = "tagSignPicture";
            this.tagSignPicture.Size = new System.Drawing.Size(32, 32);
            this.tagSignPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.tagSignPicture.TabIndex = 4;
            this.tagSignPicture.TabStop = false;
            // 
            // txtTagGpgInfo
            // 
            this.txtTagGpgInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTagGpgInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTagGpgInfo.HideSelection = false;
            this.txtTagGpgInfo.Location = new System.Drawing.Point(41, 126);
            this.txtTagGpgInfo.Multiline = true;
            this.txtTagGpgInfo.Name = "txtTagGpgInfo";
            this.txtTagGpgInfo.ReadOnly = true;
            this.txtTagGpgInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTagGpgInfo.Size = new System.Drawing.Size(432, 118);
            this.txtTagGpgInfo.TabIndex = 1;
            this.txtTagGpgInfo.TabStop = false;
            // 
            // txtCommitGpgInfo
            // 
            this.txtCommitGpgInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCommitGpgInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCommitGpgInfo.HideSelection = false;
            this.txtCommitGpgInfo.Location = new System.Drawing.Point(41, 3);
            this.txtCommitGpgInfo.Multiline = true;
            this.txtCommitGpgInfo.Name = "txtCommitGpgInfo";
            this.txtCommitGpgInfo.ReadOnly = true;
            this.txtCommitGpgInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCommitGpgInfo.Size = new System.Drawing.Size(432, 117);
            this.txtCommitGpgInfo.TabIndex = 0;
            this.txtCommitGpgInfo.TabStop = false;
            // 
            // RevisionGpgInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "RevisionGpgInfoControl";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(492, 263);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.commitSignPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tagSignPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtTagGpgInfo;
        private System.Windows.Forms.TextBox txtCommitGpgInfo;
        private System.Windows.Forms.PictureBox tagSignPicture;
        private System.Windows.Forms.PictureBox commitSignPicture;
    }
}
