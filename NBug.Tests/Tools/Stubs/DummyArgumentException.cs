// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DummyArgumentException.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
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
			: base(
				"Testing MyArgumentException.", 
				"MyDummyParameter", 
				new Exception("Testing inner exception for MyArgumentException."))
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
