﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NBugException.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
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

		public NBugException(string message)
			: base(message)
		{
		}

		public NBugException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected NBugException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}