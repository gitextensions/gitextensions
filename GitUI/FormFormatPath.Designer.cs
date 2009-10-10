namespace GitUI
{
    partial class FormFormatPath
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFormatPath));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.MailBody = new System.Windows.Forms.RichTextBox();
            this.MailSubject = new System.Windows.Forms.TextBox();
            this.MailAddress = new System.Windows.Forms.ComboBox();
            this.SendToMail = new System.Windows.Forms.RadioButton();
            this.SaveToDir = new System.Windows.Forms.RadioButton();
            this.Browse = new System.Windows.Forms.Button();
            this.OutputPath = new System.Windows.Forms.TextBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.RevisionGrid = new GitUI.RevisionGrid();
            this.FormatPatch = new System.Windows.Forms.Button();
            this.SelectedBranch = new System.Windows.Forms.Label();
            this.CurrentBranch = new System.Windows.Forms.Label();
            this.gitRevisionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gitItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).BeginInit();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.MailBody);
            this.splitContainer1.Panel1.Controls.Add(this.MailSubject);
            this.splitContainer1.Panel1.Controls.Add(this.MailAddress);
            this.splitContainer1.Panel1.Controls.Add(this.SendToMail);
            this.splitContainer1.Panel1.Controls.Add(this.SaveToDir);
            this.splitContainer1.Panel1.Controls.Add(this.Browse);
            this.splitContainer1.Panel1.Controls.Add(this.OutputPath);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(750, 454);
            this.splitContainer1.SplitterDistance = 152;
            this.splitContainer1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Body";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Subject";
            // 
            // MailBody
            // 
            this.MailBody.Location = new System.Drawing.Point(155, 93);
            this.MailBody.Name = "MailBody";
            this.MailBody.Size = new System.Drawing.Size(502, 56);
            this.MailBody.TabIndex = 13;
            this.MailBody.Text = "";
            // 
            // MailSubject
            // 
            this.MailSubject.Location = new System.Drawing.Point(155, 66);
            this.MailSubject.Name = "MailSubject";
            this.MailSubject.Size = new System.Drawing.Size(502, 20);
            this.MailSubject.TabIndex = 12;
            // 
            // MailAddress
            // 
            this.MailAddress.FormattingEnabled = true;
            this.MailAddress.Location = new System.Drawing.Point(155, 38);
            this.MailAddress.Name = "MailAddress";
            this.MailAddress.Size = new System.Drawing.Size(502, 21);
            this.MailAddress.TabIndex = 11;
            // 
            // SendToMail
            // 
            this.SendToMail.AutoSize = true;
            this.SendToMail.Location = new System.Drawing.Point(12, 40);
            this.SendToMail.Name = "SendToMail";
            this.SendToMail.Size = new System.Drawing.Size(97, 17);
            this.SendToMail.TabIndex = 10;
            this.SendToMail.Text = "Mail patches to";
            this.SendToMail.UseVisualStyleBackColor = true;
            // 
            // SaveToDir
            // 
            this.SaveToDir.AutoSize = true;
            this.SaveToDir.Checked = true;
            this.SaveToDir.Location = new System.Drawing.Point(12, 14);
            this.SaveToDir.Name = "SaveToDir";
            this.SaveToDir.Size = new System.Drawing.Size(145, 17);
            this.SaveToDir.TabIndex = 9;
            this.SaveToDir.TabStop = true;
            this.SaveToDir.Text = "Save patches in directory";
            this.SaveToDir.UseVisualStyleBackColor = true;
            this.SaveToDir.CheckedChanged += new System.EventHandler(this.SaveToDir_CheckedChanged);
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(663, 8);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 8;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // OutputPath
            // 
            this.OutputPath.Location = new System.Drawing.Point(155, 11);
            this.OutputPath.Name = "OutputPath";
            this.OutputPath.Size = new System.Drawing.Size(502, 20);
            this.OutputPath.TabIndex = 7;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.RevisionGrid);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.FormatPatch);
            this.splitContainer2.Panel2.Controls.Add(this.SelectedBranch);
            this.splitContainer2.Panel2.Controls.Add(this.CurrentBranch);
            this.splitContainer2.Size = new System.Drawing.Size(750, 298);
            this.splitContainer2.SplitterDistance = 260;
            this.splitContainer2.TabIndex = 0;
            // 
            // RevisionGrid
            // 
            this.RevisionGrid.currentCheckout = null;
            this.RevisionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RevisionGrid.Filter = "";
            this.RevisionGrid.HeadFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.RevisionGrid.LastRow = 0;
            this.RevisionGrid.Location = new System.Drawing.Point(0, 0);
            this.RevisionGrid.Name = "RevisionGrid";
            this.RevisionGrid.NormalFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RevisionGrid.Size = new System.Drawing.Size(750, 260);
            this.RevisionGrid.TabIndex = 0;
            // 
            // FormatPatch
            // 
            this.FormatPatch.Location = new System.Drawing.Point(639, 4);
            this.FormatPatch.Name = "FormatPatch";
            this.FormatPatch.Size = new System.Drawing.Size(108, 23);
            this.FormatPatch.TabIndex = 0;
            this.FormatPatch.Text = "Create patch(es)";
            this.FormatPatch.UseVisualStyleBackColor = true;
            this.FormatPatch.Click += new System.EventHandler(this.FormatPatch_Click);
            // 
            // SelectedBranch
            // 
            this.SelectedBranch.AutoSize = true;
            this.SelectedBranch.Location = new System.Drawing.Point(12, 12);
            this.SelectedBranch.Name = "SelectedBranch";
            this.SelectedBranch.Size = new System.Drawing.Size(41, 13);
            this.SelectedBranch.TabIndex = 4;
            this.SelectedBranch.Text = "Branch";
            // 
            // CurrentBranch
            // 
            this.CurrentBranch.AutoSize = true;
            this.CurrentBranch.Location = new System.Drawing.Point(60, 13);
            this.CurrentBranch.Name = "CurrentBranch";
            this.CurrentBranch.Size = new System.Drawing.Size(0, 13);
            this.CurrentBranch.TabIndex = 5;
            // 
            // gitRevisionBindingSource
            // 
            this.gitRevisionBindingSource.DataSource = typeof(GitCommands.GitRevision);
            // 
            // gitItemBindingSource
            // 
            this.gitItemBindingSource.DataSource = typeof(GitCommands.GitItem);
            // 
            // FormFormatPath
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 454);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFormatPath";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Format patch";
            this.Load += new System.EventHandler(this.FormFormatPath_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gitRevisionBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gitItemBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
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
    }
}