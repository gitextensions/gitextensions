using System.Windows.Forms;
namespace GitUI
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
            this.components = new System.ComponentModel.Container();
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.SaveToDir = new System.Windows.Forms.RadioButton();
            this.MailBody = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.MailSubject = new System.Windows.Forms.TextBox();
            this.OutputPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Browse = new System.Windows.Forms.Button();
            this.SendToMail = new System.Windows.Forms.RadioButton();
            this.MailAddress = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.FormatPatch = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.SelectedBranch = new System.Windows.Forms.Label();
            this.CurrentBranch = new System.Windows.Forms.Label();
            this.RevisionGrid = new GitUI.RevisionGrid();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
#endif
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gitRevisionBindingSource
            // 
            this.gitRevisionBindingSource.DataSource = typeof(GitCommands.GitRevision);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
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
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel1MinSize = 120;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer1.Panel2MinSize = 100;
            this.splitContainer1.Size = new System.Drawing.Size(824, 532);
            this.splitContainer1.SplitterDistance = 165;
            this.splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.SaveToDir, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.MailBody, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.MailSubject, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.OutputPath, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Browse, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.SendToMail, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.MailAddress, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(824, 165);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // SaveToDir
            // 
            this.SaveToDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveToDir.AutoSize = true;
            this.SaveToDir.Checked = true;
            this.SaveToDir.Location = new System.Drawing.Point(3, 3);
            this.SaveToDir.Name = "SaveToDir";
            this.SaveToDir.Size = new System.Drawing.Size(156, 25);
            this.SaveToDir.TabIndex = 9;
            this.SaveToDir.TabStop = true;
            this.SaveToDir.Text = "Save patches in directory";
            this.SaveToDir.UseVisualStyleBackColor = true;
            this.SaveToDir.CheckedChanged += new System.EventHandler(this.SaveToDir_CheckedChanged);
            // 
            // MailBody
            // 
            this.MailBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MailBody.Location = new System.Drawing.Point(165, 92);
            this.MailBody.Name = "MailBody";
            this.MailBody.Size = new System.Drawing.Size(550, 70);
            this.MailBody.TabIndex = 13;
            this.MailBody.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 89);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(15, 5, 0, 0);
            this.label2.Size = new System.Drawing.Size(49, 20);
            this.label2.TabIndex = 15;
            this.label2.Text = "Body";
            // 
            // MailSubject
            // 
            this.MailSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MailSubject.Location = new System.Drawing.Point(165, 63);
            this.MailSubject.Name = "MailSubject";
            this.MailSubject.Size = new System.Drawing.Size(550, 23);
            this.MailSubject.TabIndex = 12;
            // 
            // OutputPath
            // 
            this.OutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.OutputPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.OutputPath.Location = new System.Drawing.Point(165, 3);
            this.OutputPath.Name = "OutputPath";
            this.OutputPath.Size = new System.Drawing.Size(550, 23);
            this.OutputPath.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 60);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(61, 29);
            this.label1.TabIndex = 14;
            this.label1.Text = "Subject";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Browse
            // 
            this.Browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Browse.Location = new System.Drawing.Point(721, 3);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(100, 25);
            this.Browse.TabIndex = 8;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // SendToMail
            // 
            this.SendToMail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.SendToMail.AutoSize = true;
            this.SendToMail.Location = new System.Drawing.Point(3, 34);
            this.SendToMail.Name = "SendToMail";
            this.SendToMail.Size = new System.Drawing.Size(106, 23);
            this.SendToMail.TabIndex = 10;
            this.SendToMail.Text = "Mail patches to";
            this.SendToMail.UseVisualStyleBackColor = true;
            // 
            // MailAddress
            // 
            this.MailAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MailAddress.FormattingEnabled = true;
            this.MailAddress.Location = new System.Drawing.Point(165, 34);
            this.MailAddress.Name = "MailAddress";
            this.MailAddress.Size = new System.Drawing.Size(550, 23);
            this.MailAddress.TabIndex = 11;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.RevisionGrid, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(824, 363);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.FormatPatch, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 324);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(818, 36);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // FormatPatch
            // 
            this.FormatPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FormatPatch.Location = new System.Drawing.Point(675, 8);
            this.FormatPatch.Name = "FormatPatch";
            this.FormatPatch.Size = new System.Drawing.Size(140, 25);
            this.FormatPatch.TabIndex = 0;
            this.FormatPatch.Text = "Create patch(es)";
            this.FormatPatch.UseVisualStyleBackColor = true;
            this.FormatPatch.Click += new System.EventHandler(this.FormatPatch_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.SelectedBranch);
            this.flowLayoutPanel1.Controls.Add(this.CurrentBranch);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(666, 30);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // SelectedBranch
            // 
            this.SelectedBranch.AutoSize = true;
            this.SelectedBranch.Location = new System.Drawing.Point(3, 0);
            this.SelectedBranch.Name = "SelectedBranch";
            this.SelectedBranch.Size = new System.Drawing.Size(44, 15);
            this.SelectedBranch.TabIndex = 4;
            this.SelectedBranch.Text = "Branch";
            // 
            // CurrentBranch
            // 
            this.CurrentBranch.AutoSize = true;
            this.CurrentBranch.Location = new System.Drawing.Point(53, 0);
            this.CurrentBranch.Name = "CurrentBranch";
            this.CurrentBranch.Size = new System.Drawing.Size(0, 15);
            this.CurrentBranch.TabIndex = 5;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Location = new System.Drawing.Point(3, 3);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.Size = new System.Drawing.Size(818, 315);
            this.RevisionGrid.TabIndex = 0;
            // 
            // FormFormatPatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(824, 532);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 325);
            this.Name = "FormFormatPatch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Format patch";
            this.Load += new System.EventHandler(this.FormFormatPath_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
#if Mono212Released //waiting for mono 2.12
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
#endif
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.BindingSource gitItemBindingSource;
        private System.Windows.Forms.BindingSource gitRevisionBindingSource;
        private System.Windows.Forms.Label CurrentBranch;
        private System.Windows.Forms.Label SelectedBranch;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.TextBox OutputPath;
        private System.Windows.Forms.Button FormatPatch;
        private RevisionGrid RevisionGrid;
        private System.Windows.Forms.ComboBox MailAddress;
        private System.Windows.Forms.RadioButton SendToMail;
        private System.Windows.Forms.RadioButton SaveToDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox MailBody;
        private System.Windows.Forms.TextBox MailSubject;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}