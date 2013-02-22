using System;
using System.Windows.Forms;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.Notifications
{
    public sealed partial class NotificationFeed
    {
        /// <summary>Base class for a notification control which expires.</summary>
        abstract class NotificationControl : ToolStripMenuItem
        {
            /// <summary>Gets the displayed notification.</summary>
            internal Notification Notification { get; private set; }
            public string Id { get; private set; }

            /// <summary>syncronizes the notification's relevancy duration</summary>
            Timer timer;

            /// <summary>Creates a new notification control using the specified <paramref name="notification"/>.</summary>
            protected NotificationControl(Notification notification = null)
            {
                if (notification == null)
                {
                    return;
                }
                Id = Guid.NewGuid().ToString();
                Name = Id;
                Notification = notification;
                Update(notification);
            }

            /// <summary>Starts the relevance timer which will remove the notification when it expires.</summary>
            protected void StartExpiration(TimeSpan expiration)
            {
                timer = new Timer { Interval = (int)expiration.TotalMilliseconds };
                timer.Tick += OnExpiredPrivate;
                timer.Start();
            }

            void OnExpiredPrivate(object sender, EventArgs e)
            {
                if (timer == null)
                {
                    return;
                }

                timer.Stop();
                timer.Tick -= OnExpired;
                timer.Dispose();
                timer = null;

                //Visible = false;
                OnExpired(sender, e);
            }

            /// <summary>Raises the <see cref="Expired"/> event.</summary>
            protected virtual void OnExpired(object sender, EventArgs e)
            {
                var handler = Expired;
                if (handler != null)
                {
                    handler(this, null);
                }
            }

            /// <summary>Occurs when a notification's relevance has expired.</summary>
            public event EventHandler Expired;

            /// <summary>Updates the text and image for UI.</summary>
            public virtual void Update(Notification notification)
            {
                if (notification != null)
                {
                    Notification = notification;
                    Text = notification.Text;
                    Image = notification.GetImage();
                }
                else
                {
                    Text = string.Empty;
                    Image = NotifierHelpers.blank;
                }

            }

            public override string ToString()
            {
                return Notification.ToString();
            }

            /// <summary>Disposes the <see cref="timer"/>.</summary>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing && timer != null)
                {
                    timer.Dispose();
                }
            }
        }
    }
}