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
            System.Windows.Forms.Panel panel1;
            this.labelPreview = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblMenuEntries = new System.Windows.Forms.Label();
            this.chlMenuEntries = new System.Windows.Forms.CheckedListBox();
            this.cbAlwaysShowAllCommands = new System.Windows.Forms.CheckBox();
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbShellExtensions = new System.Windows.Forms.GroupBox();
            this.tlpnlCascadingMenu = new System.Windows.Forms.TableLayoutPanel();
            this.chlExtendedContextMenuEntries = new System.Windows.Forms.CheckedListBox();
            this.lblExtendedContextMenuEntries = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            this.tlpnlMain.SuspendLayout();
            this.gbShellExtensions.SuspendLayout();
            this.tlpnlCascadingMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(this.labelPreview);
            panel1.Controls.Add(this.label1);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(578, 99);
            panel1.Margin = new System.Windows.Forms.Padding(2);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(4);
            this.tlpnlCascadingMenu.SetRowSpan(panel1, 4);
            panel1.Size = new System.Drawing.Size(572, 522);
            panel1.TabIndex = 0;
            // 
            // labelPreview
            // 
            this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPreview.Location = new System.Drawing.Point(4, 24);
            this.labelPreview.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(564, 494);
            this.labelPreview.TabIndex = 1;
            this.labelPreview.Text = "...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Context menu preview:";
            // 
            // lblMenuEntries
            // 
            this.lblMenuEntries.AutoSize = true;
            this.tlpnlCascadingMenu.SetColumnSpan(this.lblMenuEntries, 2);
            this.lblMenuEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMenuEntries.Location = new System.Drawing.Point(4, 57);
            this.lblMenuEntries.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMenuEntries.Name = "lblMenuEntries";
            this.lblMenuEntries.Size = new System.Drawing.Size(1144, 40);
            this.lblMenuEntries.TabIndex = 2;
            this.lblMenuEntries.Text = "Select items to be shown in the cascaded context menu.\r\n(Unchecked items will be " +
    "shown top level for direct access.)";
            // 
            // chlMenuEntries
            // 
            this.chlMenuEntries.CheckOnClick = true;
            this.chlMenuEntries.Dock = System.Windows.Forms.DockStyle.Fill;
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
            "Pull/Fetch...",
            "Push...",
            "Reset file changes..",
            "Revert",
            "Settings",
            "View stash",
            "View changes"});
            this.chlMenuEntries.Location = new System.Drawing.Point(4, 101);
            this.chlMenuEntries.Margin = new System.Windows.Forms.Padding(4);
            this.chlMenuEntries.Name = "chlMenuEntries";
            this.chlMenuEntries.Size = new System.Drawing.Size(568, 356);
            this.chlMenuEntries.TabIndex = 3;
            this.chlMenuEntries.SelectedValueChanged += new System.EventHandler(this.chlMenuEntries_SelectedValueChanged);
            // 
            // cbAlwaysShowAllCommands
            // 
            this.cbAlwaysShowAllCommands.AutoSize = true;
            this.tlpnlCascadingMenu.SetColumnSpan(this.cbAlwaysShowAllCommands, 2);
            this.cbAlwaysShowAllCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbAlwaysShowAllCommands.Location = new System.Drawing.Point(4, 4);
            this.cbAlwaysShowAllCommands.Margin = new System.Windows.Forms.Padding(4);
            this.cbAlwaysShowAllCommands.Name = "cbAlwaysShowAllCommands";
            this.cbAlwaysShowAllCommands.Size = new System.Drawing.Size(1144, 24);
            this.cbAlwaysShowAllCommands.TabIndex = 1;
            this.cbAlwaysShowAllCommands.Text = "Always show all commands";
            this.cbAlwaysShowAllCommands.UseVisualStyleBackColor = true;
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.AutoSize = true;
            this.tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlMain.ColumnCount = 1;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlMain.Controls.Add(this.gbShellExtensions, 0, 0);
            this.tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlMain.Location = new System.Drawing.Point(10, 10);
            this.tlpnlMain.Margin = new System.Windows.Forms.Padding(4);
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 1;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 671F));
            this.tlpnlMain.Size = new System.Drawing.Size(1180, 671);
            this.tlpnlMain.TabIndex = 0;
            // 
            // gbShellExtensions
            // 
            this.gbShellExtensions.AutoSize = true;
            this.gbShellExtensions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbShellExtensions.Controls.Add(this.tlpnlCascadingMenu);
            this.gbShellExtensions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbShellExtensions.Location = new System.Drawing.Point(4, 4);
            this.gbShellExtensions.Margin = new System.Windows.Forms.Padding(4);
            this.gbShellExtensions.Name = "gbShellExtensions";
            this.gbShellExtensions.Padding = new System.Windows.Forms.Padding(10);
            this.gbShellExtensions.Size = new System.Drawing.Size(1172, 663);
            this.gbShellExtensions.TabIndex = 0;
            this.gbShellExtensions.TabStop = false;
            this.gbShellExtensions.Text = "Cascaded Context Menu";
            // 
            // tlpnlCascadingMenu
            // 
            this.tlpnlCascadingMenu.AutoSize = true;
            this.tlpnlCascadingMenu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlCascadingMenu.ColumnCount = 2;
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpnlCascadingMenu.Controls.Add(this.chlExtendedContextMenuEntries, 0, 6);
            this.tlpnlCascadingMenu.Controls.Add(this.cbAlwaysShowAllCommands, 0, 0);
            this.tlpnlCascadingMenu.Controls.Add(this.lblMenuEntries, 0, 2);
            this.tlpnlCascadingMenu.Controls.Add(panel1, 1, 3);
            this.tlpnlCascadingMenu.Controls.Add(this.chlMenuEntries, 0, 3);
            this.tlpnlCascadingMenu.Controls.Add(this.lblExtendedContextMenuEntries, 0, 4);
            this.tlpnlCascadingMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlCascadingMenu.Location = new System.Drawing.Point(10, 30);
            this.tlpnlCascadingMenu.Margin = new System.Windows.Forms.Padding(4);
            this.tlpnlCascadingMenu.Name = "tlpnlCascadingMenu";
            this.tlpnlCascadingMenu.RowCount = 7;
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 245F));
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlCascadingMenu.Size = new System.Drawing.Size(1152, 623);
            this.tlpnlCascadingMenu.TabIndex = 0;
            // 
            // chlExtendedContextMenuEntries
            // 
            this.chlExtendedContextMenuEntries.CheckOnClick = true;
            this.chlExtendedContextMenuEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chlExtendedContextMenuEntries.FormattingEnabled = true;
            this.chlExtendedContextMenuEntries.Items.AddRange(new object[] {
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
            "Pull/Fetch...",
            "Push...",
            "Reset file changes..",
            "Revert",
            "Settings",
            "View stash",
            "View changes"});
            this.chlExtendedContextMenuEntries.Location = new System.Drawing.Point(4, 505);
            this.chlExtendedContextMenuEntries.Margin = new System.Windows.Forms.Padding(4);
            this.chlExtendedContextMenuEntries.Name = "chlExtendedContextMenuEntries";
            this.chlExtendedContextMenuEntries.Size = new System.Drawing.Size(568, 114);
            this.chlExtendedContextMenuEntries.TabIndex = 5;
            // 
            // lblExtendedContextMenuEntries
            // 
            this.lblExtendedContextMenuEntries.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblExtendedContextMenuEntries.AutoSize = true;
            this.lblExtendedContextMenuEntries.Location = new System.Drawing.Point(3, 461);
            this.lblExtendedContextMenuEntries.Name = "lblExtendedContextMenuEntries";
            this.lblExtendedContextMenuEntries.Size = new System.Drawing.Size(375, 40);
            this.lblExtendedContextMenuEntries.TabIndex = 6;
            this.lblExtendedContextMenuEntries.Text = "Select items to be shown in the extended context menu\r\n(accessible in File Explor" +
    "er via Shift + Right-Click).";
            // 
            // ShellExtensionSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tlpnlMain);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ShellExtensionSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(1200, 691);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.gbShellExtensions.ResumeLayout(false);
            this.gbShellExtensions.PerformLayout();
            this.tlpnlCascadingMenu.ResumeLayout(false);
            this.tlpnlCascadingMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMenuEntries;
        private System.Windows.Forms.CheckedListBox chlMenuEntries;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.CheckBox cbAlwaysShowAllCommands;
        private System.Windows.Forms.TableLayoutPanel tlpnlCascadingMenu;
        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
        private System.Windows.Forms.GroupBox gbShellExtensions;
        private System.Windows.Forms.CheckedListBox chlExtendedContextMenuEntries;
        private System.Windows.Forms.Label lblExtendedContextMenuEntries;
    }
}
