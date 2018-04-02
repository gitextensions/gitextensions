using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Properties;
using Gravatar;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    public partial class GravatarControl : GitExtensionsControl
    {
        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();
        private readonly IImageCache _avatarCache;
        private readonly IAvatarService _avatarService;

        public GravatarControl()
        {
            InitializeComponent();
            Translate();

            noneToolStripMenuItem.Tag = DefaultImageType.None;
            identiconToolStripMenuItem.Tag = DefaultImageType.Identicon;
            monsterIdToolStripMenuItem.Tag = DefaultImageType.MonsterId;
            wavatarToolStripMenuItem.Tag = DefaultImageType.Wavatar;
            retroToolStripMenuItem.Tag = DefaultImageType.Retro;

            // We cache avatar images on disk...
            var persistentCache = new DirectoryImageCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);

            // And in memory...
            _avatarCache = new MruImageCache(persistentCache);

            _avatarService = new AvatarService(_avatarCache);
        }

        [CanBeNull]
        [Browsable(false)]
        public string Email { get; private set; }

        public void LoadImage(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                RefreshImage(Resources.User);
                return;
            }

            Email = email;
            ThreadHelper.JoinableTaskFactory.RunAsync(() => UpdateGravatarAsync()).FileAndForget();
        }

        private void RefreshImage(Image image)
        {
            _gravatarImg.Image = image ?? Resources.User;
            _gravatarImg.Refresh();
        }

        private async Task UpdateGravatarAsync()
        {
            await this.SwitchToMainThreadAsync();

            // resize our control (I'm not using AutoSize for a reason)
            var size = new Size(AppSettings.AuthorImageSize, AppSettings.AuthorImageSize);

            DpiUtil.Scale(ref size);

            Size = _gravatarImg.Size = size;

            var email = Email;

            if (!AppSettings.ShowAuthorGravatar || string.IsNullOrWhiteSpace(email))
            {
                RefreshImage(Resources.User);
                return;
            }

            var token = _cancellationTokenSequence.Next();
            var image = await _avatarService.GetAvatarAsync(email, Math.Max(size.Width, size.Height), AppSettings.GravatarDefaultImageType);

            if (!token.IsCancellationRequested)
            {
                RefreshImage(image);
            }
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            var email = Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory
                .RunAsync(
                    async () =>
                    {
                        await _avatarService.DeleteAvatarAsync(email).ConfigureAwait(true);
                        await UpdateGravatarAsync().ConfigureAwait(false);
                    })
                .FileAndForget();
        }

        private void RegisterAtGravatarcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"http://www.gravatar.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void ClearImagecacheToolStripMenuItemClick(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(
                    async () =>
                    {
                        await _avatarCache.ClearAsync().ConfigureAwait(true);
                        await UpdateGravatarAsync().ConfigureAwait(false);
                    })
                .FileAndForget();
        }

        private void noImageService_Click(object sender, EventArgs e)
        {
            var tag = (sender as ToolStripMenuItem)?.Tag;

            if (tag is DefaultImageType type)
            {
                AppSettings.GravatarDefaultImageType = type.ToString();

                ThreadHelper.JoinableTaskFactory
                    .RunAsync(
                        async () =>
                        {
                            await _avatarCache.ClearAsync().ConfigureAwait(true);
                            await UpdateGravatarAsync().ConfigureAwait(false);
                        })
                    .FileAndForget();
            }
        }

        private void noImageGeneratorToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var defaultImageType = _avatarService.GetDefaultImageType(AppSettings.GravatarDefaultImageType);
            ToolStripMenuItem selectedItem = null;
            foreach (ToolStripMenuItem menu in noImageGeneratorToolStripMenuItem.DropDownItems)
            {
                menu.Checked = false;
                if ((DefaultImageType)menu.Tag == defaultImageType)
                {
                    selectedItem = menu;
                }
            }

            if (selectedItem == null)
            {
                AppSettings.GravatarDefaultImageType = DefaultImageType.None.ToString();
                selectedItem = noneToolStripMenuItem;
            }

            selectedItem.Checked = true;
        }
    }
}