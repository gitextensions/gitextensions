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
            this._NO_TRANSLATE_smallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_mediumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_largeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.registerAtGravatarcomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this._NO_TRANSLATE_toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._gravatarImg)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _gravatarImg
            // 
            this._gravatarImg.ContextMenuStrip = this.contextMenuStrip;
            this._gravatarImg.Image = global::GitUI.Properties.Resources.User;
            this._gravatarImg.Location = new System.Drawing.Point(0, 0);
            this._gravatarImg.Name = "_gravatarImg";
            this._gravatarImg.Size = new System.Drawing.Size(80, 80);
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
            this.toolStripSeparator1,
            this.registerAtGravatarcomToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(232, 120);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.refreshToolStripMenuItem.Text = "Refresh image";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.RefreshToolStripMenuItemClick);
            // 
            // clearImagecacheToolStripMenuItem
            // 
            this.clearImagecacheToolStripMenuItem.Name = "clearImagecacheToolStripMenuItem";
            this.clearImagecacheToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.clearImagecacheToolStripMenuItem.Text = "Clear image cache";
            this.clearImagecacheToolStripMenuItem.Click += new System.EventHandler(this.ClearImagecacheToolStripMenuItemClick);
            // 
            // imageSizeToolStripMenuItem
            // 
            this.imageSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._NO_TRANSLATE_smallToolStripMenuItem,
            this._NO_TRANSLATE_toolStripMenuItem3,
            this._NO_TRANSLATE_mediumToolStripMenuItem,
            this._NO_TRANSLATE_toolStripMenuItem7,
            this._NO_TRANSLATE_toolStripMenuItem8,
            this._NO_TRANSLATE_largeToolStripMenuItem,
            this._NO_TRANSLATE_toolStripMenuItem4,
            this._NO_TRANSLATE_toolStripMenuItem5,
            this._NO_TRANSLATE_toolStripMenuItem2,
            this._NO_TRANSLATE_toolStripMenuItem6,
            this._NO_TRANSLATE_toolStripMenuItem9,
            this._NO_TRANSLATE_toolStripMenuItem10});
            this.imageSizeToolStripMenuItem.Name = "imageSizeToolStripMenuItem";
            this.imageSizeToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.imageSizeToolStripMenuItem.Text = "Image size";
            // 
            // smallToolStripMenuItem
            // 
            this._NO_TRANSLATE_smallToolStripMenuItem.Name = "smallToolStripMenuItem";
            this._NO_TRANSLATE_smallToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_smallToolStripMenuItem.Text = "80";
            this._NO_TRANSLATE_smallToolStripMenuItem.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem3
            // 
            this._NO_TRANSLATE_toolStripMenuItem3.Name = "toolStripMenuItem3";
            this._NO_TRANSLATE_toolStripMenuItem3.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem3.Text = "100";
            this._NO_TRANSLATE_toolStripMenuItem3.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // mediumToolStripMenuItem
            // 
            this._NO_TRANSLATE_mediumToolStripMenuItem.Name = "mediumToolStripMenuItem";
            this._NO_TRANSLATE_mediumToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_mediumToolStripMenuItem.Text = "120";
            this._NO_TRANSLATE_mediumToolStripMenuItem.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem7
            // 
            this._NO_TRANSLATE_toolStripMenuItem7.Name = "toolStripMenuItem7";
            this._NO_TRANSLATE_toolStripMenuItem7.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem7.Text = "140";
            this._NO_TRANSLATE_toolStripMenuItem7.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem8
            // 
            this._NO_TRANSLATE_toolStripMenuItem8.Name = "toolStripMenuItem8";
            this._NO_TRANSLATE_toolStripMenuItem8.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem8.Text = "160";
            this._NO_TRANSLATE_toolStripMenuItem8.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // largeToolStripMenuItem
            // 
            this._NO_TRANSLATE_largeToolStripMenuItem.Name = "largeToolStripMenuItem";
            this._NO_TRANSLATE_largeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_largeToolStripMenuItem.Text = "180";
            this._NO_TRANSLATE_largeToolStripMenuItem.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem4
            // 
            this._NO_TRANSLATE_toolStripMenuItem4.Name = "toolStripMenuItem4";
            this._NO_TRANSLATE_toolStripMenuItem4.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem4.Text = "200";
            this._NO_TRANSLATE_toolStripMenuItem4.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem5
            // 
            this._NO_TRANSLATE_toolStripMenuItem5.Name = "toolStripMenuItem5";
            this._NO_TRANSLATE_toolStripMenuItem5.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem5.Text = "220";
            this._NO_TRANSLATE_toolStripMenuItem5.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem2
            // 
            this._NO_TRANSLATE_toolStripMenuItem2.Name = "toolStripMenuItem2";
            this._NO_TRANSLATE_toolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem2.Text = "240";
            this._NO_TRANSLATE_toolStripMenuItem2.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem6
            // 
            this._NO_TRANSLATE_toolStripMenuItem6.Name = "toolStripMenuItem6";
            this._NO_TRANSLATE_toolStripMenuItem6.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem6.Text = "260";
            this._NO_TRANSLATE_toolStripMenuItem6.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(228, 6);
            // 
            // registerAtGravatarcomToolStripMenuItem
            // 
            this.registerAtGravatarcomToolStripMenuItem.Name = "registerAtGravatarcomToolStripMenuItem";
            this.registerAtGravatarcomToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.registerAtGravatarcomToolStripMenuItem.Text = "Register at gravatar.com";
            this.registerAtGravatarcomToolStripMenuItem.Click += new System.EventHandler(RegisterAtGravatarcomToolStripMenuItemClick);
            // 
            // toolStripMenuItem9
            // 
            this._NO_TRANSLATE_toolStripMenuItem9.Name = "toolStripMenuItem9";
            this._NO_TRANSLATE_toolStripMenuItem9.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem9.Text = "280";
            this._NO_TRANSLATE_toolStripMenuItem9.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // toolStripMenuItem10
            // 
            this._NO_TRANSLATE_toolStripMenuItem10.Name = "toolStripMenuItem10";
            this._NO_TRANSLATE_toolStripMenuItem10.Size = new System.Drawing.Size(152, 22);
            this._NO_TRANSLATE_toolStripMenuItem10.Text = "300";
            this._NO_TRANSLATE_toolStripMenuItem10.Click += new System.EventHandler(this.SmallToolStripMenuItemClick);
            // 
            // Gravatar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._gravatarImg);
            this.Name = "GravatarControl";
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
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_smallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_mediumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_largeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem _NO_TRANSLATE_toolStripMenuItem10;
    }
}
