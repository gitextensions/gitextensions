using System;
using System.Text.RegularExpressions;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitUI
{
    public sealed partial class RevisionGridControl
    {
        private sealed class RevisionGridInMemFilter
        {
            private readonly string _authorFilter;
            private readonly Regex? _authorFilterRegex;
            private readonly string _committerFilter;
            private readonly Regex? _committerFilterRegex;
            private readonly string _messageFilter;
            private readonly Regex? _messageFilterRegex;
            private readonly string? _shaFilter;
            private readonly Regex? _shaFilterRegex;

            private RevisionGridInMemFilter(string? authorFilter, string? committerFilter, string? messageFilter, bool ignoreCase)
            {
                (_authorFilter, _authorFilterRegex) = SetUpVars(authorFilter, ignoreCase);
                (_committerFilter, _committerFilterRegex) = SetUpVars(committerFilter, ignoreCase);
                (_messageFilter, _messageFilterRegex) = SetUpVars(messageFilter, ignoreCase);

                if (!string.IsNullOrEmpty(_messageFilter) && ObjectId.IsValidPartial(_messageFilter, minLength: 5))
                {
                    (_shaFilter, _shaFilterRegex) = SetUpVars(messageFilter, false);
                }

                (string filterStr, Regex? filterRegex) SetUpVars(string? filterValue, bool ignoreKase)
                {
                    var filterStr = filterValue?.Trim() ?? string.Empty;

                    try
                    {
                        var options = ignoreKase ? RegexOptions.IgnoreCase : RegexOptions.None;
                        return (filterStr, new Regex(filterStr, options));
                    }
                    catch (ArgumentException)
                    {
                        return (filterStr, null);
                    }
                }
            }

            public bool Predicate(GitRevision rev)
            {
                return CheckCondition(_authorFilter, _authorFilterRegex, rev.Author) &&
                       CheckCondition(_committerFilter, _committerFilterRegex, rev.Committer) &&
                       (CheckCondition(_messageFilter, _messageFilterRegex, rev.Body) ||
                        (_shaFilter is not null && CheckCondition(_shaFilter, _shaFilterRegex, rev.Guid)));

                static bool CheckCondition(string? filter, Regex? regex, string? value)
                {
                    return string.IsNullOrEmpty(filter) ||
                           (regex is not null && value is not null && regex.IsMatch(value));
                }
            }

            public static RevisionGridInMemFilter? CreateIfNeeded(
                string? authorFilter,
                string? committerFilter,
                string? messageFilter,
                bool ignoreCase)
            {
                if (string.IsNullOrEmpty(authorFilter) &&
                    string.IsNullOrEmpty(committerFilter) &&
                    (string.IsNullOrEmpty(messageFilter) ||
                     !ObjectId.IsValidPartial(messageFilter, minLength: 5)))
                {
                    return null;
                }

                return new RevisionGridInMemFilter(
                    authorFilter,
                    committerFilter,
                    messageFilter,
                    ignoreCase);
            }
        }
    }
}
