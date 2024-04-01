using GitUI.UserControls.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ColorsSettingsPage
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
            components = new System.ComponentModel.Container();
            TableLayoutPanel tlpnlMain;
            gbRevisionGraph = new GroupBox();
            tlpnlRevisionGraph = new TableLayoutPanel();
            chkFillRefLabels = new CheckBox();
            chkHighlightAuthored = new CheckBox();
            MulticolorBranches = new CheckBox();
            chkDrawAlternateBackColor = new CheckBox();
            DrawNonRelativesTextGray = new CheckBox();
            DrawNonRelativesGray = new CheckBox();
            gbTheme = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            chkUseSystemVisualStyle = new CheckBox();
            chkColorblind = new CheckBox();
            sbOpenThemeFolder = new GitUI.ScriptsEngine.SplitButton();
            cmsOpenThemeFolders = new ContextMenuStrip(components);
            tsmiApplicationFolder = new ToolStripMenuItem();
            tsmiUserFolder = new ToolStripMenuItem();
            _NO_TRANSLATE_cbSelectTheme = new ComboBox();
            lblRestartNeeded = new Label();
            tlpnlMain = new TableLayoutPanel();
            tlpnlMain.SuspendLayout();
            gbRevisionGraph.SuspendLayout();
            tlpnlRevisionGraph.SuspendLayout();
            gbTheme.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            cmsOpenThemeFolders.SuspendLayout();
            SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(gbRevisionGraph, 0, 0);
            tlpnlMain.Controls.Add(gbTheme, 0, 2);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 4;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.Size = new Size(1511, 583);
            tlpnlMain.TabIndex = 0;
            // 
            // gbRevisionGraph
            // 
            gbRevisionGraph.AutoSize = true;
            gbRevisionGraph.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbRevisionGraph.Controls.Add(tlpnlRevisionGraph);
            gbRevisionGraph.Dock = DockStyle.Fill;
            gbRevisionGraph.Location = new Point(3, 3);
            gbRevisionGraph.Name = "gbRevisionGraph";
            gbRevisionGraph.Padding = new Padding(8);
            gbRevisionGraph.Size = new Size(1505, 182);
            gbRevisionGraph.TabIndex = 0;
            gbRevisionGraph.TabStop = false;
            gbRevisionGraph.Text = "Revision graph";
            // 
            // tlpnlRevisionGraph
            // 
            tlpnlRevisionGraph.AutoSize = true;
            tlpnlRevisionGraph.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlRevisionGraph.ColumnCount = 2;
            tlpnlRevisionGraph.ColumnStyles.Add(new ColumnStyle());
            tlpnlRevisionGraph.ColumnStyles.Add(new ColumnStyle());
            tlpnlRevisionGraph.Controls.Add(chkFillRefLabels, 0, 5);
            tlpnlRevisionGraph.Controls.Add(chkHighlightAuthored, 0, 4);
            tlpnlRevisionGraph.Controls.Add(MulticolorBranches, 0, 0);
            tlpnlRevisionGraph.Controls.Add(chkDrawAlternateBackColor, 0, 1);
            tlpnlRevisionGraph.Controls.Add(DrawNonRelativesTextGray, 0, 3);
            tlpnlRevisionGraph.Controls.Add(DrawNonRelativesGray, 0, 2);
            tlpnlRevisionGraph.Dock = DockStyle.Fill;
            tlpnlRevisionGraph.Location = new Point(8, 24);
            tlpnlRevisionGraph.Name = "tlpnlRevisionGraph";
            tlpnlRevisionGraph.RowCount = 6;
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.Size = new Size(1489, 150);
            tlpnlRevisionGraph.TabIndex = 0;
            // 
            // chkFillRefLabels
            // 
            chkFillRefLabels.AutoSize = true;
            chkFillRefLabels.Dock = DockStyle.Fill;
            chkFillRefLabels.Location = new Point(3, 128);
            chkFillRefLabels.Name = "chkFillRefLabels";
            chkFillRefLabels.Size = new Size(183, 19);
            chkFillRefLabels.TabIndex = 6;
            chkFillRefLabels.Text = "Fill git ref labels";
            chkFillRefLabels.UseVisualStyleBackColor = true;
            // 
            // chkHighlightAuthored
            // 
            chkHighlightAuthored.AutoSize = true;
            chkHighlightAuthored.Dock = DockStyle.Fill;
            chkHighlightAuthored.Location = new Point(3, 103);
            chkHighlightAuthored.Name = "chkHighlightAuthored";
            chkHighlightAuthored.Size = new Size(183, 19);
            chkHighlightAuthored.TabIndex = 5;
            chkHighlightAuthored.Text = "Highlight authored revisions";
            chkHighlightAuthored.UseVisualStyleBackColor = true;
            // 
            // MulticolorBranches
            // 
            MulticolorBranches.AutoSize = true;
            MulticolorBranches.Dock = DockStyle.Fill;
            MulticolorBranches.Location = new Point(3, 3);
            MulticolorBranches.Name = "MulticolorBranches";
            MulticolorBranches.Size = new Size(183, 19);
            MulticolorBranches.TabIndex = 0;
            MulticolorBranches.Text = "Multicolor branches";
            MulticolorBranches.UseVisualStyleBackColor = true;
            // 
            // chkDrawAlternateBackColor
            // 
            chkDrawAlternateBackColor.AutoSize = true;
            chkDrawAlternateBackColor.Dock = DockStyle.Fill;
            chkDrawAlternateBackColor.Location = new Point(3, 28);
            chkDrawAlternateBackColor.Name = "chkDrawAlternateBackColor";
            chkDrawAlternateBackColor.Size = new Size(183, 19);
            chkDrawAlternateBackColor.TabIndex = 2;
            chkDrawAlternateBackColor.Text = "Draw alternate background";
            chkDrawAlternateBackColor.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesTextGray
            // 
            DrawNonRelativesTextGray.AutoSize = true;
            DrawNonRelativesTextGray.Dock = DockStyle.Fill;
            DrawNonRelativesTextGray.Location = new Point(3, 78);
            DrawNonRelativesTextGray.Name = "DrawNonRelativesTextGray";
            DrawNonRelativesTextGray.Size = new Size(183, 19);
            DrawNonRelativesTextGray.TabIndex = 4;
            DrawNonRelativesTextGray.Text = "Draw non relatives text gray";
            DrawNonRelativesTextGray.UseVisualStyleBackColor = true;
            // 
            // DrawNonRelativesGray
            // 
            DrawNonRelativesGray.AutoSize = true;
            DrawNonRelativesGray.Dock = DockStyle.Fill;
            DrawNonRelativesGray.Location = new Point(3, 53);
            DrawNonRelativesGray.Name = "DrawNonRelativesGray";
            DrawNonRelativesGray.Size = new Size(183, 19);
            DrawNonRelativesGray.TabIndex = 3;
            DrawNonRelativesGray.Text = "Draw non relatives graph gray";
            DrawNonRelativesGray.UseVisualStyleBackColor = true;
            // 
            // gbTheme
            // 
            gbTheme.AutoSize = true;
            gbTheme.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbTheme.Controls.Add(tableLayoutPanel1);
            gbTheme.Dock = DockStyle.Fill;
            gbTheme.Location = new Point(3, 191);
            gbTheme.Name = "gbTheme";
            gbTheme.Padding = new Padding(8);
            gbTheme.Size = new Size(1505, 129);
            gbTheme.TabIndex = 3;
            gbTheme.TabStop = false;
            gbTheme.Text = "Theme";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(chkUseSystemVisualStyle, 0, 3);
            tableLayoutPanel1.Controls.Add(chkColorblind, 0, 2);
            tableLayoutPanel1.Controls.Add(sbOpenThemeFolder, 1, 1);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_cbSelectTheme, 0, 1);
            tableLayoutPanel1.Controls.Add(lblRestartNeeded, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(8, 24);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1489, 97);
            tableLayoutPanel1.TabIndex = 9;
            // 
            // chkUseSystemVisualStyle
            // 
            chkUseSystemVisualStyle.Anchor = AnchorStyles.Left;
            chkUseSystemVisualStyle.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(chkUseSystemVisualStyle, 2);
            chkUseSystemVisualStyle.Location = new Point(3, 75);
            chkUseSystemVisualStyle.Name = "chkUseSystemVisualStyle";
            chkUseSystemVisualStyle.Size = new Size(339, 19);
            chkUseSystemVisualStyle.TabIndex = 4;
            chkUseSystemVisualStyle.Text = "Use system-defined visual style (looks bad with dark colors)";
            chkUseSystemVisualStyle.UseVisualStyleBackColor = true;
            // 
            // chkColorblind
            // 
            chkColorblind.Anchor = AnchorStyles.Left;
            chkColorblind.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(chkColorblind, 2);
            chkColorblind.Location = new Point(3, 50);
            chkColorblind.Name = "chkColorblind";
            chkColorblind.Size = new Size(82, 19);
            chkColorblind.TabIndex = 6;
            chkColorblind.Text = "Colorblind";
            chkColorblind.UseVisualStyleBackColor = true;
            // 
            // sbOpenThemeFolder
            // 
            sbOpenThemeFolder.AutoSize = true;
            sbOpenThemeFolder.ContextMenuStrip = cmsOpenThemeFolders;
            sbOpenThemeFolder.Image = Properties.Images.BrowseFileExplorer;
            sbOpenThemeFolder.ImageAlign = ContentAlignment.MiddleLeft;
            sbOpenThemeFolder.Location = new Point(222, 19);
            sbOpenThemeFolder.Name = "sbOpenThemeFolder";
            sbOpenThemeFolder.Padding = new Padding(40, 0, 0, 0);
            sbOpenThemeFolder.Size = new Size(175, 25);
            sbOpenThemeFolder.SplitMenuStrip = cmsOpenThemeFolders;
            sbOpenThemeFolder.TabIndex = 7;
            sbOpenThemeFolder.Text = "Open theme folder";
            sbOpenThemeFolder.UseVisualStyleBackColor = true;
            sbOpenThemeFolder.WholeButtonDropdown = true;
            // 
            // cmsOpenThemeFolders
            // 
            cmsOpenThemeFolders.Items.AddRange(new ToolStripItem[] {
            tsmiApplicationFolder,
            tsmiUserFolder});
            cmsOpenThemeFolders.Name = "cmsOpenThemeFolders";
            cmsOpenThemeFolders.Size = new Size(170, 48);
            // 
            // tsmiApplicationFolder
            // 
            tsmiApplicationFolder.Name = "tsmiApplicationFolder";
            tsmiApplicationFolder.Size = new Size(169, 22);
            tsmiApplicationFolder.Text = "Application folder";
            tsmiApplicationFolder.Click += tsmiApplicationFolder_Click;
            // 
            // tsmiUserFolder
            // 
            tsmiUserFolder.Name = "tsmiUserFolder";
            tsmiUserFolder.Size = new Size(169, 22);
            tsmiUserFolder.Text = "User folder";
            tsmiUserFolder.Click += tsmiUserFolder_Click;
            // 
            // _NO_TRANSLATE_cbSelectTheme
            // 
            _NO_TRANSLATE_cbSelectTheme.Anchor = AnchorStyles.Left;
            _NO_TRANSLATE_cbSelectTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_cbSelectTheme.FormattingEnabled = true;
            _NO_TRANSLATE_cbSelectTheme.Location = new Point(3, 20);
            _NO_TRANSLATE_cbSelectTheme.Name = "_NO_TRANSLATE_cbSelectTheme";
            _NO_TRANSLATE_cbSelectTheme.Size = new Size(213, 23);
            _NO_TRANSLATE_cbSelectTheme.TabIndex = 0;
            // 
            // lblRestartNeeded
            // 
            lblRestartNeeded.Anchor = AnchorStyles.Left;
            tableLayoutPanel1.SetColumnSpan(lblRestartNeeded, 2);
            lblRestartNeeded.Image = Properties.Images.Warning;
            lblRestartNeeded.ImageAlign = ContentAlignment.MiddleLeft;
            lblRestartNeeded.Location = new Point(3, 0);
            lblRestartNeeded.Name = "lblRestartNeeded";
            lblRestartNeeded.Size = new Size(200, 16);
            lblRestartNeeded.TabIndex = 5;
            lblRestartNeeded.Text = "Restart required to apply changes";
            lblRestartNeeded.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ColorsSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tlpnlMain);
            Name = "ColorsSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1527, 599);
            Text = "Colors";
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            gbRevisionGraph.ResumeLayout(false);
            gbRevisionGraph.PerformLayout();
            tlpnlRevisionGraph.ResumeLayout(false);
            tlpnlRevisionGraph.PerformLayout();
            gbTheme.ResumeLayout(false);
            gbTheme.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            cmsOpenThemeFolders.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox gbRevisionGraph;
        private CheckBox DrawNonRelativesTextGray;
        private CheckBox DrawNonRelativesGray;
        private CheckBox MulticolorBranches;
        private CheckBox chkDrawAlternateBackColor;
        private TableLayoutPanel tlpnlRevisionGraph;
        private CheckBox chkHighlightAuthored;
        private GroupBox gbTheme;
        private ContextMenuStrip cmsOpenThemeFolders;
        private ToolStripMenuItem tsmiApplicationFolder;
        private ToolStripMenuItem tsmiUserFolder;
        private TableLayoutPanel tableLayoutPanel1;
        private CheckBox chkUseSystemVisualStyle;
        private CheckBox chkColorblind;
        private GitUI.ScriptsEngine.SplitButton sbOpenThemeFolder;
        private ComboBox _NO_TRANSLATE_cbSelectTheme;
        private Label lblRestartNeeded;
        private CheckBox chkFillRefLabels;
    }
}
