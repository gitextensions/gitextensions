using System;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class GitStatusMonitor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private bool disposed = false;

        ~GitStatusMonitor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _workTreeWatcher.Dispose();
                    _gitDirWatcher.Dispose();
                    _globalIgnoreWatcher.Dispose();
                    ignoredFilesTimer.Dispose();
                    ignoredFilesTimer = null;
                    timerRefresh.Dispose();
                    timerRefresh = null;
                    if (components != null)
                        components.Dispose();
                }
                disposed = true;
            }
        }


        #region Windows Form Designer generated code

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
            // _timerRefresh
            // 
            this.timerRefresh.Enabled = true;
            this.timerRefresh.Interval = 500;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // _ignoredFilesTimer
            // 
            this.ignoredFilesTimer.Interval = 600000;
            this.ignoredFilesTimer.Tick += new System.EventHandler(this.ignoredFilesTimer_Tick);
        }
        #endregion

        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Timer ignoredFilesTimer;
    }
}