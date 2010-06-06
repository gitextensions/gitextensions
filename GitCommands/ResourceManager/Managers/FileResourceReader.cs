//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//===============================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Resources;
using System.Security.Permissions;
using System.Collections;
using System.Globalization;

namespace ResourceManager
{
    /// <summary>
    /// Custom abstract Resource Reader to read resources from a file
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class FileResourceReader : IResourceReader
    {
        #region Fields
        private string pathName;
        private string baseName;
        private CultureInfo cultureInfo;
        private string extension;
        private bool useDataNodes;
        private bool disposed;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether to use data node values from the reader.
        /// </summary>
        /// <value><c>true</c> if the reader is to use data nodes; otherwise, <c>false</c>.</value>
        public bool UseDataNodes
        {
            get { return useDataNodes; }
            set { useDataNodes = value; }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DataResourceReader"/> class.
        /// </summary>
        /// <param name="pathName">the file path to the resource files.</param>
        /// <param name="baseName">the base name for the set of resources.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="extension">The extension.</param>
        /// <remarks>Inheritance classes should pass through an extension but not expose it
        /// publically</remarks>
        protected FileResourceReader(string pathName, string baseName, CultureInfo cultureInfo, string extension)
        {
            if (String.IsNullOrEmpty(pathName))
                throw new ArgumentNullException("pathName");

            if (String.IsNullOrEmpty(baseName))
                throw new ArgumentNullException("baseName");

            if (cultureInfo == null)
                cultureInfo = CultureInfo.CurrentCulture;

            if (String.IsNullOrEmpty(extension))
                throw new ArgumentNullException("extension");

            this.pathName = pathName;
            this.baseName = baseName;
            this.cultureInfo = cultureInfo;
            this.extension = extension;
        }

        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// </summary>
        /// <remarks>
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </remarks>
        ~FileResourceReader()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates the specific file based resource reader.
        /// </summary>
        /// <param name="fileName">Name of the resources file.</param>
        /// <param name="useResourceDataNodes">if set to <c>true</c> use data nodes.</param>
        /// <returns>Resource reader</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public abstract IResourceReader CreateResourceReader(string fileName, bool useResourceDataNodes);

        /// <summary>
        /// Translates the given data node into an IResourceDataNode type.
        /// </summary>
        /// <param name="dataNode">The data node.</param>
        /// <returns>IResourceDataNode type</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public abstract IResourceDataNode ResourceDataNodeTranslator(IDictionaryEnumerator dataNode);

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> of the resources for this reader.
        /// </summary>
        /// <returns>
        /// A dictionary enumerator for the resources for this reader.
        /// </returns>
        /// <remarks>The advantage of this approach is that the file is released once the resources have been read</remarks>
        public virtual IDictionaryEnumerator GetEnumerator()
        {
            Hashtable hashTable = new Hashtable();
            string fileName = BuildResourceFileName();

            new SecurityPermission(PermissionState.Unrestricted).Demand();
            if (File.Exists(fileName))
            {
                IResourceReader reader = CreateResourceReader(fileName, useDataNodes);
                try
                {
                    IDictionaryEnumerator enumerator = reader.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (useDataNodes)
                            hashTable.Add(enumerator.Key, ResourceDataNodeTranslator(enumerator));
                        else
                            hashTable.Add(enumerator.Key, enumerator.Value);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return hashTable.GetEnumerator();
        }

        /// <summary>
        /// Closes the resource reader after releasing any resources associated with it.
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Gets the name of the resource file.
        /// </summary>
        /// <remarks>This assumes that all file based resources will be of the format
        /// pathname\basename[.culturename].extension where the culture name is missing for the
        /// invariant culture. If this assumption is wrong then the method can be overridden</remarks>
        /// <returns>fully qualified resource file name including the file path</returns>
        protected virtual string BuildResourceFileName()
        {
            return ResourceFileName.Build(pathName, baseName, cultureInfo, extension);
        }
        #endregion

        #region Private Methods
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
        #endregion
    }
}
