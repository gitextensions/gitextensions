using System;
using System.IO;

namespace GitCommands
{
    class PathUtil
    {
        /// <summary>
        /// Code guideline: "A directory path should always end with / or \.
        /// Better use Path.Combine instead of Setting.PathSeparator"
        /// 
        /// This method can be used to add (or keep) a trailing path separator character to a directory path.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string EnsureTrailingPathSeparator(string dirPath)
        {
            if (!dirPath.IsNullOrEmpty() && !dirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                dirPath += Path.DirectorySeparatorChar;
            }

            return dirPath;
        }
    }
}
