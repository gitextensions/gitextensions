// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreviewForm.cs" company="NBusy Project">
//   Copyright © 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Configurator
{
	using System;
	using System.IO;
	using System.Windows.Forms;

	using NBug.Core.Reporting.Info;
	using NBug.Core.UI.Console;
	using NBug.Core.UI.WinForms;
	using NBug.Core.UI.WPF;
	using NBug.Core.Util.Serialization;
	using NBug.Enums;

	internal partial class PreviewForm : Form
	{
		public PreviewForm()
		{
			InitializeComponent();
		}

		internal void ShowDialog(UIMode uiMode, UIProvider uiProvider)
		{
			var exception = new SerializableException(new ArgumentException("Argument exception preview.", new Exception("Inner exception for argument exception.")));
			var report = new Report(exception);

			var consoleOut = new StringWriter();
			Console.SetOut(consoleOut);

			if (uiProvider == UIProvider.Console)
			{
				ConsoleUI.ShowDialog(uiMode, exception, report);
				this.consoleOutputTextBox.Text = consoleOut.ToString();
				this.ShowDialog();
			}
			else if (uiProvider == UIProvider.WinForms)
			{
				WinFormsUI.ShowDialog(uiMode, exception, report);
				this.Close();
			}
			else if (uiProvider == UIProvider.WPF)
			{
				WPFUI.ShowDialog(uiMode, exception, report);
				this.Close();
			}
			else
			{
				throw new ArgumentException("Parameter supplied for UIProvider argument is invalid.");
			}
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
