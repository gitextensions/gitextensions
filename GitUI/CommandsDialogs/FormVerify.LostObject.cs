﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtUtils;
using GitUIPluginInterfaces;

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
            private static readonly string LogCommandArgumentsFormat = (ArgumentString)new GitArgumentBuilder("log")
            {
                "-n1",
                "--pretty=format:\"%aN\u001F%s\u001F%ct\u001F%P\" {0}"
            };

            private static readonly string TagCommandArgumentsFormat = (ArgumentString)new GitArgumentBuilder("cat-file")
            {
                "-p",
                "{0}"
            };

            private static readonly Regex RawDataRegex = new(@"^((dangling|missing|unreachable) (commit|blob|tree|tag)|warning in tree) ([a-f\d]{40})(.)*$", RegexOptions.Compiled);
            private static readonly Regex LogRegex = new("^(?<author>[^\u001F]+)\u001F(?<subject>.*)\u001F(?<date>\\d+)\u001F(?<first_parent>[^ ]+)?( .+)?$", RegexOptions.Compiled | RegexOptions.Singleline);
            private static readonly Regex TagRegex = new(@"^object (.+)\ntype commit\ntag (.+)\ntagger (.+) <.*> (.+) .*\n\n(.*)\n", RegexOptions.Compiled | RegexOptions.Multiline);

            public LostObjectType ObjectType { get; }

            /// <summary>
            /// Id (SHA-1 hash) of the lost object.
            /// </summary>
            public ObjectId ObjectId { get; }

            /// <summary>
            /// Id (SHA-1 hash) of parent commit to the lost object.
            /// </summary>
            public ObjectId? Parent { get; private set; }

            /// <summary>
            /// Diagnostics and object type.
            /// </summary>
            public string RawType { get; }

            public string? Author { get; private set; }
            public string? Subject { get; private set; }
            public DateTime? Date { get; private set; }

            /// <summary>
            /// Tag name (for a tag object).
            /// </summary>
            public string? TagName { get; set; }

            private LostObject(LostObjectType objectType, string rawType, ObjectId objectId)
            {
                // TODO use enum for RawType
                ObjectType = objectType;
                RawType = rawType;
                ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));
            }

            public static LostObject? TryParse(GitModule module, string raw)
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
                var rawType = matchedGroups[1].Value;
                var objectType = GetObjectType(matchedGroups[3]);
                var objectId = ObjectId.Parse(raw, matchedGroups[4]);
                LostObject result = new(objectType, rawType, objectId);

                if (objectType == LostObjectType.Commit)
                {
                    var commitLog = GetLostCommitLog();
                    var logPatternMatch = LogRegex.Match(commitLog);
                    if (logPatternMatch.Success)
                    {
                        result.Author = module.ReEncodeStringFromLossless(logPatternMatch.Groups["author"].Value);
                        result.Subject = module.ReEncodeCommitMessage(logPatternMatch.Groups["subject"].Value) ?? "";
                        result.Date = DateTimeUtils.ParseUnixTime(logPatternMatch.Groups["date"].Value);
                        var firstParent = logPatternMatch.Groups["first_parent"].Value;
                        if (!string.IsNullOrEmpty(firstParent))
                        {
                            result.Parent = ObjectId.Parse(firstParent);
                        }
                    }
                }
                else if (objectType == LostObjectType.Tag)
                {
                    var tagData = GetLostTagData();
                    var tagPatternMatch = TagRegex.Match(tagData);
                    if (tagPatternMatch.Success)
                    {
                        result.Parent = ObjectId.Parse(tagData, tagPatternMatch.Groups[1]);
                        result.Author = module.ReEncodeStringFromLossless(tagPatternMatch.Groups[3].Value);
                        result.TagName = tagPatternMatch.Groups[2].Value;
                        result.Subject = result.TagName + ":" + tagPatternMatch.Groups[5].Value;
                        result.Date = DateTimeUtils.ParseUnixTime(tagPatternMatch.Groups[4].Value);
                    }
                }
                else if (objectType == LostObjectType.Blob)
                {
                    var hash = objectId.ToString();
                    var blobPath = Path.Combine(module.WorkingDirGitDir, "objects", hash.Substring(0, 2), hash.Substring(2, ObjectId.Sha1CharCount - 2));
                    result.Date = new FileInfo(blobPath).CreationTime;
                }

                return result;

                string GetLostCommitLog() => VerifyHashAndRunCommand(LogCommandArgumentsFormat);
                string GetLostTagData() => VerifyHashAndRunCommand(TagCommandArgumentsFormat);

                string VerifyHashAndRunCommand(ArgumentString commandFormat)
                {
                    return module.GitExecutable.GetOutput(string.Format(commandFormat, objectId), outputEncoding: GitModule.LosslessEncoding);
                }

                LostObjectType GetObjectType(Group matchedGroup)
                {
                    if (!matchedGroup.Success)
                    {
                        return LostObjectType.Other;
                    }

                    return matchedGroup.Value switch
                    {
                        "commit" => LostObjectType.Commit,
                        "blob" => LostObjectType.Blob,
                        "tree" => LostObjectType.Tree,
                        "tag" => LostObjectType.Tag,
                        _ => LostObjectType.Other
                    };
                }
            }
        }
    }
}
