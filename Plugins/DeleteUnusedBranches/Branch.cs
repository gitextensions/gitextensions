using System;

namespace DeleteUnusedBranches
{
    public sealed class Branch
    {
        public Branch(string name, DateTime date, string author, string message, bool delete)
        {
            Name = name;
            Date = date;
            Author = author;
            Message = message;
            Delete = delete;
        }

        public string Name { get; private set; }
        public DateTime Date { get; private set; }
        public bool Delete { get; set; }
        public string Author { get; private set; }
        public string Message { get; private set; }
    }
}
