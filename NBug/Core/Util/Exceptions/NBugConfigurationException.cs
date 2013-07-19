// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NBugConfigurationException.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util.Exceptions
{
	using System;
	using System.Linq.Expressions;

	[Serializable]
	public class NBugConfigurationException : NBugException
	{
		public NBugConfigurationException(string message, Exception inner)
			: base(message, inner)
		{
			this.MisconfiguredProperty = string.Empty;
		}

		private NBugConfigurationException(string propertyName, string message)
			: base(message)
		{
			this.MisconfiguredProperty = propertyName;
		}

		public string MisconfiguredProperty { get; set; }

		public static NBugConfigurationException Create<T>(Expression<Func<T>> propertyExpression, string message)
		{
			return new NBugConfigurationException(((MemberExpression)propertyExpression.Body).Member.Name, message);
		}
	}
}