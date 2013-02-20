using System;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.Notifications
{
    public sealed partial class NotificationFeed
    {
        /// <summary>Single notification item in the <see cref="NotificationFeed"/>.</summary>
        sealed class NotificationItem : NotificationControl
        {
            public NotificationItem(Notification notification)
                : base(notification)
            {
                StartExpiration(TimeSpan.FromSeconds(15));
            }
        }
    }
}