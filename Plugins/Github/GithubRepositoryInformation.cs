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
            GithubRepositoryInformation repoInfo = obj as GithubRepositoryInformation;
            return repoInfo != null && Owner == repoInfo.Owner && Name == repoInfo.Name;
        }

        public override int GetHashCode()
        {
            return (Owner ?? "").GetHashCode() + (Name ?? "").GetHashCode();
        }
    }
}
    