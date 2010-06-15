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
            this._Title = new System.Windows.Forms.LinkLabel();
            this._Description = new System.Windows.Forms.Label();
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
            this._Title.AutoEllipsis = true;
            this._Title.AutoSize = true;
            this._Title.Cursor = System.Windows.Forms.Cursors.Hand;
            this._Title.Location = new System.Drawing.Point(22, 2);
            this._Title.Name = "Title";
            this._Title.Size = new System.Drawing.Size(49, 13);
            this._Title.TabIndex = 1;
            this._Title.TabStop = true;
            this._Title.Text = "##label1";
            this._Title.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this._Title.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // Description
            // 
            this._Description.AutoSize = true;
            this._Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this._Description.Location = new System.Drawing.Point(25, 22);
            this._Description.Name = "Description";
            this._Description.Size = new System.Drawing.Size(38, 13);
            this._Description.TabIndex = 3;
            this._Description.Text = "##text";
            this._Description.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this._Description.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // DashboardItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._Description);
            this.Controls.Add(this._Title);
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
        private System.Windows.Forms.LinkLabel _Title;
        private System.Windows.Forms.Label _Description;
    }
}
