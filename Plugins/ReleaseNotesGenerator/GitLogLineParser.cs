using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ReleaseNotesGenerator
{
    public interface IGitLogLineParser
    {
        LogLine Parse(string line);
        IEnumerable<LogLine> Parse(IEnumerable<string> lines);
    }

    public sealed class GitLogLineParser : IGitLogLineParser
    {
        private static readonly Regex LogLineRegex = new Regex("^([a-zA-Z0-9]{1,})@(.*)", RegexOptions.Compiled);

        [CanBeNull]
        public LogLine Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var m = LogLineRegex.Match(line);
            if (!m.Success)
            {
                return null;
            }

            var logLine = new LogLine(m.Groups[1].Value, m.Groups[2].Value);
            return logLine;
        }

        public IEnumerable<LogLine> Parse(IEnumerable<string> lines)
        {
            var resultList = new List<LogLine>();
            if (lines == null)
            {
                return resultList;
            }

            LogLine loglineCurrent = null;
            foreach (string line in lines)
            {
                var logLine1 = Parse(line);
                if (logLine1 != null)
                {
                    if (loglineCurrent != null)
                    {
                        resultList.Add(loglineCurrent);
                    }

                    loglineCurrent = logLine1;
                }
                else
                {
                    loglineCurrent?.MessageLines.Add(line);
                }
            }

            if (loglineCurrent != null)
            {
                resultList.Add(loglineCurrent);
            }

            return resultList;
        }
    }
}