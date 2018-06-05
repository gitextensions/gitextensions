using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using GitCommands.Git.Extensions;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;

namespace GitCommands
{
    public sealed class GitRevision : IGitItem, INotifyPropertyChanged
    {
        /// <summary>40 characters of 1's</summary>
        public const string UnstagedGuid = "1111111111111111111111111111111111111111";

        /// <summary>40 characters of 2's</summary>
        public const string IndexGuid = "2222222222222222222222222222222222222222";

        /// <summary>40 characters of 2's
        /// Artificial commit for the combined diff</summary>
        public const string CombinedDiffGuid = "3333333333333333333333333333333333333333";

        /// <summary>40 characters of a-f or any digit.</summary>
        public const string Sha1HashPattern = @"[a-f\d]{40}";

        public const string Sha1HashShortPattern = @"[a-f\d]{7,40}";
        public static readonly Regex Sha1HashRegex = new Regex("^" + Sha1HashPattern + "$", RegexOptions.Compiled);
        public static readonly Regex Sha1HashShortRegex = new Regex(string.Format(@"\b{0}\b", Sha1HashShortPattern), RegexOptions.Compiled);

        private BuildInfo _buildStatus;

        public GitRevision(string guid)
        {
            // TODO: this looks like an incorrect behaviour, rev.Guid must be validated and set to "" if null or empty.
            Guid = guid;
            Subject = "";
            SubjectCount = "";
        }

        public IReadOnlyList<IGitRef> Refs { get; set; } = Array.Empty<IGitRef>();

        // TODO seems inconsistent that this can be null, yet Refs is never null
        [CanBeNull, ItemNotNull]
        public IReadOnlyList<string> ParentGuids { get; set; }

        public ObjectId TreeGuid { get; set; }

        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public string CommitterEmail { get; set; }
        public DateTime CommitDate { get; set; }

        public BuildInfo BuildStatus
        {
            get => _buildStatus;
            set
            {
                if (Equals(value, _buildStatus))
                {
                    return;
                }

                _buildStatus = value;
                OnPropertyChanged();
            }
        }

        public string Subject { get; set; }

        // Count for artificial commits (could be changed to object lists)
        public string SubjectCount { get; set; }
        public string Body { get; set; }
        public bool HasMultiLineMessage { get; set; }

        // UTF-8 when is null or empty
        public string MessageEncoding { get; set; }

        #region IGitItem Members

        public string Guid { get; set; }
        public string Name { get; set; }

        #endregion

        [CanBeNull]
        public ObjectId ObjectId => ObjectId.TryParse(Guid, out var id) ? id : null;

        public override string ToString()
        {
            var sha = Guid;
            if (sha.Length > 8)
            {
                sha = sha.Substring(0, 4) + ".." + sha.Substring(sha.Length - 4, 4);
            }

            return string.Format("{0}:{1}{2}", sha, SubjectCount, Subject);
        }

        public static string ToShortSha(string sha)
        {
            if (sha == null)
            {
                throw new ArgumentNullException(nameof(sha));
            }

            const int maxShaLength = 10;
            if (sha.Length > maxShaLength)
            {
                sha = sha.Substring(0, maxShaLength);
            }

            return sha;
        }

        /// <summary>
        /// Indicates whether the commit is an artificial commit.
        /// </summary>
        public bool IsArtificial => Guid.IsArtificial();

        public bool HasParent => ParentGuids != null && ParentGuids.Count > 0;

        public string FirstParentGuid => ParentGuids?.FirstOrDefault();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Returns a value indicating whether <paramref name="id"/> is a valid SHA-1 hash.
        /// </summary>
        /// <remarks>
        /// To be valid the string must contain exactly 40 lower-case hexadecimal characters.
        /// </remarks>
        /// <param name="id">The string to validate.</param>
        /// <returns><c>true</c> if <paramref name="id"/> is a valid SHA-1 hash, otherwise <c>false</c>.</returns>
        public static bool IsFullSha1Hash(string id)
        {
            return Sha1HashRegex.IsMatch(id);
        }
    }
}