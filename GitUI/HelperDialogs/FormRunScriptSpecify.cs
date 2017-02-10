using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.HelperDialogs
{
    public partial class FormRunScriptSpecify : Form
    {
        public FormRunScriptSpecify(IEnumerable<IGitRef> options, string label)
        {
            InitializeComponent();
            specifyLabel.Text = "Specify '" + label+"':";
            foreach (var head in options)
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
