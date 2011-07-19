using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI
{
    partial class FormVerify
    {
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
            private readonly string type;
            private readonly string hash;
            private readonly string raw;

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
            public string Type { get { return type; } }

            public string Author { get; private set; }
            public string Subject { get; private set; }
            public DateTime? Date { get; private set; }

            private LostObject(string type, string hash, string raw)
            {
                this.type = type;
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

                var result = new LostObject(matchedGroups[1].Value, hash, raw);

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
        }
    }
}
