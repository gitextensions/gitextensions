namespace GitUI.UserControls.RevisionGrid
{
    partial class EmptyRepoControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.btnEditGitIgnore = new System.Windows.Forms.Button();
            this.btnOpenCommitForm = new System.Windows.Forms.Button();
            this.lblEmptyRepository = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnEditGitIgnore
            // 
            this.btnEditGitIgnore.Location = new System.Drawing.Point(1, 58);
            this.btnEditGitIgnore.Name = "btnEditGitIgnore";
            this.btnEditGitIgnore.Size = new System.Drawing.Size(134, 27);
            this.btnEditGitIgnore.TabIndex = 0;
            this.btnEditGitIgnore.Text = "Edit .gitignore";
            this.btnEditGitIgnore.UseVisualStyleBackColor = true;
            // 
            // btnOpenCommitForm
            // 
            this.btnOpenCommitForm.Location = new System.Drawing.Point(141, 58);
            this.btnOpenCommitForm.Name = "btnOpenCommitForm";
            this.btnOpenCommitForm.Size = new System.Drawing.Size(134, 27);
            this.btnOpenCommitForm.TabIndex = 1;
            this.btnOpenCommitForm.Text = "Commit";
            this.btnOpenCommitForm.UseVisualStyleBackColor = true;
            // 
            // lblEmptyRepository
            // 
            this.lblEmptyRepository.AutoSize = true;
            this.lblEmptyRepository.Location = new System.Drawing.Point(20, 22);
            this.lblEmptyRepository.Name = "lblEmptyRepository";
            this.lblEmptyRepository.Size = new System.Drawing.Size(246, 13);
            this.lblEmptyRepository.TabIndex = 2;
            this.lblEmptyRepository.Text = "This repository does not yet contain any commits.";
            // 
            // panel1
            // 
            this.panel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panel1.Controls.Add(this.btnEditGitIgnore);
            this.panel1.Controls.Add(this.btnOpenCommitForm);
            this.panel1.Controls.Add(this.lblEmptyRepository);
            this.panel1.Location = new System.Drawing.Point(274, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(277, 88);
            this.panel1.TabIndex = 3;
            // 
            // EmptyRepoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.panel1);
            this.Name = "EmptyRepoControl";
            this.Size = new System.Drawing.Size(827, 189);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblEmptyRepository;
        private System.Windows.Forms.Button btnEditGitIgnore;
        private System.Windows.Forms.Button btnOpenCommitForm;
        private System.Windows.Forms.Panel panel1;
    }
}
