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
            this._titleTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._bodyTB = new GitUI.SpellChecker.EditNetSpell();
            this._pullReqTargetsCB = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this._createBtn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this._yourBranchesCB = new System.Windows.Forms.ComboBox();
            this._remoteBranchesCB = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _titleTB
            // 
            this._titleTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._titleTB.Location = new System.Drawing.Point(58, 19);
            this._titleTB.Name = "_titleTB";
            this._titleTB.Size = new System.Drawing.Size(462, 23);
            this._titleTB.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Title:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Body:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._bodyTB);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._titleTB);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 100);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(526, 175);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pull request data";
            // 
            // _bodyTB
            // 
            this._bodyTB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bodyTB.Location = new System.Drawing.Point(58, 45);
            this._bodyTB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._bodyTB.Name = "_bodyTB";
            this._bodyTB.Size = new System.Drawing.Size(462, 124);
            this._bodyTB.TabIndex = 1;
            this._bodyTB.WatermarkText = "";
            // 
            // _pullReqTargetsCB
            // 
            this._pullReqTargetsCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._pullReqTargetsCB.FormattingEnabled = true;
            this._pullReqTargetsCB.Location = new System.Drawing.Point(141, 12);
            this._pullReqTargetsCB.Name = "_pullReqTargetsCB";
            this._pullReqTargetsCB.Size = new System.Drawing.Size(246, 23);
            this._pullReqTargetsCB.TabIndex = 3;
            this._pullReqTargetsCB.SelectedIndexChanged += new System.EventHandler(this._pullReqTargetsCB_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Target repository:";
            // 
            // _createBtn
            // 
            this._createBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._createBtn.Location = new System.Drawing.Point(426, 281);
            this._createBtn.Name = "_createBtn";
            this._createBtn.Size = new System.Drawing.Size(112, 33);
            this._createBtn.TabIndex = 2;
            this._createBtn.Text = "Create";
            this._createBtn.UseVisualStyleBackColor = true;
            this._createBtn.Click += new System.EventHandler(this._createBtn_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "Your branch:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "Target branch:";
            // 
            // _yourBranchesCB
            // 
            this._yourBranchesCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._yourBranchesCB.FormattingEnabled = true;
            this._yourBranchesCB.Location = new System.Drawing.Point(141, 39);
            this._yourBranchesCB.Name = "_yourBranchesCB";
            this._yourBranchesCB.Size = new System.Drawing.Size(246, 23);
            this._yourBranchesCB.TabIndex = 0;
            this._yourBranchesCB.SelectedIndexChanged += new System.EventHandler(this._yourBranchCB_SelectedIndexChanged);
            // 
            // _remoteBranchesCB
            // 
            this._remoteBranchesCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._remoteBranchesCB.FormattingEnabled = true;
            this._remoteBranchesCB.Location = new System.Drawing.Point(141, 66);
            this._remoteBranchesCB.Name = "_remoteBranchesCB";
            this._remoteBranchesCB.Size = new System.Drawing.Size(246, 23);
            this._remoteBranchesCB.TabIndex = 1;
            this._remoteBranchesCB.SelectedIndexChanged += new System.EventHandler(this._yourBranchCB_SelectedIndexChanged);
            // 
            // CreatePullRequestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(546, 323);
            this.Controls.Add(this._remoteBranchesCB);
            this.Controls.Add(this._yourBranchesCB);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._createBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._pullReqTargetsCB);
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(552, 345);
            this.Name = "CreatePullRequestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Pull Request";
            this.Load += new System.EventHandler(this.CreatePullRequestForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _titleTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private GitUI.SpellChecker.EditNetSpell _bodyTB;
        private System.Windows.Forms.ComboBox _pullReqTargetsCB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button _createBtn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox _yourBranchesCB;
        private System.Windows.Forms.ComboBox _remoteBranchesCB;
    }
}