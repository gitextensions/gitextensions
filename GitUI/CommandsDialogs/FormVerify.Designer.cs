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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Panel panel1;
            System.Windows.Forms.Panel panel2;
            System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormVerify));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCloseDialog = new System.Windows.Forms.Button();
            this.btnRestoreSelectedObjects = new System.Windows.Forms.Button();
            this.DeleteAllLostAndFoundTags = new System.Windows.Forms.Button();
            this.Remove = new System.Windows.Forms.Button();
            this.SaveObjects = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ShowOtherObjects = new System.Windows.Forms.CheckBox();
            this.ShowCommitsAndTags = new System.Windows.Forms.CheckBox();
            this.NoReflogs = new System.Windows.Forms.CheckBox();
            this.FullCheck = new System.Windows.Forms.CheckBox();
            this.Unreachable = new System.Windows.Forms.CheckBox();
            this.mnuLostObjects = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuLostObjectView = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLostObjectsCreateTag = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLostObjectsCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.copyHashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Warnings = new System.Windows.Forms.DataGridView();
            this.copyParentHashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.columnIsLostObjectSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSubject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnHash = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnParent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panel1 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            this.mnuLostObjects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Warnings)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(this.btnCloseDialog);
            panel1.Controls.Add(this.btnRestoreSelectedObjects);
            panel1.Controls.Add(this.DeleteAllLostAndFoundTags);
            panel1.Controls.Add(this.Remove);
            panel1.Controls.Add(this.SaveObjects);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 514);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(859, 61);
            panel1.TabIndex = 1;
            // 
            // btnCloseDialog
            // 
            this.btnCloseDialog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCloseDialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCloseDialog.Location = new System.Drawing.Point(647, 33);
            this.btnCloseDialog.Name = "btnCloseDialog";
            this.btnCloseDialog.Size = new System.Drawing.Size(208, 25);
            this.btnCloseDialog.TabIndex = 9;
            this.btnCloseDialog.Text = "Cancel";
            this.btnCloseDialog.UseVisualStyleBackColor = true;
            // 
            // btnRestoreSelectedObjects
            // 
            this.btnRestoreSelectedObjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestoreSelectedObjects.Location = new System.Drawing.Point(298, 21);
            this.btnRestoreSelectedObjects.Name = "btnRestoreSelectedObjects";
            this.btnRestoreSelectedObjects.Size = new System.Drawing.Size(317, 25);
            this.btnRestoreSelectedObjects.TabIndex = 5;
            this.btnRestoreSelectedObjects.Text = "Recover selected objects";
            this.btnRestoreSelectedObjects.UseVisualStyleBackColor = true;
            this.btnRestoreSelectedObjects.Click += new System.EventHandler(this.btnRestoreSelectedObjects_Click);
            // 
            // DeleteAllLostAndFoundTags
            // 
            this.DeleteAllLostAndFoundTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DeleteAllLostAndFoundTags.Location = new System.Drawing.Point(3, 33);
            this.DeleteAllLostAndFoundTags.Name = "DeleteAllLostAndFoundTags";
            this.DeleteAllLostAndFoundTags.Size = new System.Drawing.Size(252, 25);
            this.DeleteAllLostAndFoundTags.TabIndex = 7;
            this.DeleteAllLostAndFoundTags.Text = "Delete all LOST_AND_FOUND tags";
            this.DeleteAllLostAndFoundTags.UseVisualStyleBackColor = true;
            this.DeleteAllLostAndFoundTags.Click += new System.EventHandler(this.DeleteAllLostAndFoundTagsClick);
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Remove.Location = new System.Drawing.Point(3, 6);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(252, 25);
            this.Remove.TabIndex = 6;
            this.Remove.Text = "Remove all dangling objects";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.RemoveClick);
            // 
            // SaveObjects
            // 
            this.SaveObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveObjects.Location = new System.Drawing.Point(647, 6);
            this.SaveObjects.Name = "SaveObjects";
            this.SaveObjects.Size = new System.Drawing.Size(208, 25);
            this.SaveObjects.TabIndex = 8;
            this.SaveObjects.Text = "Save objects to .git/lost-found";
            this.SaveObjects.UseVisualStyleBackColor = true;
            this.SaveObjects.Click += new System.EventHandler(this.SaveObjectsClick);
            // 
            // panel2
            // 
            panel2.AutoSize = true;
            panel2.Controls.Add(flowLayoutPanel1);
            panel2.Controls.Add(this.ShowOtherObjects);
            panel2.Controls.Add(this.ShowCommitsAndTags);
            panel2.Controls.Add(this.NoReflogs);
            panel2.Controls.Add(this.FullCheck);
            panel2.Controls.Add(this.Unreachable);
            panel2.Dock = System.Windows.Forms.DockStyle.Top;
            panel2.Location = new System.Drawing.Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(859, 134);
            panel2.TabIndex = 4;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(this.label2);
            flowLayoutPanel1.Controls.Add(this.label1);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            flowLayoutPanel1.Size = new System.Drawing.Size(322, 134);
            flowLayoutPanel1.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(306, 91);
            this.label2.TabIndex = 15;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 101);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Double-click on a row for quick view";
            // 
            // ShowOtherObjects
            // 
            this.ShowOtherObjects.AutoSize = true;
            this.ShowOtherObjects.Location = new System.Drawing.Point(588, 9);
            this.ShowOtherObjects.Name = "ShowOtherObjects";
            this.ShowOtherObjects.Size = new System.Drawing.Size(119, 17);
            this.ShowOtherObjects.TabIndex = 0;
            this.ShowOtherObjects.Text = "Show other objects";
            this.ShowOtherObjects.UseVisualStyleBackColor = true;
            this.ShowOtherObjects.CheckedChanged += new System.EventHandler(this.ShowOtherObjects_CheckedChanged);
            // 
            // ShowCommitsAndTags
            // 
            this.ShowCommitsAndTags.AutoSize = true;
            this.ShowCommitsAndTags.Checked = true;
            this.ShowCommitsAndTags.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowCommitsAndTags.Location = new System.Drawing.Point(430, 9);
            this.ShowCommitsAndTags.Name = "ShowCommitsAndTags";
            this.ShowCommitsAndTags.Size = new System.Drawing.Size(138, 17);
            this.ShowCommitsAndTags.TabIndex = 0;
            this.ShowCommitsAndTags.Text = "Show commits and tags";
            this.ShowCommitsAndTags.UseVisualStyleBackColor = true;
            this.ShowCommitsAndTags.CheckedChanged += new System.EventHandler(this.ShowCommitsCheckedChanged);
            // 
            // NoReflogs
            // 
            this.NoReflogs.AutoSize = true;
            this.NoReflogs.Checked = true;
            this.NoReflogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.NoReflogs.Location = new System.Drawing.Point(430, 35);
            this.NoReflogs.Name = "NoReflogs";
            this.NoReflogs.Size = new System.Drawing.Size(345, 30);
            this.NoReflogs.TabIndex = 1;
            this.NoReflogs.Text = "Do not consider commits that are referenced only by an entry in a \r\nreflog to be " +
    "reachable.";
            this.NoReflogs.UseVisualStyleBackColor = true;
            this.NoReflogs.CheckedChanged += new System.EventHandler(this.NoReflogsCheckedChanged);
            // 
            // FullCheck
            // 
            this.FullCheck.AutoSize = true;
            this.FullCheck.Location = new System.Drawing.Point(430, 101);
            this.FullCheck.Name = "FullCheck";
            this.FullCheck.Size = new System.Drawing.Size(376, 30);
            this.FullCheck.TabIndex = 3;
            this.FullCheck.Text = "Check not just objects in GIT_OBJECT_DIRECTORY ($GIT_DIR/objects), \r\nbut also the" +
    " ones found in alternate object pools.\r\n";
            this.FullCheck.UseVisualStyleBackColor = true;
            this.FullCheck.CheckedChanged += new System.EventHandler(this.FullCheckCheckedChanged);
            // 
            // Unreachable
            // 
            this.Unreachable.AutoSize = true;
            this.Unreachable.Location = new System.Drawing.Point(430, 68);
            this.Unreachable.Name = "Unreachable";
            this.Unreachable.Size = new System.Drawing.Size(403, 30);
            this.Unreachable.TabIndex = 2;
            this.Unreachable.Text = "Print out objects that exist but that aren\'t readable from any of the reference \r" +
    "\nnodes.\r\n";
            this.Unreachable.UseVisualStyleBackColor = true;
            this.Unreachable.CheckedChanged += new System.EventHandler(this.UnreachableCheckedChanged);
            // 
            // mnuLostObjects
            // 
            this.mnuLostObjects.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLostObjectView,
            this.mnuLostObjectsCreateTag,
            this.mnuLostObjectsCreateBranch,
            this.copyHashToolStripMenuItem,
            this.copyParentHashToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.mnuLostObjects.Name = "mnuLostObjects";
            this.mnuLostObjects.Size = new System.Drawing.Size(190, 114);
            this.mnuLostObjects.Opening += new System.ComponentModel.CancelEventHandler(this.mnuLostObjects_Opening);
            // 
            // mnuLostObjectView
            // 
            this.mnuLostObjectView.Image = global::GitUI.Properties.Images.ViewFile;
            this.mnuLostObjectView.Name = "mnuLostObjectView";
            this.mnuLostObjectView.Size = new System.Drawing.Size(189, 22);
            this.mnuLostObjectView.Text = "View";
            this.mnuLostObjectView.Click += new System.EventHandler(this.mnuLostObjectView_Click);
            // 
            // mnuLostObjectsCreateTag
            // 
            this.mnuLostObjectsCreateTag.Image = global::GitUI.Properties.Images.TagCreate;
            this.mnuLostObjectsCreateTag.Name = "mnuLostObjectsCreateTag";
            this.mnuLostObjectsCreateTag.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.mnuLostObjectsCreateTag.Size = new System.Drawing.Size(189, 22);
            this.mnuLostObjectsCreateTag.Text = "Create tag";
            this.mnuLostObjectsCreateTag.Click += new System.EventHandler(this.mnuLostObjectsCreateTag_Click);
            // 
            // mnuLostObjectsCreateBranch
            // 
            this.mnuLostObjectsCreateBranch.Image = global::GitUI.Properties.Images.BranchCreate;
            this.mnuLostObjectsCreateBranch.Name = "mnuLostObjectsCreateBranch";
            this.mnuLostObjectsCreateBranch.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.mnuLostObjectsCreateBranch.Size = new System.Drawing.Size(189, 22);
            this.mnuLostObjectsCreateBranch.Text = "Create branch";
            this.mnuLostObjectsCreateBranch.Click += new System.EventHandler(this.mnuLostObjectsCreateBranch_Click);
            // 
            // copyHashToolStripMenuItem
            // 
            this.copyHashToolStripMenuItem.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyHashToolStripMenuItem.Name = "copyHashToolStripMenuItem";
            this.copyHashToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.copyHashToolStripMenuItem.Text = "Copy object hash";
            this.copyHashToolStripMenuItem.Click += new System.EventHandler(this.copyHashToolStripMenuItem_Click);
            // 
            // Warnings
            // 
            this.Warnings.AllowUserToAddRows = false;
            this.Warnings.AllowUserToDeleteRows = false;
            this.Warnings.AllowUserToOrderColumns = true;
            this.Warnings.AllowUserToResizeRows = false;
            this.Warnings.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.Warnings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Warnings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnIsLostObjectSelected,
            this.columnDate,
            this.columnType,
            this.columnSubject,
            this.columnAuthor,
            this.columnHash,
            this.columnParent});
            this.Warnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Warnings.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.Warnings.Location = new System.Drawing.Point(0, 134);
            this.Warnings.MultiSelect = false;
            this.Warnings.Name = "Warnings";
            this.Warnings.RowHeadersVisible = false;
            this.Warnings.RowTemplate.ContextMenuStrip = this.mnuLostObjects;
            this.Warnings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Warnings.ShowEditingIcon = false;
            this.Warnings.Size = new System.Drawing.Size(859, 380);
            this.Warnings.TabIndex = 4;
            this.Warnings.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Warnings_CellMouseDoubleClick);
            this.Warnings.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Warnings_CellMouseDown);
            this.Warnings.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Warnings_KeyDown);
            // 
            // copyParentHashToolStripMenuItem
            // 
            this.copyParentHashToolStripMenuItem.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyParentHashToolStripMenuItem.Name = "copyParentHashToolStripMenuItem";
            this.copyParentHashToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.copyParentHashToolStripMenuItem.Text = "Copy parent hash";
            this.copyParentHashToolStripMenuItem.Click += new System.EventHandler(this.copyParentHashToolStripMenuItem_Click);
            // 
            // columnIsLostObjectSelected
            // 
            this.columnIsLostObjectSelected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnIsLostObjectSelected.HeaderText = "";
            this.columnIsLostObjectSelected.MinimumWidth = 20;
            this.columnIsLostObjectSelected.Name = "columnIsLostObjectSelected";
            this.columnIsLostObjectSelected.Width = 20;
            // 
            // columnDate
            // 
            this.columnDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnDate.HeaderText = "Date";
            this.columnDate.Name = "columnDate";
            this.columnDate.ReadOnly = true;
            this.columnDate.Width = 55;
            // 
            // columnType
            // 
            this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnType.HeaderText = "Type";
            this.columnType.Name = "columnType";
            this.columnType.ReadOnly = true;
            this.columnType.Width = 56;
            // 
            // columnSubject
            // 
            this.columnSubject.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.columnSubject.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnSubject.HeaderText = "Subject";
            this.columnSubject.Name = "columnSubject";
            this.columnSubject.ReadOnly = true;
            // 
            // columnAuthor
            // 
            this.columnAuthor.HeaderText = "Author";
            this.columnAuthor.Name = "columnAuthor";
            this.columnAuthor.ReadOnly = true;
            // 
            // columnHash
            // 
            this.columnHash.HeaderText = "Hash";
            this.columnHash.Name = "columnHash";
            this.columnHash.ReadOnly = true;
            // 
            // columnParent
            // 
            this.columnParent.HeaderText = "Parent(s) hashs";
            this.columnParent.Name = "columnParent";
            this.columnParent.ReadOnly = true;
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::GitUI.Properties.Images.SaveAs;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // FormVerify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnCloseDialog;
            this.ClientSize = new System.Drawing.Size(859, 575);
            this.Controls.Add(this.Warnings);
            this.Controls.Add(panel2);
            this.Controls.Add(panel1);
            this.MinimizeBox = false;
            this.Name = "FormVerify";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Verify database";
            this.Shown += new System.EventHandler(this.FormVerifyShown);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            this.mnuLostObjects.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Warnings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip mnuLostObjects;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectsCreateTag;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectView;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectsCreateBranch;
        private System.Windows.Forms.Button btnCloseDialog;
        private System.Windows.Forms.Button btnRestoreSelectedObjects;
        private System.Windows.Forms.Button DeleteAllLostAndFoundTags;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button SaveObjects;
        private System.Windows.Forms.DataGridView Warnings;
        private System.Windows.Forms.CheckBox ShowCommitsAndTags;
        private System.Windows.Forms.CheckBox NoReflogs;
        private System.Windows.Forms.CheckBox FullCheck;
        private System.Windows.Forms.CheckBox Unreachable;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem copyHashToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyParentHashToolStripMenuItem;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnIsLostObjectSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSubject;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHash;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnParent;
        private System.Windows.Forms.CheckBox ShowOtherObjects;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
    }
}