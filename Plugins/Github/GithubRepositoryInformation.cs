using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Github
{
    public class GithubRepositoryInformation
    {
        public string Owner { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if ((Owner == null) != (Name == null))
                return false;
            if (Owner == null)
                return true;
            return Owner.Equals(Name);
        }

        public override int GetHashCode()
        {
            return (Owner ?? "").GetHashCode() + (Name ?? "").GetHashCode();
        }
    }
}
