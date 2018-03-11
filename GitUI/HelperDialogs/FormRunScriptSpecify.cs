using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.HelperDialogs
{
    public partial class FormRunScriptSpecify : Form
    {
        public FormRunScriptSpecify(IEnumerable<IGitRef> options, string label)
        {
            InitializeComponent();
            specifyLabel.Text = "Specify '" + label + "':";
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

        public string Ret { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            if (branchesListView.SelectedItems.Count > 0)
            {
                Ret = branchesListView.SelectedItems[0].Text;
                Close();
            }
        }
    }
}
