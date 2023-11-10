using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    partial class FormSubmodules
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
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            tableLayoutPanel2 = new TableLayoutPanel();
            AddSubmodule = new Button();
            Submodules = new DataGridView();
            nameDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            Status = new DataGridViewTextBoxColumn();
            gitSubmoduleBindingSource = new BindingSource(components);
            tableLayoutPanel3 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            RemoveSubmodule = new Button();
            UpdateSubmodule = new Button();
            SynchronizeSubmodule = new Button();
            groupBox1 = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            SubModuleStatus = new TextBox();
            label6 = new Label();
            label1 = new Label();
            label2 = new Label();
            label5 = new Label();
            SubModuleName = new TextBox();
            label4 = new Label();
            SubModuleRemotePath = new TextBox();
            label3 = new Label();
            SubModuleLocalPath = new TextBox();
            SubModuleCommit = new TextBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            SubModuleBranch = new TextBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            Pull = new Button();

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();

            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(Submodules)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(gitSubmoduleBindingSource)).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tableLayoutPanel3);
            splitContainer1.Size = new Size(782, 372);
            splitContainer1.SplitterDistance = 222;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(AddSubmodule, 0, 1);
            tableLayoutPanel2.Controls.Add(Submodules, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(222, 372);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // AddSubmodule
            // 
            AddSubmodule.Location = new Point(4, 337);
            AddSubmodule.Margin = new Padding(4);
            AddSubmodule.Name = "AddSubmodule";
            AddSubmodule.Size = new Size(202, 31);
            AddSubmodule.TabIndex = 0;
            AddSubmodule.Text = "Add submodule";
            AddSubmodule.UseVisualStyleBackColor = true;
            AddSubmodule.Click += AddSubmoduleClick;
            // 
            // Submodules
            // 
            Submodules.AllowUserToAddRows = false;
            Submodules.AllowUserToDeleteRows = false;
            Submodules.AllowUserToResizeRows = false;
            Submodules.AutoGenerateColumns = false;
            Submodules.BorderStyle = BorderStyle.None;
            Submodules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Submodules.Columns.AddRange(new DataGridViewColumn[] {
            nameDataGridViewTextBoxColumn,
            Status});
            Submodules.DataSource = gitSubmoduleBindingSource;
            Submodules.Dock = DockStyle.Fill;
            Submodules.Location = new Point(0, 0);
            Submodules.Margin = new Padding(0);
            Submodules.MultiSelect = false;
            Submodules.Name = "Submodules";
            Submodules.ReadOnly = true;
            Submodules.RowHeadersVisible = false;
            Submodules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Submodules.Size = new Size(214, 325);
            Submodules.TabIndex = 0;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            nameDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            nameDataGridViewTextBoxColumn.HeaderText = "Name";
            nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // Status
            // 
            Status.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            Status.HeaderText = "Status";
            Status.Name = "Status";
            Status.ReadOnly = true;
            // 
            // gitSubmoduleBindingSource
            // 
            gitSubmoduleBindingSource.DataSource = typeof(GitCommands.GitSubmoduleInfo);
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(flowLayoutPanel1, 0, 1);
            tableLayoutPanel3.Controls.Add(groupBox1, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Margin = new Padding(4);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(555, 372);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(RemoveSubmodule);
            flowLayoutPanel1.Controls.Add(UpdateSubmodule);
            flowLayoutPanel1.Controls.Add(SynchronizeSubmodule);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(0, 331);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(555, 41);
            flowLayoutPanel1.TabIndex = 0;
            flowLayoutPanel1.WrapContents = false;
            // 
            // RemoveSubmodule
            // 
            RemoveSubmodule.AutoSize = true;
            RemoveSubmodule.Location = new Point(407, 4);
            RemoveSubmodule.Margin = new Padding(4);
            RemoveSubmodule.Name = "RemoveSubmodule";
            RemoveSubmodule.Size = new Size(144, 33);
            RemoveSubmodule.TabIndex = 6;
            RemoveSubmodule.Text = "Remove";
            RemoveSubmodule.UseVisualStyleBackColor = true;
            RemoveSubmodule.Click += RemoveSubmoduleClick;
            // 
            // UpdateSubmodule
            // 
            UpdateSubmodule.AutoSize = true;
            UpdateSubmodule.Image = Properties.Images.SubmodulesUpdate;
            UpdateSubmodule.ImageAlign = ContentAlignment.MiddleLeft;
            UpdateSubmodule.Location = new Point(255, 4);
            UpdateSubmodule.Margin = new Padding(4);
            UpdateSubmodule.Name = "UpdateSubmodule";
            UpdateSubmodule.Size = new Size(144, 33);
            UpdateSubmodule.TabIndex = 3;
            UpdateSubmodule.Text = "Update";
            UpdateSubmodule.UseVisualStyleBackColor = true;
            UpdateSubmodule.Click += UpdateSubmoduleClick;
            // 
            // SynchronizeSubmodule
            // 
            SynchronizeSubmodule.AutoSize = true;
            SynchronizeSubmodule.Image = Properties.Images.SubmodulesSync;
            SynchronizeSubmodule.ImageAlign = ContentAlignment.MiddleLeft;
            SynchronizeSubmodule.Location = new Point(103, 4);
            SynchronizeSubmodule.Margin = new Padding(4);
            SynchronizeSubmodule.Name = "SynchronizeSubmodule";
            SynchronizeSubmodule.Size = new Size(144, 33);
            SynchronizeSubmodule.TabIndex = 5;
            SynchronizeSubmodule.Text = "Synchronize";
            SynchronizeSubmodule.UseVisualStyleBackColor = true;
            SynchronizeSubmodule.Click += SynchronizeSubmoduleClick;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tableLayoutPanel1);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(4, 4);
            groupBox1.Margin = new Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4);
            groupBox1.Size = new Size(547, 323);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Details";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(SubModuleStatus, 1, 5);
            tableLayoutPanel1.Controls.Add(label6, 0, 5);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(label5, 0, 4);
            tableLayoutPanel1.Controls.Add(SubModuleName, 1, 0);
            tableLayoutPanel1.Controls.Add(label4, 0, 3);
            tableLayoutPanel1.Controls.Add(SubModuleRemotePath, 1, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Controls.Add(SubModuleLocalPath, 1, 2);
            tableLayoutPanel1.Controls.Add(SubModuleCommit, 1, 3);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 1, 4);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(4, 27);
            tableLayoutPanel1.Margin = new Padding(4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(539, 292);
            tableLayoutPanel1.TabIndex = 12;
            // 
            // SubModuleStatus
            // 
            SubModuleStatus.Anchor = AnchorStyles.Left;
            SubModuleStatus.DataBindings.Add(new Binding("Text", this.gitSubmoduleBindingSource, "Status", true));
            SubModuleStatus.Dock = DockStyle.Top;
            SubModuleStatus.Location = new Point(120, 183);
            SubModuleStatus.Name = "SubModuleStatus";
            SubModuleStatus.ReadOnly = true;
            SubModuleStatus.Size = new Size(214, 30);
            SubModuleStatus.TabIndex = 13;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Left;
            label6.AutoSize = true;
            label6.Location = new Point(4, 186);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(57, 23);
            label6.TabIndex = 11;
            label6.Text = "Status";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(4, 6);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(56, 23);
            label1.TabIndex = 6;
            label1.Text = "Name";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(4, 42);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(109, 23);
            label2.TabIndex = 7;
            label2.Text = "Remote path";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Left;
            label5.AutoSize = true;
            label5.Location = new Point(4, 150);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(63, 23);
            label5.TabIndex = 10;
            label5.Text = "Branch";
            // 
            // SubModuleName
            // 
            SubModuleName.Anchor = AnchorStyles.Left;
            SubModuleName.DataBindings.Add(new Binding("Text", this.gitSubmoduleBindingSource, "Name", true));
            SubModuleName.Dock = DockStyle.Top;
            SubModuleName.Location = new Point(120, 3);
            SubModuleName.Name = "SubModuleName";
            SubModuleName.ReadOnly = true;
            SubModuleName.Size = new Size(214, 30);
            SubModuleName.TabIndex = 8;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(4, 114);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(71, 23);
            label4.TabIndex = 9;
            label4.Text = "Commit";
            // 
            // SubModuleRemotePath
            // 
            SubModuleRemotePath.DataBindings.Add(new Binding("Text", this.gitSubmoduleBindingSource, "RemotePath", true));
            SubModuleRemotePath.Dock = DockStyle.Top;
            SubModuleRemotePath.Location = new Point(120, 39);
            SubModuleRemotePath.Name = "SubModuleRemotePath";
            SubModuleRemotePath.ReadOnly = true;
            SubModuleRemotePath.Size = new Size(416, 30);
            SubModuleRemotePath.TabIndex = 9;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(4, 78);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(89, 23);
            label3.TabIndex = 8;
            label3.Text = "Local path";
            // 
            // SubModuleLocalPath
            // 
            SubModuleLocalPath.DataBindings.Add(new Binding("Text", this.gitSubmoduleBindingSource, "LocalPath", true));
            SubModuleLocalPath.Dock = DockStyle.Top;
            SubModuleLocalPath.Location = new Point(120, 75);
            SubModuleLocalPath.Name = "SubModuleLocalPath";
            SubModuleLocalPath.ReadOnly = true;
            SubModuleLocalPath.Size = new Size(416, 30);
            SubModuleLocalPath.TabIndex = 10;
            // 
            // SubModuleCommit
            // 
            SubModuleCommit.DataBindings.Add(new Binding("Text", this.gitSubmoduleBindingSource, "CurrentCommitId", true));
            SubModuleCommit.Dock = DockStyle.Top;
            SubModuleCommit.Location = new Point(120, 111);
            SubModuleCommit.Name = "SubModuleCommit";
            SubModuleCommit.ReadOnly = true;
            SubModuleCommit.Size = new Size(416, 30);
            SubModuleCommit.TabIndex = 17;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.AutoSize = true;
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.Controls.Add(SubModuleBranch, 0, 0);
            tableLayoutPanel4.Controls.Add(flowLayoutPanel2, 1, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(117, 144);
            tableLayoutPanel4.Margin = new Padding(0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(422, 36);
            tableLayoutPanel4.TabIndex = 18;
            // 
            // SubModuleBranch
            // 
            SubModuleBranch.DataBindings.Add(new Binding("Text", this.gitSubmoduleBindingSource, "Branch", true));
            SubModuleBranch.Dock = DockStyle.Top;
            SubModuleBranch.Location = new Point(3, 3);
            SubModuleBranch.Name = "SubModuleBranch";
            SubModuleBranch.ReadOnly = true;
            SubModuleBranch.Size = new Size(386, 30);
            SubModuleBranch.TabIndex = 13;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Anchor = AnchorStyles.None;
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Controls.Add(Pull);
            flowLayoutPanel2.Location = new Point(395, 6);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(24, 24);
            flowLayoutPanel2.TabIndex = 14;
            // 
            // Pull
            // 
            Pull.Anchor = AnchorStyles.Bottom;
            Pull.BackgroundImage = Properties.Images.Pull;
            Pull.BackgroundImageLayout = ImageLayout.Zoom;
            Pull.Location = new Point(0, 0);
            Pull.Margin = new Padding(0);
            Pull.MinimumSize = new Size(24, 24);
            Pull.Name = "Pull";
            Pull.Size = new Size(24, 24);
            Pull.TabIndex = 22;
            Pull.UseVisualStyleBackColor = true;
            Pull.Click += Pull_Click;
            // 
            // FormSubmodules
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(782, 372);
            Controls.Add(splitContainer1);
            Margin = new Padding(4);
            MinimumSize = new Size(710, 350);
            Name = "FormSubmodules";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Submodules";
            Shown += FormSubmodulesShown;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();

            splitContainer1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(Submodules)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(gitSubmoduleBindingSource)).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            groupBox1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private DataGridView Submodules;
        private Button AddSubmodule;
        private BindingSource gitSubmoduleBindingSource;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn Status;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox SubModuleStatus;
        private Label label6;
        private Label label1;
        private Label label2;
        private Label label5;
        private TextBox SubModuleName;
        private Label label4;
        private TextBox SubModuleRemotePath;
        private Label label3;
        private TextBox SubModuleLocalPath;
        private GroupBox groupBox1;
        private Button SynchronizeSubmodule;
        private Button UpdateSubmodule;
        private Button RemoveSubmodule;
        private FlowLayoutPanel flowLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TextBox SubModuleCommit;
        private TableLayoutPanel tableLayoutPanel4;
        private TextBox SubModuleBranch;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button Pull;
    }
}