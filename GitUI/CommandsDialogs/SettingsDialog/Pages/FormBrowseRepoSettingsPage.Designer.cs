namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class FormBrowseRepoSettingsPage
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkShowGpgInformation = new System.Windows.Forms.CheckBox();
            this.chkChowConsoleTab = new System.Windows.Forms.CheckBox();
            this.gbTabs = new System.Windows.Forms.GroupBox();
            this._shellsListView = new GitUI.UserControls.NativeListView();
            this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnCommand = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnArguments = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.addButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this._shellPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.gbTabs.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkShowGpgInformation
            // 
            this.chkShowGpgInformation.AutoSize = true;
            this.chkShowGpgInformation.Location = new System.Drawing.Point(6, 45);
            this.chkShowGpgInformation.Name = "chkShowGpgInformation";
            this.chkShowGpgInformation.Size = new System.Drawing.Size(133, 17);
            this.chkShowGpgInformation.TabIndex = 3;
            this.chkShowGpgInformation.Text = "Show GPG information";
            this.chkShowGpgInformation.UseVisualStyleBackColor = true;
            // 
            // chkChowConsoleTab
            // 
            this.chkChowConsoleTab.AutoSize = true;
            this.chkChowConsoleTab.Location = new System.Drawing.Point(6, 22);
            this.chkChowConsoleTab.Name = "chkChowConsoleTab";
            this.chkChowConsoleTab.Size = new System.Drawing.Size(130, 17);
            this.chkChowConsoleTab.TabIndex = 2;
            this.chkChowConsoleTab.Text = "Show the Console tab";
            this.chkChowConsoleTab.UseVisualStyleBackColor = true;
            // 
            // gbTabs
            // 
            this.gbTabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTabs.Controls.Add(this.chkShowGpgInformation);
            this.gbTabs.Controls.Add(this.chkChowConsoleTab);
            this.gbTabs.Location = new System.Drawing.Point(0, 704);
            this.gbTabs.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.gbTabs.Name = "gbTabs";
            this.gbTabs.Padding = new System.Windows.Forms.Padding(0);
            this.gbTabs.Size = new System.Drawing.Size(1331, 82);
            this.gbTabs.TabIndex = 4;
            this.gbTabs.TabStop = false;
            this.gbTabs.Text = "Tabs (restart required)";
            // 
            // _shellsListView
            // 
            this._shellsListView.CheckBoxes = true;
            this._shellsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnName,
            this.columnCommand,
            this.columnArguments});
            this._shellsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._shellsListView.FullRowSelect = true;
            this._shellsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._shellsListView.HideSelection = false;
            this._shellsListView.LabelWrap = false;
            this._shellsListView.Location = new System.Drawing.Point(0, 0);
            this._shellsListView.Margin = new System.Windows.Forms.Padding(0);
            this._shellsListView.MultiSelect = false;
            this._shellsListView.Name = "_shellsListView";
            this._shellsListView.ShowItemToolTips = true;
            this._shellsListView.Size = new System.Drawing.Size(1331, 350);
            this._shellsListView.TabIndex = 0;
            this._shellsListView.UseCompatibleStateImageBehavior = false;
            this._shellsListView.View = System.Windows.Forms.View.Details;
            // 
            // columnName
            // 
            this.columnName.Tag = "Name";
            this.columnName.Text = "Name";
            this.columnName.Width = 150;
            // 
            // columnCommand
            // 
            this.columnCommand.Tag = "Command";
            this.columnCommand.Text = "Command";
            this.columnCommand.Width = 500;
            // 
            // columnArguments
            // 
            this.columnArguments.Tag = "Arguments";
            this.columnArguments.Text = "Arguments";
            this.columnArguments.Width = 200;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panelButtons, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._shellsListView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._shellPropertyGrid, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.gbTabs, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1365, 786);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // panelButtons
            // 
            this.panelButtons.AutoSize = true;
            this.panelButtons.Controls.Add(this.addButton);
            this.panelButtons.Controls.Add(this.removeButton);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panelButtons.Location = new System.Drawing.Point(1331, 0);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(34, 350);
            this.panelButtons.TabIndex = 5;
            // 
            // addButton
            // 
            this.addButton.Image = global::GitUI.Properties.Images.RemoteAdd;
            this.addButton.Location = new System.Drawing.Point(8, 0);
            this.addButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(26, 26);
            this.addButton.TabIndex = 0;
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Image = global::GitUI.Properties.Images.RemoteDelete;
            this.removeButton.Location = new System.Drawing.Point(8, 34);
            this.removeButton.Margin = new System.Windows.Forms.Padding(8, 4, 0, 4);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(26, 26);
            this.removeButton.TabIndex = 1;
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // _shellPropertyGrid
            // 
            this._shellPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._shellPropertyGrid.Location = new System.Drawing.Point(0, 358);
            this._shellPropertyGrid.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this._shellPropertyGrid.Name = "_shellPropertyGrid";
            this._shellPropertyGrid.Size = new System.Drawing.Size(1331, 342);
            this._shellPropertyGrid.TabIndex = 1;
            this._shellPropertyGrid.ToolbarVisible = false;
            // 
            // FormBrowseRepoSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "FormBrowseRepoSettingsPage";
            this.Padding = new System.Windows.Forms.Padding(4, 0, 3, 0);
            this.Size = new System.Drawing.Size(1372, 786);
            this.gbTabs.ResumeLayout(false);
            this.gbTabs.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkShowGpgInformation;
        private System.Windows.Forms.CheckBox chkChowConsoleTab;
        private System.Windows.Forms.GroupBox gbTabs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PropertyGrid _shellPropertyGrid;
        private UserControls.NativeListView _shellsListView;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnCommand;
        private System.Windows.Forms.ColumnHeader columnArguments;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button removeButton;
    }
}
