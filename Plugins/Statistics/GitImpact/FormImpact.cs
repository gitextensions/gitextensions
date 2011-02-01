using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands.Statistics;

namespace GitImpact
{
    public partial class FormImpact : Form
    {
        public FormImpact()
        {
            InitializeComponent();
            Impact.UpdateData();
        }
    }
}
