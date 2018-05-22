namespace GitUI.CommandsDialogs.SettingsDialog
{
    sealed partial class SettingsTreeViewUserControl
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
            System.Windows.Forms.Label lblSpacer1;
            System.Windows.Forms.Label lblSpacer2;
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.treeView1 = new GitUI.UserControls.NativeTreeView();
            lblSpacer1 = new System.Windows.Forms.Label();
            lblSpacer2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSpacer1
            // 
            lblSpacer1.Dock = System.Windows.Forms.DockStyle.Top;
            lblSpacer1.Location = new System.Drawing.Point(0, 0);
            lblSpacer1.Name = "lblSpacer1";
            lblSpacer1.Size = new System.Drawing.Size(200, 8);
            lblSpacer1.TabIndex = 0;
            // 
            // lblSpacer2
            // 
            lblSpacer2.Dock = System.Windows.Forms.DockStyle.Top;
            lblSpacer2.Location = new System.Drawing.Point(0, 39);
            lblSpacer2.Name = "lblSpacer2";
            lblSpacer2.Size = new System.Drawing.Size(200, 8);
            lblSpacer2.TabIndex = 2;
            // 
            // textBoxFind
            // 
            this.textBoxFind.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBoxFind.Location = new System.Drawing.Point(0, 0);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(200, 31);
            this.textBoxFind.TabIndex = 1;
            this.textBoxFind.TextChanged += new System.EventHandler(this.textBoxFind_TextChanged);
            this.textBoxFind.Enter += new System.EventHandler(this.textBoxFind_Enter);
            this.textBoxFind.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxFind_KeyUp);
            this.textBoxFind.Leave += new System.EventHandler(this.textBoxFind_Leave);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.FullRowSelect = true;
            this.treeView1.HideSelection = false;
            this.treeView1.HotTracking = true;
            this.treeView1.Location = new System.Drawing.Point(0, 31);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(200, 189);
            this.treeView1.TabIndex = 3;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // SettingsTreeViewUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.treeView1);
            this.Controls.Add(lblSpacer2);
            this.Controls.Add(this.textBoxFind);
            this.Controls.Add(lblSpacer1);
            this.MinimumSize = new System.Drawing.Size(100, 220);
            this.Name = "SettingsTreeViewUserControl";
            this.Size = new System.Drawing.Size(200, 220);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxFind;
        private UserControls.NativeTreeView treeView1;
    }
}
