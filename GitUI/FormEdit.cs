using System;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormEdit : GitExtensionsForm
    {
        public FormEdit(string text)
        {
            InitializeComponent();
            Translate();
            Viewer.ViewText("", text);
            Viewer.IsReadOnly = false;
        }

        private void FormEditFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("edit");
        }

        private void FormEditLoad(object sender, EventArgs e)
        {
            RestorePosition("edit");
        }
    }
}