
using ICSharpCode.TextEditor.Actions;

namespace GitUI
{
    partial class FormFindInCommitFilesGitGrep
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            lblOptions = new Label();
            cboFindInCommitFilesGitGrep = new ComboBox();
            txtOptions = new TextBox();
            chkMatchWholeWord = new CheckBox();
            chkMatchCase = new CheckBox();
            btnSearch = new Button();
            chkShowSearchBox = new CheckBox();
            lblSearchCommitGitGrepWatermark = new Label();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(lblSearchCommitGitGrepWatermark);
            MainPanel.Controls.Add(chkMatchCase);
            MainPanel.Controls.Add(chkMatchWholeWord);
            MainPanel.Controls.Add(txtOptions);
            MainPanel.Controls.Add(cboFindInCommitFilesGitGrep);
            MainPanel.Controls.Add(lblOptions);
            MainPanel.Controls.Add(label1);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(425, 103);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(btnSearch);
            ControlsPanel.Controls.Add(chkShowSearchBox);
            ControlsPanel.Location = new Point(0, 50);
            ControlsPanel.Size = new Size(425, 41);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 0;
            label1.Text = "Fi&nd what:";
            // 
            // lblOptions
            // 
            lblOptions.AutoSize = true;
            lblOptions.Location = new Point(12, 35);
            lblOptions.Name = "lblOptions";
            lblOptions.Size = new Size(52, 15);
            lblOptions.TabIndex = 2;
            lblOptions.Text = "&Options:";
            // 
            // cboFindInCommitFilesGitGrep
            // 
            cboFindInCommitFilesGitGrep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboFindInCommitFilesGitGrep.Location = new Point(90, 6);
            cboFindInCommitFilesGitGrep.Name = "cboFindInCommitFilesGitGrep";
            cboFindInCommitFilesGitGrep.Size = new Size(323, 23);
            cboFindInCommitFilesGitGrep.TabIndex = 1;
            cboFindInCommitFilesGitGrep.GotFocus += cboSearchCommitGitGrep_GotFocus;
            cboFindInCommitFilesGitGrep.LostFocus += cboSearchCommitGitGrep_LostFocus;
            // 
            // txtOptions
            // 
            txtOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOptions.Location = new Point(90, 32);
            txtOptions.Name = "txtOptions";
            txtOptions.Size = new Size(323, 23);
            txtOptions.TabIndex = 2;
            txtOptions.TextChanged += txtOptions_TextChanged;
            // 
            // chkMatchWholeWord
            // 
            chkMatchWholeWord.AutoSize = true;
            chkMatchWholeWord.Location = new Point(90, 81);
            chkMatchWholeWord.Name = "chkMatchWholeWord";
            chkMatchWholeWord.Size = new Size(125, 19);
            chkMatchWholeWord.TabIndex = 5;
            chkMatchWholeWord.Text = "Match &whole word";
            chkMatchWholeWord.UseVisualStyleBackColor = true;
            chkMatchWholeWord.CheckedChanged += chkMatchWholeWord_CheckedChanged;
            // 
            // chkMatchCase
            // 
            chkMatchCase.AutoSize = true;
            chkMatchCase.Location = new Point(90, 58);
            chkMatchCase.Name = "chkMatchCase";
            chkMatchCase.Size = new Size(86, 19);
            chkMatchCase.TabIndex = 4;
            chkMatchCase.Text = "Match &case";
            chkMatchCase.UseVisualStyleBackColor = true;
            chkMatchCase.CheckedChanged += chkMatchCase_CheckedChanged;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSearch.AutoSize = true;
            btnSearch.DialogResult = DialogResult.Cancel;
            btnSearch.Location = new Point(337, 8);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 25);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "&Find";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // chkShowSearchBox
            // 
            chkShowSearchBox.AutoSize = true;
            chkShowSearchBox.Location = new Point(89, 8);
            chkShowSearchBox.MinimumSize = new Size(0, 25);
            chkShowSearchBox.Name = "chkShowSearchBox";
            chkShowSearchBox.Size = new Size(242, 25);
            chkShowSearchBox.TabIndex = 6;
            chkShowSearchBox.Text = "&Show 'Find in commit files using git-grep'";
            chkShowSearchBox.UseVisualStyleBackColor = true;
            chkShowSearchBox.CheckedChanged += chkShowSearchBox_CheckedChanged;
            // 
            // lblSearchCommitGitGrepWatermark
            // 
            lblSearchCommitGitGrepWatermark.AutoSize = true;
            lblSearchCommitGitGrepWatermark.BackColor = SystemColors.Window;
            lblSearchCommitGitGrepWatermark.ForeColor = SystemColors.GrayText;
            lblSearchCommitGitGrepWatermark.Location = new Point(93, 8);
            lblSearchCommitGitGrepWatermark.Name = "lblSearchCommitGitGrepWatermark";
            lblSearchCommitGitGrepWatermark.Padding = new Padding(2);
            lblSearchCommitGitGrepWatermark.Size = new Size(162, 19);
            lblSearchCommitGitGrepWatermark.TabIndex = 3;
            lblSearchCommitGitGrepWatermark.Text = "git-grep regular expression...";
            lblSearchCommitGitGrepWatermark.Click += lblSearchCommitGitGrepWatermark_Click;
            // 
            // FormFindInCommitFilesGitGrep
            // 
            AcceptButton = btnSearch;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(425, 144);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            HelpButton = true;
            ManualSectionAnchorName = "diff";
            ManualSectionSubfolder = "browse_repository";
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormFindInCommitFilesGitGrep";
            StartPosition = FormStartPosition.Manual;
            Text = "Find in commit files using git-grep";
            FormClosing += FormFindInCommitFilesGitGrep_FormClosing;
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblSearchCommitGitGrepWatermark;
        private Label label1;
        private Label lblOptions;
        private ComboBox cboFindInCommitFilesGitGrep;
        private TextBox txtOptions;
        private CheckBox chkMatchWholeWord;
        private CheckBox chkMatchCase;
        private Button btnSearch;
        private CheckBox chkShowSearchBox;
    }
}
