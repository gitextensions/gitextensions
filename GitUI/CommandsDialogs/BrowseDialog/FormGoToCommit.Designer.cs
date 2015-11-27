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
            this.goButton = new System.Windows.Forms.Button();
            this.textboxCommitExpression = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkGitRevParse = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxTags = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxBranches = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // goButton
            // 
            this.goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.goButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.goButton.Location = new System.Drawing.Point(521, 10);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(75, 28);
            this.goButton.TabIndex = 3;
            this.goButton.Text = "Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // textboxCommitExpression
            // 
            this.textboxCommitExpression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxCommitExpression.Location = new System.Drawing.Point(155, 13);
            this.textboxCommitExpression.Name = "textboxCommitExpression";
            this.textboxCommitExpression.Size = new System.Drawing.Size(360, 23);
            this.textboxCommitExpression.TabIndex = 0;
            this.textboxCommitExpression.TextChanged += new System.EventHandler(this.commitExpression_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Commit expression:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.linkGitRevParse);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(45, 43);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 141);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Help";
            // 
            // linkGitRevParse
            // 
            this.linkGitRevParse.AutoSize = true;
            this.linkGitRevParse.Location = new System.Drawing.Point(19, 112);
            this.linkGitRevParse.Name = "linkGitRevParse";
            this.linkGitRevParse.Size = new System.Drawing.Size(126, 15);
            this.linkGitRevParse.TabIndex = 0;
            this.linkGitRevParse.TabStop = true;
            this.linkGitRevParse.Text = "More see git-rev-parse";
            this.linkGitRevParse.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkGitRevParse_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(413, 75);
            this.label2.TabIndex = 0;
            this.label2.Text = "Commit expression examples:\r\n- complete commit hash: e. g.: 8eab51fcb9c4538eb74c4" +
    "dcd4c31ffd693ad25c9\r\n- partial commit hash (if unique): e. g.: 8eab51fcb9c453\r\n-" +
    " tag name\r\n- branch name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 219);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Go to tag:";
            // 
            // comboBoxTags
            // 
            this.comboBoxTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxTags.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxTags.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxTags.FormattingEnabled = true;
            this.comboBoxTags.Location = new System.Drawing.Point(155, 216);
            this.comboBoxTags.Name = "comboBoxTags";
            this.comboBoxTags.Size = new System.Drawing.Size(287, 23);
            this.comboBoxTags.TabIndex = 1;
            this.comboBoxTags.SelectionChangeCommitted += new System.EventHandler(this.comboBoxTags_SelectionChangeCommitted);
            this.comboBoxTags.TextChanged += new System.EventHandler(this.comboBoxTags_TextChanged);
            this.comboBoxTags.Enter += new System.EventHandler(this.comboBoxTags_Enter);
            this.comboBoxTags.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBoxTags_KeyUp);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 261);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Go to branch:";
            // 
            // comboBoxBranches
            // 
            this.comboBoxBranches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxBranches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxBranches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxBranches.FormattingEnabled = true;
            this.comboBoxBranches.Location = new System.Drawing.Point(155, 258);
            this.comboBoxBranches.Name = "comboBoxBranches";
            this.comboBoxBranches.Size = new System.Drawing.Size(287, 23);
            this.comboBoxBranches.TabIndex = 2;
            this.comboBoxBranches.SelectionChangeCommitted += new System.EventHandler(this.comboBoxBranches_SelectionChangeCommitted);
            this.comboBoxBranches.TextChanged += new System.EventHandler(this.comboBoxBranches_TextChanged);
            this.comboBoxBranches.Enter += new System.EventHandler(this.comboBoxBranches_Enter);
            this.comboBoxBranches.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBoxBranches_KeyUp);
            // 
            // FormGoToCommit
            // 
            this.AcceptButton = this.goButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(604, 302);
            this.Controls.Add(this.comboBoxBranches);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxTags);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textboxCommitExpression);
            this.Controls.Add(this.goButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 340);
            this.Name = "FormGoToCommit";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Go to commit";
            this.Load += new System.EventHandler(this.FormGoToCommit_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.TextBox textboxCommitExpression;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkGitRevParse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxTags;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxBranches;
    }
}