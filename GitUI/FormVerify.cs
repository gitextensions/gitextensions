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
            Warnings.ContextMenu = new ContextMenu();
            Warnings.AutoGenerateColumns = false;
        }

        private LostObject CurrentItem
        {
            get { return Warnings.SelectedRows.Count == 0 ? null : filteredLostObjects[Warnings.SelectedRows[0].Index]; }
        }

        private void FormVerifyShown(object sender, EventArgs e)
        {
            UpdateLostObjects();
            Warnings.DataSource = filteredLostObjects;
        }

        private void UpdateLostObjects()
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

            UpdateFilteredLostObjects();
            Cursor.Current = Cursors.Default;
        }

        private void UpdateFilteredLostObjects()
        {
            SuspendLayout();
            filteredLostObjects.Clear();
            filteredLostObjects.AddRange(lostObjects.Where(IsMatchToFilter));
            //Warnings.DataSource = filteredLostObjects;
            ResumeLayout();
        }

        private bool IsMatchToFilter(LostObject lostObject)
        {
            if (ShowOnlyCommits.Checked)
                return lostObject.ObjectType == LostObjectType.Commit;
            return true;
        }

        private void SaveObjectsClick(object sender, EventArgs e)
        {
            var options = GetOptions();

            var process = new FormProcess("fsck-objects --lost-found" + options);
            process.ShowDialog();
            UpdateLostObjects();
        }

        private string GetOptions()
        {
            var options = string.Empty;

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
            ViewCurrentItem();
        }

        private void ViewCurrentItem()
        {
            var currenItem = CurrentItem;
            if (currenItem == null)
                return;
            new FormEdit(GitCommandHelpers.ShowSha1(currenItem.Hash)).ShowDialog();
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                _removeDanglingObjectsQuestion.Text,
                _removeDanglingObjectsCaption.Text,
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            new FormProcess("prune").ShowDialog();
            UpdateLostObjects();
        }

        private void mnuLostObjectCreateTag_Click(object sender, EventArgs e)
        {
            var currentItem = CurrentItem;
            if (currentItem == null)
                throw new InvalidOperationException();

            new FormTagSmall { Revision = new GitRevision(currentItem.Hash) }.ShowDialog();
        }

        private void mnuLostObjectView_Click(object sender, EventArgs e)
        {
            ViewCurrentItem();
        }

        private void UnreachableCheckedChanged(object sender, EventArgs e)
        {
            UpdateLostObjects();
        }

        private void TagAllObjectsClick(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            CreateLostFoundTags(false);
            UpdateLostObjects();
        }

        private void CreateLostFoundTags(bool onlyCommits)
        {
            var currentTag = 0;
            foreach (var lostObject in filteredLostObjects)
            {
                //if (onlyCommits && !warningString.Contains("commit"))
                //    continue;
                currentTag++;
                GitCommandHelpers.Tag("LOST_FOUND_" + currentTag, lostObject.Hash, false);
            }

            MessageBox.Show(string.Format(_xTagsCreated.Text, currentTag), "Tags created");
        }

        private void DeleteAllLostAndFoundTagsClick(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            UpdateLostObjects();
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
            UpdateLostObjects();
        }

        private void NoReflogsCheckedChanged(object sender, EventArgs e)
        {
            UpdateLostObjects();
        }

        private void TagAllCommitsClick(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            CreateLostFoundTags(true);
            UpdateLostObjects();
        }

        private void ShowOnlyCommitsCheckedChanged(object sender, EventArgs e)
        {
            UpdateFilteredLostObjects();
        }

        // NOTE: hack to select row under cursor on right click and context menu open
        private void Warnings_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex != -1)
            {
                Warnings.Rows[e.RowIndex].Selected = true;
            }
        }


    }
}