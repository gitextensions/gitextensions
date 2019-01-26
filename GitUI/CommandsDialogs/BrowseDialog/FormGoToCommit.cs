using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormGoToCommit : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormGoToCommit()
        {
            InitializeComponent();
        }

        public FormGoToCommit(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        protected override void OnLoad(EventArgs e)
        {
            SetCommitExpressionFromClipboard();
            base.OnLoad(e);
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

            // scale up for hi DPI
            MinimumSize = DpiUtil.Scale(new Size(500, 255));
            Size = MinimumSize;
        }

        /// <summary>
        /// returns null if revision does not exist (could not be rev-parsed)
        /// </summary>
        [CanBeNull]
        public ObjectId ValidateAndGetSelectedRevision()
        {
            return Module.RevParse(textboxCommitExpression.Text.Trim());
        }

        private void commitExpression_TextChanged(object sender, EventArgs e)
        {
            goButton.Enabled = !string.IsNullOrWhiteSpace(textboxCommitExpression.Text.Trim());
        }

        private void Go()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            Go();
        }

        private void linkGitRevParse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://git-scm.com/docs/git-rev-parse#_specifying_revisions");
        }

        private void SetCommitExpressionFromClipboard()
        {
            string text = Clipboard.GetText().Trim();
            if (text.IsNullOrEmpty())
            {
                return;
            }

            var guid = Module.RevParse(text);
            if (guid != null)
            {
                textboxCommitExpression.Text = text;
                textboxCommitExpression.SelectAll();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
