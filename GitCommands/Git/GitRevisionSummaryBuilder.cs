using System;
using System.Text;

namespace GitCommands
{
    public interface IGitRevisionSummaryBuilder
    {
        string BuildSummary(string body);
    }

    public class GitRevisionSummaryBuilder : IGitRevisionSummaryBuilder
    {
        private const int CommitSummaryMaxLineLength = 150;
        private const int CommitSummaryMaxNumberOfLines = 30;

        private const int LineEllipsisLength = 7;
        private const int CommitSummaryEllipsisLength = 5;

        // Maximum size of the commit summary with ellipsis strings on each lines and at the end
        private const int CommitSummaryWorstCaseLength = (CommitSummaryMaxNumberOfLines * (CommitSummaryMaxLineLength + LineEllipsisLength)) + CommitSummaryEllipsisLength;

        public string BuildSummary(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            var s = new StringBuilder(Math.Min(body.Length, CommitSummaryWorstCaseLength));

            int lineCount = 0;
            int lineStartPos = 0;
            int pos;
            for (pos = 0; pos < body.Length; ++pos)
            {
                if (body[pos] == '\n')
                {
                    if (pos - lineStartPos > CommitSummaryMaxLineLength)
                    {
                        AppendLine(CommitSummaryMaxLineLength, withEllipsis: true);
                    }
                    else
                    {
                        AppendLine(pos - lineStartPos);
                    }

                    lineStartPos = pos + 1;

                    if (++lineCount == CommitSummaryMaxNumberOfLines)
                    {
                        return s.Append("[...]").ToString();
                    }
                }

                if (pos == body.Length - 1)
                {
                    if (pos - lineStartPos > CommitSummaryMaxLineLength)
                    {
                        AppendLine(CommitSummaryMaxLineLength, withEllipsis: true);
                    }
                    else
                    {
                        AppendLine(pos - lineStartPos + 1);
                    }
                }

                void AppendLine(int length, bool withEllipsis = false)
                {
                    s.Append(body.Substring(lineStartPos, length));
                    if (withEllipsis)
                    {
                        s.Append(" [...]");
                    }

                    s.Append("\n");
                }
            }

            return s.ToString().TrimEnd('\n');
        }
    }
}
