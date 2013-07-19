// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Feedback.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.WinForms
{
	using System.Windows.Forms;

	using NBug.Properties;

	internal partial class Feedback : Form
	{
		public Feedback()
		{
			this.InitializeComponent();
			this.Icon = Resources.NBug_icon_16;
		}
	}
}