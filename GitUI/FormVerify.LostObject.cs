using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI
{
    partial class FormVerify
    {
        private enum LostObjectType
        {
            Commit,
            Blob,
            Tree,
            Other
        }

        private sealed class LostObject
        {
            /// <summary>
            /// {0} - lost object's hash.
            /// %aN - committer name.
            /// %s  - subject.
            /// %ct - committer date, UNIX timestamp (easy to parse format).
            /// </summary>
            private const string LogCommandArgumentsFormat = "log -n1 --pretty=format:\"%aN, %s, %ct\" {0}";
            private const string LogPattern = @"^([^,]+), (.+), (\d+)$";
            private const string Sha1HashPattern = @"[a-f\d]{40}";
            private const string RawDataPattern = "^((dangling|missing|unreachable) (commit|blob|tree)|warning in tree) (" + Sha1HashPattern + ")(.)*$";

            private static readonly Regex Sha1HashRegex = new Regex("^" + Sha1HashPattern + "$", RegexOptions.Compiled);
            private static readonly Regex RawDataRegex = new Regex(RawDataPattern, RegexOptions.Compiled);
            private static readonly Regex LogRegex = new Regex(LogPattern, RegexOptions.Compiled);

            private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            private readonly LostObjectType objectType;
            private readonly string rawType;
            private readonly string hash;
            private readonly string raw;

            public LostObjectType ObjectType
            {
                get { return objectType; }
            }

            /// <summary>
            /// Raw data returned by fsck-objects command.
            /// </summary>
            public string Raw { get { return raw; } }

            /// <summary>
            /// Sha1 hash of lost object.
            /// </summary>
            public string Hash { get { return hash; } }

            /// <summary>
            /// Diagnostics and object type.
            /// </summary>
            public string RawType { get { return rawType; } }

            public string Author { get; private set; }
            public string Subject { get; private set; }
            public DateTime? Date { get; private set; }

            public bool IsSelected { get; set; }

            private LostObject(LostObjectType objectType, string rawType, string hash, string raw)
            {
                this.objectType = objectType;
                this.rawType = rawType;
                this.hash = hash;
                this.raw = raw;
            }

            public static LostObject TryParse(string raw)
            {
                if (string.IsNullOrEmpty(raw))
                    throw new ArgumentException("Raw source must be non-empty string", raw);

                var patterMatch = RawDataRegex.Match(raw);

                // show failed assertion for unsupported cases (for developers)
                // if you get this message, 
                //     you can implement this format parsing
                //     or post an issue to https://github.com/spdr870/gitextensions/issues
                Debug.Assert(patterMatch.Success, "Lost object's extracted diagnostics format not implemented", raw);

                // skip unsupported raw data format (for end users)
                if (!patterMatch.Success)
                    return null;

                var matchedGroups = patterMatch.Groups;
                Debug.Assert(matchedGroups[4].Success);
                var hash = matchedGroups[4].Value;

                var result = new LostObject(GetObjectType(matchedGroups), matchedGroups[1].Value, hash, raw);

                var objectInfo = GetLostObjectLog(hash);
                var logPatternMatch = LogRegex.Match(objectInfo);
                if (logPatternMatch.Success)
                {
                    result.Author = logPatternMatch.Groups[1].Value;
                    result.Subject = logPatternMatch.Groups[2].Value;
                    result.Date = UnixEpoch.AddSeconds(long.Parse(logPatternMatch.Groups[3].Value));
                }
                return result;
            }

            private static string GetLostObjectLog(string hash)
            {
                if (string.IsNullOrEmpty(hash) || !Sha1HashRegex.IsMatch(hash))
                    throw new ArgumentOutOfRangeException("hash", hash, "Hash must be a valid SHA1 hash.");

                return GitCommandHelpers.RunCmd(Settings.GitCommand, string.Format(LogCommandArgumentsFormat, hash));
            }

            private static LostObjectType GetObjectType(GroupCollection matchedGroup)
            {
                if (!matchedGroup[3].Success)
                    return LostObjectType.Other;

                switch (matchedGroup[3].Value)
                {
                    case "commit": return LostObjectType.Commit;
                    case "blob": return LostObjectType.Blob;
                    case "tree": return LostObjectType.Tree;
                    default: return LostObjectType.Other;
                }
            }
        }
    }
}
