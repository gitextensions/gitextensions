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
            components = new System.ComponentModel.Container();
            contextMenu = new ContextMenuStrip(components);
            copyToolStripMenuItem = new ToolStripMenuItem();
            stageSelectedLinesToolStripMenuItem = new ToolStripMenuItem();
            unstageSelectedLinesToolStripMenuItem = new ToolStripMenuItem();
            resetSelectedLinesToolStripMenuItem = new ToolStripMenuItem();
            copyPatchToolStripMenuItem = new ToolStripMenuItem();
            copyNewVersionToolStripMenuItem = new ToolStripMenuItem();
            copyOldVersionToolStripMenuItem = new ToolStripMenuItem();
            findToolStripMenuItem = new ToolStripMenuItem();
            replaceToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            ignoreWhitespaceAtEolToolStripMenuItem = new ToolStripMenuItem();
            ignoreWhitespaceChangesToolStripMenuItem = new ToolStripMenuItem();
            ignoreAllWhitespaceChangesToolStripMenuItem = new ToolStripMenuItem();
            increaseNumberOfLinesToolStripMenuItem = new ToolStripMenuItem();
            decreaseNumberOfLinesToolStripMenuItem = new ToolStripMenuItem();
            showEntireFileToolStripMenuItem = new ToolStripMenuItem();
            showSyntaxHighlightingToolStripMenuItem = new ToolStripMenuItem();
            diffAppearanceToolStripMenuItem = new ToolStripMenuItem();
            showPatchToolStripMenuItem = new ToolStripMenuItem();
            showGitWordColoringToolStripMenuItem = new ToolStripMenuItem();
            showDifftasticToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            treatAllFilesAsTextToolStripMenuItem = new ToolStripMenuItem();
            automaticContinuousScrollToolStripMenuItem = new ToolStripMenuItem();
            showNonprintableCharactersToolStripMenuItem = new ToolStripMenuItem();
            goToLineToolStripMenuItem = new ToolStripMenuItem();
            fileviewerToolbar = new GitUI.ToolStripEx();
            nextChangeButton = new ToolStripButton();
            previousChangeButton = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            increaseNumberOfLines = new ToolStripButton();
            decreaseNumberOfLines = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            showEntireFileButton = new ToolStripButton();
            showNonPrintChars = new ToolStripButton();
            ignoreWhitespaceAtEol = new ToolStripButton();
            ignoreWhiteSpaces = new ToolStripButton();
            settingsButton = new ToolStripButton();
            encodingToolStripComboBox = new ToolStripComboBox();
            ignoreAllWhitespaces = new ToolStripButton();
            PictureBox = new PictureBox();
            _NO_TRANSLATE_lblShowPreview = new LinkLabel();
            internalFileViewer = new GitUI.Editor.FileViewerInternal();
            showSyntaxHighlighting = new ToolStripButton();
            contextMenu.SuspendLayout();
            fileviewerToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(PictureBox)).BeginInit();
            SuspendLayout();
            // 
            // contextMenu
            // 
            contextMenu.Items.AddRange(new ToolStripItem[] {
            stageSelectedLinesToolStripMenuItem,
            unstageSelectedLinesToolStripMenuItem,
            resetSelectedLinesToolStripMenuItem,
            copyToolStripMenuItem,
            copyPatchToolStripMenuItem,
            copyNewVersionToolStripMenuItem,
            copyOldVersionToolStripMenuItem,
            toolStripSeparator1,
            increaseNumberOfLinesToolStripMenuItem,
            decreaseNumberOfLinesToolStripMenuItem,
            showEntireFileToolStripMenuItem,
            showNonprintableCharactersToolStripMenuItem,
            showSyntaxHighlightingToolStripMenuItem,
            ignoreWhitespaceAtEolToolStripMenuItem,
            ignoreWhitespaceChangesToolStripMenuItem,
            ignoreAllWhitespaceChangesToolStripMenuItem,
            diffAppearanceToolStripMenuItem,
            toolStripSeparator2,
            treatAllFilesAsTextToolStripMenuItem,
            automaticContinuousScrollToolStripMenuItem,
            findToolStripMenuItem,
            replaceToolStripMenuItem,
            goToLineToolStripMenuItem});
            contextMenu.Name = "ContextMenu";
            contextMenu.Size = new Size(244, 346);
            // 
            // stageSelectedLinesToolStripMenuItem
            // 
            stageSelectedLinesToolStripMenuItem.Image = Properties.Images.Stage;
            stageSelectedLinesToolStripMenuItem.Name = "stageSelectedLinesToolStripMenuItem";
            stageSelectedLinesToolStripMenuItem.Size = new Size(243, 22);
            stageSelectedLinesToolStripMenuItem.Text = TranslatedStrings.StageSelectedLines;
            stageSelectedLinesToolStripMenuItem.Click += stageSelectedLinesToolStripMenuItem_Click;
            // 
            // unstageSelectedLinesToolStripMenuItem
            // 
            unstageSelectedLinesToolStripMenuItem.Image = Properties.Images.Unstage;
            unstageSelectedLinesToolStripMenuItem.Name = "chunstageSelectedLinesToolStripMenuItemerrypickSelectedLinesToolStripMenuItem";
            unstageSelectedLinesToolStripMenuItem.Size = new Size(243, 22);
            unstageSelectedLinesToolStripMenuItem.Text = TranslatedStrings.UnstageSelectedLines;
            unstageSelectedLinesToolStripMenuItem.Click += unstageSelectedLinesToolStripMenuItem_Click;
            // 
            // resetSelectedLinesToolStripMenuItem
            // 
            resetSelectedLinesToolStripMenuItem.Image = Properties.Images.ResetWorkingDirChanges;
            resetSelectedLinesToolStripMenuItem.Name = "resetSelectedLinesToolStripMenuItem";
            resetSelectedLinesToolStripMenuItem.Size = new Size(243, 22);
            resetSelectedLinesToolStripMenuItem.Text = TranslatedStrings.ResetSelectedLines;
            resetSelectedLinesToolStripMenuItem.Click += resetSelectedLinesToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = ((Keys)((Keys.Control | Keys.C)));
            copyToolStripMenuItem.Size = new Size(243, 22);
            copyToolStripMenuItem.Text = "&Copy";
            copyToolStripMenuItem.Click += CopyToolStripMenuItemClick;
            // 
            // copyPatchToolStripMenuItem
            // 
            copyPatchToolStripMenuItem.Name = "copyPatchToolStripMenuItem";
            copyPatchToolStripMenuItem.Size = new Size(243, 22);
            copyPatchToolStripMenuItem.Text = "Copy &patch";
            copyPatchToolStripMenuItem.Click += CopyPatchToolStripMenuItemClick;
            // 
            // copyNewVersionToolStripMenuItem
            // 
            copyNewVersionToolStripMenuItem.Name = "copyNewVersionToolStripMenuItem";
            copyNewVersionToolStripMenuItem.Size = new Size(243, 22);
            copyNewVersionToolStripMenuItem.Text = "Copy &new version";
            copyNewVersionToolStripMenuItem.Click += copyNewVersionToolStripMenuItem_Click;
            // 
            // copyOldVersionToolStripMenuItem
            // 
            copyOldVersionToolStripMenuItem.Name = "copyOldVersionToolStripMenuItem";
            copyOldVersionToolStripMenuItem.Size = new Size(243, 22);
            copyOldVersionToolStripMenuItem.Text = "Copy &old version";
            copyOldVersionToolStripMenuItem.Click += copyOldVersionToolStripMenuItem_Click;
            // 
            // findToolStripMenuItem
            // 
            findToolStripMenuItem.Image = Properties.Images.Preview;
            findToolStripMenuItem.Name = "findToolStripMenuItem";
            findToolStripMenuItem.Size = new Size(243, 22);
            findToolStripMenuItem.Text = "&Find...";
            findToolStripMenuItem.Click += FindToolStripMenuItemClick;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new Size(243, 22);
            replaceToolStripMenuItem.Text = "&Replace...";
            replaceToolStripMenuItem.Click += FindToolStripMenuItemClick;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(240, 6);
            // 
            // ignoreWhitespaceAtEolToolStripMenuItem
            // 
            ignoreWhitespaceAtEolToolStripMenuItem.Name = "ignoreWhitespaceAtEolToolStripMenuItem";
            ignoreWhitespaceAtEolToolStripMenuItem.Size = new Size(243, 22);
            ignoreWhitespaceAtEolToolStripMenuItem.Text = "Ignore whitespace changes at end of &line";
            ignoreWhitespaceAtEolToolStripMenuItem.Click += IgnoreWhitespaceAtEolToolStripMenuItem_Click;
            // 
            // ignoreWhitespaceChangesToolStripMenuItem
            // 
            ignoreWhitespaceChangesToolStripMenuItem.Name = "ignoreWhitespaceChangesToolStripMenuItem";
            ignoreWhitespaceChangesToolStripMenuItem.Size = new Size(243, 22);
            ignoreWhitespaceChangesToolStripMenuItem.Text = "Ignore changes in &amount of whitespace";
            ignoreWhitespaceChangesToolStripMenuItem.Click += IgnoreWhitespaceChangesToolStripMenuItemClick;
            // 
            // ignoreAllWhitespaceChangesToolStripMenuItem
            // 
            ignoreAllWhitespaceChangesToolStripMenuItem.Name = "ignoreAllWhitespaceChangesToolStripMenuItem";
            ignoreAllWhitespaceChangesToolStripMenuItem.Size = new Size(243, 22);
            ignoreAllWhitespaceChangesToolStripMenuItem.Text = "Ignore all &whitespace changes";
            ignoreAllWhitespaceChangesToolStripMenuItem.Click += IgnoreAllWhitespaceChangesToolStripMenuItem_Click;
            // 
            // increaseNumberOfLinesToolStripMenuItem
            // 
            increaseNumberOfLinesToolStripMenuItem.Image = Properties.Images.NumberOfLinesIncrease;
            increaseNumberOfLinesToolStripMenuItem.Name = "increaseNumberOfLinesToolStripMenuItem";
            increaseNumberOfLinesToolStripMenuItem.Size = new Size(243, 22);
            increaseNumberOfLinesToolStripMenuItem.Text = "&Increase the number of lines of context";
            increaseNumberOfLinesToolStripMenuItem.Click += IncreaseNumberOfLinesToolStripMenuItemClick;
            // 
            // descreaseNumberOfLinesToolStripMenuItem
            // 
            decreaseNumberOfLinesToolStripMenuItem.Image = Properties.Images.NumberOfLinesDecrease;
            decreaseNumberOfLinesToolStripMenuItem.Name = "decreaseNumberOfLinesToolStripMenuItem";
            decreaseNumberOfLinesToolStripMenuItem.Size = new Size(243, 22);
            decreaseNumberOfLinesToolStripMenuItem.Text = "&Decrease the number of lines of context";
            decreaseNumberOfLinesToolStripMenuItem.Click += DecreaseNumberOfLinesToolStripMenuItemClick;
            // 
            // showEntireFileToolStripMenuItem
            // 
            showEntireFileToolStripMenuItem.Image = Properties.Images.ShowEntireFile;
            showEntireFileToolStripMenuItem.Name = "showEntireFileToolStripMenuItem";
            showEntireFileToolStripMenuItem.Size = new Size(243, 22);
            showEntireFileToolStripMenuItem.Text = "Show &entire file";
            showEntireFileToolStripMenuItem.Click += ShowEntireFileToolStripMenuItemClick;
            // 
            // showSyntaxHighlightingToolStripMenuItem
            // 
            showSyntaxHighlightingToolStripMenuItem.Image = Properties.Resources.SyntaxHighlighting;
            showSyntaxHighlightingToolStripMenuItem.ImageTransparentColor = Color.Magenta;
            showSyntaxHighlightingToolStripMenuItem.Name = "showSyntaxHighlightingToolStripMenuItem";
            showSyntaxHighlightingToolStripMenuItem.Size = new Size(243, 22);
            showSyntaxHighlightingToolStripMenuItem.Text = "Show synta&x highlighting";
            showSyntaxHighlightingToolStripMenuItem.Click += ShowSyntaxHighlighting_Click;
            // 
            // diffAppearanceToolStripMenuItem
            // 
            diffAppearanceToolStripMenuItem.Image = Properties.Images.Diff;
            diffAppearanceToolStripMenuItem.Name = "diffAppearanceToolStripMenuItem";
            diffAppearanceToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                showPatchToolStripMenuItem,
                showGitWordColoringToolStripMenuItem,
                showDifftasticToolStripMenuItem,
            });
            diffAppearanceToolStripMenuItem.Size = new Size(243, 22);
            diffAppearanceToolStripMenuItem.Text = "Diff appea&rance";
            diffAppearanceToolStripMenuItem.Click += ResetPatchAppearanceToolStripMenuItemClick;
            // 
            // showPatchToolStripMenuItem
            // 
            showPatchToolStripMenuItem.Image = Properties.Images.Diff;
            showPatchToolStripMenuItem.Name = "showPatchToolStripMenuItem";
            showPatchToolStripMenuItem.Size = new Size(243, 22);
            showPatchToolStripMenuItem.Text = "&Patch";
            showPatchToolStripMenuItem.Click += ToggleGitWordColoringToolStripMenuItemClick;
            // 
            // showGitWordColoringToolStripMenuItem
            // 
            showGitWordColoringToolStripMenuItem.Image = Properties.Images.EditColor;
            showGitWordColoringToolStripMenuItem.Name = "showGitWordColoringToolStripMenuItem";
            showGitWordColoringToolStripMenuItem.Size = new Size(243, 22);
            showGitWordColoringToolStripMenuItem.Text = "Git wor&d diff";
            showGitWordColoringToolStripMenuItem.Click += ToggleGitWordColoringToolStripMenuItemClick;
            // 
            // showDifftasticToolStripMenuItem
            // 
            showDifftasticToolStripMenuItem.Image = Properties.Images.Difftastic;
            showDifftasticToolStripMenuItem.Name = "showDifftasticToolStripMenuItem";
            showDifftasticToolStripMenuItem.Size = new Size(243, 22);
            showDifftasticToolStripMenuItem.Text = "Diff&tastic";
            showDifftasticToolStripMenuItem.Click += ToggleDifftasticToolStripMenuItemClick;

            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(240, 6);
            // 
            // treatAllFilesAsTextToolStripMenuItem
            // 
            treatAllFilesAsTextToolStripMenuItem.Name = "treatAllFilesAsTextToolStripMenuItem";
            treatAllFilesAsTextToolStripMenuItem.Size = new Size(243, 22);
            treatAllFilesAsTextToolStripMenuItem.Text = "&Treat all files as text";
            treatAllFilesAsTextToolStripMenuItem.Click += TreatAllFilesAsTextToolStripMenuItemClick;
            // 
            // automaticContinuousScrollToolStripMenuItem
            // 
            automaticContinuousScrollToolStripMenuItem.Image = Properties.Images.UiScrollBar;
            automaticContinuousScrollToolStripMenuItem.Name = "automaticContinuousScrollToolStripMenuItem";
            automaticContinuousScrollToolStripMenuItem.Size = new Size(243, 22);
            automaticContinuousScrollToolStripMenuItem.Click += ContinuousScrollToolStripMenuItemClick;
            // 
            // showNonprintableCharactersToolStripMenuItem
            // 
            showNonprintableCharactersToolStripMenuItem.Image = Properties.Images.ShowWhitespace;
            showNonprintableCharactersToolStripMenuItem.Name = "showNonprintableCharactersToolStripMenuItem";
            showNonprintableCharactersToolStripMenuItem.Size = new Size(243, 22);
            showNonprintableCharactersToolStripMenuItem.Text = "S&how nonprinting characters";
            showNonprintableCharactersToolStripMenuItem.Click += ShowNonprintableCharactersToolStripMenuItemClick;
            // 
            // goToLineToolStripMenuItem
            // 
            goToLineToolStripMenuItem.Name = "goToLineToolStripMenuItem";
            goToLineToolStripMenuItem.Size = new Size(243, 22);
            goToLineToolStripMenuItem.Text = "&Go to line";
            goToLineToolStripMenuItem.Click += goToLineToolStripMenuItem_Click;
            // 
            // fileviewerToolbar
            // 
            fileviewerToolbar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            fileviewerToolbar.BackColor = SystemColors.Control;
            fileviewerToolbar.ClickThrough = true;
            fileviewerToolbar.Dock = DockStyle.None;
            fileviewerToolbar.DrawBorder = false;
            fileviewerToolbar.Items.AddRange(new ToolStripItem[] {
            nextChangeButton,
            previousChangeButton,
            toolStripSeparator3,
            increaseNumberOfLines,
            decreaseNumberOfLines,
            toolStripSeparator4,
            showEntireFileButton,
            showNonPrintChars,
            showSyntaxHighlighting,
            ignoreWhitespaceAtEol,
            ignoreWhiteSpaces,
            ignoreAllWhitespaces,
            encodingToolStripComboBox,
            settingsButton});
            fileviewerToolbar.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
            fileviewerToolbar.Location = new Point(458, 0);
            fileviewerToolbar.Name = "fileviewerToolbar";
            fileviewerToolbar.Size = new Size(393, 23);
            fileviewerToolbar.Visible = false;
            fileviewerToolbar.TabIndex = 0;
            fileviewerToolbar.VisibleChanged += fileviewerToolbar_VisibleChanged;
            // 
            // nextChangeButton
            // 
            nextChangeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            nextChangeButton.Image = Properties.Images.ArrowDown;
            nextChangeButton.ImageTransparentColor = Color.Magenta;
            nextChangeButton.Name = "nextChangeButton";
            nextChangeButton.Size = new Size(23, 20);
            nextChangeButton.ToolTipText = "Next change";
            nextChangeButton.Click += NextChangeButtonClick;
            // 
            // previousChangeButton
            // 
            previousChangeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            previousChangeButton.Image = Properties.Images.ArrowUp;
            previousChangeButton.ImageTransparentColor = Color.Magenta;
            previousChangeButton.Name = "previousChangeButton";
            previousChangeButton.Size = new Size(23, 20);
            previousChangeButton.ToolTipText = "Previous change";
            previousChangeButton.Click += PreviousChangeButtonClick;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 23);
            // 
            // increaseNumberOfLines
            // 
            increaseNumberOfLines.DisplayStyle = ToolStripItemDisplayStyle.Image;
            increaseNumberOfLines.Image = Properties.Images.NumberOfLinesIncrease;
            increaseNumberOfLines.ImageTransparentColor = Color.Magenta;
            increaseNumberOfLines.Name = "increaseNumberOfLines";
            increaseNumberOfLines.Size = new Size(23, 20);
            increaseNumberOfLines.ToolTipText = "Increase the number of lines of context";
            increaseNumberOfLines.Click += IncreaseNumberOfLinesToolStripMenuItemClick;
            // 
            // DecreaseNumberOfLines
            // 
            decreaseNumberOfLines.DisplayStyle = ToolStripItemDisplayStyle.Image;
            decreaseNumberOfLines.Image = Properties.Images.NumberOfLinesDecrease;
            decreaseNumberOfLines.ImageTransparentColor = Color.Magenta;
            decreaseNumberOfLines.Name = "decreaseNumberOfLines";
            decreaseNumberOfLines.Size = new Size(23, 20);
            decreaseNumberOfLines.ToolTipText = "Decrease the number of lines of context";
            decreaseNumberOfLines.Click += DecreaseNumberOfLinesToolStripMenuItemClick;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 23);
            // 
            // showEntireFileButton
            // 
            showEntireFileButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            showEntireFileButton.Image = Properties.Images.ShowEntireFile;
            showEntireFileButton.ImageTransparentColor = Color.Magenta;
            showEntireFileButton.Name = "showEntireFileButton";
            showEntireFileButton.Size = new Size(23, 20);
            showEntireFileButton.ToolTipText = "Show entire file";
            showEntireFileButton.Click += ShowEntireFileToolStripMenuItemClick;
            // 
            // showNonPrintChars
            // 
            showNonPrintChars.DisplayStyle = ToolStripItemDisplayStyle.Image;
            showNonPrintChars.Image = Properties.Images.ShowWhitespace;
            showNonPrintChars.ImageTransparentColor = Color.Magenta;
            showNonPrintChars.Name = "showNonPrintChars";
            showNonPrintChars.Size = new Size(23, 20);
            showNonPrintChars.ToolTipText = "Show nonprinting characters";
            showNonPrintChars.Click += ShowNonprintableCharactersToolStripMenuItemClick;
            // 
            // ignoreWhitespaceAtEol
            // 
            ignoreWhitespaceAtEol.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ignoreWhitespaceAtEol.ImageTransparentColor = Color.Magenta;
            ignoreWhitespaceAtEol.Name = "ignoreWhitespaceAtEol";
            ignoreWhitespaceAtEol.Size = new Size(23, 4);
            ignoreWhitespaceAtEol.ToolTipText = "Ignore whitespace changes at end of line";
            ignoreWhitespaceAtEol.Click += IgnoreWhitespaceAtEolToolStripMenuItem_Click;
            // 
            // ignoreWhiteSpaces
            // 
            ignoreWhiteSpaces.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ignoreWhiteSpaces.ImageTransparentColor = Color.Magenta;
            ignoreWhiteSpaces.Name = "ignoreWhiteSpaces";
            ignoreWhiteSpaces.Size = new Size(23, 4);
            ignoreWhiteSpaces.ToolTipText = "Ignore changes in amount of whitespace";
            ignoreWhiteSpaces.Click += IgnoreWhitespaceChangesToolStripMenuItemClick;
            // 
            // encodingToolStripComboBox
            // 
            encodingToolStripComboBox.BackColor = SystemColors.Control;
            encodingToolStripComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            encodingToolStripComboBox.FlatStyle = FlatStyle.Flat;
            encodingToolStripComboBox.Name = "encodingToolStripComboBox";
            encodingToolStripComboBox.Size = new Size(140, 23);
            encodingToolStripComboBox.SelectedIndexChanged += encodingToolStripComboBox_SelectedIndexChanged;
            // 
            // settingsButton
            // 
            settingsButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            settingsButton.Image = Properties.Images.Settings;
            settingsButton.ImageTransparentColor = Color.Magenta;
            settingsButton.Name = "settingsButton";
            settingsButton.Size = new Size(23, 20);
            settingsButton.ToolTipText = "Settings";
            settingsButton.Click += settingsButton_Click;
            // 
            // ignoreAllWhitespaces
            // 
            ignoreAllWhitespaces.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ignoreAllWhitespaces.ImageTransparentColor = Color.Magenta;
            ignoreAllWhitespaces.Name = "ignoreAllWhitespaces";
            ignoreAllWhitespaces.Size = new Size(23, 4);
            ignoreAllWhitespaces.ToolTipText = "Ignore all whitespace changes";
            ignoreAllWhitespaces.Click += IgnoreAllWhitespaceChangesToolStripMenuItem_Click;
            // 
            // PictureBox
            // 
            PictureBox.BackColor = SystemColors.ControlLightLight;
            PictureBox.BackgroundImageLayout = ImageLayout.Center;
            PictureBox.Dock = DockStyle.Fill;
            PictureBox.Location = new Point(0, 0);
            PictureBox.Margin = new Padding(0);
            PictureBox.Name = "PictureBox";
            PictureBox.Size = new Size(757, 518);
            PictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            PictureBox.TabIndex = 7;
            PictureBox.TabStop = false;
            PictureBox.Visible = false;
            //
            // llShowPreview
            //
            _NO_TRANSLATE_lblShowPreview.AutoSize = true;
            _NO_TRANSLATE_lblShowPreview.BackColor = Color.White;
            _NO_TRANSLATE_lblShowPreview.Location = new Point(43, 23);
            _NO_TRANSLATE_lblShowPreview.Name = "_NO_TRANSLATE_lblShowPreview";
            _NO_TRANSLATE_lblShowPreview.Size = new Size(214, 13);
            _NO_TRANSLATE_lblShowPreview.TabIndex = 2;
            _NO_TRANSLATE_lblShowPreview.TabStop = true;
            _NO_TRANSLATE_lblShowPreview.Text = "This file is over 5 MB. Click to show preview";
            _NO_TRANSLATE_lblShowPreview.Visible = false;
            _NO_TRANSLATE_lblShowPreview.LinkClicked += llShowPreview_LinkClicked;
            // 
            // internalFileViewerControl
            // 
            internalFileViewer.Dock = DockStyle.Fill;
            internalFileViewer.IsReadOnly = false;
            internalFileViewer.Location = new Point(0, 40);
            internalFileViewer.Margin = new Padding(0);
            internalFileViewer.Name = "internalFileViewer";
            internalFileViewer.VScrollPosition = 0;
            internalFileViewer.EolMarkerStyle = ICSharpCode.TextEditor.Document.EolMarkerStyle.None;
            internalFileViewer.ShowSpaces = false;
            internalFileViewer.ShowTabs = false;
            internalFileViewer.Size = new Size(757, 518);
            internalFileViewer.TabIndex = 1;
            // 
            // showSyntaxHighlighting
            // 
            showSyntaxHighlighting.DisplayStyle = ToolStripItemDisplayStyle.Image;
            showSyntaxHighlighting.Image = Properties.Resources.SyntaxHighlighting;
            showSyntaxHighlighting.ImageTransparentColor = Color.Magenta;
            showSyntaxHighlighting.Name = "showSyntaxHighlighting";
            showSyntaxHighlighting.Size = new Size(23, 22);
            showSyntaxHighlighting.ToolTipText = "Show syntax highlighting";
            showSyntaxHighlighting.Click += ShowSyntaxHighlighting_Click;
            // 
            // FileViewer
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(_NO_TRANSLATE_lblShowPreview);
            Controls.Add(internalFileViewer);
            Controls.Add(PictureBox);
            Controls.Add(fileviewerToolbar);
            Margin = new Padding(0);
            Name = "FileViewer";
            Size = new Size(757, 518);
            contextMenu.ResumeLayout(false);
            fileviewerToolbar.ResumeLayout(false);
            fileviewerToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(PictureBox)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem findToolStripMenuItem;
        private ToolStripMenuItem replaceToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem ignoreWhitespaceAtEolToolStripMenuItem;
        private ToolStripMenuItem ignoreWhitespaceChangesToolStripMenuItem;
        private ToolStripMenuItem increaseNumberOfLinesToolStripMenuItem;
        private ToolStripMenuItem decreaseNumberOfLinesToolStripMenuItem;
        private ToolStripMenuItem showEntireFileToolStripMenuItem;
        private ToolStripMenuItem showSyntaxHighlightingToolStripMenuItem;
        private ToolStripMenuItem diffAppearanceToolStripMenuItem;
        private ToolStripMenuItem showPatchToolStripMenuItem;
        private ToolStripMenuItem showGitWordColoringToolStripMenuItem;
        private ToolStripMenuItem showDifftasticToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem treatAllFilesAsTextToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem copyPatchToolStripMenuItem;
        private ToolStripEx fileviewerToolbar;
        private FileViewerInternal internalFileViewer;
        private ToolStripButton nextChangeButton;
        private ToolStripButton previousChangeButton;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton increaseNumberOfLines;
        private ToolStripButton decreaseNumberOfLines;
        private ToolStripButton showEntireFileButton;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton showNonPrintChars;
        private ToolStripMenuItem automaticContinuousScrollToolStripMenuItem;
        private ToolStripMenuItem showNonprintableCharactersToolStripMenuItem;
        private ToolStripButton ignoreWhitespaceAtEol;
        private ToolStripButton ignoreWhiteSpaces;
        private ToolStripButton settingsButton;
        private PictureBox PictureBox;
        private ToolStripComboBox encodingToolStripComboBox;
        private ToolStripMenuItem goToLineToolStripMenuItem;
        private ToolStripMenuItem copyNewVersionToolStripMenuItem;
        private ToolStripMenuItem copyOldVersionToolStripMenuItem;
        private ToolStripMenuItem stageSelectedLinesToolStripMenuItem;
        private ToolStripMenuItem unstageSelectedLinesToolStripMenuItem;
        private ToolStripMenuItem resetSelectedLinesToolStripMenuItem;
        private ToolStripButton ignoreAllWhitespaces;
        private ToolStripMenuItem ignoreAllWhitespaceChangesToolStripMenuItem;
        private LinkLabel _NO_TRANSLATE_lblShowPreview;
        private ToolStripButton showSyntaxHighlighting;
    }
}
