// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WinFormsUITests.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Functional
{
	using NBug.Core.Reporting.Info;
	using NBug.Core.UI.WinForms;
	using NBug.Enums;
	using NBug.Tests.Tools.Extensions;
	using NBug.Tests.Tools.Fixtures;
	using NBug.Tests.Tools.Stubs;

	using Xunit;

	public class WinFormsUITests : IUseFixture<SettingsFixture>, IUseFixture<UIFixture>
	{
		[Fact, UI]
		public void Full()
		{
			var exception = new DummySerializableException();
			WinFormsUI.ShowDialog(UIMode.Full, exception, new Report(exception));
		}

		[Fact, UI]
		public void Minimal()
		{
			var exception = new DummySerializableException();
			WinFormsUI.ShowDialog(UIMode.Minimal, exception, new Report(exception));
		}

		public void SetFixture(SettingsFixture settings)
		{
		}

		public void SetFixture(UIFixture data)
		{
		}
	}
}
