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
            components = new System.ComponentModel.Container();
            Panel panel1;
            labelPreview = new Label();
            lblMenuEntries = new Label();
            _NO_TRANSLATE_chlMenuEntries = new CheckedListBox();
            cbAlwaysShowAllCommands = new CheckBox();
            tlpnlMain = new TableLayoutPanel();
            gbExplorerIntegration = new GroupBox();
            flpnlExplorerIntegration = new FlowLayoutPanel();
            RegisterButton = new Button();
            UnregisterButton = new Button();
            gbCascadingMenu = new GroupBox();
            tlpnlCascadingMenu = new TableLayoutPanel();
            menuHelp = new PictureBox();
            label1 = new Label();
            toolTip1 = new ToolTip(components);
            panel1 = new Panel();
            panel1.SuspendLayout();
            tlpnlMain.SuspendLayout();
            gbExplorerIntegration.SuspendLayout();
            flpnlExplorerIntegration.SuspendLayout();
            gbCascadingMenu.SuspendLayout();
            tlpnlCascadingMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(menuHelp)).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(labelPreview);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(323, 71);
            panel1.Margin = new Padding(2);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(3);
            panel1.Size = new Size(1139, 397);
            panel1.TabIndex = 0;
            // 
            // labelPreview
            // 
            labelPreview.Dock = DockStyle.Fill;
            labelPreview.Location = new Point(3, 3);
            labelPreview.Name = "labelPreview";
            labelPreview.Size = new Size(1133, 391);
            labelPreview.TabIndex = 1;
            labelPreview.Text = "...";
            // 
            // lblMenuEntries
            // 
            lblMenuEntries.AutoSize = true;
            lblMenuEntries.Dock = DockStyle.Fill;
            lblMenuEntries.ImageAlign = ContentAlignment.MiddleLeft;
            lblMenuEntries.Location = new Point(3, 45);
            lblMenuEntries.Margin = new Padding(3, 0, 0, 0);
            lblMenuEntries.Name = "lblMenuEntries";
            lblMenuEntries.Size = new Size(237, 24);
            lblMenuEntries.TabIndex = 4;
            lblMenuEntries.Text = "Configuration of items in the context menu";
            lblMenuEntries.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_chlMenuEntries
            // 
            _NO_TRANSLATE_chlMenuEntries.CheckOnClick = true;
            tlpnlCascadingMenu.SetColumnSpan(_NO_TRANSLATE_chlMenuEntries, 2);
            _NO_TRANSLATE_chlMenuEntries.FormattingEnabled = true;
            _NO_TRANSLATE_chlMenuEntries.Items.AddRange(new object[] {
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
            _NO_TRANSLATE_chlMenuEntries.Location = new Point(3, 72);
            _NO_TRANSLATE_chlMenuEntries.Name = "_NO_TRANSLATE_chlMenuEntries";
            _NO_TRANSLATE_chlMenuEntries.Size = new Size(315, 94);
            _NO_TRANSLATE_chlMenuEntries.TabIndex = 5;
            _NO_TRANSLATE_chlMenuEntries.ItemCheck += chlMenuEntries_ItemCheck;
            _NO_TRANSLATE_chlMenuEntries.SelectedValueChanged += chlMenuEntries_SelectedValueChanged;
            // 
            // cbAlwaysShowAllCommands
            // 
            cbAlwaysShowAllCommands.AutoSize = true;
            tlpnlCascadingMenu.SetColumnSpan(cbAlwaysShowAllCommands, 3);
            cbAlwaysShowAllCommands.Dock = DockStyle.Fill;
            cbAlwaysShowAllCommands.Location = new Point(3, 3);
            cbAlwaysShowAllCommands.Name = "cbAlwaysShowAllCommands";
            cbAlwaysShowAllCommands.Size = new Size(1458, 19);
            cbAlwaysShowAllCommands.TabIndex = 3;
            cbAlwaysShowAllCommands.Text = "Always show all commands";
            cbAlwaysShowAllCommands.UseVisualStyleBackColor = true;
            // 
            // tlpnlMain
            // 
            tlpnlMain.AutoSize = true;
            tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlMain.Controls.Add(gbExplorerIntegration, 0, 0);
            tlpnlMain.Controls.Add(gbCascadingMenu, 0, 1);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 1;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 508F));
            tlpnlMain.Size = new Size(1486, 315);
            tlpnlMain.TabIndex = 0;
            // 
            // gbExplorerIntegration
            // 
            gbExplorerIntegration.AutoSize = true;
            gbExplorerIntegration.Controls.Add(flpnlExplorerIntegration);
            gbExplorerIntegration.Dock = DockStyle.Fill;
            gbExplorerIntegration.Location = new Point(3, 3);
            gbExplorerIntegration.Name = "gbExplorerIntegration";
            gbExplorerIntegration.Padding = new Padding(8);
            gbExplorerIntegration.Size = new Size(1480, 63);
            gbExplorerIntegration.TabIndex = 0;
            gbExplorerIntegration.TabStop = false;
            gbExplorerIntegration.Text = "Windows Explorer integration";
            // 
            // flpnlExplorerIntegration
            // 
            flpnlExplorerIntegration.AutoSize = true;
            flpnlExplorerIntegration.Controls.Add(RegisterButton);
            flpnlExplorerIntegration.Controls.Add(UnregisterButton);
            flpnlExplorerIntegration.Dock = DockStyle.Fill;
            flpnlExplorerIntegration.Location = new Point(8, 24);
            flpnlExplorerIntegration.Name = "flpnlExplorerIntegration";
            flpnlExplorerIntegration.Size = new Size(1464, 31);
            flpnlExplorerIntegration.TabIndex = 0;
            // 
            // RegisterButton
            // 
            RegisterButton.AutoSize = true;
            RegisterButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            RegisterButton.Location = new Point(3, 3);
            RegisterButton.Name = "RegisterButton";
            RegisterButton.Size = new Size(133, 25);
            RegisterButton.TabIndex = 1;
            RegisterButton.Text = "&Enable shell extension";
            RegisterButton.UseVisualStyleBackColor = true;
            RegisterButton.Click += RegisterButton_Click;
            // 
            // UnregisterButton
            // 
            UnregisterButton.AutoSize = true;
            UnregisterButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            UnregisterButton.Location = new Point(142, 3);
            UnregisterButton.Name = "UnregisterButton";
            UnregisterButton.Size = new Size(136, 25);
            UnregisterButton.TabIndex = 2;
            UnregisterButton.Text = "&Disable shell extension";
            UnregisterButton.UseVisualStyleBackColor = true;
            UnregisterButton.Click += UnregisterButton_Click;
            // 
            // gbCascadingMenu
            // 
            gbCascadingMenu.AutoSize = true;
            gbCascadingMenu.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbCascadingMenu.Controls.Add(tlpnlCascadingMenu);
            gbCascadingMenu.Dock = DockStyle.Fill;
            gbCascadingMenu.Location = new Point(3, 72);
            gbCascadingMenu.Name = "gbCascadingMenu";
            gbCascadingMenu.Padding = new Padding(8);
            gbCascadingMenu.Size = new Size(1480, 502);
            gbCascadingMenu.TabIndex = 0;
            gbCascadingMenu.TabStop = false;
            gbCascadingMenu.Text = "Cascaded context menu";
            // 
            // tlpnlCascadingMenu
            // 
            tlpnlCascadingMenu.AutoSize = true;
            tlpnlCascadingMenu.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlCascadingMenu.ColumnCount = 3;
            tlpnlCascadingMenu.ColumnStyles.Add(new ColumnStyle());
            tlpnlCascadingMenu.ColumnStyles.Add(new ColumnStyle());
            tlpnlCascadingMenu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlCascadingMenu.Controls.Add(menuHelp, 1, 2);
            tlpnlCascadingMenu.Controls.Add(label1, 2, 2);
            tlpnlCascadingMenu.Controls.Add(cbAlwaysShowAllCommands, 0, 0);
            tlpnlCascadingMenu.Controls.Add(lblMenuEntries, 0, 2);
            tlpnlCascadingMenu.Controls.Add(_NO_TRANSLATE_chlMenuEntries, 0, 3);
            tlpnlCascadingMenu.Controls.Add(panel1, 2, 3);
            tlpnlCascadingMenu.Dock = DockStyle.Fill;
            tlpnlCascadingMenu.Location = new Point(8, 24);
            tlpnlCascadingMenu.Name = "tlpnlCascadingMenu";
            tlpnlCascadingMenu.RowCount = 4;
            tlpnlCascadingMenu.RowStyles.Add(new RowStyle());
            tlpnlCascadingMenu.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpnlCascadingMenu.RowStyles.Add(new RowStyle());
            tlpnlCascadingMenu.RowStyles.Add(new RowStyle());
            tlpnlCascadingMenu.Size = new Size(1464, 470);
            tlpnlCascadingMenu.TabIndex = 0;
            // 
            // menuHelp
            // 
            menuHelp.Cursor = Cursors.Hand;
            menuHelp.Image = Properties.Resources.information;
            menuHelp.Location = new Point(240, 50);
            menuHelp.Margin = new Padding(0, 5, 3, 3);
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new Size(16, 16);
            menuHelp.SizeMode = PictureBoxSizeMode.AutoSize;
            menuHelp.TabIndex = 13;
            menuHelp.TabStop = false;
            menuHelp.Click += menuHelp_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(324, 45);
            label1.Name = "label1";
            label1.Size = new Size(1137, 24);
            label1.TabIndex = 4;
            label1.Text = "Context menu preview:";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ShellExtensionSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tlpnlMain);
            Name = "ShellExtensionSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1502, 331);
            Text = "Shell extension";
            panel1.ResumeLayout(false);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            gbExplorerIntegration.ResumeLayout(false);
            gbExplorerIntegration.PerformLayout();
            flpnlExplorerIntegration.ResumeLayout(false);
            flpnlExplorerIntegration.PerformLayout();
            gbCascadingMenu.ResumeLayout(false);
            gbCascadingMenu.PerformLayout();
            tlpnlCascadingMenu.ResumeLayout(false);
            tlpnlCascadingMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(menuHelp)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label lblMenuEntries;
        private CheckedListBox _NO_TRANSLATE_chlMenuEntries;
        private Label labelPreview;
        private CheckBox cbAlwaysShowAllCommands;
        private FlowLayoutPanel flpnlExplorerIntegration;
        private TableLayoutPanel tlpnlCascadingMenu;
        private TableLayoutPanel tlpnlMain;
        private GroupBox gbCascadingMenu;
        private GroupBox gbExplorerIntegration;
        private Button RegisterButton;
        private Button UnregisterButton;
        private Label label1;
        private PictureBox menuHelp;
        private ToolTip toolTip1;
    }
}
