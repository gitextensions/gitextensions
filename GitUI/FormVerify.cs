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
            Cursor.Current = Cursors.WaitCursor;
            FormProcess process = new FormProcess("fsck-objects");
            Warnings.DataSource = process.outputString.ToString().Split('\n');
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormProcess process = new FormProcess("fsck-objects --lost-found");
            FormVerify_Shown(null, null);
        }

        private void Warnings_DoubleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            
            string sha1 = FindSha1();
            if (!string.IsNullOrEmpty(sha1))
            {
                new FormEdit(GitCommands.GitCommands.ShowSha1(sha1)).ShowDialog();
            }
        }

        private string FindSha1()
        {
            string warningString = Warnings.SelectedValue as string;
            foreach (string sha1 in warningString.Split(' '))
            {
                if (sha1.Length == 40)
                {
                    return sha1;
                }
            }

            return "";
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete dangling objects?", "Remove", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            string sha1 = FindSha1();
            if (!string.IsNullOrEmpty(sha1))
            {
                FormTagSmall form = new FormTagSmall();
                form.Revision = new GitRevision();
                form.Revision.Guid = sha1;
                form.ShowDialog();
            }
        }
    }
}
