// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummySerializableException.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
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
