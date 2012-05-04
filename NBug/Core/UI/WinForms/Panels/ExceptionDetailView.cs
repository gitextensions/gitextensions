// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionDetailView.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.WinForms.Panels
{
	using System;
	using System.Windows.Forms;

	internal partial class ExceptionDetailView : Form
	{
		public ExceptionDetailView()
		{
			this.InitializeComponent();
		}

		internal void ShowDialog(string property, string info)
		{
			this.propertyTextBox.Text = property;
			this.propertyInformationTextBox.Text = info;
			this.ShowDialog();
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
