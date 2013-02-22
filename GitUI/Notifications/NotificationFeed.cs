using System;
using System.Windows.Forms;
using GitUI.Properties;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.Notifications
{
    /// <summary><see cref="ToolStripButton"/> which holds notifications, with most recent items first.</summary>
    public sealed partial class NotificationFeed : ToolStripDropDownButton
    {
        /// <summary>Initalizes a new control which holds notifications, with most recent items first.</summary>
        public NotificationFeed(INotifications notifications)
            : this(notifications.Notifications) { }

        /// <summary>Initalizes a new control which holds notifications, with most recent items first.</summary>
        public NotificationFeed(IObservable<Notification> notifications)
        {
            //TextAlign = ContentAlignment.MiddleRight;
            //Image = Resources.NotifyWarn;
            //TextImageRelation = TextImageRelation.ImageBeforeText;
            //Dock = DockStyle.Left;

            Image = Resources.Information;
            subscription = notifications.Subscribe(Notify);
        }

        IDisposable subscription;

        /// <summary>Disposes of the <see cref="Notification"/> subscription.</summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && subscription != null)
            {
                subscription.Dispose();
            }
        }

        MostRecentNotification mostRecent;

        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="notification">Status update to show to user.</param>
        void Notify(Notification notification)
        {
            if (mostRecent != null)
            {
                mostRecent.Update(notification);
            }
            Add(notification);
        }

        /// <summary>Sets or adds to the parent's ImageList.</summary>
        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);
           
            if (mostRecent != null) { return; }

            mostRecent = new MostRecentNotification();
            newParent.Items.Insert(1, mostRecent);
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
            var control = new NotificationItem(notification);
            control.Click += RemoveNotification;
            control.Expired += RemoveNotification;

            Add(control);
        }

        /// <summary>Adds the <see cref="NotificationControl"/> then updates the UI.</summary>
        void Add(NotificationControl control)
        {
            nNotifications++;
            this.Insertion(control);
            Update();
        }

        /// <summary>De-registers for notification control events, then removes it.</summary>
        void RemoveNotification(object sender, EventArgs e)
        {
            nNotifications--;
            NotificationControl control = (NotificationControl)sender;
            control.Click -= RemoveNotification;
            control.Expired -= RemoveNotification;
            this.Removal(control);
            Update();
        }
    }
}