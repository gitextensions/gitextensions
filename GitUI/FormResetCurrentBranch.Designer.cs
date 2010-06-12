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
            this._BranchInfo = new System.Windows.Forms.Label();
            this._Commit = new System.Windows.Forms.Label();
            this._Author = new System.Windows.Forms.Label();
            this._Message = new System.Windows.Forms.Label();
            this._Date = new System.Windows.Forms.Label();
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
            this._BranchInfo.AutoSize = true;
            this._BranchInfo.Location = new System.Drawing.Point(13, 13);
            this._BranchInfo.Name = "BranchInfo";
            this._BranchInfo.Size = new System.Drawing.Size(81, 13);
            this._BranchInfo.TabIndex = 0;
            this._BranchInfo.Text = "##Reset {0} to:";
            // 
            // Commit
            // 
            this._Commit.AutoSize = true;
            this._Commit.Location = new System.Drawing.Point(31, 34);
            this._Commit.Name = "Commit";
            this._Commit.Size = new System.Drawing.Size(75, 13);
            this._Commit.TabIndex = 1;
            this._Commit.Text = "##Commit: {0}";
            // 
            // Author
            // 
            this._Author.AutoSize = true;
            this._Author.Location = new System.Drawing.Point(31, 57);
            this._Author.Name = "Author";
            this._Author.Size = new System.Drawing.Size(72, 13);
            this._Author.TabIndex = 2;
            this._Author.Text = "##Author: {0}";
            // 
            // Message
            // 
            this._Message.AutoSize = true;
            this._Message.Location = new System.Drawing.Point(31, 106);
            this._Message.Name = "Message";
            this._Message.Size = new System.Drawing.Size(84, 13);
            this._Message.TabIndex = 3;
            this._Message.Text = "##Message: {0}";
            // 
            // Date
            // 
            this._Date.AutoSize = true;
            this._Date.Location = new System.Drawing.Point(31, 82);
            this._Date.Name = "Date";
            this._Date.Size = new System.Drawing.Size(99, 13);
            this._Date.TabIndex = 4;
            this._Date.Text = "##Commit date: {0}";
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
            this.Controls.Add(this._Date);
            this.Controls.Add(this._Message);
            this.Controls.Add(this._Author);
            this.Controls.Add(this._Commit);
            this.Controls.Add(this._BranchInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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

        private System.Windows.Forms.Label _BranchInfo;
        private System.Windows.Forms.Label _Commit;
        private System.Windows.Forms.Label _Author;
        private System.Windows.Forms.Label _Message;
        private System.Windows.Forms.Label _Date;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Hard;
        private System.Windows.Forms.RadioButton Mixed;
        private System.Windows.Forms.RadioButton Soft;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
    }
}