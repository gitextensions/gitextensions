namespace Gerrit
{
    partial class FormGitReview
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
            System.Windows.Forms.Panel panel1;
            System.Windows.Forms.Panel panel2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGitReview));
            this.Save = new System.Windows.Forms.Button();
            this.lnkGitReviewHelp = new System.Windows.Forms.LinkLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._NO_TRANSLATE_GitReviewEdit = new GitUI.Editor.FileViewer();
            this.label1 = new System.Windows.Forms.TextBox();
            panel1 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(this.Save);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 480);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(230, 39);
            panel1.TabIndex = 5;
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Location = new System.Drawing.Point(67, 11);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(160, 25);
            this.Save.TabIndex = 1;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // panel2
            // 
            panel2.Controls.Add(this.lnkGitReviewHelp);
            panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel2.Location = new System.Drawing.Point(0, 458);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(230, 22);
            panel2.TabIndex = 7;
            // 
            // lnkGitReviewHelp
            // 
            this.lnkGitReviewHelp.AutoSize = true;
            this.lnkGitReviewHelp.Dock = System.Windows.Forms.DockStyle.Right;
            this.lnkGitReviewHelp.Location = new System.Drawing.Point(82, 0);
            this.lnkGitReviewHelp.Name = "lnkGitReviewHelp";
            this.lnkGitReviewHelp.Size = new System.Drawing.Size(148, 15);
            this.lnkGitReviewHelp.TabIndex = 6;
            this.lnkGitReviewHelp.TabStop = true;
            this.lnkGitReviewHelp.Text = "GitHub page for git-review";
            this.lnkGitReviewHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitReviewPatterns_LinkClicked);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_GitReviewEdit);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(panel2);
            this.splitContainer1.Panel2.Controls.Add(panel1);
            this.splitContainer1.Size = new System.Drawing.Size(634, 519);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_GitReviewEdit
            // 
            this._NO_TRANSLATE_GitReviewEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_GitReviewEdit.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this._NO_TRANSLATE_GitReviewEdit.IsReadOnly = false;
            this._NO_TRANSLATE_GitReviewEdit.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_GitReviewEdit.Name = "_NO_TRANSLATE_GitReviewEdit";
            this._NO_TRANSLATE_GitReviewEdit.Size = new System.Drawing.Size(400, 519);
            this._NO_TRANSLATE_GitReviewEdit.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Multiline = true;
            this.label1.Name = "label1";
            this.label1.ReadOnly = true;
            this.label1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.label1.Size = new System.Drawing.Size(230, 458);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.WordWrap = false;
            // 
            // FormGitReview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 519);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormGitReview";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit .gitreview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGitReviewFormClosing);
            this.Load += new System.EventHandler(this.FormGitIgnoreLoad);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private GitUI.Editor.FileViewer _NO_TRANSLATE_GitReviewEdit;
        private System.Windows.Forms.TextBox label1;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.LinkLabel lnkGitReviewHelp;

    }
}