using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
