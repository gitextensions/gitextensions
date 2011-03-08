using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Text.RegularExpressions;

namespace GitUI
{
    public partial class FormGoToCommit : GitExtensionsForm
    {
        public FormGoToCommit()
        {
            InitializeComponent();
            Translate();
        }

        public string GetRevision()
        {
            return GitCommandHelpers.RevParse(commitExpression.Text);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
