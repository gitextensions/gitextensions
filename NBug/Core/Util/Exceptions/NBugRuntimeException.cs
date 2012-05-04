// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NBugRuntimeException.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util.Exceptions
{
	using System;

	[Serializable]
	public class NBugRuntimeException : NBugException
	{
		public NBugRuntimeException(string message, Exception inner) : base(message, inner)
		{
		}

		public NBugRuntimeException(string message) : base(message)
		{
		}
	}
}
