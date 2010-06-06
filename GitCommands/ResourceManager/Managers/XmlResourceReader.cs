//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//===============================================================================

using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Security.Permissions;
using System.Collections;
using System.Globalization;

namespace ResourceManager
{
    /// <summary>
    /// Custom Resource Reader to read resources from an xml file
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class XmlResourceReader : FileResourceReader
    {
        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="T:XmlResourceReader"/> class.
        /// </summary>
        /// <param name="pathName">the file path to the resource files.</param>
        /// <param name="baseName">the base name for the set of resources.</param>
        /// <param name="cultureInfo">The culture information.</param>
        public XmlResourceReader(string pathName, string baseName, CultureInfo cultureInfo)
            : base(/*pathName*/@"C:\Development\GitUI\Translations\", baseName, cultureInfo, CommonSettings.XmlExtension)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates the specific file based resource reader.
        /// </summary>
        /// <param name="fileName">Fully qualified name of the resources file.</param>
        /// <param name="useResourceDataNodes">if set to <c>true</c> use data nodes.</param>
        /// <returns>Resource reader</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public override IResourceReader CreateResourceReader(string fileName, bool useResourceDataNodes)
        {
            ResXResourceReader resourceReader = new ResXResourceReader(fileName);
            resourceReader.UseResXDataNodes = useResourceDataNodes;
            return resourceReader;
        }

        /// <summary>
        /// Translates the given data node into an IResourceDataNode type.
        /// </summary>
        /// <param name="dataNode">The data node.</param>
        /// <returns>IResourceDataNode type</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public override IResourceDataNode ResourceDataNodeTranslator(IDictionaryEnumerator dataNode)
        {
            if (dataNode == null)
                throw new ArgumentNullException("dataNode");

            IResourceDataNode resourceDataNode = null;

            ResXDataNode nativeDataNode = dataNode.Value as ResXDataNode;
            if (nativeDataNode != null)
            {
                if (nativeDataNode.FileRef == null)
                    resourceDataNode = new ResourceDataNode(nativeDataNode.Name, nativeDataNode.GetValue((ITypeResolutionService)null));
                else
                {
                    ResourceFileRef resourceFileRef = new ResourceFileRef(nativeDataNode.FileRef.FileName, nativeDataNode.FileRef.TypeName, nativeDataNode.FileRef.TextFileEncoding);
                    resourceDataNode = new ResourceDataNode(nativeDataNode.Name, resourceFileRef);
                }
                resourceDataNode.Comment = nativeDataNode.Comment;
            }

            return resourceDataNode;
        }
        #endregion
    }
}
