namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ScriptsSettingsPage
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
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnArgumentsHelp = new System.Windows.Forms.Button();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lvScripts = new GitUI.UserControls.NativeListView();
            this.chdrNames = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chdrEvents = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chdrCommand = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveUp.Enabled = false;
            this.btnMoveUp.Image = global::GitUI.Properties.Images.ArrowUp;
            this.btnMoveUp.Location = new System.Drawing.Point(7, 67);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(26, 26);
            this.btnMoveUp.TabIndex = 0;
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveDown.Enabled = false;
            this.btnMoveDown.Image = global::GitUI.Properties.Images.ArrowDown;
            this.btnMoveDown.Location = new System.Drawing.Point(7, 99);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(26, 26);
            this.btnMoveDown.TabIndex = 3;
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnArgumentsHelp
            // 
            this.btnArgumentsHelp.Location = new System.Drawing.Point(7, 131);
            this.btnArgumentsHelp.Name = "btnArgumentsHelp";
            this.btnArgumentsHelp.Size = new System.Drawing.Size(26, 26);
            this.btnArgumentsHelp.TabIndex = 13;
            this.btnArgumentsHelp.Text = "?";
            this.btnArgumentsHelp.UseVisualStyleBackColor = true;
            this.btnArgumentsHelp.Click += new System.EventHandler(this.btnArgumentsHelp_Click);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 276);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(3, 3, 40, 3);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(394, 259);
            this.propertyGrid1.TabIndex = 2;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lvScripts);
            this.panel2.Controls.Add(this.panelButtons);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(431, 259);
            this.panel2.TabIndex = 3;
            // 
            // lvScripts
            // 
            this.lvScripts.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvScripts.CheckBoxes = true;
            this.lvScripts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chdrNames,
            this.chdrEvents,
            this.chdrCommand});
            this.lvScripts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvScripts.FullRowSelect = true;
            this.lvScripts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvScripts.HideSelection = false;
            this.lvScripts.LabelWrap = false;
            this.lvScripts.Location = new System.Drawing.Point(0, 0);
            this.lvScripts.Margin = new System.Windows.Forms.Padding(0);
            this.lvScripts.MultiSelect = false;
            this.lvScripts.Name = "lvScripts";
            this.lvScripts.ShowItemToolTips = true;
            this.lvScripts.Size = new System.Drawing.Size(395, 259);
            this.lvScripts.TabIndex = 1;
            this.lvScripts.TileSize = new System.Drawing.Size(136, 18);
            this.lvScripts.UseCompatibleStateImageBehavior = false;
            this.lvScripts.View = System.Windows.Forms.View.Details;
            // 
            // chdrNames
            // 
            this.chdrNames.Text = "Name";
            this.chdrNames.Width = 200;
            // 
            // chdrEvents
            // 
            this.chdrEvents.Text = "Event";
            this.chdrEvents.Width = 130;
            // 
            // chdrCommand
            // 
            this.chdrCommand.Text = "Command";
            this.chdrCommand.Width = 330;
            // 
            // panelButtons
            // 
            this.panelButtons.AutoSize = true;
            this.panelButtons.Controls.Add(this.btnAdd);
            this.panelButtons.Controls.Add(this.btnDelete);
            this.panelButtons.Controls.Add(this.btnMoveUp);
            this.panelButtons.Controls.Add(this.btnMoveDown);
            this.panelButtons.Controls.Add(this.btnArgumentsHelp);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panelButtons.Location = new System.Drawing.Point(395, 0);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(8);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.panelButtons.Size = new System.Drawing.Size(36, 259);
            this.panelButtons.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.Image = global::GitUI.Properties.Images.RemoteAdd;
            this.btnAdd.Location = new System.Drawing.Point(7, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(26, 26);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Image = global::GitUI.Properties.Images.RemoteDelete;
            this.btnDelete.Location = new System.Drawing.Point(7, 35);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(26, 26);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.propertyGrid1, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(437, 538);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // ScriptsSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ScriptsSettingsPage";
            this.Size = new System.Drawing.Size(1335, 885);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnArgumentsHelp;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Panel panel2;
        private UserControls.NativeListView lvScripts;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ColumnHeader chdrNames;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ColumnHeader chdrEvents;
        private System.Windows.Forms.ColumnHeader chdrCommand;
    }
}
