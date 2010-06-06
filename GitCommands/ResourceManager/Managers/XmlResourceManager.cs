//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//===============================================================================

using System;
using System.Globalization;

namespace ResourceManager
{
    /// <summary>
    /// Custom resource manager for managing resources held in xml files
    /// </summary>
    public class XmlResourceManager : FileResourceManager
    {
        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResourceManager"/> class.
        /// </summary>
        /// <param name="pathName">the file path to the resource files.</param>
        /// <param name="baseName">base name for the set of resources.</param>
        public XmlResourceManager(string pathName, string baseName)
            : base(pathName, baseName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResourceManager"/> class.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        public XmlResourceManager(Type resourceType)
            : base(resourceType)
        {
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Creates the resource set.
        /// </summary>
        /// <param name="filePathName">Name of the file path.</param>
        /// <param name="fileBaseName">Name of the file base.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>a common resource set type</returns>
        protected override CommonResourceSet CreateResourceSet(string filePathName, string fileBaseName, CultureInfo cultureInfo)
        {
            return new XmlResourceSet(filePathName, fileBaseName, cultureInfo);
        }
        #endregion
    }
}
