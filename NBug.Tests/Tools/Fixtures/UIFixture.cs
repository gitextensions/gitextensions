// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UIFixture.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
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
