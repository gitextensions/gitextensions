using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;

namespace GitCommands
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

        public static readonly Regex Sha1HashRegex = new Regex(@"^[a-f\d]{40}$", RegexOptions.Compiled);
        public static readonly Regex Sha1HashShortRegex = new Regex(@"\b[a-f\d]{7,40}\b", RegexOptions.Compiled);

        private BuildInfo _buildStatus;
        private const int CommitSummaryMaxLineLength = 150;
        private const int CommitSummaryMaxNumberOfLines = 30;

        public GitRevision([NotNull] ObjectId objectId)
        {
            ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));
        }

        [NotNull]
        public ObjectId ObjectId { get; }

        [NotNull]
        public string Guid => ObjectId.ToString();

        // TODO this should probably be null when not yet populated, similar to how ParentIds works
        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> Refs { get; set; } = Array.Empty<IGitRef>();

        /// <summary>
        /// Gets the revision's parent IDs.
        /// </summary>
        /// <remarks>
        /// Can return <c>null</c> in cases where the data has not been populated
        /// for whatever reason.
        /// </remarks>
        [CanBeNull, ItemNotNull]
        public IReadOnlyList<ObjectId> ParentIds { get; set; }

        public ObjectId TreeGuid { get; set; }

        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Committer { get; set; }
        public string CommitterEmail { get; set; }
        public DateTime CommitDate { get; set; }

        [CanBeNull]
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

        public string Subject { get; set; } = "";
        [CanBeNull]
        public string Body { get; set; }

        [CanBeNull]
        public string BodySummary
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Body) || !HasMultiLineMessage)
                {
                    return Body;
                }

                return BuildSummary(Body.TrimEnd());
            }
        }

        private static string BuildSummary(string body)
        {
            var s = new StringBuilder(100);

            int lineCount = 0;
            int lineStartPos = 0;
            int pos;
            for (pos = 0; pos < body.Length; ++pos)
            {
                if (body[pos] == '\n')
                {
                    if (pos - lineStartPos > CommitSummaryMaxLineLength)
                    {
                        AppendLine(CommitSummaryMaxLineLength, withEllipsis: true);
                    }
                    else
                    {
                        AppendLine(pos - lineStartPos);
                    }

                    lineStartPos = pos + 1;

                    if (++lineCount == CommitSummaryMaxNumberOfLines)
                    {
                        return s.Append("[...]").ToString();
                    }
                }

                if (pos == body.Length - 1)
                {
                    if (pos - lineStartPos > CommitSummaryMaxLineLength)
                    {
                        AppendLine(CommitSummaryMaxLineLength, withEllipsis: true);
                    }
                    else
                    {
                        AppendLine(pos - lineStartPos + 1);
                    }
                }

                void AppendLine(int length, bool withEllipsis = false)
                {
                    s.Append(body.Substring(lineStartPos, length));
                    if (withEllipsis)
                    {
                        s.Append(" [...]");
                    }

                    s.Append("\n");
                }
            }

            return s.ToString().TrimEnd('\n');
        }

        public bool HasMultiLineMessage { get; set; }
        public bool HasNotes { get; set; }

        // UTF-8 when is null or empty
        public string MessageEncoding { get; set; }

        public string Name { get; set; }

        public override string ToString() => $"{ObjectId.ToShortString(8)}:{Subject}";

        /// <summary>
        /// Indicates whether the commit is an artificial commit.
        /// </summary>
        public bool IsArtificial => ObjectId.IsArtificial;

        public bool HasParent => ParentIds?.Count > 0;

        [CanBeNull]
        public ObjectId FirstParentGuid => ParentIds?.FirstOrDefault();

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
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