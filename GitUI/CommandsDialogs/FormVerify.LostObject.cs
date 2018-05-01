using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    partial class FormVerify
    {
        private enum LostObjectType
        {
            Commit,
            Blob,
            Tree,
            Tag,
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
            private const string LogCommandArgumentsFormat = "log -n1 --pretty=format:\"%aN, %e, %s, %ct, %P\" {0}";
            private const string LogPattern = @"^([^,]+), (.*), (.+), (\d+), (.+)?$";
            private const string RawDataPattern = "^((dangling|missing|unreachable) (commit|blob|tree|tag)|warning in tree) (" + GitRevision.Sha1HashPattern + ")(.)*$";

            private const string TagCommandArguments = "cat-file -p {0}";
            private const string TagPattern = @"^object (.+)\ntype commit\ntag (.+)\ntagger (.+) <.*> (.+) .*\n\n(.*)\n";

            private static readonly Regex RawDataRegex = new Regex(RawDataPattern, RegexOptions.Compiled);
            private static readonly Regex LogRegex = new Regex(LogPattern, RegexOptions.Compiled | RegexOptions.Singleline);
            private static readonly Regex TagRegex = new Regex(TagPattern, RegexOptions.Compiled | RegexOptions.Multiline);

            public LostObjectType ObjectType { get; }

            /// <summary>
            /// Sha1 hash of lost object.
            /// </summary>
            public string Hash { get; }

            /// <summary>
            /// Sha1 hash of parent commit of commit lost object.
            /// </summary>
            public string Parent { get; private set; }

            /// <summary>
            /// Diagnostics and object type.
            /// </summary>
            public string RawType { get; }

            public string Author { get; private set; }
            public string Subject { get; private set; }
            public DateTime? Date { get; private set; }

            /// <summary>
            /// Tag name (for a tag object)
            /// </summary>
            public string TagName { get; set; }

            private LostObject(LostObjectType objectType, string rawType, string hash)
            {
                ObjectType = objectType;
                RawType = rawType;
                Hash = hash;
            }

            public static LostObject TryParse(GitModule module, string raw)
            {
                if (string.IsNullOrEmpty(raw))
                {
                    throw new ArgumentException("Raw source must be non-empty string", raw);
                }

                var patternMatch = RawDataRegex.Match(raw);

                // show failed assertion for unsupported cases (for developers)
                // if you get this message,
                //     you can implement this format parsing
                //     or post an issue to https://github.com/gitextensions/gitextensions/issues
                Debug.Assert(patternMatch.Success, "Lost object's extracted diagnostics format not implemented", raw);

                // skip unsupported raw data format (for end users)
                if (!patternMatch.Success)
                {
                    return null;
                }

                var matchedGroups = patternMatch.Groups;
                Debug.Assert(matchedGroups[4].Success, "matchedGroups[4].Success");
                var hash = matchedGroups[4].Value;

                var result = new LostObject(GetObjectType(matchedGroups), matchedGroups[1].Value, hash);

                if (result.ObjectType == LostObjectType.Commit)
                {
                    var commitLog = GetLostCommitLog(module, hash);
                    var logPatternMatch = LogRegex.Match(commitLog);
                    if (logPatternMatch.Success)
                    {
                        result.Author = module.ReEncodeStringFromLossless(logPatternMatch.Groups[1].Value);
                        string encodingName = logPatternMatch.Groups[2].Value;
                        result.Subject = module.ReEncodeCommitMessage(logPatternMatch.Groups[3].Value, encodingName);
                        result.Date = DateTimeUtils.ParseUnixTime(logPatternMatch.Groups[4].Value);
                        if (logPatternMatch.Groups.Count >= 5)
                        {
                            result.Parent = logPatternMatch.Groups[5].Value;
                        }
                    }
                }

                if (result.ObjectType == LostObjectType.Tag)
                {
                    var tagData = GetLostTagData(module, hash);
                    var tagPatternMatch = TagRegex.Match(tagData);
                    if (tagPatternMatch.Success)
                    {
                        result.Parent = tagPatternMatch.Groups[1].Value;
                        result.Author = module.ReEncodeStringFromLossless(tagPatternMatch.Groups[3].Value);
                        result.TagName = tagPatternMatch.Groups[2].Value;
                        result.Subject = result.TagName + ":" + tagPatternMatch.Groups[5].Value;
                        result.Date = DateTimeUtils.ParseUnixTime(tagPatternMatch.Groups[4].Value);
                    }
                }

                if (result.ObjectType == LostObjectType.Blob)
                {
                    var blobPath = Path.Combine(module.WorkingDirGitDir, "objects", hash.Substring(0, 2), hash.Substring(2, hash.Length - 2));
                    result.Date = new FileInfo(blobPath).CreationTime;
                }

                return result;
            }

            private static string GetLostCommitLog(GitModule module, string hash)
            {
                return VerifyHashAndRunCommand(module, hash, LogCommandArgumentsFormat);
            }

            private static string VerifyHashAndRunCommand(GitModule module, string hash, string command)
            {
                if (string.IsNullOrEmpty(hash) || !GitRevision.Sha1HashRegex.IsMatch(hash))
                {
                    throw new ArgumentOutOfRangeException(nameof(hash), hash, "Hash must be a valid SHA-1 hash.");
                }

                return module.RunGitCmd(string.Format(command, hash), GitModule.LosslessEncoding);
            }

            private static string GetLostTagData(GitModule module, string hash)
            {
                return VerifyHashAndRunCommand(module, hash, TagCommandArguments);
            }

            private static LostObjectType GetObjectType(GroupCollection matchedGroup)
            {
                if (!matchedGroup[3].Success)
                {
                    return LostObjectType.Other;
                }

                switch (matchedGroup[3].Value)
                {
                    case "commit": return LostObjectType.Commit;
                    case "blob": return LostObjectType.Blob;
                    case "tree": return LostObjectType.Tree;
                    case "tag": return LostObjectType.Tag;
                    default: return LostObjectType.Other;
                }
            }
        }
    }
}
