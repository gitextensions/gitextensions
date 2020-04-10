using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.Editor
{
    public sealed class ContinuousScrollEventManager
    {
        public EventHandler BottomScrollReached;
        public EventHandler TopScrollReached;

        private bool IsScrollDisabled
            => Control.ModifierKeys != Keys.Alt && !AppSettings.AutomaticContinuousScroll;
        private bool IsScrollTooFast
            => DateTime.Now - LastScrollEventFiredDate < TimeSpan.FromMilliseconds(AppSettings.AutomaticContinuousScrollDelay);
        private DateTime LastScrollEventFiredDate { get; set; } = DateTime.MinValue;

        public void RaiseBottomScrollReached(object sender, EventArgs e)
        {
            if (IsScrollDisabled || IsScrollTooFast)
            {
                return;
            }

            LastScrollEventFiredDate = DateTime.Now;
            BottomScrollReached?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseTopScrollReached(object sender, EventArgs e)
        {
            if (IsScrollDisabled || IsScrollTooFast)
            {
                return;
            }

            LastScrollEventFiredDate = DateTime.Now;
            TopScrollReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
