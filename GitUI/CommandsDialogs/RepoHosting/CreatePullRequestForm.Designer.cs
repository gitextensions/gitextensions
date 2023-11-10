namespace GitUI.CommandsDialogs.RepoHosting
{
    partial class CreatePullRequestForm
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
            _titleTB = new TextBox();
            label1 = new Label();
            label2 = new Label();
            groupBox1 = new GroupBox();
            _bodyTB = new GitUI.SpellChecker.EditNetSpell();
            _pullReqTargetsCB = new ComboBox();
            label3 = new Label();
            _createBtn = new Button();
            label4 = new Label();
            label5 = new Label();
            _yourBranchesCB = new ComboBox();
            _remoteBranchesCB = new ComboBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // _titleTB
            // 
            _titleTB.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _titleTB.Location = new Point(58, 19);
            _titleTB.Name = "_titleTB";
            _titleTB.Size = new Size(462, 20);
            _titleTB.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 22);
            label1.Name = "label1";
            label1.Size = new Size(30, 13);
            label1.TabIndex = 1;
            label1.Text = "Title:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 48);
            label2.Name = "label2";
            label2.Size = new Size(34, 13);
            label2.TabIndex = 2;
            label2.Text = "Body:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(_bodyTB);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(_titleTB);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(12, 100);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(526, 175);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Pull request data";
            // 
            // _bodyTB
            // 
            _bodyTB.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _bodyTB.Location = new Point(58, 45);
            _bodyTB.Margin = new Padding(3, 4, 3, 4);
            _bodyTB.Name = "_bodyTB";
            _bodyTB.Size = new Size(462, 124);
            _bodyTB.TabIndex = 1;
            _bodyTB.WatermarkText = "";
            // 
            // _pullReqTargetsCB
            // 
            _pullReqTargetsCB.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _pullReqTargetsCB.DropDownStyle = ComboBoxStyle.DropDownList;
            _pullReqTargetsCB.FormattingEnabled = true;
            _pullReqTargetsCB.Location = new Point(141, 12);
            _pullReqTargetsCB.Name = "_pullReqTargetsCB";
            _pullReqTargetsCB.Size = new Size(391, 21);
            _pullReqTargetsCB.TabIndex = 3;
            _pullReqTargetsCB.SelectedIndexChanged += _pullReqTargetsCB_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(9, 15);
            label3.Name = "label3";
            label3.Size = new Size(89, 13);
            label3.TabIndex = 5;
            label3.Text = "Target repository:";
            // 
            // _createBtn
            // 
            _createBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _createBtn.Location = new Point(426, 281);
            _createBtn.Name = "_createBtn";
            _createBtn.Size = new Size(112, 33);
            _createBtn.TabIndex = 2;
            _createBtn.Text = "Create";
            _createBtn.UseVisualStyleBackColor = true;
            _createBtn.Click += _createBtn_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(9, 42);
            label4.Name = "label4";
            label4.Size = new Size(68, 13);
            label4.TabIndex = 7;
            label4.Text = "Your branch:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(9, 69);
            label5.Name = "label5";
            label5.Size = new Size(77, 13);
            label5.TabIndex = 8;
            label5.Text = "Target branch:";
            // 
            // _yourBranchesCB
            // 
            _yourBranchesCB.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _yourBranchesCB.DropDownStyle = ComboBoxStyle.DropDownList;
            _yourBranchesCB.FormattingEnabled = true;
            _yourBranchesCB.Location = new Point(141, 39);
            _yourBranchesCB.Name = "_yourBranchesCB";
            _yourBranchesCB.Size = new Size(391, 21);
            _yourBranchesCB.TabIndex = 0;
            _yourBranchesCB.SelectedIndexChanged += _yourBranchCB_SelectedIndexChanged;
            // 
            // _remoteBranchesCB
            // 
            _remoteBranchesCB.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _remoteBranchesCB.DropDownStyle = ComboBoxStyle.DropDownList;
            _remoteBranchesCB.FormattingEnabled = true;
            _remoteBranchesCB.Location = new Point(141, 66);
            _remoteBranchesCB.Name = "_remoteBranchesCB";
            _remoteBranchesCB.Size = new Size(391, 21);
            _remoteBranchesCB.TabIndex = 1;
            _remoteBranchesCB.SelectedIndexChanged += _yourBranchCB_SelectedIndexChanged;
            // 
            // CreatePullRequestForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(546, 323);
            Controls.Add(_remoteBranchesCB);
            Controls.Add(_yourBranchesCB);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(_createBtn);
            Controls.Add(label3);
            Controls.Add(_pullReqTargetsCB);
            Controls.Add(groupBox1);
            MinimumSize = new Size(552, 345);
            Name = "CreatePullRequestForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create Pull Request";
            Load += CreatePullRequestForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox _titleTB;
        private Label label1;
        private Label label2;
        private GroupBox groupBox1;
        private GitUI.SpellChecker.EditNetSpell _bodyTB;
        private ComboBox _pullReqTargetsCB;
        private Label label3;
        private Button _createBtn;
        private Label label4;
        private Label label5;
        private ComboBox _yourBranchesCB;
        private ComboBox _remoteBranchesCB;
    }
}
