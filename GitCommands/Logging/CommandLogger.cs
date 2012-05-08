﻿using System;
using System.Collections.Generic;

namespace GitCommands.Logging
{
    public sealed class CommandLogger
    {
        public delegate void CommandsChangedHandler();
        private const int LogLimit = 100;
        private readonly Queue<string> _logQueue = new Queue<string>(LogLimit);

        public event CommandsChangedHandler CommandsChanged = delegate { };

        public string[] GetCommands()
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
            CommandsChanged();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, GetCommands());
        }
    }
}