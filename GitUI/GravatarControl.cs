using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GitUI.Properties;
using Gravatar;
using Settings = GitCommands.Settings;

namespace GitUI
{
    public partial class GravatarControl : GitExtensionsControl
    {
        private readonly SynchronizationContext _syncContext;

        public GravatarControl()
        {
            _syncContext = SynchronizationContext.Current;

            InitializeComponent();
            Translate();

            _gravatarImg.Visible = false;
        }

        public string Email { get; private set; }

        public string ImageFileName { get; private set; }

        public void LoadImageForEmail(string email)
        {
            if (!string.IsNullOrEmpty(email))
                _gravatarImg.Visible = true;
            Email = email;
            ImageFileName = string.Concat(Email, ".png");
            UpdateGravatar();
        }

        /// <summary>
        ///   Update the Gravatar anytime an attribute is changed
        /// </summary>
        private void UpdateGravatar()
        {
            // resize our control (I'm not using AutoSize for a reason)
            Size = new Size(Settings.AuthorImageSize, Settings.AuthorImageSize);
            _gravatarImg.Size = new Size(Settings.AuthorImageSize, Settings.AuthorImageSize);

            if (!Settings.ShowAuthorGravatar || string.IsNullOrEmpty(Email))
            {
                RefreshImage(Resources.User);
                return;
            }

            ThreadPool.QueueUserWorkItem(o =>
                                         GravatarService.LoadCachedImage(
                                             ImageFileName,
                                             Email,
                                             Resources.User,
                                             Settings.AuthorImageCacheDays,
                                             Settings.AuthorImageSize,
                                             Settings.ApplicationDataPath + "Images\\",
                                             RefreshImage));
        }

        private void RefreshImage(Image image)
        {
            _syncContext.Post(state => { _gravatarImg.Image = image; }, null);
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            GravatarService.RemoveImageFromCache(ImageFileName);
            UpdateGravatar();
        }

        private static void RegisterAtGravatarcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                GravatarService.OpenGravatarRegistration();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearImagecacheToolStripMenuItemClick(object sender, EventArgs e)
        {
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }

        private void SmallToolStripMenuItemClick(object sender, EventArgs e)
        {
            var toolStripItem = (ToolStripItem) sender;

            Settings.AuthorImageSize = int.Parse(toolStripItem.Text);
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }
    }
}