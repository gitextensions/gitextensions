namespace GitUI.CommandsDialogs.Menus
{
    partial class ToolsToolStripMenuItem
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
            gitBashToolStripMenuItem = new ToolStripMenuItem();
            gitGUIToolStripMenuItem = new ToolStripMenuItem();
            kGitToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator41 = new ToolStripSeparator();
            toolStripSeparator6 = new ToolStripSeparator();
            PuTTYToolStripMenuItem = new ToolStripMenuItem();
            gitcommandLogToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            startAuthenticationAgentToolStripMenuItem = new ToolStripMenuItem();
            generateOrImportKeyToolStripMenuItem = new ToolStripMenuItem();
            // 
            // gitBashToolStripMenuItem
            // 
            gitBashToolStripMenuItem.Image = Properties.Images.GitForWindows;
            gitBashToolStripMenuItem.Name = "gitBashToolStripMenuItem";
            gitBashToolStripMenuItem.Size = new Size(217, 22);
            gitBashToolStripMenuItem.Text = "Git &bash";
            gitBashToolStripMenuItem.Click += gitBashToolStripMenuItem_Click;
            // 
            // gitGUIToolStripMenuItem
            // 
            gitGUIToolStripMenuItem.Image = Properties.Images.GitGui;
            gitGUIToolStripMenuItem.Name = "gitGUIToolStripMenuItem";
            gitGUIToolStripMenuItem.Size = new Size(217, 22);
            gitGUIToolStripMenuItem.Text = "Git &GUI";
            gitGUIToolStripMenuItem.Click += GitGuiToolStripMenuItemClick;
            // 
            // kGitToolStripMenuItem
            // 
            kGitToolStripMenuItem.Image = Properties.Images.Gitk;
            kGitToolStripMenuItem.Name = "kGitToolStripMenuItem";
            kGitToolStripMenuItem.Size = new Size(217, 22);
            kGitToolStripMenuItem.Text = "Git&K";
            kGitToolStripMenuItem.Click += KGitToolStripMenuItemClick;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(214, 6);
            // 
            // PuTTYToolStripMenuItem
            // 
            PuTTYToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            startAuthenticationAgentToolStripMenuItem,
            generateOrImportKeyToolStripMenuItem});
            PuTTYToolStripMenuItem.Image = Properties.Images.Putty;
            PuTTYToolStripMenuItem.Name = "PuTTYToolStripMenuItem";
            PuTTYToolStripMenuItem.Size = new Size(217, 22);
            PuTTYToolStripMenuItem.Text = "&PuTTY";
            // 
            // startAuthenticationAgentToolStripMenuItem
            // 
            startAuthenticationAgentToolStripMenuItem.Image = Properties.Images.Pageant16;
            startAuthenticationAgentToolStripMenuItem.Name = "startAuthenticationAgentToolStripMenuItem";
            startAuthenticationAgentToolStripMenuItem.Size = new Size(211, 22);
            startAuthenticationAgentToolStripMenuItem.Text = "Start authentication agent";
            startAuthenticationAgentToolStripMenuItem.Click += StartAuthenticationAgentToolStripMenuItemClick;
            // 
            // generateOrImportKeyToolStripMenuItem
            // 
            generateOrImportKeyToolStripMenuItem.Image = Properties.Images.PuttyGen;
            generateOrImportKeyToolStripMenuItem.Name = "generateOrImportKeyToolStripMenuItem";
            generateOrImportKeyToolStripMenuItem.Size = new Size(211, 22);
            generateOrImportKeyToolStripMenuItem.Text = "Generate or import key";
            generateOrImportKeyToolStripMenuItem.Click += GenerateOrImportKeyToolStripMenuItemClick;
            // 
            // toolStripSeparator41
            // 
            toolStripSeparator41.Name = "toolStripSeparator41";
            toolStripSeparator41.Size = new Size(214, 6);
            // 
            // gitcommandLogToolStripMenuItem
            // 
            gitcommandLogToolStripMenuItem.Image = Properties.Images.GitCommandLog;
            gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            gitcommandLogToolStripMenuItem.ShortcutKeys = Keys.F12;
            gitcommandLogToolStripMenuItem.Size = new Size(217, 22);
            gitcommandLogToolStripMenuItem.Text = "Git &command log";
            gitcommandLogToolStripMenuItem.Click += GitcommandLogToolStripMenuItemClick;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(214, 6);
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Image = Properties.Images.Settings;
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(217, 22);
            settingsToolStripMenuItem.Text = "&Settings...";
            settingsToolStripMenuItem.Click += OnShowSettingsClick;

            DropDownItems.AddRange(new ToolStripItem[] {
            gitBashToolStripMenuItem,
            gitGUIToolStripMenuItem,
            kGitToolStripMenuItem,
            toolStripSeparator6,
            PuTTYToolStripMenuItem,
            toolStripSeparator41,
            gitcommandLogToolStripMenuItem,
            toolStripSeparator7,
            settingsToolStripMenuItem});
            Text = "&Tools";
        }

        #endregion

        private ToolStripMenuItem gitBashToolStripMenuItem;
        private ToolStripMenuItem gitGUIToolStripMenuItem;
        private ToolStripMenuItem kGitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem PuTTYToolStripMenuItem;
        private ToolStripMenuItem startAuthenticationAgentToolStripMenuItem;
        private ToolStripMenuItem generateOrImportKeyToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator41;
        private ToolStripMenuItem gitcommandLogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
    }
}
