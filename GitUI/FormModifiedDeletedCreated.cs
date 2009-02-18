using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormModifiedDeletedCreated : GitExtensionsForm
    {
        public FormModifiedDeletedCreated()
        {
            InitializeComponent();
            Aborted = true;
        }

        public bool Aborted { get; set; }
        public bool Delete { get; set; }

        private void Created_Click(object sender, EventArgs e)
        {
            Aborted = false;
            Delete = false;
            Close();
        }

        private void Deleted_Click(object sender, EventArgs e)
        {
            Aborted = false;
            Delete = true;
            Close();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Aborted = true;
            Close();
        }
    }
}
