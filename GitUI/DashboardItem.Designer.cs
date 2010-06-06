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
            this.Icon = new System.Windows.Forms.PictureBox();
            this.Title = new System.Windows.Forms.LinkLabel();
            this.Description = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            this.SuspendLayout();
            // 
            // Icon
            // 
            this.Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Icon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Icon.Location = new System.Drawing.Point(0, 2);
            this.Icon.Name = "Icon";
            this.Icon.Size = new System.Drawing.Size(16, 16);
            this.Icon.TabIndex = 0;
            this.Icon.TabStop = false;
            this.Icon.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Icon.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // Title
            // 
            this.Title.AutoEllipsis = true;
            this.Title.AutoSize = true;
            this.Title.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Title.Location = new System.Drawing.Point(22, 2);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(35, 13);
            this.Title.TabIndex = 1;
            this.Title.TabStop = true;
            this.Title.Text = "label1";
            this.Title.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Title.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // Description
            // 
            this.Description.AutoSize = true;
            this.Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Description.Location = new System.Drawing.Point(25, 22);
            this.Description.Name = "Description";
            this.Description.Size = new System.Drawing.Size(24, 13);
            this.Description.TabIndex = 3;
            this.Description.Text = "text";
            this.Description.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Description.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // DashboardItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Description);
            this.Controls.Add(this.Title);
            this.Controls.Add(this.Icon);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "DashboardItem";
            this.Size = new System.Drawing.Size(723, 35);
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
