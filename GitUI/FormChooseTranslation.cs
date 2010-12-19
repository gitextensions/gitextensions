using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormChooseTranslation : GitExtensionsForm
    {
        public FormChooseTranslation()
        {
            InitializeComponent();
            Translate();
        }

        private void dutch_Click(object sender, EventArgs e)
        {
            Settings.Translation = "Dutch";
            Close();
        }

        private void brittish_Click(object sender, EventArgs e)
        {
            Settings.Translation = "English";
            Close();
        }

        private void italian_Click(object sender, EventArgs e)
        {
            Settings.Translation = "Italiano";
            Close();
        }

        private void japanese_Click(object sender, EventArgs e)
        {
            Settings.Translation = "Japanese";
            Close();
        }

        private void spanish_Click(object sender, EventArgs e)
        {
            Settings.Translation = "Spanish";
            Close();
        }

        private void FormChooseTranslation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Translation))
                Settings.Translation = "English";
        }
    }
}
