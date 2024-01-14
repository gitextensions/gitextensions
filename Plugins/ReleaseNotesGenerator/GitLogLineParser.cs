using System.Text.RegularExpressions;

namespace GitExtensions.Plugins.ReleaseNotesGenerator
{
    public interface IGitLogLineParser
    {
        LogLine? Parse(string line);
        IEnumerable<LogLine> Parse(IEnumerable<string>? lines);
    }

    public sealed partial class GitLogLineParser : IGitLogLineParser
    {
        [GeneratedRegex(@"^(?<before>[a-zA-Z0-9]{1,})@(?<after>.*)", RegexOptions.ExplicitCapture)]
        private static partial Regex LogLineRegex();

        public LogLine? Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            Match m = LogLineRegex().Match(line);
            if (!m.Success)
            {
                return null;
            }

            LogLine logLine = new(m.Groups["before"].Value, m.Groups["after"].Value);
            return logLine;
        }

        public IEnumerable<LogLine> Parse(IEnumerable<string>? lines)
        {
            List<LogLine> resultList = [];
            if (lines is null)
            {
                return resultList;
            }

            LogLine? logLineCurrent = null;
            foreach (string line in lines)
            {
                LogLine logLine1 = Parse(line);
                if (logLine1 is not null)
                {
                    if (logLineCurrent is not null)
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

            if (logLineCurrent is not null)
            {
                resultList.Add(logLineCurrent);
            }

            return resultList;
        }
    }
}
