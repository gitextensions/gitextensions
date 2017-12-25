using System;

namespace GitCommands.Git
{
    /// <summary>Stored local modifications.</summary>
    public class GitStash
    {
        /// <summary>"stash@{i}"</summary>
        private const string NameFormat = "stash@{{{0}}}";
        private readonly string _stash;


        /// <summary>Name of the stash. <remarks>Usually, "stash@{n}"</remarks></summary>
        public string Name { get; set; }
        /// <summary>Short description of the commit the stash was based on.</summary>
        public string Message { get; set; }
        /// <summary>Gets the index of the stash in the list.</summary>
        public int Index { get; set; }


        /// <summary>Initializes a new <see cref="GitStash"/> with all properties null.</summary>
        public GitStash(string stash)
        {
            if (string.IsNullOrWhiteSpace(stash))
            {
                throw new ArgumentException("stash");
            }
            _stash = stash;
        }

        public GitStash(string stash, int i)
            : this(stash)
        {
            // "stash@{i}: WIP on {branch}: {PreviousCommitMiniSHA} {PreviousCommitMessage}"
            // "stash@{i}: On {branch}: {Message}"
            // "stash@{i}: autostash"

            Index = i;

            Name = string.Format(NameFormat, Index);

            int msgStart = stash.IndexOf(':') + 1;
            if (msgStart < stash.Length)
            {
                Message = stash.Substring(msgStart).Trim();
            }
        }


        public override string ToString() { return Message; }

        public override bool Equals(object obj)
        {
            if (null == obj) { return false; }
            if (this == obj) { return true; }

            GitStash other = obj as GitStash;
            return other != null && Equals(other);
        }

        protected bool Equals(GitStash other)
        {
            return string.Equals(_stash, other._stash);
        }

        public override int GetHashCode()
        {
            return (_stash.GetHashCode());
        }
    }
}
