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
            components = new System.ComponentModel.Container();
            _avatarImage = new PictureBox();
            contextMenuStrip = new ContextMenuStrip(components);
            clearImagecacheToolStripMenuItem = new ToolStripMenuItem();
            avatarProviderToolStripMenuItem = new ToolStripMenuItem();
            fallbackAvatarStyleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            registerGravatarToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(_avatarImage)).BeginInit();
            contextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // _avatarImage
            // 
            _avatarImage.ContextMenuStrip = contextMenuStrip;
            _avatarImage.Image = Properties.Images.User80;
            _avatarImage.Location = new Point(0, 0);
            _avatarImage.Margin = new Padding(0);
            _avatarImage.Name = "_avatarImage";
            _avatarImage.Size = new Size(96, 96);
            _avatarImage.SizeMode = PictureBoxSizeMode.Zoom;
            _avatarImage.TabIndex = 0;
            _avatarImage.TabStop = false;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] {
            clearImagecacheToolStripMenuItem,
            avatarProviderToolStripMenuItem,
            fallbackAvatarStyleToolStripMenuItem,
            toolStripSeparator1,
            registerGravatarToolStripMenuItem});
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(252, 184);
            // 
            // clearImagecacheToolStripMenuItem
            // 
            clearImagecacheToolStripMenuItem.Image = Properties.Images.CleanupRepo;
            clearImagecacheToolStripMenuItem.Name = "clearImagecacheToolStripMenuItem";
            clearImagecacheToolStripMenuItem.Size = new Size(251, 38);
            clearImagecacheToolStripMenuItem.Text = "Clear image cache";
            clearImagecacheToolStripMenuItem.Click += OnClearCacheClick;
            // 
            // avatarProviderToolStripMenuItem
            // 
            avatarProviderToolStripMenuItem.Name = "avatarProviderToolStripMenuItem";
            avatarProviderToolStripMenuItem.Size = new Size(251, 38);
            avatarProviderToolStripMenuItem.Text = "Avatar provider";
            avatarProviderToolStripMenuItem.DropDownOpening += avatarProviderToolStripMenuItem_DropDownOpening;
            // 
            // fallbackAvatarStyleToolStripMenuItem
            // 
            fallbackAvatarStyleToolStripMenuItem.Name = "fallbackAvatarStyleToolStripMenuItem";
            fallbackAvatarStyleToolStripMenuItem.Size = new Size(251, 38);
            fallbackAvatarStyleToolStripMenuItem.Text = "Fallback generated avatar style";
            fallbackAvatarStyleToolStripMenuItem.DropDownOpening += OnDefaultImageDropDownOpening;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(248, 6);
            // 
            // registerGravatarToolStripMenuItem
            // 
            registerGravatarToolStripMenuItem.Name = "registerGravatarToolStripMenuItem";
            registerGravatarToolStripMenuItem.Size = new Size(251, 38);
            registerGravatarToolStripMenuItem.Text = "Register at gravatar.com";
            registerGravatarToolStripMenuItem.Click += OnRegisterGravatarClick;
            // 
            // AvatarControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(_avatarImage);
            Margin = new Padding(0);
            Name = "AvatarControl";
            Size = new Size(96, 96);
            ((System.ComponentModel.ISupportInitialize)(_avatarImage)).EndInit();
            contextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private PictureBox _avatarImage;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem registerGravatarToolStripMenuItem;
        private ToolStripMenuItem clearImagecacheToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem fallbackAvatarStyleToolStripMenuItem;
        private ToolStripMenuItem avatarProviderToolStripMenuItem;
    }
}
