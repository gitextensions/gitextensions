using System.Windows.Forms;
using GitCommands.Git;

namespace GitUI.CommandsDialogs;

partial class FormFormatPatch
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        gitRevisionBindingSource = new BindingSource(components);
        gitItemBindingSource = new BindingSource(components);
        tableLayoutPanelSaveTo = new TableLayoutPanel();
        lblPatches = new Label();
        OutputPath = new TextBox();
        Browse = new Button();
        tableLayoutPanelForm = new TableLayoutPanel();
        FormatPatch = new Button();
        flowLayoutPanelBranch = new FlowLayoutPanel();
        SelectedBranch = new Label();
        CurrentBranch = new Label();
        RevisionGrid = new GitUI.RevisionGridControl();
        ((System.ComponentModel.ISupportInitialize)(gitRevisionBindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(gitItemBindingSource)).BeginInit();
        tableLayoutPanelSaveTo.SuspendLayout();
        tableLayoutPanelForm.SuspendLayout();
        flowLayoutPanelBranch.SuspendLayout();
        SuspendLayout();
        // 
        // gitRevisionBindingSource
        // 
        gitRevisionBindingSource.DataSource = typeof(GitUIPluginInterfaces.GitRevision);
        // 
        // gitItemBindingSource
        // 
        gitItemBindingSource.DataSource = typeof(GitItem);
        // 
        // tableLayoutPanelSaveTo
        // 
        tableLayoutPanelForm.SetColumnSpan(tableLayoutPanelSaveTo, 2);
        tableLayoutPanelSaveTo.ColumnCount = 3;
        tableLayoutPanelSaveTo.ColumnStyles.Add(new ColumnStyle());
        tableLayoutPanelSaveTo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelSaveTo.ColumnStyles.Add(new ColumnStyle());
        tableLayoutPanelSaveTo.Controls.Add(lblPatches, 0, 0);
        tableLayoutPanelSaveTo.Controls.Add(OutputPath, 1, 0);
        tableLayoutPanelSaveTo.Controls.Add(Browse, 2, 0);
        tableLayoutPanelSaveTo.Dock = DockStyle.Fill;
        tableLayoutPanelSaveTo.Location = new Point(0, 0);
        tableLayoutPanelSaveTo.Margin = new Padding(4, 4, 4, 4);
        tableLayoutPanelSaveTo.Name = "tableLayoutPanelSaveTo";
        tableLayoutPanelSaveTo.RowCount = 1;
        tableLayoutPanelSaveTo.RowStyles.Add(new RowStyle());
        tableLayoutPanelSaveTo.Size = new Size(1030, 35);
        // 
        // 
        // lblPatches
        // 
        lblPatches.AutoSize = true;
        lblPatches.Location = new Point(4, 154);
        lblPatches.Margin = new Padding(4, 0, 4, 0);
        lblPatches.Name = "lblPatches";
        lblPatches.Padding = new Padding(0, 6, 0, 0);
        lblPatches.Size = new Size(30, 29);
        lblPatches.Text = "Patch:";
        // OutputPath
        // 
        OutputPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        OutputPath.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        OutputPath.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
        OutputPath.Location = new Point(233, 4);
        OutputPath.Margin = new Padding(4, 4, 4, 4);
        OutputPath.Name = "OutputPath";
        OutputPath.Size = new Size(660, 30);
        // 
        // Browse
        // 
        Browse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        Browse.Location = new Point(901, 4);
        Browse.Margin = new Padding(4, 4, 4, 4);
        Browse.Name = "Browse";
        Browse.Size = new Size(80, 31);
        Browse.Text = "Browse";
        Browse.UseVisualStyleBackColor = true;
        Browse.Click += Browse_Click;
        // 
        // RevisionGrid
        // 
        tableLayoutPanelForm.SetColumnSpan(RevisionGrid, 2);
        RevisionGrid.Dock = DockStyle.Fill;
        RevisionGrid.Location = new Point(4, 4);
        RevisionGrid.Margin = new Padding(4, 4, 4, 4);
        RevisionGrid.Name = "RevisionGrid";
        RevisionGrid.Size = new Size(1022, 382);
        // 
        // flowLayoutPanelBranch
        // 
        flowLayoutPanelBranch.Controls.Add(SelectedBranch);
        flowLayoutPanelBranch.Controls.Add(CurrentBranch);
        flowLayoutPanelBranch.Dock = DockStyle.Fill;
        flowLayoutPanelBranch.Location = new Point(4, 4);
        flowLayoutPanelBranch.Margin = new Padding(4, 4, 4, 4);
        flowLayoutPanelBranch.Name = "flowLayoutPanelBranch";
        flowLayoutPanelBranch.Size = new Size(831, 37);
        // 
        // SelectedBranch
        // 
        SelectedBranch.AutoSize = true;
        SelectedBranch.Location = new Point(4, 0);
        SelectedBranch.Margin = new Padding(4, 0, 4, 0);
        SelectedBranch.Name = "SelectedBranch";
        SelectedBranch.Size = new Size(63, 23);
        SelectedBranch.Text = "Branch";
        // 
        // CurrentBranch
        // 
        CurrentBranch.AutoSize = true;
        CurrentBranch.Location = new Point(75, 0);
        CurrentBranch.Margin = new Padding(4, 0, 4, 0);
        CurrentBranch.Name = "CurrentBranch";
        CurrentBranch.Size = new Size(0, 23);
        // 
        // FormatPatch
        // 
        FormatPatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        FormatPatch.Location = new Point(843, 10);
        FormatPatch.Margin = new Padding(4, 4, 4, 4);
        FormatPatch.Name = "FormatPatch";
        FormatPatch.Size = new Size(175, 31);
        FormatPatch.Text = "Create patch(es)";
        FormatPatch.UseVisualStyleBackColor = true;
        FormatPatch.Click += FormatPatch_Click;
        // 
        // tableLayoutPanelForm
        // 
        tableLayoutPanelForm.ColumnCount = 2;
        tableLayoutPanelForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelForm.ColumnStyles.Add(new ColumnStyle());
        tableLayoutPanelForm.Controls.Add(tableLayoutPanelSaveTo, 0, 0);
        tableLayoutPanelForm.Controls.Add(RevisionGrid, 0, 1);
        tableLayoutPanelForm.Controls.Add(flowLayoutPanelBranch, 0, 2);
        tableLayoutPanelForm.Controls.Add(FormatPatch, 1, 2);
        tableLayoutPanelForm.Dock = DockStyle.Fill;
        tableLayoutPanelForm.Location = new Point(4, 394);
        tableLayoutPanelForm.Margin = new Padding(4, 4, 4, 4);
        tableLayoutPanelForm.Name = "tableLayoutPanelForm";
        tableLayoutPanelForm.RowCount = 1;
        tableLayoutPanelForm.RowStyles.Add(new RowStyle());
        tableLayoutPanelForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanelForm.RowStyles.Add(new RowStyle());
        tableLayoutPanelForm.Size = new Size(1022, 45);
        // 
        // FormFormatPatch
        // 
        AutoScaleDimensions = new SizeF(120F, 120F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1030, 665);
        Controls.Add(tableLayoutPanelForm);
        Margin = new Padding(4, 4, 4, 4);
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(558, 395);
        Name = "FormFormatPatch";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Format patch";
        Load += FormFormatPath_Load;
        ((System.ComponentModel.ISupportInitialize)(gitRevisionBindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(gitItemBindingSource)).EndInit();

        tableLayoutPanelSaveTo.ResumeLayout(false);
        tableLayoutPanelSaveTo.PerformLayout();
        tableLayoutPanelForm.ResumeLayout(false);
        flowLayoutPanelBranch.ResumeLayout(false);
        flowLayoutPanelBranch.PerformLayout();
        ResumeLayout(false);

    }

    #endregion

    private BindingSource gitItemBindingSource;
    private BindingSource gitRevisionBindingSource;
    private Label CurrentBranch;
    private Label SelectedBranch;
    private Button Browse;
    private TextBox OutputPath;
    private Button FormatPatch;
    private RevisionGridControl RevisionGrid;
    private Label lblPatches;
    private TableLayoutPanel tableLayoutPanelSaveTo;
    private TableLayoutPanel tableLayoutPanelForm;
    private FlowLayoutPanel flowLayoutPanelBranch;
}
