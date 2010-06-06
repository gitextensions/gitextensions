//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//===============================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Globalization;

namespace ResourceManager
{
    /// <summary>
    /// Resource set for creating sets of resources from xml files
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class XmlResourceSet : CommonResourceSet
    {
        #region Fields
        string pathName;
        string baseName;
        CultureInfo cultureInfo;
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResourceSet"/> class.
        /// </summary>
        /// <param name="pathName">the file path to the resource files.</param>
        /// <param name="baseName">the base name of the set of resources.</param>
        /// <param name="cultureInfo">The culture.</param>
        public XmlResourceSet(string pathName, string baseName, CultureInfo cultureInfo)
            : base(new XmlResourceReader(pathName, baseName, cultureInfo))
        {
            this.pathName = pathName;
            this.baseName = baseName;
            this.cultureInfo = cultureInfo;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the preferred resource reader class for this kind of <see cref="T:System.Resources.ResourceSet"></see>.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="T:System.Type"></see> for the preferred resource reader for this kind of <see cref="T:System.Resources.ResourceSet"></see>.
        /// </returns>
        public override Type GetDefaultReader()
        {
            return typeof(XmlResourceReader);
        }

        /// <summary>
        /// Returns the preferred resource writer class for this kind of <see cref="T:System.Resources.ResourceSet"></see>.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="T:System.Type"></see> for the preferred resource writer for this kind of <see cref="T:System.Resources.ResourceSet"></see>.
        /// </returns>
        public override Type GetDefaultWriter()
        {
            return typeof(XmlResourceWriter);
        }

        /// <summary>
        /// Creates the default resource reader.
        /// </summary>
        /// <returns>IResourceReader instance</returns>
        public override IResourceReader CreateDefaultReader()
        {
            return new XmlResourceReader(pathName, baseName, cultureInfo);
        }

        /// <summary>
        /// Creates the default resource writer.
        /// </summary>
        /// <returns>IResourceWriter instance</returns>
        public override IResourceWriter CreateDefaultWriter()
        {
            return new XmlResourceWriter(ResourceFileName.Build(pathName, baseName, cultureInfo, CommonSettings.XmlExtension));
        }
        #endregion
    }
}
