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

        public Repository(string path, string description, string title)
        {
            Path = path;
            Description = description;
            Title = title;
            FromRssFeed = false;
        }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public bool FromRssFeed { get; set; }
    }
}
