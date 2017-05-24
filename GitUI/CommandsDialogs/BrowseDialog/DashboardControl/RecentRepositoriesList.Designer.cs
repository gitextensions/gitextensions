namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class RecentRepositoriesList
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
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cboRepoCategories = new System.Windows.Forms.ComboBox();
            this.lblRecentRepositories = new System.Windows.Forms.Label();
            this.btnConfigureRecent = new System.Windows.Forms.Button();
            this.flpnlBody = new System.Windows.Forms.FlowLayoutPanel();
            this.tsmiCategories = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryNone = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategoryAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAddToMostRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveFromMostRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveFromList = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlHeader.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(320, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(320, 6);
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.tableLayoutPanel1);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHeader.MinimumSize = new System.Drawing.Size(675, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(30, 0, 30, 17);
            this.pnlHeader.Size = new System.Drawing.Size(706, 110);
            this.pnlHeader.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 291F));
            this.tableLayoutPanel1.Controls.Add(this.cboRepoCategories, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblRecentRepositories, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnConfigureRecent, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(30, 45);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(646, 48);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // cboRepoCategories
            // 
            this.cboRepoCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cboRepoCategories.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(34)))), ((int)(((byte)(42)))));
            this.cboRepoCategories.DisplayMember = "Description";
            this.cboRepoCategories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRepoCategories.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboRepoCategories.FormattingEnabled = true;
            this.cboRepoCategories.Items.AddRange(new object[] {
            "All",
            ".NET Core projects",
            "Web project"});
            this.cboRepoCategories.Location = new System.Drawing.Point(493, 17);
            this.cboRepoCategories.Name = "cboRepoCategories";
            this.cboRepoCategories.Size = new System.Drawing.Size(150, 27);
            this.cboRepoCategories.TabIndex = 2;
            this.cboRepoCategories.Visible = false;
            this.cboRepoCategories.DropDown += new System.EventHandler(this.cboRepoCategories_DropDown);
            // 
            // lblRecentRepositories
            // 
            this.lblRecentRepositories.AutoSize = true;
            this.lblRecentRepositories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecentRepositories.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblRecentRepositories.ForeColor = System.Drawing.Color.DimGray;
            this.lblRecentRepositories.Location = new System.Drawing.Point(0, 0);
            this.lblRecentRepositories.Margin = new System.Windows.Forms.Padding(0);
            this.lblRecentRepositories.Name = "lblRecentRepositories";
            this.lblRecentRepositories.Size = new System.Drawing.Size(335, 48);
            this.lblRecentRepositories.TabIndex = 1;
            this.lblRecentRepositories.Text = "Recent Repositories";
            // 
            // btnConfigureRecent
            // 
            this.btnConfigureRecent.AutoSize = true;
            this.btnConfigureRecent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnConfigureRecent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConfigureRecent.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnConfigureRecent.FlatAppearance.BorderSize = 0;
            this.btnConfigureRecent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfigureRecent.Image = global::GitUI.Properties.Resources.SettingsBw;
            this.btnConfigureRecent.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnConfigureRecent.Location = new System.Drawing.Point(339, 5);
            this.btnConfigureRecent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConfigureRecent.Name = "btnConfigureRecent";
            this.btnConfigureRecent.Size = new System.Drawing.Size(12, 38);
            this.btnConfigureRecent.TabIndex = 2;
            this.btnConfigureRecent.UseVisualStyleBackColor = true;
            this.btnConfigureRecent.Click += new System.EventHandler(this.btnConfigureRecent_Click);
            this.btnConfigureRecent.MouseEnter += new System.EventHandler(this.btnConfigureRecent_MouseEnter);
            this.btnConfigureRecent.MouseLeave += new System.EventHandler(this.btnConfigureRecent_MouseLeave);
            // 
            // flpnlBody
            // 
            this.flpnlBody.AutoScroll = true;
            this.flpnlBody.AutoScrollMargin = new System.Drawing.Size(30, 30);
            this.flpnlBody.AutoScrollMinSize = new System.Drawing.Size(390, 0);
            this.flpnlBody.BackColor = System.Drawing.Color.SlateGray;
            this.flpnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlBody.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpnlBody.Location = new System.Drawing.Point(0, 110);
            this.flpnlBody.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flpnlBody.MinimumSize = new System.Drawing.Size(675, 0);
            this.flpnlBody.Name = "flpnlBody";
            this.flpnlBody.Padding = new System.Windows.Forms.Padding(45, 30, 45, 87);
            this.flpnlBody.Size = new System.Drawing.Size(706, 623);
            this.flpnlBody.TabIndex = 1;
            this.flpnlBody.WrapContents = false;
            this.flpnlBody.Layout += new System.Windows.Forms.LayoutEventHandler(this.flpnlBody_Layout);
            // 
            // tsmiCategories
            // 
            this.tsmiCategories.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCategoryNone,
            this.tsmiCategoryAdd});
            this.tsmiCategories.Name = "tsmiCategories";
            this.tsmiCategories.Size = new System.Drawing.Size(323, 30);
            this.tsmiCategories.Text = "Categories";
            this.tsmiCategories.DropDownOpening += new System.EventHandler(this.tsmiCategories_DropDownOpening);
            // 
            // tsmiCategoryNone
            // 
            this.tsmiCategoryNone.Name = "tsmiCategoryNone";
            this.tsmiCategoryNone.Size = new System.Drawing.Size(180, 30);
            this.tsmiCategoryNone.Text = "(none)";
            this.tsmiCategoryNone.Click += new System.EventHandler(this.tsmiCategory_Click);
            // 
            // tsmiCategoryAdd
            // 
            this.tsmiCategoryAdd.Image = global::GitUI.Properties.Resources.bullet_add;
            this.tsmiCategoryAdd.Name = "tsmiCategoryAdd";
            this.tsmiCategoryAdd.Size = new System.Drawing.Size(180, 30);
            this.tsmiCategoryAdd.Text = "Add new...";
            this.tsmiCategoryAdd.Click += new System.EventHandler(this.tsmiCategoryAdd_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddToMostRecent,
            this.tsmiRemoveFromMostRecent,
            this.tsmiRemoveFromList,
            this.toolStripMenuItem1,
            this.tsmiCategories,
            this.toolStripMenuItem2,
            this.tsmiOpenFolder});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(324, 166);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // tsmiAddToMostRecent
            // 
            this.tsmiAddToMostRecent.Image = global::GitUI.Properties.Resources.Star;
            this.tsmiAddToMostRecent.Name = "tsmiAddToMostRecent";
            this.tsmiAddToMostRecent.Size = new System.Drawing.Size(323, 30);
            this.tsmiAddToMostRecent.Text = "Add to most recent";
            this.tsmiAddToMostRecent.Click += new System.EventHandler(this.tsmiAddToMostRecent_Click);
            // 
            // tsmiRemoveFromMostRecent
            // 
            this.tsmiRemoveFromMostRecent.Image = global::GitUI.Properties.Resources.StarBw;
            this.tsmiRemoveFromMostRecent.Name = "tsmiRemoveFromMostRecent";
            this.tsmiRemoveFromMostRecent.Size = new System.Drawing.Size(323, 30);
            this.tsmiRemoveFromMostRecent.Text = "Remove from most recent";
            this.tsmiRemoveFromMostRecent.Click += new System.EventHandler(this.tsmiRemoveFromMostRecent_Click);
            // 
            // tsmiRemoveFromList
            // 
            this.tsmiRemoveFromList.Name = "tsmiRemoveFromList";
            this.tsmiRemoveFromList.Size = new System.Drawing.Size(323, 30);
            this.tsmiRemoveFromList.Text = "Remove project from the list";
            this.tsmiRemoveFromList.Click += new System.EventHandler(this.tsmiRemoveFromList_Click);
            // 
            // tsmiOpenFolder
            // 
            this.tsmiOpenFolder.Image = global::GitUI.Properties.Resources.IconBrowseFileExplorer;
            this.tsmiOpenFolder.Name = "tsmiOpenFolder";
            this.tsmiOpenFolder.Size = new System.Drawing.Size(323, 30);
            this.tsmiOpenFolder.Text = "Open containing folder";
            this.tsmiOpenFolder.Click += new System.EventHandler(this.tsmiOpenFolder_Click);
            // 
            // RecentRepositoriesList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoScrollMargin = new System.Drawing.Size(40, 200);
            this.AutoScrollMinSize = new System.Drawing.Size(450, 0);
            this.Controls.Add(this.flpnlBody);
            this.Controls.Add(this.pnlHeader);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "RecentRepositoriesList";
            this.Size = new System.Drawing.Size(706, 733);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblRecentRepositories;
        private System.Windows.Forms.Button btnConfigureRecent;
        private System.Windows.Forms.ComboBox cboRepoCategories;
        private System.Windows.Forms.FlowLayoutPanel flpnlBody;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem tsmiAddToMostRecent;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveFromMostRecent;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveFromList;
        private System.Windows.Forms.ToolStripMenuItem tsmiOpenFolder;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategoryAdd;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategories;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategoryNone;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
