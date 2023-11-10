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
            tableLayoutPanel1 = new TableLayoutPanel();
            commitSignPicture = new PictureBox();
            tagSignPicture = new PictureBox();
            txtTagGpgInfo = new TextBox();
            txtCommitGpgInfo = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(commitSignPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(tagSignPicture)).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(commitSignPicture, 0, 0);
            tableLayoutPanel1.Controls.Add(tagSignPicture, 0, 1);
            tableLayoutPanel1.Controls.Add(txtTagGpgInfo, 1, 1);
            tableLayoutPanel1.Controls.Add(txtCommitGpgInfo, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(8, 8);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(476, 247);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // commitSignPicture
            // 
            commitSignPicture.Image = Properties.Images.CommitSignatureOk;
            commitSignPicture.Location = new Point(3, 3);
            commitSignPicture.Name = "commitSignPicture";
            commitSignPicture.Size = new Size(32, 32);
            commitSignPicture.SizeMode = PictureBoxSizeMode.AutoSize;
            commitSignPicture.TabIndex = 5;
            commitSignPicture.TabStop = false;
            // 
            // tagSignPicture
            // 
            tagSignPicture.Image = Properties.Images.CommitSignatureOk;
            tagSignPicture.Location = new Point(3, 126);
            tagSignPicture.Name = "tagSignPicture";
            tagSignPicture.Size = new Size(32, 32);
            tagSignPicture.SizeMode = PictureBoxSizeMode.AutoSize;
            tagSignPicture.TabIndex = 4;
            tagSignPicture.TabStop = false;
            // 
            // txtTagGpgInfo
            // 
            txtTagGpgInfo.BorderStyle = BorderStyle.None;
            txtTagGpgInfo.Dock = DockStyle.Fill;
            txtTagGpgInfo.HideSelection = false;
            txtTagGpgInfo.Location = new Point(41, 126);
            txtTagGpgInfo.Multiline = true;
            txtTagGpgInfo.Name = "txtTagGpgInfo";
            txtTagGpgInfo.ReadOnly = true;
            txtTagGpgInfo.ScrollBars = ScrollBars.Vertical;
            txtTagGpgInfo.Size = new Size(432, 118);
            txtTagGpgInfo.TabIndex = 1;
            txtTagGpgInfo.TabStop = false;
            // 
            // txtCommitGpgInfo
            // 
            txtCommitGpgInfo.BorderStyle = BorderStyle.None;
            txtCommitGpgInfo.Dock = DockStyle.Fill;
            txtCommitGpgInfo.HideSelection = false;
            txtCommitGpgInfo.Location = new Point(41, 3);
            txtCommitGpgInfo.Multiline = true;
            txtCommitGpgInfo.Name = "txtCommitGpgInfo";
            txtCommitGpgInfo.ReadOnly = true;
            txtCommitGpgInfo.ScrollBars = ScrollBars.Vertical;
            txtCommitGpgInfo.Size = new Size(432, 117);
            txtCommitGpgInfo.TabIndex = 0;
            txtCommitGpgInfo.TabStop = false;
            // 
            // RevisionGpgInfo
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "RevisionGpgInfoControl";
            Padding = new Padding(8);
            Size = new Size(492, 263);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(commitSignPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(tagSignPicture)).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TextBox txtTagGpgInfo;
        private TextBox txtCommitGpgInfo;
        private PictureBox tagSignPicture;
        private PictureBox commitSignPicture;
    }
}
