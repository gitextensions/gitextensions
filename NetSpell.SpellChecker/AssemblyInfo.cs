﻿// Copyright (c) 2003, Paul Welter
// All rights reserved.

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("NetSpell.SpellChecker")]
[assembly: AssemblyDescription("NetSpell is a spell checking engine written entirely in managed C# .net code")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("LoreSoft")]
[assembly: AssemblyProduct("NetSpell")]
[assembly: AssemblyCopyright("Copyright © 2003 Paul Welter, modified by Henk Westhuis for Git Extensions.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("2.1.7.35462")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

namespace NetSpell.SpellChecker
{
	/// <summary>
	///     AssemblyInfo class
	/// </summary>
	public class AssemblyInfo
	{

		private Type myType;

		/// <summary>
		///     Initialized the AssemblyInfo class with the given type
		/// </summary>
		public AssemblyInfo(Type type)
		{
			myType = type;
		}

		/// <summary>
		/// CodeBase of Assembly
		/// </summary>
		public string CodeBase
		{
			get {return myType.Assembly.CodeBase.ToString(CultureInfo.CurrentUICulture);}
		}

		/// <summary>
		/// Company of Assembly
		/// </summary>
		public string Company
		{
			get
			{
				Object[] r = myType.Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				AssemblyCompanyAttribute ct = (AssemblyCompanyAttribute)r[0];
				return ct.Company;
			}
		}

		/// <summary>
		/// Copyright of Assembly
		/// </summary>
		public string Copyright
		{
			get
			{
				Object[] r = myType.Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				AssemblyCopyrightAttribute ct = (AssemblyCopyrightAttribute)r[0];
				return ct.Copyright;
			}
		}

		/// <summary>
		/// Description of Assembly
		/// </summary>
		public string Description
		{
			get
			{
				Object[] r = myType.Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				AssemblyDescriptionAttribute ct = (AssemblyDescriptionAttribute)r[0];
				return ct.Description;
			}
		}

		/// <summary>
		///		FullName of Assembly
		/// </summary>
		public string FullName
		{
			get {return myType.Assembly.GetName().FullName.ToString(CultureInfo.CurrentUICulture);}
		}

		/// <summary>
		/// Name of Assembly
		/// </summary>
		public string Name
		{
			get	{return myType.Assembly.GetName().Name.ToString(CultureInfo.CurrentUICulture);}
		}

		/// <summary>
		/// Product of Assembly
		/// </summary>
		public string Product
		{
			get
			{
				Object[] r = myType.Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				AssemblyProductAttribute ct = (AssemblyProductAttribute)r[0];
				return ct.Product;
			}
		}

		/// <summary>
		/// Title of Assembly
		/// </summary>
		public string Title
		{
			get
			{
				Object[] r = myType.Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				AssemblyTitleAttribute ct = (AssemblyTitleAttribute)r[0];
				return ct.Title;
			}
		}

		/// <summary>
		/// Version of Assembly
		/// </summary>
		public string Version
		{
			get { return myType.Assembly.GetName().Version.ToString(); }
		}
	}
}

