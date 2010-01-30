using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GitStatistics;
using System.Text.RegularExpressions;

namespace GitStatistics
{
    public class LineCounter
    {
        public LineCounter(DirectoryInfo directory)
        {
            this.Directory = directory;
        }

        private DirectoryInfo Directory;

        public int NumberLines = 0;
        public int NumberBlankLines = 0;
        public int NumberLinesInDesignerFiles = 0;
        public int NumberCommentsLines = 0;
        public int NumberCodeLines = 0;
        public int NumberTestCodeLines = 0;

        public Dictionary<string, int> LinesOfCodePerExtension = new Dictionary<string,int>();

        private bool DirectoryIsFiltered(DirectoryInfo dir, string[] directoryFilters)
        {
            foreach(string directoryFilter in directoryFilters)
            {
                //if (Regex.IsMatch(dir.FullName, directoryFilter, RegexOptions.IgnoreCase))
                if (dir.FullName.EndsWith(directoryFilter, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public void Count(string filePattern, string directoriesToIgnore)
        {
            string[] filters = filePattern.Split(';');
            string[] directoryFilter = directoriesToIgnore.Split(';');

            foreach (string filter in filters)
            {
                FileInfo[] files = Directory.GetFiles(filter.Trim(), SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {

                    if (!DirectoryIsFiltered(file.Directory, directoryFilter))
                    {

                        if (!LinesOfCodePerExtension.ContainsKey(file.Extension.ToLower()))
                            LinesOfCodePerExtension.Add(file.Extension.ToLower(), 0);

                        CodeFile codeFile = new CodeFile(file.FullName);
                        codeFile.CountLines();
                        NumberLines += codeFile.NumberLines;
                        NumberBlankLines += codeFile.NumberBlankLines;
                        NumberCommentsLines += codeFile.NumberCommentsLines;
                        NumberLinesInDesignerFiles += codeFile.NumberLinesInDesignerFiles;
                        int codeLines = codeFile.NumberLines - codeFile.NumberBlankLines - codeFile.NumberCommentsLines - codeFile.NumberLinesInDesignerFiles;
                        LinesOfCodePerExtension[file.Extension.ToLower()] += codeLines;
                        NumberCodeLines += codeLines;

                        if (codeFile.IsTestFile)
                        {
                            NumberTestCodeLines += codeLines;
                        }
                    }
                }

                NumberLines = Math.Max(0, NumberLines);
                NumberBlankLines = Math.Max(0, NumberBlankLines);
                NumberLinesInDesignerFiles = Math.Max(0, NumberLinesInDesignerFiles);
                NumberCommentsLines = Math.Max(0, NumberCommentsLines);
                NumberCodeLines = Math.Max(0, NumberCodeLines);
                NumberTestCodeLines = Math.Max(0, NumberTestCodeLines);
            }
        }
    }
}
