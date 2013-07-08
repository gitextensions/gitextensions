using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace ReleaseNotesGenerator
{
    /// <summary>
    /// Test on GE repository from "2.00" to "2.10". Should display 687 items.
    /// </summary>
    public partial class ReleaseNotesGeneratorForm : Form
    {
        private readonly IGitPluginSettingsContainer _settings;
        private readonly GitUIBaseEventArgs _gitUiCommands;
        private IEnumerable<LogLine> _lastGeneratedLogLines;

        public ReleaseNotesGeneratorForm(IGitPluginSettingsContainer settings, GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();

            _settings = settings;
            _gitUiCommands = gitUiCommands;
            Icon = _gitUiCommands.GitUICommands.FormIcon;
        }

        private void ReleaseNotesGeneratorForm_Load(object sender, EventArgs e)
        {
            textBoxResult_TextChanged(null, null);
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            int exitCode;
            string logArgs = string.Format(textBoxGitLogArguments.Text, textBoxRevFrom.Text, textBoxRevTo.Text);
            string result = _gitUiCommands.GitModule.RunGitCmd("log " + logArgs, out exitCode);

            if (!result.Contains("\r\n"))
            {
                // if result does not contain \r\n we have to assume that line separator is only \n
                // but \r\n is needed for correctly shown in text box
                result = result.Replace("\n", "\r\n");
            }

            textBoxResult.Text = result;

            try
            {
                _lastGeneratedLogLines = CreateLogLinesFromGitOutput(textBoxResult.Lines);
                labelRevCount.Text = _lastGeneratedLogLines.Count().ToString();
            }
            catch
            {
                labelRevCount.Text = "n/a";
            }

            textBoxResult_TextChanged(null, null);
        }

        private void textBoxResult_TextChanged(object sender, EventArgs e)
        {
            groupBoxCopy.Enabled = _lastGeneratedLogLines != null && _lastGeneratedLogLines.Any();
        }

        private void buttonCopyOrigOutput_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxResult.Text);
        }

        private void buttonCopyAsPlainText_Click(object sender, EventArgs e)
        {
            string result = CreateTextTable(_lastGeneratedLogLines, true, true);
            Clipboard.SetText(result);
        }

        private void buttonCopyAsTextTableSpace_Click(object sender, EventArgs e)
        {
            string result = CreateTextTable(_lastGeneratedLogLines, true, false);
            Clipboard.SetText(result);
        }

        const string mostRecentHint = "most recent changes are listed on top";

        private void buttonCopyAsHtml_Click(object sender, EventArgs e)
        {
            string headerHtml = string.Format("<p>Commit log from '{0}' to '{1}' ({2}):</p>",
                textBoxRevFrom.Text, textBoxRevTo.Text, mostRecentHint);
            string tableHtml = CreateHtmlTable(_lastGeneratedLogLines);
            HtmlFragment.CopyToClipboard(headerHtml + tableHtml);
            ////HtmlFragment.CopyToClipboard("<table><tr><td>A</td><td>B</td></tr><tr><td>C</td><td>D</td></tr></table>");
        }

        private IEnumerable<LogLine> CreateLogLinesFromGitOutput(string[] lines)
        {
            var resultList = new List<LogLine>();

            LogLine logLine = null;
            foreach (string line in lines)
            {
                // 2e4cfb3@ (the very first line MUST start with something like this!)
                if (line.Length > 8 && line[7] == '@')
                {
                    if (logLine != null)
                    {
                        resultList.Add(logLine);
                        logLine = null;
                    }

                    logLine = new LogLine();
                    logLine.Commit = line.Substring(0, 7);
                    logLine.MessageLines.Add(line.Substring(8));
                }
                else
                {
                    logLine.MessageLines.Add(line);
                }
            }

            if (logLine != null)
            {
                resultList.Add(logLine);
                logLine = null;
            }

            return resultList;
        }

        private string CreateTextTable(IEnumerable<LogLine> logLines, bool suppressEmptyLines = true, bool separateColumnWithTabInsteadOfSpaces = true)
        {
            string headerText = string.Format("Commit log from '{0}' to '{1}' ({2}):",
                textBoxRevFrom.Text, textBoxRevTo.Text, mostRecentHint);

            string colSeparatorFirstLine = separateColumnWithTabInsteadOfSpaces ? "\t" : " ";
            string colSeparatorRestLines = separateColumnWithTabInsteadOfSpaces ? "\t" : "        ";

            var stringBuilder = new StringBuilder();

            foreach (var logLine in logLines)
            {
                string message = string.Join(Environment.NewLine + colSeparatorRestLines,
                    logLine.MessageLines.Where(
                    a => suppressEmptyLines ? !string.IsNullOrWhiteSpace(a) : true));
                stringBuilder.AppendFormat("{0}{1}{2}{3}", logLine.Commit, colSeparatorFirstLine, message, Environment.NewLine);
            }

            string result = headerText + Environment.NewLine + stringBuilder.ToString();
            return result;
        }

        private string CreateHtmlTable(IEnumerable<LogLine> logLines)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("<table>\r\n");
            foreach (var logLine in logLines)
            {
                string message = string.Join("<br/>", logLine.MessageLines.Select(a => WebUtility.HtmlEncode(a)));
                stringBuilder.AppendFormat("<tr>\r\n  <td>{0}</td>\r\n  <td>{1}</td>\r\n</tr>\r\n", logLine.Commit, message);
            }
            stringBuilder.Append("</table>");
            return stringBuilder.ToString();
        }
    }

    public class LogLine
    {
        /// <summary>
        /// one revision
        /// </summary>
        public LogLine()
        {
            MessageLines = new List<string>();
        }
        public string Commit;
        public IList<string> MessageLines;
    }
}
