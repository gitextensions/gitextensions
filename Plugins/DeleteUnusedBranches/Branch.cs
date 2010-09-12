using System;
using System.Collections.Generic;
using System.Text;

namespace DeleteUnusedBranches
{
    public class Branch
    {
        public Branch(string name, DateTime date, bool delete)
        {
            Name = name;
            Date = date;
            Delete = delete;
        }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public bool Delete { get; set; }
        public string Result { get; set; }
    }
}
