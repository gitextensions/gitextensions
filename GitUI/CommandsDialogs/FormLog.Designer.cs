﻿using GitUI.Editor;

namespace GitUI.CommandsDialogs
{
    partial class FormLog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.RevisionGrid = new GitUI.RevisionGridControl();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.DiffFiles = new GitUI.FileStatusList();
            this.diffViewer = new GitUI.Editor.FileViewer();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();

            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();

            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.RevisionGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(750, 529);
            this.splitContainer1.SplitterDistance = 205;
            this.splitContainer1.TabIndex = 0;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.Margin = new System.Windows.Forms.Padding(4);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.Size = new System.Drawing.Size(750, 205);
            this.RevisionGrid.TabIndex = 1;
            this.RevisionGrid.SelectionChanged += new System.EventHandler(this.RevisionGridSelectionChanged);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.DiffFiles);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.diffViewer);
            this.splitContainer3.Size = new System.Drawing.Size(750, 320);
            this.splitContainer3.SplitterDistance = 188;
            this.splitContainer3.TabIndex = 1;
            // 
            // DiffFiles
            // 
            this.DiffFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiffFiles.Location = new System.Drawing.Point(0, 0);
            this.DiffFiles.Name = "DiffFiles";
            this.DiffFiles.Size = new System.Drawing.Size(188, 320);
            this.DiffFiles.TabIndex = 0;
            this.DiffFiles.SelectedIndexChanged += new System.EventHandler(this.DiffFilesSelectedIndexChanged);
            // 
            // diffViewer
            // 
            this.diffViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diffViewer.Location = new System.Drawing.Point(0, 0);
            this.diffViewer.Margin = new System.Windows.Forms.Padding(4);
            this.diffViewer.Name = "diffViewer";
            this.diffViewer.Size = new System.Drawing.Size(558, 320);
            this.diffViewer.TabIndex = 1;
            // 
            // FormDiff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(750, 529);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(250, 250);
            this.Name = "FormDiff";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Diff";
            this.Load += new System.EventHandler(this.FormDiffLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();

            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();

            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private FileStatusList DiffFiles;
        private RevisionGridControl RevisionGrid;
        private FileViewer diffViewer;
    }
}
