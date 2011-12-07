using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCherryPickMerge : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _noneParentSelectedText =
            new TranslationString("None parent is selected!");
        private readonly TranslationString _noneParentSelectedTextCaption =
            new TranslationString("Error");
        #endregion
        
        public bool OkClicked;
        public FormCherryPickMerge(GitRevision[] parents)
            : base()
        {
            InitializeComponent();

            Translate();

            //constructor for translation
            if (parents != null)
            {
                for (int i = 0; i < parents.Length; i++)
                {
                    ParentsList.Items.Add(i + 1 + "");
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].Message);
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].Author);
                    ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].CommitDate.ToShortDateString());
                }
                ParentsList.TopItem.Selected = true;
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (ParentsList.SelectedItems.Count == 0)
                MessageBox.Show(this, _noneParentSelectedText.Text, _noneParentSelectedTextCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                OkClicked = true;
                Close();
            }

        }
        
    }
}
