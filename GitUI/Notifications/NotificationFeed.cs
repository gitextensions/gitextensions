using System;
using System.Drawing;
using System.Windows.Forms;
using GitUI.Properties;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.Notifications
{
    /// <summary><see cref="ToolStripButton"/> which holds notifications, with most recent items first.</summary>
    public sealed class NotificationFeed : ToolStripDropDownButton
    {
        /// <summary>Initalizes a new control which holds notifications, with most recent items first.</summary>
        public NotificationFeed(IGitUICommandsSource uiCommandsSource)
        {
            //TextAlign = ContentAlignment.MiddleRight;
            //TextImageRelation = TextImageRelation.ImageBeforeText;
            //Dock = DockStyle.Left;

            Image = Resources.Information;

            this.uiCommandsSource = uiCommandsSource;
            uiCommandsSource.GitUICommandsChanged += OnGitUICommandsChanged;
            Subscribe(uiCommandsSource);
        }

        /// <summary>Subscribes to a git UI commands source's notifications.</summary>
        private void OnGitUICommandsChanged(object sender, GitUICommandsChangedEventArgs e)
        {
            Subscribe(sender as IGitUICommandsSource);
        }

        /// <summary>Subscribes to a git UI commands source's notifications.</summary>
        void Subscribe(IGitUICommandsSource iGitUICommandsSource)
        {
            if (subscription != null)
            {
                subscription.Dispose();
            }
            if (iGitUICommandsSource.UICommands != null)
            {
                subscription = iGitUICommandsSource.UICommands.Notifications.Notifications.Subscribe(OnNewNotification);
            }
        }

        IDisposable subscription;
        IGitUICommandsSource uiCommandsSource;

        /// <summary>Disposes of the <see cref="Notification"/> subscription.</summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && subscription != null)
            {
                subscription.Dispose();
            }
            uiCommandsSource.GitUICommandsChanged -= OnGitUICommandsChanged;
        }

        NotificationPopup popup;

        /// <summary>Occurs when a new notification has been published.</summary>
        /// <param name="notification">The new notification message.</param>
        void OnNewNotification(Notification notification)
        {
            if (popup != null)
            {
                popup.New(notification, GetPopupLocation());
            }
            Add(notification);
        }

        Point GetPopupLocation()
        {
            throw new NotImplementedException();
        }

        /// <summary>Sets or adds to the parent's ImageList.</summary>
        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);

            if (popup != null) { return; }

            popup = new NotificationPopup();
        }

        /// <summary>Updates the text and image according to the current notifications, if any.</summary>
        void Update()
        {
            if (nNotifications >= 1)
            {// 1 or more notifications -> [Image] [N]
                Text = nNotifications.ToString();
                if (nNotifications >= 2) { return; }// only set values once

                Enabled = true;
                ShowDropDownArrow = true;
                Image = Resources.NotifyInfo;
            }
            else
            {// (no notifications)
                Enabled = false;
                ShowDropDownArrow = false;
                Text = string.Empty;
                Image = Resources.Information;
            }
        }

        int nNotifications;
     
        /// <summary>Adds a notification to the status feed.</summary>
        void Add(Notification notification)
        {
            nNotifications++;
            Update();
        }
    }
}
