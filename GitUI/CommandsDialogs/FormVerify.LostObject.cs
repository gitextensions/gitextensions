using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

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
            private const string RawDataPattern = @"^((dangling|missing|unreachable) (commit|blob|tree|tag)|warning in tree) ([a-f\d]{40})(.)*$";

            private const string TagCommandArgumentsFormat = "cat-file -p {0}";
            private const string TagPattern = @"^object (.+)\ntype commit\ntag (.+)\ntagger (.+) <.*> (.+) .*\n\n(.*)\n";

            private static readonly Regex RawDataRegex = new Regex(RawDataPattern, RegexOptions.Compiled);
            private static readonly Regex LogRegex = new Regex(LogPattern, RegexOptions.Compiled | RegexOptions.Singleline);
            private static readonly Regex TagRegex = new Regex(TagPattern, RegexOptions.Compiled | RegexOptions.Multiline);

            public LostObjectType ObjectType { get; }

            /// <summary>
            /// Id (SHA-1 hash) of the lost object.
            /// </summary>
            [NotNull]
            public ObjectId ObjectId { get; }

            /// <summary>
            /// Id (SHA-1 hash) of parent commit to the lost object.
            /// </summary>
            [CanBeNull]
            public ObjectId Parent { get; private set; }

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

            private LostObject(LostObjectType objectType, string rawType, [NotNull] ObjectId objectId)
            {
                // TODO use enum for RawType
                ObjectType = objectType;
                RawType = rawType;
                ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));
            }

            [CanBeNull]
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
                var objectId = ObjectId.Parse(matchedGroups[4].Value);

                var result = new LostObject(GetObjectType(matchedGroups), matchedGroups[1].Value, objectId);

                if (result.ObjectType == LostObjectType.Commit)
                {
                    var commitLog = GetLostCommitLog();
                    var logPatternMatch = LogRegex.Match(commitLog);
                    if (logPatternMatch.Success)
                    {
                        result.Author = module.ReEncodeStringFromLossless(logPatternMatch.Groups[1].Value);
                        string encodingName = logPatternMatch.Groups[2].Value;
                        result.Subject = module.ReEncodeCommitMessage(logPatternMatch.Groups[3].Value, encodingName);
                        result.Date = DateTimeUtils.ParseUnixTime(logPatternMatch.Groups[4].Value);
                        if (logPatternMatch.Groups.Count >= 5)
                        {
                            var parentId = logPatternMatch.Groups[5].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                            if (parentId != null)
                            {
                                result.Parent = ObjectId.Parse(parentId);
                            }
                        }
                    }
                }
                else if (result.ObjectType == LostObjectType.Tag)
                {
                    var tagData = GetLostTagData();
                    var tagPatternMatch = TagRegex.Match(tagData);
                    if (tagPatternMatch.Success)
                    {
                        result.Parent = ObjectId.Parse(tagPatternMatch.Groups[1].Value);
                        result.Author = module.ReEncodeStringFromLossless(tagPatternMatch.Groups[3].Value);
                        result.TagName = tagPatternMatch.Groups[2].Value;
                        result.Subject = result.TagName + ":" + tagPatternMatch.Groups[5].Value;
                        result.Date = DateTimeUtils.ParseUnixTime(tagPatternMatch.Groups[4].Value);
                    }
                }
                else if (result.ObjectType == LostObjectType.Blob)
                {
                    var hash = objectId.ToString();
                    var blobPath = Path.Combine(module.WorkingDirGitDir, "objects", hash.Substring(0, 2), hash.Substring(2, ObjectId.Sha1CharCount - 2));
                    result.Date = new FileInfo(blobPath).CreationTime;
                }

                return result;

                string GetLostCommitLog() => VerifyHashAndRunCommand(LogCommandArgumentsFormat);
                string GetLostTagData() => VerifyHashAndRunCommand(TagCommandArgumentsFormat);

                string VerifyHashAndRunCommand(string commandFormat)
                {
                    return module.RunGitCmd(string.Format(commandFormat, objectId), GitModule.LosslessEncoding);
                }
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
