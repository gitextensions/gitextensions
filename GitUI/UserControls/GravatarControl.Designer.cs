namespace GitUI
{
    partial class GravatarControl
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
            this._gravatarImg = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearImagecacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noImageGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.identiconToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monsterIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wavatarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.retroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.registerAtGravatarcomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._gravatarImg)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _gravatarImg
            // 
            this._gravatarImg.ContextMenuStrip = this.contextMenuStrip;
            this._gravatarImg.Image = global::GitUI.Properties.Resources.User;
            this._gravatarImg.Location = new System.Drawing.Point(0, 0);
            this._gravatarImg.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._gravatarImg.Name = "_gravatarImg";
            this._gravatarImg.Size = new System.Drawing.Size(93, 92);
            this._gravatarImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._gravatarImg.TabIndex = 0;
            this._gravatarImg.TabStop = false;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.clearImagecacheToolStripMenuItem,
            this.noImageGeneratorToolStripMenuItem,
            this.toolStripSeparator1,
            this.registerAtGravatarcomToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(203, 120);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.refreshToolStripMenuItem.Text = "Refresh image";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // clearImagecacheToolStripMenuItem
            // 
            this.clearImagecacheToolStripMenuItem.Image = global::GitUI.Properties.Resources.IconCleanupRepo;
            this.clearImagecacheToolStripMenuItem.Name = "clearImagecacheToolStripMenuItem";
            this.clearImagecacheToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.clearImagecacheToolStripMenuItem.Text = "Clear image cache";
            this.clearImagecacheToolStripMenuItem.Click += new System.EventHandler(this.ClearImagecacheToolStripMenuItemClick);
            // 
            // noImageGeneratorToolStripMenuItem
            // 
            this.noImageGeneratorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noneToolStripMenuItem,
            this.identiconToolStripMenuItem,
            this.monsterIdToolStripMenuItem,
            this.wavatarToolStripMenuItem,
            this.retroToolStripMenuItem});
            this.noImageGeneratorToolStripMenuItem.Name = "noImageGeneratorToolStripMenuItem";
            this.noImageGeneratorToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.noImageGeneratorToolStripMenuItem.Text = "No image service";
            this.noImageGeneratorToolStripMenuItem.DropDownOpening += new System.EventHandler(this.noImageGeneratorToolStripMenuItem_DropDownOpening);
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.CheckOnClick = true;
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.noneToolStripMenuItem.Text = "None";
            this.noneToolStripMenuItem.Click += new System.EventHandler(this.noImageService_Click);
            // 
            // identiconToolStripMenuItem
            // 
            this.identiconToolStripMenuItem.CheckOnClick = true;
            this.identiconToolStripMenuItem.Name = "identiconToolStripMenuItem";
            this.identiconToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.identiconToolStripMenuItem.Text = "Identicon";
            this.identiconToolStripMenuItem.Click += new System.EventHandler(this.noImageService_Click);
            // 
            // monsterIdToolStripMenuItem
            // 
            this.monsterIdToolStripMenuItem.CheckOnClick = true;
            this.monsterIdToolStripMenuItem.Name = "monsterIdToolStripMenuItem";
            this.monsterIdToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.monsterIdToolStripMenuItem.Text = "Monster Id";
            this.monsterIdToolStripMenuItem.Click += new System.EventHandler(this.noImageService_Click);
            // 
            // wavatarToolStripMenuItem
            // 
            this.wavatarToolStripMenuItem.CheckOnClick = true;
            this.wavatarToolStripMenuItem.Name = "wavatarToolStripMenuItem";
            this.wavatarToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.wavatarToolStripMenuItem.Text = "Wavatar";
            this.wavatarToolStripMenuItem.Click += new System.EventHandler(this.noImageService_Click);
            // 
            // retroToolStripMenuItem
            // 
            this.retroToolStripMenuItem.CheckOnClick = true;
            this.retroToolStripMenuItem.Name = "retroToolStripMenuItem";
            this.retroToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.retroToolStripMenuItem.Text = "Retro";
            this.retroToolStripMenuItem.Click += new System.EventHandler(this.noImageService_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(199, 6);
            // 
            // registerAtGravatarcomToolStripMenuItem
            // 
            this.registerAtGravatarcomToolStripMenuItem.Name = "registerAtGravatarcomToolStripMenuItem";
            this.registerAtGravatarcomToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.registerAtGravatarcomToolStripMenuItem.Text = "Register at gravatar.com";
            this.registerAtGravatarcomToolStripMenuItem.Click += new System.EventHandler(this.RegisterAtGravatarcomToolStripMenuItemClick);
            // 
            // GravatarControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this._gravatarImg);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "GravatarControl";
            this.Size = new System.Drawing.Size(175, 172);
            ((System.ComponentModel.ISupportInitialize)(this._gravatarImg)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _gravatarImg;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registerAtGravatarcomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearImagecacheToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem noImageGeneratorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem identiconToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monsterIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wavatarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retroToolStripMenuItem;
    }
}
