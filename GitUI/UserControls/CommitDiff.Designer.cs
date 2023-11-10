namespace GitUI.UserControls
{
    partial class CommitDiff
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            commitInfo = new GitUI.CommitInfo.CommitInfo();
            splitContainer2 = new SplitContainer();
            DiffFiles = new GitUI.FileStatusList();
            DiffText = new GitUI.Editor.FileViewer();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(commitInfo);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(717, 529);
            splitContainer1.SplitterDistance = 160;
            splitContainer1.TabIndex = 1;
            // 
            // commitInfo
            // 
            commitInfo.Dock = DockStyle.Fill;
            commitInfo.Location = new Point(0, 0);
            commitInfo.Margin = new Padding(0);
            commitInfo.Name = "commitInfo";
            commitInfo.Size = new Size(717, 160);
            commitInfo.TabIndex = 0;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(DiffFiles);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(DiffText);
            splitContainer2.Size = new Size(717, 365);
            splitContainer2.SplitterDistance = 238;
            splitContainer2.TabIndex = 0;
            // 
            // DiffFiles
            // 
            DiffFiles.Dock = DockStyle.Fill;
            DiffFiles.Location = new Point(0, 0);
            DiffFiles.Margin = new Padding(0);
            DiffFiles.Name = "DiffFiles";
            DiffFiles.Size = new Size(238, 365);
            DiffFiles.TabIndex = 0;
            DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;
            // 
            // DiffText
            // 
            DiffText.AutoSize = true;
            DiffText.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            DiffText.Dock = DockStyle.Fill;
            DiffText.Location = new Point(0, 0);
            DiffText.Margin = new Padding(0);
            DiffText.Name = "DiffText";
            DiffText.Size = new Size(475, 365);
            DiffText.TabIndex = 0;
            // 
            // CommitDiff
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(splitContainer1);
            MinimumSize = new Size(150, 148);
            Name = "CommitDiff";
            Size = new Size(885, 578);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer2)).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private CommitInfo.CommitInfo commitInfo;
        private SplitContainer splitContainer2;
        private FileStatusList DiffFiles;
        private Editor.FileViewer DiffText;
    }
}
