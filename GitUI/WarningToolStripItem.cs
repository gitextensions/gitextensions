using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
            if (newParent == null) { return; }

            if (newParent.ImageList == null)
            {
                newParent.ImageList = images;
            }
        }

        public static void Removal(this ToolStripDropDownItem dropDown, ToolStripItem item)
        {
            dropDown.Act(item, (items, child) =>
            {
                items.Remove(child);
                child.Dispose();
            });
        }

        public static void Insertion(this ToolStripDropDownItem dropDown, ToolStripItem item)
        {
            dropDown.Act(item, (items, child) => items.Insert(0, child));
        }

        static Dictionary<ToolStripDropDownItem, Queue<ScheduledAction>> scheduledActions
            = new Dictionary<ToolStripDropDownItem, Queue<ScheduledAction>>();

        class ScheduledAction
        {
            public Action<ToolStripItemCollection, ToolStripItem> action;
            public ToolStripItem item;
        }

        static void Act(this ToolStripDropDownItem dropDown, ToolStripItem item, Action<ToolStripItemCollection, ToolStripItem> action)
        {
            if (dropDown.DropDown.Visible)
            {// visible
                if (scheduledActions.ContainsKey(dropDown) == false)
                {
                    scheduledActions[dropDown] = new Queue<ScheduledAction>();
                }
                scheduledActions[dropDown].Enqueue(new ScheduledAction { action = action, item = item });
                dropDown.DropDownClosed += Schedule;
            }
            else
            {
                dropDown.DropDown.Enabled = false;
                action(dropDown.DropDownItems, item);
                dropDown.DropDown.Enabled = true;
            }
        }

        static void Schedule(object sender, EventArgs e)
        {
            ToolStripDropDownItem dropDown = (ToolStripDropDownItem)sender;
            Queue<ScheduledAction> actions = scheduledActions[dropDown];
            while (actions.Any())
            {
                ScheduledAction scheduledAction = actions.Dequeue();
                scheduledAction.action(dropDown.DropDownItems, scheduledAction.item);
            }
        }

        public static void Invoke(this Control control, Action action)
        {
            control.Invoke(action);
        }
    }

    /// <summary><see cref="ToolStripButton"/> which holds notifications, with most recent items first.</summary>
    public sealed class NotificationFeed : ToolStripDropDownButton
    {
        public IObserver<Notification> Observer { get; private set; }

        /// <summary>Initalizes a new control which holds notifications, with most recent items first.</summary>
        public NotificationFeed()
        {
            //TextAlign = ContentAlignment.MiddleRight;
            //Image = Resources.NotifyWarn;
            //TextImageRelation = TextImageRelation.ImageBeforeText;
            //Dock = DockStyle.Left;

            Image = Resources.Information;

            Observer = System.Reactive.Observer
                            .Create<Notification>(Notify)
                            .NotifyOn(Scheduler.CurrentThread);
        }

        MostRecentNotification mostRecent;

        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="notification">Status update to show to user.</param>
        public void Notify(Notification notification)
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
            newParent.SetImageList();

            if (mostRecent != null) { return; }

            StatusStrip statusStrip = newParent as StatusStrip;
            if (statusStrip == null) { return; }

            mostRecent = new MostRecentNotification();
            statusStrip.Items.Insert(1, mostRecent);
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
        Dictionary<string, NotificationBatch> batches = new Dictionary<string, NotificationBatch>();

        /// <summary>Adds a notification to the status feed.</summary>
        void Add(Notification notification)
        {
            var control = new NotificationItem(notification);
            control.Click += RemoveNotification;
            control.Expired += RemoveNotification;

            Add(control);
        }

        void Add(INotificationBatch batch)
        {

        }

        void Add(BatchNotification batchNotification)
        {
            // (part of batch)
            if (batchNotification.BatchPart == BatchNotification.BatchEntry.First)
            {
                // first -> add to status feed
                NotificationBatch batch = new NotificationBatch(notification);
                batches[notification.BatchId.ToString()] = batch;
                Add(batch);
            }
            else
            {
                // (NOT first)
                NotificationBatch batch = batches[notification.BatchId.ToString()];
                batch.Add(notification);
                if (notification.BatchPart == BatchNotification.BatchEntry.Last)
                {
                    // last
                    batch.Click += RemoveNotification;
                    batch.Expired += RemoveNotification;
                }
            }
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

    /// <summary>Base class for a notification control which expires.</summary>
    abstract class NotificationControl : ToolStripMenuItem
    {
        /// <summary>Gets the displayed notification.</summary>
        internal Notification Notification { get; private set; }
        public string Id { get; private set; }

        /// <summary>syncronizes the notification's relevancy duration</summary>
        Timer timer;

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
            if (timer == null) { return; }

            timer.Stop();
            timer.Tick -= OnExpired;

            //Visible = false;
            OnExpired(sender, e);
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

        /// <summary>Occurs when a notification's relevance has expired.</summary>
        public event EventHandler Expired;

        /// <summary>Updates the text and image for UI.</summary>
        public virtual void Update(Notification notification)
        {
            if (notification != null)
            {
                Notification = notification;
                Text = notification.Text;
                ImageKey = notification.GetImageKey();
            }
            else
            {
                Text = string.Empty;
                ImageKey = NotifierHelpers.blank;
            }

        }

        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);
            newParent.SetImageList();
        }

        public override string ToString() { return Notification.ToString(); }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (timer != null)
            {
                timer.Dispose();
            }
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
                StartExpiration(TimeSpan.FromSeconds(15));
            }

            // TODO: ?allow click removal for batch items?
            // if allowed, what happens when they remove all items before the last item has been received?
        }

        protected override void OnExpired(object sender, EventArgs e)
        {
            DropDownItems.Clear();
            base.OnExpired(sender, e);
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
                StartExpiration(TimeSpan.FromSeconds(15));
            }
        }
    }

    sealed class MostRecentNotification : NotificationControl
    {
        public override void Update(Notification notification)
        {
            base.Update(notification);
            Visible = true;
            StartExpiration(TimeSpan.FromSeconds(5));
        }

        void Empty()
        {
            Visible = false;
            base.Update(null);
        }

        protected override void OnExpired(object sender, EventArgs e)
        {
            base.OnExpired(sender, e);
            Empty();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Empty();
        }
    }
}
