using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

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

        private sealed partial class LostObject
        {
            [GeneratedRegex(@"^(?<rawtype>(dangling|missing|unreachable) (?<objecttype>commit|blob|tree|tag)|warning in tree) (?<objectid>[a-f\d]{40})(.)*$", RegexOptions.ExplicitCapture)]
            private static partial Regex RawDataRegex();
            [GeneratedRegex(@"^(?<author>[^\u001F]+)\u001F(?<subject>.*)\u001F(?<date>\d+)\u001F(?<first_parent>[^ ]+)?( .+)?$", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
            private static partial Regex LogRegex();
            [GeneratedRegex(@"^object (?<parent>.+)\ntype commit\ntag (?<tagname>.+)\ntagger (?<author>.+) <.*> (?<date>.+) .*\n\n(?<subject>.*)\n", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
            private static partial Regex TagRegex();

            /// <summary>
            /// {0} - lost object's hash.
            /// %aN - committer name.
            /// %s  - subject.
            /// %ct - committer date, UNIX timestamp (easy to parse format).
            /// </summary>
            private static readonly string ShowCommandArgumentsFormat = (ArgumentString)new GitArgumentBuilder("show")
            {
                "--quiet",
                "--pretty=format:\"%aN\u001F%s\u001F%ct\u001F%P\" {0}"
            };

            private static readonly string TagCommandArgumentsFormat = (ArgumentString)new GitArgumentBuilder("cat-file")
            {
                "-p",
                "{0}"
            };

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
            public string RawType { get; set; }

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

            /// <summary>
            /// Retrieve commits metadata for all the commits hashes provided.
            /// </summary>
            /// <param name="module">Git module to run the git command.</param>
            /// <param name="commitIds">The collection of commits hashes.</param>
            /// <returns>the metadata for all the commits.</returns>
            public static string[] GetCommitsMetadata(IGitModule module, IEnumerable<string> commitIds)
            {
                return module.GitExecutable
                    .GetOutput(string.Format(ShowCommandArgumentsFormat, string.Join(" ", commitIds)), outputEncoding: GitModule.LosslessEncoding)
                    .Split('\n');
            }

            public void FillCommitData(IGitModule module, string commitLog)
            {
                Match logPatternMatch = LogRegex().Match(commitLog);
                if (logPatternMatch.Success)
                {
                    // TODO: cache
                    Author = module.ReEncodeStringFromLossless(logPatternMatch.Groups["author"].Value);
                    Subject = module.ReEncodeCommitMessage(logPatternMatch.Groups["subject"].Value) ?? "";
                    Date = DateTimeUtils.ParseUnixTime(logPatternMatch.Groups["date"].Value);
                    string firstParent = logPatternMatch.Groups["first_parent"].Value;
                    if (!string.IsNullOrEmpty(firstParent))
                    {
                        Parent = ObjectId.Parse(firstParent);
                    }
                }
            }

            public static LostObject? TryParse(IGitModule module, string raw)
            {
                if (string.IsNullOrEmpty(raw))
                {
                    throw new ArgumentException("Raw source must be non-empty string", raw);
                }

                Match patternMatch = RawDataRegex().Match(raw);

                // show failed assertion for unsupported cases (for developers)
                // if you get this message,
                //     you can implement this format parsing
                //     or post an issue to https://github.com/gitextensions/gitextensions/issues
                DebugHelpers.Assert(patternMatch.Success, "Lost object's extracted diagnostics format not implemented");

                // skip unsupported raw data format (for end users)
                if (!patternMatch.Success)
                {
                    return null;
                }

                string rawType = patternMatch.Groups["rawtype"].Value;
                LostObjectType objectType = GetObjectType(patternMatch.Groups["objecttype"]);
                ObjectId objectId = ObjectId.Parse(raw, patternMatch.Groups["objectid"]);
                LostObject result = new(objectType, rawType, objectId);

                if (objectType == LostObjectType.Commit)
                {
                    // perf: Commit metadata will be fetched by batch
                }
                else if (objectType == LostObjectType.Tag)
                {
                    string tagData = GetLostTagData();
                    Match tagPatternMatch = TagRegex().Match(tagData);
                    if (tagPatternMatch.Success)
                    {
                        result.Parent = ObjectId.Parse(tagData, tagPatternMatch.Groups["parent"]);
                        result.Author = module.ReEncodeStringFromLossless(tagPatternMatch.Groups["author"].Value);
                        result.TagName = tagPatternMatch.Groups["tagname"].Value;
                        result.Subject = result.TagName + ":" + tagPatternMatch.Groups["subject"].Value;
                        result.Date = DateTimeUtils.ParseUnixTime(tagPatternMatch.Groups["date"].Value);
                    }
                }
                else if (objectType == LostObjectType.Blob)
                {
                    string hash = objectId.ToString();
                    string blobPath = Path.Combine(module.WorkingDirGitDir, "objects", hash[..2], hash[2..ObjectId.Sha1CharCount]);
                    result.Date = new FileInfo(blobPath).CreationTime;
                }

                return result;

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
