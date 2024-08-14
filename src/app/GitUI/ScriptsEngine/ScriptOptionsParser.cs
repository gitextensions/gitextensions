using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Config;
using GitCommands.UserRepositoryHistory;
using GitCommands.Utils;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.ScriptsEngine
{
    public sealed partial class ScriptOptionsParser
    {
        /// <summary>
        /// Name of the option which requires the full commit message.
        /// </summary>
        private const string currentMessage = "cMessage";

        private const string head = "HEAD";

        [GeneratedRegex(@"(?<!\\)""")]
        private static partial Regex QuoteRegex();

        /// <summary>
        /// Gets the list of available script options.
        /// </summary>
        public static readonly IReadOnlyList<string> Options = new[]
        {
            "sHashes",
            "sTag",
            "sBranch",
            "sLocalBranch",
            "sRemoteBranch",
            "sRemoteBranchName",
            "sRemote",
            "sRemoteUrl",
            "sRemotePathFromUrl",
            "sHash",
            "sMessage",
            "sSubject",
            "sAuthor",
            "sCommitter",
            "sAuthorDate",
            "sCommitDate",
            head,
            "cTag",
            "cBranch",
            "cLocalBranch",
            "cRemoteBranch",
            "cRemoteBranchName",
            "cHash",
            currentMessage,
            "cSubject",
            "cAuthor",
            "cCommitter",
            "cAuthorDate",
            "cCommitDate",
            "cDefaultRemote",
            "cDefaultRemoteUrl",
            "cDefaultRemotePathFromUrl",
            "RepoName",
            "WorkingDir"
        };

        /// <summary>
        ///  Checks whether <paramref name="arguments"/> contains the supplied <paramref name="option"/>.
        /// </summary>
        /// <param name="arguments">The script argument.</param>
        /// <param name="option">The option.</param>
        /// <returns><see langword="true"/> if the argument contains the option; otherwise <see langword="false"/>.</returns>
        public static bool Contains(string arguments, string option)
        {
            arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            option = option ?? throw new ArgumentNullException(nameof(option));

            return arguments.Contains(CreateOption(option, quoted: false));
        }

        /// <summary>
        ///  Checks whether <paramref name="option"/> starts with "s".
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns><see langword="true"/> if the options starts with "s"; otherwise <see langword="false"/>.</returns>
        public static bool DependsOnSelectedRevision(string option)
        {
            option = option ?? throw new ArgumentNullException(nameof(option));

            return option.StartsWith("s");
        }

        public static (string? arguments, bool abort) Parse(string? arguments, IGitUICommands uiCommands, IWin32Window owner, IScriptOptionsProvider? scriptOptionsProvider = null)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                return (arguments, abort: false);
            }

            ArgumentNullException.ThrowIfNull(uiCommands);
            ArgumentNullException.ThrowIfNull(uiCommands.Module);

            bool selectedRevisionCached = false;
            GitRevision? selectedRevision = null;
            bool currentRevisionCached = false;
            GitRevision? currentRevision = null;

            IReadOnlyList<GitRevision> allSelectedRevisions = Array.Empty<GitRevision>();
            List<IGitRef> selectedLocalBranches = [];
            List<IGitRef> selectedRemoteBranches = [];
            List<string> selectedRemotes = [];
            List<IGitRef> selectedBranches = [];
            List<IGitRef> selectedTags = [];
            List<IGitRef> currentLocalBranches = [];
            List<IGitRef> currentRemoteBranches = [];
            string currentRemote = "";
            List<IGitRef> currentBranches = [];
            List<IGitRef> currentTags = [];
            IEnumerable<string> allOptions = GetOptions(Options, scriptOptionsProvider);

            foreach (string option in allOptions)
            {
                if (!Contains(arguments, option) && !Contains(arguments, "{if:" + option + "}") && !Contains(arguments, "{ifnot:" + option + "}"))
                {
                    continue;
                }

                if (currentRevisionCached && (option.StartsWith("c") || option == head))
                {
                    currentRevisionCached = true;
                    currentRevision = GetCurrentRevision(uiCommands.Module, currentTags, currentLocalBranches, currentRemoteBranches, currentBranches,
                        loadBody: Contains(arguments, currentMessage));
                    if (currentRevision is not null)
                    {
                        if (currentLocalBranches.Count == 1)
                        {
                            currentRemote = uiCommands.Module.GetSetting(string.Format(SettingKeyString.BranchRemote, currentLocalBranches[0].Name));
                        }
                        else
                        {
                            currentRemote = uiCommands.Module.GetCurrentRemote();
                            if (string.IsNullOrEmpty(currentRemote))
                            {
                                currentRemote = uiCommands.Module.GetSetting(string.Format(SettingKeyString.BranchRemote,
                                    AskToSpecify(currentLocalBranches, uiCommands, owner)));
                            }
                        }
                    }
                }
                else if (!selectedRevisionCached && uiCommands.BrowseRepo is not null && DependsOnSelectedRevision(option))
                {
                    selectedRevisionCached = true;
                    allSelectedRevisions = uiCommands.BrowseRepo.GetSelectedRevisions();
                    selectedRevision = CalculateSelectedRevision(uiCommands, selectedRemoteBranches, selectedRemotes, selectedLocalBranches, selectedBranches, selectedTags);
                }

                arguments = ParseScriptArguments(arguments, option, owner, scriptOptionsProvider, uiCommands, allSelectedRevisions, selectedTags, selectedBranches, selectedLocalBranches, selectedRemoteBranches, selectedRemotes, selectedRevision!, currentTags, currentBranches, currentLocalBranches, currentRemoteBranches, currentRevision!, currentRemote);
                if (arguments is null)
                {
                    return (arguments: null, abort: true);
                }
            }

            // Second pass - After all {if}s have been processed, verify that all options have been replaced
            foreach (string option in allOptions)
            {
                if (!Contains(arguments, option) && !Contains(arguments, "{if:" + option + "}") && !Contains(arguments, "{ifnot:" + option + "}"))
                {
                    continue;
                }

                return (arguments: null, abort: true);
            }

            return (arguments, abort: false);

            static IEnumerable<string> GetOptions(IReadOnlyList<string> options, IScriptOptionsProvider scriptOptionsProvider)
                => scriptOptionsProvider is null ? options : options.Union(scriptOptionsProvider.Options);
        }

        private static string AskToSpecify(IEnumerable<IGitRef> options, IGitUICommands uiCommands, IWin32Window owner)
        {
            List<IGitRef> items = options.ToList();
            if (items.Count == 0)
            {
                return string.Empty;
            }

            using FormQuickGitRefSelector f = new();
            f.Location = uiCommands.BrowseRepo?.GetQuickItemSelectorLocation() ?? new Point();
            f.Init(FormQuickGitRefSelector.QuickAction.Select, items);
            f.ShowDialog(owner);
            return f.SelectedRef?.Name ?? "";
        }

        private static string AskToSpecify(IEnumerable<string> options, IGitUICommands uiCommands, IWin32Window owner)
        {
            using FormQuickStringSelector f = new();
            f.Location = uiCommands.BrowseRepo?.GetQuickItemSelectorLocation() ?? new Point();
            f.Init(options.ToList());
            f.ShowDialog(owner);
            return f.SelectedString ?? "";
        }

        private static GitRevision? CalculateSelectedRevision(IGitUICommands uiCommands, List<IGitRef> selectedRemoteBranches,
            List<string> selectedRemotes, List<IGitRef> selectedLocalBranches,
            List<IGitRef> selectedBranches, List<IGitRef> selectedTags)
        {
            GitRevision? selectedRevision = uiCommands.BrowseRepo?.GetLatestSelectedRevision();
            if (selectedRevision is null)
            {
                return null;
            }

            foreach (IGitRef head in selectedRevision.Refs)
            {
                if (head.IsTag)
                {
                    selectedTags.Add(head);
                }
                else if (head.IsHead || head.IsRemote)
                {
                    selectedBranches.Add(head);
                    if (head.IsRemote)
                    {
                        selectedRemoteBranches.Add(head);
                        if (!selectedRemotes.Contains(head.Remote))
                        {
                            selectedRemotes.Add(head.Remote);
                        }
                    }
                    else
                    {
                        selectedLocalBranches.Add(head);
                    }
                }
            }

            return selectedRevision;
        }

        private static string CreateOption(string option, bool quoted)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return string.Empty;
            }

            string result = "{" + option.Trim() + "}";

            if (quoted)
            {
                result = "{" + result + "}";
            }

            return result;
        }

        private static GitRevision? GetCurrentRevision(
            IGitModule module, List<IGitRef> currentTags, List<IGitRef> currentLocalBranches,
            List<IGitRef> currentRemoteBranches, List<IGitRef> currentBranches, bool loadBody)
        {
            GitRevision currentRevision;
            IEnumerable<IGitRef> refs;
            currentRevision = module.GetRevision(shortFormat: !loadBody, loadRefs: true);
            refs = currentRevision?.Refs ?? Array.Empty<IGitRef>();

            foreach (IGitRef gitRef in refs)
            {
                if (gitRef.IsTag)
                {
                    currentTags.Add(gitRef);
                }
                else if (gitRef.IsHead || gitRef.IsRemote)
                {
                    currentBranches.Add(gitRef);
                    if (gitRef.IsRemote)
                    {
                        currentRemoteBranches.Add(gitRef);
                    }
                    else
                    {
                        currentLocalBranches.Add(gitRef);
                    }
                }
            }

            return currentRevision;
        }

        private static string GetRemotePath(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                Uri.TryCreate("ssh://" + url.Replace(":", "/"), UriKind.Absolute, out uri))
            {
                return uri.LocalPath.SubstringUntilLast('.');
            }

            return "";
        }

        private static string? ParseScriptArguments(string arguments, string option, IWin32Window owner,
            IScriptOptionsProvider? scriptOptionsProvider, IGitUICommands uiCommands, IReadOnlyList<GitRevision> allSelectedRevisions,
            in IList<IGitRef> selectedTags, in IList<IGitRef> selectedBranches, in IList<IGitRef> selectedLocalBranches,
            in IList<IGitRef> selectedRemoteBranches, in IList<string> selectedRemotes, GitRevision selectedRevision,
            in IList<IGitRef> currentTags, in IList<IGitRef> currentBranches, in IList<IGitRef> currentLocalBranches,
            in IList<IGitRef> currentRemoteBranches, GitRevision currentRevision, string currentRemote)
        {
            string? newString = null;
            string remote;
            string url;
            switch (option)
            {
                case "sHashes":
                    if (allSelectedRevisions is null)
                    {
                        break;
                    }

                    newString = string.Join(" ", allSelectedRevisions.Select(revision => revision.Guid).ToArray());
                    break;

                case "sTag":
                    if (selectedTags is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(selectedTags);
                    break;

                case "sBranch":
                    if (selectedBranches is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(selectedBranches);
                    break;

                case "sLocalBranch":
                    if (selectedLocalBranches is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(selectedLocalBranches);
                    break;

                case "sRemoteBranch":
                    if (selectedRemoteBranches is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(selectedRemoteBranches);
                    break;

                case "sRemoteBranchName":
                    if (selectedRemoteBranches is null)
                    {
                        break;
                    }

                    newString = StripRemoteName(SelectOneRef(selectedRemoteBranches));
                    break;

                case "sRemote":
                    if (selectedRemotes is null)
                    {
                        break;
                    }

                    newString = SelectOneString(selectedRemotes);
                    break;

                case "sRemoteUrl":
                    if (selectedRemotes is null)
                    {
                        break;
                    }

                    newString = SelectOneString(selectedRemotes);
                    if (!string.IsNullOrEmpty(newString))
                    {
                        remote = newString;
                        newString = uiCommands.Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                    }

                    break;

                case "sRemotePathFromUrl":
                    if (selectedRemotes is null)
                    {
                        break;
                    }

                    newString = SelectOneString(selectedRemotes);
                    if (!string.IsNullOrEmpty(newString))
                    {
                        remote = newString;
                        url = uiCommands.Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                        newString = GetRemotePath(url);
                    }

                    break;

                case "sHash":
                    if (selectedRevision is null)
                    {
                        break;
                    }

                    newString = selectedRevision.Guid;
                    break;

                case "sMessage":
                    if (selectedRevision is null)
                    {
                        break;
                    }

                    newString = EscapeLinefeeds(selectedRevision.Body) ?? selectedRevision.Subject;
                    break;

                case "sSubject":
                    if (selectedRevision is null)
                    {
                        break;
                    }

                    newString = selectedRevision.Subject;
                    break;

                case "sAuthor":
                    if (selectedRevision is null)
                    {
                        break;
                    }

                    newString = selectedRevision.Author;
                    break;

                case "sCommitter":
                    if (selectedRevision is null)
                    {
                        break;
                    }

                    newString = selectedRevision.Committer;
                    break;

                case "sAuthorDate":
                    if (selectedRevision is null)
                    {
                        break;
                    }

                    newString = selectedRevision.AuthorDate.ToString();
                    break;

                case "sCommitDate":
                    if (selectedRevision is null)
                    {
                        break;
                    }

                    newString = selectedRevision.CommitDate.ToString();
                    break;

                case "cTag":
                    if (currentTags is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(currentTags);
                    break;

                case head:
                    newString = uiCommands.Module.GetSelectedBranch(emptyIfDetached: true);
                    if (string.IsNullOrEmpty(newString))
                    {
                        newString = currentRevision.Guid;
                    }

                    break;

                case "cBranch":
                    if (currentBranches is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(currentBranches);
                    break;

                case "cLocalBranch":
                    if (currentLocalBranches is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(currentLocalBranches);
                    break;

                case "cRemoteBranch":
                    if (currentRemoteBranches is null)
                    {
                        break;
                    }

                    newString = SelectOneRef(currentRemoteBranches);
                    break;

                case "cRemoteBranchName":
                    if (currentRemoteBranches is null)
                    {
                        break;
                    }

                    newString = StripRemoteName(SelectOneRef(currentRemoteBranches));
                    break;

                case "cHash":
                    if (currentRevision is null)
                    {
                        break;
                    }

                    newString = currentRevision.Guid;
                    break;

                case "cMessage":
                    if (currentRevision is null)
                    {
                        break;
                    }

                    newString = EscapeLinefeeds(currentRevision.Body) ?? currentRevision.Subject;
                    break;

                case "cSubject":
                    if (currentRevision is null)
                    {
                        break;
                    }

                    newString = currentRevision.Subject;
                    break;

                case "cAuthor":
                    if (currentRevision is null)
                    {
                        break;
                    }

                    newString = currentRevision.Author;
                    break;

                case "cCommitter":
                    if (currentRevision is null)
                    {
                        break;
                    }

                    newString = currentRevision.Committer;
                    break;

                case "cAuthorDate":
                    if (currentRevision is null)
                    {
                        break;
                    }

                    newString = currentRevision.AuthorDate.ToString();
                    break;

                case "cCommitDate":
                    if (currentRevision is null)
                    {
                        break;
                    }

                    newString = currentRevision.CommitDate.ToString();
                    break;

                case "cDefaultRemote":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        newString = "";
                    }
                    else
                    {
                        newString = currentRemote;
                    }

                    break;

                case "cDefaultRemoteUrl":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        newString = "";
                    }
                    else
                    {
                        newString = uiCommands.Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote));
                    }

                    break;

                case "cDefaultRemotePathFromUrl":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        newString = "";
                    }
                    else
                    {
                        url = uiCommands.Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote));
                        newString = GetRemotePath(url);
                    }

                    break;

                case "RepoName":
                    newString = uiCommands.GetRequiredService<IRepositoryDescriptionProvider>().Get(uiCommands.Module.WorkingDir);
                    break;

                case "WorkingDir":
                    newString = uiCommands.Module.WorkingDir;
                    break;

                default:
                    newString = scriptOptionsProvider?.GetValue(option);
                    break;
            }

            return ReplaceOption(option, arguments, newString);

            static string? EscapeLinefeeds(string? multiLine) => multiLine?.Replace("\n", "\\n");

            string SelectOneRef(IList<IGitRef> refs) => SelectOne(refs, uiCommands, owner);

            string SelectOneString(IList<string> strings) => SelectOne(strings, uiCommands, owner);
        }

        public static string ReplaceOption(string option, string arguments, string newString)
        {
            if (newString is not null)
            {
                string newStringQuoted = QuoteRegex().Replace(newString, "\\\"");
                newStringQuoted = "\"" + newStringQuoted;
                if (newStringQuoted.EndsWith("\\"))
                {
                    newStringQuoted += "\\";
                }

                newStringQuoted += "\"";

                arguments = arguments.Replace(CreateOption(option, quoted: true), newStringQuoted);
                arguments = arguments.Replace(CreateOption(option, quoted: false), newString);

                arguments = FilterConditionals(arguments, option, true, true);
                arguments = FilterConditionals(arguments, option, false, false);
            }
            else
            {
                arguments = FilterConditionals(arguments, option, true, false);
                arguments = FilterConditionals(arguments, option, false, true);
            }

            return arguments;
        }

        private static string SelectOne(IList<IGitRef> refs, IGitUICommands uiCommands, IWin32Window owner)
        {
            return refs.Count switch
            {
                0 => string.Empty,
                1 => refs[0].Name,
                _ => AskToSpecify(refs, uiCommands, owner)
            };
        }

        private static string SelectOne(IList<string> strings, IGitUICommands uiCommands, IWin32Window owner)
        {
            return strings.Count switch
            {
                0 => string.Empty,
                1 => strings[0],
                _ => AskToSpecify(strings, uiCommands, owner)
            };
        }

        private static string StripRemoteName(string remoteBranchName)
        {
            if (string.IsNullOrEmpty(remoteBranchName))
            {
                return string.Empty;
            }

            int firstSlashIndex = remoteBranchName.IndexOf('/');
            if (firstSlashIndex >= 0)
            {
                return remoteBranchName[(firstSlashIndex + 1)..];
            }

            return remoteBranchName;
        }

        private static string FilterConditionals(string source, string option, bool positiveCondition, bool keep)
        {
            string exactMatch = positiveCondition ? ("{if:" + option + "}") : ("{ifnot:" + option + "}");
            int prevOffset = 0;
            int depth = 0;
            List<int> depthStack = new();
            StringBuilder sb = new();
            foreach (Match match in Regex.Matches(source, positiveCondition ? @"(\{if:[A-Za-z]*\})|(\{/if\})" : @"(\{ifnot:[A-Za-z]*\})|(\{/ifnot\})"))
            {
                bool write = keep || depthStack.Count <= 0;
                bool skip = false;
                if (match.Value.StartsWith("{/"))
                {
                    if (depth > 0)
                    {
                        depth--;
                        if (depthStack.Count > 0 && depthStack[depthStack.Count - 1] == depth)
                        {
                            skip = true;
                            depthStack.RemoveAt(depthStack.Count - 1);
                        }
                    }
                }
                else
                {
                    if (match.Value == exactMatch)
                    {
                        skip = true;
                        depthStack.Add(depth);
                    }

                    depth++;
                }

                if (write)
                {
                    sb.Append(source.Substring(prevOffset, match.Index + (skip ? 0 : match.Length) - prevOffset));
                }

                prevOffset = match.Index + match.Length;
            }

            if (keep || depthStack.Count <= 0)
            {
                sb.Append(source.Substring(prevOffset));
            }

            return sb.ToString();
        }

        internal static TestAccessor GetTestAccessor() => new();

        internal readonly struct TestAccessor
        {
            public string CreateOption(string option, bool quoted)
                => ScriptOptionsParser.CreateOption(option, quoted);

            public GitRevision? GetCurrentRevision(IGitModule module, List<IGitRef> currentTags,
                List<IGitRef> currentLocalBranches, List<IGitRef> currentRemoteBranches, List<IGitRef> currentBranches, bool loadBody)
                => ScriptOptionsParser.GetCurrentRevision(module, currentTags, currentLocalBranches, currentRemoteBranches, currentBranches, loadBody);

            public string? ParseScriptArguments(string arguments, string option, IWin32Window owner, IScriptOptionsProvider? scriptOptionsProvider,
                IGitUICommands uiCommands, IReadOnlyList<GitRevision> allSelectedRevisions, List<IGitRef> selectedTags, List<IGitRef> selectedBranches,
                List<IGitRef> selectedLocalBranches, List<IGitRef> selectedRemoteBranches, List<string> selectedRemotes, GitRevision selectedRevision,
                List<IGitRef> currentTags, List<IGitRef> currentBranches, List<IGitRef> currentLocalBranches, List<IGitRef> currentRemoteBranches,
                GitRevision currentRevision, string currentRemote)
                => ScriptOptionsParser.ParseScriptArguments(arguments, option, owner, scriptOptionsProvider, uiCommands, allSelectedRevisions,
                    selectedTags, selectedBranches, selectedLocalBranches, selectedRemoteBranches, selectedRemotes, selectedRevision,
                    currentTags, currentBranches, currentLocalBranches, currentRemoteBranches, currentRevision, currentRemote);
        }
    }
}
