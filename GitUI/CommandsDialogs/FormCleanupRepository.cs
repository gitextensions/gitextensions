﻿using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.Linq;

namespace GitUI.CommandsDialogs
{
    public partial class FormCleanupRepository : GitModuleForm
    {
        private readonly TranslationString _reallyCleanupQuestion =
            new TranslationString("Are you sure you want to cleanup the repository?");
        private readonly TranslationString _reallyCleanupQuestionCaption = new TranslationString("Cleanup");


        public FormCleanupRepository(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent(); Translate();
            PreviewOutput.ReadOnly = true;
            checkBoxPathFilter_CheckedChanged(null, null);
        }

        public void SetPathArgument(string path)
        {
            if (path.IsNullOrEmpty())
            {
                checkBoxPathFilter.Checked = false;
                textBoxPaths.Text = "";
            }
            else
            {
                checkBoxPathFilter.Checked = true;
                textBoxPaths.Text = path;
            }
        }

        private void Preview_Click(object sender, EventArgs e)
        {
            var cleanUpCmd = GitCommandHelpers.CleanUpCmd(true, RemoveDirectories.Checked, RemoveNonIgnored.Checked, RemoveIngnored.Checked, GetPathArgumentFromGui());
            PreviewOutput.Text = FormProcess.ReadDialog(this, cleanUpCmd);
        }

        private void Cleanup_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _reallyCleanupQuestion.Text, _reallyCleanupQuestionCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var cleanUpCmd = GitCommandHelpers.CleanUpCmd(false, RemoveDirectories.Checked, RemoveNonIgnored.Checked, RemoveIngnored.Checked, GetPathArgumentFromGui());
                PreviewOutput.Text = FormProcess.ReadDialog(this, cleanUpCmd);
            }
        }

        private string GetPathArgumentFromGui()
        {
            if (!checkBoxPathFilter.Checked)
            {
                return null;
            }
            else
            {
                // 1. get all lines from text box which are not empty
                // 2. wrap lines with ""
                // 3. join together with space as separator
                return string.Join(" ", textBoxPaths.Lines.Where(a => !a.IsNullOrEmpty()).Select(a => string.Format("\"{0}\"", a)));
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBoxPathFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool filterByPath = checkBoxPathFilter.Checked;
            textBoxPaths.Enabled = filterByPath;
            labelPathHint.Visible = filterByPath;
        }
    }
}
