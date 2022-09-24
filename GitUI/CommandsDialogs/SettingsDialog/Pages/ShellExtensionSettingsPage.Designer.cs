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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Panel panel1;
            this.labelPreview = new System.Windows.Forms.Label();
            this.lblMenuEntries = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_chlMenuEntries = new System.Windows.Forms.CheckedListBox();
            this.cbAlwaysShowAllCommands = new System.Windows.Forms.CheckBox();
            this.tlpnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbExplorerIntegration = new System.Windows.Forms.GroupBox();
            this.flpnlExplorerIntegration = new System.Windows.Forms.FlowLayoutPanel();
            this.RegisterButton = new System.Windows.Forms.Button();
            this.UnregisterButton = new System.Windows.Forms.Button();
            this.gbCascadingMenu = new System.Windows.Forms.GroupBox();
            this.tlpnlCascadingMenu = new System.Windows.Forms.TableLayoutPanel();
            this.menuHelp = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            this.tlpnlMain.SuspendLayout();
            this.gbExplorerIntegration.SuspendLayout();
            this.flpnlExplorerIntegration.SuspendLayout();
            this.gbCascadingMenu.SuspendLayout();
            this.tlpnlCascadingMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.menuHelp)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(this.labelPreview);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(323, 71);
            panel1.Margin = new System.Windows.Forms.Padding(2);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(3);
            panel1.Size = new System.Drawing.Size(1139, 397);
            panel1.TabIndex = 0;
            // 
            // labelPreview
            // 
            this.labelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelPreview.Location = new System.Drawing.Point(3, 3);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(1133, 391);
            this.labelPreview.TabIndex = 1;
            this.labelPreview.Text = "...";
            // 
            // lblMenuEntries
            // 
            this.lblMenuEntries.AutoSize = true;
            this.lblMenuEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMenuEntries.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblMenuEntries.Location = new System.Drawing.Point(3, 45);
            this.lblMenuEntries.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblMenuEntries.Name = "lblMenuEntries";
            this.lblMenuEntries.Size = new System.Drawing.Size(237, 24);
            this.lblMenuEntries.TabIndex = 4;
            this.lblMenuEntries.Text = "Configuration of items in the context menu";
            this.lblMenuEntries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_chlMenuEntries
            // 
            this._NO_TRANSLATE_chlMenuEntries.CheckOnClick = true;
            this.tlpnlCascadingMenu.SetColumnSpan(this._NO_TRANSLATE_chlMenuEntries, 2);
            this._NO_TRANSLATE_chlMenuEntries.FormattingEnabled = true;
            this._NO_TRANSLATE_chlMenuEntries.Items.AddRange(new object[] {
            "Add files...",
            "Apply patch...",
            "Open repository",
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
            this._NO_TRANSLATE_chlMenuEntries.Location = new System.Drawing.Point(3, 72);
            this._NO_TRANSLATE_chlMenuEntries.Name = "_NO_TRANSLATE_chlMenuEntries";
            this._NO_TRANSLATE_chlMenuEntries.Size = new System.Drawing.Size(315, 94);
            this._NO_TRANSLATE_chlMenuEntries.TabIndex = 5;
            this._NO_TRANSLATE_chlMenuEntries.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chlMenuEntries_ItemCheck);
            this._NO_TRANSLATE_chlMenuEntries.SelectedValueChanged += new System.EventHandler(this.chlMenuEntries_SelectedValueChanged);
            // 
            // cbAlwaysShowAllCommands
            // 
            this.cbAlwaysShowAllCommands.AutoSize = true;
            this.tlpnlCascadingMenu.SetColumnSpan(this.cbAlwaysShowAllCommands, 3);
            this.cbAlwaysShowAllCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbAlwaysShowAllCommands.Location = new System.Drawing.Point(3, 3);
            this.cbAlwaysShowAllCommands.Name = "cbAlwaysShowAllCommands";
            this.cbAlwaysShowAllCommands.Size = new System.Drawing.Size(1458, 19);
            this.cbAlwaysShowAllCommands.TabIndex = 3;
            this.cbAlwaysShowAllCommands.Text = "Always show all commands";
            this.cbAlwaysShowAllCommands.UseVisualStyleBackColor = true;
            // 
            // tlpnlMain
            // 
            this.tlpnlMain.AutoSize = true;
            this.tlpnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlMain.ColumnCount = 1;
            this.tlpnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlMain.Controls.Add(this.gbExplorerIntegration, 0, 0);
            this.tlpnlMain.Controls.Add(this.gbCascadingMenu, 0, 1);
            this.tlpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlMain.Location = new System.Drawing.Point(8, 8);
            this.tlpnlMain.Name = "tlpnlMain";
            this.tlpnlMain.RowCount = 1;
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 508F));
            this.tlpnlMain.Size = new System.Drawing.Size(1486, 315);
            this.tlpnlMain.TabIndex = 0;
            // 
            // gbExplorerIntegration
            // 
            this.gbExplorerIntegration.AutoSize = true;
            this.gbExplorerIntegration.Controls.Add(this.flpnlExplorerIntegration);
            this.gbExplorerIntegration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbExplorerIntegration.Location = new System.Drawing.Point(3, 3);
            this.gbExplorerIntegration.Name = "gbExplorerIntegration";
            this.gbExplorerIntegration.Padding = new System.Windows.Forms.Padding(8);
            this.gbExplorerIntegration.Size = new System.Drawing.Size(1480, 63);
            this.gbExplorerIntegration.TabIndex = 0;
            this.gbExplorerIntegration.TabStop = false;
            this.gbExplorerIntegration.Text = "Windows Explorer integration";
            // 
            // flpnlExplorerIntegration
            // 
            this.flpnlExplorerIntegration.AutoSize = true;
            this.flpnlExplorerIntegration.Controls.Add(this.RegisterButton);
            this.flpnlExplorerIntegration.Controls.Add(this.UnregisterButton);
            this.flpnlExplorerIntegration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlExplorerIntegration.Location = new System.Drawing.Point(8, 24);
            this.flpnlExplorerIntegration.Name = "flpnlExplorerIntegration";
            this.flpnlExplorerIntegration.Size = new System.Drawing.Size(1464, 31);
            this.flpnlExplorerIntegration.TabIndex = 0;
            // 
            // RegisterButton
            // 
            this.RegisterButton.AutoSize = true;
            this.RegisterButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.RegisterButton.Location = new System.Drawing.Point(3, 3);
            this.RegisterButton.Name = "RegisterButton";
            this.RegisterButton.Size = new System.Drawing.Size(133, 25);
            this.RegisterButton.TabIndex = 1;
            this.RegisterButton.Text = "&Enable shell extension";
            this.RegisterButton.UseVisualStyleBackColor = true;
            this.RegisterButton.Click += new System.EventHandler(this.RegisterButton_Click);
            // 
            // UnregisterButton
            // 
            this.UnregisterButton.AutoSize = true;
            this.UnregisterButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.UnregisterButton.Location = new System.Drawing.Point(142, 3);
            this.UnregisterButton.Name = "UnregisterButton";
            this.UnregisterButton.Size = new System.Drawing.Size(136, 25);
            this.UnregisterButton.TabIndex = 2;
            this.UnregisterButton.Text = "&Disable shell extension";
            this.UnregisterButton.UseVisualStyleBackColor = true;
            this.UnregisterButton.Click += new System.EventHandler(this.UnregisterButton_Click);
            // 
            // gbCascadingMenu
            // 
            this.gbCascadingMenu.AutoSize = true;
            this.gbCascadingMenu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbCascadingMenu.Controls.Add(this.tlpnlCascadingMenu);
            this.gbCascadingMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCascadingMenu.Location = new System.Drawing.Point(3, 72);
            this.gbCascadingMenu.Name = "gbCascadingMenu";
            this.gbCascadingMenu.Padding = new System.Windows.Forms.Padding(8);
            this.gbCascadingMenu.Size = new System.Drawing.Size(1480, 502);
            this.gbCascadingMenu.TabIndex = 0;
            this.gbCascadingMenu.TabStop = false;
            this.gbCascadingMenu.Text = "Cascaded Context Menu";
            // 
            // tlpnlCascadingMenu
            // 
            this.tlpnlCascadingMenu.AutoSize = true;
            this.tlpnlCascadingMenu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlCascadingMenu.ColumnCount = 3;
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlCascadingMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlCascadingMenu.Controls.Add(this.menuHelp, 1, 2);
            this.tlpnlCascadingMenu.Controls.Add(this.label1, 2, 2);
            this.tlpnlCascadingMenu.Controls.Add(this.cbAlwaysShowAllCommands, 0, 0);
            this.tlpnlCascadingMenu.Controls.Add(this.lblMenuEntries, 0, 2);
            this.tlpnlCascadingMenu.Controls.Add(this._NO_TRANSLATE_chlMenuEntries, 0, 3);
            this.tlpnlCascadingMenu.Controls.Add(panel1, 2, 3);
            this.tlpnlCascadingMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlCascadingMenu.Location = new System.Drawing.Point(8, 24);
            this.tlpnlCascadingMenu.Name = "tlpnlCascadingMenu";
            this.tlpnlCascadingMenu.RowCount = 4;
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlCascadingMenu.Size = new System.Drawing.Size(1464, 470);
            this.tlpnlCascadingMenu.TabIndex = 0;
            // 
            // menuHelp
            // 
            this.menuHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.menuHelp.Image = global::GitUI.Properties.Resources.information;
            this.menuHelp.Location = new System.Drawing.Point(240, 50);
            this.menuHelp.Margin = new System.Windows.Forms.Padding(0, 5, 3, 3);
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(16, 16);
            this.menuHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.menuHelp.TabIndex = 13;
            this.menuHelp.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(324, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1137, 24);
            this.label1.TabIndex = 4;
            this.label1.Text = "Context menu preview:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ShellExtensionSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tlpnlMain);
            this.Name = "ShellExtensionSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(1502, 331);
            panel1.ResumeLayout(false);
            this.tlpnlMain.ResumeLayout(false);
            this.tlpnlMain.PerformLayout();
            this.gbExplorerIntegration.ResumeLayout(false);
            this.gbExplorerIntegration.PerformLayout();
            this.flpnlExplorerIntegration.ResumeLayout(false);
            this.flpnlExplorerIntegration.PerformLayout();
            this.gbCascadingMenu.ResumeLayout(false);
            this.gbCascadingMenu.PerformLayout();
            this.tlpnlCascadingMenu.ResumeLayout(false);
            this.tlpnlCascadingMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.menuHelp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMenuEntries;
        private System.Windows.Forms.CheckedListBox _NO_TRANSLATE_chlMenuEntries;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.CheckBox cbAlwaysShowAllCommands;
        private System.Windows.Forms.FlowLayoutPanel flpnlExplorerIntegration;
        private System.Windows.Forms.TableLayoutPanel tlpnlCascadingMenu;
        private System.Windows.Forms.TableLayoutPanel tlpnlMain;
        private System.Windows.Forms.GroupBox gbCascadingMenu;
        private System.Windows.Forms.GroupBox gbExplorerIntegration;
        private Button RegisterButton;
        private Button UnregisterButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox menuHelp;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
