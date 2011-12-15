using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    using System.IO;

    using GitCommands;

    public partial class FormSvnClone : Form
    {
        public FormSvnClone()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                var dirTo = destinationComboBox.Text;
                if (!dirTo.EndsWith(Settings.PathSeparator.ToString()) && !dirTo.EndsWith(Settings.PathSeparatorWrong.ToString()))
                    dirTo += Settings.PathSeparator.ToString();

                dirTo += subdirectoryTextBox.Text;

                //Repositories.RepositoryHistory.AddMostRecentRepository(_NO_TRANSLATE_From.Text);
                //Repositories.RepositoryHistory.AddMostRecentRepository(dirTo);

                if (!Directory.Exists(dirTo))
                    Directory.CreateDirectory(dirTo);

                var authorsfile = this.authorsFileTextBox.Text;
                bool resetauthorsfile = false;
                if (authorsfile != null && authorsfile.Trim().Length != 0 && !File.Exists(authorsfile.Trim()) && !(resetauthorsfile = this.AskContinutWithoutAuthorsFile(authorsfile)))
                {
                    return;
                }
                if (resetauthorsfile)
                {
                    authorsfile = null;
                }
                var fromProcess = new FormProcess(
                    Settings.GitCommand, GitSvnCommandHelpers.CloneCmd(svnRepositoryComboBox.Text, dirTo, authorsfile));
                
                fromProcess.ShowDialog(this);

                if (fromProcess.ErrorOccurred() || Settings.Module.InTheMiddleOfPatch())
                    return;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Exception: " + ex.Message, "Clone failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AskContinutWithoutAuthorsFile(string authorsfile)
        {
            return MessageBox.Show(
                this,
                "Authors file " + authorsfile + "does not exists. Continue without authors file?",
                "Authors file",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = destinationComboBox.Text };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                destinationComboBox.Text = dialog.SelectedPath;
        }

        private void authorsFileBrowseButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog() { InitialDirectory = this.destinationComboBox.Text };
            if (dialog.ShowDialog(this) == DialogResult.OK) 
                authorsFileTextBox.Text = dialog.FileName;
        }
    }
}
