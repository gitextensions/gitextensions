namespace BranchTree
{
    partial class BranchTreeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BranchTreeForm));
            this.BranchesTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // BranchesTreeView
            // 
            this.BranchesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BranchesTreeView.Location = new System.Drawing.Point(0, 0);
            this.BranchesTreeView.Name = "BranchesTreeView";
            this.BranchesTreeView.Size = new System.Drawing.Size(368, 539);
            this.BranchesTreeView.TabIndex = 0;
            // 
            // BranchTreeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 539);
            this.Controls.Add(this.BranchesTreeView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BranchTreeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Branch tree";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView BranchesTreeView;
    }
}