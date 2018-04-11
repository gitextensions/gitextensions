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
using ResourceManager;

namespace GitUI
{
    public partial class GravatarControl : GitExtensionsControl
    {
        private readonly IImageCache _avatarCache;
        private readonly IAvatarService _gravatarService;

        public GravatarControl()
        {
            InitializeComponent();
            Translate();

            noneToolStripMenuItem.Tag = DefaultImageType.None;
            identiconToolStripMenuItem.Tag = DefaultImageType.Identicon;
            monsterIdToolStripMenuItem.Tag = DefaultImageType.MonsterId;
            wavatarToolStripMenuItem.Tag = DefaultImageType.Wavatar;
            retroToolStripMenuItem.Tag = DefaultImageType.Retro;

            _avatarCache = new DirectoryImageCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);
            _gravatarService = new GravatarService(_avatarCache);
        }

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

            if (!AppSettings.ShowAuthorGravatar || string.IsNullOrEmpty(Email))
            {
                RefreshImage(Resources.User);
                return;
            }

            var image = await _gravatarService.GetAvatarAsync(Email, Math.Max(size.Width, size.Height), AppSettings.GravatarDefaultImageType);

            RefreshImage(image);
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(
                    async () =>
                    {
                        await _gravatarService.DeleteAvatarAsync(Email).ConfigureAwait(true);
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
            if (!(tag is DefaultImageType))
            {
                return;
            }

            AppSettings.GravatarDefaultImageType = ((DefaultImageType)tag).ToString();

            ThreadHelper.JoinableTaskFactory
                .RunAsync(
                    async () =>
                    {
                        await _avatarCache.ClearAsync().ConfigureAwait(true);
                        await UpdateGravatarAsync().ConfigureAwait(false);
                    })
                .FileAndForget();
        }

        private void noImageGeneratorToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var defaultImageType = _gravatarService.GetDefaultImageType(AppSettings.GravatarDefaultImageType);
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