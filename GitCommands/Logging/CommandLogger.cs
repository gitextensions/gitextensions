using System.Collections.Generic;
using System.Text;
using System;

namespace GitCommands.Logging
{
    public class CommandLogger
    {
        public delegate void CommandsChangedHandler(CommandLogger sender);

        private const int LogLimit = 100;
        private Queue<string> _logQueue = new Queue<string>(LogLimit);       
        private CommandsChangedHandler _CommandsChanged;

        public string[] Commands()
        {
            lock (_logQueue)
            {
                return _logQueue.ToArray();
            }
        }

        public void Log(string command)
        {
            lock (_logQueue)
            {
                if (_logQueue.Count >= LogLimit)
                    _logQueue.Dequeue();

                _logQueue.Enqueue(command);
            }
            FireCommandsChanged();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            var logQueueArray = Commands();

            for (int n = logQueueArray.Length; n > 0; n--)
            {
                stringBuilder.AppendLine(logQueueArray[n - 1]);
            }

            return stringBuilder.ToString();
        }

        public event CommandsChangedHandler CommandsChanged
        {
            add
            {
                lock (_logQueue)
                {
                    _CommandsChanged += value;
                }
            }
            remove
            {
                lock (_logQueue)
                {
                    _CommandsChanged -= value;
                }
            }
        }

        protected void FireCommandsChanged()
        {
            CommandsChangedHandler handler;
            lock (_logQueue)
            {
                handler = _CommandsChanged;
            }
            if (handler != null)
            {
                handler (this);
            }
        }

    }
}