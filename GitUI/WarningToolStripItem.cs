using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        public static readonly ImageList images;
        static NotifierHelpers()
        {
            images = new ImageList();
            images.Images.Add(blank, Resources.BlankIcon);
            images.Images.Add(success, Resources.NotifySuccess);
            images.Images.Add(warn, Resources.NotifyWarn); // add when needed
            images.Images.Add(fail, Resources.NotifyError);
            images.Images.Add(info, Resources.NotifyInfo);
        }

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

        public static void SetImageList(this ToolStrip newParent)
        {
            if (newParent == null)
            {
                return;
            }

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
    }

    /// <summary><see cref="ToolStripButton"/> which holds notifications, with most recent items first.</summary>
    public sealed class NotificationFeed : ToolStripDropDownButton, INotifier
    {
        /// <summary>Initalizes a new control which holds notifications, with most recent items first.</summary>
        public NotificationFeed()
        {
            //TextAlign = ContentAlignment.MiddleRight;
            //Image = Resources.NotifyWarn;
            //TextImageRelation = TextImageRelation.ImageBeforeText;
            //Dock = DockStyle.Left;

            Image = Resources.Information;
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
            newParent.SetImageList();
        }

        /// <summary>Updates the text and image according to the current notifications, if any.</summary>
        void Update()
        {
            if (notifications.Any())
            {// 1 or more notifications -> [Image] [N]
                Text = notifications.Count.ToString();
                Image = Resources.NotifyInfo;
            }
            else
            {// (no notifications)
                Text = string.Empty;
                Image = Resources.Information;
            }
        }

        List<Notification> notifications = new List<Notification>();
        Dictionary<string, NotificationBatch> batches = new Dictionary<string, NotificationBatch>();

        /// <summary>Adds a notification to the status feed.</summary>
        void Add(Notification notification)
        {
            var control = new NotificationItem(notification);

            if (notification.BatchPart == Notification.BatchEntry.No)
            {// normal notification (NOT part of batch)
                control.Click += RemoveNotification;
                control.Expired += RemoveNotification;

                Add(control);
            }
            else
            {// (part of batch)
                if (notification.BatchPart == Notification.BatchEntry.First)
                {
                    NotificationBatch batch = new NotificationBatch(notification);
                    batches[notification.BatchId.ToString()] = batch;
                    Add(batch);
                }
                else
                {
                    batches[notification.BatchId.ToString()].Add(notification);
                }
            }
        }

        /// <summary>Adds the <see cref="NotificationControl"/> then updates the UI.</summary>
        void Add(NotificationControl control)
        {
            notifications.Insert(0, control.Notification);
            DropDownItems.Insert(0, control);
            Update();
        }

        /// <summary>Removes a notification from the status feed, then updates the UI.</summary>
        void Remove(NotificationControl control)
        {
            notifications.Remove(control.Notification);
            DropDownItems.Remove(control);
            control.Dispose();
            Update();
        }

        /// <summary>De-registers for notification control events, then removes it.</summary>
        void RemoveNotification(object sender, EventArgs e)
        {
            NotificationItem control = (NotificationItem)sender;
            control.Click -= RemoveNotification;
            control.Expired -= RemoveNotification;
            Remove(control);
        }
    }

    /// <summary>Base class for a notification control which expires.</summary>
    abstract class NotificationControl : ToolStripMenuItem
    {
        /// <summary>Gets the displayed notification.</summary>
        internal Notification Notification { get; private set; }
        /// <summary>syncronizes the notification's relevancy duration</summary>
        Timer timer;

        protected NotificationControl(Notification notification)
        {
            Notification = notification;
            Update(notification);
        }

        /// <summary>Starts the relevance timer which will remove the notification when it expires.</summary>
        protected void StartExpiration()
        {
            timer = new Timer { Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds };
            timer.Tick += OnExpiredPrivate;
            timer.Start();
        }

        /// <summary>Hides the notification.</summary>
        protected override void OnClick(EventArgs e)
        {
            Visible = false;
            base.OnClick(e);
        }

        /// <summary>Invoked when the notification's relevance expires.</summary>
        protected virtual void OnExpired(object sender, EventArgs e)
        {
            var handler = Expired;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        void OnExpiredPrivate(object sender, EventArgs e)
        {
            if (timer == null) { return; }

            timer.Stop();
            timer.Tick -= OnExpired;
            timer.Dispose();

            Visible = false;
            OnExpired(sender, e);
        }

        /// <summary>Occurs when a notification's relevance has expired.</summary>
        public event EventHandler Expired;

        /// <summary>Updates the text and image for UI.</summary>
        protected void Update(Notification notification = null)
        {
            notification = notification ?? Notification;
            Text = notification.Text;
            ImageKey = notification.GetImageKey();
        }

        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);
            newParent.SetImageList();
        }
    }

    /// <summary>List of notifications which are related to a specific batch operation.</summary>
    sealed class NotificationBatch : NotificationControl
    {
        /// <summary>Initializes a new batch notification control, adding the notification to the batch.</summary>
        public NotificationBatch(Notification notification)
            : base(notification)
        {
            Add(notification);
        }

        /// <summary>Adds a <see cref="Notification"/> to the batch.</summary>
        public void Add(Notification notification)
        {
            var control = new NotificationItem(notification);
            DropDownItems.Insert(0, control);

            Update(notification);

            if (notification.BatchPart == Notification.BatchEntry.Last)
            {
                StartExpiration();
            }

            // TODO: ?allow click removal for batch items?
            // if allowed, what happens when they remove all items before the last item has been received?
        }

        protected override void OnExpired(object sender, EventArgs e)
        {
            base.OnExpired(sender, e);
            foreach (NotificationItem item in DropDownItems)
            {
                item.Dispose();
            }
        }
    }

    /// <summary>Single notification item in the status feed.</summary>
    sealed class NotificationItem : NotificationControl
    {
        public NotificationItem(Notification notification)
            : base(notification)
        {
            if (notification.BatchPart == Notification.BatchEntry.No)
            {// NOT part of a batch start expiration
                StartExpiration();
            }
        }
    }
}
