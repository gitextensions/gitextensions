using System;
﻿
namespace GitCommands
{
    public class GitStash
    {
        private string name;
        public string Name 
        { 
            get
            {
                return name;
            }
            set
            {
                name = value;
            } 
        }
        
        public string Message;

        public override string ToString()
        {
            return name;
        }
    }
}
