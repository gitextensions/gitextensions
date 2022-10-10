﻿using System.Net;
using System.Text;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.ReleaseNotesGenerator
{
    /// <summary>
    /// Test on GE repository from "2.00" to "2.10". Should display 687 items.
    /// </summary>
    public partial class ReleaseNotesGeneratorForm : GitExtensionsFormBase
    {
        private readonly TranslationString _commitLogFrom = new("Commit log from '{0}' to '{1}' ({2}):");
        private readonly TranslationString _fromCommitNotSpecified = new("'From' commit must be specified");
        private readonly TranslationString _toCommitNotSpecified = new("'To' commit must be specified");
        private readonly TranslationString _caption = new("Invalid input");

        private const string MostRecentHint = "most recent changes are listed on top";
        private readonly GitUIEventArgs _gitUiCommands;
        private IEnumerable<LogLine> _lastGeneratedLogLines = Enumerable.Empty<LogLine>();
        private readonly IGitLogLineParser _gitLogLineParser;

        public ReleaseNotesGeneratorForm(GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();
            InitializeComplete();

            _gitUiCommands = gitUiCommands;
            _gitLogLineParser = new GitLogLineParser();
        }

        private void ReleaseNotesGeneratorForm_Load(object sender, EventArgs e)
        {
            Icon = Owner?.Icon;
            textBoxResult_TextChanged(sender, e);
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            textBoxResult.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(textBoxRevFrom.Text))
            {
                MessageBox.Show(this, _fromCommitNotSpecified.Text, _caption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxRevFrom.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_NO_TRANSLATE_textBoxRevTo.Text))
            {
                MessageBox.Show(this, _toCommitNotSpecified.Text, _caption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _NO_TRANSLATE_textBoxRevTo.Focus();
                return;
            }

            GitArgumentBuilder args = new("log")
            {
                string.Format(_NO_TRANSLATE_textBoxGitLogArguments.Text, textBoxRevFrom.Text, _NO_TRANSLATE_textBoxRevTo.Text)
            };

            string result = _gitUiCommands.GitModule.GitExecutable.GetOutput(args);

            if (EnvUtils.RunningOnWindows())
            {
                result = string.Join(Environment.NewLine, result.Split(new[] { Environment.NewLine }, StringSplitOptions.None).SelectMany(l => l.Split('\n')));
            }

            textBoxResult.Text = result;
            try
            {
                _lastGeneratedLogLines = _gitLogLineParser.Parse(textBoxResult.Lines);
                labelRevCount.Text = _lastGeneratedLogLines.Count().ToString();
            }
            catch
            {
                labelRevCount.Text = "n/a";
            }

            textBoxResult_TextChanged(sender, e);
        }

        private void textBoxResult_TextChanged(object sender, EventArgs e)
        {
            groupBoxCopy.Enabled = _lastGeneratedLogLines is not null && _lastGeneratedLogLines.Any();
        }

        private void buttonCopyOrigOutput_Click(object sender, EventArgs e)
        {
            ClipboardUtil.TrySetText(textBoxResult.Text);
        }

        private void buttonCopyAsPlainText_Click(object sender, EventArgs e)
        {
            string result = CreateTextTable(_lastGeneratedLogLines, true, true);
            ClipboardUtil.TrySetText(result);
        }

        private void buttonCopyAsTextTableSpace_Click(object sender, EventArgs e)
        {
            string result = CreateTextTable(_lastGeneratedLogLines, true, false);
            ClipboardUtil.TrySetText(result);
        }

        private void buttonCopyAsHtml_Click(object sender, EventArgs e)
        {
            string headerHtml = string.Format("<p>Commit log from '{0}' to '{1}' ({2}):</p>", textBoxRevFrom.Text, _NO_TRANSLATE_textBoxRevTo.Text, MostRecentHint);
            string tableHtml = CreateHtmlTable(_lastGeneratedLogLines);
            HtmlFragment.CopyToClipboard(headerHtml + tableHtml);
        }

        private string CreateTextTable(IEnumerable<LogLine> logLines, bool suppressEmptyLines = true, bool separateColumnWithTabInsteadOfSpaces = true)
        {
            string headerText = string.Format(_commitLogFrom.Text,
                textBoxRevFrom.Text, _NO_TRANSLATE_textBoxRevTo.Text, MostRecentHint);

            string colSeparatorFirstLine = separateColumnWithTabInsteadOfSpaces ? "\t" : " ";
            string colSeparatorRestLines = separateColumnWithTabInsteadOfSpaces ? "\t" : "        ";

            StringBuilder stringBuilder = new();

            foreach (var logLine in logLines)
            {
                string message = string.Join(Environment.NewLine + colSeparatorRestLines,
                    logLine.MessageLines.Where(
                    a => suppressEmptyLines ? !string.IsNullOrWhiteSpace(a) : true));
                stringBuilder.AppendFormat("{0}{1}{2}{3}", logLine.Commit, colSeparatorFirstLine, message, Environment.NewLine);
            }

            string result = headerText + Environment.NewLine + stringBuilder;
            return result;
        }

        private static string CreateHtmlTable(IEnumerable<LogLine> logLines)
        {
            StringBuilder stringBuilder = new();
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
}
