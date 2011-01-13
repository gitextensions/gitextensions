using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Github
{
    public class GithubHostedRemoteInformation
    {
        public string Name { get; set; } //Name locally, IE name of remote, like "origin"
        public string Owner { get; set; }
        public string NameAtGithub { get; set; }

        public override bool Equals(object obj)
        {
            GithubHostedRemoteInformation repoInfo = obj as GithubHostedRemoteInformation;
            return repoInfo != null && Owner == repoInfo.Owner && Name == repoInfo.Name && NameAtGithub == repoInfo.NameAtGithub;
        }

        public override int GetHashCode()
        {
            return (Owner ?? "").GetHashCode() + (Name ?? "").GetHashCode() + (NameAtGithub ?? "").GetHashCode();
        }
    }
}
    