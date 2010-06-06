//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//===============================================================================

using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Security.Permissions;

namespace ResourceManager
{
    /// <summary>
    /// Wrapper class for the ResXResourceWriter to handle an
    /// IResourceDataNode for consistency between all writers
    /// </summary>
    /// <remarks>The wrapper also enables additional parameter checking in the constructors</remarks>
    public sealed class XmlResourceWriter : IResourceWriter
    {
        #region Field
        private ResXResourceWriter resourceWriter;
        /// <summary>
        /// Specifies the default content type for a binary object. This field is read-only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string BinSerializedObjectMimeType = ResXResourceWriter.BinSerializedObjectMimeType;
        /// <summary>
        /// Specifies the default content type for a byte array object. This field is read-only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string ByteArraySerializedObjectMimeType = ResXResourceWriter.ByteArraySerializedObjectMimeType;
        /// <summary>
        /// Specifies the default content type for an object. This field is read-only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string DefaultSerializedObjectMimeType = ResXResourceWriter.DefaultSerializedObjectMimeType;
        /// <summary>
        /// Specifies the content type of an XML resource. This field is read-only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string ResMimeType = ResXResourceWriter.ResMimeType;
        /// <summary>
        /// Specifies the schema to use in writing the XML file. This field is read-only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string ResourceSchema = ResXResourceWriter.ResourceSchema;
        /// <summary>
        /// Specifies the content type for a SOAP object. This field is read-only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string SoapSerializedObjectMimeType = ResXResourceWriter.SoapSerializedObjectMimeType;
        /// <summary>
        /// Specifies the version of the schema that the XML output conforms to. This field is read-only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string Version = ResXResourceWriter.Version;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the base path.
        /// </summary>
        /// <value>The base path.</value>
        public string BasePath
        {
            get
            {
                new SecurityPermission(PermissionState.Unrestricted).Demand();
                return resourceWriter.BasePath;
            }
            set
            {
                new SecurityPermission(PermissionState.Unrestricted).Demand();
                resourceWriter.BasePath = value;
            }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResourceWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public XmlResourceWriter(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter = new ResXResourceWriter(stream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResourceWriter"/> class.
        /// </summary>
        /// <param name="textWriter">The <see cref="T:System.IO.TextWriter"></see> object to send the output to.</param>
        public XmlResourceWriter(TextWriter textWriter)
        {
            if (textWriter == null)
                throw new ArgumentNullException("textWriter");

            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter = new ResXResourceWriter(textWriter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResourceWriter"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public XmlResourceWriter(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter = new ResXResourceWriter(fileName);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds the specified alias to a list of aliases.
        /// </summary>
        /// <param name="aliasName">The name of the alias.</param>
        /// <param name="assemblyName">The name of the assembly represented by aliasName.</param>
        public void AddAlias(string aliasName, AssemblyName assemblyName)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.AddAlias(aliasName, assemblyName);
        }

        /// <summary>
        /// Adds a design-time property whose value is specified as an object to the list of resources to write.
        /// </summary>
        /// <param name="name">The name of a property.</param>
        /// <param name="value">An object that is the value of the property to add.</param>
        public void AddMetadata(string name, object value)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.AddMetadata(name, value);
        }

        /// <summary>
        /// Adds a design-time property whose value is specified as a string to the list of resources to write.
        /// </summary>
        /// <param name="name">The name of a property.</param>
        /// <param name="value">An object that is the value of the property to add.</param>
        public void AddMetadata(string name, string value)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.AddMetadata(name, value);
        }

        /// <summary>
        /// Adds a design-time property whose value is specified as a byte array to the list of resources to write.
        /// </summary>
        /// <param name="name">The name of a property.</param>
        /// <param name="value">An object that is the value of the property to add.</param>
        public void AddMetadata(string name, byte[] value)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.AddMetadata(name, value);
        }

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="resourceDataNode">The resource data node.</param>
        private void AddResource(IResourceDataNode resourceDataNode)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            if (resourceDataNode == null)
                throw new ArgumentNullException("resourceDataNode");

            ResXDataNode node = null;
            if (resourceDataNode.FileRef == null)
                node = new ResXDataNode(resourceDataNode.Name, resourceDataNode.Value);
            else
            {
                ResXFileRef fileRef = null;
                if (resourceDataNode.FileRef.TextFileEncoding == null)
                    fileRef = new ResXFileRef(resourceDataNode.FileRef.FileName, resourceDataNode.FileRef.TypeName);
                else
                    fileRef = new ResXFileRef(resourceDataNode.FileRef.FileName, resourceDataNode.FileRef.TypeName, resourceDataNode.FileRef.TextFileEncoding);
                node = new ResXDataNode(resourceDataNode.Name, fileRef);
            }
            node.Comment = resourceDataNode.Comment;

            resourceWriter.AddResource(node);
        }

        /// <summary>
        /// Adds a named resource specified as an object to the list of resource to be written.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="value">The value.</param>
        public void AddResource(string name, object value)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            IResourceDataNode dataNode = value as IResourceDataNode;
            if (dataNode != null)
                this.AddResource(dataNode);
            else
                resourceWriter.AddResource(name, value);
        }

        /// <summary>
        /// Adds a named resource specified as a string to the list of resource to be written.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="value">The value.</param>
        public void AddResource(string name, string value)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.AddResource(name, value);
        }

        /// <summary>
        /// Adds a named resource specified as a byte array to the list of resource to be written.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="value">The value.</param>
        public void AddResource(string name, byte[] value)
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.AddResource(name, value);
        }

        /// <summary>
        /// Saves all resources to the output stream in the system default format.
        /// </summary>
        public void Generate()
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.Generate();
        }

        /// <summary>
        /// Saves the resources to the output stream and then closes it.
        /// </summary>
        public void Close()
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.Close();
        }

        /// <summary>
        /// Allow users to save the resources to the output resource file or stream before closing it,
        /// explicitly releasing resources.
        /// </summary>
        public void Dispose()
        {
            new SecurityPermission(PermissionState.Unrestricted).Demand();
            resourceWriter.Dispose();
        }
        #endregion
    }
}
