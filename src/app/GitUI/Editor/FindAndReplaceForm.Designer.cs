namespace GitUI
{
	partial class FindAndReplaceForm
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
            lblReplaceWith = new Label();
            txtLookFor = new TextBox();
            txtReplaceWith = new TextBox();
            btnFindNext = new Button();
            btnReplace = new Button();
            btnReplaceAll = new Button();
            chkMatchWholeWord = new CheckBox();
            chkMatchCase = new CheckBox();
            btnHighlightAll = new Button();
            btnCancel = new Button();
            btnFindPrevious = new Button();
            SuspendLayout();
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
            // lblReplaceWith
            // 
            lblReplaceWith.AutoSize = true;
            lblReplaceWith.Location = new Point(12, 35);
            lblReplaceWith.Name = "lblReplaceWith";
            lblReplaceWith.Size = new Size(77, 15);
            lblReplaceWith.TabIndex = 2;
            lblReplaceWith.Text = "Re&place with:";
            // 
            // txtLookFor
            // 
            txtLookFor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLookFor.Location = new Point(90, 6);
            txtLookFor.Name = "txtLookFor";
            txtLookFor.Size = new Size(317, 23);
            txtLookFor.TabIndex = 1;
            // 
            // txtReplaceWith
            // 
            txtReplaceWith.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtReplaceWith.Location = new Point(90, 32);
            txtReplaceWith.Name = "txtReplaceWith";
            txtReplaceWith.Size = new Size(317, 23);
            txtReplaceWith.TabIndex = 3;
            // 
            // btnFindNext
            // 
            btnFindNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFindNext.Location = new Point(307, 106);
            btnFindNext.Name = "btnFindNext";
            btnFindNext.Size = new Size(100, 25);
            btnFindNext.TabIndex = 6;
            btnFindNext.Text = "&Find next";
            btnFindNext.UseVisualStyleBackColor = true;
            btnFindNext.Click += btnFindNext_Click;
            // 
            // btnReplace
            // 
            btnReplace.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReplace.Location = new Point(86, 134);
            btnReplace.Name = "btnReplace";
            btnReplace.Size = new Size(100, 25);
            btnReplace.TabIndex = 7;
            btnReplace.Text = "&Replace";
            btnReplace.UseVisualStyleBackColor = true;
            btnReplace.Click += btnReplace_Click;
            // 
            // btnReplaceAll
            // 
            btnReplaceAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnReplaceAll.Location = new Point(201, 134);
            btnReplaceAll.Name = "btnReplaceAll";
            btnReplaceAll.Size = new Size(100, 25);
            btnReplaceAll.TabIndex = 9;
            btnReplaceAll.Text = "Replace &All";
            btnReplaceAll.UseVisualStyleBackColor = true;
            btnReplaceAll.Click += btnReplaceAll_Click;
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
            // 
            // btnHighlightAll
            // 
            btnHighlightAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnHighlightAll.Location = new Point(86, 134);
            btnHighlightAll.Name = "btnHighlightAll";
            btnHighlightAll.Size = new Size(215, 25);
            btnHighlightAll.TabIndex = 8;
            btnHighlightAll.Text = "Find && highlight &all";
            btnHighlightAll.UseVisualStyleBackColor = true;
            btnHighlightAll.Visible = false;
            btnHighlightAll.Click += btnHighlightAll_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(307, 134);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 25);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnFindPrevious
            // 
            btnFindPrevious.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFindPrevious.Location = new Point(201, 106);
            btnFindPrevious.Name = "btnFindPrevious";
            btnFindPrevious.Size = new Size(100, 25);
            btnFindPrevious.TabIndex = 6;
            btnFindPrevious.Text = "Find pre&vious";
            btnFindPrevious.UseVisualStyleBackColor = true;
            btnFindPrevious.Click += btnFindPrevious_Click;
            // 
            // FindAndReplaceForm
            // 
            AcceptButton = btnFindNext;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = btnCancel;
            ClientSize = new Size(419, 169);
            Controls.Add(chkMatchCase);
            Controls.Add(chkMatchWholeWord);
            Controls.Add(btnReplaceAll);
            Controls.Add(btnReplace);
            Controls.Add(btnHighlightAll);
            Controls.Add(btnCancel);
            Controls.Add(btnFindPrevious);
            Controls.Add(btnFindNext);
            Controls.Add(txtReplaceWith);
            Controls.Add(txtLookFor);
            Controls.Add(lblReplaceWith);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FindAndReplaceForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Find and replace";
            FormClosing += FindAndReplaceForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();

		}

		#endregion

		private Label label1;
		private Label lblReplaceWith;
		private TextBox txtLookFor;
		private TextBox txtReplaceWith;
		private Button btnFindNext;
		private Button btnReplace;
		private Button btnReplaceAll;
		private CheckBox chkMatchWholeWord;
		private CheckBox chkMatchCase;
		private Button btnHighlightAll;
		private Button btnCancel;
		private Button btnFindPrevious;
	}
}