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

    /// <summary><see cref="ToolStripButton"/> which is meant to notify a user.</summary>
    public class NotificationsToolStripButton : ToolStripDropDownButton, INotifier
    {
        /// <summary>blank icon</summary>
        static string blank = Guid.NewGuid().ToString();
        /// <summary>info icon</summary>
        static string info = Guid.NewGuid().ToString();
        /// <summary>success icon</summary>
        static string success = Guid.NewGuid().ToString();
        /// <summary>warning icon</summary>
        static string warn = Guid.NewGuid().ToString();
        /// <summary>failure icon</summary>
        static string fail = Guid.NewGuid().ToString();

        readonly ImageList images;

        /// <summary>3 seconds</summary>
        static readonly double TotalBlinkDuration = 3000;
        /// <summary>150ms</summary>
        static readonly int BlinkInterval = 150;
        readonly Timer _blinkTimer = new Timer { Interval = BlinkInterval };
        int nTicks;
        int nBlinks;

        Blinker currentBlinker;
        Queue<Notification> statusQueue = new Queue<Notification>();



        /// <summary>Toggles between an 'on' and 'off' action.</summary>
        void ToggleOnOff(Action on, Action off)
        {
            nTicks++; // increment

            if (nTicks % 2 == 1)
            {// even -> off
                off();
                Debug.WriteLine("off {0}", currentBlinker.Notification);
            }
            else
            {// odd -> on
                on();
                nBlinks++;
                Debug.WriteLine("on {0}", currentBlinker.Notification);
            }

            if (nBlinks >= currentBlinker.NumberOfBlinks)
            {// enough blinks -> stop blinking
                StopBlink();
            }
        }

        public NotificationsToolStripButton()
        {
            //TextAlign = ContentAlignment.MiddleRight;
            //Image = Resources.NotifyWarn;
            //TextImageRelation = TextImageRelation.ImageBeforeText;
            //Dock = DockStyle.Left;

            images = new ImageList();
            images.Images.Add(blank, Resources.BlankIcon);
            images.Images.Add(success, Resources.NotifySuccess);
            images.Images.Add(warn, Resources.NotifyWarn); // add when needed
            images.Images.Add(fail, Resources.NotifyError);
            images.Images.Add(info, Resources.NotifyInfo);
        }

        /// <summary>Toggles the <see cref="Image"/> on or off during a tick.</summary>
        void Blink(object o, EventArgs e)
        {
            ToggleOnOff(() =>
            {
                ImageKey = currentBlinker.ImageKey;
            }, () =>
            {
                ImageKey = blank;
            });
        }

        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="notification">Status update to show to user.</param>
        public void Notify(Notification notification)
        {
            Debug.WriteLine("<Notify {0}", notification);
            Visible = true;

            if (currentBlinker == null)
            {// no status is being shown
                Start(notification);

            }
            else if (currentBlinker != null && notification.Severity > currentBlinker.Notification.Severity)
            {// new status more severe than current -> start this one
                Close();
                Start(notification);
            }
            else
            {// (another status is currently blinking) -> queue
                Debug.WriteLine("<Queue {0}", notification);
                statusQueue.Enqueue(notification);
            }
        }

        /// <summary>Shows the status text and starts blinking the icon.</summary>
        void Start(Notification notification)
        {
            Debug.WriteLine("<StartBlink {0}", notification);

            currentBlinker = Blinker.Create(notification);

            // TODO: ?status log?

            ImageKey = currentBlinker.ImageKey;
            Text = notification.Text;
            Visible = true;

            nTicks = 0;
            if (currentBlinker.NumberOfBlinks == 0)
            {
                LeaveOn();
            }
            else
            {
                StartBlink();
            }
        }

        void StartBlink()
        {
            _blinkTimer.Tick += Blink;
            _blinkTimer.Interval = 500;
            _blinkTimer.Start();
        }

        /// <summary>Stops the blinking.</summary>
        void StopBlink()
        {
            Debug.WriteLine(">StopBlink {0}", currentBlinker != null ? currentBlinker.Notification : "initial" as object);
            nTicks = 0;
            nBlinks = 0;
            _blinkTimer.Stop();
            _blinkTimer.Tick -= Blink;

            StartNewOrLeaveOn();
        }

        /// <summary>Leaves the notification on for a certain amount of time.</summary>
        void LeaveOn()
        {
            if (currentBlinker.OnDuration == TimeSpan.Zero)
            {// leave on until user clicks (OR next notification)
                Debug.WriteLine(">LeaveOn -- {0}", currentBlinker.Notification);
                currentBlinker = null;
                return;
            }
            Debug.WriteLine(">LeaveOn 5s {0}", currentBlinker.Notification);
            _blinkTimer.Interval = (int)currentBlinker.OnDuration.TotalMilliseconds;
            _blinkTimer.Tick += Close;
            _blinkTimer.Start();
        }

        /// <summary>Leaves the status on or starts the next one.</summary>
        void StartNewOrLeaveOn()
        {
            if (statusQueue.Any())
            {// if any in queue -> start next
                Start(statusQueue.Dequeue());
            }
            else
            {// (none in queue) -> leave on
                LeaveOn();
            }
        }

        /// <summary>Close the current blinker.</summary>
        void Close()
        {
            Debug.WriteLine(">Close {0}", currentBlinker.Notification);
            _blinkTimer.Stop();
            _blinkTimer.Tick -= Close;
            Visible = false;
            Text = string.Empty;
            ImageKey = blank;
        }

        /// <summary>EventHandler to close the current blinker.</summary>
        void Close(object sender, EventArgs e)
        {
            Close();
        }

        class Blinker
        {
            public Notification Notification { get; private set; }
            public string ImageKey { get; private set; }
            public int NumberOfBlinks { get; private set; }
            public TimeSpan OnDuration { get; private set; }

            public Blinker(Notification status, string imageKey, TimeSpan onDuration, int nBlinks = 0)
            {
                Notification = status;
                ImageKey = imageKey;
                NumberOfBlinks = nBlinks;
                OnDuration = onDuration;
            }

            public static Blinker Create(Notification statusUpdate)
            {
                switch (statusUpdate.Severity)
                {// TODO: blink rate proportional to severity level
                    case StatusSeverity.Info:
                        // NO blinks, stay on for 5s
                        return new Blinker(statusUpdate, info, TimeSpan.FromSeconds(5));
                    case StatusSeverity.Success:
                        // NO blinks, stay on for 5s
                        return new Blinker(statusUpdate, success, TimeSpan.FromSeconds(5));
                    case StatusSeverity.Warn:
                        // blink once, stay on until clicked
                        return new Blinker(statusUpdate, warn, TimeSpan.Zero, 1);
                    case StatusSeverity.Fail:
                        // blink twice, stay on until clicked
                        return new Blinker(statusUpdate, fail, TimeSpan.Zero, 2);
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);
            if (newParent != null)
            {
                newParent.ImageList = images;
            }
        }

        void Add(Notification notification)
        {
            var control = new NotificationItem(notification);
            control.Click += OnNotificationClick;
        }

        List<Notification> notifications = new List<Notification>();

        void OnNotificationClick(object sender, EventArgs e)
        {
            NotificationItem control = (NotificationItem)sender;
            control.Click -= OnNotificationClick;
            Remove(control);
        }

        /// <summary>Removes a notification from the status feed.</summary>
        void Remove(NotificationItem control)
        {
            DropDownItems.Remove(control);
            notifications.Remove(control.Notification);
        }

        class NotificationSet
        {

        }
    }

    class NotificationSetToolStrip : ToolStripMenuItem
    {
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right)
            {

            }
        }

        void Test()
        {

        }
    }

    /// <summary>Single notification item in the status feed.</summary>
    sealed class NotificationItem : ToolStripMenuItem
    {
        internal Notification Notification { get; private set; }

        public NotificationItem(Notification notification)
        {
            throw new NotImplementedException("image");
            Text = notification.Text;
            Notification = notification;
        }



        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Visible = false;
        }

    }
}
