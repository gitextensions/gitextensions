namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormGoToCommit
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
            goButton = new Button();
            textboxCommitExpression = new TextBox();
            label1 = new Label();
            groupBox1 = new GroupBox();
            linkGitRevParse = new LinkLabel();
            label2 = new Label();
            label3 = new Label();
            comboBoxTags = new ComboBox();
            label4 = new Label();
            comboBoxBranches = new ComboBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // goButton
            // 
            goButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            goButton.DialogResult = DialogResult.OK;
            goButton.Location = new Point(521, 10);
            goButton.Name = "goButton";
            goButton.Size = new Size(75, 28);
            goButton.TabIndex = 3;
            goButton.Text = "Go";
            goButton.UseVisualStyleBackColor = true;
            goButton.Click += goButton_Click;
            // 
            // textboxCommitExpression
            // 
            textboxCommitExpression.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textboxCommitExpression.Location = new Point(155, 13);
            textboxCommitExpression.Name = "textboxCommitExpression";
            textboxCommitExpression.Size = new Size(360, 23);
            textboxCommitExpression.TabIndex = 0;
            textboxCommitExpression.TextChanged += commitExpression_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 17);
            label1.Name = "label1";
            label1.Size = new Size(112, 15);
            label1.TabIndex = 2;
            label1.Text = "Commit expression:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(linkGitRevParse);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(45, 43);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(470, 141);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Help";
            // 
            // linkGitRevParse
            // 
            linkGitRevParse.AutoSize = true;
            linkGitRevParse.Location = new Point(19, 112);
            linkGitRevParse.Name = "linkGitRevParse";
            linkGitRevParse.Size = new Size(126, 15);
            linkGitRevParse.TabIndex = 0;
            linkGitRevParse.TabStop = true;
            linkGitRevParse.Text = "More see git-rev-parse";
            linkGitRevParse.LinkClicked += linkGitRevParse_LinkClicked;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(19, 22);
            label2.Name = "label2";
            label2.Size = new Size(413, 75);
            label2.TabIndex = 0;
            label2.Text = "Commit expression examples:\r\n- complete commit hash: e. g.: 8eab51fcb9c4538eb74c4" +
    "dcd4c31ffd693ad25c9\r\n- partial commit hash (if unique): e. g.: 8eab51fcb9c453\r\n-" +
    " tag name\r\n- branch name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 219);
            label3.Name = "label3";
            label3.Size = new Size(59, 15);
            label3.TabIndex = 4;
            label3.Text = "Go to tag:";
            // 
            // comboBoxTags
            // 
            comboBoxTags.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxTags.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBoxTags.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxTags.FormattingEnabled = true;
            comboBoxTags.Location = new Point(155, 216);
            comboBoxTags.Name = "comboBoxTags";
            comboBoxTags.Size = new Size(287, 23);
            comboBoxTags.TabIndex = 1;
            comboBoxTags.SelectionChangeCommitted += comboBoxTags_SelectionChangeCommitted;
            comboBoxTags.TextChanged += comboBoxTags_TextChanged;
            comboBoxTags.Enter += comboBoxTags_Enter;
            comboBoxTags.KeyUp += comboBoxTags_KeyUp;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 261);
            label4.Name = "label4";
            label4.Size = new Size(79, 15);
            label4.TabIndex = 5;
            label4.Text = "Go to branch:";
            // 
            // comboBoxBranches
            // 
            comboBoxBranches.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxBranches.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBoxBranches.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxBranches.FormattingEnabled = true;
            comboBoxBranches.Location = new Point(155, 258);
            comboBoxBranches.Name = "comboBoxBranches";
            comboBoxBranches.Size = new Size(287, 23);
            comboBoxBranches.TabIndex = 2;
            comboBoxBranches.SelectionChangeCommitted += comboBoxBranches_SelectionChangeCommitted;
            comboBoxBranches.TextChanged += comboBoxBranches_TextChanged;
            comboBoxBranches.Enter += comboBoxBranches_Enter;
            comboBoxBranches.KeyUp += comboBoxBranches_KeyUp;
            // 
            // FormGoToCommit
            // 
            AcceptButton = goButton;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(604, 302);
            Controls.Add(comboBoxBranches);
            Controls.Add(label4);
            Controls.Add(comboBoxTags);
            Controls.Add(label3);
            Controls.Add(groupBox1);
            Controls.Add(label1);
            Controls.Add(textboxCommitExpression);
            Controls.Add(goButton);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(620, 340);
            Name = "FormGoToCommit";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Go to commit";
            Load += FormGoToCommit_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button goButton;
        private TextBox textboxCommitExpression;
        private Label label1;
        private GroupBox groupBox1;
        private Label label2;
        private LinkLabel linkGitRevParse;
        private Label label3;
        private ComboBox comboBoxTags;
        private Label label4;
        private ComboBox comboBoxBranches;
    }
}