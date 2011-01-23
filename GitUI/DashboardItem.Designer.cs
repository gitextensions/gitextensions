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
            this._NO_TRANSLATE_Title = new System.Windows.Forms.LinkLabel();
            this._NO_TRANSLATE_Description = new System.Windows.Forms.Label();
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
            this._NO_TRANSLATE_Title.AutoEllipsis = true;
            this._NO_TRANSLATE_Title.AutoSize = true;
            this._NO_TRANSLATE_Title.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Title.Location = new System.Drawing.Point(22, 2);
            this._NO_TRANSLATE_Title.Name = "Title";
            this._NO_TRANSLATE_Title.Size = new System.Drawing.Size(49, 13);
            this._NO_TRANSLATE_Title.TabIndex = 1;
            this._NO_TRANSLATE_Title.TabStop = true;
            this._NO_TRANSLATE_Title.Text = "##label1";
            this._NO_TRANSLATE_Title.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this._NO_TRANSLATE_Title.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // Description
            // 
            this._NO_TRANSLATE_Description.AutoSize = true;
            this._NO_TRANSLATE_Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Description.Location = new System.Drawing.Point(25, 22);
            this._NO_TRANSLATE_Description.Name = "Description";
            this._NO_TRANSLATE_Description.Size = new System.Drawing.Size(38, 13);
            this._NO_TRANSLATE_Description.TabIndex = 3;
            this._NO_TRANSLATE_Description.Text = "##text";
            this._NO_TRANSLATE_Description.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this._NO_TRANSLATE_Description.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            // 
            // DashboardItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._NO_TRANSLATE_Description);
            this.Controls.Add(this._NO_TRANSLATE_Title);
            this.Controls.Add(this.Icon);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "DashboardItem";
            this.Size = new System.Drawing.Size(723, 35);
            this.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.Leave += new System.EventHandler(DashboardItem_Leave);
            this.Resize += new System.EventHandler(DashboardItem_Resize);
            this.Enter += new System.EventHandler(DashboardItem_Enter);
            this.SizeChanged += new System.EventHandler(this.DashboardItem_SizeChanged);
            this.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Icon;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_Title;
        private System.Windows.Forms.Label _NO_TRANSLATE_Description;
    }
}
