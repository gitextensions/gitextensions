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
            this.commitFontChangeButton = new System.Windows.Forms.Button();
            this.diffFontChangeButton = new System.Windows.Forms.Button();
            this.applicationFontChangeButton = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.diffFontDialog = new System.Windows.Forms.FontDialog();
            this.applicationDialog = new System.Windows.Forms.FontDialog();
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
            this.gbFonts.Padding = new System.Windows.Forms.Padding(16);
            this.gbFonts.Size = new System.Drawing.Size(2066, 181);
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
            this.tableLayoutPanel1.Controls.Add(this.commitFontChangeButton, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.diffFontChangeButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.applicationFontChangeButton, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label34, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label26, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(16, 42);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(2034, 123);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label56.Location = new System.Drawing.Point(3, 0);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(159, 41);
            this.label56.TabIndex = 0;
            this.label56.Text = "Code font";
            this.label56.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // commitFontChangeButton
            // 
            this.commitFontChangeButton.AutoSize = true;
            this.commitFontChangeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commitFontChangeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commitFontChangeButton.Location = new System.Drawing.Point(168, 85);
            this.commitFontChangeButton.Name = "commitFontChangeButton";
            this.commitFontChangeButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.commitFontChangeButton.Size = new System.Drawing.Size(130, 35);
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
            this.diffFontChangeButton.Location = new System.Drawing.Point(168, 3);
            this.diffFontChangeButton.Name = "diffFontChangeButton";
            this.diffFontChangeButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.diffFontChangeButton.Size = new System.Drawing.Size(130, 35);
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
            this.applicationFontChangeButton.Location = new System.Drawing.Point(168, 44);
            this.applicationFontChangeButton.Name = "applicationFontChangeButton";
            this.applicationFontChangeButton.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.applicationFontChangeButton.Size = new System.Drawing.Size(130, 35);
            this.applicationFontChangeButton.TabIndex = 3;
            this.applicationFontChangeButton.Text = "font name";
            this.applicationFontChangeButton.UseVisualStyleBackColor = true;
            this.applicationFontChangeButton.Click += new System.EventHandler(this.applicationFontChangeButton_Click);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label34.Location = new System.Drawing.Point(3, 82);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(159, 41);
            this.label34.TabIndex = 4;
            this.label34.Text = "Commit font";
            this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label26.Location = new System.Drawing.Point(3, 41);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(159, 41);
            this.label26.TabIndex = 2;
            this.label26.Text = "Application font";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            // commitFontDialog
            // 
            this.commitFontDialog.AllowVerticalFonts = false;
            this.commitFontDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // AppearanceFontsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.gbFonts);
            this.MinimumSize = new System.Drawing.Size(515, 510);
            this.Name = "AppearanceFontsSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(2082, 1141);
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
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.FontDialog diffFontDialog;
        private System.Windows.Forms.FontDialog applicationDialog;
        private System.Windows.Forms.Button commitFontChangeButton;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.FontDialog commitFontDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
