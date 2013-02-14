using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GitUI.Properties;

namespace GitUI
{
    /// <summary><see cref="ToolStripButton"/> which blinks when</summary>
    public class WarningToolStripItem : ToolStripButton
    {
        /// <summary>3 seconds</summary>
        static readonly double TotalBlinkDuration = 3000;
        /// <summary>150ms</summary>
        static readonly int BlinkInterval = 150;
        readonly Timer _blinkTimer = new Timer { Interval = BlinkInterval };
        int _counter;
        Color _offColor;

        public WarningToolStripItem()
        {
            Width = 200;
            Height = 20;

            _offColor = Parent != null ? Parent.BackColor : SystemColors.Control;
            _blinkTimer.Tick += BlinkOnTick;

            _blinkTimer.Start();
        }

        void BlinkOnTick(object s, EventArgs e)
        {
            ToggleOnOff(() =>
             {
                 BackColor = Color.Salmon;
             }, () =>
             {
                 BackColor = _offColor;
             });
        }

        /// <summary>Toggles between an 'on' and 'off' action.</summary>
        void ToggleOnOff(Action on, Action off, Action finish = null)
        {
            if (_counter % 2 == 0)
            {// even -> off
                off();
                Debug.WriteLine("off");
            }
            else
            {// odd -> on
                on();
                Debug.WriteLine("on");
            }

            _counter++;// increment

            if (_counter >= 20)
            {// enough blinks -> stop blinking
                Debug.WriteLine(">stop");
                _blinkTimer.Stop();
                _blinkTimer.Tick -= BlinkOnTick;
                if (finish != null)
                {
                    finish();
                }
                Debug.WriteLine(">done");
            }
        }
    }

    /// <summary>Provides the ability to notify the user.</summary>
    public interface INotifier
    {
        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="notification">Status update to show to user.</param>
        void Notify(Notification notification);
    }

    /// <summary>Provides helpful members for notifications.</summary>
    internal static class NotifierHelpers
    {
        /// <summary>blank icon</summary>
        internal static string blank = Guid.NewGuid().ToString();
        /// <summary>info icon</summary>
        internal static string info = Guid.NewGuid().ToString();
        /// <summary>success icon</summary>
        internal static string success = Guid.NewGuid().ToString();
        /// <summary>warning icon</summary>
        internal static string warn = Guid.NewGuid().ToString();
        /// <summary>failure icon</summary>
        internal static string fail = Guid.NewGuid().ToString();

        /// <summary>Gets the image key for a <see cref="Notification"/>.</summary>
        public static string GetImageKey(this Notification notification)
        {
            switch (notification.Severity)
            {
                case StatusSeverity.Info: return info;
                case StatusSeverity.Success: return success;
                case StatusSeverity.Warn: return warn;
                case StatusSeverity.Fail: return fail;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>Resets a <see cref="Timer"/>.</summary>
        public static void Reset(this Timer timer)
        {
            timer.Stop();
            timer.Start();
        }
    }

    /// <summary><see cref="ToolStripButton"/> which holds notifications, with most recent items first.</summary>
    public class NotificationFeed : ToolStripDropDownButton, INotifier
    {
        readonly ImageList images;

        public NotificationFeed()
        {
            //TextAlign = ContentAlignment.MiddleRight;
            //Image = Resources.NotifyWarn;
            //TextImageRelation = TextImageRelation.ImageBeforeText;
            //Dock = DockStyle.Left;

            images = new ImageList();
            images.Images.Add(NotifierHelpers.blank, Resources.BlankIcon);
            images.Images.Add(NotifierHelpers.success, Resources.NotifySuccess);
            images.Images.Add(NotifierHelpers.warn, Resources.NotifyWarn); // add when needed
            images.Images.Add(NotifierHelpers.fail, Resources.NotifyError);
            images.Images.Add(NotifierHelpers.info, Resources.NotifyInfo);
        }

        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="notification">Status update to show to user.</param>
        public void Notify(Notification notification)
        {
            Add(notification);
        }

        /// <summary>Sets or adds to the parent's ImageList.</summary>
        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);
            if (newParent == null) { return; }

            if (newParent.ImageList == null)
            {
                newParent.ImageList = images;
            }
            else
            {
                foreach (Image image in images.Images)
                {
                    newParent.ImageList.Images.Add(image);
                }

            }
        }

        void Update()
        {
            Text = notifications.Count.ToString();
        }

        List<Notification> notifications = new List<Notification>();

        void Add(Notification notification)
        {
            var control = new NotificationItem(notification);
            control.Click += RemoveNotification;
            control.Expired += RemoveNotification;

            notifications.Insert(0, notification);
            DropDownItems.Insert(0, control);

            Update();
        }

        /// <summary>Removes a notification from the status feed.</summary>
        void Remove(NotificationItem control)
        {
            notifications.Remove(control.Notification);
            DropDownItems.Remove(control);
            Update();
        }

        void RemoveNotification(object sender, EventArgs e)
        {
            NotificationItem control = (NotificationItem)sender;
            control.Click -= RemoveNotification;
            control.Expired -= RemoveNotification;
            Remove(control);
        }

        class NotificationSet
        {

        }
    }

    internal abstract class NotificationControl : ToolStripMenuItem
    {
        /// <summary>Gets the displayed notification.</summary>
        internal Notification Notification { get; private set; }
        /// <summary>syncronizes the notification's relevancy duration</summary>
        protected Timer timer;

        protected NotificationControl(Notification notification)
        {
            Notification = notification;
            timer = new Timer { Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds };
            timer.Tick += OnExpired;
            timer.Start();
        }

        /// <summary>Hides the notification.</summary>
        protected override void OnClick(EventArgs e)
        {
            Visible = false;
            base.OnClick(e);
        }

        /// <summary>Invoked when the notification's relevance expires.</summary>
        void OnExpired(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Tick -= OnExpired;
            timer.Dispose();

            Visible = false;
            var handler = Expired;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>Occurs when a notification's relevance has expired.</summary>
        public event EventHandler Expired;

        protected void Update(Notification notification)
        {
            Text = notification.Text;
            ImageKey = Notification.GetImageKey();
        }
    }

    class NotificationBatch : NotificationControl
    {
        public NotificationBatch(Notification notification)
            : base(notification)
        {
            timer.Stop();// don't start timer until Last batch entry
        }

        public void Add(Notification notification)
        {
            throw new NotImplementedException("remove click stuff");
            
            DropDownItems.Insert(0, new NotificationItem(notification));
            Update(notification); 

            if (notification.BatchPart == Notification.BatchEntry.Last)
            {
                timer.Start();
            }
        }
    }

    /// <summary>Single notification item in the status feed.</summary>
    sealed class NotificationItem : NotificationControl
    {
        public NotificationItem(Notification notification)
            : base(notification) { }
    }
}
