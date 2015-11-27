namespace GitUI.CommandsDialogs.SettingsDialog.Pages
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelPreview = new System.Windows.Forms.Label();
            this.cbAlwaysShowAllCommands = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblMenuEntries
            // 
            this.lblMenuEntries.AutoSize = true;
            this.lblMenuEntries.Location = new System.Drawing.Point(3, 15);
            this.lblMenuEntries.Name = "lblMenuEntries";
            this.lblMenuEntries.Size = new System.Drawing.Size(367, 38);
            this.lblMenuEntries.TabIndex = 5;
            this.lblMenuEntries.Text = "Select items to be shown in the cascaded context menu.\r\n(Unchecked items will be " +
    "shown top level for direct access.)";
            // 
            // chlMenuEntries
            // 
            this.chlMenuEntries.CheckOnClick = true;
            this.chlMenuEntries.FormattingEnabled = true;
            this.chlMenuEntries.Items.AddRange(new object[] {
            "Add files...",
            "Apply patch...",
            "Browse",
            "Create branch...",
            "Checkout branch...",
            "Checkout revision...",
            "Clone...",
            "Commit...",
            "Create new repository...",
            "Open with difftool",
            "File history",
            "Pull...",
            "Push...",
            "Reset file changes..",
            "Revert",
            "Settings",
            "View stash",
            "View changes"});
            this.chlMenuEntries.Location = new System.Drawing.Point(5, 60);
            this.chlMenuEntries.Name = "chlMenuEntries";
            this.chlMenuEntries.Size = new System.Drawing.Size(282, 382);
            this.chlMenuEntries.TabIndex = 4;
            this.chlMenuEntries.SelectedValueChanged += new System.EventHandler(this.chlMenuEntries_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(293, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 38);
            this.label1.TabIndex = 6;
            this.label1.Text = "Context menu preview\r\nwhen all items are shown:";
            // 
            // labelPreview
            // 
            this.labelPreview.Location = new System.Drawing.Point(306, 100);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(265, 342);
            this.labelPreview.TabIndex = 7;
            this.labelPreview.Text = "...";
            // 
            // cbAlwaysShowAllCommands
            // 
            this.cbAlwaysShowAllCommands.AutoSize = true;
            this.cbAlwaysShowAllCommands.Location = new System.Drawing.Point(21, 461);
            this.cbAlwaysShowAllCommands.Name = "cbAlwaysShowAllCommands";
            this.cbAlwaysShowAllCommands.Size = new System.Drawing.Size(194, 23);
            this.cbAlwaysShowAllCommands.TabIndex = 8;
            this.cbAlwaysShowAllCommands.Text = "Always show all commands";
            this.cbAlwaysShowAllCommands.UseVisualStyleBackColor = true;
            // 
            // ShellExtensionSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.cbAlwaysShowAllCommands);
            this.Controls.Add(this.labelPreview);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblMenuEntries);
            this.Controls.Add(this.chlMenuEntries);
            this.MinimumSize = new System.Drawing.Size(350, 320);
            this.Name = "ShellExtensionSettingsPage";
            this.Size = new System.Drawing.Size(707, 501);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMenuEntries;
        private System.Windows.Forms.CheckedListBox chlMenuEntries;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.CheckBox cbAlwaysShowAllCommands;
    }
}
