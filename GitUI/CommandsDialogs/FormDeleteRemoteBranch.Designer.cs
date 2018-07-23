namespace GitUI.CommandsDialogs
{
    partial class FormDeleteRemoteBranch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDeleteRemoteBranch));
            this.Delete = new System.Windows.Forms.Button();
            this.labelSelectBranches = new System.Windows.Forms.Label();
            this.labelDeleteBranchWarning = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.DeleteRemote = new System.Windows.Forms.CheckBox();
            this.Branches = new GitUI.BranchComboBox();
            this.gotoUserManualControl1 = new GitUI.UserControls.GotoUserManualControl();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Delete
            // 
            this.Delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Delete.Enabled = false;
            this.Delete.ForeColor = System.Drawing.Color.Black;
            this.Delete.Image = global::GitUI.Properties.Images.BranchDelete;
            this.Delete.Location = new System.Drawing.Point(553, 80);
            this.Delete.Margin = new System.Windows.Forms.Padding(4);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(150, 31);
            this.Delete.TabIndex = 4;
            this.Delete.Text = "Delete";
            this.Delete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Delete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.OkClick);
            // 
            // labelSelectBranches
            // 
            this.labelSelectBranches.AutoSize = true;
            this.labelSelectBranches.ForeColor = System.Drawing.Color.Black;
            this.labelSelectBranches.Location = new System.Drawing.Point(11, 11);
            this.labelSelectBranches.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSelectBranches.Name = "labelSelectBranches";
            this.labelSelectBranches.Size = new System.Drawing.Size(104, 17);
            this.labelSelectBranches.TabIndex = 1;
            this.labelSelectBranches.Text = "Select branches";
            // 
            // labelDeleteBranchWarning
            // 
            this.labelDeleteBranchWarning.AutoSize = true;
            this.labelDeleteBranchWarning.ForeColor = System.Drawing.Color.Black;
            this.labelDeleteBranchWarning.Location = new System.Drawing.Point(49, 145);
            this.labelDeleteBranchWarning.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDeleteBranchWarning.MaximumSize = new System.Drawing.Size(625, 0);
            this.labelDeleteBranchWarning.Name = "labelDeleteBranchWarning";
            this.labelDeleteBranchWarning.Size = new System.Drawing.Size(612, 68);
            this.labelDeleteBranchWarning.TabIndex = 5;
            this.labelDeleteBranchWarning.Text = resources.GetString("labelDeleteBranchWarning.Text");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GitUI.Properties.Images.Warning;
            this.pictureBox1.InitialImage = global::GitUI.Properties.Images.Warning;
            this.pictureBox1.Location = new System.Drawing.Point(15, 178);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(26, 25);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // DeleteRemote
            // 
            this.DeleteRemote.AutoSize = true;
            this.DeleteRemote.Location = new System.Drawing.Point(15, 86);
            this.DeleteRemote.Margin = new System.Windows.Forms.Padding(4);
            this.DeleteRemote.Name = "DeleteRemote";
            this.DeleteRemote.Size = new System.Drawing.Size(176, 21);
            this.DeleteRemote.TabIndex = 3;
            this.DeleteRemote.Text = "Delete branche(s) from remote repository";
            this.DeleteRemote.UseVisualStyleBackColor = true;
            this.DeleteRemote.CheckedChanged += new System.EventHandler(this.DeleteRemote_CheckedChanged);
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.BranchesToSelect = null;
            this.Branches.Location = new System.Drawing.Point(15, 44);
            this.Branches.Margin = new System.Windows.Forms.Padding(0);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(688, 26);
            this.Branches.TabIndex = 2;
            // 
            // gotoUserManualControl1
            // 
            this.gotoUserManualControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gotoUserManualControl1.AutoSize = true;
            this.gotoUserManualControl1.Location = new System.Drawing.Point(12, 264);
            this.gotoUserManualControl1.ManualSectionAnchorName = "delete-branch";
            this.gotoUserManualControl1.ManualSectionSubfolder = "branches";
            this.gotoUserManualControl1.Margin = new System.Windows.Forms.Padding(4);
            this.gotoUserManualControl1.MinimumSize = new System.Drawing.Size(88, 25);
            this.gotoUserManualControl1.Name = "gotoUserManualControl1";
            this.gotoUserManualControl1.Size = new System.Drawing.Size(88, 25);
            this.gotoUserManualControl1.TabIndex = 8;
            // 
            // FormDeleteRemoteBranch
            // 
            this.AcceptButton = this.Delete;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(724, 295);
            this.Controls.Add(this.gotoUserManualControl1);
            this.Controls.Add(this.Branches);
            this.Controls.Add(this.DeleteRemote);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelDeleteBranchWarning);
            this.Controls.Add(this.Delete);
            this.Controls.Add(this.labelSelectBranches);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(652, 326);
            this.Name = "FormDeleteRemoteBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Delete branch";
            this.Load += new System.EventHandler(this.FormDeleteRemoteBranchLoad);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Label labelSelectBranches;
        private System.Windows.Forms.Label labelDeleteBranchWarning;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox DeleteRemote;
        private BranchComboBox Branches;
        private UserControls.GotoUserManualControl gotoUserManualControl1;
    }
}