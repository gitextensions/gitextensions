using System;
using System.Collections;
using System.Globalization;
using System.Resources;

namespace ResourceManager
{
    /// <summary>
    /// Custom abstract resource manager for managing resources held in files
    /// </summary>
    public abstract class FileResourceManager : ExtendedComponentResourceManager
    {
        #region Fields
        private string pathName;
        private string baseName;
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="FileResourceManager"/> class.
        /// </summary>
        /// <param name="pathName">the file path to the resource files.</param>
        /// <param name="baseName">base name for the set of resources.</param>
        protected FileResourceManager(string pathName, string baseName)
        {
            Initialize(pathName, baseName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileResourceManager"/> class.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        protected FileResourceManager(Type resourceType)
        {
            if (resourceType == null)
                throw new ArgumentNullException("resourceType");



            Initialize(resourceType.Assembly.Location.Substring(0, resourceType.Assembly.Location.LastIndexOf("\\")) + "\\resources", resourceType.Name);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Initializes the resource manager.
        /// </summary>
        /// <param name="filePathName">Name of the file path.</param>
        /// <param name="fileBaseName">Name of the file base.</param>
        protected void Initialize(string filePathName, string fileBaseName)
        {
            this.pathName = filePathName;
            this.baseName = fileBaseName;
            this.ResourceSets = new Hashtable();
        }

        /// <summary>
        /// Provides the implementation for finding a <see cref="T:System.Resources.ResourceSet"></see>.
        /// </summary>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to look for.</param>
        /// <param name="createIfNotExists">If true and if the <see cref="T:System.Resources.ResourceSet"></see> has not been loaded yet, load it.</param>
        /// <param name="tryParents">If the <see cref="T:System.Resources.ResourceSet"></see> cannot be loaded, try parent <see cref="T:System.Globalization.CultureInfo"></see> objects to see if they exist.</param>
        /// <returns>
        /// The specified <see cref="T:System.Resources.ResourceSet"></see>.
        /// </returns>
        /// <exception cref="T:System.Resources.MissingManifestResourceException">The database contains no resources or fallback resources for the culture given and it is required to look up a resource. </exception>
        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            CommonResourceSet resourceSet = null;

            // check the resource set cache first
            if (ResourceSets.Contains(culture.Name))
                resourceSet = ResourceSets[culture.Name] as CommonResourceSet;
            else
            {
                // create a new resource set
                resourceSet = CreateResourceSet(pathName, baseName, culture);
                // check the number of resources returned
                if (resourceSet.Count == 0)
                {
                    // try the parent culture if not already at the invariant culture
                    if (tryParents)
                    {
                        //if (culture.Equals(CultureInfo.InvariantCulture))
                        //    throw new MissingManifestResourceException(this.baseName + Environment.NewLine + this.pathName + Environment.NewLine + culture.Name);

                        if (!culture.Equals(CultureInfo.InvariantCulture))
                            // do a recursive call on this method with the parent culture
                            resourceSet = this.InternalGetResourceSet(culture.Parent, createIfNotExists, tryParents) as CommonResourceSet;
                    }
                }
                else
                {
                    // only cache the resource if the createIfNotExists flag is set
                    if (createIfNotExists)
                        ResourceSets.Add(culture.Name, resourceSet);
                }
            }
            return resourceSet;
        }

        /// <summary>
        /// Creates the resource set.
        /// </summary>
        /// <param name="filePathName">Name of the file path.</param>
        /// <param name="fileBaseName">Name of the file base.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>a common resource set type</returns>
        protected abstract CommonResourceSet CreateResourceSet(string filePathName, string fileBaseName, CultureInfo cultureInfo);
        #endregion
    }
}
