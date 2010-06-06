//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//===============================================================================

using System;
using System.IO;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Security.Permissions;

namespace ResourceManager
{
    /// <summary>
    /// Common resource set to expand the features provided by <see cref="T:ResourceSet"/> 
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class CommonResourceSet : ResourceSet
    {
        #region Properties
        /// <summary>
        /// Gets the number of resources in the set.
        /// </summary>
        /// <value>The number of resources.</value>
        public int Count
        {
            get { return base.Table.Count; }
        }

        /// <summary>
        /// Gets the internal resource table.
        /// </summary>
        /// <value>The resource table.</value>
        public Hashtable Resources
        {
            get { return base.Table; }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonResourceSet"/> class.
        /// </summary>
        /// <param name="resourceReader">The resource reader.</param>
        public CommonResourceSet(IResourceReader resourceReader)
            : base(resourceReader)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonResourceSet"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public CommonResourceSet(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonResourceSet"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public CommonResourceSet(string fileName)
            : base(fileName)
        {
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Creates the default resource reader.
        /// </summary>
        /// <returns>IResourceReader instance</returns>
        public virtual IResourceReader CreateDefaultReader()
        {
            Reader.GetEnumerator().Reset();
            return Reader;
        }

        /// <summary>
        /// Creates the default resource writer.
        /// </summary>
        /// <returns>IResourceWriter instance</returns>
        public virtual IResourceWriter CreateDefaultWriter()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
