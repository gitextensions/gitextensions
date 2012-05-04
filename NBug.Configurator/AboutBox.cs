namespace NBug.Configurator
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Windows.Forms;

	/// <summary>
	/// The about box.
	/// </summary>
	internal partial class AboutBox : Form
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AboutBox"/> class.
		/// </summary>
		internal AboutBox()
		{
			this.InitializeComponent();

			// Initialize the AboutBox to display the product information from the assembly information.
			// Change assembly information settings for your application through either:
			// - Project->Properties->Application->Assembly Information
			// - AssemblyInfo.cs
			// this.Text = string.Format("About {0}", this.AssemblyTitle);
			this.Text = string.Format("About {0}", this.AssemblyProduct);
			this.productNameLabel.Text = this.AssemblyProduct;
			this.versionLabel.Text = string.Format("Version {0}", this.AssemblyFileVersion);
			this.copyrightLabel.Text = this.AssemblyCopyright;
		}

		internal string AssemblyCompany
		{
			get
			{
				// Get all Company attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

				// If there aren't any Company attributes, return an empty string
				if (attributes.Length == 0)
				{
					return string.Empty;
				}

				// If there is a Company attribute, return its value
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}

		internal string AssemblyCopyright
		{
			get
			{
				// Get all Copyright attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

				// If there aren't any Copyright attributes, return an empty string
				if (attributes.Length == 0)
				{
					return string.Empty;
				}

				// If there is a Copyright attribute, return its value
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		internal string AssemblyDescription
		{
			get
			{
				// Get all Description attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(
					typeof(AssemblyDescriptionAttribute), false);

				// If there aren't any Description attributes, return an empty string
				if (attributes.Length == 0)
				{
					return string.Empty;
				}

				// If there is a Description attribute, return its value
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		internal string AssemblyProduct
		{
			get
			{
				// Get all Product attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);

				// If there aren't any Product attributes, return an empty string
				if (attributes.Length == 0)
				{
					return string.Empty;
				}

				// If there is a Product attribute, return its value
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		internal string AssemblyTitle
		{
			get
			{
				// Get all Title attributes on this assembly
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

				// If there is at least one Title attribute
				if (attributes.Length > 0)
				{
					// Select the first one
					var titleAttribute = (AssemblyTitleAttribute)attributes[0];

					// If it is not an empty string, return it
					if (titleAttribute.Title != string.Empty)
					{
						return titleAttribute.Title;
					}
				}

				// If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
				return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		internal string AssemblyFileVersion
		{
			get
			{
				return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
			}
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void LeadDeveloperLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("http://www.soygul.com/");
		}
	}
}