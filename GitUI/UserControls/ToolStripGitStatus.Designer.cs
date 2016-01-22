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
                if (_repoStatusSubscription != null)
                {
                    _repoStatusSubscription.Dispose();
                }
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
            this.ignoredFilesTimer = new System.Windows.Forms.Timer(this.components);
            // 
            // ignoredFilesTimer
            // 
            this.ignoredFilesTimer.Interval = 600000;
            this.ignoredFilesTimer.Tick += new System.EventHandler(this.ignoredFilesTimer_Tick);

        }

        #endregion

        private System.Windows.Forms.Timer ignoredFilesTimer;
    }
}
