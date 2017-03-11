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
            System.Windows.Forms.Panel panel1;
            System.Windows.Forms.Panel panel2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGitIgnore));
            this.AddDefault = new System.Windows.Forms.Button();
            this.AddPattern = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.lnkGitIgnoreGenerate = new System.Windows.Forms.LinkLabel();
            this.lnkGitIgnorePatterns = new System.Windows.Forms.LinkLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._NO_TRANSLATE_GitIgnoreEdit = new GitUI.Editor.FileViewer();
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
            panel1.Controls.Add(this.AddDefault);
            panel1.Controls.Add(this.AddPattern);
            panel1.Controls.Add(this.Save);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 530);
            panel1.Margin = new System.Windows.Forms.Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(289, 119);
            panel1.TabIndex = 5;
            // 
            // AddDefault
            // 
            this.AddDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddDefault.Location = new System.Drawing.Point(85, 45);
            this.AddDefault.Margin = new System.Windows.Forms.Padding(4);
            this.AddDefault.Name = "AddDefault";
            this.AddDefault.Size = new System.Drawing.Size(200, 31);
            this.AddDefault.TabIndex = 2;
            this.AddDefault.Text = "Add default ignores";
            this.AddDefault.UseVisualStyleBackColor = true;
            this.AddDefault.Click += new System.EventHandler(this.AddDefaultClick);
            // 
            // AddPattern
            // 
            this.AddPattern.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddPattern.Location = new System.Drawing.Point(85, 6);
            this.AddPattern.Margin = new System.Windows.Forms.Padding(4);
            this.AddPattern.Name = "AddPattern";
            this.AddPattern.Size = new System.Drawing.Size(200, 31);
            this.AddPattern.TabIndex = 3;
            this.AddPattern.Text = "Add pattern";
            this.AddPattern.UseVisualStyleBackColor = true;
            this.AddPattern.Click += new System.EventHandler(this.AddPattern_Click);
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Location = new System.Drawing.Point(85, 83);
            this.Save.Margin = new System.Windows.Forms.Padding(4);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(200, 31);
            this.Save.TabIndex = 1;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // panel2
            // 
            panel2.Controls.Add(this.lnkGitIgnoreGenerate);
            panel2.Controls.Add(this.lnkGitIgnorePatterns);
            panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel2.Location = new System.Drawing.Point(0, 474);
            panel2.Margin = new System.Windows.Forms.Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(289, 56);
            panel2.TabIndex = 7;
            // 
            // lnkGitIgnoreGenerate
            // 
            this.lnkGitIgnoreGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkGitIgnoreGenerate.AutoSize = true;
            this.lnkGitIgnoreGenerate.Location = new System.Drawing.Point(0, 28);
            this.lnkGitIgnoreGenerate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkGitIgnoreGenerate.Name = "lnkGitIgnoreGenerate";
            this.lnkGitIgnoreGenerate.Size = new System.Drawing.Size(181, 17);
            this.lnkGitIgnoreGenerate.TabIndex = 7;
            this.lnkGitIgnoreGenerate.TabStop = true;
            this.lnkGitIgnoreGenerate.Text = "Generate a custom gitignore";
            this.lnkGitIgnoreGenerate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitIgnoreGenerate_LinkClicked);
            // 
            // lnkGitIgnorePatterns
            // 
            this.lnkGitIgnorePatterns.AutoSize = true;
            this.lnkGitIgnorePatterns.Dock = System.Windows.Forms.DockStyle.Left;
            this.lnkGitIgnorePatterns.Location = new System.Drawing.Point(0, 0);
            this.lnkGitIgnorePatterns.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkGitIgnorePatterns.Name = "lnkGitIgnorePatterns";
            this.lnkGitIgnorePatterns.Size = new System.Drawing.Size(150, 17);
            this.lnkGitIgnorePatterns.TabIndex = 6;
            this.lnkGitIgnorePatterns.TabStop = true;
            this.lnkGitIgnorePatterns.Text = "More gitignore patterns";
            this.lnkGitIgnorePatterns.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitIgnorePatterns_LinkClicked);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_GitIgnoreEdit);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(panel2);
            this.splitContainer1.Panel2.Controls.Add(panel1);
            this.splitContainer1.Size = new System.Drawing.Size(792, 649);
            this.splitContainer1.SplitterDistance = 498;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_GitIgnoreEdit
            // 
            this._NO_TRANSLATE_GitIgnoreEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_GitIgnoreEdit.IsReadOnly = false;
            this._NO_TRANSLATE_GitIgnoreEdit.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_GitIgnoreEdit.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this._NO_TRANSLATE_GitIgnoreEdit.Name = "_NO_TRANSLATE_GitIgnoreEdit";
            this._NO_TRANSLATE_GitIgnoreEdit.Size = new System.Drawing.Size(498, 649);
            this._NO_TRANSLATE_GitIgnoreEdit.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Multiline = true;
            this.label1.Name = "label1";
            this.label1.ReadOnly = true;
            this.label1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.label1.Size = new System.Drawing.Size(289, 474);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.WordWrap = false;
            // 
            // FormGitIgnore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(792, 649);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormGitIgnore";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit .gitignore";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGitIgnoreFormClosing);
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
        private GitUI.Editor.FileViewer _NO_TRANSLATE_GitIgnoreEdit;
        private System.Windows.Forms.TextBox label1;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button AddDefault;
        private System.Windows.Forms.Button AddPattern;
        private System.Windows.Forms.LinkLabel lnkGitIgnorePatterns;
        private System.Windows.Forms.LinkLabel lnkGitIgnoreGenerate;
    }
}