// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummySerializableException.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Tools.Stubs
{
	using NBug.Core.Util.Serialization;

	public class DummySerializableException : SerializableException
	{
		public DummySerializableException()
			: base(new DummyArgumentException())
		{
		}
	}
}