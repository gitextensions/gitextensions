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
            this.checkBoxRebaseAutostash = new System.Windows.Forms.CheckBox();
            this.checkBoxFetchPrune = new System.Windows.Forms.CheckBox();
            this.checkBoxPullRebase = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxRebaseAutostash
            // 
            this.checkBoxRebaseAutostash.AutoSize = true;
            this.checkBoxRebaseAutostash.Location = new System.Drawing.Point(19, 80);
            this.checkBoxRebaseAutostash.Name = "checkBoxRebaseAutostash";
            this.checkBoxRebaseAutostash.Size = new System.Drawing.Size(228, 17);
            this.checkBoxRebaseAutostash.TabIndex = 1;
            this.checkBoxRebaseAutostash.Text = "Automatically stash before doing a rebase";
            this.checkBoxRebaseAutostash.UseVisualStyleBackColor = true;
            // 
            // checkBoxFetchPrune
            // 
            this.checkBoxFetchPrune.AutoSize = true;
            this.checkBoxFetchPrune.Location = new System.Drawing.Point(19, 47);
            this.checkBoxFetchPrune.Name = "checkBoxFetchPrune";
            this.checkBoxFetchPrune.Size = new System.Drawing.Size(199, 17);
            this.checkBoxFetchPrune.TabIndex = 2;
            this.checkBoxFetchPrune.Text = "Prune remote branches during fetch";
            this.checkBoxFetchPrune.UseVisualStyleBackColor = true;
            // 
            // checkBoxPullRebase
            // 
            this.checkBoxPullRebase.AutoSize = true;
            this.checkBoxPullRebase.Location = new System.Drawing.Point(19, 14);
            this.checkBoxPullRebase.Name = "checkBoxPullRebase";
            this.checkBoxPullRebase.Size = new System.Drawing.Size(276, 17);
            this.checkBoxPullRebase.TabIndex = 3;
            this.checkBoxPullRebase.Text = "Rebase local branch when pulling (instead of merge)";
            this.checkBoxPullRebase.UseVisualStyleBackColor = true;
            // 
            // GitConfigAdvancedSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.checkBoxRebaseAutostash);
            this.Controls.Add(this.checkBoxFetchPrune);
            this.Controls.Add(this.checkBoxPullRebase);
            this.Name = "GitConfigAdvancedSettingsPage";
            this.Size = new System.Drawing.Size(1211, 542);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxRebaseAutostash;
        private System.Windows.Forms.CheckBox checkBoxFetchPrune;
        private System.Windows.Forms.CheckBox checkBoxPullRebase;
    }
}
