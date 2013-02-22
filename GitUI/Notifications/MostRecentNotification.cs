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
            bool isMouseInside = false;
            bool hasExpired = false;

            /// <summary>Updates the text and image for UI, and start expiration timer.</summary>
            public override void Update(Notification notification)
            {
                base.Update(notification);
                Visible = true;
                hasExpired = false;
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
                hasExpired = true;
                if (isMouseInside) { return; }// don't change the control while user inside

                base.OnExpired(sender, e);
                Empty();
            }

            /// <summary>Raises the <see cref="E:System.Windows.Forms.ToolStripItem.Click"/> event.</summary>
            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                Empty();
            }

            /// <summary>Sets a flag indicating the user is inside the control.</summary>
            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                isMouseInside = true;
            }

            /// <summary>If the notification has expired, invokes <see cref="OnExpired"/>.</summary>
            protected override void OnMouseLeave(EventArgs e)
            {
                isMouseInside = false;
                if (hasExpired)
                {
                    OnExpired(null, null);
                }
                base.OnMouseLeave(e);
            }
        }
    }
}