namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class GitConfigAdvancedSettingsPage
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
            if (disposing && (components is not null))
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
            this.checkBoxRebaseAutostash = new System.Windows.Forms.CheckBox();
            this.checkBoxFetchPrune = new System.Windows.Forms.CheckBox();
            this.checkBoxPullRebase = new System.Windows.Forms.CheckBox();
            this.checkBoxRebaseAutosquash = new System.Windows.Forms.CheckBox();
            this.checkBoxUpdateRefs = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxRebaseAutostash
            // 
            this.checkBoxRebaseAutostash.AutoSize = true;
            this.checkBoxRebaseAutostash.Location = new System.Drawing.Point(19, 82);
            this.checkBoxRebaseAutostash.Name = "checkBoxRebaseAutostash";
            this.checkBoxRebaseAutostash.Size = new System.Drawing.Size(247, 19);
            this.checkBoxRebaseAutostash.TabIndex = 3;
            this.checkBoxRebaseAutostash.Text = "Automatically stash before doing a rebase";
            this.checkBoxRebaseAutostash.UseVisualStyleBackColor = true;
            // 
            // checkBoxFetchPrune
            // 
            this.checkBoxFetchPrune.AutoSize = true;
            this.checkBoxFetchPrune.Location = new System.Drawing.Point(19, 48);
            this.checkBoxFetchPrune.Name = "checkBoxFetchPrune";
            this.checkBoxFetchPrune.Size = new System.Drawing.Size(217, 19);
            this.checkBoxFetchPrune.TabIndex = 2;
            this.checkBoxFetchPrune.Text = "Prune remote branches during fetch";
            this.checkBoxFetchPrune.UseVisualStyleBackColor = true;
            // 
            // checkBoxPullRebase
            // 
            this.checkBoxPullRebase.AutoSize = true;
            this.checkBoxPullRebase.Location = new System.Drawing.Point(19, 14);
            this.checkBoxPullRebase.Name = "checkBoxPullRebase";
            this.checkBoxPullRebase.Size = new System.Drawing.Size(303, 19);
            this.checkBoxPullRebase.TabIndex = 1;
            this.checkBoxPullRebase.Text = "Rebase local branch when pulling (instead of merge)";
            this.checkBoxPullRebase.UseVisualStyleBackColor = true;
            // 
            // checkBoxRebaseAutosquash
            // 
            this.checkBoxRebaseAutosquash.AutoSize = true;
            this.checkBoxRebaseAutosquash.Location = new System.Drawing.Point(19, 116);
            this.checkBoxRebaseAutosquash.Name = "checkBoxRebaseAutosquash";
            this.checkBoxRebaseAutosquash.Size = new System.Drawing.Size(367, 19);
            this.checkBoxRebaseAutosquash.TabIndex = 3;
            this.checkBoxRebaseAutosquash.Text = "Automatically squash commits when doing an interactive rebase";
            this.checkBoxRebaseAutosquash.UseVisualStyleBackColor = true;
            // 
            // checkBoxUpdateRefs
            // 
            this.checkBoxUpdateRefs.AutoSize = true;
            this.checkBoxUpdateRefs.Location = new System.Drawing.Point(19, 152);
            this.checkBoxUpdateRefs.Name = "checkBoxUpdateRefs";
            this.checkBoxUpdateRefs.Size = new System.Drawing.Size(198, 19);
            this.checkBoxUpdateRefs.TabIndex = 3;
            this.checkBoxUpdateRefs.Text = "Rebase also dependent branches";
            this.checkBoxUpdateRefs.UseVisualStyleBackColor = true;
            // 
            // GitConfigAdvancedSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.checkBoxRebaseAutostash);
            this.Controls.Add(this.checkBoxFetchPrune);
            this.Controls.Add(this.checkBoxPullRebase);
            this.Controls.Add(this.checkBoxUpdateRefs);
            this.Controls.Add(this.checkBoxRebaseAutosquash);
            this.Name = "GitConfigAdvancedSettingsPage";
            this.Size = new System.Drawing.Size(1439, 516);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxRebaseAutostash;
        private System.Windows.Forms.CheckBox checkBoxFetchPrune;
        private System.Windows.Forms.CheckBox checkBoxPullRebase;
        private System.Windows.Forms.CheckBox checkBoxRebaseAutosquash;
        private System.Windows.Forms.CheckBox checkBoxUpdateRefs;
    }
}
