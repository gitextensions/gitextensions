using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace GitUI
{
    class ProcessOutputTimer
    {
        static Timer _timer; 
        public static StringBuilder linesToAdd = new StringBuilder();
        private static FormStatus _form;

        public static void Start(FormStatus form)
        {
            _form = form;
            _timer = new Timer(300);
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true;
        }

        public static void addLine(string line)
        {
            lock(linesToAdd)
            {
                linesToAdd.Append(line);
            }
        }
        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (linesToAdd)
            {
                if (linesToAdd.ToString().Length > 0)
                    _form.AddOutputCrossThread(linesToAdd.ToString());
                linesToAdd.Remove(0, linesToAdd.Length);
                //linesToAdd = new StringBuilder();
            }
        }
        public static void Stop()
        {
            _timer_Elapsed(null, null);
            _timer.Stop();
        }
    }
}
