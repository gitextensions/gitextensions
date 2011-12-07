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
            System.Windows.Forms.Panel panel1;
            this._NO_TRANSLATE_BranchName = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Title = new System.Windows.Forms.LinkLabel();
            this.Icon = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_Description = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(this._NO_TRANSLATE_BranchName);
            panel1.Controls.Add(this._NO_TRANSLATE_Title);
            panel1.Location = new System.Drawing.Point(28, 2);
            panel1.MinimumSize = new System.Drawing.Size(250, 15);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(250, 15);
            panel1.TabIndex = 5;
            // 
            // _NO_TRANSLATE_BranchName
            // 
            this._NO_TRANSLATE_BranchName.AutoSize = true;
            this._NO_TRANSLATE_BranchName.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_BranchName.Dock = System.Windows.Forms.DockStyle.Left;
            this._NO_TRANSLATE_BranchName.Location = new System.Drawing.Point(52, 0);
            this._NO_TRANSLATE_BranchName.Name = "_NO_TRANSLATE_BranchName";
            this._NO_TRANSLATE_BranchName.Size = new System.Drawing.Size(58, 15);
            this._NO_TRANSLATE_BranchName.TabIndex = 4;
            this._NO_TRANSLATE_BranchName.Text = "##branch";
            this._NO_TRANSLATE_BranchName.Click += new System.EventHandler(this.Title_Click);
            // 
            // _NO_TRANSLATE_Title
            // 
            this._NO_TRANSLATE_Title.AutoEllipsis = true;
            this._NO_TRANSLATE_Title.AutoSize = true;
            this._NO_TRANSLATE_Title.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Title.Dock = System.Windows.Forms.DockStyle.Left;
            this._NO_TRANSLATE_Title.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_Title.Name = "_NO_TRANSLATE_Title";
            this._NO_TRANSLATE_Title.Size = new System.Drawing.Size(52, 15);
            this._NO_TRANSLATE_Title.TabIndex = 1;
            this._NO_TRANSLATE_Title.TabStop = true;
            this._NO_TRANSLATE_Title.Text = "##label1";
            this._NO_TRANSLATE_Title.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_Title.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // Icon
            // 
            this.Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Icon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Icon.Location = new System.Drawing.Point(0, 2);
            this.Icon.Name = "Icon";
            this.Icon.Size = new System.Drawing.Size(19, 18);
            this.Icon.TabIndex = 0;
            this.Icon.TabStop = false;
            this.Icon.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this.Icon.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // _NO_TRANSLATE_Description
            // 
            this._NO_TRANSLATE_Description.AutoSize = true;
            this._NO_TRANSLATE_Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Description.Location = new System.Drawing.Point(29, 25);
            this._NO_TRANSLATE_Description.Name = "_NO_TRANSLATE_Description";
            this._NO_TRANSLATE_Description.Size = new System.Drawing.Size(40, 15);
            this._NO_TRANSLATE_Description.TabIndex = 3;
            this._NO_TRANSLATE_Description.Text = "##text";
            this._NO_TRANSLATE_Description.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_Description.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // DashboardItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(panel1);
            this.Controls.Add(this._NO_TRANSLATE_Description);
            this.Controls.Add(this.Icon);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "DashboardItem";
            this.Size = new System.Drawing.Size(281, 40);
            this.SizeChanged += new System.EventHandler(this.DashboardItem_SizeChanged);
            this.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Icon;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_Title;
        private System.Windows.Forms.Label _NO_TRANSLATE_Description;
        private System.Windows.Forms.Label _NO_TRANSLATE_BranchName;
    }
}
