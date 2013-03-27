using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.Notifications
{
    /// <summary>Displays the unread notifications.</summary>
    public partial class NotificationsForm : GitExtensionsForm
    {
        /// <summary>For Visual Studio designer.</summary>
        private NotificationsForm()
            : this(null) { }

        public NotificationsForm(NotificationList notificationList)
        {
            InitializeComponent();
            Translate();
        }
    }
}
