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
            groupBoxConsoleSettings = new GroupBox();
            consoleFontChangeButton = new Button();
            lblFontName = new Label();
            label1 = new Label();
            _NO_TRANSLATE_cboStyle = new ComboBox();
            consoleFontDialog = new FontDialog();
            groupBoxConsoleSettings.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxConsoleSettings
            // 
            groupBoxConsoleSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxConsoleSettings.AutoSize = true;
            groupBoxConsoleSettings.Controls.Add(consoleFontChangeButton);
            groupBoxConsoleSettings.Controls.Add(lblFontName);
            groupBoxConsoleSettings.Controls.Add(label1);
            groupBoxConsoleSettings.Controls.Add(_NO_TRANSLATE_cboStyle);
            groupBoxConsoleSettings.Location = new Point(8, 10);
            groupBoxConsoleSettings.Margin = new Padding(17, 2, 3, 2);
            groupBoxConsoleSettings.Name = "groupBoxConsoleSettings";
            groupBoxConsoleSettings.Padding = new Padding(3, 2, 3, 2);
            groupBoxConsoleSettings.Size = new Size(1653, 95);
            groupBoxConsoleSettings.TabIndex = 1;
            groupBoxConsoleSettings.TabStop = false;
            groupBoxConsoleSettings.Text = "Console settings (restart required)";
            // 
            // consoleFontChangeButton
            // 
            consoleFontChangeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            consoleFontChangeButton.Location = new Point(118, 54);
            consoleFontChangeButton.Margin = new Padding(0);
            consoleFontChangeButton.Name = "consoleFontChangeButton";
            consoleFontChangeButton.Size = new Size(262, 23);
            consoleFontChangeButton.TabIndex = 6;
            consoleFontChangeButton.Text = "font name";
            consoleFontChangeButton.UseVisualStyleBackColor = true;
            consoleFontChangeButton.Click += consoleFontChangeButton_Click;
            // 
            // lblFontName
            // 
            lblFontName.AutoSize = true;
            lblFontName.Location = new Point(5, 58);
            lblFontName.Name = "lblFontName";
            lblFontName.Size = new Size(31, 15);
            lblFontName.TabIndex = 4;
            lblFontName.Text = "Font";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 28);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 2;
            label1.Text = "Console style";
            // 
            // _NO_TRANSLATE_cboStyle
            // 
            _NO_TRANSLATE_cboStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_cboStyle.FormattingEnabled = true;
            _NO_TRANSLATE_cboStyle.Items.AddRange(new object[] {
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
            _NO_TRANSLATE_cboStyle.Location = new Point(118, 26);
            _NO_TRANSLATE_cboStyle.Margin = new Padding(3, 2, 3, 2);
            _NO_TRANSLATE_cboStyle.Name = "_NO_TRANSLATE_cboStyle";
            _NO_TRANSLATE_cboStyle.Size = new Size(262, 23);
            _NO_TRANSLATE_cboStyle.TabIndex = 3;
            // 
            // consoleFontDialog
            // 
            consoleFontDialog.AllowVerticalFonts = false;
            consoleFontDialog.Color = SystemColors.ControlText;
            // 
            // ConsoleStyleSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1679, 477);
            Controls.Add(groupBoxConsoleSettings);
            Name = "ConsoleStyleSettingsPage";
            Padding = new Padding(8);
            Text = "Console style";
            groupBoxConsoleSettings.ResumeLayout(false);
            groupBoxConsoleSettings.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox groupBoxConsoleSettings;
        private Label lblFontName;
        private Label label1;
        private ComboBox _NO_TRANSLATE_cboStyle;
        private Button consoleFontChangeButton;
        private FontDialog consoleFontDialog;
    }
}
