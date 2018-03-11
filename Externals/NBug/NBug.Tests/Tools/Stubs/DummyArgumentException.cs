// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyArgumentException.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Tools.Stubs
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class DummyArgumentException : ArgumentException
	{
		public DummyArgumentException()
			: base("Testing MyArgumentException.", "MyDummyParameter", new Exception("Testing inner exception for MyArgumentException."))
		{
		}

		public override IDictionary Data
		{
			get
			{
				return new Dictionary<string, string> { { "StringDataProperty", "Just testing the data property with a set of strings as a dictionary." } };
			}
		}
	}
}