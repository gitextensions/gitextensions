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
            Label lblSpacer1;
            Label lblSpacer2;
            textBoxFind = new TextBox();
            treeView1 = new GitUI.UserControls.NativeTreeView();
            lblSpacer1 = new Label();
            lblSpacer2 = new Label();
            SuspendLayout();
            // 
            // lblSpacer1
            // 
            lblSpacer1.Dock = DockStyle.Top;
            lblSpacer1.Location = new Point(0, 0);
            lblSpacer1.Name = "lblSpacer1";
            lblSpacer1.Size = new Size(200, 8);
            lblSpacer1.TabIndex = 0;
            // 
            // lblSpacer2
            // 
            lblSpacer2.Dock = DockStyle.Top;
            lblSpacer2.Location = new Point(0, 39);
            lblSpacer2.Name = "lblSpacer2";
            lblSpacer2.Size = new Size(200, 8);
            lblSpacer2.TabIndex = 2;
            // 
            // textBoxFind
            // 
            textBoxFind.Dock = DockStyle.Top;
            textBoxFind.Location = new Point(0, 0);
            textBoxFind.Name = "textBoxFind";
            textBoxFind.Size = new Size(200, 31);
            textBoxFind.TabIndex = 1;
            textBoxFind.TextChanged += textBoxFind_TextChanged;
            textBoxFind.KeyUp += textBoxFind_KeyUp;
            // 
            // treeView1
            // 
            treeView1.Dock = DockStyle.Fill;
            treeView1.FullRowSelect = true;
            treeView1.HideSelection = false;
            treeView1.HotTracking = true;
            treeView1.Location = new Point(0, 31);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(200, 189);
            treeView1.TabIndex = 3;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // SettingsTreeViewUserControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(treeView1);
            Controls.Add(lblSpacer2);
            Controls.Add(textBoxFind);
            Controls.Add(lblSpacer1);
            MinimumSize = new Size(100, 220);
            Name = "SettingsTreeViewUserControl";
            Size = new Size(200, 220);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox textBoxFind;
        private UserControls.NativeTreeView treeView1;
    }
}
