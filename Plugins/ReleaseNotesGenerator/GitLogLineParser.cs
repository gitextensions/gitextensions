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

            LogLine logLineCurrent = null;
            foreach (string line in lines)
            {
                var logLine1 = Parse(line);
                if (logLine1 != null)
                {
                    if (logLineCurrent != null)
                    {
                        resultList.Add(logLineCurrent);
                    }

                    logLineCurrent = logLine1;
                }
                else
                {
                    logLineCurrent?.MessageLines.Add(line);
                }
            }

            if (logLineCurrent != null)
            {
                resultList.Add(logLineCurrent);
            }

            return resultList;
        }
    }
}