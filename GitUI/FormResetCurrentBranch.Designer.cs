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
            this._NO_TRANSLATE_BranchInfo = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Commit = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Author = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Message = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Date = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Hard = new System.Windows.Forms.RadioButton();
            this.Mixed = new System.Windows.Forms.RadioButton();
            this.Soft = new System.Windows.Forms.RadioButton();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_BranchInfo
            // 
            this._NO_TRANSLATE_BranchInfo.AutoSize = true;
            this._NO_TRANSLATE_BranchInfo.Location = new System.Drawing.Point(13, 13);
            this._NO_TRANSLATE_BranchInfo.Name = "_NO_TRANSLATE_BranchInfo";
            this._NO_TRANSLATE_BranchInfo.Size = new System.Drawing.Size(87, 13);
            this._NO_TRANSLATE_BranchInfo.TabIndex = 0;
            this._NO_TRANSLATE_BranchInfo.Text = "##Reset {0} to:";
            // 
            // _NO_TRANSLATE_Commit
            // 
            this._NO_TRANSLATE_Commit.AutoSize = true;
            this._NO_TRANSLATE_Commit.Location = new System.Drawing.Point(31, 34);
            this._NO_TRANSLATE_Commit.Name = "_NO_TRANSLATE_Commit";
            this._NO_TRANSLATE_Commit.Size = new System.Drawing.Size(81, 13);
            this._NO_TRANSLATE_Commit.TabIndex = 1;
            this._NO_TRANSLATE_Commit.Text = "##Commit: {0}";
            // 
            // _NO_TRANSLATE_Author
            // 
            this._NO_TRANSLATE_Author.AutoSize = true;
            this._NO_TRANSLATE_Author.Location = new System.Drawing.Point(31, 57);
            this._NO_TRANSLATE_Author.Name = "_NO_TRANSLATE_Author";
            this._NO_TRANSLATE_Author.Size = new System.Drawing.Size(79, 13);
            this._NO_TRANSLATE_Author.TabIndex = 2;
            this._NO_TRANSLATE_Author.Text = "##Author: {0}";
            // 
            // _NO_TRANSLATE_Message
            // 
            this._NO_TRANSLATE_Message.AutoSize = true;
            this._NO_TRANSLATE_Message.Location = new System.Drawing.Point(31, 106);
            this._NO_TRANSLATE_Message.Name = "_NO_TRANSLATE_Message";
            this._NO_TRANSLATE_Message.Size = new System.Drawing.Size(88, 13);
            this._NO_TRANSLATE_Message.TabIndex = 3;
            this._NO_TRANSLATE_Message.Text = "##Message: {0}";
            // 
            // _NO_TRANSLATE_Date
            // 
            this._NO_TRANSLATE_Date.AutoSize = true;
            this._NO_TRANSLATE_Date.Location = new System.Drawing.Point(31, 82);
            this._NO_TRANSLATE_Date.Name = "_NO_TRANSLATE_Date";
            this._NO_TRANSLATE_Date.Size = new System.Drawing.Size(106, 13);
            this._NO_TRANSLATE_Date.TabIndex = 4;
            this._NO_TRANSLATE_Date.Text = "##Commit date: {0}";
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
            this.Hard.Size = new System.Drawing.Size(455, 17);
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
            this.Mixed.Size = new System.Drawing.Size(256, 17);
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
            this.Soft.Size = new System.Drawing.Size(237, 17);
            this.Soft.TabIndex = 0;
            this.Soft.Text = "Soft: leave working dir and index untouched";
            this.Soft.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(335, 239);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 25);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(416, 239);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 25);
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
            this.Controls.Add(this._NO_TRANSLATE_Date);
            this.Controls.Add(this._NO_TRANSLATE_Message);
            this.Controls.Add(this._NO_TRANSLATE_Author);
            this.Controls.Add(this._NO_TRANSLATE_Commit);
            this.Controls.Add(this._NO_TRANSLATE_BranchInfo);
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

        private System.Windows.Forms.Label _NO_TRANSLATE_BranchInfo;
        private System.Windows.Forms.Label _NO_TRANSLATE_Commit;
        private System.Windows.Forms.Label _NO_TRANSLATE_Author;
        private System.Windows.Forms.Label _NO_TRANSLATE_Message;
        private System.Windows.Forms.Label _NO_TRANSLATE_Date;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Hard;
        private System.Windows.Forms.RadioButton Mixed;
        private System.Windows.Forms.RadioButton Soft;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
    }
}