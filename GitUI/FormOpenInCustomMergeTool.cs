using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ResourceManager.Translation;
using GitCommands;
using GitCommands.Repository;

namespace GitUI
{
    public partial class FormOpenInCustomMergeTool : GitExtensionsForm
    {
        TranslationString descriptionText = new TranslationString("Choose a custom mergetool and enter arguments to merge:\n{0}");


        public FormOpenInCustomMergeTool()
        {
            InitializeComponent();
            Translate();
        }

        public FormOpenInCustomMergeTool(string fileName)
        {
            InitializeComponent();
            Translate();
            FileName = fileName;
        }

        public string FileName { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _NO_TRANSLATE_description.Text = string.Format(descriptionText.Text, FileName);
            mergeTool.DataSource = Repositories.RepositoryHistory.Repositories;
            mergeTool.DisplayMember = "Path";

            //try to suggest a custom mergetool
            
            //KDIFF3
            mergeTool.Text = GitCommandHelpers.GetGlobalSetting("mergetool." + GitCommandHelpers.GetGlobalSetting("merge.tool").Trim() + ".path").Trim();
            mergeToolArguments.Text = "\"$BASE\" \"$LOCAL\" \"$REMOTE\" -o \"$MERGED\"";
        }

        private void browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "c:\\";
            ofd.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
                mergeTool.Text = ofd.FileName;
        }

        private void merge_Click(object sender, EventArgs e)
        {
            Repositories.RepositoryHistory.AddMostRecentRepository(mergeTool.Text);

            string[] filenames = GitCommandHelpers.GetConflictedFiles(FileName);

            string arguments = mergeToolArguments.Text;
            arguments = arguments.Replace("$BASE", filenames[0]);
            arguments = arguments.Replace("$LOCAL", filenames[1]);
            arguments = arguments.Replace("$REMOTE", filenames[2]);
            arguments = arguments.Replace("$MERGED", FileName);

            int exitCode;
            GitCommandHelpers.RunCmd(mergeTool.Text, "" + arguments + "", out exitCode);

            Close();
        }
    }
}
