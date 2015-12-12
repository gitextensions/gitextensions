namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class DashboardItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.Icon = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_BranchName = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_Title = new System.Windows.Forms.LinkLabel();
            this._NO_TRANSLATE_Description = new System.Windows.Forms.Label();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel1);
            this.flowLayoutPanel2.Controls.Add(this._NO_TRANSLATE_Description);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.flowLayoutPanel2.Size = new System.Drawing.Size(177, 49);
            this.flowLayoutPanel2.TabIndex = 8;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.Icon);
            this.flowLayoutPanel1.Controls.Add(this._NO_TRANSLATE_Title);
            this.flowLayoutPanel1.Controls.Add(this._NO_TRANSLATE_BranchName);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 2);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(177, 22);
            this.flowLayoutPanel1.TabIndex = 7;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // Icon
            // 
            this.Icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Icon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Icon.Location = new System.Drawing.Point(3, 2);
            this.Icon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Icon.Name = "Icon";
            this.Icon.Size = new System.Drawing.Size(19, 18);
            this.Icon.TabIndex = 0;
            this.Icon.TabStop = false;
            this.Icon.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this.Icon.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // _NO_TRANSLATE_BranchName
            // 
            this._NO_TRANSLATE_BranchName.AutoSize = true;
            this._NO_TRANSLATE_BranchName.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_BranchName.Location = new System.Drawing.Point(102, 0);
            this._NO_TRANSLATE_BranchName.Name = "_NO_TRANSLATE_BranchName";
            this._NO_TRANSLATE_BranchName.Size = new System.Drawing.Size(72, 20);
            this._NO_TRANSLATE_BranchName.TabIndex = 4;
            this._NO_TRANSLATE_BranchName.Text = "##branch";
            this._NO_TRANSLATE_BranchName.Visible = false;
            this._NO_TRANSLATE_BranchName.Click += new System.EventHandler(this.Title_Click);
            this._NO_TRANSLATE_BranchName.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_BranchName.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // _NO_TRANSLATE_Title
            // 
            this._NO_TRANSLATE_Title.AutoEllipsis = true;
            this._NO_TRANSLATE_Title.AutoSize = true;
            this._NO_TRANSLATE_Title.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Title.Location = new System.Drawing.Point(28, 0);
            this._NO_TRANSLATE_Title.Name = "_NO_TRANSLATE_Title";
            this._NO_TRANSLATE_Title.Size = new System.Drawing.Size(68, 20);
            this._NO_TRANSLATE_Title.TabIndex = 1;
            this._NO_TRANSLATE_Title.TabStop = true;
            this._NO_TRANSLATE_Title.Text = "##label1";
            this._NO_TRANSLATE_Title.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_Title.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this._NO_TRANSLATE_Title.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.OnKeyDown);			
            // 
            // _NO_TRANSLATE_Description
            // 
            this._NO_TRANSLATE_Description.AutoSize = true;
            this._NO_TRANSLATE_Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Description.Location = new System.Drawing.Point(3, 24);
            this._NO_TRANSLATE_Description.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this._NO_TRANSLATE_Description.Name = "_NO_TRANSLATE_Description";
            this._NO_TRANSLATE_Description.Padding = new System.Windows.Forms.Padding(35, 0, 0, 0);
            this._NO_TRANSLATE_Description.Size = new System.Drawing.Size(87, 20);
            this._NO_TRANSLATE_Description.TabIndex = 3;
            this._NO_TRANSLATE_Description.Text = "##text";
            this._NO_TRANSLATE_Description.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this._NO_TRANSLATE_Description.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            // 
            // DashboardItem
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.flowLayoutPanel2);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DashboardItem";
            this.Size = new System.Drawing.Size(177, 49);
            this.SizeChanged += new System.EventHandler(this.DashboardItem_SizeChanged);
            this.MouseEnter += new System.EventHandler(this.DashboardItem_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.DashboardItem_MouseLeave);
            this.VisibleChanged += new System.EventHandler(DashboardItem_VisibleChanged);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Icon;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_Title;
        private System.Windows.Forms.Label _NO_TRANSLATE_Description;
        private System.Windows.Forms.Label _NO_TRANSLATE_BranchName;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}
