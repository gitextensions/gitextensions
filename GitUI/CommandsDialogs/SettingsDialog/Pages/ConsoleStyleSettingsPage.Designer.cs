namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ConsoleStyleSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxConsoleSettings = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboFontSize = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_cboStyle = new System.Windows.Forms.ComboBox();
            this.groupBoxConsoleSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxConsoleSettings
            // 
            this.groupBoxConsoleSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConsoleSettings.AutoSize = true;
            this.groupBoxConsoleSettings.Controls.Add(this.label3);
            this.groupBoxConsoleSettings.Controls.Add(this.cboFontSize);
            this.groupBoxConsoleSettings.Controls.Add(this.label1);
            this.groupBoxConsoleSettings.Controls.Add(this._NO_TRANSLATE_cboStyle);
            this.groupBoxConsoleSettings.Location = new System.Drawing.Point(8, 10);
            this.groupBoxConsoleSettings.Margin = new System.Windows.Forms.Padding(17, 2, 3, 2);
            this.groupBoxConsoleSettings.Name = "groupBoxConsoleSettings";
            this.groupBoxConsoleSettings.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxConsoleSettings.Size = new System.Drawing.Size(1653, 89);
            this.groupBoxConsoleSettings.TabIndex = 1;
            this.groupBoxConsoleSettings.TabStop = false;
            this.groupBoxConsoleSettings.Text = "Console settings (restart required)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 4;
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
            this.cboFontSize.Location = new System.Drawing.Point(118, 51);
            this.cboFontSize.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboFontSize.Name = "cboFontSize";
            this.cboFontSize.Size = new System.Drawing.Size(262, 21);
            this.cboFontSize.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Console style";
            // 
            // _NO_TRANSLATE_cboStyle
            // 
            this._NO_TRANSLATE_cboStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_cboStyle.FormattingEnabled = true;
            this._NO_TRANSLATE_cboStyle.Items.AddRange(new object[] {
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
            this._NO_TRANSLATE_cboStyle.Location = new System.Drawing.Point(118, 26);
            this._NO_TRANSLATE_cboStyle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._NO_TRANSLATE_cboStyle.Name = "_NO_TRANSLATE_cboStyle";
            this._NO_TRANSLATE_cboStyle.Size = new System.Drawing.Size(262, 21);
            this._NO_TRANSLATE_cboStyle.TabIndex = 3;
            // 
            // ConsoleStyleSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBoxConsoleSettings);
            this.Name = "ConsoleStyleSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1711, 555);
            this.groupBoxConsoleSettings.ResumeLayout(false);
            this.groupBoxConsoleSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxConsoleSettings;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboFontSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cboStyle;
    }
}
