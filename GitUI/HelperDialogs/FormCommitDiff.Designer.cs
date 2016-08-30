﻿using GitUI.Editor;

namespace GitUI.HelperDialogs
{
    partial class FormCommitDiff
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.DiffText = new GitUI.Editor.FileViewer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.commitInfo = new GitUI.CommitInfo.CommitInfo();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();

            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();

            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.DiffFiles);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.DiffText);
            this.splitContainer2.Size = new System.Drawing.Size(717, 365);
            this.splitContainer2.SplitterDistance = 238;
            this.splitContainer2.TabIndex = 0;
            // 
            // DiffFiles
            // 
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Size = new System.Drawing.Size(238, 365);
            this.DiffFiles.TabIndex = 0;
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFiles_SelectedIndexChanged);
            // 
            // DiffText
            // 
            this.DiffText.AutoSize = true;
            this.DiffText.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DiffText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffText.Location = new System.Drawing.Point(0, 0);
            this.DiffText.Margin = new System.Windows.Forms.Padding(4);
            this.DiffText.Name = "DiffText";
            this.DiffText.Size = new System.Drawing.Size(475, 365);
            this.DiffText.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.commitInfo);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(717, 529);
            this.splitContainer1.SplitterDistance = 160;
            this.splitContainer1.TabIndex = 1;
            // 
            // commitInfo
            // 
            this.commitInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitInfo.Location = new System.Drawing.Point(0, 0);
            this.commitInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.commitInfo.Name = "commitInfo";
            this.commitInfo.Size = new System.Drawing.Size(717, 160);
            this.commitInfo.TabIndex = 0;
            // 
            // FormCommitDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(717, 529);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(150, 148);
            this.Name = "FormCommitDiff";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Diff";
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();

            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();

            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer2;
        private FileStatusList DiffFiles;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FileViewer DiffText;
        private CommitInfo.CommitInfo commitInfo;
    }
}