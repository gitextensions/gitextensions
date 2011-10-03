﻿namespace GitUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGitIgnore));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._NO_TRANSLATE_GitIgnoreEdit = new GitUI.Editor.FileViewer();
            this.AddPattern = new System.Windows.Forms.Button();
            this.AddDefault = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_GitIgnoreEdit);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.AddPattern);
            this.splitContainer1.Panel2.Controls.Add(this.AddDefault);
            this.splitContainer1.Panel2.Controls.Add(this.Save);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(634, 519);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_GitIgnoreEdit
            // 
            this._NO_TRANSLATE_GitIgnoreEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_GitIgnoreEdit.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this._NO_TRANSLATE_GitIgnoreEdit.IgnoreWhitespaceChanges = false;
            this._NO_TRANSLATE_GitIgnoreEdit.IsReadOnly = false;
            this._NO_TRANSLATE_GitIgnoreEdit.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_GitIgnoreEdit.Name = "_NO_TRANSLATE_GitIgnoreEdit";
            this._NO_TRANSLATE_GitIgnoreEdit.NumberOfVisibleLines = 3;
            this._NO_TRANSLATE_GitIgnoreEdit.ScrollPos = 0;
            this._NO_TRANSLATE_GitIgnoreEdit.ShowEntireFile = false;
            this._NO_TRANSLATE_GitIgnoreEdit.ShowLineNumbers = true;
            this._NO_TRANSLATE_GitIgnoreEdit.Size = new System.Drawing.Size(400, 519);
            this._NO_TRANSLATE_GitIgnoreEdit.TabIndex = 0;
            this._NO_TRANSLATE_GitIgnoreEdit.TreatAllFilesAsText = false;
            // 
            // AddPattern
            // 
            this.AddPattern.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddPattern.Location = new System.Drawing.Point(67, 429);
            this.AddPattern.Name = "AddPattern";
            this.AddPattern.Size = new System.Drawing.Size(160, 25);
            this.AddPattern.TabIndex = 3;
            this.AddPattern.Text = "Add pattern";
            this.AddPattern.UseVisualStyleBackColor = true;
            this.AddPattern.Click += new System.EventHandler(this.AddPattern_Click);
            // 
            // AddDefault
            // 
            this.AddDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddDefault.Location = new System.Drawing.Point(67, 460);
            this.AddDefault.Name = "AddDefault";
            this.AddDefault.Size = new System.Drawing.Size(160, 25);
            this.AddDefault.TabIndex = 2;
            this.AddDefault.Text = "Add default ignores";
            this.AddDefault.UseVisualStyleBackColor = true;
            this.AddDefault.Click += new System.EventHandler(this.AddDefaultClick);
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Location = new System.Drawing.Point(67, 491);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(160, 25);
            this.Save.TabIndex = 1;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 312);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // FormGitIgnore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 519);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormGitIgnore";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit .gitignore";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGitIgnoreFormClosing);
            this.Load += new System.EventHandler(this.FormGitIgnoreLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private GitUI.Editor.FileViewer _NO_TRANSLATE_GitIgnoreEdit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button AddDefault;
        private System.Windows.Forms.Button AddPattern;

    }
}