// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UIAttribute.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Tools.Extensions
{
	using Xunit;

	public class UIAttribute : TraitAttribute
	{
		public UIAttribute() : base("Category", "UI")
		{
		}
	}
}
