
using ICSharpCode.TextEditor.Actions;

namespace GitUI
{
	partial class FormSearchCommit
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
            txtSearchFor = new ComboBox();
            txtOptions = new TextBox();
            chkMatchWholeWord = new CheckBox();
            chkMatchCase = new CheckBox();
            btnSearch = new Button();
            chkShowSearchBox = new CheckBox();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(chkMatchCase);
            MainPanel.Controls.Add(chkMatchWholeWord);
            MainPanel.Controls.Add(txtOptions);
            MainPanel.Controls.Add(txtSearchFor);
            MainPanel.Controls.Add(lblOptions);
            MainPanel.Controls.Add(label1);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(419, 103);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(btnSearch);
            ControlsPanel.Controls.Add(chkShowSearchBox);
            ControlsPanel.Location = new Point(0, 50);
            ControlsPanel.Size = new Size(419, 41);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(45, 15);
            label1.TabIndex = 0;
            label1.Text = "&Search:";
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
            // txtSearchFor
            // 
            txtSearchFor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearchFor.Location = new Point(90, 6);
            txtSearchFor.Name = "txtSearchFor";
            txtSearchFor.Size = new Size(317, 23);
            txtSearchFor.TabIndex = 1;
            // 
            // txtOptions
            // 
            txtOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOptions.Location = new Point(90, 32);
            txtOptions.Name = "txtOptions";
            txtOptions.Size = new Size(317, 23);
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
            btnSearch.DialogResult = DialogResult.Cancel;
            btnSearch.Location = new Point(306, 8);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 25);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // chkShowSearchBox
            // 
            chkShowSearchBox.AutoSize = true;
            chkShowSearchBox.Location = new Point(12, 10);
            chkShowSearchBox.Name = "chkShowSearchBox";
            chkShowSearchBox.Size = new Size(125, 19);
            chkShowSearchBox.TabIndex = 6;
            chkShowSearchBox.Text = "Show search &box";
            chkShowSearchBox.UseVisualStyleBackColor = true;
            chkShowSearchBox.CheckedChanged += chkShowSearchBox_CheckedChanged;
            // 
            // FormSearchCommit
            // 
            AcceptButton = btnSearch;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(419, 144);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            HelpButton = true;
            ManualSectionAnchorName = "diff";
            ManualSectionSubfolder = "browse_repository";
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormSearchCommit";
            StartPosition = FormStartPosition.Manual;
            Text = "Search files in commit";
            FormClosing += FormSearchCommit_FormClosing;
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
		private Label lblOptions;
		private ComboBox txtSearchFor;
		private TextBox txtOptions;
		private CheckBox chkMatchWholeWord;
		private CheckBox chkMatchCase;
		private Button btnSearch;
        private CheckBox chkShowSearchBox;
    }
}
