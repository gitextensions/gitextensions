namespace GitUI.UserControls.RevisionGrid
{
    partial class EmptyRepoControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components is not null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            btnEditGitIgnore = new Button();
            btnOpenCommitForm = new Button();
            lblEmptyRepository = new Label();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnEditGitIgnore
            // 
            btnEditGitIgnore.Location = new Point(1, 58);
            btnEditGitIgnore.Name = "btnEditGitIgnore";
            btnEditGitIgnore.Size = new Size(134, 27);
            btnEditGitIgnore.TabIndex = 0;
            btnEditGitIgnore.Text = "Edit .gitignore";
            btnEditGitIgnore.UseVisualStyleBackColor = true;
            // 
            // btnOpenCommitForm
            // 
            btnOpenCommitForm.Location = new Point(141, 58);
            btnOpenCommitForm.Name = "btnOpenCommitForm";
            btnOpenCommitForm.Size = new Size(134, 27);
            btnOpenCommitForm.TabIndex = 1;
            btnOpenCommitForm.Text = "Commit";
            btnOpenCommitForm.UseVisualStyleBackColor = true;
            // 
            // lblEmptyRepository
            // 
            lblEmptyRepository.AutoSize = true;
            lblEmptyRepository.Location = new Point(20, 22);
            lblEmptyRepository.Name = "lblEmptyRepository";
            lblEmptyRepository.Size = new Size(246, 13);
            lblEmptyRepository.TabIndex = 2;
            lblEmptyRepository.Text = "This repository does not yet contain any commits.";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.None;
            panel1.Controls.Add(btnEditGitIgnore);
            panel1.Controls.Add(btnOpenCommitForm);
            panel1.Controls.Add(lblEmptyRepository);
            panel1.Location = new Point(274, 50);
            panel1.Name = "panel1";
            panel1.Size = new Size(277, 88);
            panel1.TabIndex = 3;
            // 
            // EmptyRepoControl
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            Controls.Add(panel1);
            Name = "EmptyRepoControl";
            Size = new Size(827, 189);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Label lblEmptyRepository;
        private Button btnEditGitIgnore;
        private Button btnOpenCommitForm;
        private Panel panel1;
    }
}
