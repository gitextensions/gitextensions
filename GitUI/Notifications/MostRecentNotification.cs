using System;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.Notifications
{
    public sealed partial class NotificationFeed
    {
        /// <summary>Provides a control for "most recent" notifications.
        /// <remarks>This control privately held within <see cref="NotificationFeed"/>.</remarks></summary>
        sealed class MostRecentNotification : NotificationControl
        {
            /// <summary>Updates the text and image for UI, and start expiration timer.</summary>
            public override void Update(Notification notification)
            {
                base.Update(notification);
                Visible = true;
                StartExpiration(TimeSpan.FromSeconds(5));
            }

            /// <summary>Hide the control and empty its contents (text and image).</summary>
            void Empty()
            {
                Visible = false;
                base.Update(null);
            }

            /// <summary>Raises the <see cref="NotificationControl.Expired"/> event.</summary>
            protected override void OnExpired(object sender, EventArgs e)
            {
                base.OnExpired(sender, e);
                Empty();
            }

            /// <summary>Raises the <see cref="E:System.Windows.Forms.ToolStripItem.Click"/> event.</summary>
            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                Empty();
            }
        }

    }
}