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

namespace ResourceManager
{
    /// <summary>
    /// IResourceDataNode is an interface for describing a resources data node
    /// </summary>
    public interface IResourceDataNode
    {
        #region Properties
        /// <summary>
        /// A comment about the resource entry
        /// </summary>
        string Comment { get; set; }
        /// <summary>
        /// The name of the resource entry
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        string TypeName { get; }
        /// <summary>
        /// The value of the resource entry
        /// </summary>
        object Value { get; set; }
        /// <summary>
        /// The IResourceFileRef of the resource entry
        /// </summary>
        IResourceFileRef FileRef { get; }
        #endregion
    }
}
