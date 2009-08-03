using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormVerify : GitExtensionsForm
    {
        public FormVerify()
        {
            InitializeComponent();
        }

        private void FormVerify_Shown(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void LoadLostObjects()
        {
            Cursor.Current = Cursors.WaitCursor;
            
            string options = "";

            if (Unreachable.Checked)
                options += " --unreachable";
            
            if (FullCheck.Checked)
                options += " --full";

            if (NoReflogs.Checked)
                options += " --no-reflogs";
                
            FormProcess process = new FormProcess("fsck-objects" + options);

            List<string> warningList = new List<string>();

            foreach (string warning in process.outputString.ToString().Split('\n'))
            {
                if (!ShowOnlyCommits.Checked || warning.Contains("commit"))
                    warningList.Add(ExtendWarning(warning));
            }

            Warnings.DataSource = warningList;
        }

        private string ExtendWarning(string warning)
        {
            string sha1 = FindSha1(warning);

            if (String.IsNullOrEmpty(sha1))
                return warning;

            string commitInfo = GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "log -n1 --pretty=format:\"%aN, %s, %cd\" " + FindSha1(warning));

            if (String.IsNullOrEmpty(commitInfo))
                return warning;

            return warning + " -> " + commitInfo;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string options = "";

            if (Unreachable.Checked)
                options += " --unreachable";

            if (FullCheck.Checked)
                options += " --full";

            if (NoReflogs.Checked)
                options += " --no-reflogs";

            FormProcess process = new FormProcess("fsck-objects --lost-found" + options);
            FormVerify_Shown(null, null);
        }

        private void Warnings_DoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string sha1 = FindSha1(Warnings.SelectedValue as string);
            if (!string.IsNullOrEmpty(sha1))
            {
                new FormEdit(GitCommands.GitCommands.ShowSha1(sha1)).ShowDialog();
            }
        }

        private string FindSha1(string warningString)
        {
            foreach (string sha1 in warningString.Split(' '))
            {
                if (sha1.Trim().Length == 40)
                {
                    return sha1.Trim();
                }
            }

            return "";
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all dangling objects?", "Remove", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                new FormProcess("prune");
                FormVerify_Shown(null, null);
            }
        }

        private void FormVerify_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sha1 = FindSha1(Warnings.SelectedValue as string);
            if (!string.IsNullOrEmpty(sha1))
            {
                FormTagSmall form = new FormTagSmall();
                form.Revision = new GitRevision();
                form.Revision.Guid = sha1;
                form.ShowDialog();
            }
        }

        private void ViewObject_Click(object sender, EventArgs e)
        {
            Warnings_DoubleClick(null, null);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void TagAllObjects_Click(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            CreateLostFoundTags(false);
            LoadLostObjects();
        }

        private void CreateLostFoundTags(bool onlyCommits)
        {
            int currentTag = 0;
            foreach (string warningString in Warnings.DataSource as List<string>)
            {
                if (!onlyCommits || warningString.Contains("commit"))
                {
                    string sha1 = FindSha1(warningString);
                    currentTag++;
                    GitCommands.GitCommands.Tag("LOST_FOUND_" + currentTag, sha1);
                }
            }

            MessageBox.Show(currentTag + " Tags created." + Environment.NewLine + Environment.NewLine + "Do not forget to delete these tags when finished.", "Tags created");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            LoadLostObjects();
        }

        private static void DeleteLostFoundTags()
        {
            foreach (GitHead head in GitCommands.GitCommands.GetHeads(true, false))
            {
                if (head.Name.StartsWith("LOST_FOUND_"))
                    GitCommands.GitCommands.DeleteTag(head.Name);
            }
        }

        private void FullCheck_CheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void NoReflogs_CheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }

        private void TagAllCommits_Click(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            CreateLostFoundTags(true);
            LoadLostObjects();
        }

        private void ShowOnlyCommits_CheckedChanged(object sender, EventArgs e)
        {
            LoadLostObjects();
        }
    }
}
