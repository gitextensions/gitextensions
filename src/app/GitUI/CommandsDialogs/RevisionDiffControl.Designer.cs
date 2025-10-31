using System.Windows.Forms;
using GitUI.CommandsDialogs.Menus;

namespace GitUI.CommandsDialogs;

partial class RevisionDiffControl
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
        DiffSplitContainer = new SplitContainer();
        LeftSplitContainer = new SplitContainer();
        DiffFiles = new FileStatusList();
        DiffText = new GitUI.Editor.FileViewer();
        BlameControl = new GitUI.Blame.BlameControl();
        ((System.ComponentModel.ISupportInitialize)DiffSplitContainer).BeginInit();
        DiffSplitContainer.Panel1.SuspendLayout();
        DiffSplitContainer.Panel2.SuspendLayout();
        DiffSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)LeftSplitContainer).BeginInit();
        LeftSplitContainer.Panel1.SuspendLayout();
        LeftSplitContainer.SuspendLayout();
        SuspendLayout();
        // 
        // DiffSplitContainer
        // 
        DiffSplitContainer.BackColor = SystemColors.Control;
        DiffSplitContainer.Dock = DockStyle.Fill;
        DiffSplitContainer.FixedPanel = FixedPanel.Panel1;
        DiffSplitContainer.Location = new Point(0, 0);
        DiffSplitContainer.Margin = new Padding(0);
        DiffSplitContainer.Name = "DiffSplitContainer";
        // 
        // DiffSplitContainer.Panel1
        // 
        DiffSplitContainer.Panel1.Controls.Add(LeftSplitContainer);
        // 
        // DiffSplitContainer.Panel2
        // 
        DiffSplitContainer.Panel2.Controls.Add(DiffText);
        DiffSplitContainer.Panel2.Controls.Add(BlameControl);
        DiffSplitContainer.Size = new Size(850, 415);
        DiffSplitContainer.SplitterDistance = 300;
        DiffSplitContainer.SplitterWidth = 7;
        DiffSplitContainer.TabIndex = 0;
        // 
        // LeftSplitContainer
        // 
        LeftSplitContainer.BackColor = SystemColors.Control;
        LeftSplitContainer.Dock = DockStyle.Fill;
        LeftSplitContainer.FixedPanel = FixedPanel.Panel2;
        LeftSplitContainer.Location = new Point(0, 0);
        LeftSplitContainer.Margin = new Padding(0);
        LeftSplitContainer.Name = "LeftSplitContainer";
        LeftSplitContainer.Orientation = Orientation.Horizontal;
        // 
        // LeftSplitContainer.Panel1
        // 
        LeftSplitContainer.Panel1.BackColor = SystemColors.Window;
        LeftSplitContainer.Panel1.Controls.Add(DiffFiles);
        LeftSplitContainer.Panel2Collapsed = true;
        LeftSplitContainer.Size = new Size(300, 415);
        LeftSplitContainer.SplitterDistance = 197;
        LeftSplitContainer.SplitterWidth = 7;
        LeftSplitContainer.TabIndex = 0;
        LeftSplitContainer.TabStop = false;
        // 
        // DiffFiles
        // 
        DiffFiles.Dock = DockStyle.Fill;
        DiffFiles.GroupByRevision = true;
        DiffFiles.Location = new Point(0, 0);
        DiffFiles.Margin = new Padding(0);
        DiffFiles.Name = "DiffFiles";
        DiffFiles.SelectFirstItemOnSetItems = false;
        DiffFiles.Size = new Size(300, 415);
        DiffFiles.TabIndex = 1;
        DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;
        DiffFiles.DataSourceChanged += DiffFiles_DataSourceChanged;
        DiffFiles.DoubleClick += DiffFiles_DoubleClick;
        // 
        // DiffText
        // 
        DiffText.Dock = DockStyle.Fill;
        DiffText.EnableAutomaticContinuousScroll = false;
        DiffText.Location = new Point(0, 0);
        DiffText.Margin = new Padding(0);
        DiffText.Name = "DiffText";
        DiffText.Size = new Size(543, 415);
        DiffText.TabIndex = 0;
        DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
        DiffText.PatchApplied += DiffText_PatchApplied;
        // 
        // BlameControl
        // 
        BlameControl.Dock = DockStyle.Fill;
        BlameControl.Location = new Point(0, 0);
        BlameControl.Margin = new Padding(0);
        BlameControl.Name = "BlameControl";
        BlameControl.Size = new Size(543, 415);
        BlameControl.TabIndex = 1;
        // 
        // RevisionDiffControl
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(DiffSplitContainer);
        Margin = new Padding(0);
        Name = "RevisionDiffControl";
        Size = new Size(850, 415);
        DiffSplitContainer.Panel1.ResumeLayout(false);
        DiffSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)DiffSplitContainer).EndInit();
        DiffSplitContainer.ResumeLayout(false);
        LeftSplitContainer.Panel1.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)LeftSplitContainer).EndInit();
        LeftSplitContainer.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion
    private SplitContainer DiffSplitContainer;
    public SplitContainer LeftSplitContainer;
    private FileStatusList DiffFiles;
    private Editor.FileViewer DiffText;
    private Blame.BlameControl BlameControl;
}
