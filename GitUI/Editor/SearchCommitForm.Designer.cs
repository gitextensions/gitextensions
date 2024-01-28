
namespace GitUI
{
	partial class SearchCommitForm
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
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(45, 15);
            label1.TabIndex = 0;
            label1.Text = "Search:";
            // 
            // lblOptions
            // 
            lblOptions.AutoSize = true;
            lblOptions.Location = new Point(12, 35);
            lblOptions.Name = "lblOptions";
            lblOptions.Size = new Size(52, 15);
            lblOptions.TabIndex = 2;
            lblOptions.Text = "Options:";
            // 
            // txtSearchFor
            // 
            txtSearchFor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearchFor.Location = new Point(90, 6);
            txtSearchFor.Name = "txtSearchFor";
            txtSearchFor.Size = new Size(317, 23);
            txtSearchFor.TabIndex = 1;
            txtSearchFor.KeyDown += txtSearchFor_KeyDown;
            txtSearchFor.SelectedIndexChanged += btnSearch_Click;
            // 
            // txtOptions
            // 
            txtOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOptions.Location = new Point(90, 32);
            txtOptions.Name = "txtOptions";
            txtOptions.Size = new Size(317, 23);
            txtOptions.TabIndex = 2;
            txtOptions.TextChanged += txtOptions_TextChanged;
            txtOptions.KeyDown += txtSearchFor_KeyDown;
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
            // chkShowSearchBox
            // 
            chkShowSearchBox.AutoSize = true;
            chkShowSearchBox.Location = new Point(90, 106);
            chkShowSearchBox.Name = "chkShowSearchBox";
            chkShowSearchBox.Size = new Size(125, 19);
            chkShowSearchBox.TabIndex = 6;
            chkShowSearchBox.Text = "Show search &box";
            chkShowSearchBox.UseVisualStyleBackColor = true;
            chkShowSearchBox.CheckedChanged += chkShowSearchBox_CheckedChanged;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSearch.DialogResult = DialogResult.Cancel;
            btnSearch.Location = new Point(307, 134);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(100, 25);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "&Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // SearchCommitForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = btnSearch;
            ClientSize = new Size(419, 169);
            Controls.Add(chkShowSearchBox);
            Controls.Add(chkMatchCase);
            Controls.Add(chkMatchWholeWord);
            Controls.Add(btnSearch);
            Controls.Add(txtOptions);
            Controls.Add(txtSearchFor);
            Controls.Add(lblOptions);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SearchCommitForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Search commit";
            FormClosing += SearchCommitForm_FormClosing;
            Load += SearchCommitForm_Load;
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
