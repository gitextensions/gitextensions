namespace GitUI.Editor
{
    partial class FileViewer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileViewer));
            this.ContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ignoreWhitespaceChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.increaseNumberOfLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.descreaseNumberOfLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEntireFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.treatAllFilesAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showNonprintableCharactersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileviewerToolbar = new ToolStripEx();
            this.nextChangeButton = new System.Windows.Forms.ToolStripButton();
            this.previousChangeButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.increaseNumberOfLines = new System.Windows.Forms.ToolStripButton();
            this.DecreaseNumberOfLines = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.showEntireFileButton = new System.Windows.Forms.ToolStripButton();
            this.showNonPrintChars = new System.Windows.Forms.ToolStripButton();
            this.ignoreWhiteSpaces = new System.Windows.Forms.ToolStripButton();
            this.encodingToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.goToLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMenu.SuspendLayout();
            this.fileviewerToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ContextMenu
            // 
            this.ContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.copyPatchToolStripMenuItem,
            this.toolStripSeparator1,
            this.ignoreWhitespaceChangesToolStripMenuItem,
            this.increaseNumberOfLinesToolStripMenuItem,
            this.descreaseNumberOfLinesToolStripMenuItem,
            this.showEntireFileToolStripMenuItem,
            this.toolStripSeparator2,
            this.treatAllFilesAsTextToolStripMenuItem,
            this.showNonprintableCharactersToolStripMenuItem,
            this.goToLineToolStripMenuItem});
            this.ContextMenu.Name = "ContextMenu";
            this.ContextMenu.Size = new System.Drawing.Size(239, 258);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.FindToolStripMenuItemClick);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItemClick);
            // 
            // copyPatchToolStripMenuItem
            // 
            this.copyPatchToolStripMenuItem.Name = "copyPatchToolStripMenuItem";
            this.copyPatchToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.copyPatchToolStripMenuItem.Text = "Copy patch";
            this.copyPatchToolStripMenuItem.Click += new System.EventHandler(this.CopyPatchToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(235, 6);
            // 
            // ignoreWhitespaceChangesToolStripMenuItem
            // 
            this.ignoreWhitespaceChangesToolStripMenuItem.Name = "ignoreWhitespaceChangesToolStripMenuItem";
            this.ignoreWhitespaceChangesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.ignoreWhitespaceChangesToolStripMenuItem.Text = "Ignore whitespace changes";
            this.ignoreWhitespaceChangesToolStripMenuItem.Click += new System.EventHandler(this.IgnoreWhitespaceChangesToolStripMenuItemClick);
            // 
            // increaseNumberOfLinesToolStripMenuItem
            // 
            this.increaseNumberOfLinesToolStripMenuItem.Name = "increaseNumberOfLinesToolStripMenuItem";
            this.increaseNumberOfLinesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.increaseNumberOfLinesToolStripMenuItem.Text = "Increase number of lines visible";
            this.increaseNumberOfLinesToolStripMenuItem.Click += new System.EventHandler(this.IncreaseNumberOfLinesToolStripMenuItemClick);
            // 
            // descreaseNumberOfLinesToolStripMenuItem
            // 
            this.descreaseNumberOfLinesToolStripMenuItem.Name = "descreaseNumberOfLinesToolStripMenuItem";
            this.descreaseNumberOfLinesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.descreaseNumberOfLinesToolStripMenuItem.Text = "Decrease number of lines visible";
            this.descreaseNumberOfLinesToolStripMenuItem.Click += new System.EventHandler(this.DescreaseNumberOfLinesToolStripMenuItemClick);
            // 
            // showEntireFileToolStripMenuItem
            // 
            this.showEntireFileToolStripMenuItem.Name = "showEntireFileToolStripMenuItem";
            this.showEntireFileToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.showEntireFileToolStripMenuItem.Text = "Show entire file";
            this.showEntireFileToolStripMenuItem.Click += new System.EventHandler(this.ShowEntireFileToolStripMenuItemClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(235, 6);
            // 
            // treatAllFilesAsTextToolStripMenuItem
            // 
            this.treatAllFilesAsTextToolStripMenuItem.Name = "treatAllFilesAsTextToolStripMenuItem";
            this.treatAllFilesAsTextToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.treatAllFilesAsTextToolStripMenuItem.Text = "Treat all files as text";
            this.treatAllFilesAsTextToolStripMenuItem.Click += new System.EventHandler(this.TreatAllFilesAsTextToolStripMenuItemClick);
            // 
            // showNonprintableCharactersToolStripMenuItem
            // 
            this.showNonprintableCharactersToolStripMenuItem.Name = "showNonprintableCharactersToolStripMenuItem";
            this.showNonprintableCharactersToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.showNonprintableCharactersToolStripMenuItem.Text = "Show nonprinting characters";
            this.showNonprintableCharactersToolStripMenuItem.Click += new System.EventHandler(this.ShowNonprintableCharactersToolStripMenuItemClick);
            // 
            // fileviewerToolbar
            // 
            this.fileviewerToolbar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fileviewerToolbar.Dock = System.Windows.Forms.DockStyle.None;
            this.fileviewerToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nextChangeButton,
            this.previousChangeButton,
            this.toolStripSeparator3,
            this.increaseNumberOfLines,
            this.DecreaseNumberOfLines,
            this.toolStripSeparator4,
            this.showEntireFileButton,
            this.showNonPrintChars,
            this.ignoreWhiteSpaces,
            this.encodingToolStripComboBox});
            this.fileviewerToolbar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.fileviewerToolbar.Location = new System.Drawing.Point(328, 0);
            this.fileviewerToolbar.Name = "fileviewerToolbar";
            this.fileviewerToolbar.Size = new System.Drawing.Size(297, 23);
            this.fileviewerToolbar.TabIndex = 4;
            this.fileviewerToolbar.Visible = false;
            this.fileviewerToolbar.VisibleChanged += new System.EventHandler(this.fileviewerToolbar_VisibleChanged);
            // 
            // nextChangeButton
            // 
            this.nextChangeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nextChangeButton.Image = global::GitUI.Properties.Resources.ArrowDown;
            this.nextChangeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nextChangeButton.Name = "nextChangeButton";
            this.nextChangeButton.Size = new System.Drawing.Size(23, 20);
            this.nextChangeButton.ToolTipText = "Next change";
            this.nextChangeButton.Click += new System.EventHandler(this.NextChangeButtonClick);
            // 
            // previousChangeButton
            // 
            this.previousChangeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.previousChangeButton.Image = global::GitUI.Properties.Resources.MoveUp;
            this.previousChangeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.previousChangeButton.Name = "previousChangeButton";
            this.previousChangeButton.Size = new System.Drawing.Size(23, 20);
            this.previousChangeButton.ToolTipText = "Previous change";
            this.previousChangeButton.Click += new System.EventHandler(this.PreviousChangeButtonClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
            // 
            // increaseNumberOfLines
            // 
            this.increaseNumberOfLines.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.increaseNumberOfLines.Image = ((System.Drawing.Image)(resources.GetObject("increaseNumberOfLines.Image")));
            this.increaseNumberOfLines.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.increaseNumberOfLines.Name = "increaseNumberOfLines";
            this.increaseNumberOfLines.Size = new System.Drawing.Size(23, 20);
            this.increaseNumberOfLines.ToolTipText = "Increase number of visible lines";
            this.increaseNumberOfLines.Click += new System.EventHandler(this.IncreaseNumberOfLinesClick);
            // 
            // DecreaseNumberOfLines
            // 
            this.DecreaseNumberOfLines.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DecreaseNumberOfLines.Image = ((System.Drawing.Image)(resources.GetObject("DecreaseNumberOfLines.Image")));
            this.DecreaseNumberOfLines.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DecreaseNumberOfLines.Name = "DecreaseNumberOfLines";
            this.DecreaseNumberOfLines.Size = new System.Drawing.Size(23, 20);
            this.DecreaseNumberOfLines.ToolTipText = "Decrease number of visible lines";
            this.DecreaseNumberOfLines.Click += new System.EventHandler(this.DecreaseNumberOfLinesClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 23);
            // 
            // showEntireFileButton
            // 
            this.showEntireFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showEntireFileButton.Image = ((System.Drawing.Image)(resources.GetObject("showEntireFileButton.Image")));
            this.showEntireFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showEntireFileButton.Name = "showEntireFileButton";
            this.showEntireFileButton.Size = new System.Drawing.Size(23, 20);
            this.showEntireFileButton.ToolTipText = "Show entire file";
            this.showEntireFileButton.Click += new System.EventHandler(this.ShowEntireFileButtonClick);
            // 
            // showNonPrintChars
            // 
            this.showNonPrintChars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showNonPrintChars.Image = global::GitUI.Properties.Resources.nonprintchar;
            this.showNonPrintChars.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showNonPrintChars.Name = "showNonPrintChars";
            this.showNonPrintChars.Size = new System.Drawing.Size(23, 20);
            this.showNonPrintChars.ToolTipText = "Show nonprinting characters";
            this.showNonPrintChars.Click += new System.EventHandler(this.ShowNonPrintCharsClick);
            // 
            // ignoreWhiteSpaces
            // 
            this.ignoreWhiteSpaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ignoreWhiteSpaces.Image = ((System.Drawing.Image)(resources.GetObject("ignoreWhiteSpaces.Image")));
            this.ignoreWhiteSpaces.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ignoreWhiteSpaces.Name = "ignoreWhiteSpaces";
            this.ignoreWhiteSpaces.Size = new System.Drawing.Size(23, 20);
            this.ignoreWhiteSpaces.ToolTipText = "Ignore whitespaces";
            this.ignoreWhiteSpaces.Click += new System.EventHandler(this.ignoreWhiteSpaces_Click);
            // 
            // encodingToolStripComboBox
            // 
            this.encodingToolStripComboBox.Name = "encodingToolStripComboBox";
            this.encodingToolStripComboBox.Size = new System.Drawing.Size(121, 21);
            this.encodingToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.encodingToolStripComboBox_SelectedIndexChanged);
            // 
            // PictureBox
            // 
            this.PictureBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.PictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.PictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PictureBox.Location = new System.Drawing.Point(0, 0);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(649, 449);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox.TabIndex = 7;
            this.PictureBox.TabStop = false;
            this.PictureBox.Visible = false;
            // 
            // goToLineToolStripMenuItem
            // 
            this.goToLineToolStripMenuItem.Name = "goToLineToolStripMenuItem";
            this.goToLineToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.goToLineToolStripMenuItem.Text = "Go to line";
            this.goToLineToolStripMenuItem.Click += new System.EventHandler(this.goToLineToolStripMenuItem_Click);
            // 
            // FileViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.ContextMenu;
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.fileviewerToolbar);
            this.Name = "FileViewer";
            this.Size = new System.Drawing.Size(649, 449);
            this.ContextMenu.ResumeLayout(false);
            this.fileviewerToolbar.ResumeLayout(false);
            this.fileviewerToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private new System.Windows.Forms.ContextMenuStrip ContextMenu;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ignoreWhitespaceChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem increaseNumberOfLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem descreaseNumberOfLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEntireFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem treatAllFilesAsTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyPatchToolStripMenuItem;
        private ToolStripEx fileviewerToolbar;
        private System.Windows.Forms.ToolStripButton nextChangeButton;
        private System.Windows.Forms.ToolStripButton previousChangeButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton increaseNumberOfLines;
        private System.Windows.Forms.ToolStripButton DecreaseNumberOfLines;
        private System.Windows.Forms.ToolStripButton showEntireFileButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton showNonPrintChars;
        private System.Windows.Forms.ToolStripMenuItem showNonprintableCharactersToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton ignoreWhiteSpaces;
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.ToolStripComboBox encodingToolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem goToLineToolStripMenuItem;
    }
}
