namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class DetailedSettingsPage
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
            this.PushWindowGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkRemotesFromServer = new System.Windows.Forms.CheckBox();
            this.BrowseRepoGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxConsoleSettings = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboFontSize = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cboTerminal = new System.Windows.Forms.ComboBox();
            this.cboStyle = new System.Windows.Forms.ComboBox();
            this.chkChowConsoleTab = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.PushWindowGB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.BrowseRepoGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBoxConsoleSettings.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // PushWindowGB
            // 
            this.PushWindowGB.AutoSize = true;
            this.PushWindowGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PushWindowGB.Controls.Add(this.tableLayoutPanel1);
            this.PushWindowGB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PushWindowGB.Location = new System.Drawing.Point(3, 198);
            this.PushWindowGB.Name = "PushWindowGB";
            this.PushWindowGB.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.PushWindowGB.Size = new System.Drawing.Size(1103, 53);
            this.PushWindowGB.TabIndex = 1;
            this.PushWindowGB.TabStop = false;
            this.PushWindowGB.Text = "Push window";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chkRemotesFromServer, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 22);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1087, 23);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // chkRemotesFromServer
            // 
            this.chkRemotesFromServer.AutoSize = true;
            this.chkRemotesFromServer.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.chkRemotesFromServer.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkRemotesFromServer.Location = new System.Drawing.Point(3, 3);
            this.chkRemotesFromServer.Name = "chkRemotesFromServer";
            this.chkRemotesFromServer.Size = new System.Drawing.Size(1081, 17);
            this.chkRemotesFromServer.TabIndex = 4;
            this.chkRemotesFromServer.Text = "Get remote branches directly from the remote";
            this.chkRemotesFromServer.UseVisualStyleBackColor = true;
            // 
            // BrowseRepoGB
            // 
            this.BrowseRepoGB.AutoSize = true;
            this.BrowseRepoGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BrowseRepoGB.Controls.Add(this.tableLayoutPanel3);
            this.BrowseRepoGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.BrowseRepoGB.Location = new System.Drawing.Point(3, 3);
            this.BrowseRepoGB.Name = "BrowseRepoGB";
            this.BrowseRepoGB.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.BrowseRepoGB.Size = new System.Drawing.Size(1103, 189);
            this.BrowseRepoGB.TabIndex = 0;
            this.BrowseRepoGB.TabStop = false;
            this.BrowseRepoGB.Text = "Browse repository window";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.groupBoxConsoleSettings, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.chkChowConsoleTab, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(8, 22);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1087, 159);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // groupBoxConsoleSettings
            // 
            this.groupBoxConsoleSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConsoleSettings.AutoSize = true;
            this.groupBoxConsoleSettings.Controls.Add(this.label3);
            this.groupBoxConsoleSettings.Controls.Add(this.cboFontSize);
            this.groupBoxConsoleSettings.Controls.Add(this.label2);
            this.groupBoxConsoleSettings.Controls.Add(this.label1);
            this.groupBoxConsoleSettings.Controls.Add(this.cboTerminal);
            this.groupBoxConsoleSettings.Controls.Add(this.cboStyle);
            this.groupBoxConsoleSettings.Location = new System.Drawing.Point(17, 25);
            this.groupBoxConsoleSettings.Margin = new System.Windows.Forms.Padding(17, 2, 3, 2);
            this.groupBoxConsoleSettings.Name = "groupBoxConsoleSettings";
            this.groupBoxConsoleSettings.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxConsoleSettings.Size = new System.Drawing.Size(1067, 132);
            this.groupBoxConsoleSettings.TabIndex = 3;
            this.groupBoxConsoleSettings.TabStop = false;
            this.groupBoxConsoleSettings.Text = "Console settings (a restart is needed to take effect)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Font size";
            // 
            // cboFontSize
            // 
            this.cboFontSize.FormattingEnabled = true;
            this.cboFontSize.Items.AddRange(new object[] {
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "16",
            "18",
            "19",
            "20",
            "24"});
            this.cboFontSize.Location = new System.Drawing.Point(118, 93);
            this.cboFontSize.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboFontSize.Name = "cboFontSize";
            this.cboFontSize.Size = new System.Drawing.Size(262, 21);
            this.cboFontSize.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Shell to run";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Console style";
            // 
            // cboTerminal
            // 
            this.cboTerminal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTerminal.FormattingEnabled = true;
            this.cboTerminal.Items.AddRange(new object[] {
            "bash",
            "cmd",
            "powershell"});
            this.cboTerminal.Location = new System.Drawing.Point(118, 59);
            this.cboTerminal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboTerminal.Name = "cboTerminal";
            this.cboTerminal.Size = new System.Drawing.Size(262, 21);
            this.cboTerminal.TabIndex = 4;
            // 
            // cboStyle
            // 
            this.cboStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStyle.FormattingEnabled = true;
            this.cboStyle.Items.AddRange(new object[] {
            "Default",
            "<Default Windows scheme>",
            "<Base16>",
            "<Cobalt2>",
            "<ConEmu>",
            "<Gamma 1>",
            "<Monokai>",
            "<Murena scheme>",
            "<PowerShell>",
            "<Solarized>",
            "<Solarized Git>",
            "<Solarized (Luke Maciak)>",
            "<Solarized (John Doe)>",
            "<Solarized Light>",
            "<SolarMe>",
            "<Standard VGA>",
            "<tc-maxx>",
            "<Terminal.app>",
            "<Tomorrow>",
            "<Tomorrow Night>",
            "<Tomorrow Night Blue>",
            "<Tomorrow Night Bright>",
            "<Tomorrow Night Eighties>",
            "<Twilight>",
            "<Ubuntu>",
            "<xterm>",
            "<Zenburn>"});
            this.cboStyle.Location = new System.Drawing.Point(118, 26);
            this.cboStyle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboStyle.Name = "cboStyle";
            this.cboStyle.Size = new System.Drawing.Size(262, 21);
            this.cboStyle.TabIndex = 3;
            // 
            // chkChowConsoleTab
            // 
            this.chkChowConsoleTab.AutoSize = true;
            this.chkChowConsoleTab.Location = new System.Drawing.Point(3, 3);
            this.chkChowConsoleTab.Name = "chkChowConsoleTab";
            this.chkChowConsoleTab.Size = new System.Drawing.Size(131, 17);
            this.chkChowConsoleTab.TabIndex = 0;
            this.chkChowConsoleTab.Text = "Show the Console tab";
            this.chkChowConsoleTab.UseVisualStyleBackColor = true;
            this.chkChowConsoleTab.CheckedChanged += new System.EventHandler(this.chkChowConsoleTab_CheckedChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.BrowseRepoGB, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.PushWindowGB, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1109, 461);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // DetailedSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "DetailedSettingsPage";
            this.Size = new System.Drawing.Size(1109, 461);
            this.PushWindowGB.ResumeLayout(false);
            this.PushWindowGB.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.BrowseRepoGB.ResumeLayout(false);
            this.BrowseRepoGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBoxConsoleSettings.ResumeLayout(false);
            this.groupBoxConsoleSettings.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox PushWindowGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkRemotesFromServer;
        private System.Windows.Forms.GroupBox BrowseRepoGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox chkChowConsoleTab;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBoxConsoleSettings;
        private System.Windows.Forms.ComboBox cboTerminal;
        private System.Windows.Forms.ComboBox cboStyle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboFontSize;
    }
}