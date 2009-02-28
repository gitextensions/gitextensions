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
            InitializeComponent();
            Viewer.ViewText("", text);
        }

        private void FormEdit_Load(object sender, EventArgs e)
        {

        }
    }
}
