namespace GitUI.CommandsDialogs.Menus
{
    partial class HelpToolStripMenuItem
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
            components = new System.ComponentModel.Container();
            userManualToolStripMenuItem = new ToolStripMenuItem();
            changelogToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            translateToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator16 = new ToolStripSeparator();
            donateToolStripMenuItem = new ToolStripMenuItem();
            tsmiTelemetryEnabled = new ToolStripMenuItem();
            reportAnIssueToolStripMenuItem = new ToolStripMenuItem();
            checkForUpdatesToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            // 
            // userManualToolStripMenuItem
            // 
            userManualToolStripMenuItem.Image = Properties.Images.GotoManual;
            userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            userManualToolStripMenuItem.Size = new Size(184, 22);
            userManualToolStripMenuItem.Text = "User &manual";
            userManualToolStripMenuItem.Click += UserManualToolStripMenuItemClick;
            // 
            // changelogToolStripMenuItem
            // 
            changelogToolStripMenuItem.Image = Properties.Images.Changelog;
            changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            changelogToolStripMenuItem.Size = new Size(184, 22);
            changelogToolStripMenuItem.Text = "&Changelog";
            changelogToolStripMenuItem.Click += ChangelogToolStripMenuItemClick;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(181, 6);
            // 
            // translateToolStripMenuItem
            // 
            translateToolStripMenuItem.Image = Properties.Images.Translate;
            translateToolStripMenuItem.Name = "translateToolStripMenuItem";
            translateToolStripMenuItem.Size = new Size(184, 22);
            translateToolStripMenuItem.Text = "&Translate";
            translateToolStripMenuItem.Click += TranslateToolStripMenuItemClick;
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new Size(181, 6);
            // 
            // donateToolStripMenuItem
            // 
            donateToolStripMenuItem.Image = Properties.Images.DollarSign;
            donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            donateToolStripMenuItem.Size = new Size(184, 22);
            donateToolStripMenuItem.Text = "&Donate";
            donateToolStripMenuItem.Click += DonateToolStripMenuItemClick;
            // 
            // tsmiTelemetryEnabled
            // 
            tsmiTelemetryEnabled.Checked = true;
            tsmiTelemetryEnabled.CheckState = CheckState.Checked;
            tsmiTelemetryEnabled.Name = "tsmiTelemetryEnabled";
            tsmiTelemetryEnabled.Size = new Size(184, 22);
            tsmiTelemetryEnabled.Text = "&Yes, I allow telemetry";
            tsmiTelemetryEnabled.Click += TsmiTelemetryEnabled_Click;
            // 
            // reportAnIssueToolStripMenuItem
            // 
            reportAnIssueToolStripMenuItem.Image = Properties.Images.BugReport;
            reportAnIssueToolStripMenuItem.Name = "reportAnIssueToolStripMenuItem";
            reportAnIssueToolStripMenuItem.Size = new Size(184, 22);
            reportAnIssueToolStripMenuItem.Text = "&Report an issue";
            reportAnIssueToolStripMenuItem.Click += reportAnIssueToolStripMenuItem_Click;
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            checkForUpdatesToolStripMenuItem.Image = Properties.Images.CheckForUpdates;
            checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            checkForUpdatesToolStripMenuItem.Size = new Size(184, 22);
            checkForUpdatesToolStripMenuItem.Text = "Check for &updates";
            checkForUpdatesToolStripMenuItem.Click += checkForUpdatesToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Image = Properties.Images.Information;
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(184, 22);
            aboutToolStripMenuItem.Text = "&About";
            aboutToolStripMenuItem.Click += AboutToolStripMenuItemClick;
            // 
            // HelpToolStripMenuItem
            // 
            DropDownItems.AddRange(new ToolStripItem[] {
            userManualToolStripMenuItem,
            changelogToolStripMenuItem,
            toolStripSeparator3,
            translateToolStripMenuItem,
            toolStripSeparator16,
            donateToolStripMenuItem,
            tsmiTelemetryEnabled,
            reportAnIssueToolStripMenuItem,
            checkForUpdatesToolStripMenuItem,
            aboutToolStripMenuItem});
            Text = "&Help";
            DropDownOpening += this_DropDownOpening;
        }

        #endregion

        private ToolStripMenuItem userManualToolStripMenuItem;
        private ToolStripMenuItem changelogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem translateToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem donateToolStripMenuItem;
        private ToolStripMenuItem tsmiTelemetryEnabled;
        private ToolStripMenuItem reportAnIssueToolStripMenuItem;
        private ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
    }
}
