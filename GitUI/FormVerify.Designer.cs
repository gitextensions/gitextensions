namespace GitUI
{
    partial class FormVerify
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormVerify));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.ShowOnlyCommits = new System.Windows.Forms.CheckBox();
            this.NoReflogs = new System.Windows.Forms.CheckBox();
            this.FullCheck = new System.Windows.Forms.CheckBox();
            this.Unreachable = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Warnings = new System.Windows.Forms.DataGridView();
            this.columnIsLostObjectSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSubject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnHash = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mnuLostObjects = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuLostObjectView = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLostObjectCreateTag = new System.Windows.Forms.ToolStripMenuItem();
            this.TagAllCommits = new System.Windows.Forms.Button();
            this.DeleteAllLostAndFoundTags = new System.Windows.Forms.Button();
            this.TagAllObjects = new System.Windows.Forms.Button();
            this.Remove = new System.Windows.Forms.Button();
            this.SaveObjects = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Warnings)).BeginInit();
            this.mnuLostObjects.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.TagAllCommits);
            this.splitContainer1.Panel2.Controls.Add(this.DeleteAllLostAndFoundTags);
            this.splitContainer1.Panel2.Controls.Add(this.TagAllObjects);
            this.splitContainer1.Panel2.Controls.Add(this.Remove);
            this.splitContainer1.Panel2.Controls.Add(this.SaveObjects);
            this.splitContainer1.Size = new System.Drawing.Size(859, 524);
            this.splitContainer1.SplitterDistance = 466;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.ShowOnlyCommits);
            this.splitContainer2.Panel1.Controls.Add(this.NoReflogs);
            this.splitContainer2.Panel1.Controls.Add(this.FullCheck);
            this.splitContainer2.Panel1.Controls.Add(this.Unreachable);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.Warnings);
            this.splitContainer2.Size = new System.Drawing.Size(859, 466);
            this.splitContainer2.SplitterDistance = 137;
            this.splitContainer2.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(320, 105);
            this.label2.TabIndex = 5;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // ShowOnlyCommits
            // 
            this.ShowOnlyCommits.AutoSize = true;
            this.ShowOnlyCommits.Checked = true;
            this.ShowOnlyCommits.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowOnlyCommits.Location = new System.Drawing.Point(430, 3);
            this.ShowOnlyCommits.Name = "ShowOnlyCommits";
            this.ShowOnlyCommits.Size = new System.Drawing.Size(131, 19);
            this.ShowOnlyCommits.TabIndex = 4;
            this.ShowOnlyCommits.Text = "Show only commits";
            this.ShowOnlyCommits.UseVisualStyleBackColor = true;
            this.ShowOnlyCommits.CheckedChanged += new System.EventHandler(this.ShowOnlyCommitsCheckedChanged);
            // 
            // NoReflogs
            // 
            this.NoReflogs.AutoSize = true;
            this.NoReflogs.Checked = true;
            this.NoReflogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.NoReflogs.Location = new System.Drawing.Point(430, 29);
            this.NoReflogs.Name = "NoReflogs";
            this.NoReflogs.Size = new System.Drawing.Size(375, 34);
            this.NoReflogs.TabIndex = 3;
            this.NoReflogs.Text = "Do not consider commits that are referenced only by an entry in a \r\nreflog to be " +
    "reachable.";
            this.NoReflogs.UseVisualStyleBackColor = true;
            this.NoReflogs.CheckedChanged += new System.EventHandler(this.NoReflogsCheckedChanged);
            // 
            // FullCheck
            // 
            this.FullCheck.AutoSize = true;
            this.FullCheck.Location = new System.Drawing.Point(430, 95);
            this.FullCheck.Name = "FullCheck";
            this.FullCheck.Size = new System.Drawing.Size(397, 34);
            this.FullCheck.TabIndex = 2;
            this.FullCheck.Text = "Check not just objects in GIT_OBJECT_DIRECTORY ($GIT_DIR/objects), \r\nbut also the" +
    " ones found in alternate object pools.\r\n";
            this.FullCheck.UseVisualStyleBackColor = true;
            this.FullCheck.CheckedChanged += new System.EventHandler(this.FullCheckCheckedChanged);
            // 
            // Unreachable
            // 
            this.Unreachable.AutoSize = true;
            this.Unreachable.Location = new System.Drawing.Point(430, 62);
            this.Unreachable.Name = "Unreachable";
            this.Unreachable.Size = new System.Drawing.Size(429, 34);
            this.Unreachable.TabIndex = 1;
            this.Unreachable.Text = "Print out objects that exist but that aren\'t readable from any of the reference \r" +
    "\nnodes.\r\n";
            this.Unreachable.UseVisualStyleBackColor = true;
            this.Unreachable.CheckedChanged += new System.EventHandler(this.UnreachableCheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 116);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(299, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Double-click on a row containing a sha1 to view object.";
            // 
            // Warnings
            // 
            this.Warnings.AllowUserToAddRows = false;
            this.Warnings.AllowUserToDeleteRows = false;
            this.Warnings.AllowUserToOrderColumns = true;
            this.Warnings.AllowUserToResizeRows = false;
            this.Warnings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Warnings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnIsLostObjectSelected,
            this.columnDate,
            this.columnType,
            this.columnSubject,
            this.columnAuthor,
            this.columnHash});
            this.Warnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Warnings.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.Warnings.Location = new System.Drawing.Point(0, 0);
            this.Warnings.MultiSelect = false;
            this.Warnings.Name = "Warnings";
            this.Warnings.RowHeadersVisible = false;
            this.Warnings.RowTemplate.ContextMenuStrip = this.mnuLostObjects;
            this.Warnings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Warnings.Size = new System.Drawing.Size(859, 325);
            this.Warnings.TabIndex = 0;
            this.Warnings.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Warnings_CellMouseDown);
            this.Warnings.DoubleClick += new System.EventHandler(this.WarningsDoubleClick);
            // 
            // columnIsLostObjectSelected
            // 
            this.columnIsLostObjectSelected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnIsLostObjectSelected.DataPropertyName = "IsSelected";
            this.columnIsLostObjectSelected.HeaderText = "";
            this.columnIsLostObjectSelected.MinimumWidth = 10;
            this.columnIsLostObjectSelected.Name = "columnIsLostObjectSelected";
            this.columnIsLostObjectSelected.Width = 10;
            // 
            // columnDate
            // 
            this.columnDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnDate.DataPropertyName = "Date";
            this.columnDate.HeaderText = "Date";
            this.columnDate.Name = "columnDate";
            this.columnDate.Width = 56;
            // 
            // columnType
            // 
            this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnType.DataPropertyName = "RawType";
            this.columnType.HeaderText = "Type";
            this.columnType.Name = "columnType";
            this.columnType.Width = 58;
            // 
            // columnSubject
            // 
            this.columnSubject.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnSubject.DataPropertyName = "Subject";
            this.columnSubject.HeaderText = "Subject";
            this.columnSubject.Name = "columnSubject";
            // 
            // columnAuthor
            // 
            this.columnAuthor.DataPropertyName = "Author";
            this.columnAuthor.HeaderText = "Author";
            this.columnAuthor.Name = "columnAuthor";
            this.columnAuthor.Width = 150;
            // 
            // columnHash
            // 
            this.columnHash.DataPropertyName = "Hash";
            this.columnHash.HeaderText = "Hash";
            this.columnHash.Name = "columnHash";
            this.columnHash.Width = 80;
            // 
            // mnuLostObjects
            // 
            this.mnuLostObjects.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLostObjectView,
            this.mnuLostObjectCreateTag});
            this.mnuLostObjects.Name = "mnuLostObjects";
            this.mnuLostObjects.Size = new System.Drawing.Size(129, 48);
            // 
            // mnuLostObjectView
            // 
            this.mnuLostObjectView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.mnuLostObjectView.Name = "mnuLostObjectView";
            this.mnuLostObjectView.Size = new System.Drawing.Size(128, 22);
            this.mnuLostObjectView.Text = "View";
            this.mnuLostObjectView.Click += new System.EventHandler(this.mnuLostObjectView_Click);
            // 
            // mnuLostObjectCreateTag
            // 
            this.mnuLostObjectCreateTag.Name = "mnuLostObjectCreateTag";
            this.mnuLostObjectCreateTag.Size = new System.Drawing.Size(128, 22);
            this.mnuLostObjectCreateTag.Text = "Create tag";
            this.mnuLostObjectCreateTag.Click += new System.EventHandler(this.mnuLostObjectCreateTag_Click);
            // 
            // TagAllCommits
            // 
            this.TagAllCommits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TagAllCommits.Location = new System.Drawing.Point(300, 3);
            this.TagAllCommits.Name = "TagAllCommits";
            this.TagAllCommits.Size = new System.Drawing.Size(173, 25);
            this.TagAllCommits.TabIndex = 6;
            this.TagAllCommits.Text = "Tag all lost commits";
            this.TagAllCommits.UseVisualStyleBackColor = true;
            this.TagAllCommits.Click += new System.EventHandler(this.TagAllCommitsClick);
            // 
            // DeleteAllLostAndFoundTags
            // 
            this.DeleteAllLostAndFoundTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DeleteAllLostAndFoundTags.Location = new System.Drawing.Point(4, 29);
            this.DeleteAllLostAndFoundTags.Name = "DeleteAllLostAndFoundTags";
            this.DeleteAllLostAndFoundTags.Size = new System.Drawing.Size(290, 25);
            this.DeleteAllLostAndFoundTags.TabIndex = 5;
            this.DeleteAllLostAndFoundTags.Text = "Delete all LOST_AND_FOUND tags";
            this.DeleteAllLostAndFoundTags.UseVisualStyleBackColor = true;
            this.DeleteAllLostAndFoundTags.Click += new System.EventHandler(this.DeleteAllLostAndFoundTagsClick);
            // 
            // TagAllObjects
            // 
            this.TagAllObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TagAllObjects.Location = new System.Drawing.Point(150, 3);
            this.TagAllObjects.Name = "TagAllObjects";
            this.TagAllObjects.Size = new System.Drawing.Size(144, 25);
            this.TagAllObjects.TabIndex = 4;
            this.TagAllObjects.Text = "Tag all lost objects";
            this.TagAllObjects.UseVisualStyleBackColor = true;
            this.TagAllObjects.Click += new System.EventHandler(this.TagAllObjectsClick);
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Remove.Location = new System.Drawing.Point(644, 3);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(207, 25);
            this.Remove.TabIndex = 1;
            this.Remove.Text = "Remove all dangling objects";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.RemoveClick);
            // 
            // SaveObjects
            // 
            this.SaveObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveObjects.Location = new System.Drawing.Point(644, 29);
            this.SaveObjects.Name = "SaveObjects";
            this.SaveObjects.Size = new System.Drawing.Size(208, 25);
            this.SaveObjects.TabIndex = 0;
            this.SaveObjects.Text = "Save objects to .git/lost-found";
            this.SaveObjects.UseVisualStyleBackColor = true;
            this.SaveObjects.Click += new System.EventHandler(this.SaveObjectsClick);
            // 
            // FormVerify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(859, 524);
            this.Controls.Add(this.splitContainer1);
            this.MinimizeBox = false;
            this.Name = "FormVerify";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Verify database";
            this.Shown += new System.EventHandler(this.FormVerifyShown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Warnings)).EndInit();
            this.mnuLostObjects.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button SaveObjects;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.CheckBox Unreachable;
        private System.Windows.Forms.Button TagAllObjects;
        private System.Windows.Forms.Button DeleteAllLostAndFoundTags;
        private System.Windows.Forms.CheckBox FullCheck;
        private System.Windows.Forms.CheckBox NoReflogs;
        private System.Windows.Forms.Button TagAllCommits;
        private System.Windows.Forms.CheckBox ShowOnlyCommits;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView Warnings;
        private System.Windows.Forms.ContextMenuStrip mnuLostObjects;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectCreateTag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnIsLostObjectSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSubject;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHash;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectView;
    }
}