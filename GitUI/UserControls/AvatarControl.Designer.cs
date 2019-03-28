namespace GitUI
{
    partial class AvatarControl
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
            this.components = new System.ComponentModel.Container();
            this._avatarImage = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearImagecacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.avatarProviderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fallbackAvatarStyleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.registerGravatarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._avatarImage)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _avatarImage
            // 
            this._avatarImage.ContextMenuStrip = this.contextMenuStrip;
            this._avatarImage.Image = global::GitUI.Properties.Images.User80;
            this._avatarImage.Location = new System.Drawing.Point(0, 0);
            this._avatarImage.Margin = new System.Windows.Forms.Padding(0);
            this._avatarImage.Name = "_avatarImage";
            this._avatarImage.Size = new System.Drawing.Size(96, 96);
            this._avatarImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._avatarImage.TabIndex = 0;
            this._avatarImage.TabStop = false;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearImagecacheToolStripMenuItem,
            this.avatarProviderToolStripMenuItem,
            this.fallbackAvatarStyleToolStripMenuItem,
            this.toolStripSeparator1,
            this.registerGravatarToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(252, 184);
            // 
            // clearImagecacheToolStripMenuItem
            // 
            this.clearImagecacheToolStripMenuItem.Image = global::GitUI.Properties.Images.CleanupRepo;
            this.clearImagecacheToolStripMenuItem.Name = "clearImagecacheToolStripMenuItem";
            this.clearImagecacheToolStripMenuItem.Size = new System.Drawing.Size(251, 38);
            this.clearImagecacheToolStripMenuItem.Text = "Clear image cache";
            this.clearImagecacheToolStripMenuItem.Click += new System.EventHandler(this.OnClearCacheClick);
            // 
            // avatarProviderToolStripMenuItem
            // 
            this.avatarProviderToolStripMenuItem.CheckOnClick = true;
            this.avatarProviderToolStripMenuItem.Name = "avatarProviderToolStripMenuItem";
            this.avatarProviderToolStripMenuItem.Size = new System.Drawing.Size(251, 38);
            this.avatarProviderToolStripMenuItem.Text = "Avatar provider";
            this.avatarProviderToolStripMenuItem.DropDownOpening += new System.EventHandler(this.avatarProviderToolStripMenuItem_DropDownOpening);
            // 
            // fallbackAvatarStyleToolStripMenuItem
            // 
            this.fallbackAvatarStyleToolStripMenuItem.Name = "fallbackAvatarStyleToolStripMenuItem";
            this.fallbackAvatarStyleToolStripMenuItem.Size = new System.Drawing.Size(251, 38);
            this.fallbackAvatarStyleToolStripMenuItem.Text = "Fallback generated avatar style";
            this.fallbackAvatarStyleToolStripMenuItem.DropDownOpening += new System.EventHandler(this.OnDefaultImageDropDownOpening);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(248, 6);
            // 
            // registerGravatarToolStripMenuItem
            // 
            this.registerGravatarToolStripMenuItem.Name = "registerGravatarToolStripMenuItem";
            this.registerGravatarToolStripMenuItem.Size = new System.Drawing.Size(251, 38);
            this.registerGravatarToolStripMenuItem.Text = "Register at gravatar.com";
            this.registerGravatarToolStripMenuItem.Click += new System.EventHandler(this.OnRegisterGravatarClick);
            // 
            // AvatarControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this._avatarImage);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AvatarControl";
            this.Size = new System.Drawing.Size(96, 96);
            ((System.ComponentModel.ISupportInitialize)(this._avatarImage)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _avatarImage;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem registerGravatarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearImagecacheToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem fallbackAvatarStyleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem avatarProviderToolStripMenuItem;
    }
}
