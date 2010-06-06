using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Diagnostics.CodeAnalysis;

namespace ResourceManager
{
    /// <summary>
    /// This interface defines the contract that must be implemented by all resource managers. 
    /// </summary>
    public interface IResourceManager
    {
        #region Properties
        /// <summary>
        /// Gets the resource manager instance name.
        /// </summary>
        /// <value>The name.</value>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets or sets the culture info.
        /// </summary>
        /// <value>A <see cref="CultureInfo" /> object.</value>
        /// <remarks>The CultureInfo describes the language to use with respect to resources returned by the ResourceManager.</remarks>
        CultureInfo CultureInfo
        {
            get;
            set;
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
        string GetString(string key);

        /// <summary>
        /// Gets the string resource for the given key and culture.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>string resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        string GetString(string key, CultureInfo culture);

        /// <summary>
        /// Gets the object resource for the given key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>object resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        object GetObject(string key);

        /// <summary>
        /// Gets the object resource for the given key and culture.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>object resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        object GetObject(string key, CultureInfo culture);

        /// <summary>
        /// Gets the unmanaged memory stream resource for the given key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>an unmanaged stream resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        UnmanagedMemoryStream GetStream(string key);

        /// <summary>
        /// Gets the unmanaged memory stream resource for the given key and culture.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>an unmanaged stream resource value</returns>
        /// <remarks>this method is exposed for compatibility reasons only,
        /// it is recommended that you use indexers when you can.</remarks>
        UnmanagedMemoryStream GetStream(string key, CultureInfo culture);

        /// <summary>
        /// Gets the resource set.
        /// </summary>
        /// <returns></returns>
        ResourceSet GetResourceSet();

        /// <summary>
        /// Gets the resource set.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        ResourceSet GetResourceSet(CultureInfo culture);

        /// <summary>
        /// Applies resources to all localizable properties of an object.
        /// </summary>
        /// <param name="value">The object itself.</param>
        /// <param name="objectName">the name of the object.</param>
        void ApplyResources(object value, string objectName);

        /// <summary>
        /// Applies resources to all localizable properties of an object.
        /// </summary>
        /// <param name="value">The object itself.</param>
        /// <param name="objectName">the name of the object.</param>
        /// <param name="culture">The culture.</param>
        void ApplyResources(object value, string objectName, CultureInfo culture);

        /// <summary>
        /// Releases all resources.
        /// </summary>
        void ReleaseAllResources();
        #endregion

        #region Indexers
        /// <summary>
        /// Indexes the Resource by key
        /// </summary>
        /// <param name="key">key to retrieve from resource</param>
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
        ///		ResourceManager rm = ResourceFactory.GetResourceManager("Resource Manager");
        ///		string s = rm["MSG_HELLO_WORLD"];
        ///		return s;
        /// }
        /// </code>
        /// This might return something like "Hello World"
        /// </example>
        string this[string key] { get; }

        /// <summary>
        /// Overloaded indexer allows for embedded markers to be replaced with the arguments passed in args
        /// </summary>
        /// <param name="key">key to retrieve from resource</param>
        /// <param name="args">a variable number of arguments used as replacement strings</param>
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
        string this[string key, params object[] args] { get; }

        /// <summary>
        /// Overloaded indexer to look up a named resource using a string key, object type and return an object
        /// </summary>
        /// <param name="key">key to retrieve from resource</param>
        /// <param name="type">the type of the resource used to verify the type of resource</param>
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
        object this[string key, Type type] { get; }
        #endregion
    }
}