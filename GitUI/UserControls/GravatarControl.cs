﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GitUI.Properties;
using Gravatar;
using Settings = GitCommands.Properties.Settings;

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

        [Browsable(false)]
        public string Email { get; private set; }

        [Browsable(false)]
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
            Size = new Size(Settings.Default.AuthorImageSize, Settings.Default.AuthorImageSize);
            _gravatarImg.Size = new Size(Settings.Default.AuthorImageSize, Settings.Default.AuthorImageSize);

            if (!Settings.Default.ShowAuthorGravatar || string.IsNullOrEmpty(Email))
            {
                RefreshImage(Resources.User);
                return;
            }

            FallBackService gravatarFallBack = FallBackService.Identicon;
            try
            {
                gravatarFallBack = (FallBackService)Enum.Parse(typeof(FallBackService), Settings.Default.GravatarFallbackService);
            }
            catch
            {
                Settings.Default.GravatarFallbackService = gravatarFallBack.ToString();
            }

            ThreadPool.QueueUserWorkItem(o =>
                                         GravatarService.LoadCachedImage(
                                             ImageFileName,
                                             Email,
                                             Resources.User,
                                             Settings.Default.AuthorImageCacheDays,
                                             Settings.Default.AuthorImageSize,
                                             Settings.Default.GravatarCachePath,
                                             RefreshImage,
                                             gravatarFallBack));
        }

        private void RefreshImage(Image image)
        {
            _syncContext.Post(state => { _gravatarImg.Image = image; _gravatarImg.Refresh(); }, null);
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            GravatarService.RemoveImageFromCache(ImageFileName);
            UpdateGravatar();
        }

        private void RegisterAtGravatarcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                GravatarService.OpenGravatarRegistration();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void ClearImagecacheToolStripMenuItemClick(object sender, EventArgs e)
        {
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }

        private void SmallToolStripMenuItemClick(object sender, EventArgs e)
        {
            var toolStripItem = (ToolStripItem)sender;

            Settings.Default.AuthorImageSize = int.Parse(toolStripItem.Text);
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }

        private void identiconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.GravatarFallbackService = FallBackService.Identicon.ToString();
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }

        private void monsterIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.GravatarFallbackService = FallBackService.MonsterId.ToString();
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }

        private void wavatarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.GravatarFallbackService = FallBackService.Wavatar.ToString();
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }

        private void retroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.GravatarFallbackService = FallBackService.Retro.ToString();
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.GravatarFallbackService = FallBackService.None.ToString();
            GravatarService.ClearImageCache();
            UpdateGravatar();
        }
    }
}