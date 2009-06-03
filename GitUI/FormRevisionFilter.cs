using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormRevisionFilter : GitExtensionsForm
    {
        public FormRevisionFilter()
        {
            InitializeComponent();
        }

        private void FormRevisionFilter_Load(object sender, EventArgs e)
        {
            EnableFilters();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void SinceCheck_CheckedChanged(object sender, EventArgs e)
        {
            EnableFilters();
        }

        private void EnableFilters()
        {
            Since.Enabled = SinceCheck.Checked;
            Until.Enabled = CheckUntil.Checked;
            Author.Enabled = AuthorCheck.Checked;
            Committer.Enabled = CommitterCheck.Checked;
            Message.Enabled = MessageCheck.Checked;
            Limit.Enabled = LimitCheck.Checked;
            FileFilter.Enabled = FileFilterCheck.Checked;
        }

        public string GetFilter()
        {
            string filter = "";
            if (SinceCheck.Checked)
                filter += " --since=\"" + Since.Value.ToString() + "\"";
            if (CheckUntil.Checked)
                filter += " --until=\"" + Until.Value.ToString() + "\"";
            if (AuthorCheck.Checked)
                filter += " --author=\"" + Author.Text + "\"";
            if (CommitterCheck.Checked)
                filter += " --committer=\"" + Committer.Text + "\"";
            if (MessageCheck.Checked)
                filter += " --grep=\"" + Message.Text + "\"";
            if (LimitCheck.Checked)
                filter += " --max-count=\"" + Limit.Value.ToString("N") + "\"";
            if (!string.IsNullOrEmpty(filter) && IgnoreCase.Checked)
                filter += " --regexp-ignore-case";
            if (FileFilterCheck.Checked)
                filter += " \"" + FileFilter.Text + "\"";

            return filter;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
