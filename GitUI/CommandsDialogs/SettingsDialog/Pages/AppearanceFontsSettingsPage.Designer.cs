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
            this.gbFonts = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label56 = new System.Windows.Forms.Label();
            this.monospaceFontChangeButton = new System.Windows.Forms.Button();
            this.commitFontChangeButton = new System.Windows.Forms.Button();
            this.diffFontChangeButton = new System.Windows.Forms.Button();
            this.applicationFontChangeButton = new System.Windows.Forms.Button();
            this.label36 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.diffFontDialog = new System.Windows.Forms.FontDialog();
            this.applicationDialog = new System.Windows.Forms.FontDialog();
            this.monospaceFontDialog = new System.Windows.Forms.FontDialog();
            this.commitFontDialog = new System.Windows.Forms.FontDialog();
            this.gbFonts.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbFonts
            // 
            this.gbFonts.AutoSize = true;
            this.gbFonts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbFonts.Controls.Add(this.tableLayoutPanel1);
            this.gbFonts.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbFonts.Location = new System.Drawing.Point(8, 8);
            this.gbFonts.Name = "gbFonts";
            this.gbFonts.Padding = new System.Windows.Forms.Padding(8);
            this.gbFonts.Size = new System.Drawing.Size(2132, 117);
            this.gbFonts.TabIndex = 2;
            this.gbFonts.TabStop = false;
            this.gbFonts.Text = "Fonts";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label56, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.monospaceFontChangeButton, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.commitFontChangeButton, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.diffFontChangeButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.applicationFontChangeButton, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label36, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label34, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label26, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 22);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(2116, 116);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label56.Location = new System.Drawing.Point(3, 0);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(82, 29);
            this.label56.TabIndex = 0;
            this.label56.Text = "Code font";
            this.label56.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // commitFontChangeButton
            // 
            this.commitFontChangeButton.AutoSize = true;
            this.commitFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitFontChangeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitFontChangeButton.Location = new System.Drawing.Point(91, 61);
            this.commitFontChangeButton.Name = "commitFontChangeButton";
            this.commitFontChangeButton.Size = new System.Drawing.Size(66, 23);
            this.commitFontChangeButton.TabIndex = 5;
            this.commitFontChangeButton.Text = "font name";
            this.commitFontChangeButton.UseVisualStyleBackColor = true;
            this.commitFontChangeButton.Click += new System.EventHandler(this.commitFontChangeButton_Click);
            // 
            // diffFontChangeButton
            // 
            this.diffFontChangeButton.AutoSize = true;
            this.diffFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.diffFontChangeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diffFontChangeButton.Location = new System.Drawing.Point(91, 3);
            this.diffFontChangeButton.Name = "diffFontChangeButton";
            this.diffFontChangeButton.Size = new System.Drawing.Size(66, 23);
            this.diffFontChangeButton.TabIndex = 1;
            this.diffFontChangeButton.Text = "font name";
            this.diffFontChangeButton.UseVisualStyleBackColor = true;
            this.diffFontChangeButton.Click += new System.EventHandler(this.diffFontChangeButton_Click);
            // 
            // applicationFontChangeButton
            // 
            this.applicationFontChangeButton.AutoSize = true;
            this.applicationFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.applicationFontChangeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.applicationFontChangeButton.Location = new System.Drawing.Point(91, 32);
            this.applicationFontChangeButton.Name = "applicationFontChangeButton";
            this.applicationFontChangeButton.Size = new System.Drawing.Size(66, 23);
            this.applicationFontChangeButton.TabIndex = 3;
            this.applicationFontChangeButton.Text = "font name";
            this.applicationFontChangeButton.UseVisualStyleBackColor = true;
            this.applicationFontChangeButton.Click += new System.EventHandler(this.applicationFontChangeButton_Click);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label34.Location = new System.Drawing.Point(3, 58);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(82, 29);
            this.label34.TabIndex = 4;
            this.label34.Text = "Commit font";
            this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label26.Location = new System.Drawing.Point(3, 29);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(82, 29);
            this.label26.TabIndex = 2;
            this.label26.Text = "Application font";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label36.Location = new System.Drawing.Point(3, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(82, 29);
            this.label36.TabIndex = 0;
            this.label36.Text = "Monospace font";
            this.label36.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // monospaceFontChangeButton
            // 
            this.monospaceFontChangeButton.AutoSize = true;
            this.monospaceFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.monospaceFontChangeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.monospaceFontChangeButton.Location = new System.Drawing.Point(91, 61);
            this.monospaceFontChangeButton.Name = "monospaceFontChangeButton";
            this.monospaceFontChangeButton.Size = new System.Drawing.Size(66, 23);
            this.monospaceFontChangeButton.TabIndex = 6;
            this.monospaceFontChangeButton.Text = "font name";
            this.monospaceFontChangeButton.UseVisualStyleBackColor = true;
            this.monospaceFontChangeButton.Click += new System.EventHandler(this.monospaceFontChangeButton_Click);
            // 
            // diffFontDialog
            // 
            this.diffFontDialog.AllowVerticalFonts = false;
            this.diffFontDialog.Color = System.Drawing.SystemColors.ControlText;
            this.diffFontDialog.FixedPitchOnly = true;
            // 
            // applicationDialog
            // 
            this.applicationDialog.AllowVerticalFonts = false;
            this.applicationDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // monospaceFontDialog
            // 
            this.monospaceFontDialog.AllowVerticalFonts = false;
            this.monospaceFontDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // commitFontDialog
            // 
            this.commitFontDialog.AllowVerticalFonts = false;
            this.commitFontDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // AppearanceFontsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.gbFonts);
            this.MinimumSize = new System.Drawing.Size(258, 255);
            this.Name = "AppearanceFontsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(2148, 1371);
            this.gbFonts.ResumeLayout(false);
            this.gbFonts.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox gbFonts;
        private System.Windows.Forms.Button diffFontChangeButton;
        private System.Windows.Forms.Button applicationFontChangeButton;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.FontDialog diffFontDialog;
        private System.Windows.Forms.FontDialog applicationDialog;
        private System.Windows.Forms.Button monospaceFontChangeButton;
        private System.Windows.Forms.Button commitFontChangeButton;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.FontDialog monospaceFontDialog;
        private System.Windows.Forms.FontDialog commitFontDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
