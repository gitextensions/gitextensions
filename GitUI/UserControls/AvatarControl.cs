using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.Avatars;
using GitUI.Properties;
using ResourceManager;

namespace GitUI
{
    public sealed partial class AvatarControl : GitExtensionsControl
    {
        private readonly CancellationTokenSequence _cancellationTokenSequence = new();
        private IAvatarProvider _avatarProvider = AvatarService.DefaultProvider;
        private IAvatarCacheCleaner _avatarCacheCleaner = AvatarService.CacheCleaner;

        public AvatarControl()
        {
            InitializeComponent();
            InitializeComplete();

            clearImagecacheToolStripMenuItem.Click += delegate { ClearCache(); };

            foreach (var avatarProvider in EnumHelper.GetValues<AvatarProvider>())
            {
                var item = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = avatarProvider,
                    Checked = avatarProvider == AppSettings.AvatarProvider,
                    Text = avatarProvider.GetDescription(),
                };

                item.Click += delegate
                {
                    AppSettings.AvatarProvider = avatarProvider;
                    ClearCache();
                };

                avatarProviderToolStripMenuItem.DropDownItems.Add(item);
            }

            foreach (var defaultImageType in EnumHelper.GetValues<AvatarFallbackType>())
            {
                var item = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = defaultImageType,
                    Text = defaultImageType.GetDescription(),
                };

                item.Click += delegate
                {
                    AppSettings.AvatarFallbackType = defaultImageType;
                    ClearCache();
                };

                fallbackAvatarStyleToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        public void ClearCache()
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(
                    async () =>
                    {
                        AvatarService.UpdateAvatarProvider();
                        await _avatarCacheCleaner.ClearCacheAsync().ConfigureAwait(true);
                        await UpdateAvatarAsync().ConfigureAwait(false);
                    })
                .FileAndForget();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSequence.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        [Browsable(false)]
        public string? Email { get; private set; }

        [Browsable(false)]
        public string? AuthorName { get; private set; }

        public void LoadImage(string? email, string? name)
        {
            if (string.IsNullOrEmpty(email))
            {
                RefreshImage(Images.User80);
                return;
            }

            Email = email;
            AuthorName = name;
            ThreadHelper.JoinableTaskFactory.RunAsync(() => UpdateAvatarAsync()).FileAndForget();
        }

        private void RefreshImage(Image? image)
        {
            _avatarImage.Image = image ?? Images.User80;
            _avatarImage.Refresh();
        }

        private async Task UpdateAvatarAsync()
        {
            await this.SwitchToMainThreadAsync();

            // resize our control (I'm not using AutoSize for a reason)
            var size = new Size(AppSettings.AuthorImageSizeInCommitInfo, AppSettings.AuthorImageSizeInCommitInfo);

            DpiUtil.Scale(ref size);

            Size = _avatarImage.Size = size;

            var email = Email;

            if (!AppSettings.ShowAuthorAvatarInCommitInfo || string.IsNullOrWhiteSpace(email))
            {
                RefreshImage(Images.User80);
                return;
            }

            var token = _cancellationTokenSequence.Next();
            var image = await _avatarProvider.GetAvatarAsync(email, AuthorName, Math.Max(size.Width, size.Height));

            if (!token.IsCancellationRequested)
            {
                await this.SwitchToMainThreadAsync();

                RefreshImage(image);
            }
        }

        private void OnClearCacheClick(object sender, EventArgs e)
        {
            var email = Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            ClearCache();
        }

        private void OnRegisterGravatarClick(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://www.gravatar.com");
        }

        private void OnDefaultImageDropDownOpening(object sender, EventArgs e)
        {
            UpdateMenuItemSelection(fallbackAvatarStyleToolStripMenuItem.DropDownItems, AppSettings.AvatarFallbackType);
        }

        private void avatarProviderToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            UpdateMenuItemSelection(avatarProviderToolStripMenuItem.DropDownItems, AppSettings.AvatarProvider);
        }

        private static void UpdateMenuItemSelection<T>(ToolStripItemCollection toolStripItems, T currentValue)
        {
            foreach (ToolStripMenuItem item in toolStripItems)
            {
                item.Checked = Equals((T)item.Tag, currentValue);
            }
        }
    }
}
