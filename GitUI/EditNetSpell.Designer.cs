namespace GitUI
{
    partial class EditNetSpell
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditNetSpell));
            this.SpellCheckContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SpellCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.TextBox = new System.Windows.Forms.RichTextBox();
            this.EmptyLabel = new System.Windows.Forms.Label();
            this.SpellCheckContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // SpellCheckContextMenu
            // 
            this.SpellCheckContextMenu.AccessibleDescription = null;
            this.SpellCheckContextMenu.AccessibleName = null;
            resources.ApplyResources(this.SpellCheckContextMenu, "SpellCheckContextMenu");
            this.SpellCheckContextMenu.BackgroundImage = null;
            this.SpellCheckContextMenu.Font = null;
            this.SpellCheckContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1});
            this.SpellCheckContextMenu.Name = "SpellCheckContextMenu";
            this.SpellCheckContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.SpellCheckContextMenu_Opening);
            this.SpellCheckContextMenu.Click += new System.EventHandler(this.SpellCheckContextMenu_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.AccessibleDescription = null;
            this.toolStripSeparator1.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // SpellCheckTimer
            // 
            this.SpellCheckTimer.Interval = 250;
            this.SpellCheckTimer.Tick += new System.EventHandler(this.SpellCheckTimer_Tick);
            // 
            // TextBox
            // 
            this.TextBox.AcceptsTab = true;
            this.TextBox.AccessibleDescription = null;
            this.TextBox.AccessibleName = null;
            resources.ApplyResources(this.TextBox, "TextBox");
            this.TextBox.BackgroundImage = null;
            this.TextBox.ContextMenuStrip = this.SpellCheckContextMenu;
            this.TextBox.Font = null;
            this.TextBox.Name = "TextBox";
            this.TextBox.SizeChanged += new System.EventHandler(this.TextBox_SizeChanged);
            this.TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            this.TextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged_3);
            // 
            // EmptyLabel
            // 
            this.EmptyLabel.AccessibleDescription = null;
            this.EmptyLabel.AccessibleName = null;
            resources.ApplyResources(this.EmptyLabel, "EmptyLabel");
            this.EmptyLabel.BackColor = System.Drawing.SystemColors.Window;
            this.EmptyLabel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.EmptyLabel.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.EmptyLabel.Name = "EmptyLabel";
            this.EmptyLabel.Click += new System.EventHandler(this.EmptyLabel_Click);
            // 
            // EditNetSpell
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.EmptyLabel);
            this.Controls.Add(this.TextBox);
            this.Font = null;
            this.Name = "EditNetSpell";
            this.Load += new System.EventHandler(this.EditNetSpell_Load);
            this.SpellCheckContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip SpellCheckContextMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer SpellCheckTimer;
        private System.Windows.Forms.RichTextBox TextBox;
        private System.Windows.Forms.Label EmptyLabel;
    }
}
