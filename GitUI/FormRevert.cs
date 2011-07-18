using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormRevert : GitExtensionsForm
    {
        private readonly TranslationString _resetChangesCaption = new TranslationString("Reset changes");
        private readonly TranslationString _undoChangesIn = new TranslationString("Undo changes in:\n{0}?");
        private readonly string fileName;

        public FormRevert(string fileName)
        {
            this.fileName = fileName;
            InitializeComponent();
            Translate();
        }

        private void FormRevert_Load(object sender, EventArgs e)
        {
            _NO_TRANSLATE_RevertLabel.Text = string.Format(_undoChangesIn.Text, fileName);
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string output = GitCommandHelpers.ResetFile(fileName);

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, _resetChangesCaption.Text);
            Close();
            Cursor.Current = Cursors.Default;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
