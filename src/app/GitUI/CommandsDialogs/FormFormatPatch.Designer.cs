using System.Windows.Forms;
using GitCommands.Git;

namespace GitUI.CommandsDialogs
{
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
            splitContainer1 = new SplitContainer();
            tableLayoutPanel1 = new TableLayoutPanel();
            SaveToDir = new RadioButton();
            MailBody = new RichTextBox();
            label2 = new Label();
            MailSubject = new TextBox();
            OutputPath = new TextBox();
            label1 = new Label();
            Browse = new Button();
            MailTo = new ComboBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            FormatPatch = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            SelectedBranch = new Label();
            CurrentBranch = new Label();
            RevisionGrid = new GitUI.RevisionGridControl();
            MailFrom = new TextBox();
            radioButton1 = new RadioButton();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)(gitRevisionBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(gitItemBindingSource)).BeginInit();

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();

            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
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
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 4, 4, 4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            splitContainer1.Panel1MinSize = 120;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tableLayoutPanel2);
            splitContainer1.Panel2MinSize = 100;
            splitContainer1.Size = new Size(1030, 665);
            splitContainer1.SplitterDistance = 217;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(SaveToDir, 0, 0);
            tableLayoutPanel1.Controls.Add(MailBody, 1, 4);
            tableLayoutPanel1.Controls.Add(label2, 0, 4);
            tableLayoutPanel1.Controls.Add(MailSubject, 1, 3);
            tableLayoutPanel1.Controls.Add(OutputPath, 1, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 3);
            tableLayoutPanel1.Controls.Add(Browse, 2, 0);
            tableLayoutPanel1.Controls.Add(MailTo, 1, 2);
            tableLayoutPanel1.Controls.Add(MailFrom, 1, 1);
            tableLayoutPanel1.Controls.Add(radioButton1, 0, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 4, 4, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1030, 217);
            tableLayoutPanel1.TabIndex = 16;
            // 
            // SaveToDir
            // 
            SaveToDir.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            SaveToDir.AutoSize = true;
            SaveToDir.Checked = true;
            SaveToDir.Location = new Point(4, 4);
            SaveToDir.Margin = new Padding(4, 4, 4, 4);
            SaveToDir.Name = "SaveToDir";
            SaveToDir.Size = new Size(221, 31);
            SaveToDir.TabIndex = 9;
            SaveToDir.TabStop = true;
            SaveToDir.Text = "Save patches in directory";
            SaveToDir.UseVisualStyleBackColor = true;
            SaveToDir.CheckedChanged += SaveToDir_CheckedChanged;
            // 
            // MailBody
            // 
            MailBody.Dock = DockStyle.Fill;
            MailBody.Location = new Point(233, 158);
            MailBody.Margin = new Padding(4, 4, 4, 4);
            MailBody.Name = "MailBody";
            MailBody.Size = new Size(660, 55);
            MailBody.TabIndex = 13;
            MailBody.Text = "";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 154);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Padding = new Padding(19, 6, 0, 0);
            label2.Size = new Size(67, 29);
            label2.TabIndex = 15;
            label2.Text = "Body";
            // 
            // MailSubject
            // 
            MailSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MailSubject.Location = new Point(233, 120);
            MailSubject.Margin = new Padding(4, 4, 4, 4);
            MailSubject.Name = "MailSubject";
            MailSubject.Size = new Size(660, 30);
            MailSubject.TabIndex = 12;
            // 
            // OutputPath
            // 
            OutputPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OutputPath.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            OutputPath.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            OutputPath.Location = new Point(233, 4);
            OutputPath.Margin = new Padding(4, 4, 4, 4);
            OutputPath.Name = "OutputPath";
            OutputPath.Size = new Size(660, 30);
            OutputPath.TabIndex = 7;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(4, 116);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(19, 0, 0, 0);
            label1.Size = new Size(85, 38);
            label1.TabIndex = 14;
            label1.Text = "Subject";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Browse
            // 
            Browse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Browse.Location = new Point(901, 4);
            Browse.Margin = new Padding(4, 4, 4, 4);
            Browse.Name = "Browse";
            Browse.Size = new Size(125, 31);
            Browse.TabIndex = 8;
            Browse.Text = "Browse";
            Browse.UseVisualStyleBackColor = true;
            Browse.Click += Browse_Click;
            // 
            // MailTo
            // 
            MailTo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MailTo.FormattingEnabled = true;
            MailTo.Location = new Point(233, 81);
            MailTo.Margin = new Padding(4, 4, 4, 4);
            MailTo.Name = "MailTo";
            MailTo.Size = new Size(660, 31);
            MailTo.TabIndex = 11;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel2.Controls.Add(RevisionGrid, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(4, 4, 4, 4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(1030, 443);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.Controls.Add(FormatPatch, 1, 0);
            tableLayoutPanel3.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(4, 394);
            tableLayoutPanel3.Margin = new Padding(4, 4, 4, 4);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(1022, 45);
            tableLayoutPanel3.TabIndex = 2;
            // 
            // FormatPatch
            // 
            FormatPatch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            FormatPatch.Location = new Point(843, 10);
            FormatPatch.Margin = new Padding(4, 4, 4, 4);
            FormatPatch.Name = "FormatPatch";
            FormatPatch.Size = new Size(175, 31);
            FormatPatch.TabIndex = 0;
            FormatPatch.Text = "Create patch(es)";
            FormatPatch.UseVisualStyleBackColor = true;
            FormatPatch.Click += FormatPatch_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(SelectedBranch);
            flowLayoutPanel1.Controls.Add(CurrentBranch);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(4, 4);
            flowLayoutPanel1.Margin = new Padding(4, 4, 4, 4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(831, 37);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // SelectedBranch
            // 
            SelectedBranch.AutoSize = true;
            SelectedBranch.Location = new Point(4, 0);
            SelectedBranch.Margin = new Padding(4, 0, 4, 0);
            SelectedBranch.Name = "SelectedBranch";
            SelectedBranch.Size = new Size(63, 23);
            SelectedBranch.TabIndex = 4;
            SelectedBranch.Text = "Branch";
            // 
            // CurrentBranch
            // 
            CurrentBranch.AutoSize = true;
            CurrentBranch.Location = new Point(75, 0);
            CurrentBranch.Margin = new Padding(4, 0, 4, 0);
            CurrentBranch.Name = "CurrentBranch";
            CurrentBranch.Size = new Size(0, 23);
            CurrentBranch.TabIndex = 5;
            // 
            // RevisionGrid
            // 
            RevisionGrid.Dock = DockStyle.Fill;
            RevisionGrid.Location = new Point(4, 4);
            RevisionGrid.Margin = new Padding(4, 4, 4, 4);
            RevisionGrid.Name = "RevisionGrid";
            RevisionGrid.Size = new Size(1022, 382);
            RevisionGrid.TabIndex = 0;
            // 
            // MailFrom
            // 
            MailFrom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MailFrom.Location = new Point(233, 43);
            MailFrom.Margin = new Padding(4);
            MailFrom.Name = "MailFrom";
            MailFrom.Size = new Size(660, 30);
            MailFrom.TabIndex = 12;
            // 
            // radioButton1
            // 
            radioButton1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(4, 43);
            radioButton1.Margin = new Padding(4);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(168, 30);
            radioButton1.TabIndex = 10;
            radioButton1.Text = "Mail patches from";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(4, 77);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Padding = new Padding(19, 0, 0, 0);
            label3.Size = new Size(48, 39);
            label3.TabIndex = 14;
            label3.Text = "To";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FormFormatPatch
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1030, 665);
            Controls.Add(splitContainer1);
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
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();

            splitContainer1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private BindingSource gitItemBindingSource;
        private BindingSource gitRevisionBindingSource;
        private Label CurrentBranch;
        private Label SelectedBranch;
        private Button Browse;
        private TextBox OutputPath;
        private Button FormatPatch;
        private RevisionGridControl RevisionGrid;
        private ComboBox MailTo;
        private RadioButton SaveToDir;
        private Label label2;
        private Label label1;
        private RichTextBox MailBody;
        private TextBox MailSubject;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private FlowLayoutPanel flowLayoutPanel1;
        private TextBox MailFrom;
        private RadioButton radioButton1;
        private Label label3;
    }
}