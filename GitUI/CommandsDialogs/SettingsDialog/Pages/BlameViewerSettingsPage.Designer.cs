using GitUI.UserControls.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class BlameViewerSettingsPage
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
            groupBoxBlameSettings = new GroupBox();
            tableLayoutPanelBlameSettings = new TableLayoutPanel();
            cbIgnoreWhitespace = new CheckBox();
            cbDetectMoveAndCopyInThisFile = new SettingsCheckBox();
            cbDetectMoveAndCopyInAllFiles = new SettingsCheckBox();
            groupBoxDisplayResult = new GroupBox();
            tableLayoutPanelDisplayResult = new TableLayoutPanel();
            cbDisplayAuthorFirst = new CheckBox();
            cbShowAuthor = new CheckBox();
            cbShowAuthorDate = new CheckBox();
            cbShowAuthorTime = new CheckBox();
            cbShowLineNumbers = new CheckBox();
            cbShowOriginalFilePath = new CheckBox();
            cbShowAuthorAvatar = new CheckBox();
            groupBoxBlameSettings.SuspendLayout();
            tableLayoutPanelBlameSettings.SuspendLayout();
            groupBoxDisplayResult.SuspendLayout();
            tableLayoutPanelDisplayResult.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxBlameSettings
            // 
            groupBoxBlameSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxBlameSettings.AutoSize = true;
            groupBoxBlameSettings.Controls.Add(tableLayoutPanelBlameSettings);
            groupBoxBlameSettings.Location = new Point(11, 11);
            groupBoxBlameSettings.Name = "groupBoxBlameSettings";
            groupBoxBlameSettings.Size = new Size(319, 97);
            groupBoxBlameSettings.TabIndex = 0;
            groupBoxBlameSettings.TabStop = false;
            groupBoxBlameSettings.Text = "Blame settings";
            // 
            // tableLayoutPanelBlameSettings
            // 
            tableLayoutPanelBlameSettings.AutoSize = true;
            tableLayoutPanelBlameSettings.ColumnCount = 1;
            tableLayoutPanelBlameSettings.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelBlameSettings.Controls.Add(cbIgnoreWhitespace, 0, 0);
            tableLayoutPanelBlameSettings.Controls.Add(cbDetectMoveAndCopyInThisFile, 1, 0);
            tableLayoutPanelBlameSettings.Controls.Add(cbDetectMoveAndCopyInAllFiles, 2, 0);
            tableLayoutPanelBlameSettings.Dock = DockStyle.Fill;
            tableLayoutPanelBlameSettings.Location = new Point(3, 19);
            tableLayoutPanelBlameSettings.Name = "tableLayoutPanelBlameSettings";
            tableLayoutPanelBlameSettings.RowCount = 3;
            tableLayoutPanelBlameSettings.RowStyles.Add(new RowStyle());
            tableLayoutPanelBlameSettings.RowStyles.Add(new RowStyle());
            tableLayoutPanelBlameSettings.RowStyles.Add(new RowStyle());
            tableLayoutPanelBlameSettings.Size = new Size(313, 75);
            tableLayoutPanelBlameSettings.TabIndex = 0;
            // 
            // cbIgnoreWhitespace
            // 
            cbIgnoreWhitespace.AutoSize = true;
            cbIgnoreWhitespace.Dock = DockStyle.Fill;
            cbIgnoreWhitespace.Location = new Point(3, 3);
            cbIgnoreWhitespace.Name = "cbIgnoreWhitespace";
            cbIgnoreWhitespace.Size = new Size(297, 19);
            cbIgnoreWhitespace.TabIndex = 0;
            cbIgnoreWhitespace.Text = "Ignore whitespace";
            // 
            // cbDetectMoveAndCopyInThisFile
            // 
            cbDetectMoveAndCopyInThisFile.AutoSize = true;
            cbDetectMoveAndCopyInThisFile.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cbDetectMoveAndCopyInThisFile.Checked = false;
            cbDetectMoveAndCopyInThisFile.Dock = DockStyle.Fill;
            cbDetectMoveAndCopyInThisFile.Location = new Point(3, 28);
            cbDetectMoveAndCopyInThisFile.Margin = new Padding(3, 3, 3, 3);
            cbDetectMoveAndCopyInThisFile.ManualSectionAnchorName = "blame-viewer-blame-settings-detect-move-and-copy-in-this-file";
            cbDetectMoveAndCopyInThisFile.Name = "cbDetectMoveAndCopyInThisFile";
            cbDetectMoveAndCopyInThisFile.Size = new Size(297, 19);
            cbDetectMoveAndCopyInThisFile.TabIndex = 1;
            cbDetectMoveAndCopyInThisFile.Text = "Detect moved or copied lines within blamed file";
            cbDetectMoveAndCopyInThisFile.ToolTipText = null;
            cbDetectMoveAndCopyInThisFile.ToolTipIcon = UserControls.Settings.ToolTipIcon.Warning;
            // 
            // cbDetectMoveAndCopyInAllFiles
            // 
            cbDetectMoveAndCopyInAllFiles.AutoSize = true;
            cbDetectMoveAndCopyInAllFiles.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cbDetectMoveAndCopyInAllFiles.Checked = false;
            cbDetectMoveAndCopyInAllFiles.Dock = DockStyle.Fill;
            cbDetectMoveAndCopyInAllFiles.Location = new Point(3, 53);
            cbDetectMoveAndCopyInAllFiles.Margin = new Padding(3, 3, 3, 3);
            cbDetectMoveAndCopyInAllFiles.ManualSectionAnchorName = "blame-viewer-blame-settings-detect-move-and-copy-in-all-files";
            cbDetectMoveAndCopyInAllFiles.Name = "cbDetectMoveAndCopyInAllFiles";
            cbDetectMoveAndCopyInAllFiles.Size = new Size(297, 19);
            cbDetectMoveAndCopyInAllFiles.TabIndex = 2;
            cbDetectMoveAndCopyInAllFiles.Text = "Detect moved or copied lines from all files in same commit";
            cbDetectMoveAndCopyInAllFiles.ToolTipText = null;
            cbDetectMoveAndCopyInAllFiles.ToolTipIcon = UserControls.Settings.ToolTipIcon.Warning;
            // 
            // groupBoxDisplayResult
            // 
            groupBoxDisplayResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxDisplayResult.AutoSize = true;
            groupBoxDisplayResult.Controls.Add(tableLayoutPanelDisplayResult);
            groupBoxDisplayResult.Location = new Point(11, 114);
            groupBoxDisplayResult.Name = "groupBoxDisplayResult";
            groupBoxDisplayResult.Size = new Size(319, 197);
            groupBoxDisplayResult.TabIndex = 1;
            groupBoxDisplayResult.TabStop = false;
            groupBoxDisplayResult.Text = "Display result settings";
            // 
            // tableLayoutPanelDisplayResult
            // 
            tableLayoutPanelDisplayResult.AutoSize = true;
            tableLayoutPanelDisplayResult.ColumnCount = 1;
            tableLayoutPanelDisplayResult.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelDisplayResult.Controls.Add(cbDisplayAuthorFirst, 0, 0);
            tableLayoutPanelDisplayResult.Controls.Add(cbShowAuthor, 1, 0);
            tableLayoutPanelDisplayResult.Controls.Add(cbShowAuthorDate, 2, 0);
            tableLayoutPanelDisplayResult.Controls.Add(cbShowAuthorTime, 3, 0);
            tableLayoutPanelDisplayResult.Controls.Add(cbShowLineNumbers, 4, 0);
            tableLayoutPanelDisplayResult.Controls.Add(cbShowOriginalFilePath, 5, 0);
            tableLayoutPanelDisplayResult.Controls.Add(cbShowAuthorAvatar, 6, 0);
            tableLayoutPanelDisplayResult.Dock = DockStyle.Fill;
            tableLayoutPanelDisplayResult.Location = new Point(3, 19);
            tableLayoutPanelDisplayResult.Name = "tableLayoutPanelDisplayResult";
            tableLayoutPanelDisplayResult.RowCount = 7;
            tableLayoutPanelDisplayResult.RowStyles.Add(new RowStyle());
            tableLayoutPanelDisplayResult.RowStyles.Add(new RowStyle());
            tableLayoutPanelDisplayResult.RowStyles.Add(new RowStyle());
            tableLayoutPanelDisplayResult.RowStyles.Add(new RowStyle());
            tableLayoutPanelDisplayResult.RowStyles.Add(new RowStyle());
            tableLayoutPanelDisplayResult.RowStyles.Add(new RowStyle());
            tableLayoutPanelDisplayResult.RowStyles.Add(new RowStyle());
            tableLayoutPanelDisplayResult.Size = new Size(313, 175);
            tableLayoutPanelDisplayResult.TabIndex = 0;
            // 
            // cbDisplayAuthorFirst
            // 
            cbDisplayAuthorFirst.AutoSize = true;
            cbDisplayAuthorFirst.Dock = DockStyle.Fill;
            cbDisplayAuthorFirst.Location = new Point(3, 3);
            cbDisplayAuthorFirst.Name = "cbDisplayAuthorFirst";
            cbDisplayAuthorFirst.Size = new Size(297, 19);
            cbDisplayAuthorFirst.TabIndex = 0;
            cbDisplayAuthorFirst.Text = "Display author first";
            // 
            // cbShowAuthor
            // 
            cbShowAuthor.AutoSize = true;
            cbShowAuthor.Dock = DockStyle.Fill;
            cbShowAuthor.Location = new Point(3, 28);
            cbShowAuthor.Name = "cbShowAuthor";
            cbShowAuthor.Size = new Size(297, 19);
            cbShowAuthor.TabIndex = 1;
            cbShowAuthor.Text = "Show author";
            // 
            // cbShowAuthorDate
            // 
            cbShowAuthorDate.AutoSize = true;
            cbShowAuthorDate.Dock = DockStyle.Fill;
            cbShowAuthorDate.Location = new Point(3, 53);
            cbShowAuthorDate.Name = "cbShowAuthorDate";
            cbShowAuthorDate.Size = new Size(297, 19);
            cbShowAuthorDate.TabIndex = 2;
            cbShowAuthorDate.Text = "Show author date";
            // 
            // cbShowAuthorTime
            // 
            cbShowAuthorTime.AutoSize = true;
            cbShowAuthorTime.Dock = DockStyle.Fill;
            cbShowAuthorTime.Location = new Point(3, 78);
            cbShowAuthorTime.Name = "cbShowAuthorTime";
            cbShowAuthorTime.Size = new Size(297, 19);
            cbShowAuthorTime.TabIndex = 3;
            cbShowAuthorTime.Text = "Show author time";
            // 
            // cbShowLineNumbers
            // 
            cbShowLineNumbers.AutoSize = true;
            cbShowLineNumbers.Dock = DockStyle.Fill;
            cbShowLineNumbers.Location = new Point(3, 103);
            cbShowLineNumbers.Name = "cbShowLineNumbers";
            cbShowLineNumbers.Size = new Size(297, 19);
            cbShowLineNumbers.TabIndex = 4;
            cbShowLineNumbers.Text = "Show line numbers";
            // 
            // cbShowOriginalFilePath
            // 
            cbShowOriginalFilePath.AutoSize = true;
            cbShowOriginalFilePath.Dock = DockStyle.Fill;
            cbShowOriginalFilePath.Location = new Point(3, 128);
            cbShowOriginalFilePath.Name = "cbShowOriginalFilePath";
            cbShowOriginalFilePath.Size = new Size(297, 19);
            cbShowOriginalFilePath.TabIndex = 5;
            cbShowOriginalFilePath.Text = "Show original file path";
            // 
            // cbShowAuthorAvatar
            // 
            cbShowAuthorAvatar.AutoSize = true;
            cbShowAuthorAvatar.Dock = DockStyle.Fill;
            cbShowAuthorAvatar.Location = new Point(3, 153);
            cbShowAuthorAvatar.Name = "cbShowAuthorAvatar";
            cbShowAuthorAvatar.Size = new Size(297, 19);
            cbShowAuthorAvatar.TabIndex = 6;
            cbShowAuthorAvatar.Text = "Show author avatar";
            // 
            // BlameViewerSettingsPage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(groupBoxDisplayResult);
            Controls.Add(groupBoxBlameSettings);
            Name = "BlameViewerSettingsPage";
            Size = new Size(341, 272);
            Text = "Blame viewer";
            groupBoxBlameSettings.ResumeLayout(false);
            groupBoxBlameSettings.PerformLayout();
            tableLayoutPanelBlameSettings.ResumeLayout(false);
            tableLayoutPanelBlameSettings.PerformLayout();
            groupBoxDisplayResult.ResumeLayout(false);
            groupBoxDisplayResult.PerformLayout();
            tableLayoutPanelDisplayResult.ResumeLayout(false);
            tableLayoutPanelDisplayResult.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private GroupBox groupBoxBlameSettings;
        private TableLayoutPanel tableLayoutPanelBlameSettings;
        private CheckBox cbIgnoreWhitespace;
        private SettingsCheckBox cbDetectMoveAndCopyInThisFile;
        private SettingsCheckBox cbDetectMoveAndCopyInAllFiles;
        private GroupBox groupBoxDisplayResult;
        private TableLayoutPanel tableLayoutPanelDisplayResult;
        private CheckBox cbDisplayAuthorFirst;
        private CheckBox cbShowAuthorDate;
        private CheckBox cbShowAuthorTime;
        private CheckBox cbShowLineNumbers;
        private CheckBox cbShowAuthor;
        private CheckBox cbShowOriginalFilePath;
        private CheckBox cbShowAuthorAvatar;
    }
}
