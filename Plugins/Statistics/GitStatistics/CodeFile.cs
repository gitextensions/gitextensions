using System.IO;

namespace GitStatistics
{
    /// <summary>
    ///   Represents a .NET file containing code: a .vb, .cs, .cpp, or .h file here.
    /// </summary>
    public class CodeFile
    {
        private readonly bool _isDesignerFile;
        protected int NumberCodeFiles;
        private bool _inCodeGeneratedRegion;
        private bool _inCommentBlock;
        private bool _skipResetFlag;

        internal CodeFile(string fullName)
        {
            File = new FileInfo(fullName);
            _isDesignerFile = IsDesignerFile();
            IsTestFile = false;
        }

        public FileInfo File { get; private set; }

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
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line != null)
                        IncrementLineCountsFromLine(line.Trim());
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
            else if (File.Extension.ToLower() == ".py" && line.StartsWith("#"))
                NumberCommentsLines++;
            else if (File.Extension.ToLower() == ".rb" && line.StartsWith("#"))
                NumberCommentsLines++;
            else if (File.Extension.ToLower() == ".pl" && line.StartsWith("#"))
                NumberCommentsLines++;
            else if (File.Extension.ToLower() == ".lua" && line.StartsWith("--"))
                NumberCommentsLines++;

            if (!_skipResetFlag)
                ResetCodeBlockFlags(line);
        }

        private void SetCodeBlockFlags(string line)
        {
            _skipResetFlag = false;

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

            if (File.Extension.ToLower() == ".pas" || File.Extension.ToLower() == ".inc")
            {
                if (line.StartsWith("(*") && !line.StartsWith("(*$"))
                    _inCommentBlock = true;
                if (line.StartsWith("{") && !line.StartsWith("{$"))
                    _inCommentBlock = true;
            }

            if (File.Extension.ToLower() == ".rb" && line.StartsWith("=begin"))
                _inCommentBlock = true;
            else if (File.Extension.ToLower() == ".pl" && line.StartsWith("=begin"))
                _inCommentBlock = true;
            else if (File.Extension.ToLower() == ".lua" && line.StartsWith("--[["))
                _inCommentBlock = true;

            if (File.Extension.ToLower() == ".py" && !_inCommentBlock)
            {
                if (line.StartsWith("'''") || line.StartsWith("\"\"\""))
                {
                    _inCommentBlock = true;
                    _skipResetFlag = true;
                }
            }

            if (!_inCommentBlock && !_inCodeGeneratedRegion && line.StartsWith("[Test"))
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

            if (File.Extension.ToLower() == ".pas" || File.Extension.ToLower() == ".inc")
            {
                if (line.Contains("*)") || line.Contains("}"))
                    _inCommentBlock = false;
            }

            if (File.Extension.ToLower() == ".rb" && line.Contains("=end"))
                _inCommentBlock = false;
            else if (File.Extension.ToLower() == ".pl" && line.Contains("=end"))
                _inCommentBlock = false;
           else if (File.Extension.ToLower() == ".lua" && line.Contains("]]"))
                _inCommentBlock = false;

            if (File.Extension.ToLower() == ".py")
            {
                if (line.Contains("'''") || line.Contains("\"\"\""))
                    _inCommentBlock = false;
            }
        }
    }
}