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
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            btnArgumentsHelp = new Button();
            propertyGrid1 = new PropertyGrid();
            panel2 = new Panel();
            lvScripts = new GitUI.UserControls.NativeListView();
            chdrNames = ((ColumnHeader)(new ColumnHeader()));
            chdrEvents = ((ColumnHeader)(new ColumnHeader()));
            chdrCommand = ((ColumnHeader)(new ColumnHeader()));
            panelButtons = new FlowLayoutPanel();
            btnAdd = new Button();
            btnDelete = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel2.SuspendLayout();
            panelButtons.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnMoveUp
            // 
            btnMoveUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMoveUp.Enabled = false;
            btnMoveUp.Image = Properties.Images.ArrowUp;
            btnMoveUp.Location = new Point(7, 67);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(26, 26);
            btnMoveUp.TabIndex = 0;
            btnMoveUp.UseVisualStyleBackColor = true;
            btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMoveDown.Enabled = false;
            btnMoveDown.Image = Properties.Images.ArrowDown;
            btnMoveDown.Location = new Point(7, 99);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(26, 26);
            btnMoveDown.TabIndex = 3;
            btnMoveDown.UseVisualStyleBackColor = true;
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // btnArgumentsHelp
            // 
            btnArgumentsHelp.Location = new Point(7, 131);
            btnArgumentsHelp.Name = "btnArgumentsHelp";
            btnArgumentsHelp.Size = new Size(26, 26);
            btnArgumentsHelp.TabIndex = 13;
            btnArgumentsHelp.Text = "?";
            btnArgumentsHelp.UseVisualStyleBackColor = true;
            btnArgumentsHelp.Click += btnArgumentsHelp_Click;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(3, 276);
            propertyGrid1.Margin = new Padding(3, 3, 40, 3);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(394, 259);
            propertyGrid1.TabIndex = 2;
            propertyGrid1.ToolbarVisible = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(lvScripts);
            panel2.Controls.Add(panelButtons);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(431, 259);
            panel2.TabIndex = 3;
            // 
            // lvScripts
            // 
            lvScripts.Activation = ItemActivation.OneClick;
            lvScripts.CheckBoxes = true;
            lvScripts.Columns.AddRange(new ColumnHeader[] {
            chdrNames,
            chdrEvents,
            chdrCommand});
            lvScripts.Dock = DockStyle.Fill;
            lvScripts.FullRowSelect = true;
            lvScripts.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvScripts.HideSelection = false;
            lvScripts.LabelWrap = false;
            lvScripts.Location = new Point(0, 0);
            lvScripts.Margin = new Padding(0);
            lvScripts.MultiSelect = false;
            lvScripts.Name = "lvScripts";
            lvScripts.ShowItemToolTips = true;
            lvScripts.Size = new Size(395, 259);
            lvScripts.TabIndex = 1;
            lvScripts.TileSize = new Size(136, 18);
            lvScripts.UseCompatibleStateImageBehavior = false;
            lvScripts.View = View.Details;
            // 
            // chdrNames
            // 
            chdrNames.Text = "Name";
            chdrNames.Width = 200;
            // 
            // chdrEvents
            // 
            chdrEvents.Text = "Event";
            chdrEvents.Width = 130;
            // 
            // chdrCommand
            // 
            chdrCommand.Text = "Command";
            chdrCommand.Width = 330;
            // 
            // panelButtons
            // 
            panelButtons.AutoSize = true;
            panelButtons.Controls.Add(btnAdd);
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnMoveUp);
            panelButtons.Controls.Add(btnMoveDown);
            panelButtons.Controls.Add(btnArgumentsHelp);
            panelButtons.Dock = DockStyle.Right;
            panelButtons.FlowDirection = FlowDirection.TopDown;
            panelButtons.Location = new Point(395, 0);
            panelButtons.Margin = new Padding(8);
            panelButtons.Name = "panelButtons";
            panelButtons.Padding = new Padding(4, 0, 0, 0);
            panelButtons.Size = new Size(36, 259);
            panelButtons.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Image = Properties.Images.RemoteAdd;
            btnAdd.Location = new Point(7, 3);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(26, 26);
            btnAdd.TabIndex = 0;
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDelete
            // 
            btnDelete.Image = Properties.Images.RemoteDelete;
            btnDelete.Location = new Point(7, 35);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(26, 26);
            btnDelete.TabIndex = 1;
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(panel2, 0, 0);
            tableLayoutPanel1.Controls.Add(propertyGrid1, 0, 2);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(437, 538);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // ScriptsSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tableLayoutPanel1);
            Name = "ScriptsSettingsPage";
            Size = new Size(1335, 885);
            Text = "Scripts";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panelButtons.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnArgumentsHelp;
        private PropertyGrid propertyGrid1;
        private Panel panel2;
        private UserControls.NativeListView lvScripts;
        private FlowLayoutPanel panelButtons;
        private Button btnAdd;
        private Button btnDelete;
        private ColumnHeader chdrNames;
        private TableLayoutPanel tableLayoutPanel1;
        private ColumnHeader chdrEvents;
        private ColumnHeader chdrCommand;
    }
}
