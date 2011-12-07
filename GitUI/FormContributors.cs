using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormContributors : GitExtensionsForm
    {
        public FormContributors()
        {
            InitializeComponent();

            Translate();
        }

        public void LoadContributors(string coders, string translators, string designers, string others)
        {
            codersLabel.Text = coders;
            translatorsLabel.Text = translators;
            designersLabel.Text = designers;
        }
    }
}
