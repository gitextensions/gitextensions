using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.HelperDialogs
{
    public partial class FormRunScriptSpecify : Form
    {
        public FormRunScriptSpecify(IEnumerable<GitRef> options, string label)
        {
            InitializeComponent();
            specifyLabel.Text = "Specify '" + label+"':";
            foreach (GitRef head in options)
            {
                branchesListView.Items.Add(head.Name);
            }
        }
        public FormRunScriptSpecify(IEnumerable<string> options, string label)
        {
            InitializeComponent();
            specifyLabel.Text = "Specify '" + label + "':";
            foreach (string head in options)
            {
                branchesListView.Items.Add(head);
            }
        }
        public string ret { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            ret = branchesListView.SelectedItems[0].Text;
            Close();
        }
    }
}
