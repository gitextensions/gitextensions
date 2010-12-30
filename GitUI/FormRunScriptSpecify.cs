﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormRunScriptSpecify : Form
    {
        public FormRunScriptSpecify(List<GitCommands.GitHead> options, string label)
        {
            InitializeComponent();
            specifyLabel.Text = "Specify '" + label+"':";
            foreach (GitCommands.GitHead head in options)
            {
                branchesListView.Items.Add(head.Name);
            }
        }
        public FormRunScriptSpecify(List<string> options, string label)
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
            this.Close();
        }
    }
}
