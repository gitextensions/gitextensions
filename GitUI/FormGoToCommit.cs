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
        private RevisionGrid _revGrid;

        public FormGoToCommit(RevisionGrid revGrid)
        {
            InitializeComponent();
            Translate();
            _revGrid = revGrid;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string commitExpr = GitCommandHelpers.RevParse(commitExpression.Text);
            if (!commitExpr.Equals(""))
            {
                _revGrid.SetSelectedRevision(new GitRevision { Guid = commitExpr });
            }
            Close();
        }
    }
}
