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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormVerify));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.ShowOnlyCommits = new System.Windows.Forms.CheckBox();
            this.NoReflogs = new System.Windows.Forms.CheckBox();
            this.FullCheck = new System.Windows.Forms.CheckBox();
            this.Unreachable = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Warnings = new System.Windows.Forms.ListBox();
            this.TagAllCommits = new System.Windows.Forms.Button();
            this.DeleteAllLostAndFoundTags = new System.Windows.Forms.Button();
            this.TagAllObjects = new System.Windows.Forms.Button();
            this.ViewObject = new System.Windows.Forms.Button();
            this.TagSelectedObject = new System.Windows.Forms.Button();
            this.Remove = new System.Windows.Forms.Button();
            this.SaveObjects = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
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
            this.splitContainer1.Panel2.Controls.Add(this.ViewObject);
            this.splitContainer1.Panel2.Controls.Add(this.TagSelectedObject);
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
            this.Warnings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Warnings.FormattingEnabled = true;
            this.Warnings.Location = new System.Drawing.Point(0, 0);
            this.Warnings.Name = "Warnings";
            this.Warnings.Size = new System.Drawing.Size(859, 316);
            this.Warnings.TabIndex = 0;
            this.Warnings.DoubleClick += new System.EventHandler(this.WarningsDoubleClick);
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
            // ViewObject
            // 
            this.ViewObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ViewObject.Location = new System.Drawing.Point(300, 29);
            this.ViewObject.Name = "ViewObject";
            this.ViewObject.Size = new System.Drawing.Size(173, 25);
            this.ViewObject.TabIndex = 3;
            this.ViewObject.Text = "View selected object";
            this.ViewObject.UseVisualStyleBackColor = true;
            this.ViewObject.Click += new System.EventHandler(this.ViewObjectClick);
            // 
            // TagSelectedObject
            // 
            this.TagSelectedObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TagSelectedObject.Location = new System.Drawing.Point(3, 3);
            this.TagSelectedObject.Name = "TagSelectedObject";
            this.TagSelectedObject.Size = new System.Drawing.Size(141, 25);
            this.TagSelectedObject.TabIndex = 2;
            this.TagSelectedObject.Text = "Tag selected object";
            this.TagSelectedObject.UseVisualStyleBackColor = true;
            this.TagSelectedObject.Click += new System.EventHandler(this.TagSelectedObjectClick);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(859, 524);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button SaveObjects;
        private System.Windows.Forms.ListBox Warnings;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button TagSelectedObject;
        private System.Windows.Forms.Button ViewObject;
        private System.Windows.Forms.CheckBox Unreachable;
        private System.Windows.Forms.Button TagAllObjects;
        private System.Windows.Forms.Button DeleteAllLostAndFoundTags;
        private System.Windows.Forms.CheckBox FullCheck;
        private System.Windows.Forms.CheckBox NoReflogs;
        private System.Windows.Forms.Button TagAllCommits;
        private System.Windows.Forms.CheckBox ShowOnlyCommits;
        private System.Windows.Forms.Label label2;
    }
}