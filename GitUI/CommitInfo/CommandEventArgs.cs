﻿using System;

namespace GitUI.CommitInfo
{
    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(string command, string data)
        {
            this.Command = command;
            this.Data = data;
        }
        public string Command { get; set; }
        public string Data { get; set; }
    }
}
