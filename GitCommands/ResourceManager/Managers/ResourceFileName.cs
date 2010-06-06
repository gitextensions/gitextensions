//===============================================================================
// Microsoft patterns & practices Enterprise Library Contribution
// Resource Application Block
//===============================================================================

using System;
using System.IO;
using System.Globalization;

namespace ResourceManager
{
    /// <summary>
    /// Resource file name helper class
    /// </summary>
    public static class ResourceFileName
    {
        #region Public Methods
        /// <summary>
        /// Builds the specified resource file name.
        /// </summary>
        /// <param name="pathName">The path to the resource file.</param>
        /// <param name="baseName">resource file base name.</param>
        /// <param name="cultureInfo">The resource culture.</param>
        /// <param name="extension">The resource file extension.</param>
        /// <returns></returns>
        public static string Build(string pathName, string baseName, CultureInfo cultureInfo, string extension)
        {
            CultureInfo culture;
            string fileName;

            if (pathName == null)
                throw new ArgumentNullException("pathName");

            if (String.IsNullOrEmpty(baseName))
                throw new ArgumentNullException("baseName");

            if (cultureInfo == null)
                culture = CultureInfo.InvariantCulture;
            else
                culture = cultureInfo;

            if (String.IsNullOrEmpty(extension))
                throw new ArgumentNullException("extension");

            fileName = baseName;
            if (culture.Equals(CultureInfo.InvariantCulture))
                fileName += extension;
            else
                fileName += "." + culture.Name + extension;

            return Path.Combine(pathName, fileName);
        }
        #endregion
    }
}
