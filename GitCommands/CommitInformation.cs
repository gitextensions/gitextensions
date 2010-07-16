using System;
using System.Collections.Generic;

namespace GitCommands
{
    public class CommitInformation
    {
        /// <summary>
        /// Gets the commit info.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        static public string GetCommitInfo(string sha1)
        {
            string info = GitCommands.RunCachableCmd(Settings.GitCommand, "show -s --pretty=format:\"" + Strings.GetAutorText() + ":\t\t%aN (%aE)%n" + Strings.GetAuthorDateText() + ":\t%ar (%ad)%n" + Strings.GetCommitterText() + ":\t%cN (%cE)%n" + Strings.GetCommitterDateText() + ":\t%cr (%cd)%n" + Strings.GetCommitHashText() + ":\t%H%n%n%s%n%n%b\" " + sha1);
            if (info.Trim().StartsWith("fatal"))
                return String.Empty;

            info = RemoveRedundancies(info);

            return info;
        }

        static private string RemoveRedundancies(string info)
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
                info = RemoveField(info, Strings.GetCommitterDateText() + ":").Replace(Strings.GetAuthorDateText() + ":\t", Strings.GetDateText() + ":\t\t");
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

        static private string GetField(string data, string header)
        {
            int valueIndex = IndexOfValue(data, header);

            if (valueIndex == -1)
                return null;

            int length = LengthOfValue(data, valueIndex);
            return data.Substring(valueIndex, length);
        }

                static private int LengthOfValue(string data, int valueIndex)
        {
            if (valueIndex == -1)
                return 0;

            int endIndex = data.IndexOf('\n', valueIndex);

            if (endIndex == -1)
                endIndex = data.Length - 1;

            return endIndex - valueIndex;
        }

        static private int IndexOfValue(string data, string header)
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