using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUI.Avatars;
using GitUI.Properties;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    public sealed partial class AvatarControl : GitExtensionsControl
    {
        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();
        private readonly IAvatarProvider _avatarProvider = AvatarService.Default;

        public AvatarControl()
        {
            InitializeComponent();
            InitializeComplete();

            clearImagecacheToolStripMenuItem.Click += delegate { ClearCache(); };
            UpdateGravatarOptionDisplayState();

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
                    UpdateGravatarOptionDisplayState();
                    ClearCache();
                };

                avatarProviderToolStripMenuItem.DropDownItems.Add(item);
            }

            foreach (var defaultImageType in EnumHelper.GetValues<GravatarFallbackAvatarType>())
            {
                var item = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = defaultImageType,
                    Text = defaultImageType.GetDescription(),
                };

                item.Click += delegate
                {
                    AppSettings.GravatarFallbackAvatarType = defaultImageType;
                    ClearCache();
                };

                fallbackAvatarStyleToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void UpdateGravatarOptionDisplayState()
        {
            var isGravatarOptionsVisible = AppSettings.AvatarProvider == AvatarProvider.Gravatar;
            fallbackAvatarStyleToolStripMenuItem.Visible = isGravatarOptionsVisible;
            registerGravatarToolStripMenuItem.Visible = isGravatarOptionsVisible;
            toolStripSeparator1.Visible = isGravatarOptionsVisible;
        }

        public void ClearCache()
        {
            ThreadHelper.JoinableTaskFactory
                .RunAsync(
                    async () =>
                    {
                        await _avatarProvider.ClearCacheAsync().ConfigureAwait(true);
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
            if (disposing && (components != null))
            {
                _cancellationTokenSequence.Dispose();
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        [CanBeNull]
        [Browsable(false)]
        public string Email { get; private set; }

        [CanBeNull]
        [Browsable(false)]
        public string AuthorName { get; private set; }

        public void LoadImage(string email, string name)
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

        private void RefreshImage(Image image)
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

            ThreadHelper.JoinableTaskFactory
                .RunAsync(
                    async () =>
                    {
                        await _avatarProvider.ClearCacheAsync().ConfigureAwait(true);
                        await UpdateAvatarAsync().ConfigureAwait(false);
                    })
                .FileAndForget();
        }

        private void OnRegisterGravatarClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://www.gravatar.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void OnDefaultImageDropDownOpening(object sender, EventArgs e)
        {
            var defaultImageType = AppSettings.GravatarFallbackAvatarType;

            ToolStripMenuItem selectedItem = null;
            ToolStripMenuItem noneItem = null;
            foreach (ToolStripMenuItem menu in fallbackAvatarStyleToolStripMenuItem.DropDownItems)
            {
                menu.Checked = false;

                var type = (GravatarFallbackAvatarType)menu.Tag;

                if (type == defaultImageType)
                {
                    selectedItem = menu;
                }

                if (type == GravatarFallbackAvatarType.None)
                {
                    noneItem = menu;
                }
            }

            Debug.Assert(noneItem != null && selectedItem != null, "noneItem != null && selectedItem != null");

            if (selectedItem == null)
            {
                AppSettings.GravatarFallbackAvatarType = GravatarFallbackAvatarType.None;
                selectedItem = noneItem;
            }

            selectedItem.Checked = true;
        }

        private void avatarProviderToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var avatarProvider = AppSettings.AvatarProvider;

            ToolStripMenuItem selectedItem = null;
            ToolStripMenuItem defaultItem = null;
            foreach (ToolStripMenuItem menu in avatarProviderToolStripMenuItem.DropDownItems)
            {
                menu.Checked = false;

                var type = (AvatarProvider)menu.Tag;

                if (type == avatarProvider)
                {
                    selectedItem = menu;
                }

                if (type == AvatarProvider.Gravatar)
                {
                    defaultItem = menu;
                }
            }

            Debug.Assert(defaultItem != null && selectedItem != null, "noneItem != null && selectedItem != null");

            if (selectedItem == null)
            {
                AppSettings.AvatarProvider = AvatarProvider.Gravatar;
                selectedItem = defaultItem;
            }

            selectedItem.Checked = true;
        }
    }
}