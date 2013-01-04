using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using System.Web;
using System.Net;

namespace ReleaseNotesGenerator
{
    public partial class ReleaseNotesGeneratorForm : Form
    {
        private readonly IGitPluginSettingsContainer _settings;
        private readonly GitUIBaseEventArgs _gitUiCommands;

        public ReleaseNotesGeneratorForm(IGitPluginSettingsContainer settings, GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();

            _settings = settings;
            _gitUiCommands = gitUiCommands;
        }

        private void ReleaseNotesGeneratorForm_Load(object sender, EventArgs e)
        {
            textBoxResult_TextChanged(null, null);
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            int exitCode;
            string logArgs = string.Format(textBoxGitLogArguments.Text, textBoxRevFrom.Text, textBoxRevTo.Text);
            string result = _gitUiCommands.GitModule.RunGit("log " + logArgs, out exitCode);
            textBoxResult.Text = result;
        }

        private void textBoxResult_TextChanged(object sender, EventArgs e)
        {
            groupBoxCopy.Enabled = textBoxResult.Text.Length != 0;
        }

        private void buttonCopyAsPlainText_Click(object sender, EventArgs e)
        {
            textBoxResult.SelectAll();
            textBoxResult.Copy();
            textBoxResult.SelectionStart = 0;
            textBoxResult.SelectionLength = 0;
        }

        private void buttonCopyAsHtml_Click(object sender, EventArgs e)
        {
            var logLines = CreateLogLinesFromGitOutput(textBoxResult.Lines);
            string html = CreateHtmlTable(logLines);
            HtmlFragment.CopyToClipboard(html);
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

        private string CreateHtmlTable(IEnumerable<LogLine> logLines)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("<table>");
            foreach (var logLine in logLines)
            {
                string message = string.Join("<br/>", logLine.MessageLines.Select(a => WebUtility.HtmlEncode(a)));
                stringBuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", logLine.Commit, message);
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
