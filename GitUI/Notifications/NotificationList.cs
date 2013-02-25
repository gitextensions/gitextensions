using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.Notifications
{
    /// <summary>List of <see cref="Notification"/>s.<remarks>
    /// Can be thought of as a <see cref="Stack{T}"/>.
    /// Newer elements are more important; therefore, they appear at the front of the list.
    /// Older elements are pushed to the bottom.
    /// </remarks></summary>
    public class NotificationList : ObservableCollection<Notification> { }
}
