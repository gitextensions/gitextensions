using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GitStatistics
{
    /// <summary>
    /// Represents a .NET file containing code: a .vb, .cs, .cpp, or .h file here.
    /// </summary>
    public class CodeFile
    {
        #region Member variables
        protected FileInfo file;
        protected string relativeFileName = "";
        protected int numberLines = 0;
        protected int numberBlankLines = 0;
        protected int numberLinesInDesignerFiles = 0;
        protected int numberCommentsLines = 0;
        protected int numberCodeFiles = 0;

        #endregion

        #region Getters
        internal int NumberLines { get { return numberLines; } }
        internal int NumberBlankLines { get { return numberBlankLines; } }
        internal int NumberLinesInDesignerFiles { get { return numberLinesInDesignerFiles; } }
        internal int NumberCommentsLines { get { return numberCommentsLines; } }
        internal bool IsTestFile { get { return isTestFile; } }
        #endregion



        bool isDesignerFile = false;
        bool isTestFile = false;

        #region Constructors
        internal CodeFile(string fullName)
        {
            file = new FileInfo(fullName);
            isDesignerFile = IsDesignerFile();
            isTestFile = false;
        }
        internal CodeFile(string fullName, string relativeName)
        {
            file = new FileInfo(fullName);
            this.relativeFileName = relativeName;
            isDesignerFile = IsDesignerFile();
            isTestFile = false;
        }
        private bool IsDesignerFile()
        {
            bool isWebReferenceFile = file.FullName.Contains(@"\Web References\") && file.Name == "Reference.cs";  // Ugh
            return isWebReferenceFile || file.Name.Contains(".Designer.") || file.Name.Contains(".designer.");
        }
        #endregion

        #region Count lines method
        bool inCodeGeneratedRegion = false;
        bool inCommentBlock = false;
        public void CountLines()
        {
            InitializeCountLines();
            if (file.Exists)
            {
                StreamReader sr = new StreamReader(file.FullName, true);
                try
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        IncrementLineCountsFromLine(line);
                    }
                }
                finally
                {
                    if (sr != null) sr.Close();
                }

            }

            if (isTestFile)
            {
            }
        }

        private void InitializeCountLines()
        {
            SetLineCountsToZero();
            numberCodeFiles = 1;
            inCodeGeneratedRegion = false;
            inCommentBlock = false;
        }

        protected void SetLineCountsToZero()
        {
            numberLines = 0;
            numberBlankLines = 0;
            numberLinesInDesignerFiles = 0;
            numberCommentsLines = 0;
            numberCodeFiles = 0;
        }

        private void IncrementLineCountsFromLine(string line)
        {
            SetCodeBlockFlags(line);

            this.numberLines++;
            if (inCodeGeneratedRegion || this.isDesignerFile)
                this.numberLinesInDesignerFiles++;
            else if (line == "")
                this.numberBlankLines++;
            else if (inCommentBlock || line.StartsWith("'") || line.StartsWith(@"//"))
                this.numberCommentsLines++;

            ResetCodeBlockFlags(line);
        }

        private void SetCodeBlockFlags(string line)
        {
            // The number of code-generated lines is an approximation at best, particularly
            // with VS 2003 code.  Change code here if you don't like the way it's working.
            // if (line.Contains("Designer generated code") // Might be cleaner
            if (line.StartsWith("#region Windows Form Designer generated code") ||
                line.StartsWith("#Region \" Windows Form Designer generated code") ||
                line.StartsWith("#region Component Designer generated code") ||
                line.StartsWith("#Region \" Component Designer generated code \"") ||
                line.StartsWith("#region Web Form Designer generated code") ||
                line.StartsWith("#Region \" Web Form Designer Generated Code \"")
                )
                inCodeGeneratedRegion = true;
            if (line.StartsWith("/*"))
                inCommentBlock = true;
            if (!inCommentBlock && !inCodeGeneratedRegion && (
                line.StartsWith("[Test")
                ))
            {
                isTestFile = true;
            }

        }

        private void ResetCodeBlockFlags(string line)
        {
            if (inCodeGeneratedRegion && (line.Contains("#endregion") || line.Contains("#End Region")))
                inCodeGeneratedRegion = false;
            if (inCommentBlock && line.Contains("*/"))
                inCommentBlock = false;
        }
        #endregion


        public bool CheckValidExtension(string fileName)
        {
            return true;
            //return fileName.EndsWith(".cs") || fileName.EndsWith(".vb") ||
            //    fileName.EndsWith(".cpp") || fileName.EndsWith(".h") || fileName.EndsWith(".hpp");
        }
    }
}
