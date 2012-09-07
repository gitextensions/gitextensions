﻿namespace GitUI
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
            this._NO_TRANSLATE_Panel = new System.Windows.Forms.Panel();
            this._NO_TRANSLATE_BranchName = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Title = new System.Windows.Forms.LinkLabel();
            this.Icon = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_Description = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this._NO_TRANSLATE_Panel.AutoSize = true;
            this._NO_TRANSLATE_Panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._NO_TRANSLATE_Panel.Controls.Add(this._NO_TRANSLATE_BranchName);
            this._NO_TRANSLATE_Panel.Controls.Add(this._NO_TRANSLATE_Title);
            this._NO_TRANSLATE_Panel.Location = new System.Drawing.Point(24, 2);
            this._NO_TRANSLATE_Panel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._NO_TRANSLATE_Panel.MinimumSize = new System.Drawing.Size(214, 12);
            this._NO_TRANSLATE_Panel.Name = "panel1";
            this._NO_TRANSLATE_Panel.Size = new System.Drawing.Size(214, 12);
            this._NO_TRANSLATE_Panel.TabIndex = 5;
            this._NO_TRANSLATE_Panel.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_Panel.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // _NO_TRANSLATE_BranchName
            // 
            this._NO_TRANSLATE_BranchName.AutoSize = true;
            this._NO_TRANSLATE_BranchName.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_BranchName.Dock = System.Windows.Forms.DockStyle.Left;
            this._NO_TRANSLATE_BranchName.Location = new System.Drawing.Point(42, 0);
            this._NO_TRANSLATE_BranchName.Name = "_NO_TRANSLATE_BranchName";
            this._NO_TRANSLATE_BranchName.Size = new System.Drawing.Size(48, 12);
            this._NO_TRANSLATE_BranchName.TabIndex = 4;
            this._NO_TRANSLATE_BranchName.Text = "##branch";
            this._NO_TRANSLATE_BranchName.Click += new System.EventHandler(this.Title_Click);
            this._NO_TRANSLATE_BranchName.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_BranchName.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // _NO_TRANSLATE_Title
            // 
            this._NO_TRANSLATE_Title.AutoEllipsis = true;
            this._NO_TRANSLATE_Title.AutoSize = true;
            this._NO_TRANSLATE_Title.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Title.Dock = System.Windows.Forms.DockStyle.Left;
            this._NO_TRANSLATE_Title.Location = new System.Drawing.Point(0, 0);
            this._NO_TRANSLATE_Title.Name = "_NO_TRANSLATE_Title";
            this._NO_TRANSLATE_Title.Size = new System.Drawing.Size(42, 12);
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
            this.Icon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Icon.Name = "Icon";
            this.Icon.Size = new System.Drawing.Size(16, 14);
            this.Icon.TabIndex = 0;
            this.Icon.TabStop = false;
            this.Icon.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this.Icon.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // _NO_TRANSLATE_Description
            // 
            this._NO_TRANSLATE_Description.AutoSize = true;
            this._NO_TRANSLATE_Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Description.Location = new System.Drawing.Point(25, 20);
            this._NO_TRANSLATE_Description.Name = "_NO_TRANSLATE_Description";
            this._NO_TRANSLATE_Description.Size = new System.Drawing.Size(33, 12);
            this._NO_TRANSLATE_Description.TabIndex = 3;
            this._NO_TRANSLATE_Description.Text = "##text";
            this._NO_TRANSLATE_Description.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_Description.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // DashboardItem
            // 
            
            
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._NO_TRANSLATE_Panel);
            this.Controls.Add(this._NO_TRANSLATE_Description);
            this.Controls.Add(this.Icon);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DashboardItem";
            this.Size = new System.Drawing.Size(241, 32);
            this.SizeChanged += new System.EventHandler(this.DashboardItem_SizeChanged);
            this.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this._NO_TRANSLATE_Panel.ResumeLayout(false);
            this._NO_TRANSLATE_Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Icon;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_Title;
        private System.Windows.Forms.Label _NO_TRANSLATE_Description;
        private System.Windows.Forms.Label _NO_TRANSLATE_BranchName;
        private System.Windows.Forms.Panel _NO_TRANSLATE_Panel;
    }
}
