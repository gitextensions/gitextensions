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

using System.Text;

namespace ResourceManager
{
    /// <summary>
    /// Interface for managing resource file references
    /// </summary>
    public interface IResourceFileRef
    {
        #region Properties
        /// <summary>
        /// FileName is the name of the file in the file reference
        /// </summary>
        string FileName { get; set; }
        /// <summary>
        /// TypeName is the string of the type of data in the file
        /// </summary>
        string TypeName { get; set; }
        /// <summary>
        /// TextFileEncoding is the encoding mechanism of the file if the file is a text file
        /// </summary>
        Encoding TextFileEncoding { get; set; }
        #endregion
    }
}
