namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class LocalSettingsSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label61 = new System.Windows.Forms.Label();
            this.Local_FilesEncoding = new System.Windows.Forms.ComboBox();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.localAutoCrlfFalse = new System.Windows.Forms.RadioButton();
            this.localAutoCrlfInput = new System.Windows.Forms.RadioButton();
            this.localAutoCrlfTrue = new System.Windows.Forms.RadioButton();
            this.label30 = new System.Windows.Forms.Label();
            this.InvalidGitPathLocal = new System.Windows.Forms.Panel();
            this.label21 = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.NoGitRepo = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.LocalMergeTool = new System.Windows.Forms.ComboBox();
            this.KeepMergeBackup = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.Editor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.UserEmail = new System.Windows.Forms.TextBox();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox10.SuspendLayout();
            this.InvalidGitPathLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Location = new System.Drawing.Point(9, 258);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(127, 15);
            this.label61.TabIndex = 61;
            this.label61.Text = "Files content encoding";
            // 
            // Local_FilesEncoding
            // 
            this.Local_FilesEncoding.FormattingEnabled = true;
            this.Local_FilesEncoding.Location = new System.Drawing.Point(173, 255);
            this.Local_FilesEncoding.Name = "Local_FilesEncoding";
            this.Local_FilesEncoding.Size = new System.Drawing.Size(262, 23);
            this.Local_FilesEncoding.TabIndex = 60;
            // 
            // groupBox10
            // 
            this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox10.Controls.Add(this.localAutoCrlfFalse);
            this.groupBox10.Controls.Add(this.localAutoCrlfInput);
            this.groupBox10.Controls.Add(this.localAutoCrlfTrue);
            this.groupBox10.Location = new System.Drawing.Point(8, 144);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(696, 105);
            this.groupBox10.TabIndex = 59;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Line endings";
            // 
            // localAutoCrlfFalse
            // 
            this.localAutoCrlfFalse.AutoSize = true;
            this.localAutoCrlfFalse.Location = new System.Drawing.Point(5, 74);
            this.localAutoCrlfFalse.Name = "localAutoCrlfFalse";
            this.localAutoCrlfFalse.Size = new System.Drawing.Size(349, 19);
            this.localAutoCrlfFalse.TabIndex = 2;
            this.localAutoCrlfFalse.TabStop = true;
            this.localAutoCrlfFalse.Text = "Checkout as-is, commit as-is (\"core.autocrlf\"  is set to \"false\")";
            this.localAutoCrlfFalse.UseVisualStyleBackColor = true;
            // 
            // localAutoCrlfInput
            // 
            this.localAutoCrlfInput.AutoSize = true;
            this.localAutoCrlfInput.Location = new System.Drawing.Point(5, 48);
            this.localAutoCrlfInput.Name = "localAutoCrlfInput";
            this.localAutoCrlfInput.Size = new System.Drawing.Size(448, 19);
            this.localAutoCrlfInput.TabIndex = 1;
            this.localAutoCrlfInput.TabStop = true;
            this.localAutoCrlfInput.Text = "Checkout as-is, commit Unix-style line endings (\"core.autocrlf\"  is set to \"input" +
    "\")";
            this.localAutoCrlfInput.UseVisualStyleBackColor = true;
            // 
            // localAutoCrlfTrue
            // 
            this.localAutoCrlfTrue.AutoSize = true;
            this.localAutoCrlfTrue.Location = new System.Drawing.Point(5, 22);
            this.localAutoCrlfTrue.Name = "localAutoCrlfTrue";
            this.localAutoCrlfTrue.Size = new System.Drawing.Size(495, 19);
            this.localAutoCrlfTrue.TabIndex = 0;
            this.localAutoCrlfTrue.TabStop = true;
            this.localAutoCrlfTrue.Text = "Checkout Windows-style, commit Unix-style line endings (\"core.autocrlf\"  is set t" +
    "o \"true\")";
            this.localAutoCrlfTrue.UseVisualStyleBackColor = true;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(3, 124);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(174, 15);
            this.label30.TabIndex = 58;
            this.label30.Text = "Keep backup (.orig) after merge";
            // 
            // InvalidGitPathLocal
            // 
            this.InvalidGitPathLocal.BackColor = System.Drawing.SystemColors.Info;
            this.InvalidGitPathLocal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InvalidGitPathLocal.Controls.Add(this.label21);
            this.InvalidGitPathLocal.Controls.Add(this.pictureBox3);
            this.InvalidGitPathLocal.Location = new System.Drawing.Point(433, 6);
            this.InvalidGitPathLocal.Name = "InvalidGitPathLocal";
            this.InvalidGitPathLocal.Size = new System.Drawing.Size(262, 65);
            this.InvalidGitPathLocal.TabIndex = 57;
            this.InvalidGitPathLocal.Visible = false;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(61, 7);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(193, 45);
            this.label21.TabIndex = 19;
            this.label21.Text = "You need to set the correct path to \r\ngit before you can change\r\nlocal settings.\r" +
    "\n";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::GitUI.Properties.Resources.error;
            this.pictureBox3.Location = new System.Drawing.Point(3, 4);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(54, 50);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox3.TabIndex = 18;
            this.pictureBox3.TabStop = false;
            // 
            // NoGitRepo
            // 
            this.NoGitRepo.AutoSize = true;
            this.NoGitRepo.ForeColor = System.Drawing.Color.Red;
            this.NoGitRepo.Location = new System.Drawing.Point(430, 72);
            this.NoGitRepo.Name = "NoGitRepo";
            this.NoGitRepo.Size = new System.Drawing.Size(122, 15);
            this.NoGitRepo.TabIndex = 56;
            this.NoGitRepo.Text = "Not in a git repository";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.BackColor = System.Drawing.SystemColors.Info;
            this.label20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label20.Location = new System.Drawing.Point(433, 9);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(165, 62);
            this.label20.TabIndex = 55;
            this.label20.Text = "You only need local settings\r\nif you want to override the \r\nglobal settings for t" +
    "he current\r\nrepository.";
            // 
            // LocalMergeTool
            // 
            this.LocalMergeTool.FormattingEnabled = true;
            this.LocalMergeTool.Items.AddRange(new object[] {
            "Araxis",
            "BeyondCompare3",
            "DiffMerge",
            "kdiff3",
            "p4merge",
            "TortoiseMerge"});
            this.LocalMergeTool.Location = new System.Drawing.Point(145, 91);
            this.LocalMergeTool.Name = "LocalMergeTool";
            this.LocalMergeTool.Size = new System.Drawing.Size(159, 23);
            this.LocalMergeTool.TabIndex = 54;
            // 
            // KeepMergeBackup
            // 
            this.KeepMergeBackup.AutoSize = true;
            this.KeepMergeBackup.Checked = true;
            this.KeepMergeBackup.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.KeepMergeBackup.Location = new System.Drawing.Point(467, 124);
            this.KeepMergeBackup.Name = "KeepMergeBackup";
            this.KeepMergeBackup.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.KeepMergeBackup.Size = new System.Drawing.Size(15, 14);
            this.KeepMergeBackup.TabIndex = 53;
            this.KeepMergeBackup.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 94);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(62, 15);
            this.label8.TabIndex = 52;
            this.label8.Text = "Mergetool";
            // 
            // Editor
            // 
            this.Editor.Location = new System.Drawing.Point(145, 64);
            this.Editor.Name = "Editor";
            this.Editor.Size = new System.Drawing.Size(280, 23);
            this.Editor.TabIndex = 51;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 15);
            this.label5.TabIndex = 50;
            this.label5.Text = "Editor";
            // 
            // UserEmail
            // 
            this.UserEmail.Location = new System.Drawing.Point(145, 37);
            this.UserEmail.Name = "UserEmail";
            this.UserEmail.Size = new System.Drawing.Size(280, 23);
            this.UserEmail.TabIndex = 49;
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(145, 9);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(280, 23);
            this.UserName.TabIndex = 47;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 15);
            this.label2.TabIndex = 48;
            this.label2.Text = "User email";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 15);
            this.label1.TabIndex = 46;
            this.label1.Text = "User name";
            // 
            // LocalSettingsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.label61);
            this.Controls.Add(this.Local_FilesEncoding);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.InvalidGitPathLocal);
            this.Controls.Add(this.label30);
            this.Controls.Add(this.NoGitRepo);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.LocalMergeTool);
            this.Controls.Add(this.KeepMergeBackup);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.Editor);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.UserEmail);
            this.Controls.Add(this.UserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "LocalSettingsSettingsPage";
            this.Size = new System.Drawing.Size(707, 302);
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.InvalidGitPathLocal.ResumeLayout(false);
            this.InvalidGitPathLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label61;
        private System.Windows.Forms.ComboBox Local_FilesEncoding;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.RadioButton localAutoCrlfFalse;
        private System.Windows.Forms.RadioButton localAutoCrlfInput;
        private System.Windows.Forms.RadioButton localAutoCrlfTrue;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Panel InvalidGitPathLocal;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label NoGitRepo;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.ComboBox LocalMergeTool;
        private System.Windows.Forms.CheckBox KeepMergeBackup;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox Editor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox UserEmail;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
