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
    public sealed partial class AvatarControl : GitExtensionsControl
    {
        private readonly CancellationTokenSequence _cancellationTokenSequence = new CancellationTokenSequence();
        private readonly IAvatarProvider _avatarProvider = AvatarService.Default;

        public AvatarControl()
        {
            InitializeComponent();
            Translate();

            clearImagecacheToolStripMenuItem.Click += delegate { ClearCache(); };

            foreach (DefaultImageType type in Enum.GetValues(typeof(DefaultImageType)))
            {
                var item = new ToolStripMenuItem
                {
                    CheckOnClick = true,
                    Tag = type,
                    Text = type.ToString()
                };

                item.Click += delegate
                {
                    AppSettings.GravatarDefaultImageType = type;
                    ClearCache();
                };

                defaultImageToolStripMenuItem.DropDownItems.Add(item);
            }

            return;

            void ClearCache()
            {
                ThreadHelper.JoinableTaskFactory
                    .RunAsync(
                        async () =>
                        {
                            await _avatarProvider.ClearCacheAsync().ConfigureAwait(true);
                            await UpdateGravatarAsync().ConfigureAwait(false);
                        })
                    .FileAndForget();
            }
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
            var image = await _avatarProvider.GetAvatarAsync(email, Math.Max(size.Width, size.Height));

            if (!token.IsCancellationRequested)
            {
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
                        await UpdateGravatarAsync().ConfigureAwait(false);
                    })
                .FileAndForget();
        }

        private void OnRegisterGravatarClick(object sender, EventArgs e)
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

        private void OnDefaultImageDropDownOpening(object sender, EventArgs e)
        {
            var defaultImageType = AppSettings.GravatarDefaultImageType;

            ToolStripMenuItem selectedItem = null;
            ToolStripMenuItem noneItem = null;
            foreach (ToolStripMenuItem menu in defaultImageToolStripMenuItem.DropDownItems)
            {
                menu.Checked = false;

                var type = (DefaultImageType)menu.Tag;

                if (type == defaultImageType)
                {
                    selectedItem = menu;
                }

                if (type == DefaultImageType.None)
                {
                    noneItem = menu;
                }
            }

            Debug.Assert(noneItem != null && selectedItem != null, "noneItem != null && selectedItem != null");

            if (selectedItem == null)
            {
                AppSettings.GravatarDefaultImageType = DefaultImageType.None;
                selectedItem = noneItem;
            }

            selectedItem.Checked = true;
        }
    }
}