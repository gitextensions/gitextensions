using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GitStatistics
{
    public readonly struct CodeFile
    {
        public int TotalLineCount { get; }
        public int BlankLineCount { get; }
        public int DesignerLineCount { get; }
        public int CommentLineCount { get; }
        public int CodeLineCount { get; }
        public bool IsTestFile { get; }

        public static CodeFile Parse(FileInfo file)
        {
            var lineCount = 0;
            var blankLineCount = 0;
            var designerLineCount = 0;
            var commentLineCount = 0;
            var isTestFile = false;

            var extension = file.Extension;
            var inCodeGeneratedRegion = false;
            var inCommentBlock = false;

            foreach (var line in ReadLines())
            {
                ProcessLine(line);
            }

            return new CodeFile(lineCount, blankLineCount, designerLineCount, commentLineCount, isTestFile);

            IEnumerable<string> ReadLines()
            {
                // NOTE not using File.ReadLines here as it doesn't appear to detect a BOM
                using (var reader = new StreamReader(file.FullName, detectEncodingFromByteOrderMarks: true))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();

                        if (line == null)
                        {
                            yield break;
                        }

                        yield return line;
                    }
                }
            }

            void ProcessLine(string line)
            {
                line = line.TrimStart();

                bool skipResetFlag;

                SetCodeBlockFlags();

                lineCount++;

                if (inCodeGeneratedRegion || IsDesignerFile())
                {
                    designerLineCount++;
                }
                else if (line.Length == 0)
                {
                    blankLineCount++;
                }
                else if (inCommentBlock || line.StartsWith("'") || line.StartsWith("//"))
                {
                    commentLineCount++;
                }
                else if (extension.Equals(".py", StringComparison.OrdinalIgnoreCase) && line.StartsWith("#"))
                {
                    commentLineCount++;
                }
                else if (extension.Equals(".rb", StringComparison.OrdinalIgnoreCase) && line.StartsWith("#"))
                {
                    commentLineCount++;
                }
                else if (extension.Equals(".pl", StringComparison.OrdinalIgnoreCase) && line.StartsWith("#"))
                {
                    commentLineCount++;
                }
                else if (extension.Equals(".lua", StringComparison.OrdinalIgnoreCase) && line.StartsWith("--"))
                {
                    commentLineCount++;
                }
                else if (extension.Equals(".cshtml", StringComparison.OrdinalIgnoreCase) && (line.Contains("@*") && line.Contains("*@")))
                {
                    commentLineCount++;
                }
                else if (extension.Equals(".m", StringComparison.OrdinalIgnoreCase) && line.StartsWith("%"))
                {
                    commentLineCount++;
                }
                else if ((extension.Equals(".asm", StringComparison.OrdinalIgnoreCase) ||
                          extension.Equals(".s", StringComparison.OrdinalIgnoreCase) ||
                          extension.Equals(".inc", StringComparison.OrdinalIgnoreCase)) &&
                          line.StartsWith(";"))
                {
                    commentLineCount++;
                }

                if (!skipResetFlag)
                {
                    ResetCodeBlockFlags();
                }

                return;

                bool IsDesignerFile()
                {
                    var isWebReferenceFile = file.FullName.Contains("Web References") && file.Name == "Reference.cs";

                    return isWebReferenceFile || file.Name.Contains(".Designer.", StringComparison.CurrentCultureIgnoreCase);
                }

                void SetCodeBlockFlags()
                {
                    skipResetFlag = false;

                    // The number of code-generated lines is an approximation at best, particularly
                    // with VS 2003 code.  Change code here if you don't like the way it's working.
                    // if (line.Contains("Designer generated code") // Might be cleaner
                    if (line.StartsWith("#region Windows Form Designer generated code") ||
                        line.StartsWith("#Region \" Windows Form Designer generated code") ||
                        line.StartsWith("#region Component Designer generated code") ||
                        line.StartsWith("#Region \" Component Designer generated code \"") ||
                        line.StartsWith("#region Web Form Designer generated code") ||
                        line.StartsWith("#Region \" Web Form Designer Generated Code \""))
                    {
                        inCodeGeneratedRegion = true;
                    }

                    if (line.StartsWith("/*"))
                    {
                        inCommentBlock = true;
                    }

                    if (extension.Equals(".pas", StringComparison.OrdinalIgnoreCase) || extension.Equals(".inc", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.StartsWith("(*") && !line.StartsWith("(*$"))
                        {
                            inCommentBlock = true;
                        }

                        if (line.StartsWith("{") && !line.StartsWith("{$"))
                        {
                            inCommentBlock = true;
                        }
                    }

                    if (extension.Equals(".rb", StringComparison.OrdinalIgnoreCase) && line.StartsWith("=begin"))
                    {
                        inCommentBlock = true;
                    }
                    else if (extension.Equals(".pl", StringComparison.OrdinalIgnoreCase) && line.StartsWith("=begin"))
                    {
                        inCommentBlock = true;
                    }
                    else if (extension.Equals(".lua", StringComparison.OrdinalIgnoreCase) && line.StartsWith("--[["))
                    {
                        inCommentBlock = true;
                    }

                    if (extension.Equals(".m", StringComparison.OrdinalIgnoreCase) && line.StartsWith("%{"))
                    {
                        inCommentBlock = true;
                    }

                    // If we're not in a code-generated region, we should still check for normal
                    // comments. This should help improve accuracy on resx files
                    if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
                        (extension.Equals(".resx", StringComparison.OrdinalIgnoreCase) && !inCodeGeneratedRegion) ||
                        extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(".htm", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.Contains("<!--"))
                        {
                            inCommentBlock = true;
                        }
                    }

                    if (extension.Equals(".aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.Contains("<%--"))
                        {
                            inCommentBlock = true;
                        }
                    }

                    if (extension.Equals(".py", StringComparison.OrdinalIgnoreCase) && !inCommentBlock)
                    {
                        if (line.StartsWith("'''") || line.StartsWith("\"\"\""))
                        {
                            inCommentBlock = true;
                            skipResetFlag = true;
                        }
                    }

                    if (!inCommentBlock && !inCodeGeneratedRegion && line.StartsWith("[Test"))
                    {
                        isTestFile = true;
                    }
                }

                void ResetCodeBlockFlags()
                {
                    if (inCodeGeneratedRegion && (line.Contains("#endregion") || line.Contains("#End Region")))
                    {
                        inCodeGeneratedRegion = false;
                    }

                    if (inCommentBlock && line.Contains("*/"))
                    {
                        inCommentBlock = false;
                    }

                    if (extension.ToLower() == ".pas" || extension.ToLower() == ".inc")
                    {
                        if (line.Contains("*)") || line.Contains("}"))
                        {
                            inCommentBlock = false;
                        }
                    }

                    if (extension.Equals(".rb", StringComparison.OrdinalIgnoreCase) && line.Contains("=end"))
                    {
                        inCommentBlock = false;
                    }
                    else if (extension.Equals(".pl", StringComparison.OrdinalIgnoreCase) && line.Contains("=end"))
                    {
                        inCommentBlock = false;
                    }
                    else if (extension.Equals(".lua", StringComparison.OrdinalIgnoreCase) && line.Contains("]]"))
                    {
                        inCommentBlock = false;
                    }

                    if (extension.Equals(".m", StringComparison.OrdinalIgnoreCase) && line.Contains("%}"))
                    {
                        inCommentBlock = false;
                    }

                    if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
                        (extension.Equals(".resx", StringComparison.OrdinalIgnoreCase) && !inCodeGeneratedRegion) ||
                        extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(".htm", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.Contains("-->"))
                        {
                            inCommentBlock = false;
                        }
                    }

                    if (extension.Equals(".aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.Contains("--%>"))
                        {
                            inCommentBlock = false;
                        }
                    }

                    if (extension.Equals(".py", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.Contains("'''") || line.Contains("\"\"\""))
                        {
                            inCommentBlock = false;
                        }
                    }
                }
            }
        }

        private CodeFile(int totalLineCount, int blankLineCount, int designerLineCount, int commentLineCount, bool isTestFile)
        {
            TotalLineCount = totalLineCount;
            BlankLineCount = blankLineCount;
            DesignerLineCount = designerLineCount;
            CommentLineCount = commentLineCount;
            IsTestFile = isTestFile;

            CodeLineCount = totalLineCount - blankLineCount - designerLineCount - commentLineCount;

            Debug.Assert(CodeLineCount >= 0, "CodeLineCount >= 0");
        }
    }
}
