using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Tag;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormVerify : GitExtensionsForm
    {
        private readonly TranslationString _removeDanglingObjectsCaption = new TranslationString("Remove");

        private readonly TranslationString _removeDanglingObjectsQuestion =
            new TranslationString("Are you sure you want to delete all dangling objects?");

        private readonly TranslationString _xTagsCreated =
            new TranslationString("{0} Tags created." + Environment.NewLine + Environment.NewLine +
                                  "Do not forget to delete these tags when finished.");

        private readonly List<LostObject> lostObjects = new List<LostObject>();
        private readonly SortableLostObjectsList filteredLostObjects = new SortableLostObjectsList();


        public FormVerify()
        {
            InitializeComponent();
            Translate();
            Warnings.AutoGenerateColumns = false;
        }

        private LostObject CurrentItem
        {
            get { return Warnings.CurrentRow == null ? null : filteredLostObjects[Warnings.CurrentRow.Index]; }
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

            lostObjects.Clear();
            lostObjects.AddRange(process.OutputString.ToString()
                .Split('\r', '\n')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(LostObject.TryParse)
                .Where(parsedLostObject => parsedLostObject != null));

            var warningList = new List<string>();

            foreach (var warning in process.OutputString.ToString().Split('\n', '\r'))
            {
                if (!string.IsNullOrEmpty(warning) && (!ShowOnlyCommits.Checked || warning.Contains("commit")))
                    warningList.Add(ExtendWarning(warning));
            }

            filteredLostObjects.Clear();
            filteredLostObjects.AddRange(lostObjects);
            Warnings.DataSource = filteredLostObjects;
            Cursor.Current = Cursors.Default;
        }

        private static string ExtendWarning(string warning)
        {
            var sha1 = FindSha1(warning);

            if (String.IsNullOrEmpty(sha1))
                return warning;

            var commitInfo = GitCommandHelpers.RunCmd(
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
            var currenItem = CurrentItem;
            if (currenItem == null)
                return;
            new FormEdit(GitCommandHelpers.ShowSha1(currenItem.Hash)).ShowDialog();
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
            var currentItem = CurrentItem;
            if (currentItem == null)
                throw new InvalidOperationException();

            new FormTagSmall { Revision = new GitRevision(currentItem.Hash) }.ShowDialog();
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
            foreach (var warningString in filteredLostObjects.Select(x => x.Raw))
            {
                if (onlyCommits && !warningString.Contains("commit"))
                    continue;
                var sha1 = FindSha1(warningString);
                currentTag++;
                GitCommandHelpers.Tag("LOST_FOUND_" + currentTag, sha1, false);
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
            foreach (var head in GitCommandHelpers.GetHeads(true, false))
            {
                if (head.Name.StartsWith("LOST_FOUND_"))
                    GitCommandHelpers.DeleteTag(head.Name);
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