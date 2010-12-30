// Copyright (c) 2003, Paul Welter
// All rights reserved.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;

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
[assembly: AssemblyCopyright("Copyright © 2003 Paul Welter.  All rights reserved.")]
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

[assembly: AssemblyVersion("2.1.7.*")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"..\..\Spell.snk")]
[assembly: AssemblyKeyName("")]

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
			this.myType = type;
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

