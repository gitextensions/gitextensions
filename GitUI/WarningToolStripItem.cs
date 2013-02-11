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
        string blank = Guid.NewGuid().ToString();
        string success = Guid.NewGuid().ToString();
        string warn = Guid.NewGuid().ToString();
        string fail = Guid.NewGuid().ToString();
        string info = Guid.NewGuid().ToString();

        readonly ImageList images;

        /// <summary>3 seconds</summary>
        static readonly double TotalBlinkDuration = 3000;
        /// <summary>150ms</summary>
        static readonly int BlinkInterval = 150;
        readonly Timer _blinkTimer = new Timer { Interval = BlinkInterval };
        int _counter;

        public WarningToolStripItem()
        {
            Width = 200;
            Height = 20;

            Color offColor = Parent != null ? Parent.BackColor : SystemColors.Control;
            _blinkTimer.Tick += (o, e) => ToggleOnOff(() =>
            {
                BackColor = Color.Salmon;
            }, () =>
            {
                BackColor = offColor;
            });

            _blinkTimer.Start();
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
                Stop();
                if (finish != null)
                {
                    finish();
                }
                Debug.WriteLine(">done");
            }
        }

        public WarningToolStripItem(object nul)
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

        string currentImg;

        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="statusUpdate">Status update to show to user.</param>
        /// <param name="blink">true: blink; false: don't blink.</param>
        public void Notify(StatusFeedItem statusUpdate, bool blink = true)
        {
            Debug.WriteLine("<notify");
            Visible = true;

            if (currentStatus != null)
            {
                statusQueue.Enqueue(statusUpdate);
            }
            else
            {
                Start(statusUpdate);
            }
        }

        /// <summary>Toggles the <see cref="Image"/> on or off during a tick.</summary>
        void Blink(object o, EventArgs e)
        {
            ToggleOnOff(() =>
            {
                ImageKey = currentImg;
            }, () =>
            {
                ImageKey = blank;
            }, () =>
            {
                _blinkTimer.Tick -= Blink;
                if (statusQueue.Any())
                {
                    currentStatus = statusQueue.Dequeue();
                    Start(currentStatus);
                }
            });
        }

        /// <summary>Shows the status text and starts blinking the icon.</summary>
        void Start(StatusFeedItem statusUpdate)
        {
            currentStatus = statusUpdate;
            Debug.WriteLine("<start: {0}", statusUpdate);
            switch (statusUpdate.Severity)
            {// TODO: blink rate proportional to severity level
                case StatusSeverity.Info:
                    currentImg = info;
                    break;
                case StatusSeverity.Success:
                    currentImg = success;
                    break;
                case StatusSeverity.Warn:
                    currentImg = warn;
                    break;
                case StatusSeverity.Fail:
                    currentImg = fail;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            ImageKey = currentImg;
            Text = statusUpdate.Text;
            Visible = true;
           
            _counter = 0;
            _blinkTimer.Tick += Blink;
            _blinkTimer.Interval = 500;
            _blinkTimer.Start();
        }

        /// <summary>Stops the blinking.</summary>
        void Stop()
        {
            Debug.WriteLine(">stop");
            _blinkTimer.Stop();
            _blinkTimer.Tick -= Blink;
        }

        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            base.OnParentChanged(oldParent, newParent);
            if (newParent != null)
            {
                newParent.ImageList = images;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Visible = false;
        }

        StatusFeedItem currentStatus;
        Queue<StatusFeedItem> statusQueue = new Queue<StatusFeedItem>();
    }
}
