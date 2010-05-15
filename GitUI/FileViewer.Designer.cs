namespace GitUI
{
    partial class FileViewer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileViewer));
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.TextEditor = new ICSharpCode.TextEditor.TextEditorControl();
            this.ContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ignoreWhitespaceChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.increaseNumberOfLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.descreaseNumberOfLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEntireFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.treatAllFilesAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.ContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // PictureBox
            // 
            resources.ApplyResources(this.PictureBox, "PictureBox");
            this.PictureBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.PictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.PictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PictureBox.Location = new System.Drawing.Point(0, 0);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(649, 449);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox.TabIndex = 2;
            this.PictureBox.TabStop = false;
            this.PictureBox.Visible = false;
            // 
            // TextEditor
            // 
            this.TextEditor.ContextMenuStrip = this.ContextMenu;
            resources.ApplyResources(this.TextEditor, "TextEditor");
            this.TextEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextEditor.IsReadOnly = false;
            this.TextEditor.Location = new System.Drawing.Point(0, 0);
            this.TextEditor.Name = "TextEditor";
            this.TextEditor.Size = new System.Drawing.Size(649, 449);
            this.TextEditor.TabIndex = 3;
            this.TextEditor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextEditor_KeyUp);
            this.TextEditor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextEditor_KeyPress);
            this.TextEditor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextEditor_KeyDown);
            // 
            // ContextMenu
            // 
            resources.ApplyResources(this.ContextMenu, "ContextMenu");
            this.ContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.copyPatchToolStripMenuItem,
            this.toolStripSeparator1,
            this.ignoreWhitespaceChangesToolStripMenuItem,
            this.increaseNumberOfLinesToolStripMenuItem,
            this.descreaseNumberOfLinesToolStripMenuItem,
            this.showEntireFileToolStripMenuItem,
            this.toolStripSeparator2,
            this.treatAllFilesAsTextToolStripMenuItem});
            this.ContextMenu.Name = "ContextMenu";
            this.ContextMenu.Size = new System.Drawing.Size(275, 214);
            // 
            // findToolStripMenuItem
            // 
            resources.ApplyResources(this.findToolStripMenuItem, "findToolStripMenuItem");
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(271, 6);
            // 
            // ignoreWhitespaceChangesToolStripMenuItem
            // 
            resources.ApplyResources(this.ignoreWhitespaceChangesToolStripMenuItem, "ignoreWhitespaceChangesToolStripMenuItem");
            this.ignoreWhitespaceChangesToolStripMenuItem.Name = "ignoreWhitespaceChangesToolStripMenuItem";
            this.ignoreWhitespaceChangesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.ignoreWhitespaceChangesToolStripMenuItem.Click += new System.EventHandler(this.ignoreWhitespaceChangesToolStripMenuItem_Click);
            // 
            // increaseNumberOfLinesToolStripMenuItem
            // 
            resources.ApplyResources(this.increaseNumberOfLinesToolStripMenuItem, "increaseNumberOfLinesToolStripMenuItem");
            this.increaseNumberOfLinesToolStripMenuItem.Name = "increaseNumberOfLinesToolStripMenuItem";
            this.increaseNumberOfLinesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.increaseNumberOfLinesToolStripMenuItem.Click += new System.EventHandler(this.increaseNumberOfLinesToolStripMenuItem_Click);
            // 
            // descreaseNumberOfLinesToolStripMenuItem
            // 
            resources.ApplyResources(this.descreaseNumberOfLinesToolStripMenuItem, "descreaseNumberOfLinesToolStripMenuItem");
            this.descreaseNumberOfLinesToolStripMenuItem.Name = "descreaseNumberOfLinesToolStripMenuItem";
            this.descreaseNumberOfLinesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.descreaseNumberOfLinesToolStripMenuItem.Click += new System.EventHandler(this.descreaseNumberOfLinesToolStripMenuItem_Click);
            // 
            // showEntireFileToolStripMenuItem
            // 
            resources.ApplyResources(this.showEntireFileToolStripMenuItem, "showEntireFileToolStripMenuItem");
            this.showEntireFileToolStripMenuItem.Name = "showEntireFileToolStripMenuItem";
            this.showEntireFileToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.showEntireFileToolStripMenuItem.Click += new System.EventHandler(this.showEntireFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(271, 6);
            // 
            // treatAllFilesAsTextToolStripMenuItem
            // 
            resources.ApplyResources(this.treatAllFilesAsTextToolStripMenuItem, "treatAllFilesAsTextToolStripMenuItem");
            this.treatAllFilesAsTextToolStripMenuItem.Name = "treatAllFilesAsTextToolStripMenuItem";
            this.treatAllFilesAsTextToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.treatAllFilesAsTextToolStripMenuItem.Click += new System.EventHandler(this.treatAllFilesAsTextToolStripMenuItem_Click);
            // 
            // copyPatchToolStripMenuItem
            // 
            resources.ApplyResources(this.copyPatchToolStripMenuItem, "copyPatchToolStripMenuItem");
            this.copyPatchToolStripMenuItem.Name = "copyPatchToolStripMenuItem";
            this.copyPatchToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.copyPatchToolStripMenuItem.Click += new System.EventHandler(this.copyPatchToolStripMenuItem_Click);
            // 
            // FileViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TextEditor);
            this.Controls.Add(this.PictureBox);
            this.Name = "FileViewer";
            this.Size = new System.Drawing.Size(649, 449);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private ICSharpCode.TextEditor.TextEditorControl TextEditor;
        private new System.Windows.Forms.ContextMenuStrip ContextMenu;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ignoreWhitespaceChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem increaseNumberOfLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem descreaseNumberOfLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEntireFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem treatAllFilesAsTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyPatchToolStripMenuItem;
    }
}
