using System;
using System.Windows.Forms;
using ResourceManager.Translation;
using GitCommands;

namespace GitUI
{
    public partial class FormArchive : GitExtensionsForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox =
            new TranslationString("Select 1 revision to archive");

        private readonly TranslationString _saveFileDialogFilter =
            new TranslationString("Zip file (*.zip)");
        private readonly TranslationString _saveFileDialogCaption =
            new TranslationString("Save archive as");

        public FormArchive()
            : base(true)
        {
            InitializeComponent();
            Translate();
        }

        /// <summary>
        /// TODO: does not work yet
        /// </summary>
        public GitRevision PreselectRevisionOnLoad { get; set; }

        ////private EventHandler tmpEventHandler;

        private void FormArchive_Load(object sender, EventArgs e)
        {
            revisionGrid1.Load();

            // does not work
            ////if (PreselectRevisionOnLoad != null)
            ////{
            ////    revisionGrid1.SetSelectedRevision(PreselectRevisionOnLoad);
            ////}

            ////tmpEventHandler = new EventHandler(revisionGrid1_SelectionChanged);
            ////revisionGrid1.SelectionChanged += tmpEventHandler;
        }

        ////void revisionGrid1_SelectionChanged(object sender, EventArgs e)
        ////{
        ////    if (PreselectRevisionOnLoad != null)
        ////    {
        ////        revisionGrid1.SetSelectedRevision(PreselectRevisionOnLoad);
        ////    }

        ////    //revisionGrid1.SelectionChanged -= tmpEventHandler;
        ////}

        private void Save_Click(object sender, EventArgs e)
        {
            if (revisionGrid1.GetSelectedRevisions().Count != 1)
            {
                MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string revision = revisionGrid1.GetSelectedRevisions()[0].TreeGuid;

            using (var saveFileDialog = new SaveFileDialog { Filter = _saveFileDialogFilter.Text + "|*.zip", Title = _saveFileDialogCaption.Text })
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    FormProcess.ShowDialog(this, "archive --format=zip " + revision + " --output \"" + saveFileDialog.FileName + "\"");
                    Close();
                }
            }
        }
    }
}
