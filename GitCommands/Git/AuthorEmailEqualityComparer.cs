using System;
using System.Collections.Generic;

namespace GitCommands.Git
{
    public sealed class AuthorEmailEqualityComparer : IEqualityComparer<GitRevision>, IEqualityComparer<string>
    {
        private static readonly AuthorEmailEqualityComparer CachedInstance = new AuthorEmailEqualityComparer(); 
        public static AuthorEmailEqualityComparer Instance
        {
            get { return CachedInstance; }
        }

        public bool Equals(GitRevision x, GitRevision y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return Equals(x.AuthorEmail, y.AuthorEmail);
        }

        public int GetHashCode(GitRevision revision)
        {
            return GetHashCode(revision.AuthorEmail);
        } 

        public bool Equals(string firstAuthorEmail, string secondAuthorEmail)
        {
            return String.Equals(firstAuthorEmail, secondAuthorEmail, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string authorEmail)
        {
            return authorEmail != null ? authorEmail.GetHashCode() : 0;
        }
    }
}