namespace GitUI
{
    partial class DashboardCategory
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
            this.Caption = new System.Windows.Forms.Label();
            this.RecentRepositoriesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.favouritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RecentRepositoriesContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // Caption
            // 
            this.Caption.AutoSize = true;
            this.Caption.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Caption.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.Caption.Location = new System.Drawing.Point(3, 2);
            this.Caption.Name = "Caption";
            this.Caption.Size = new System.Drawing.Size(84, 17);
            this.Caption.TabIndex = 8;
            this.Caption.Text = "Favourites";
            // 
            // RecentRepositoriesContextMenu
            // 
            this.RecentRepositoriesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.RecentRepositoriesContextMenu.Name = "RecentRepositoriesContextMenu";
            this.RecentRepositoriesContextMenu.Size = new System.Drawing.Size(153, 70);
            // 
            // addToToolStripMenuItem
            // 
            this.addToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.favouritesToolStripMenuItem});
            this.addToToolStripMenuItem.Name = "addToToolStripMenuItem";
            this.addToToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addToToolStripMenuItem.Text = "Add to";
            this.addToToolStripMenuItem.DropDownOpening += new System.EventHandler(this.addToToolStripMenuItem_DropDownOpening);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // favouritesToolStripMenuItem
            // 
            this.favouritesToolStripMenuItem.Name = "favouritesToolStripMenuItem";
            this.favouritesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.favouritesToolStripMenuItem.Text = "favourites";
            // 
            // DashboardCategory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Caption);
            this.Name = "DashboardCategory";
            this.Size = new System.Drawing.Size(716, 225);
            this.SizeChanged += new System.EventHandler(this.DashboardCategory_SizeChanged);
            this.RecentRepositoriesContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Caption;
        private System.Windows.Forms.ContextMenuStrip RecentRepositoriesContextMenu;
        private System.Windows.Forms.ToolStripMenuItem addToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem favouritesToolStripMenuItem;
    }
}
