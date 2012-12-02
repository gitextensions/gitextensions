namespace GitUI.SettingsDialog.Pages
{
    partial class ShellExtensionSettingsPage
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
            this.lblMenuEntries = new System.Windows.Forms.Label();
            this.chlMenuEntries = new System.Windows.Forms.CheckedListBox();
            this.chkCascadedContextMenu = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblMenuEntries
            // 
            this.lblMenuEntries.AutoSize = true;
            this.lblMenuEntries.Location = new System.Drawing.Point(3, 38);
            this.lblMenuEntries.Name = "lblMenuEntries";
            this.lblMenuEntries.Size = new System.Drawing.Size(141, 13);
            this.lblMenuEntries.TabIndex = 5;
            this.lblMenuEntries.Text = "Visible context menu entries:";
            // 
            // chlMenuEntries
            // 
            this.chlMenuEntries.CheckOnClick = true;
            this.chlMenuEntries.FormattingEnabled = true;
            this.chlMenuEntries.Items.AddRange(new object[] {
            "Add files",
            "Apply patch",
            "Browse",
            "Create branch",
            "Checkout branch",
            "Checkout revision",
            "Clone",
            "Commit",
            "File history",
            "Reset file changes",
            "Pull",
            "Push",
            "Settings",
            "View diff"});
            this.chlMenuEntries.Location = new System.Drawing.Point(5, 56);
            this.chlMenuEntries.Name = "chlMenuEntries";
            this.chlMenuEntries.Size = new System.Drawing.Size(240, 304);
            this.chlMenuEntries.TabIndex = 4;
            // 
            // chkCascadedContextMenu
            // 
            this.chkCascadedContextMenu.AutoSize = true;
            this.chkCascadedContextMenu.Location = new System.Drawing.Point(3, 3);
            this.chkCascadedContextMenu.Name = "chkCascadedContextMenu";
            this.chkCascadedContextMenu.Size = new System.Drawing.Size(141, 17);
            this.chkCascadedContextMenu.TabIndex = 3;
            this.chkCascadedContextMenu.Text = "Cascaded context menu";
            this.chkCascadedContextMenu.UseVisualStyleBackColor = true;
            // 
            // ShellExtensionSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.lblMenuEntries);
            this.Controls.Add(this.chlMenuEntries);
            this.Controls.Add(this.chkCascadedContextMenu);
            this.MinimumSize = new System.Drawing.Size(350, 320);
            this.Name = "ShellExtensionSettingsPage";
            this.Size = new System.Drawing.Size(350, 382);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMenuEntries;
        private System.Windows.Forms.CheckedListBox chlMenuEntries;
        private System.Windows.Forms.CheckBox chkCascadedContextMenu;
    }
}
