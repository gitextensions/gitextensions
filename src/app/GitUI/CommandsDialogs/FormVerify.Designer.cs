namespace GitUI.CommandsDialogs
{
    partial class FormVerify
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Panel panel1;
            Panel panel2;
            FlowLayoutPanel flowLayoutPanel1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormVerify));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            btnCloseDialog = new Button();
            btnRestoreSelectedObjects = new Button();
            DeleteAllLostAndFoundTags = new Button();
            Remove = new Button();
            SaveObjects = new Button();
            label2 = new Label();
            label1 = new Label();
            ShowOtherObjects = new CheckBox();
            ShowCommitsAndTags = new CheckBox();
            NoReflogs = new CheckBox();
            FullCheck = new CheckBox();
            Unreachable = new CheckBox();
            splitContainer1 = new SplitContainer();
            Warnings = new DataGridView();
            columnIsLostObjectSelected = new DataGridViewCheckBoxColumn();
            columnDate = new DataGridViewTextBoxColumn();
            columnType = new DataGridViewTextBoxColumn();
            columnSubject = new DataGridViewTextBoxColumn();
            columnAuthor = new DataGridViewTextBoxColumn();
            columnHash = new DataGridViewTextBoxColumn();
            columnParent = new DataGridViewTextBoxColumn();
            mnuLostObjects = new ContextMenuStrip(components);
            mnuLostObjectView = new ToolStripMenuItem();
            mnuLostObjectsCreateTag = new ToolStripMenuItem();
            mnuLostObjectsCreateBranch = new ToolStripMenuItem();
            copyHashToolStripMenuItem = new ToolStripMenuItem();
            copyParentHashToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            fileViewer = new Editor.FileViewer();
            toolTip = new ToolTip(components);
            panel1 = new Panel();
            panel2 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Warnings).BeginInit();
            mnuLostObjects.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnCloseDialog);
            panel1.Controls.Add(btnRestoreSelectedObjects);
            panel1.Controls.Add(DeleteAllLostAndFoundTags);
            panel1.Controls.Add(Remove);
            panel1.Controls.Add(SaveObjects);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 514);
            panel1.Name = "panel1";
            panel1.Size = new Size(859, 61);
            panel1.TabIndex = 1;
            // 
            // btnCloseDialog
            // 
            btnCloseDialog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCloseDialog.DialogResult = DialogResult.Cancel;
            btnCloseDialog.Location = new Point(647, 33);
            btnCloseDialog.Name = "btnCloseDialog";
            btnCloseDialog.Size = new Size(208, 25);
            btnCloseDialog.TabIndex = 9;
            btnCloseDialog.Text = "Cancel";
            btnCloseDialog.UseVisualStyleBackColor = true;
            // 
            // btnRestoreSelectedObjects
            // 
            btnRestoreSelectedObjects.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnRestoreSelectedObjects.Location = new Point(298, 21);
            btnRestoreSelectedObjects.Name = "btnRestoreSelectedObjects";
            btnRestoreSelectedObjects.Size = new Size(317, 25);
            btnRestoreSelectedObjects.TabIndex = 5;
            btnRestoreSelectedObjects.Text = "Recover selected objects";
            btnRestoreSelectedObjects.UseVisualStyleBackColor = true;
            btnRestoreSelectedObjects.Click += btnRestoreSelectedObjects_Click;
            // 
            // DeleteAllLostAndFoundTags
            // 
            DeleteAllLostAndFoundTags.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DeleteAllLostAndFoundTags.Location = new Point(3, 33);
            DeleteAllLostAndFoundTags.Name = "DeleteAllLostAndFoundTags";
            DeleteAllLostAndFoundTags.Size = new Size(252, 25);
            DeleteAllLostAndFoundTags.TabIndex = 7;
            DeleteAllLostAndFoundTags.Text = "Delete all LOST_AND_FOUND tags";
            DeleteAllLostAndFoundTags.UseVisualStyleBackColor = true;
            DeleteAllLostAndFoundTags.Click += DeleteAllLostAndFoundTagsClick;
            // 
            // Remove
            // 
            Remove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Remove.Location = new Point(3, 6);
            Remove.Name = "Remove";
            Remove.Size = new Size(252, 25);
            Remove.TabIndex = 6;
            Remove.Text = "Remove all dangling objects";
            Remove.UseVisualStyleBackColor = true;
            Remove.Click += RemoveClick;
            // 
            // SaveObjects
            // 
            SaveObjects.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            SaveObjects.Location = new Point(647, 6);
            SaveObjects.Name = "SaveObjects";
            SaveObjects.Size = new Size(208, 25);
            SaveObjects.TabIndex = 8;
            SaveObjects.Text = "Save objects to .git/lost-found";
            SaveObjects.UseVisualStyleBackColor = true;
            SaveObjects.Click += SaveObjectsClick;
            // 
            // panel2
            // 
            panel2.AutoSize = true;
            panel2.Controls.Add(flowLayoutPanel1);
            panel2.Controls.Add(ShowOtherObjects);
            panel2.Controls.Add(ShowCommitsAndTags);
            panel2.Controls.Add(NoReflogs);
            panel2.Controls.Add(FullCheck);
            panel2.Controls.Add(Unreachable);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(859, 138);
            panel2.TabIndex = 4;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Dock = DockStyle.Left;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(5);
            flowLayoutPanel1.Size = new Size(351, 138);
            flowLayoutPanel1.TabIndex = 13;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 5);
            label2.Name = "label2";
            label2.Size = new Size(335, 105);
            label2.TabIndex = 15;
            label2.Text = resources.GetString("label2.Text");
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 115);
            label1.Margin = new Padding(3, 5, 3, 0);
            label1.Name = "label1";
            label1.Size = new Size(200, 15);
            label1.TabIndex = 16;
            label1.Text = "Double-click on a row for quick view";
            // 
            // ShowOtherObjects
            // 
            ShowOtherObjects.AutoSize = true;
            ShowOtherObjects.Location = new Point(660, 9);
            ShowOtherObjects.Name = "ShowOtherObjects";
            ShowOtherObjects.Size = new Size(138, 19);
            ShowOtherObjects.TabIndex = 0;
            ShowOtherObjects.Text = "Show blobs and trees";
            toolTip.SetToolTip(ShowOtherObjects, "To recover contents of files once staged but mistakenly deleted");
            ShowOtherObjects.UseVisualStyleBackColor = true;
            ShowOtherObjects.CheckedChanged += ShowOtherObjects_CheckedChanged;
            // 
            // ShowCommitsAndTags
            // 
            ShowCommitsAndTags.AutoSize = true;
            ShowCommitsAndTags.Checked = true;
            ShowCommitsAndTags.CheckState = CheckState.Checked;
            ShowCommitsAndTags.Location = new Point(430, 9);
            ShowCommitsAndTags.Name = "ShowCommitsAndTags";
            ShowCommitsAndTags.Size = new Size(210, 19);
            ShowCommitsAndTags.TabIndex = 0;
            ShowCommitsAndTags.Text = "Show commits and annotated tags";
            toolTip.SetToolTip(ShowCommitsAndTags, "To recover unreachable commits or annotated tags");
            ShowCommitsAndTags.UseVisualStyleBackColor = true;
            ShowCommitsAndTags.CheckedChanged += ShowCommitsCheckedChanged;
            // 
            // NoReflogs
            // 
            NoReflogs.AutoSize = true;
            NoReflogs.Checked = true;
            NoReflogs.CheckState = CheckState.Checked;
            NoReflogs.Location = new Point(430, 35);
            NoReflogs.Name = "NoReflogs";
            NoReflogs.Size = new Size(375, 34);
            NoReflogs.TabIndex = 1;
            NoReflogs.Text = "Do not consider commits that are referenced only by an entry in a \r\nreflog to be reachable.";
            NoReflogs.UseVisualStyleBackColor = true;
            NoReflogs.CheckedChanged += NoReflogsCheckedChanged;
            // 
            // FullCheck
            // 
            FullCheck.AutoSize = true;
            FullCheck.Location = new Point(430, 101);
            FullCheck.Name = "FullCheck";
            FullCheck.Size = new Size(392, 34);
            FullCheck.TabIndex = 3;
            FullCheck.Text = "Check not just objects in GIT_OBJECT_DIRECTORY ($GIT_DIR/objects), \r\nbut also the ones found in alternate object pools.\r\n";
            FullCheck.UseVisualStyleBackColor = true;
            FullCheck.CheckedChanged += FullCheckCheckedChanged;
            // 
            // Unreachable
            // 
            Unreachable.AutoSize = true;
            Unreachable.Location = new Point(430, 68);
            Unreachable.Name = "Unreachable";
            Unreachable.Size = new Size(430, 34);
            Unreachable.TabIndex = 2;
            Unreachable.Text = "Print out objects that exist but that aren't readable from any of the reference \r\nnodes.\r\n";
            Unreachable.UseVisualStyleBackColor = true;
            Unreachable.CheckedChanged += UnreachableCheckedChanged;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(0, 134);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(Warnings);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(fileViewer);
            splitContainer1.Size = new Size(859, 374);
            splitContainer1.SplitterDistance = 700;
            splitContainer1.TabIndex = 17;
            // 
            // Warnings
            // 
            Warnings.AllowUserToAddRows = false;
            Warnings.AllowUserToDeleteRows = false;
            Warnings.AllowUserToOrderColumns = true;
            Warnings.AllowUserToResizeRows = false;
            Warnings.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            Warnings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Warnings.Columns.AddRange(new DataGridViewColumn[] { columnIsLostObjectSelected, columnDate, columnType, columnSubject, columnAuthor, columnHash, columnParent });
            Warnings.Dock = DockStyle.Fill;
            Warnings.EditMode = DataGridViewEditMode.EditOnEnter;
            Warnings.Location = new Point(0, 0);
            Warnings.MultiSelect = false;
            Warnings.Name = "Warnings";
            Warnings.RowHeadersVisible = false;
            Warnings.RowTemplate.ContextMenuStrip = mnuLostObjects;
            Warnings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Warnings.ShowEditingIcon = false;
            Warnings.Size = new Size(700, 374);
            Warnings.TabIndex = 4;
            Warnings.CellMouseDoubleClick += Warnings_CellMouseDoubleClick;
            Warnings.CellMouseDown += Warnings_CellMouseDown;
            Warnings.SelectionChanged += Warnings_SelectionChanged;
            Warnings.KeyDown += Warnings_KeyDown;
            // 
            // columnIsLostObjectSelected
            // 
            columnIsLostObjectSelected.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIsLostObjectSelected.HeaderText = "";
            columnIsLostObjectSelected.MinimumWidth = 20;
            columnIsLostObjectSelected.Name = "columnIsLostObjectSelected";
            columnIsLostObjectSelected.Width = 20;
            // 
            // columnDate
            // 
            columnDate.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnDate.HeaderText = "Date";
            columnDate.Name = "columnDate";
            columnDate.ReadOnly = true;
            columnDate.Width = 56;
            // 
            // columnType
            // 
            columnType.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnType.HeaderText = "Type";
            columnType.Name = "columnType";
            columnType.ReadOnly = true;
            columnType.Width = 56;
            // 
            // columnSubject
            // 
            columnSubject.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            columnSubject.DefaultCellStyle = dataGridViewCellStyle1;
            columnSubject.HeaderText = "Subject";
            columnSubject.Name = "columnSubject";
            columnSubject.ReadOnly = true;
            // 
            // columnAuthor
            // 
            columnAuthor.HeaderText = "Author";
            columnAuthor.Name = "columnAuthor";
            columnAuthor.ReadOnly = true;
            // 
            // columnHash
            // 
            columnHash.HeaderText = "Hash";
            columnHash.Name = "columnHash";
            columnHash.ReadOnly = true;
            // 
            // columnParent
            // 
            columnParent.HeaderText = "Parent(s) hashs";
            columnParent.Name = "columnParent";
            columnParent.ReadOnly = true;
            // 
            // mnuLostObjects
            // 
            mnuLostObjects.Items.AddRange(new ToolStripItem[] { mnuLostObjectView, mnuLostObjectsCreateTag, mnuLostObjectsCreateBranch, copyHashToolStripMenuItem, copyParentHashToolStripMenuItem, saveAsToolStripMenuItem });
            mnuLostObjects.Name = "mnuLostObjects";
            mnuLostObjects.Size = new Size(190, 136);
            mnuLostObjects.Opening += mnuLostObjects_Opening;
            // 
            // mnuLostObjectView
            // 
            mnuLostObjectView.Image = Properties.Images.ViewFile;
            mnuLostObjectView.Name = "mnuLostObjectView";
            mnuLostObjectView.Size = new Size(189, 22);
            mnuLostObjectView.Text = "View";
            mnuLostObjectView.Click += mnuLostObjectView_Click;
            // 
            // mnuLostObjectsCreateTag
            // 
            mnuLostObjectsCreateTag.Image = Properties.Images.TagCreate;
            mnuLostObjectsCreateTag.Name = "mnuLostObjectsCreateTag";
            mnuLostObjectsCreateTag.ShortcutKeys = Keys.Control | Keys.T;
            mnuLostObjectsCreateTag.Size = new Size(189, 22);
            mnuLostObjectsCreateTag.Text = "Create tag";
            mnuLostObjectsCreateTag.Click += mnuLostObjectsCreateTag_Click;
            // 
            // mnuLostObjectsCreateBranch
            // 
            mnuLostObjectsCreateBranch.Image = Properties.Images.BranchCreate;
            mnuLostObjectsCreateBranch.Name = "mnuLostObjectsCreateBranch";
            mnuLostObjectsCreateBranch.ShortcutKeys = Keys.Control | Keys.B;
            mnuLostObjectsCreateBranch.Size = new Size(189, 22);
            mnuLostObjectsCreateBranch.Text = "Create branch";
            mnuLostObjectsCreateBranch.Click += mnuLostObjectsCreateBranch_Click;
            // 
            // copyHashToolStripMenuItem
            // 
            copyHashToolStripMenuItem.Image = Properties.Images.CopyToClipboard;
            copyHashToolStripMenuItem.Name = "copyHashToolStripMenuItem";
            copyHashToolStripMenuItem.Size = new Size(189, 22);
            copyHashToolStripMenuItem.Text = "Copy object hash";
            copyHashToolStripMenuItem.Click += copyHashToolStripMenuItem_Click;
            // 
            // copyParentHashToolStripMenuItem
            // 
            copyParentHashToolStripMenuItem.Image = Properties.Images.CopyToClipboard;
            copyParentHashToolStripMenuItem.Name = "copyParentHashToolStripMenuItem";
            copyParentHashToolStripMenuItem.Size = new Size(189, 22);
            copyParentHashToolStripMenuItem.Text = "Copy parent hash";
            copyParentHashToolStripMenuItem.Click += copyParentHashToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Image = Properties.Images.SaveAs;
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveAsToolStripMenuItem.Size = new Size(189, 22);
            saveAsToolStripMenuItem.Text = "Save as...";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // fileViewer
            // 
            fileViewer.Dock = DockStyle.Fill;
            fileViewer.EnableAutomaticContinuousScroll = false;
            fileViewer.Location = new Point(0, 0);
            fileViewer.Margin = new Padding(0);
            fileViewer.Name = "fileViewer";
            fileViewer.Size = new Size(155, 374);
            fileViewer.TabIndex = 0;
            // 
            // toolTip
            // 
            toolTip.AutomaticDelay = 0;
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 0;
            toolTip.ReshowDelay = 100;
            // 
            // FormVerify
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = btnCloseDialog;
            ClientSize = new Size(859, 575);
            Controls.Add(panel2);
            Controls.Add(splitContainer1);
            Controls.Add(panel1);
            MinimizeBox = false;
            Name = "FormVerify";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Verify database";
            Shown += FormVerifyShown;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Warnings).EndInit();
            mnuLostObjects.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ContextMenuStrip mnuLostObjects;
        private ToolStripMenuItem mnuLostObjectsCreateTag;
        private ToolStripMenuItem mnuLostObjectView;
        private ToolStripMenuItem mnuLostObjectsCreateBranch;
        private Button btnCloseDialog;
        private Button btnRestoreSelectedObjects;
        private Button DeleteAllLostAndFoundTags;
        private Button Remove;
        private Button SaveObjects;
        private DataGridView Warnings;
        private CheckBox ShowCommitsAndTags;
        private CheckBox NoReflogs;
        private CheckBox FullCheck;
        private CheckBox Unreachable;
        private Label label2;
        private Label label1;
        private ToolStripMenuItem copyHashToolStripMenuItem;
        private ToolStripMenuItem copyParentHashToolStripMenuItem;
        private DataGridViewCheckBoxColumn columnIsLostObjectSelected;
        private DataGridViewTextBoxColumn columnDate;
        private DataGridViewTextBoxColumn columnType;
        private DataGridViewTextBoxColumn columnSubject;
        private DataGridViewTextBoxColumn columnAuthor;
        private DataGridViewTextBoxColumn columnHash;
        private DataGridViewTextBoxColumn columnParent;
        private CheckBox ShowOtherObjects;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private SplitContainer splitContainer1;
        private Editor.FileViewer fileViewer;
        private ToolTip toolTip;
    }
}
