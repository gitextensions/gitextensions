using System;

namespace ResourceManager
{
    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(string command, string data)
        {
            Command = command;
            Data = data;
        }

        public string Command { get; set; }
        public string Data { get; set; }
    }
}
