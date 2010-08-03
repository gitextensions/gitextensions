using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitUI.Tag;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormVerify : GitExtensionsForm
    {
        private readonly TranslationString _removeDanglingObjectsCaption = new TranslationString("Remove");

        private readonly TranslationString _removeDanglingObjectsQuestion =
            new TranslationString("Are you sure you want to delete all dangling objects?");

        private readonly TranslationString _xTagsCreated =
            new TranslationString("{0} Tags created." + Environment.NewLine + Environment.NewLine +
                                  "Do not forget to delete these tags when finished.");


        public FormVerify()
        {
            InitializeComponent();
            Translate();
        }

        private void FormVerifyShown(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void LoadLostObjects()
        {
            Cursor.Current = Cursors.WaitCursor;

            var options = GetOptions();

            var process = new FormProcess("fsck-objects" + options);
            process.ShowDialog();

            var warningList = new List<string>();

            foreach (var warning in process.OutputString.ToString().Split('\n'))
            {
                if (!ShowOnlyCommits.Checked || warning.Contains("commit"))
                    warningList.Add(ExtendWarning(warning));
            }

            Warnings.DataSource = warningList;
        }

        private static string ExtendWarning(string warning)
        {
            var sha1 = FindSha1(warning);

            if (String.IsNullOrEmpty(sha1))
                return warning;

            var commitInfo = GitCommands.GitCommands.RunCmd(
                Settings.GitCommand,
                "log -n1 --pretty=format:\"%aN, %s, %cd\" " +
                FindSha1(warning));

            if (String.IsNullOrEmpty(commitInfo))
                return warning;

            return warning + " -> " + commitInfo;
        }

        private void SaveObjectsClick(object sender, EventArgs e)
        {
            var options = GetOptions();

            var process = new FormProcess("fsck-objects --lost-found" + options);
            process.ShowDialog();
            FormVerifyShown(null, null);
        }

        private string GetOptions()
        {
            var options = "";

            if (Unreachable.Checked)
                options += " --unreachable";

            if (FullCheck.Checked)
                options += " --full";

            if (NoReflogs.Checked)
                options += " --no-reflogs";
            return options;
        }

        private void WarningsDoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var sha1 = FindSha1(Warnings.SelectedValue as string);
            if (!string.IsNullOrEmpty(sha1))
            {
                new FormEdit(GitCommands.GitCommands.ShowSha1(sha1)).ShowDialog();
            }
        }

        private static string FindSha1(string warningString)
        {
            foreach (var sha1 in warningString.Split(' '))
            {
                if (sha1.Trim().Length == 40)
                {
                    return sha1.Trim();
                }
            }

            return "";
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                _removeDanglingObjectsQuestion.Text,
                _removeDanglingObjectsCaption.Text,
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            new FormProcess("prune").ShowDialog();
            FormVerifyShown(null, null);
        }


        private void TagSelectedObjectClick(object sender, EventArgs e)
        {
            var sha1 = FindSha1(Warnings.SelectedValue as string);
            if (string.IsNullOrEmpty(sha1)) return;
            var form =
                new FormTagSmall
                    {
                        Revision = new GitRevision {Guid = sha1}
                    };
            form.ShowDialog();
        }

        private void ViewObjectClick(object sender, EventArgs e)
        {
            WarningsDoubleClick(null, null);
        }

        private void UnreachableCheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void TagAllObjectsClick(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            CreateLostFoundTags(false);
            LoadLostObjects();
        }

        private void CreateLostFoundTags(bool onlyCommits)
        {
            var currentTag = 0;
            foreach (var warningString in (List<string>) Warnings.DataSource)
            {
                if (onlyCommits && !warningString.Contains("commit")) 
                    continue;
                var sha1 = FindSha1(warningString);
                currentTag++;
                GitCommands.GitCommands.Tag("LOST_FOUND_" + currentTag, sha1, false);
            }

            MessageBox.Show(string.Format(_xTagsCreated.Text, currentTag), "Tags created");
        }

        private void DeleteAllLostAndFoundTagsClick(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            LoadLostObjects();
        }

        private static void DeleteLostFoundTags()
        {
            foreach (var head in GitCommands.GitCommands.GetHeads(true, false))
            {
                if (head.Name.StartsWith("LOST_FOUND_"))
                    GitCommands.GitCommands.DeleteTag(head.Name);
            }
        }

        private void FullCheckCheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void NoReflogsCheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void TagAllCommitsClick(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            CreateLostFoundTags(true);
            LoadLostObjects();
        }

        private void ShowOnlyCommitsCheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }
    }
}