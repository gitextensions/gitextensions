using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    public sealed class GitRevision : IGitItem, INotifyPropertyChanged
    {
        /// <summary>40 characters of 1's</summary>
        public const string WorkTreeGuid = "1111111111111111111111111111111111111111";

        /// <summary>40 characters of 2's</summary>
        public const string IndexGuid = "2222222222222222222222222222222222222222";

        /// <summary>40 characters of 2's
        /// Artificial commit for the combined diff</summary>
        public const string CombinedDiffGuid = "3333333333333333333333333333333333333333";

        public static readonly Regex Sha1HashRegex = new(@"^[a-f\d]{40}$", RegexOptions.Compiled);
        public static readonly Regex Sha1HashShortRegex = new(@"\b[a-f\d]{7,40}\b(?![^@\s]*@)", RegexOptions.Compiled);

        private BuildInfo? _buildStatus;
        private string? _body;

        public GitRevision(ObjectId objectId)
        {
            ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));
        }

        /// <summary>
        /// Make a shallow clone of the object.
        /// </summary>
        /// <returns>A shallow copy.</returns>
        public GitRevision Clone()
        {
            return (GitRevision)MemberwiseClone();
        }

        public ObjectId ObjectId { get; }

        public string Guid => ObjectId.ToString();

        // TODO this should probably be null when not yet populated, similar to how ParentIds works
        public IReadOnlyList<IGitRef> Refs { get; set; } = Array.Empty<IGitRef>();

        /// <summary>
        /// Gets the revision's parent IDs.
        /// </summary>
        /// <remarks>
        /// Can return <c>null</c> in cases where the data has not been populated
        /// for whatever reason.
        /// </remarks>
        public IReadOnlyList<ObjectId>? ParentIds { get; set; }

        public ObjectId? TreeGuid { get; set; }

        public string? Author { get; set; }
        public string? AuthorEmail { get; set; }

        // Git native datetime format
        public long AuthorUnixTime { get; set; }
        public DateTime AuthorDate => FromUnixTimeSeconds(AuthorUnixTime);
        public string? Committer { get; set; }
        public string? CommitterEmail { get; set; }
        public long CommitUnixTime { get; set; }
        public DateTime CommitDate => FromUnixTimeSeconds(CommitUnixTime);

        private static DateTime FromUnixTimeSeconds(long unixTime)
            => unixTime == 0 ? DateTime.MaxValue : DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;

        public BuildInfo? BuildStatus
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

        public string Subject { get; set; } = "";

        public string? Body
        {
            // Body is not stored by default for older commits to reduce memory usage
            // Body do not have to be stored explicitly if same as subject and not multiline
            get => _body ?? (!HasMultiLineMessage ? Subject : null);
            set => _body = value;
        }

        public bool HasMultiLineMessage { get; set; }
        public bool HasNotes { get; set; }

        public override string ToString() => $"{ObjectId.ToShortString()}:{Subject}";

        /// <summary>
        /// Indicates whether the commit is an artificial commit.
        /// </summary>
        public bool IsArtificial => ObjectId.IsArtificial;

        /// <summary>
        /// Indicates whether the commit is a main stash commit.
        /// </summary>
        public bool IsStash => ReflogSelector is not null;

        /// <summary>
        /// The reflog selector, contains the stash name like "stash{0}"
        /// </summary>
        public string? ReflogSelector { get; set; }

        public bool HasParent => ParentIds?.Count > 0;

        public ObjectId? FirstParentId => ParentIds?.FirstOrDefault();

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

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
