// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UIFixture.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Tools.Fixtures
{
	using System.Windows.Forms;

	public class UIFixture
	{
		public UIFixture()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
		}
	}
}