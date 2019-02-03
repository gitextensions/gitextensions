using System;
using System.Text;
using GitCommands;
using GitCommands.Git;
using GitCommands.Patches;
using JetBrains.Annotations;
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
            var ts = new TimeSpan(RoundDateTime(originDate).Ticks - RoundDateTime(previousDate).Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60)
            {
                return Strings.GetNSecondsAgoText(ts.Seconds);
            }

            if (delta < 45 * 60)
            {
                return Strings.GetNMinutesAgoText(ts.Minutes);
            }

            if (delta < 24 * 60 * 60)
            {
                int hours = delta < 60 * 60 ? Math.Sign(ts.Minutes) * 1 : ts.Hours;
                return Strings.GetNHoursAgoText(hours);
            }

            // 30.417 = 365 days / 12 months - note that the if statement only bothers with 30 days for "1 month ago" because ts.Days is int
            if (delta < (displayWeeks ? 7 : 30) * 24 * 60 * 60)
            {
                return Strings.GetNDaysAgoText(ts.Days);
            }

            if (displayWeeks && delta < 30 * 24 * 60 * 60)
            {
                int weeks = Convert.ToInt32(ts.Days / 7.0);
                return Strings.GetNWeeksAgoText(weeks);
            }

            if (delta < 365 * 24 * 60 * 60)
            {
                int months = Convert.ToInt32(ts.Days / 30.0);
                return Strings.GetNMonthsAgoText(months);
            }

            int years = Convert.ToInt32(ts.Days / 365.0);
            return Strings.GetNYearsAgoText(years);
        }

        public static string GetFullDateString(DateTimeOffset datetime)
        {
            // previous format "ddd MMM dd HH':'mm':'ss yyyy"
            return datetime.LocalDateTime.ToString("G");
        }

        public static string GetSubmoduleText(GitModule superproject, string name, string hash)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Submodule " + name);
            sb.AppendLine();
            GitModule module = superproject.GetSubmodule(name);
            if (module.IsValidGitWorkingDir())
            {
                // TEMP, will be moved in the follow up refactor
                ICommitDataManager commitDataManager = new CommitDataManager(() => module);

                CommitData data = commitDataManager.GetCommitData(hash, out _);
                if (data == null)
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

        public static string ProcessSubmodulePatch(GitModule module, string fileName, Patch patch)
        {
            string text = patch?.Text;
            var status = GitCommandHelpers.ParseSubmoduleStatus(text, module, fileName);
            if (status == null)
            {
                return "";
            }

            return ProcessSubmoduleStatus(module, status);
        }

        public static string ProcessSubmoduleStatus([NotNull] GitModule module, [NotNull] GitSubmoduleStatus status)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            GitModule gitModule = module.GetSubmodule(status.Name);
            var sb = new StringBuilder();
            sb.AppendLine("Submodule " + status.Name + " Change");

            // TEMP, will be moved in the follow up refactor
            ICommitDataManager commitDataManager = new CommitDataManager(() => gitModule);

            sb.AppendLine();
            sb.AppendLine("From:\t" + (status.OldCommit?.ToString() ?? "null"));
            CommitData oldCommitData = null;
            if (gitModule.IsValidGitWorkingDir())
            {
                if (status.OldCommit != null)
                {
                    oldCommitData = commitDataManager.GetCommitData(status.OldCommit.ToString(), out _);
                }

                if (oldCommitData != null)
                {
                    sb.AppendLine("\t\t\t\t\t" + GetRelativeDateString(DateTime.UtcNow, oldCommitData.CommitDate.UtcDateTime) + " (" + GetFullDateString(oldCommitData.CommitDate) + ")");
                    var delimiter = new[] { '\n', '\r' };
                    var lines = oldCommitData.Body.Trim(delimiter).Split(new[] { "\r\n" }, 0);
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

            sb.AppendLine();
            string dirty = !status.IsDirty ? "" : " (dirty)";
            sb.AppendLine("To:\t\t" + (status.Commit?.ToString() ?? "null") + dirty);
            CommitData commitData = null;
            if (gitModule.IsValidGitWorkingDir())
            {
                if (status.Commit != null)
                {
                    commitData = commitDataManager.GetCommitData(status.Commit.ToString(), out _);
                }

                if (commitData != null)
                {
                    sb.AppendLine("\t\t\t\t\t" + GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) + " (" + GetFullDateString(commitData.CommitDate) + ")");
                    var delimiter = new[] { '\n', '\r' };
                    var lines = commitData.Body.Trim(delimiter).Split(new[] { "\r\n" }, 0);
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
                    sb.AppendLine("Unknown");
                    break;
            }

            if (status.AddedCommits != null && status.RemovedCommits != null &&
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

            if (status.Commit != null && status.OldCommit != null)
            {
                if (status.IsDirty)
                {
                    string statusText = gitModule.GetStatusText(untracked: false);
                    if (!string.IsNullOrEmpty(statusText))
                    {
                        sb.AppendLine("\nStatus:");
                        sb.Append(statusText);
                    }
                }

                string diffs = gitModule.GetDiffFilesText(status.OldCommit.ToString(), status.Commit.ToString());
                if (!string.IsNullOrEmpty(diffs))
                {
                    sb.AppendLine("\nDifferences:");
                    sb.Append(diffs);
                }
            }

            return sb.ToString();
        }
    }
}