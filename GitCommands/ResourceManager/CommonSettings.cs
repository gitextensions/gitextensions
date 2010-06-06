using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager
{
    /// <summary>
    /// Overall common configuration settings
    /// </summary>    
    public class CommonSettings
    {
        #region Constants
        /// <summary>
        /// manifest extension for all embedded resources.
        /// </summary>
        public const string AssemblyExtension = ".resources";
        /// <summary>
        /// extension for binary resource files.
        /// </summary>
        public const string BinaryExtension = ".resources";
        /// <summary>
        /// extension for Xml resource files.
        /// </summary>
        public const string XmlExtension = ".resx";
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonSettings"/> class.
        /// </summary>
        public CommonSettings()
        {
        }
        #endregion
    }
}
