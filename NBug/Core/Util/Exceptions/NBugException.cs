// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NBugException.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class NBugException : Exception
	{
		public NBugException()
		{
		}

		public NBugException(string message) : base(message)
		{
		}

		public NBugException(string message, Exception inner) : base(message, inner)
		{
		}

		protected NBugException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
