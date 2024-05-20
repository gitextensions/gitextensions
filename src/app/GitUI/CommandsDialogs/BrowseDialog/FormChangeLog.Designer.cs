namespace GitUI.CommandsDialogs.BrowseDialog
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
            ChangeLog = new RichTextBox();
            SuspendLayout();
            // 
            // ChangeLog
            // 
            ChangeLog.Dock = DockStyle.Fill;
            ChangeLog.Location = new Point(0, 0);
            ChangeLog.Name = "ChangeLog";
            ChangeLog.ReadOnly = true;
            ChangeLog.Size = new Size(849, 411);
            ChangeLog.TabIndex = 0;
            ChangeLog.Text = "";
            ChangeLog.WordWrap = false;
            // 
            // FormChangeLog
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(849, 411);
            Controls.Add(ChangeLog);
            Name = "FormChangeLog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Change log";
            ResumeLayout(false);

        }

        #endregion

        private RichTextBox ChangeLog;
    }
}