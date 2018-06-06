namespace GitUI.UserControls.RevisionGridClasses
{
    partial class NotAGitRepoControl
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
            this.InitRepository = new System.Windows.Forms.Button();
            this.CloneRepository = new System.Windows.Forms.Button();
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // InitRepository
            //
            this.InitRepository.Location = new System.Drawing.Point(22, 48);
            this.InitRepository.Name = "InitRepository";
            this.InitRepository.Size = new System.Drawing.Size(195, 31);
            this.InitRepository.TabIndex = 2;
            this.InitRepository.Text = "Initialize repository";
            this.InitRepository.UseVisualStyleBackColor = true;
            //
            // CloneRepository
            //
            this.CloneRepository.Location = new System.Drawing.Point(224, 48);
            this.CloneRepository.Name = "CloneRepository";
            this.CloneRepository.Size = new System.Drawing.Size(195, 31);
            this.CloneRepository.TabIndex = 3;
            this.CloneRepository.Text = "Clone repository";
            this.CloneRepository.UseVisualStyleBackColor = true;
            this.CloneRepository.Visible = false;
            //
            // label2
            //
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(15, 10);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(247, 13);
            this.label.TabIndex = 1;
            this.label.Text = "The current working directory is not a git repository.";
            //
            // NotAGitRepoControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.InitRepository);
            this.Controls.Add(this.CloneRepository);
            this.Controls.Add(this.label);
            this.Name = "NotAGitRepoControl";
            this.Size = new System.Drawing.Size(682, 235);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button InitRepository;
        private System.Windows.Forms.Button CloneRepository;
        private System.Windows.Forms.Label label;
    }
}
