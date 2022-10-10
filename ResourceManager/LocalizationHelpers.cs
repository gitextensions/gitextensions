using System.Text;
using GitCommands;
using GitCommands.Git;
using GitCommands.Patches;
using GitUIPluginInterfaces;
using ResourceManager.CommitDataRenders;

namespace ResourceManager
{
    public static class LocalizationHelpers
    {
        private static readonly ICommitDataHeaderRenderer PlainCommitDataHeaderRenderer = new CommitDataHeaderRenderer(new MonospacedHeaderLabelFormatter(), new DateFormatter(), new MonospacedHeaderRenderStyleProvider(), null);

        private static DateTime RoundDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        /// <summary>
        /// Takes a date/time which and determines a friendly string for time from now to be displayed for the relative time from the date.
        /// It is important to note that times are compared using the current timezone, so the date that is passed in should be converted
        /// to the local timezone before passing it in.
        /// </summary>
        /// <param name="originDate">Current date.</param>
        /// <param name="previousDate">The date to get relative time string for.</param>
        /// <param name="displayWeeks">Indicates whether to display weeks.</param>
        /// <returns>The human readable string for relative date.</returns>
        /// <see href="http://stackoverflow.com/questions/11/how-do-i-calculate-relative-time"/>
        public static string GetRelativeDateString(DateTime originDate, DateTime previousDate, bool displayWeeks = true)
        {
            TimeSpan ts = new(RoundDateTime(originDate).Ticks - RoundDateTime(previousDate).Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60)
            {
                return TranslatedStrings.GetNSecondsAgoText(ts.Seconds);
            }

            if (delta < 45 * 60)
            {
                return TranslatedStrings.GetNMinutesAgoText(ts.Minutes);
            }

            if (delta < 24 * 60 * 60)
            {
                int hours = delta < 60 * 60 ? Math.Sign(ts.Minutes) * 1 : ts.Hours;
                return TranslatedStrings.GetNHoursAgoText(hours);
            }

            // 30.417 = 365 days / 12 months - note that the if statement only bothers with 30 days for "1 month ago" because ts.Days is int
            if (delta < (displayWeeks ? 7 : 30) * 24 * 60 * 60)
            {
                return TranslatedStrings.GetNDaysAgoText(ts.Days);
            }

            if (displayWeeks && delta < 30 * 24 * 60 * 60)
            {
                int weeks = Convert.ToInt32(ts.Days / 7.0);
                return TranslatedStrings.GetNWeeksAgoText(weeks);
            }

            if (delta < 365 * 24 * 60 * 60)
            {
                int months = Convert.ToInt32(ts.Days / 30.0);
                return TranslatedStrings.GetNMonthsAgoText(months);
            }

            int years = Convert.ToInt32(ts.Days / 365.0);
            return TranslatedStrings.GetNYearsAgoText(years);
        }

        public static string GetFullDateString(DateTimeOffset datetime)
        {
            // previous format "ddd MMM dd HH':'mm':'ss yyyy"
            return datetime.LocalDateTime.ToString("G");
        }

        public static string GetSubmoduleText(GitModule superproject, string name, string hash, bool cache)
        {
            StringBuilder sb = new();
            sb.AppendLine("Submodule " + name);
            sb.AppendLine();
            GitModule module = superproject.GetSubmodule(name);

            // Submodule directory must exist to run commands, unknown otherwise
            if (module.IsValidGitWorkingDir())
            {
                // TEMP, will be moved in the follow up refactor
                ICommitDataManager commitDataManager = new CommitDataManager(() => module);

                CommitData? data = commitDataManager.GetCommitData(hash, out _, cache);
                if (data is null)
                {
                    sb.AppendLine("Commit hash:\t" + hash);
                    return sb.ToString();
                }

                string header = PlainCommitDataHeaderRenderer.RenderPlain(data);
                string body = "\n" + data.Body.Trim();
                sb.AppendLine(header);
                sb.Append(body);
            }
            else
            {
                sb.AppendLine("Commit hash:\t" + hash);
            }

            return sb.ToString();
        }

        public static string ProcessSubmodulePatch(GitModule module, string fileName, Patch? patch)
        {
            var status = SubmoduleHelpers.ParseSubmoduleStatus(patch?.Text, module, fileName);
            if (status is null)
            {
                return "";
            }

            return ProcessSubmoduleStatus(module, status);
        }

