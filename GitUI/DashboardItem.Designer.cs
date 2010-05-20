namespace GitUI
{
    partial class DashboardItem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardItem));
            this.Icon = new System.Windows.Forms.PictureBox();
            this.Title = new System.Windows.Forms.LinkLabel();
            this.Description = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            this.SuspendLayout();
            // 
            // Icon
            // 
            resources.ApplyResources(this.Icon, "Icon");
            this.Icon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Icon.Name = "Icon";
            this.Icon.TabStop = false;
            this.Icon.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Icon.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // Title
            // 
            this.Title.AutoEllipsis = true;
            resources.ApplyResources(this.Title, "Title");
            this.Title.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Title.Name = "Title";
            this.Title.TabStop = true;
            this.Title.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Title.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // Description
            // 
            resources.ApplyResources(this.Description, "Description");
            this.Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Description.Name = "Description";
            this.Description.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Description.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // DashboardItem
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Description);
            this.Controls.Add(this.Title);
            this.Controls.Add(this.Icon);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "DashboardItem";
            this.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Leave += new System.EventHandler(this.DashboardItem_Leave);
            this.Resize += new System.EventHandler(this.DashboardItem_Resize);
            this.Enter += new System.EventHandler(this.DashboardItem_Enter);
            this.SizeChanged += new System.EventHandler(this.DashboardItem_SizeChanged);
            this.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Icon;
        private System.Windows.Forms.LinkLabel Title;
        private System.Windows.Forms.Label Description;
    }
}
