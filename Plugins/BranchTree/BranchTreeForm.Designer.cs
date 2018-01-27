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
            this.Branches = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // Branches
            // 
            this.Branches.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Branches.Location = new System.Drawing.Point(0, 0);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(368, 539);
            this.Branches.TabIndex = 0;
            // 
            // BranchTreeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 539);
            this.Controls.Add(this.Branches);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BranchTreeForm";
            this.Text = "BranchTreeForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView Branches;
    }
}