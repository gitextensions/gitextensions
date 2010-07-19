using System.IO;

namespace GitStatistics
{
    /// <summary>
    ///   Represents a .NET file containing code: a .vb, .cs, .cpp, or .h file here.
    /// </summary>
    public class CodeFile
    {
        private readonly bool _isDesignerFile;
        protected FileInfo File;
        protected int NumberCodeFiles;
        protected string RelativeFileName = "";
        private bool _inCodeGeneratedRegion;
        private bool _inCommentBlock;

        internal CodeFile(string fullName)
        {
            File = new FileInfo(fullName);
            _isDesignerFile = IsDesignerFile();
            IsTestFile = false;
        }

        internal CodeFile(string fullName, string relativeName)
        {
            File = new FileInfo(fullName);
            RelativeFileName = relativeName;
            _isDesignerFile = IsDesignerFile();
            IsTestFile = false;
        }

        protected internal int NumberLines { get; protected set; }

        protected internal int NumberBlankLines { get; protected set; }

        protected internal int NumberLinesInDesignerFiles { get; protected set; }

        protected internal int NumberCommentsLines { get; protected set; }

        internal bool IsTestFile { get; private set; }


        private bool IsDesignerFile()
        {
            return
                IsWebReferenceFile() ||
                File.Name.Contains(".Designer.") ||
                File.Name.Contains(".designer.");
        }

        private bool IsWebReferenceFile()
        {
            return File.FullName.Contains(@"\Web References\") &&
                   File.Name == "Reference.cs"; // Ugh
        }

        public void CountLines()
        {
            InitializeCountLines();
            if (!File.Exists)
                return;

            using (var sr = new StreamReader(File.FullName, true))
            {
                try
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line != null)
                            IncrementLineCountsFromLine(line.Trim());
                    }
                }
                finally
                {
                    sr.Close();
                }
            }
        }

        private void InitializeCountLines()
        {
            SetLineCountsToZero();
            NumberCodeFiles = 1;
            _inCodeGeneratedRegion = false;
            _inCommentBlock = false;
        }

        protected void SetLineCountsToZero()
        {
            NumberLines = 0;
            NumberBlankLines = 0;
            NumberLinesInDesignerFiles = 0;
            NumberCommentsLines = 0;
            NumberCodeFiles = 0;
        }

        private void IncrementLineCountsFromLine(string line)
        {
            SetCodeBlockFlags(line);

            NumberLines++;
            if (_inCodeGeneratedRegion || _isDesignerFile)
                NumberLinesInDesignerFiles++;
            else if (line == "")
                NumberBlankLines++;
            else if (_inCommentBlock || line.StartsWith("'") || line.StartsWith(@"//"))
                NumberCommentsLines++;

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
                _inCodeGeneratedRegion = true;
            if (line.StartsWith("/*"))
                _inCommentBlock = true;
            if (!_inCommentBlock && !_inCodeGeneratedRegion && (
                                                                   line.StartsWith("[Test")
                                                               ))
            {
                IsTestFile = true;
            }
        }

        private void ResetCodeBlockFlags(string line)
        {
            if (_inCodeGeneratedRegion && (line.Contains("#endregion") || line.Contains("#End Region")))
                _inCodeGeneratedRegion = false;
            if (_inCommentBlock && line.Contains("*/"))
                _inCommentBlock = false;
        }

        public bool CheckValidExtension(string fileName)
        {
            return true;
            //return fileName.EndsWith(".cs") || fileName.EndsWith(".vb") ||
            //    fileName.EndsWith(".cpp") || fileName.EndsWith(".h") || fileName.EndsWith(".hpp");
        }
    }
}