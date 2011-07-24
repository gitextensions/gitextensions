﻿namespace GitUI
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.mnuLostObjectsCreateTag = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLostObjectsCreateBranch = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCloseDialog = new System.Windows.Forms.Button();
            this.btnRestoreSelectedObjects = new System.Windows.Forms.Button();
            this.DeleteAllLostAndFoundTags = new System.Windows.Forms.Button();
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
            this.splitContainer1.Panel2.Controls.Add(this.btnCloseDialog);
            this.splitContainer1.Panel2.Controls.Add(this.btnRestoreSelectedObjects);
            this.splitContainer1.Panel2.Controls.Add(this.DeleteAllLostAndFoundTags);
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
            this.label2.Size = new System.Drawing.Size(286, 91);
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
            this.ShowOnlyCommits.Size = new System.Drawing.Size(116, 17);
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
            this.NoReflogs.Size = new System.Drawing.Size(345, 30);
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
            this.FullCheck.Size = new System.Drawing.Size(376, 30);
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
            this.Unreachable.Size = new System.Drawing.Size(403, 30);
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
            this.label1.Size = new System.Drawing.Size(270, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Double-click on a row containing a sha1 to view object.";
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
            this.columnHash});
            this.Warnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Warnings.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.Warnings.Location = new System.Drawing.Point(0, 0);
            this.Warnings.MultiSelect = false;
            this.Warnings.Name = "Warnings";
            this.Warnings.RowHeadersVisible = false;
            this.Warnings.RowTemplate.ContextMenuStrip = this.mnuLostObjects;
            this.Warnings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Warnings.ShowEditingIcon = false;
            this.Warnings.Size = new System.Drawing.Size(859, 325);
            this.Warnings.TabIndex = 0;
            this.Warnings.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Warnings_CellMouseDown);
            this.Warnings.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.Warnings_CellMouseDoubleClick);
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
            this.columnDate.ReadOnly = true;
            this.columnDate.Width = 55;
            // 
            // columnType
            // 
            this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnType.DataPropertyName = "RawType";
            this.columnType.HeaderText = "Type";
            this.columnType.Name = "columnType";
            this.columnType.ReadOnly = true;
            this.columnType.Width = 56;
            // 
            // columnSubject
            // 
            this.columnSubject.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnSubject.DataPropertyName = "Subject";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.columnSubject.DefaultCellStyle = dataGridViewCellStyle3;
            this.columnSubject.HeaderText = "Subject";
            this.columnSubject.Name = "columnSubject";
            this.columnSubject.ReadOnly = true;
            // 
            // columnAuthor
            // 
            this.columnAuthor.DataPropertyName = "Author";
            this.columnAuthor.HeaderText = "Author";
            this.columnAuthor.Name = "columnAuthor";
            this.columnAuthor.ReadOnly = true;
            this.columnAuthor.Width = 150;
            // 
            // columnHash
            // 
            this.columnHash.DataPropertyName = "Hash";
            this.columnHash.HeaderText = "Hash";
            this.columnHash.Name = "columnHash";
            this.columnHash.ReadOnly = true;
            this.columnHash.Width = 80;
            // 
            // mnuLostObjects
            // 
            this.mnuLostObjects.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLostObjectView,
            this.mnuLostObjectsCreateTag,
            this.mnuLostObjectsCreateBranch});
            this.mnuLostObjects.Name = "mnuLostObjects";
            this.mnuLostObjects.Size = new System.Drawing.Size(193, 70);
            // 
            // mnuLostObjectView
            // 
            this.mnuLostObjectView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.mnuLostObjectView.Name = "mnuLostObjectView";
            this.mnuLostObjectView.Size = new System.Drawing.Size(192, 22);
            this.mnuLostObjectView.Text = "View";
            this.mnuLostObjectView.Click += new System.EventHandler(this.mnuLostObjectView_Click);
            // 
            // mnuLostObjectsCreateTag
            // 
            this.mnuLostObjectsCreateTag.Name = "mnuLostObjectsCreateTag";
            this.mnuLostObjectsCreateTag.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.mnuLostObjectsCreateTag.Size = new System.Drawing.Size(192, 22);
            this.mnuLostObjectsCreateTag.Text = "Create tag";
            this.mnuLostObjectsCreateTag.Click += new System.EventHandler(this.mnuLostObjectsCreateTag_Click);
            // 
            // mnuLostObjectsCreateBranch
            // 
            this.mnuLostObjectsCreateBranch.Name = "mnuLostObjectsCreateBranch";
            this.mnuLostObjectsCreateBranch.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.mnuLostObjectsCreateBranch.Size = new System.Drawing.Size(192, 22);
            this.mnuLostObjectsCreateBranch.Text = "Create branch";
            this.mnuLostObjectsCreateBranch.Click += new System.EventHandler(this.mnuLostObjectsCreateBranch_Click);
            // 
            // btnCloseDialog
            // 
            this.btnCloseDialog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCloseDialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCloseDialog.Location = new System.Drawing.Point(648, 29);
            this.btnCloseDialog.Name = "btnCloseDialog";
            this.btnCloseDialog.Size = new System.Drawing.Size(208, 25);
            this.btnCloseDialog.TabIndex = 7;
            this.btnCloseDialog.Text = "Cancel";
            this.btnCloseDialog.UseVisualStyleBackColor = true;
            // 
            // btnRestoreSelectedObjects
            // 
            this.btnRestoreSelectedObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRestoreSelectedObjects.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRestoreSelectedObjects.Location = new System.Drawing.Point(299, 17);
            this.btnRestoreSelectedObjects.Name = "btnRestoreSelectedObjects";
            this.btnRestoreSelectedObjects.Size = new System.Drawing.Size(317, 25);
            this.btnRestoreSelectedObjects.TabIndex = 6;
            this.btnRestoreSelectedObjects.Text = "Restore selected objects";
            this.btnRestoreSelectedObjects.UseVisualStyleBackColor = true;
            this.btnRestoreSelectedObjects.Click += new System.EventHandler(this.btnRestoreSelectedObjects_Click);
            // 
            // DeleteAllLostAndFoundTags
            // 
            this.DeleteAllLostAndFoundTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DeleteAllLostAndFoundTags.Location = new System.Drawing.Point(4, 29);
            this.DeleteAllLostAndFoundTags.Name = "DeleteAllLostAndFoundTags";
            this.DeleteAllLostAndFoundTags.Size = new System.Drawing.Size(252, 25);
            this.DeleteAllLostAndFoundTags.TabIndex = 5;
            this.DeleteAllLostAndFoundTags.Text = "Delete all LOST_AND_FOUND tags";
            this.DeleteAllLostAndFoundTags.UseVisualStyleBackColor = true;
            this.DeleteAllLostAndFoundTags.Click += new System.EventHandler(this.DeleteAllLostAndFoundTagsClick);
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Remove.Location = new System.Drawing.Point(4, 2);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(252, 25);
            this.Remove.TabIndex = 1;
            this.Remove.Text = "Remove all dangling objects";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.RemoveClick);
            // 
            // SaveObjects
            // 
            this.SaveObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveObjects.Location = new System.Drawing.Point(648, 2);
            this.SaveObjects.Name = "SaveObjects";
            this.SaveObjects.Size = new System.Drawing.Size(208, 25);
            this.SaveObjects.TabIndex = 0;
            this.SaveObjects.Text = "Save objects to .git/lost-found";
            this.SaveObjects.UseVisualStyleBackColor = true;
            this.SaveObjects.Click += new System.EventHandler(this.SaveObjectsClick);
            // 
            // FormVerify
            // 
            this.AcceptButton = this.btnRestoreSelectedObjects;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCloseDialog;
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
        private System.Windows.Forms.Button DeleteAllLostAndFoundTags;
        private System.Windows.Forms.CheckBox FullCheck;
        private System.Windows.Forms.CheckBox NoReflogs;
        private System.Windows.Forms.Button btnRestoreSelectedObjects;
        private System.Windows.Forms.CheckBox ShowOnlyCommits;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView Warnings;
        private System.Windows.Forms.ContextMenuStrip mnuLostObjects;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectsCreateTag;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectView;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnIsLostObjectSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSubject;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnHash;
        private System.Windows.Forms.ToolStripMenuItem mnuLostObjectsCreateBranch;
        private System.Windows.Forms.Button btnCloseDialog;
    }
}