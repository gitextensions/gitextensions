namespace GitUI
{
    partial class ToolStripGitStatus
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.ignoredFilesTimer = new System.Windows.Forms.Timer(this.components);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Enabled = true;
            this.timerRefresh.Interval = 500;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // ignoredFilesTimer
            // 
            this.ignoredFilesTimer.Interval = 600000;
            this.ignoredFilesTimer.Tick += new System.EventHandler(this.ignoredFilesTimer_Tick);

        }

        #endregion

        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Timer ignoredFilesTimer;
    }
}
