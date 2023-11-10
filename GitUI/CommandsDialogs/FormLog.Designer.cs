using GitUI.Editor;

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
            splitContainer1 = new SplitContainer();
            RevisionGrid = new GitUI.RevisionGridControl();
            splitContainer3 = new SplitContainer();
            DiffFiles = new GitUI.FileStatusList();
            diffViewer = new GitUI.Editor.FileViewer();

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();

            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(splitContainer3)).BeginInit();

            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(RevisionGrid);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer3);
            splitContainer1.Size = new Size(750, 529);
            splitContainer1.SplitterDistance = 205;
            splitContainer1.TabIndex = 0;
            // 
            // RevisionGrid
            // 
            RevisionGrid.Dock = DockStyle.Fill;
            RevisionGrid.Location = new Point(0, 0);
            RevisionGrid.Margin = new Padding(4);
            RevisionGrid.Name = "RevisionGrid";
            RevisionGrid.Size = new Size(750, 205);
            RevisionGrid.TabIndex = 1;
            RevisionGrid.SelectionChanged += RevisionGridSelectionChanged;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.FixedPanel = FixedPanel.Panel1;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(DiffFiles);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(diffViewer);
            splitContainer3.Size = new Size(750, 320);
            splitContainer3.SplitterDistance = 188;
            splitContainer3.TabIndex = 1;
            // 
            // DiffFiles
            // 
            DiffFiles.Dock = DockStyle.Fill;
            DiffFiles.Location = new Point(0, 0);
            DiffFiles.Name = "DiffFiles";
            DiffFiles.Size = new Size(188, 320);
            DiffFiles.TabIndex = 0;
            DiffFiles.SelectedIndexChanged += DiffFilesSelectedIndexChanged;
            // 
            // diffViewer
            // 
            diffViewer.Dock = DockStyle.Fill;
            diffViewer.Location = new Point(0, 0);
            diffViewer.Margin = new Padding(4);
            diffViewer.Name = "diffViewer";
            diffViewer.Size = new Size(558, 320);
            diffViewer.TabIndex = 1;
            // 
            // FormDiff
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(750, 529);
            Controls.Add(splitContainer1);
            MinimumSize = new Size(250, 250);
            Name = "FormDiff";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Diff";
            Load += FormDiffLoad;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();

            splitContainer1.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(splitContainer3)).EndInit();

            splitContainer3.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private SplitContainer splitContainer3;
        private FileStatusList DiffFiles;
        private RevisionGridControl RevisionGrid;
        private FileViewer diffViewer;
    }
}
