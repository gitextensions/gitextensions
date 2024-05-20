namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class AppearanceFontsSettingsPage
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
            gbFonts = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            label56 = new Label();
            monospaceFontChangeButton = new Button();
            commitFontChangeButton = new Button();
            diffFontChangeButton = new Button();
            ShowEolMarkerAsGlyph = new CheckBox();
            applicationFontChangeButton = new Button();
            label36 = new Label();
            label34 = new Label();
            label26 = new Label();
            diffFontDialog = new FontDialog();
            applicationDialog = new FontDialog();
            monospaceFontDialog = new FontDialog();
            commitFontDialog = new FontDialog();
            gbFonts.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // gbFonts
            // 
            gbFonts.AutoSize = true;
            gbFonts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbFonts.Controls.Add(tableLayoutPanel1);
            gbFonts.Dock = DockStyle.Top;
            gbFonts.Location = new Point(8, 8);
            gbFonts.Name = "gbFonts";
            gbFonts.Padding = new Padding(8);
            gbFonts.Size = new Size(2132, 117);
            gbFonts.TabIndex = 2;
            gbFonts.TabStop = false;
            gbFonts.Text = "Fonts (restart required)";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(label56, 0, 0);
            tableLayoutPanel1.Controls.Add(diffFontChangeButton, 1, 0);
            tableLayoutPanel1.Controls.Add(ShowEolMarkerAsGlyph, 2, 0);
            tableLayoutPanel1.Controls.Add(label26, 0, 1);
            tableLayoutPanel1.Controls.Add(applicationFontChangeButton, 1, 1);
            tableLayoutPanel1.Controls.Add(label34, 0, 2);
            tableLayoutPanel1.Controls.Add(commitFontChangeButton, 1, 2);
            tableLayoutPanel1.Controls.Add(label36, 0, 3);
            tableLayoutPanel1.Controls.Add(monospaceFontChangeButton, 1, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(8, 22);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(2116, 116);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // label56
            // 
            label56.AutoSize = true;
            label56.Dock = DockStyle.Fill;
            label56.Location = new Point(3, 0);
            label56.Name = "label56";
            label56.Size = new Size(82, 29);
            label56.TabIndex = 0;
            label56.Text = "Code font";
            label56.TextAlign = ContentAlignment.MiddleLeft;
            // commitFontChangeButton
            // 
            commitFontChangeButton.AutoSize = true;
            commitFontChangeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commitFontChangeButton.Dock = DockStyle.Fill;
            commitFontChangeButton.Location = new Point(91, 61);
            commitFontChangeButton.Name = "commitFontChangeButton";
            commitFontChangeButton.Size = new Size(66, 23);
            commitFontChangeButton.TabIndex = 2;
            commitFontChangeButton.Text = "font name";
            commitFontChangeButton.UseVisualStyleBackColor = true;
            commitFontChangeButton.Click += commitFontChangeButton_Click;
            // 
            // diffFontChangeButton
            // 
            diffFontChangeButton.AutoSize = true;
            diffFontChangeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            diffFontChangeButton.Dock = DockStyle.Fill;
            diffFontChangeButton.Location = new Point(91, 3);
            diffFontChangeButton.Name = "diffFontChangeButton";
            diffFontChangeButton.Size = new Size(66, 23);
            diffFontChangeButton.TabIndex = 0;
            diffFontChangeButton.Text = "font name";
            diffFontChangeButton.UseVisualStyleBackColor = true;
            diffFontChangeButton.Click += diffFontChangeButton_Click;
            // 
            // ShowEolMarkersAsGlyph
            // 
            ShowEolMarkerAsGlyph.AutoSize = true;
            ShowEolMarkerAsGlyph.Dock = DockStyle.Fill;
            ShowEolMarkerAsGlyph.Location = new Point(181, 3);
            ShowEolMarkerAsGlyph.Name = "ShowEolMarkerAsGlyph";
            ShowEolMarkerAsGlyph.Padding = new Padding(8, 2, 0, 0);
            ShowEolMarkerAsGlyph.Size = new Size(1336, 25);
            ShowEolMarkerAsGlyph.TabIndex = 5;
            ShowEolMarkerAsGlyph.Text = "Show end-of-line markers as glyph instead of \"\\r\\n\" etc.";
            ShowEolMarkerAsGlyph.UseVisualStyleBackColor = true;
            // 
            // applicationFontChangeButton
            // 
            applicationFontChangeButton.AutoSize = true;
            applicationFontChangeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            applicationFontChangeButton.Dock = DockStyle.Fill;
            applicationFontChangeButton.Location = new Point(91, 32);
            applicationFontChangeButton.Name = "applicationFontChangeButton";
            applicationFontChangeButton.Size = new Size(66, 23);
            applicationFontChangeButton.TabIndex = 1;
            applicationFontChangeButton.Text = "font name";
            applicationFontChangeButton.UseVisualStyleBackColor = true;
            applicationFontChangeButton.Click += applicationFontChangeButton_Click;
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Dock = DockStyle.Fill;
            label34.Location = new Point(3, 58);
            label34.Name = "label34";
            label34.Size = new Size(82, 29);
            label34.TabIndex = 4;
            label34.Text = "Commit font";
            label34.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Dock = DockStyle.Fill;
            label26.Location = new Point(3, 29);
            label26.Name = "label26";
            label26.Size = new Size(82, 29);
            label26.TabIndex = 2;
            label26.Text = "Application font";
            label26.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label36
            // 
            label36.AutoSize = true;
            label36.Dock = DockStyle.Fill;
            label36.Location = new Point(3, 0);
            label36.Name = "label36";
            label36.Size = new Size(82, 29);
            label36.TabIndex = 0;
            label36.Text = "Monospace font";
            label36.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // monospaceFontChangeButton
            // 
            monospaceFontChangeButton.AutoSize = true;
            monospaceFontChangeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            monospaceFontChangeButton.Dock = DockStyle.Fill;
            monospaceFontChangeButton.Location = new Point(91, 61);
            monospaceFontChangeButton.Name = "monospaceFontChangeButton";
            monospaceFontChangeButton.Size = new Size(66, 23);
            monospaceFontChangeButton.Text = "font name";
            monospaceFontChangeButton.UseVisualStyleBackColor = true;
            monospaceFontChangeButton.Click += monospaceFontChangeButton_Click;
            monospaceFontChangeButton.TabIndex = 3;
            // 
            // diffFontDialog
            // 
            diffFontDialog.AllowVerticalFonts = false;
            diffFontDialog.Color = SystemColors.ControlText;
            diffFontDialog.FixedPitchOnly = true;
            // 
            // applicationDialog
            // 
            applicationDialog.AllowVerticalFonts = false;
            applicationDialog.Color = SystemColors.ControlText;
            // 
            // monospaceFontDialog
            // 
            monospaceFontDialog.AllowVerticalFonts = false;
            monospaceFontDialog.Color = SystemColors.ControlText;
            // 
            // commitFontDialog
            // 
            commitFontDialog.AllowVerticalFonts = false;
            commitFontDialog.Color = SystemColors.ControlText;
            // 
            // AppearanceFontsSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(gbFonts);
            MinimumSize = new Size(258, 255);
            Name = "AppearanceFontsSettingsPage";
            Padding = new Padding(8);
            Size = new Size(2148, 1371);
            Text = "Fonts";
            gbFonts.ResumeLayout(false);
            gbFonts.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox gbFonts;
        private Button diffFontChangeButton;
        private Button applicationFontChangeButton;
        private Label label26;
        private Label label36;
        private Label label56;
        private FontDialog diffFontDialog;
        private FontDialog applicationDialog;
        private Button monospaceFontChangeButton;
        private Button commitFontChangeButton;
        private Label label34;
        private FontDialog monospaceFontDialog;
        private FontDialog commitFontDialog;
        private TableLayoutPanel tableLayoutPanel1;
        private CheckBox ShowEolMarkerAsGlyph;
    }
}
