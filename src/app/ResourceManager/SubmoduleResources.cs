using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using ResourceManager.CommitDataRenders;

namespace ResourceManager;

public static partial class SubmoduleResources
{
    private static readonly ICommitDataHeaderRenderer PlainCommitDataHeaderRenderer = new CommitDataHeaderRenderer(new MonospacedHeaderLabelFormatter(), new DateFormatter(), new MonospacedHeaderRenderStyleProvider(), null);
    [GeneratedRegex(@"^(\s*\S+)\s+", RegexOptions.Multiline)]
    private static partial Regex ReplaceTrailingSpacesWithTabRegex();

    public static string GetSubmoduleText(IGitModule superproject, string name, string hash)
    {
        StringBuilder sb = new();
        sb.AppendLine("Submodule " + name);
        sb.AppendLine();

        // Submodule directory must exist to run commands, unknown otherwise
        IGitModule module = superproject.GetSubmodule(name);
        if (module.IsValidGitWorkingDir()
            && new CommitDataManager(() => module).GetCommitData(hash) is CommitData data)
        {
            // Get body without Notes, to cache the command
            string header = PlainCommitDataHeaderRenderer.RenderPlain(data);
            string body = data.Body.Trim();
            sb.AppendLine(header);
            sb.AppendLine();
            sb.Append(body);
        }
        else if (!string.IsNullOrWhiteSpace(hash))
        {
            sb.AppendLine("Commit hash:\t" + hash);
        }
        else
        {
            sb.AppendLine("Invalid git directory and commit hash");
        }

        return sb.ToString();
    }

    public static string GetSubmoduleStatusText(IGitModule module, GitSubmoduleStatus status, bool moduleIsParent = true, bool limitOutput = false)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(status);

        IGitModule gitModule = moduleIsParent ? module.GetSubmodule(status.Name) : module;
        StringBuilder sb = new();
        sb.AppendLine($"Submodule {status.Name}");

        // TEMP, will be moved in the follow up refactor
        ICommitDataManager commitDataManager = new CommitDataManager(() => gitModule);

        CommitData? oldCommitData = null;
        if (status.OldCommit != status.Commit && status.OldCommit is not null)
        {
            sb.AppendLine();
            sb.Append("From:\t");
            sb.AppendLine(status.OldCommit?.ToString() ?? "null");

            // Submodule directory must exist to run commands, unknown otherwise
            if (gitModule.IsValidGitWorkingDir()
                && commitDataManager.GetCommitData(status.OldCommit.ToString()) is CommitData c)
            {
                oldCommitData = c;

                sb.AppendLine("\t\t\t" + LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, oldCommitData.CommitDate.UtcDateTime) + " (" +
                              LocalizationHelpers.GetFullDateString(oldCommitData.CommitDate) + ")");
                foreach (string line in oldCommitData.Body.Replace("\r\n", "\n").Split(Delimiters.LineFeed, StringSplitOptions.None))
                {
                    sb.AppendLine("\t\t" + line);
                }
            }
        }

        CommitData? commitData = null;
        if (status.Commit is not null)
        {
            sb.AppendLine();
            string dirty = !status.IsDirty ? "" : " (dirty)";

            // Note: add spaces to get "To" aligned the same as From
            sb.Append(status.OldCommit != status.Commit ? "To:  \t" : "Commit:\t");
            sb.AppendLine((status.Commit?.ToString() ?? "null") + dirty);

            // Submodule directory must exist to run commands, unknown otherwise
            if (gitModule.IsValidGitWorkingDir()
                && commitDataManager.GetCommitData(status.Commit.ToString()) is CommitData c)
            {
                commitData = c;
                sb.AppendLine("\t\t\t" + LocalizationHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) + " (" +
                              LocalizationHelpers.GetFullDateString(commitData.CommitDate) + ")");
                foreach (string line in commitData.Body.Replace("\r\n", "\n").LazySplit(Delimiters.LineFeed))
                {
                    sb.AppendLine("\t\t" + line);
                }
            }

            if (status.OldCommit == status.Commit)
            {
                oldCommitData = commitData;
            }
        }

        sb.AppendLine();
        sb.Append("Type: ");
        switch (status.Status)
        {
            case SubmoduleStatus.NewSubmodule:
                sb.Append("New submodule");
                break;
            case SubmoduleStatus.RemovedSubmodule:
                sb.AppendLine("Removed submodule");
                break;
            case SubmoduleStatus.FastForward:
                sb.Append("Fast Forward");
                break;
            case SubmoduleStatus.Rewind:
                sb.Append("Rewind");
                break;
            case SubmoduleStatus.NewerTime:
                sb.Append("Newer commit time");
                break;
            case SubmoduleStatus.OlderTime:
                sb.Append("Older commit time");
                break;
            case SubmoduleStatus.SameTime:
                sb.Append("Same commit");
                break;
            case SubmoduleStatus.Unknown:
            default:
                sb.Append("Unknown");

                break;
        }

        sb.AppendLine(status.IsDirty ? " Dirty" : "");

        if (AddedAndRemovedStringLong(status) is string addedRemoved
            && !string.IsNullOrWhiteSpace(addedRemoved))
        {
            sb.AppendLine();
            sb.AppendLine($"Commits: {addedRemoved}");
        }

        if (status.Commit is not null && status.OldCommit is not null && gitModule.IsValidGitWorkingDir())
        {
            const int maxLimitedLines = 5;
            if (status.IsDirty)
            {
                string statusText = gitModule.GetStatusText(untracked: false);
                if (!string.IsNullOrEmpty(statusText))
                {
                    sb.AppendLine();
                    sb.AppendLine("Status:");
                    if (limitOutput)
                    {
                        string[] txt = statusText.Replace("\r\n", "\n").Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
                        if (txt.Length > maxLimitedLines)
                        {
                            statusText = new List<string>(txt).Take(maxLimitedLines).Join(Environment.NewLine) +
                                $"{Environment.NewLine} {txt.Length - maxLimitedLines} more changes";
                        }
                    }

                    // format similar to Differences:
                    sb.Append(ReplaceTrailingSpacesWithTab(statusText));
                }
            }

            ExecutionResult exec = gitModule.GetDiffFiles(status.OldCommit.ToString(), status.Commit.ToString(), noCache: false, rawParsable: false, cancellationToken: default);
            if (exec.ExitedSuccessfully)
            {
                string diffs = exec.StandardOutput;
                if (!string.IsNullOrEmpty(diffs))
                {
                    sb.AppendLine();
                    sb.AppendLine("Differences:");
                    if (limitOutput)
                    {
                        string[] txt = diffs.Replace("\r\n", "\n").Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
                        if (txt.Length > maxLimitedLines)
                        {
                            diffs = new List<string>(txt).Take(maxLimitedLines).Join(Environment.NewLine) +
                                $"{Environment.NewLine} {txt.Length - maxLimitedLines} more differences";
                        }
                    }

                    sb.Append(diffs);
                }
            }
        }

        return sb.ToString();

        static string AddedAndRemovedStringLong(GitSubmoduleStatus status)
        {
            if (status.RemovedCommits is null || status.AddedCommits is null ||
                (status.RemovedCommits == 0 && status.AddedCommits == 0))
            {
                return "";
            }

            return $"{status.AddedCommits} added, {status.RemovedCommits} removed";
        }

        static string ReplaceTrailingSpacesWithTab(string input)
            => ReplaceTrailingSpacesWithTabRegex().Replace(input, "$1\t");
    }
}
