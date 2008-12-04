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
    public partial class FormClone : Form
    {
        public FormClone()
        {
            InitializeComponent();
            From.Text = Settings.WorkingDir;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                OutPut.Text = "";

                CloneDto dto = new CloneDto(From.Text, To.Text);
                GitCommands.Clone commit = new GitCommands.Clone(dto);
                commit.Execute();

                OutPut.Text = "Command executed \n" + dto.Result;
            }
            catch
            {
            }
        }

        private void FromBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                From.Text = dialog.SelectedPath;

        }

        private void ToBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                To.Text = dialog.SelectedPath;

        }
    }
}