        public static string ProcessSubmoduleStatus(GitModule module, GitSubmoduleStatus status, bool moduleIsParent = true, bool limitOutput = false)
        {
            if (module is null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            if (status is null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            GitModule gitModule = moduleIsParent ? module.GetSubmodule(status.Name) : module;
            StringBuilder sb = new();
            sb.AppendLine("Submodule " + status.Name + " Change");

            // TEMP, will be moved in the follow up refactor
            ICommitDataManager commitDataManager = new CommitDataManager(() => gitModule);

            CommitData? oldCommitData = null;
            if (status.OldCommit != status.Commit)
            {
                sb.AppendLine();
                sb.AppendLine("From:\t" + (status.OldCommit?.ToString() ?? "null"));

                // Submodule directory must exist to run commands, unknown otherwise
                if (gitModule.IsValidGitWorkingDir())
                {
                    if (status.OldCommit is not null)
                    {
                        oldCommitData = commitDataManager.GetCommitData(status.OldCommit.ToString(), out _, cache: true);
                    }

                    if (oldCommitData is not null)
                    {
                        sb.AppendLine("\t\t\t\t\t" + GetRelativeDateString(DateTime.UtcNow, oldCommitData.CommitDate.UtcDateTime) + " (" +
                                      GetFullDateString(oldCommitData.CommitDate) + ")");
                        var delimiter = new[] { '\n', '\r' };
                        var lines = oldCommitData.Body.Trim(delimiter).Split(new[] { "\r\n" }, StringSplitOptions.None);
                        foreach (var line in lines)
                        {
                            sb.AppendLine("\t\t" + line);
                        }
                    }
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            string dirty = !status.IsDirty ? "" : " (dirty)";
            sb.Append(status.OldCommit != status.Commit ? "To:\t" : "Commit:\t");
            sb.AppendLine((status.Commit?.ToString() ?? "null") + dirty);
            CommitData? commitData = null;

            // Submodule directory must exist to run commands, unknown otherwise
            if (gitModule.IsValidGitWorkingDir())
            {
                if (status.Commit is not null)
                {
                    commitData = commitDataManager.GetCommitData(status.Commit.ToString(), out _, cache: true);
                }

                if (commitData is not null)
                {
                    sb.AppendLine("\t\t\t\t\t" + GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) + " (" +
                                  GetFullDateString(commitData.CommitDate) + ")");
                    var delimiter = new[] { '\n', '\r' };
                    var lines = commitData.Body.Trim(delimiter).Split(new[] { "\r\n" }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        sb.AppendLine("\t\t" + line);
                    }
                }

                if (status.OldCommit == status.Commit)
                {
                    oldCommitData = commitData;
                }
            }
            else
            {
                sb.AppendLine();
            }

            sb.AppendLine();
            var submoduleStatus = gitModule.CheckSubmoduleStatus(status.Commit, status.OldCommit, commitData, oldCommitData);
            sb.Append("Type: ");
            switch (submoduleStatus)
            {
                case SubmoduleStatus.NewSubmodule:
                    sb.AppendLine("New submodule");
                    break;
                case SubmoduleStatus.FastForward:
                    sb.AppendLine("Fast Forward");
                    break;
                case SubmoduleStatus.Rewind:
                    sb.AppendLine("Rewind");
                    break;
                case SubmoduleStatus.NewerTime:
                    sb.AppendLine("Newer commit time");
                    break;
                case SubmoduleStatus.OlderTime:
                    sb.AppendLine("Older commit time");
                    break;
                case SubmoduleStatus.SameTime:
                    sb.AppendLine("Same commit time");
                    break;
                default:
                    sb.AppendLine(status.IsDirty ? "Dirty" : "Unknown");

                    break;
            }

            if (status.AddedCommits is not null && status.RemovedCommits is not null &&
                (status.AddedCommits != 0 || status.RemovedCommits != 0))
            {
                sb.Append("\nCommits: ");

                if (status.RemovedCommits > 0)
                {
                    sb.Append(status.RemovedCommits + " removed");

                    if (status.AddedCommits > 0)
                    {
                        sb.Append(", ");
                    }
                }

                if (status.AddedCommits > 0)
                {
                    sb.Append(status.AddedCommits + " added");
                }

                sb.AppendLine();
            }

            if (status.Commit is not null && status.OldCommit is not null)
            {
                const int maxLimitedLines = 5;
                if (status.IsDirty)
                {
                    string statusText = gitModule.GetStatusText(untracked: false);
                    if (!string.IsNullOrEmpty(statusText))
                    {
                        sb.AppendLine("\nStatus:");
                        if (limitOutput)
                        {
                            var txt = statusText.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
                            if (txt.Length > maxLimitedLines)
                            {
                                statusText = new List<string>(txt).Take(maxLimitedLines).Join(Environment.NewLine) +
                                    $"{Environment.NewLine} {txt.Length - maxLimitedLines} more changes";
                            }
                        }

                        sb.Append(statusText);
                    }
                }

                if (gitModule.IsValidGitWorkingDir())
                {
                    ExecutionResult exec = gitModule.GetDiffFiles(status.OldCommit.ToString(), status.Commit.ToString(), nullSeparated: false);
                    if (exec.ExitedSuccessfully)
                    {
                        string diffs = exec.StandardOutput;
                        if (!string.IsNullOrEmpty(diffs))
                        {
                            sb.AppendLine("\nDifferences:");
                            if (limitOutput)
                            {
                                var txt = diffs.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
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
            }

            return sb.ToString();
        }
    }
}
