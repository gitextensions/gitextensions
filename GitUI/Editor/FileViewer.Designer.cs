using System.Windows.Forms;

namespace GitUI.Editor
{
    partial class FileViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cherrypickSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyNewVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyOldVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ignoreWhitespaceChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoreAllWhitespaceChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.increaseNumberOfLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decreaseNumberOfLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEntireFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.treatAllFilesAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showNonprintableCharactersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileviewerToolbar = new GitUI.ToolStripEx();
            this.nextChangeButton = new System.Windows.Forms.ToolStripButton();
            this.previousChangeButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.increaseNumberOfLines = new System.Windows.Forms.ToolStripButton();
            this.decreaseNumberOfLines = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.showEntireFileButton = new System.Windows.Forms.ToolStripButton();
            this.showNonPrintChars = new System.Windows.Forms.ToolStripButton();
            this.ignoreWhiteSpaces = new System.Windows.Forms.ToolStripButton();
            this.settingsButton = new System.Windows.Forms.ToolStripButton();
            this.encodingToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.ignoreAllWhitespaces = new System.Windows.Forms.ToolStripButton();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.revertSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_lblShowPreview = new System.Windows.Forms.LinkLabel();
            this.internalFileViewer = new GitUI.Editor.FileViewerInternal();
            this.contextMenu.SuspendLayout();
            this.fileviewerToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.cherrypickSelectedLinesToolStripMenuItem,
            this.revertSelectedLinesToolStripMenuItem,
            this.copyPatchToolStripMenuItem,
            this.copyNewVersionToolStripMenuItem,
            this.copyOldVersionToolStripMenuItem,
            this.findToolStripMenuItem,
            this.toolStripSeparator1,
            this.ignoreWhitespaceChangesToolStripMenuItem,
            this.ignoreAllWhitespaceChangesToolStripMenuItem,
            this.increaseNumberOfLinesToolStripMenuItem,
            this.decreaseNumberOfLinesToolStripMenuItem,
            this.showEntireFileToolStripMenuItem,
            this.toolStripSeparator2,
            this.treatAllFilesAsTextToolStripMenuItem,
            this.showNonprintableCharactersToolStripMenuItem,
            this.goToLineToolStripMenuItem});
            this.contextMenu.Name = "ContextMenu";
            this.contextMenu.Size = new System.Drawing.Size(244, 346);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItemClick);
            // 
            // cherrypickSelectedLinesToolStripMenuItem
            // 
            this.cherrypickSelectedLinesToolStripMenuItem.Image = global::GitUI.Properties.Images.CherryPick;
            this.cherrypickSelectedLinesToolStripMenuItem.Name = "cherrypickSelectedLinesToolStripMenuItem";
            this.cherrypickSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.cherrypickSelectedLinesToolStripMenuItem.Text = "Cherry pick selected lines";
            this.cherrypickSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.cherrypickSelectedLinesToolStripMenuItem_Click);
            // 
            // copyPatchToolStripMenuItem
            // 
            this.copyPatchToolStripMenuItem.Name = "copyPatchToolStripMenuItem";
            this.copyPatchToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.copyPatchToolStripMenuItem.Text = "Copy patch";
            this.copyPatchToolStripMenuItem.Click += new System.EventHandler(this.CopyPatchToolStripMenuItemClick);
            // 
            // copyNewVersionToolStripMenuItem
            // 
            this.copyNewVersionToolStripMenuItem.Name = "copyNewVersionToolStripMenuItem";
            this.copyNewVersionToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.copyNewVersionToolStripMenuItem.Text = "Copy new version";
            this.copyNewVersionToolStripMenuItem.Click += new System.EventHandler(this.copyNewVersionToolStripMenuItem_Click);
            // 
            // copyOldVersionToolStripMenuItem
            // 
            this.copyOldVersionToolStripMenuItem.Name = "copyOldVersionToolStripMenuItem";
            this.copyOldVersionToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.copyOldVersionToolStripMenuItem.Text = "Copy old version";
            this.copyOldVersionToolStripMenuItem.Click += new System.EventHandler(this.copyOldVersionToolStripMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.FindToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(240, 6);
            // 
            // ignoreWhitespaceChangesToolStripMenuItem
            // 
            this.ignoreWhitespaceChangesToolStripMenuItem.Name = "ignoreWhitespaceChangesToolStripMenuItem";
            this.ignoreWhitespaceChangesToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.ignoreWhitespaceChangesToolStripMenuItem.Text = "Ignore leading and trailing whitespace changes";
            this.ignoreWhitespaceChangesToolStripMenuItem.Click += new System.EventHandler(this.IgnoreWhitespaceChangesToolStripMenuItemClick);
            // 
            // ignoreAllWhitespaceChangesToolStripMenuItem
            // 
            this.ignoreAllWhitespaceChangesToolStripMenuItem.Name = "ignoreAllWhitespaceChangesToolStripMenuItem";
            this.ignoreAllWhitespaceChangesToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.ignoreAllWhitespaceChangesToolStripMenuItem.Text = "Ignore all whitespace changes";
            this.ignoreAllWhitespaceChangesToolStripMenuItem.Click += new System.EventHandler(this.ignoreAllWhitespaceChangesToolStripMenuItem_Click);
            // 
            // increaseNumberOfLinesToolStripMenuItem
            // 
            this.increaseNumberOfLinesToolStripMenuItem.Image = global::GitUI.Properties.Images.NumberOfLinesIncrease;
            this.increaseNumberOfLinesToolStripMenuItem.Name = "increaseNumberOfLinesToolStripMenuItem";
            this.increaseNumberOfLinesToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.increaseNumberOfLinesToolStripMenuItem.Text = "Increase the number of lines of context";
            this.increaseNumberOfLinesToolStripMenuItem.Click += new System.EventHandler(this.IncreaseNumberOfLinesToolStripMenuItemClick);
            // 
            // descreaseNumberOfLinesToolStripMenuItem
            // 
            this.decreaseNumberOfLinesToolStripMenuItem.Image = global::GitUI.Properties.Images.NumberOfLinesDecrease;
            this.decreaseNumberOfLinesToolStripMenuItem.Name = "decreaseNumberOfLinesToolStripMenuItem";
            this.decreaseNumberOfLinesToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.decreaseNumberOfLinesToolStripMenuItem.Text = "Decrease the number of lines of context";
            this.decreaseNumberOfLinesToolStripMenuItem.Click += new System.EventHandler(this.DecreaseNumberOfLinesToolStripMenuItemClick);
            // 
            // showEntireFileToolStripMenuItem
            // 
            this.showEntireFileToolStripMenuItem.Image = global::GitUI.Properties.Images.ShowEntireFile;
            this.showEntireFileToolStripMenuItem.Name = "showEntireFileToolStripMenuItem";
            this.showEntireFileToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.showEntireFileToolStripMenuItem.Text = "Show entire file";
            this.showEntireFileToolStripMenuItem.Click += new System.EventHandler(this.ShowEntireFileToolStripMenuItemClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(240, 6);
            // 
            // treatAllFilesAsTextToolStripMenuItem
            // 
            this.treatAllFilesAsTextToolStripMenuItem.Name = "treatAllFilesAsTextToolStripMenuItem";
            this.treatAllFilesAsTextToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.treatAllFilesAsTextToolStripMenuItem.Text = "Treat all files as text";
            this.treatAllFilesAsTextToolStripMenuItem.Click += new System.EventHandler(this.TreatAllFilesAsTextToolStripMenuItemClick);
            // 
            // showNonprintableCharactersToolStripMenuItem
            // 
            this.showNonprintableCharactersToolStripMenuItem.Image = global::GitUI.Properties.Images.ShowWhitespace;
            this.showNonprintableCharactersToolStripMenuItem.Name = "showNonprintableCharactersToolStripMenuItem";
            this.showNonprintableCharactersToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.showNonprintableCharactersToolStripMenuItem.Text = "Show nonprinting characters";
            this.showNonprintableCharactersToolStripMenuItem.Click += new System.EventHandler(this.ShowNonprintableCharactersToolStripMenuItemClick);
            // 
            // goToLineToolStripMenuItem
            // 
            this.goToLineToolStripMenuItem.Name = "goToLineToolStripMenuItem";
            this.goToLineToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.goToLineToolStripMenuItem.Text = "Go to line";
            this.goToLineToolStripMenuItem.Click += new System.EventHandler(this.goToLineToolStripMenuItem_Click);
            // 
            // fileviewerToolbar
            // 
            this.fileviewerToolbar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fileviewerToolbar.BackColor = System.Drawing.SystemColors.Control;
            this.fileviewerToolbar.ClickThrough = true;
            this.fileviewerToolbar.Dock = System.Windows.Forms.DockStyle.None;
            this.fileviewerToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nextChangeButton,
            this.previousChangeButton,
            this.toolStripSeparator3,
            this.increaseNumberOfLines,
            this.decreaseNumberOfLines,
            this.toolStripSeparator4,
            this.showEntireFileButton,
            this.showNonPrintChars,
            this.ignoreWhiteSpaces,
            this.ignoreAllWhitespaces,
            this.encodingToolStripComboBox,
            this.settingsButton});
            this.fileviewerToolbar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.StackWithOverflow;
            this.fileviewerToolbar.Location = new System.Drawing.Point(458, 0);
            this.fileviewerToolbar.Name = "fileviewerToolbar";
            this.fileviewerToolbar.Size = new System.Drawing.Size(393, 23);
            this.fileviewerToolbar.Visible = false;
            this.fileviewerToolbar.TabIndex = 0;
            this.fileviewerToolbar.VisibleChanged += new System.EventHandler(this.fileviewerToolbar_VisibleChanged);
            // 
            // nextChangeButton
            // 
            this.nextChangeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nextChangeButton.Image = global::GitUI.Properties.Images.ArrowDown;
            this.nextChangeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nextChangeButton.Name = "nextChangeButton";
            this.nextChangeButton.Size = new System.Drawing.Size(23, 20);
            this.nextChangeButton.ToolTipText = "Next change";
            this.nextChangeButton.Click += new System.EventHandler(this.NextChangeButtonClick);
            // 
            // previousChangeButton
            // 
            this.previousChangeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.previousChangeButton.Image = global::GitUI.Properties.Images.ArrowUp;
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
            this.increaseNumberOfLines.Image = global::GitUI.Properties.Images.NumberOfLinesIncrease;
            this.increaseNumberOfLines.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.increaseNumberOfLines.Name = "increaseNumberOfLines";
            this.increaseNumberOfLines.Size = new System.Drawing.Size(23, 20);
            this.increaseNumberOfLines.ToolTipText = "Increase the number of lines of context";
            this.increaseNumberOfLines.Click += new System.EventHandler(this.IncreaseNumberOfLinesToolStripMenuItemClick);
            // 
            // DecreaseNumberOfLines
            // 
            this.decreaseNumberOfLines.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.decreaseNumberOfLines.Image = global::GitUI.Properties.Images.NumberOfLinesDecrease;
            this.decreaseNumberOfLines.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.decreaseNumberOfLines.Name = "decreaseNumberOfLines";
            this.decreaseNumberOfLines.Size = new System.Drawing.Size(23, 20);
            this.decreaseNumberOfLines.ToolTipText = "Decrease the number of lines of context";
            this.decreaseNumberOfLines.Click += new System.EventHandler(this.DecreaseNumberOfLinesToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 23);
            // 
            // showEntireFileButton
            // 
            this.showEntireFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showEntireFileButton.Image = global::GitUI.Properties.Images.ShowEntireFile;
            this.showEntireFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showEntireFileButton.Name = "showEntireFileButton";
            this.showEntireFileButton.Size = new System.Drawing.Size(23, 20);
            this.showEntireFileButton.ToolTipText = "Show entire file";
            this.showEntireFileButton.Click += new System.EventHandler(this.ShowEntireFileToolStripMenuItemClick);
            // 
            // showNonPrintChars
            // 
            this.showNonPrintChars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showNonPrintChars.Image = global::GitUI.Properties.Images.ShowWhitespace;
            this.showNonPrintChars.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showNonPrintChars.Name = "showNonPrintChars";
            this.showNonPrintChars.Size = new System.Drawing.Size(23, 20);
            this.showNonPrintChars.ToolTipText = "Show nonprinting characters";
            this.showNonPrintChars.Click += new System.EventHandler(this.ShowNonprintableCharactersToolStripMenuItemClick);
            // 
            // ignoreWhiteSpaces
            // 
            this.ignoreWhiteSpaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ignoreWhiteSpaces.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ignoreWhiteSpaces.Name = "ignoreWhiteSpaces";
            this.ignoreWhiteSpaces.Size = new System.Drawing.Size(23, 4);
            this.ignoreWhiteSpaces.ToolTipText = "Ignore leading and trailing whitespace changes";
            this.ignoreWhiteSpaces.Click += new System.EventHandler(this.IgnoreWhitespaceChangesToolStripMenuItemClick);
            // 
            // encodingToolStripComboBox
            // 
            this.encodingToolStripComboBox.BackColor = System.Drawing.SystemColors.Control;
            this.encodingToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encodingToolStripComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.encodingToolStripComboBox.Name = "encodingToolStripComboBox";
            this.encodingToolStripComboBox.Size = new System.Drawing.Size(140, 23);
            this.encodingToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.encodingToolStripComboBox_SelectedIndexChanged);
            // 
            // settingsButton
            // 
            this.settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.settingsButton.Image = global::GitUI.Properties.Images.Settings;
            this.settingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(23, 20);
            this.settingsButton.ToolTipText = "Settings";
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // ignoreAllWhitespaces
            // 
            this.ignoreAllWhitespaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ignoreAllWhitespaces.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ignoreAllWhitespaces.Name = "ignoreAllWhitespaces";
            this.ignoreAllWhitespaces.Size = new System.Drawing.Size(23, 4);
            this.ignoreAllWhitespaces.ToolTipText = "Ignore all whitespace changes";
            this.ignoreAllWhitespaces.Click += new System.EventHandler(this.ignoreAllWhitespaceChangesToolStripMenuItem_Click);
            // 
            // PictureBox
            // 
            this.PictureBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.PictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.PictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PictureBox.Location = new System.Drawing.Point(0, 0);
            this.PictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(757, 518);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox.TabIndex = 7;
            this.PictureBox.TabStop = false;
            this.PictureBox.Visible = false;
            // 
            // resetSelectedLinesToolStripMenuItem
            // 
            this.revertSelectedLinesToolStripMenuItem.Image = global::GitUI.Properties.Images.ResetFileTo;
            this.revertSelectedLinesToolStripMenuItem.Name = "revertSelectedLinesToolStripMenuItem";
            this.revertSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.revertSelectedLinesToolStripMenuItem.Text = "Revert selected lines";
            this.revertSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.revertSelectedLinesToolStripMenuItem_Click);
            //
            // llShowPreview
            //
            this._NO_TRANSLATE_lblShowPreview.AutoSize = true;
            this._NO_TRANSLATE_lblShowPreview.BackColor = System.Drawing.Color.White;
            this._NO_TRANSLATE_lblShowPreview.Location = new System.Drawing.Point(43, 23);
            this._NO_TRANSLATE_lblShowPreview.Name = "_NO_TRANSLATE_lblShowPreview";
            this._NO_TRANSLATE_lblShowPreview.Size = new System.Drawing.Size(214, 13);
            this._NO_TRANSLATE_lblShowPreview.TabIndex = 2;
            this._NO_TRANSLATE_lblShowPreview.TabStop = true;
            this._NO_TRANSLATE_lblShowPreview.Text = "This file is over 5 MB. Click to show preview";
            this._NO_TRANSLATE_lblShowPreview.Visible = false;
            this._NO_TRANSLATE_lblShowPreview.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llShowPreview_LinkClicked);
            // 
            // internalFileViewerControl
            // 
            this.internalFileViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.internalFileViewer.FirstVisibleLine = 0;
            this.internalFileViewer.IsReadOnly = false;
            this.internalFileViewer.Location = new System.Drawing.Point(0, 40);
            this.internalFileViewer.Margin = new System.Windows.Forms.Padding(0);
            this.internalFileViewer.Name = "internalFileViewer";
            this.internalFileViewer.ScrollPos = 0;
            this.internalFileViewer.ShowEOLMarkers = false;
            this.internalFileViewer.ShowLineNumbers = true;
            this.internalFileViewer.ShowSpaces = false;
            this.internalFileViewer.ShowTabs = false;
            this.internalFileViewer.Size = new System.Drawing.Size(757, 518);
            this.internalFileViewer.TabIndex = 1;
            this.internalFileViewer.VRulerPosition = 80;
            // 
            // FileViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this._NO_TRANSLATE_lblShowPreview);
            this.Controls.Add(this.internalFileViewer);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.fileviewerToolbar);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "FileViewer";
            this.Size = new System.Drawing.Size(757, 518);
            this.contextMenu.ResumeLayout(false);
            this.fileviewerToolbar.ResumeLayout(false);
            this.fileviewerToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ignoreWhitespaceChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem increaseNumberOfLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decreaseNumberOfLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEntireFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem treatAllFilesAsTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyPatchToolStripMenuItem;
        private ToolStripEx fileviewerToolbar;
        private FileViewerInternal internalFileViewer;
        private System.Windows.Forms.ToolStripButton nextChangeButton;
        private System.Windows.Forms.ToolStripButton previousChangeButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton increaseNumberOfLines;
        private System.Windows.Forms.ToolStripButton decreaseNumberOfLines;
        private System.Windows.Forms.ToolStripButton showEntireFileButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton showNonPrintChars;
        private System.Windows.Forms.ToolStripMenuItem showNonprintableCharactersToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton ignoreWhiteSpaces;
        private System.Windows.Forms.ToolStripButton settingsButton;
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.ToolStripComboBox encodingToolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem goToLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyNewVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyOldVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cherrypickSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revertSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton ignoreAllWhitespaces;
        private System.Windows.Forms.ToolStripMenuItem ignoreAllWhitespaceChangesToolStripMenuItem;
        private LinkLabel _NO_TRANSLATE_lblShowPreview;
    }
}
