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
            this.imageSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extraLargeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.imageSizeToolStripMenuItem,
            this.noImageGeneratorToolStripMenuItem,
            this.toolStripSeparator1,
            this.registerAtGravatarcomToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(241, 152);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(240, 24);
            this.refreshToolStripMenuItem.Text = "Refresh image";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // clearImagecacheToolStripMenuItem
            // 
            this.clearImagecacheToolStripMenuItem.Name = "clearImagecacheToolStripMenuItem";
            this.clearImagecacheToolStripMenuItem.Size = new System.Drawing.Size(240, 24);
            this.clearImagecacheToolStripMenuItem.Text = "Clear image cache";
            this.clearImagecacheToolStripMenuItem.Click += new System.EventHandler(this.ClearImagecacheToolStripMenuItemClick);
            // 
            // imageSizeToolStripMenuItem
            // 
            this.imageSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smallToolStripMenuItem,
            this.normalToolStripMenuItem,
            this.largeToolStripMenuItem,
            this.extraLargeToolStripMenuItem});
            this.imageSizeToolStripMenuItem.Name = "imageSizeToolStripMenuItem";
            this.imageSizeToolStripMenuItem.Size = new System.Drawing.Size(240, 24);
            this.imageSizeToolStripMenuItem.Text = "Image size";
            // 
            // smallToolStripMenuItem
            // 
            this.smallToolStripMenuItem.Name = "smallToolStripMenuItem";
            this.smallToolStripMenuItem.Size = new System.Drawing.Size(221, 24);
            this.smallToolStripMenuItem.Tag = "80";
            this.smallToolStripMenuItem.Text = "Small (80x80)";
            this.smallToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItemClick);
            // 
            // normalToolStripMenuItem
            // 
            this.normalToolStripMenuItem.Name = "normalToolStripMenuItem";
            this.normalToolStripMenuItem.Size = new System.Drawing.Size(221, 24);
            this.normalToolStripMenuItem.Tag = "160";
            this.normalToolStripMenuItem.Text = "Normal (160x160)";
            this.normalToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItemClick);
            // 
            // largeToolStripMenuItem
            // 
            this.largeToolStripMenuItem.Name = "largeToolStripMenuItem";
            this.largeToolStripMenuItem.Size = new System.Drawing.Size(221, 24);
            this.largeToolStripMenuItem.Tag = "240";
            this.largeToolStripMenuItem.Text = "Large (240x240)";
            this.largeToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItemClick);
            // 
            // extraLargeToolStripMenuItem
            // 
            this.extraLargeToolStripMenuItem.Name = "extraLargeToolStripMenuItem";
            this.extraLargeToolStripMenuItem.Size = new System.Drawing.Size(221, 24);
            this.extraLargeToolStripMenuItem.Tag = "320";
            this.extraLargeToolStripMenuItem.Text = "Extra Large (320x320)";
            this.extraLargeToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItemClick);
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
            this.noImageGeneratorToolStripMenuItem.Size = new System.Drawing.Size(240, 24);
            this.noImageGeneratorToolStripMenuItem.Text = "No image service";
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.noneToolStripMenuItem.Text = "None";
            this.noneToolStripMenuItem.Click += new System.EventHandler(this.noneToolStripMenuItem_Click);
            // 
            // identiconToolStripMenuItem
            // 
            this.identiconToolStripMenuItem.Name = "identiconToolStripMenuItem";
            this.identiconToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.identiconToolStripMenuItem.Text = "Identicon";
            this.identiconToolStripMenuItem.Click += new System.EventHandler(this.identiconToolStripMenuItem_Click);
            // 
            // monsterIdToolStripMenuItem
            // 
            this.monsterIdToolStripMenuItem.Name = "monsterIdToolStripMenuItem";
            this.monsterIdToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.monsterIdToolStripMenuItem.Text = "Monster Id";
            this.monsterIdToolStripMenuItem.Click += new System.EventHandler(this.monsterIdToolStripMenuItem_Click);
            // 
            // wavatarToolStripMenuItem
            // 
            this.wavatarToolStripMenuItem.Name = "wavatarToolStripMenuItem";
            this.wavatarToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.wavatarToolStripMenuItem.Text = "Wavatar";
            this.wavatarToolStripMenuItem.Click += new System.EventHandler(this.wavatarToolStripMenuItem_Click);
            // 
            // retroToolStripMenuItem
            // 
            this.retroToolStripMenuItem.Name = "retroToolStripMenuItem";
            this.retroToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.retroToolStripMenuItem.Text = "Retro";
            this.retroToolStripMenuItem.Click += new System.EventHandler(this.retroToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(237, 6);
            // 
            // registerAtGravatarcomToolStripMenuItem
            // 
            this.registerAtGravatarcomToolStripMenuItem.Name = "registerAtGravatarcomToolStripMenuItem";
            this.registerAtGravatarcomToolStripMenuItem.Size = new System.Drawing.Size(240, 24);
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
        private System.Windows.Forms.ToolStripMenuItem imageSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smallToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem largeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extraLargeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noImageGeneratorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem identiconToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monsterIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wavatarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retroToolStripMenuItem;
    }
}
