using System;
using System.Collections.Generic;

namespace GitCommands
{
    public class CommitInformation
    {
        /// <summary>
        /// Gets all branches which contain the given commit.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllBranchesWhichContainGivenCommit(string sha1)
        {
            string info = GitCommands.RunCmd(Settings.GitCommand, "branch --contains " + sha1);


            if (info.Trim().StartsWith("fatal"))
                return new List<string>();
            return info.Split(new[] {'\r', '\n', '*', ' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets the commit info.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public static string GetCommitInfo(string sha1)
        {
            string info = GitCommands.RunCachableCmd(
                Settings.GitCommand,
                string.Format(
                    "show -s --pretty=format:\"{0}:\t\t%aN (%aE)%n{1}:\t%ar (%ad)%n{2}:\t%cN (%cE)%n{3}:\t%cr (%cd)%n{4}:\t%H%n%n%s%n%n%b\" {5}",
                    Strings.GetAutorText(),
                    Strings.GetAuthorDateText(),
                    Strings.GetCommitterText(),
                    Strings.GetCommitterDateText(),
                    Strings.GetCommitHashText(), sha1));

            if (info.Trim().StartsWith("fatal"))
                return String.Empty;

            info = RemoveRedundancies(info);

            return info;
        }

        private static string RemoveRedundancies(string info)
        {
            string author = GetField(info, Strings.GetAutorText() + ":");
            string committer = GetField(info, Strings.GetCommitterText() + ":");

            if (String.Equals(author, committer, StringComparison.CurrentCulture))
            {
                info = RemoveField(info, Strings.GetCommitterText() + ":");
            }

            string authorDate = GetField(info, Strings.GetAuthorDateText() + ":");
            string commitDate = GetField(info, Strings.GetCommitterDateText() + ":");

            if (String.Equals(authorDate, commitDate, StringComparison.CurrentCulture))
            {
                info =
                    RemoveField(info, Strings.GetCommitterDateText() + ":").Replace(
                        Strings.GetAuthorDateText() + ":\t", Strings.GetDateText() + ":\t\t");
            }

            return info;
        }

        private static string RemoveField(string data, string header)
        {
            int headerIndex = data.IndexOf(header);

            if (headerIndex == -1)
                return data;

            int endIndex = data.IndexOf('\n', headerIndex);

            if (endIndex == -1)
                endIndex = data.Length - 1;

            int length = endIndex - headerIndex + 1;

            return data.Remove(headerIndex, length);
        }

        private static string GetField(string data, string header)
        {
            int valueIndex = IndexOfValue(data, header);

            if (valueIndex == -1)
                return null;

            int length = LengthOfValue(data, valueIndex);
            return data.Substring(valueIndex, length);
        }

        private static int LengthOfValue(string data, int valueIndex)
        {
            if (valueIndex == -1)
                return 0;

            int endIndex = data.IndexOf('\n', valueIndex);

            if (endIndex == -1)
                endIndex = data.Length - 1;

            return endIndex - valueIndex;
        }

        private static int IndexOfValue(string data, string header)
        {
            int headerIndex = data.IndexOf(header);

            if (headerIndex == -1)
                return -1;

            int valueIndex = headerIndex + header.Length;

            while (data[valueIndex] == '\t')
            {
                valueIndex++;

                if (valueIndex == data.Length)
                    return -1;
            }

            return valueIndex;
        }
    }
}