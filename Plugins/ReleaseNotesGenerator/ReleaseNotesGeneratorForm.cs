using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

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
            HtmlFragment.CopyToClipboard("<table><tr><td>A</td><td>B</td></tr><tr><td>C</td><td>D</td></tr></table>");
        }
    }
}
