using System;
using System.Text;
using System.Timers;

namespace GitUI
{
    public sealed class ProcessOutputTimer : IDisposable
    {
        public delegate void DoOutputCallback(string text);
        private Timer _timer; 
        private StringBuilder textToAdd = new StringBuilder();
        private DoOutputCallback doOutput;

        public ProcessOutputTimer(DoOutputCallback doOutput)
        {
            this.doOutput = doOutput;
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
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

        public void Append(string text)
        {
            lock(textToAdd)
            {
                textToAdd.Append(text);
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (textToAdd)
            {
                if (textToAdd.Length > 0 && doOutput != null)
                    doOutput(textToAdd.ToString());
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
            //clear will lock, to prevent outputting to disposed object
            Clear();
            doOutput = null;
            _timer.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
