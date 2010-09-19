namespace GitUI
{
    partial class FormMailMap
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._NO_TRANSLATE_MailMapText = new GitUI.Editor.FileViewer();
            this.label1 = new System.Windows.Forms.Label();
            this.Save = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_MailMapText);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.Save);
            this.splitContainer1.Size = new System.Drawing.Size(634, 474);
            this.splitContainer1.SplitterDistance = 381;
            this.splitContainer1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_MailMapText
            // 
            this._NO_TRANSLATE_MailMapText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_MailMapText.IsReadOnly = false;
            this._NO_TRANSLATE_MailMapText.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_MailMapText.Name = "_NO_TRANSLATE_MailMapText";
            this._NO_TRANSLATE_MailMapText.Size = new System.Drawing.Size(381, 474);
            this._NO_TRANSLATE_MailMapText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(233, 78);
            this.label1.TabIndex = 1;
            this.label1.Text = "Edit the mailmap.\r\nThis file is meant to correct usernames.\r\n\r\nExample:\r\nHenk Wes" +
                "thuis <Henk@.(none)>\r\nHenk Westhuis <henk_westhuis@hotmail.com>";
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Location = new System.Drawing.Point(162, 439);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 0;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // FormMailMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 474);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormMailMap";
            this.Text = "Edit .mailmap";
            this.Load += new System.EventHandler(this.FormMailMapLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMailMapFormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private GitUI.Editor.FileViewer _NO_TRANSLATE_MailMapText;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Label label1;
    }
}