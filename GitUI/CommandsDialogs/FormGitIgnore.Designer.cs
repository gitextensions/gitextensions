namespace GitUI.CommandsDialogs
{
    partial class FormGitIgnore
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
            System.Windows.Forms.FlowLayoutPanel panel1;
            System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGitIgnore));
            this.lnkGitIgnoreGenerate = new System.Windows.Forms.LinkLabel();
            this.lnkGitIgnorePatterns = new System.Windows.Forms.LinkLabel();
            this.AddDefault = new System.Windows.Forms.Button();
            this.AddPattern = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._NO_TRANSLATE_GitIgnoreEdit = new GitUI.Editor.FileViewer();
            this.label1 = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            panel1 = new System.Windows.Forms.FlowLayoutPanel();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            panel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(this.lnkGitIgnoreGenerate);
            panel1.Controls.Add(this.lnkGitIgnorePatterns);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            panel1.Location = new System.Drawing.Point(0, 383);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(0, 0, 8, 4);
            panel1.Size = new System.Drawing.Size(270, 38);
            panel1.TabIndex = 5;
            // 
            // lnkGitIgnoreGenerate
            // 
            this.lnkGitIgnoreGenerate.AutoSize = true;
            this.lnkGitIgnoreGenerate.Dock = System.Windows.Forms.DockStyle.Right;
            this.lnkGitIgnoreGenerate.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.lnkGitIgnoreGenerate.Location = new System.Drawing.Point(79, 2);
            this.lnkGitIgnoreGenerate.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lnkGitIgnoreGenerate.Name = "lnkGitIgnoreGenerate";
            this.lnkGitIgnoreGenerate.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lnkGitIgnoreGenerate.Size = new System.Drawing.Size(180, 13);
            this.lnkGitIgnoreGenerate.TabIndex = 7;
            this.lnkGitIgnoreGenerate.TabStop = true;
            this.lnkGitIgnoreGenerate.Text = "Generate a custom ignore file for git";
            this.lnkGitIgnoreGenerate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitIgnoreGenerate_LinkClicked);
            // 
            // lnkGitIgnorePatterns
            // 
            this.lnkGitIgnorePatterns.AutoSize = true;
            this.lnkGitIgnorePatterns.Dock = System.Windows.Forms.DockStyle.Right;
            this.lnkGitIgnorePatterns.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.lnkGitIgnorePatterns.Location = new System.Drawing.Point(135, 19);
            this.lnkGitIgnorePatterns.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lnkGitIgnorePatterns.Name = "lnkGitIgnorePatterns";
            this.lnkGitIgnorePatterns.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lnkGitIgnorePatterns.Size = new System.Drawing.Size(124, 13);
            this.lnkGitIgnorePatterns.TabIndex = 6;
            this.lnkGitIgnorePatterns.TabStop = true;
            this.lnkGitIgnorePatterns.Text = "Example ignore patterns";
            this.lnkGitIgnorePatterns.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitIgnorePatterns_LinkClicked);
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Controls.Add(this.AddDefault);
            flowLayoutPanel2.Controls.Add(this.AddPattern);
            flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowLayoutPanel2.Location = new System.Drawing.Point(0, 388);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(352, 33);
            flowLayoutPanel2.TabIndex = 6;
            flowLayoutPanel2.WrapContents = false;
            // 
            // AddDefault
            // 
            this.AddDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddDefault.AutoSize = true;
            this.AddDefault.Location = new System.Drawing.Point(3, 3);
            this.AddDefault.Name = "AddDefault";
            this.AddDefault.Size = new System.Drawing.Size(160, 27);
            this.AddDefault.TabIndex = 2;
            this.AddDefault.Text = "Add default ignores";
            this.AddDefault.UseVisualStyleBackColor = true;
            this.AddDefault.Click += new System.EventHandler(this.AddDefaultClick);
            // 
            // AddPattern
            // 
            this.AddPattern.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddPattern.AutoSize = true;
            this.AddPattern.Location = new System.Drawing.Point(169, 3);
            this.AddPattern.Name = "AddPattern";
            this.AddPattern.Size = new System.Drawing.Size(160, 27);
            this.AddPattern.TabIndex = 3;
            this.AddPattern.Text = "Add pattern";
            this.AddPattern.UseVisualStyleBackColor = true;
            this.AddPattern.Click += new System.EventHandler(this.AddPattern_Click);
            // 
            // Save
            // 
            this.Save.AutoSize = true;
            this.Save.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Save.Image = global::GitUI.Properties.Images.Save;
            this.Save.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Save.Location = new System.Drawing.Point(463, 3);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(160, 27);
            this.Save.TabIndex = 1;
            this.Save.Text = "Save";
            this.Save.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(4, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_GitIgnoreEdit);
            this.splitContainer1.Panel1.Controls.Add(flowLayoutPanel2);
            this.splitContainer1.Panel1MinSize = 250;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(panel1);
            this.splitContainer1.Panel2MinSize = 250;
            this.splitContainer1.Size = new System.Drawing.Size(626, 421);
            this.splitContainer1.SplitterDistance = 352;
            this.splitContainer1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_GitIgnoreEdit
            // 
            this._NO_TRANSLATE_GitIgnoreEdit.AutoScroll = true;
            this._NO_TRANSLATE_GitIgnoreEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._NO_TRANSLATE_GitIgnoreEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_GitIgnoreEdit.IsReadOnly = false;
            this._NO_TRANSLATE_GitIgnoreEdit.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_GitIgnoreEdit.Margin = new System.Windows.Forms.Padding(0, 0, 3, 2);
            this._NO_TRANSLATE_GitIgnoreEdit.Name = "_NO_TRANSLATE_GitIgnoreEdit";
            this._NO_TRANSLATE_GitIgnoreEdit.Size = new System.Drawing.Size(352, 388);
            this._NO_TRANSLATE_GitIgnoreEdit.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label1.Multiline = true;
            this.label1.Name = "label1";
            this.label1.ReadOnly = true;
            this.label1.Size = new System.Drawing.Size(270, 383);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.Save);
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 425);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(626, 33);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(382, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 27);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormGitIgnore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(634, 462);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(650, 498);
            this.Name = "FormGitIgnore";
            this.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit .gitignore";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGitIgnoreFormClosing);
            this.Load += new System.EventHandler(this.FormGitIgnoreLoad);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private GitUI.Editor.FileViewer _NO_TRANSLATE_GitIgnoreEdit;
        private System.Windows.Forms.TextBox label1;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button AddDefault;
        private System.Windows.Forms.Button AddPattern;
        private System.Windows.Forms.LinkLabel lnkGitIgnorePatterns;
        private System.Windows.Forms.LinkLabel lnkGitIgnoreGenerate;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
    }
}