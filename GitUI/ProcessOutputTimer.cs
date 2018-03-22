using System;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public sealed class ProcessOutputTimer : IDisposable
    {
        public delegate void DoOutputCallback(string text);

        private readonly Timer _timer;
        private readonly StringBuilder _textToAdd = new StringBuilder();
        private DoOutputCallback _doOutput;

        /// <param name="doOutput">Will be called on the home thread.</param>
        public ProcessOutputTimer(DoOutputCallback doOutput)
        {
            _doOutput = doOutput;
            _timer = new Timer();
            _timer.Tick += _timer_Elapsed;
        }

        public void Start(int intervalMillis = 600)
        {
            _timer.Stop();
            _timer.Interval = intervalMillis;
            _timer.Enabled = true;
        }

        public void Stop(bool flush)
        {
            _timer.Stop();

            if (flush)
            {
                _timer_Elapsed(null, null);
            }
        }

        /// <summary>
        /// Can be called on any thread.
        /// </summary>
        public void Append(string text)
        {
            lock (_textToAdd)
            {
                _textToAdd.Append(text);
            }
        }

        private void _timer_Elapsed(object sender, EventArgs eventArgs)
        {
            lock (_textToAdd)
            {
                if (_textToAdd.Length > 0)
                {
                    _doOutput?.Invoke(_textToAdd.ToString());
                }

                Clear();
            }
        }

        public void Clear()
        {
            lock (_textToAdd)
            {
                _textToAdd.Remove(0, _textToAdd.Length);
            }
        }

        public void Dispose()
        {
            Stop(false);

            // clear will lock, to prevent outputting to disposed object
            Clear();
            _doOutput = null;
            _timer.Dispose();
        }
    }
}
