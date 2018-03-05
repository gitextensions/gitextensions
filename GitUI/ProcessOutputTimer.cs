using System;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public sealed class ProcessOutputTimer : IDisposable
    {
        public delegate void DoOutputCallback(string text);
        private readonly Timer _timer;
        private readonly StringBuilder textToAdd = new StringBuilder();
        private DoOutputCallback doOutput;

        /// <summary>
        ///
        /// </summary>
        /// <param name="doOutput">Will be called on the home thread.</param>
        public ProcessOutputTimer(DoOutputCallback doOutput)
        {
            this.doOutput = doOutput;
            _timer = new Timer();
            _timer.Tick += _timer_Elapsed;
        }

        public void Start(int interval)
        {
            _timer.Stop();
            _timer.Interval = interval;
            _timer.Enabled = true;
        }

        public void Start()
        {
            Start(600);
        }

        public void Stop(bool flush)
        {
            _timer.Stop();
            if (flush)
                _timer_Elapsed(null, null);
        }

        /// <summary>
        /// Can be called on any thread.
        /// </summary>
        public void Append(string text)
        {
            lock (textToAdd)
            {
                textToAdd.Append(text);
            }
        }

        private void _timer_Elapsed(object sender, EventArgs eventArgs)
        {
            lock (textToAdd)
            {
                if (textToAdd.Length > 0)
                    doOutput?.Invoke(textToAdd.ToString());
                Clear();
            }
        }

        public void Clear()
        {
            lock (textToAdd)
            {
                textToAdd.Remove(0, textToAdd.Length);
            }
        }

        public void Dispose()
        {
            Stop(false);
            // clear will lock, to prevent outputting to disposed object
            Clear();
            doOutput = null;
            _timer.Dispose();
        }
    }
}
