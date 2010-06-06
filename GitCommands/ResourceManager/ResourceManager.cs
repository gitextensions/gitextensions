using System;
using System.IO;
using System.Globalization;
using System.Resources;
using System.Diagnostics.CodeAnalysis;

namespace ResourceManager
{
    public class ResourceManager : IResourceManager
    {
        #region Fields
        private string name;
        private ExtendedComponentResourceManager resourceManager;
        private CultureInfo cultureInfo;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the resource manager name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets or sets the culture info.
        /// </summary>
        /// <value>A <see cref="CultureInfo" /> object.</value>
        /// <remarks>The CultureInfo describes the language to use with respect to resources returned by the ResourceManager.</remarks>
        public CultureInfo CultureInfo
        {
            get
            {
                return cultureInfo;
            }
            set
            {
                cultureInfo = value;
            }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceManager"/> class.
        /// This is a wrapper for a <see cref="ExtendedComponentResourceManager"/> class to replace
        /// the "get" resource methods with indexers and provide a text replacement feature
        /// </summary>
        /// <param name="name">The resource manager name.</param>
        /// <param name="resourceManager">The resource manager.</param>
        /// <remarks>During the initialisation process a <see cref="ResourceManager"/> is retrieved from
        /// cache or generated new from the configured provider</remarks>
        internal ResourceManager(string name, ExtendedComponentResourceManager resourceManager)
        {
            this.name = name;
            this.resourceManager = resourceManager;
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Indexes the Resource by key
        /// </summary>
        /// <value>String resource value</value>
        /// <overloads>Resource index to look up a named resource using a string key</overloads>
        /// <remarks>
        /// 	<para>If a match is not possible, with the given key, then a null reference is returned.</para>
        /// 	<para>The current thread culture is used to determine the most appropriate language, unless overidden by the CultureInfo property</para>
        /// </remarks>
        /// <example>
        /// This example gets a string message from a resource with a configured name of "Resource Manager".
        /// <code>
        /// public string GetMessage()
        /// {
        ///  ResourceManager rm = ResourceFactory.GetResourceManager("Resource Manager");
        ///  string s = rm["MSG_HELLO_WORLD"];
        ///  return s;
        /// }
        /// </code>
        /// This might return something like "Hello World"
        /// </example>
        public string this[string key]
        {
            get { return resourceManager.GetString(key, cultureInfo); }
        }

        /// <summary>
        /// Overloaded indexer allows for embedded markers to be replaced with the arguments passed in args
        /// </summary>
        /// <value>String resource value with embedded markers replaced</value>
        /// <remarks>
        /// 	<para>markers are embedded in the format <c>{n}</c> where n is a sequential integer matching the index of the <c>args</c> parameter</para>
        /// 	<para>If a match is not possible, with the given key, then a null reference is returned</para>
        /// 	<para>The current thread culture is used to determine the most appropriate language, unless overidden by the CultureInfo property</para>
        /// </remarks>
        /// <example>
        /// This example gets a string message from resource and replaces a marker from a resource with a configured name of "Message Resource Manager".
        /// <code>
        /// public string GetReplaceMessage()
        /// {
        ///  ResourceManager rm = ResourceFactory.GetResourceManager(Message Resource Manager);
        ///  string replaceString = "Stevenage";
        ///  string s = rm["MSG_HELLO_WORLD", replaceString];
        ///  return s;
        /// }
        /// </code>
        /// If the message was "Hello {0}", This would return "Hello Stevenage"
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        public string this[string key, params object[] args]
        {
            get
            {
                string resourceValue = resourceManager.GetString(key, cultureInfo);

                if (resourceValue == null)
                    return null;
                else
                    return String.Format(cultureInfo, resourceValue, args);
            }
        }

        /// <summary>
        /// Overloaded indexer to look up a named resource using a string key, object type and return an object
        /// </summary>
        /// <value>Object resource value</value>
        /// <remarks>
        /// 	<para>If a match is not possible, with the given key and type, then a null reference is returned</para>
        /// 	<para>The current thread culture is used to determine the most appropriate language, unless overidden by the CultureInfo property</para>
        /// 	<para>The <see cref="Type" /> parameter is used to verify the type of the returned object</para>
        /// </remarks>
        /// <example>
        /// This example gets an object from resource and casts it as an integer from a resource with a configured name of "Object Resource Manager".
        /// <code>
        /// public int GetResourceFlag()
        /// {
        ///  ResourceManager rm = ResourceFactory.GetResourceManager("Object Resource Manager");
        ///  Object o = rm["INT_MAX_MESSAGE_LENGTH", System.Int32];
        ///  return (int)o;
        /// }
        /// </code>
        /// </example>
        /// <example>
        /// This example gets an audio object from resource and casts it as an <see cref="UnmanagedMemoryStream" /> from a resource with a configured name of "UMS Resource Manager".
        /// <code>
        /// public UnmanagedMemoryStream GetResourceAudio()
        /// {
        ///  ResourceManager rm = ResourceFactory.GetResourceManager("UMS Resource Manager");
        ///  Object o = rm["AUDIO_INTRO", System.IO.UnmanagedMemoryStream];
        ///  return (UnmanagedMemoryStream)o;
        /// }
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        public object this[string key, Type type]
        {
            get
            {
                if (type == null)
                    throw new ArgumentNullException("type");

                object resourceValue;

                if (type == typeof(UnmanagedMemoryStream))
                    resourceValue = resourceManager.GetStream(key, cultureInfo);
                else
                    resourceValue = resourceManager.GetObject(key, cultureInfo);

                if (resourceValue == null)
                    return null;
                else
                    if (type.IsInstanceOfType(resourceValue))
                        return resourceValue;
                    else
                        throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Invalid resourcetype: {0}, {1}", key, type.Name), "key");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the string resource for the given key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>string resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        public string GetString(string key)
        {
            return GetString(key, null);
        }

        /// <summary>
        /// Gets the string resource for the given key and culture.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>string resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        public string GetString(string key, CultureInfo culture)
        {
            return resourceManager.GetString(key, culture);
        }

        /// <summary>
        /// Gets the object resource for the given key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>object resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        public object GetObject(string key)
        {
            return GetObject(key, null);
        }

        /// <summary>
        /// Gets the object resource for the given key and culture.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>object resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        public object GetObject(string key, CultureInfo culture)
        {
            return resourceManager.GetObject(key, culture);
        }

        /// <summary>
        /// Gets the unmanaged memory stream resource for the given key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>an unmanaged stream resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        public UnmanagedMemoryStream GetStream(string key)
        {
            return GetStream(key, null);
        }

        /// <summary>
        /// Gets the unmanaged memory stream resource for the given key and culture.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>an unmanaged stream resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        public UnmanagedMemoryStream GetStream(string key, CultureInfo culture)
        {
            return resourceManager.GetStream(key, culture);
        }

        /// <summary>
        /// Gets the resource set.
        /// </summary>
        /// <returns></returns>
        public ResourceSet GetResourceSet()
        {
            CultureInfo culture = cultureInfo;
            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            return GetResourceSet(culture);
        }

        /// <summary>
        /// Gets the resource set.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public ResourceSet GetResourceSet(CultureInfo culture)
        {
            if (culture == null)
                culture = CultureInfo.InvariantCulture;

            return resourceManager.GetResourceSet(culture, true, true);
        }

        /// <summary>
        /// Applies resources to all localizable properties of an object.
        /// </summary>
        /// <param name="value">The object itself.</param>
        /// <param name="objectName">the name of the object.</param>
        public void ApplyResources(object value, string objectName)
        {
            ApplyResources(value, objectName, null);
        }

        /// <summary>
        /// Applies resources to all localizable properties of an object.
        /// </summary>
        /// <param name="value">The object itself.</param>
        /// <param name="objectName">the name of the object.</param>
        /// <param name="culture">The culture.</param>
        public void ApplyResources(object value, string objectName, CultureInfo culture)
        {
            resourceManager.ApplyResources(value, objectName, culture);
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void ReleaseAllResources()
        {
            resourceManager.ReleaseAllResources();
        }
        #endregion
    }
}