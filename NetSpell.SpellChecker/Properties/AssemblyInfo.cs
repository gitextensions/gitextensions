// Copyright (c) 2003, Paul Welter
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

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

namespace NetSpell.SpellChecker
{
    /// <summary>
    ///     AssemblyInfo class
    /// </summary>
    public class AssemblyInfo
    {
        private readonly Type _myType;

        /// <summary>
        ///     Initialized the AssemblyInfo class with the given type
        /// </summary>
        public AssemblyInfo(Type type)
        {
            _myType = type;
        }

        /// <summary>
        /// CodeBase of Assembly
        /// </summary>
        public string CodeBase => _myType.Assembly.CodeBase.ToString(CultureInfo.CurrentUICulture);

        /// <summary>
        /// Company of Assembly
        /// </summary>
        public string Company
        {
            get
            {
                object[] r = _myType.Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                var ct = (AssemblyCompanyAttribute)r[0];
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
                object[] r = _myType.Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                var ct = (AssemblyCopyrightAttribute)r[0];
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
                object[] r = _myType.Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                var ct = (AssemblyDescriptionAttribute)r[0];
                return ct.Description;
            }
        }

        /// <summary>
        ///     FullName of Assembly
        /// </summary>
        public string FullName => _myType.Assembly.GetName().FullName.ToString(CultureInfo.CurrentUICulture);

        /// <summary>
        /// Name of Assembly
        /// </summary>
        public string Name => _myType.Assembly.GetName().Name.ToString(CultureInfo.CurrentUICulture);

        /// <summary>
        /// Product of Assembly
        /// </summary>
        public string Product
        {
            get
            {
                object[] r = _myType.Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                var ct = (AssemblyProductAttribute)r[0];
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
                object[] r = _myType.Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                var ct = (AssemblyTitleAttribute)r[0];
                return ct.Title;
            }
        }

        /// <summary>
        /// Version of Assembly
        /// </summary>
        public string Version => _myType.Assembly.GetName().Version.ToString();
    }
}
