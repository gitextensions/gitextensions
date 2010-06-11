using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormEdit : GitExtensionsForm
    {
        public FormEdit(string text)
        {
            InitializeComponent(); Translate();
            Viewer.ViewText("", text);
        }

        private void FormEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("edit");
        }

        private void FormEdit_Load(object sender, EventArgs e)
        {
            RestorePosition("edit");
        }
    }
}
