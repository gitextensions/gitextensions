// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Http.cs" company="NBusy Project">
//   Copyright © 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Configurator.SubmitPanels.Web
{
	using System.Windows.Forms;

	public partial class Http : UserControl, ISubmitPanel
	{
		public Http()
		{
			InitializeComponent();
		}

		public string ConnectionString
		{
			get
			{
				// Check the mendatory fields
				if (string.IsNullOrEmpty(this.urlTextBox.Text))
				{
					MessageBox.Show("Mandatory field \"" + urlTextBox.Name + "\" cannot be left blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}

				// Do fixes
				if (!this.urlTextBox.Text.EndsWith("/"))
				{
					this.urlTextBox.Text += "/";
				}

				var http = new Core.Submission.Web.Http { Url = this.urlTextBox.Text };

				return http.ConnectionString;
			}

			set
			{
				var http = new Core.Submission.Web.Http(value);
				this.urlTextBox.Text = http.Url;
			}
		}
	}
}
