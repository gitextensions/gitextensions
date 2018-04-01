using System.Windows.Forms;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormCommitCount
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
            this.CommitCount = new System.Windows.Forms.RichTextBox();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbIncludeSubmodules = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // CommitCount
            // 
            this.CommitCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommitCount.Location = new System.Drawing.Point(0, 0);
            this.CommitCount.Name = "CommitCount";
            this.CommitCount.ReadOnly = true;
            this.CommitCount.Size = new System.Drawing.Size(367, 287);
            this.CommitCount.TabIndex = 0;
            this.CommitCount.Text = "";
            // 
            // Loading
            // 
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(367, 287);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 1;
            this.Loading.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbIncludeSubmodules);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 287);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(367, 30);
            this.panel1.TabIndex = 2;
            // 
            // cbIncludeSubmodules
            // 
            this.cbIncludeSubmodules.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbIncludeSubmodules.AutoSize = true;
            this.cbIncludeSubmodules.Checked = true;
            this.cbIncludeSubmodules.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeSubmodules.Location = new System.Drawing.Point(200, 8);
            this.cbIncludeSubmodules.Name = "cbIncludeSubmodules";
            this.cbIncludeSubmodules.Size = new System.Drawing.Size(133, 19);
            this.cbIncludeSubmodules.TabIndex = 0;
            this.cbIncludeSubmodules.Text = "Include submodules";
            this.cbIncludeSubmodules.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.CommitCount);
            this.panel2.Controls.Add(this.Loading);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(367, 287);
            this.panel2.TabIndex = 3;
            // 
            // FormCommitCount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(367, 317);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "FormCommitCount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Commit count";
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox CommitCount;
        private System.Windows.Forms.PictureBox Loading;
        private Panel panel1;
        private CheckBox cbIncludeSubmodules;
        private Panel panel2;
    }
}