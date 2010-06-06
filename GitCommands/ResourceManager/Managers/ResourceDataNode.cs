//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//
// Original Author: Guy Smith-Ferrier
// Date Created   : September 2005
// Notes          : This file is part of the downloadable source code for .NET Internationalization, by
//                  Guy Smith-Ferrier, published by Addison-Wesley. See http://www.dotneti18n.com for details.
// Disclaimer     : No warranty is provided. Use at your own risk.
// Modification   : You are free to modify this code providing that this block of comments is not altered
//                  and is placed before any code.
//===============================================================================

using System;

namespace ResourceManager
{
    /// <summary>
    /// ResourceDataNode is a generic implementation of a resource data node
    /// </summary>
    /// <remarks>ResourceDataNode is suitable for use with resource managers which need to support
    /// data nodes but which do not have a data node class (such as ResXDataNode)</remarks>
    [Serializable]
    public class ResourceDataNode : IResourceDataNode
    {
        #region Fields
        private string comment;
        private string name;
        private string typeName;
        private object dataValue;
        private IResourceFileRef fileRef;
        #endregion

        #region Properties
        /// <summary>
        /// A comment about the resource entry
        /// </summary>
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        /// <summary>
        /// The name of the resource entry
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// the name of the type.
        /// </summary>
        public string TypeName
        {
            get { return typeName; }
        }

        /// <summary>
        /// The value of the resource entry
        /// </summary>
        public object Value
        {
            get { return dataValue; }
            set { dataValue = value; }
        }

        /// <summary>
        /// The IResourceFileRef of the resource entry
        /// </summary>
        public IResourceFileRef FileRef
        {
            get { return fileRef; }
        }
        #endregion

        #region Construction
        /// <summary>
        /// Constructs a ResourceDataNode from an object
        /// </summary>
        /// <param name="name">The name of the resource entry</param>
        /// <param name="value">The value of the resource entry</param>
        public ResourceDataNode(string name, object value)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Type valueType;
            if (value == null)
                valueType = typeof(object);
            else
                valueType = value.GetType();

            this.name = name;
            this.typeName = valueType.AssemblyQualifiedName;

            IResourceFileRef resourceFileRef = value as IResourceFileRef;
            if (resourceFileRef == null)
                this.dataValue = value;
            else
                this.fileRef = resourceFileRef;
        }
        #endregion
    }
}
