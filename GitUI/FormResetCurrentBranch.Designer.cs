namespace GitUI
{
    partial class FormResetCurrentBranch
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormResetCurrentBranch));
            this.BranchInfo = new System.Windows.Forms.Label();
            this.Commit = new System.Windows.Forms.Label();
            this.Author = new System.Windows.Forms.Label();
            this.Message = new System.Windows.Forms.Label();
            this.Date = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Hard = new System.Windows.Forms.RadioButton();
            this.Mixed = new System.Windows.Forms.RadioButton();
            this.Soft = new System.Windows.Forms.RadioButton();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BranchInfo
            // 
            this.BranchInfo.AutoSize = true;
            this.BranchInfo.Location = new System.Drawing.Point(13, 13);
            this.BranchInfo.Name = "BranchInfo";
            this.BranchInfo.Size = new System.Drawing.Size(60, 13);
            this.BranchInfo.TabIndex = 0;
            this.BranchInfo.Text = "Reset X to:";
            // 
            // Commit
            // 
            this.Commit.AutoSize = true;
            this.Commit.Location = new System.Drawing.Point(31, 34);
            this.Commit.Name = "Commit";
            this.Commit.Size = new System.Drawing.Size(44, 13);
            this.Commit.TabIndex = 1;
            this.Commit.Text = "Commit:";
            // 
            // Author
            // 
            this.Author.AutoSize = true;
            this.Author.Location = new System.Drawing.Point(31, 57);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(41, 13);
            this.Author.TabIndex = 2;
            this.Author.Text = "Author:";
            // 
            // Message
            // 
            this.Message.AutoSize = true;
            this.Message.Location = new System.Drawing.Point(31, 106);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(53, 13);
            this.Message.TabIndex = 3;
            this.Message.Text = "Message:";
            // 
            // Date
            // 
            this.Date.AutoSize = true;
            this.Date.Location = new System.Drawing.Point(31, 82);
            this.Date.Name = "Date";
            this.Date.Size = new System.Drawing.Size(68, 13);
            this.Date.TabIndex = 4;
            this.Date.Text = "Commit date:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Hard);
            this.groupBox1.Controls.Add(this.Mixed);
            this.groupBox1.Controls.Add(this.Soft);
            this.groupBox1.Location = new System.Drawing.Point(16, 133);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(475, 100);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reset type";
            // 
            // Hard
            // 
            this.Hard.AutoSize = true;
            this.Hard.Location = new System.Drawing.Point(7, 68);
            this.Hard.Name = "Hard";
            this.Hard.Size = new System.Drawing.Size(451, 17);
            this.Hard.TabIndex = 2;
            this.Hard.Text = "Hard: reset working dir and index (discard ALL local changes, even uncommitted ch" +
                "anges)";
            this.Hard.UseVisualStyleBackColor = true;
            // 
            // Mixed
            // 
            this.Mixed.AutoSize = true;
            this.Mixed.Checked = true;
            this.Mixed.Location = new System.Drawing.Point(7, 44);
            this.Mixed.Name = "Mixed";
            this.Mixed.Size = new System.Drawing.Size(250, 17);
            this.Mixed.TabIndex = 1;
            this.Mixed.TabStop = true;
            this.Mixed.Text = "Mixed: leave working dir untouched, reset index";
            this.Mixed.UseVisualStyleBackColor = true;
            // 
            // Soft
            // 
            this.Soft.AutoSize = true;
            this.Soft.Location = new System.Drawing.Point(7, 20);
            this.Soft.Name = "Soft";
            this.Soft.Size = new System.Drawing.Size(233, 17);
            this.Soft.TabIndex = 0;
            this.Soft.Text = "Soft: leave working dir and index untouched";
            this.Soft.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(335, 239);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(416, 239);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 7;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // FormResetCurrentBranch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 269);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Date);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.Author);
            this.Controls.Add(this.Commit);
            this.Controls.Add(this.BranchInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormResetCurrentBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reset current branch";
            this.Load += new System.EventHandler(this.FormResetCurrentBranch_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BranchInfo;
        private System.Windows.Forms.Label Commit;
        private System.Windows.Forms.Label Author;
        private System.Windows.Forms.Label Message;
        private System.Windows.Forms.Label Date;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Hard;
        private System.Windows.Forms.RadioButton Mixed;
        private System.Windows.Forms.RadioButton Soft;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
    }
}