using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using GitExtensions.Core.Commands;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Core.Utils;
using ReleaseNotesGenerator.Properties;

namespace ReleaseNotesGenerator
{
    /// <summary>
    /// Test on GE repository from "2.00" to "2.10". Should display 687 items.
    /// </summary>
    public partial class ReleaseNotesGeneratorForm : Form
    {
        private const string MostRecentHint = "most recent changes are listed on top";
        private readonly GitUIEventArgs _gitUiCommands;
        private IEnumerable<LogLine> _lastGeneratedLogLines;
        private readonly IGitLogLineParser _gitLogLineParser;

        public ReleaseNotesGeneratorForm(GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();

            Text = Strings.FormText;
            buttonCopyAsHtml.Text = Strings.ButtonCopyAsHtmlText;
            buttonCopyAsTextTableSpace.Text = Strings.ButtonCopyAsTextTableSpaceText;
            buttonCopyAsTextTableTab.Text = Strings.ButtonCopyAsTextTableTabText;
            buttonCopyOrigOutput.Text = Strings.ButtonCopyOrigOutputText;
            buttonGenerate.Text = Strings.ButtonGenerateText;
            groupBox1.Text = Strings.GroupBox1Text;
            groupBoxCopy.Text = Strings.GroupBoxCopyText;
            labelRevCount.Text = Strings.LabelRevCountText;
            label1.Text = Strings.Label1Text;
            label2.Text = Strings.Label2Text;
            label3.Text = Strings.Label3Text;
            label4.Text = Strings.Label4Text;
            label5.Text = Strings.Label5Text;
            label6.Text = Strings.Label6Text;
            label7.Text = Strings.Label7Text;
            label8.Text = Strings.Label8Text;
            label9.Text = Strings.Label9Text;
            label10.Text = Strings.Label10Text;
            label11.Text = Strings.Label11Text;

            _gitUiCommands = gitUiCommands;
            _gitLogLineParser = new GitLogLineParser();
        }

        private void ReleaseNotesGeneratorForm_Load(object sender, EventArgs e)
        {
            Icon = Owner?.Icon;
            textBoxResult_TextChanged(null, null);
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            textBoxResult.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(textBoxRevFrom.Text))
            {
                MessageBox.Show(this, Strings.FromCommitNotSpecified, Strings.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxRevFrom.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_NO_TRANSLATE_textBoxRevTo.Text))
            {
                MessageBox.Show(this, Strings.ToCommitNotSpecified, Strings.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _NO_TRANSLATE_textBoxRevTo.Focus();
                return;
            }

            var args = new GitArgumentBuilder("log")
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

            textBoxResult_TextChanged(null, null);
        }

        private void textBoxResult_TextChanged(object sender, EventArgs e)
        {
            groupBoxCopy.Enabled = _lastGeneratedLogLines != null && _lastGeneratedLogLines.Any();
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
            string headerText = string.Format(Strings.CommitLogFrom,
                textBoxRevFrom.Text, _NO_TRANSLATE_textBoxRevTo.Text, MostRecentHint);

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

            string result = headerText + Environment.NewLine + stringBuilder;
            return result;
        }

        private static string CreateHtmlTable(IEnumerable<LogLine> logLines)
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
}
