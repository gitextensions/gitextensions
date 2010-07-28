namespace GitUI
{
    partial class FormChangeLog
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
            this.ChangeLog = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // ChangeLog
            // 
            this.ChangeLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangeLog.Location = new System.Drawing.Point(0, 0);
            this.ChangeLog.Name = "ChangeLog";
            this.ChangeLog.Size = new System.Drawing.Size(849, 411);
            this.ChangeLog.TabIndex = 0;
            this.ChangeLog.Text = "";
            // 
            // FormChangeLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(849, 411);
            this.Controls.Add(this.ChangeLog);
            this.Name = "FormChangeLog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change log";
            this.Load += new System.EventHandler(this.FormChangeLog1Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormChangeLog1FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox ChangeLog;
    }
}