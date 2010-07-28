using System;

namespace GitUI
{
    public partial class FormModifiedDeletedCreated : GitExtensionsForm
    {
        public FormModifiedDeletedCreated()
        {
            InitializeComponent();
            Translate();
            Aborted = true;
        }

        public bool Aborted { get; set; }
        public bool Delete { get; set; }

        private void CreatedClick(object sender, EventArgs e)
        {
            Aborted = false;
            Delete = false;
            Close();
        }

        private void DeletedClick(object sender, EventArgs e)
        {
            Aborted = false;
            Delete = true;
            Close();
        }

        private void AbortClick(object sender, EventArgs e)
        {
            Aborted = true;
            Close();
        }
    }
}