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
            this.groupBoxBlameSettings = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelBlameSettings = new System.Windows.Forms.TableLayoutPanel();
            this.cbIgnoreWhitespace = new System.Windows.Forms.CheckBox();
            this.cbDetectMoveAndCopyInThisFile = new System.Windows.Forms.CheckBox();
            this.cbDetectMoveAndCopyInAllFiles = new System.Windows.Forms.CheckBox();
            this.groupBoxDisplayResult = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelDisplayResult = new System.Windows.Forms.TableLayoutPanel();
            this.cbDisplayAuthorFirst = new System.Windows.Forms.CheckBox();
            this.cbShowAuthor = new System.Windows.Forms.CheckBox();
            this.cbShowAuthorDate = new System.Windows.Forms.CheckBox();
            this.cbShowAuthorTime = new System.Windows.Forms.CheckBox();
            this.cbShowLineNumbers = new System.Windows.Forms.CheckBox();
            this.cbShowOriginalFilePath = new System.Windows.Forms.CheckBox();
            this.cbShowAuthorAvatar = new System.Windows.Forms.CheckBox();
            this.groupBoxBlameSettings.SuspendLayout();
            this.tableLayoutPanelBlameSettings.SuspendLayout();
            this.groupBoxDisplayResult.SuspendLayout();
            this.tableLayoutPanelDisplayResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxBlameSettings
            // 
            this.groupBoxBlameSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBlameSettings.AutoSize = true;
            this.groupBoxBlameSettings.Controls.Add(this.tableLayoutPanelBlameSettings);
            this.groupBoxBlameSettings.Location = new System.Drawing.Point(11, 11);
            this.groupBoxBlameSettings.Name = "groupBoxBlameSettings";
            this.groupBoxBlameSettings.Size = new System.Drawing.Size(319, 97);
            this.groupBoxBlameSettings.TabIndex = 0;
            this.groupBoxBlameSettings.TabStop = false;
            this.groupBoxBlameSettings.Text = "Blame settings";
            // 
            // tableLayoutPanelBlameSettings
            // 
            this.tableLayoutPanelBlameSettings.AutoSize = true;
            this.tableLayoutPanelBlameSettings.ColumnCount = 1;
            this.tableLayoutPanelBlameSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelBlameSettings.Controls.Add(this.cbIgnoreWhitespace, 0, 0);
            this.tableLayoutPanelBlameSettings.Controls.Add(this.cbDetectMoveAndCopyInThisFile, 1, 0);
            this.tableLayoutPanelBlameSettings.Controls.Add(this.cbDetectMoveAndCopyInAllFiles, 2, 0);
            this.tableLayoutPanelBlameSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBlameSettings.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanelBlameSettings.Name = "tableLayoutPanelBlameSettings";
            this.tableLayoutPanelBlameSettings.RowCount = 3;
            this.tableLayoutPanelBlameSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBlameSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBlameSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBlameSettings.Size = new System.Drawing.Size(313, 75);
            this.tableLayoutPanelBlameSettings.TabIndex = 0;
            // 
            // cbIgnoreWhitespace
            // 
            this.cbIgnoreWhitespace.AutoSize = true;
            this.cbIgnoreWhitespace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbIgnoreWhitespace.Location = new System.Drawing.Point(3, 3);
            this.cbIgnoreWhitespace.Name = "cbIgnoreWhitespace";
            this.cbIgnoreWhitespace.Size = new System.Drawing.Size(297, 19);
            this.cbIgnoreWhitespace.TabIndex = 0;
            this.cbIgnoreWhitespace.Text = "Ignore whitespace";
            // 
            // cbDetectMoveAndCopyInThisFile
            // 
            this.cbDetectMoveAndCopyInThisFile.AutoSize = true;
            this.cbDetectMoveAndCopyInThisFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDetectMoveAndCopyInThisFile.Location = new System.Drawing.Point(3, 28);
            this.cbDetectMoveAndCopyInThisFile.Name = "cbDetectMoveAndCopyInThisFile";
            this.cbDetectMoveAndCopyInThisFile.Size = new System.Drawing.Size(297, 19);
            this.cbDetectMoveAndCopyInThisFile.TabIndex = 1;
            this.cbDetectMoveAndCopyInThisFile.Text = "Detect move and copy in this file";
            // 
            // cbDetectMoveAndCopyInAllFiles
            // 
            this.cbDetectMoveAndCopyInAllFiles.AutoSize = true;
            this.cbDetectMoveAndCopyInAllFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDetectMoveAndCopyInAllFiles.Location = new System.Drawing.Point(3, 53);
            this.cbDetectMoveAndCopyInAllFiles.Name = "cbDetectMoveAndCopyInAllFiles";
            this.cbDetectMoveAndCopyInAllFiles.Size = new System.Drawing.Size(297, 19);
            this.cbDetectMoveAndCopyInAllFiles.TabIndex = 2;
            this.cbDetectMoveAndCopyInAllFiles.Text = "Detect move and copy in all files";
            // 
            // groupBoxDisplayResult
            // 
            this.groupBoxDisplayResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDisplayResult.AutoSize = true;
            this.groupBoxDisplayResult.Controls.Add(this.tableLayoutPanelDisplayResult);
            this.groupBoxDisplayResult.Location = new System.Drawing.Point(11, 114);
            this.groupBoxDisplayResult.Name = "groupBoxDisplayResult";
            this.groupBoxDisplayResult.Size = new System.Drawing.Size(319, 197);
            this.groupBoxDisplayResult.TabIndex = 1;
            this.groupBoxDisplayResult.TabStop = false;
            this.groupBoxDisplayResult.Text = "Display result settings";
            // 
            // tableLayoutPanelDisplayResult
            // 
            this.tableLayoutPanelDisplayResult.AutoSize = true;
            this.tableLayoutPanelDisplayResult.ColumnCount = 1;
            this.tableLayoutPanelDisplayResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelDisplayResult.Controls.Add(this.cbDisplayAuthorFirst, 0, 0);
            this.tableLayoutPanelDisplayResult.Controls.Add(this.cbShowAuthor, 1, 0);
            this.tableLayoutPanelDisplayResult.Controls.Add(this.cbShowAuthorDate, 2, 0);
            this.tableLayoutPanelDisplayResult.Controls.Add(this.cbShowAuthorTime, 3, 0);
            this.tableLayoutPanelDisplayResult.Controls.Add(this.cbShowLineNumbers, 4, 0);
            this.tableLayoutPanelDisplayResult.Controls.Add(this.cbShowOriginalFilePath, 5, 0);
            this.tableLayoutPanelDisplayResult.Controls.Add(this.cbShowAuthorAvatar, 6, 0);
            this.tableLayoutPanelDisplayResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelDisplayResult.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanelDisplayResult.Name = "tableLayoutPanelDisplayResult";
            this.tableLayoutPanelDisplayResult.RowCount = 7;
            this.tableLayoutPanelDisplayResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDisplayResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDisplayResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDisplayResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDisplayResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDisplayResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDisplayResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDisplayResult.Size = new System.Drawing.Size(313, 175);
            this.tableLayoutPanelDisplayResult.TabIndex = 0;
            // 
            // cbDisplayAuthorFirst
            // 
            this.cbDisplayAuthorFirst.AutoSize = true;
            this.cbDisplayAuthorFirst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDisplayAuthorFirst.Location = new System.Drawing.Point(3, 3);
            this.cbDisplayAuthorFirst.Name = "cbDisplayAuthorFirst";
            this.cbDisplayAuthorFirst.Size = new System.Drawing.Size(297, 19);
            this.cbDisplayAuthorFirst.TabIndex = 0;
            this.cbDisplayAuthorFirst.Text = "Display author first";
            // 
            // cbShowAuthor
            // 
            this.cbShowAuthor.AutoSize = true;
            this.cbShowAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbShowAuthor.Location = new System.Drawing.Point(3, 28);
            this.cbShowAuthor.Name = "cbShowAuthor";
            this.cbShowAuthor.Size = new System.Drawing.Size(297, 19);
            this.cbShowAuthor.TabIndex = 1;
            this.cbShowAuthor.Text = "Show author";
            // 
            // cbShowAuthorDate
            // 
            this.cbShowAuthorDate.AutoSize = true;
            this.cbShowAuthorDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbShowAuthorDate.Location = new System.Drawing.Point(3, 53);
            this.cbShowAuthorDate.Name = "cbShowAuthorDate";
            this.cbShowAuthorDate.Size = new System.Drawing.Size(297, 19);
            this.cbShowAuthorDate.TabIndex = 2;
            this.cbShowAuthorDate.Text = "Show author date";
            // 
            // cbShowAuthorTime
            // 
            this.cbShowAuthorTime.AutoSize = true;
            this.cbShowAuthorTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbShowAuthorTime.Location = new System.Drawing.Point(3, 78);
            this.cbShowAuthorTime.Name = "cbShowAuthorTime";
            this.cbShowAuthorTime.Size = new System.Drawing.Size(297, 19);
            this.cbShowAuthorTime.TabIndex = 3;
            this.cbShowAuthorTime.Text = "Show author time";
            // 
            // cbShowLineNumbers
            // 
            this.cbShowLineNumbers.AutoSize = true;
            this.cbShowLineNumbers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbShowLineNumbers.Location = new System.Drawing.Point(3, 103);
            this.cbShowLineNumbers.Name = "cbShowLineNumbers";
            this.cbShowLineNumbers.Size = new System.Drawing.Size(297, 19);
            this.cbShowLineNumbers.TabIndex = 4;
            this.cbShowLineNumbers.Text = "Show line numbers";
            // 
            // cbShowOriginalFilePath
            // 
            this.cbShowOriginalFilePath.AutoSize = true;
            this.cbShowOriginalFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbShowOriginalFilePath.Location = new System.Drawing.Point(3, 128);
            this.cbShowOriginalFilePath.Name = "cbShowOriginalFilePath";
            this.cbShowOriginalFilePath.Size = new System.Drawing.Size(297, 19);
            this.cbShowOriginalFilePath.TabIndex = 5;
            this.cbShowOriginalFilePath.Text = "Show original file path";
            // 
            // cbShowAuthorAvatar
            // 
            this.cbShowAuthorAvatar.AutoSize = true;
            this.cbShowAuthorAvatar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbShowAuthorAvatar.Location = new System.Drawing.Point(3, 153);
            this.cbShowAuthorAvatar.Name = "cbShowAuthorAvatar";
            this.cbShowAuthorAvatar.Size = new System.Drawing.Size(297, 19);
            this.cbShowAuthorAvatar.TabIndex = 6;
            this.cbShowAuthorAvatar.Text = "Show author avatar";
            // 
            // BlameViewerSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.groupBoxDisplayResult);
            this.Controls.Add(this.groupBoxBlameSettings);
            this.Name = "BlameViewerSettingsPage";
            this.Size = new System.Drawing.Size(341, 272);
            this.groupBoxBlameSettings.ResumeLayout(false);
            this.groupBoxBlameSettings.PerformLayout();
            this.tableLayoutPanelBlameSettings.ResumeLayout(false);
            this.tableLayoutPanelBlameSettings.PerformLayout();
            this.groupBoxDisplayResult.ResumeLayout(false);
            this.groupBoxDisplayResult.PerformLayout();
            this.tableLayoutPanelDisplayResult.ResumeLayout(false);
            this.tableLayoutPanelDisplayResult.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxBlameSettings;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBlameSettings;
        private System.Windows.Forms.CheckBox cbIgnoreWhitespace;
        private System.Windows.Forms.CheckBox cbDetectMoveAndCopyInThisFile;
        private System.Windows.Forms.CheckBox cbDetectMoveAndCopyInAllFiles;
        private System.Windows.Forms.GroupBox groupBoxDisplayResult;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDisplayResult;
        private System.Windows.Forms.CheckBox cbDisplayAuthorFirst;
        private System.Windows.Forms.CheckBox cbShowAuthorDate;
        private System.Windows.Forms.CheckBox cbShowAuthorTime;
        private System.Windows.Forms.CheckBox cbShowLineNumbers;
        private System.Windows.Forms.CheckBox cbShowAuthor;
        private System.Windows.Forms.CheckBox cbShowOriginalFilePath;
        private System.Windows.Forms.CheckBox cbShowAuthorAvatar;
    }
}
