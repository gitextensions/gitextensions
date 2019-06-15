// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionDetailView.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Forms;

namespace GitUI.NBugReports
{
    internal partial class ExceptionDetailView : Form
    {
        public ExceptionDetailView()
        {
            InitializeComponent();
        }

        internal void ShowDialog(string property, string info)
        {
            propertyTextBox.Text = property;
            propertyInformationTextBox.Text = info;
            ShowDialog();
        }
    }
}