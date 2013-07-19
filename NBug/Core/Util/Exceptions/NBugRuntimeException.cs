// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NBugRuntimeException.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util.Exceptions
{
	using System;

	[Serializable]
	public class NBugRuntimeException : NBugException
	{
		public NBugRuntimeException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public NBugRuntimeException(string message)
			: base(message)
		{
		}
	}
}