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
using System.Text;

namespace ResourceManager
{
    /// <summary>
    /// ResourceFileRef manages resource file references
    /// </summary>
    [Serializable]
    public class ResourceFileRef : IResourceFileRef
    {
        #region Fields
        private string fileName;
        private string typeName;
        private Encoding textFileEncoding;
        #endregion

        #region Construction
        /// <summary>
        /// Constructs a ResourceFileRef object
        /// </summary>
        /// <param name="fileName">The name of the file in the file reference</param>
        /// <param name="typeName">The string of the type of data in the file</param>
        public ResourceFileRef(string fileName, string typeName)
        {
            this.fileName = fileName;
            this.typeName = typeName;
        }

        /// <summary>
        /// Constructs a ResourceFileRef object
        /// </summary>
        /// <param name="fileName">The name of the file in the file reference</param>
        /// <param name="typeName">The string of the type of data in the file</param>
        /// <param name="textFileEncoding">The encoding mechanism of the file if the file is a text file</param>
        public ResourceFileRef(string fileName, string typeName, Encoding textFileEncoding)
            : this(fileName, typeName)
        {
            this.textFileEncoding = textFileEncoding;
        }
        #endregion

        #region Properties
        /// <summary>
        /// FileName is the name of the file in the file reference
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        /// <summary>
        /// TypeName is the string of the type of data in the file
        /// </summary>
        public string TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }

        /// <summary>
        /// TextFileEncoding is the encoding mechanism of the file if the file is a text file
        /// </summary>
        public Encoding TextFileEncoding
        {
            get { return textFileEncoding; }
            set { textFileEncoding = value; }
        }
        #endregion
    }
}
