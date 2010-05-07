using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GitCommands
{
    public class Repository
    {
        public Repository()
        {
        }

        public Repository(string path, string description, Icon icon)
        {
            Path = path;
            Description = description;
            Icon = icon;
        }
        public string Path { get; set; }
        public string Description { get; set; }
        public Icon Icon { get; set; }
    }
}
