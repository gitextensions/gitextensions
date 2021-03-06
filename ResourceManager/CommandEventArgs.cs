using System;

namespace ResourceManager
{
    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(string command, string? data)
        {
            Command = command;
            Data = data;
        }

        public string Command { get; }
        public string? Data { get; }
    }
}
