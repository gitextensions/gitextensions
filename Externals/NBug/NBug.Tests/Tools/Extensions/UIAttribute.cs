// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UIAttribute.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Tools.Extensions
{
	using Xunit;

	public class UIAttribute : TraitAttribute
	{
		public UIAttribute()
			: base("Category", "UI")
		{
		}
	}
}