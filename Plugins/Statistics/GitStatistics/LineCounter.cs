using System;
using System.Collections.Generic;
using System.IO;

namespace GitStatistics
{
    public class LineCounter
    {
        private readonly DirectoryInfo _directory;
        public Dictionary<string, int> LinesOfCodePerExtension = new Dictionary<string, int>();

        public int NumberBlankLines;
        public int NumberCodeLines;
        public int NumberCommentsLines;
        public int NumberLines;
        public int NumberLinesInDesignerFiles;
        public int NumberTestCodeLines;

        public LineCounter(DirectoryInfo directory)
        {
            _directory = directory;
        }

        private static bool DirectoryIsFiltered(FileSystemInfo dir, IEnumerable<string> directoryFilters)
        {
            foreach (var directoryFilter in directoryFilters)
            {
                if (dir.FullName.EndsWith(directoryFilter, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public void Count(string filePattern, string directoriesToIgnore)
        {
            var filters = filePattern.Split(';');
            var directoryFilter = directoriesToIgnore.Split(';');

            foreach (var filter in filters)
            {
                var files = _directory.GetFiles(filter.Trim(), SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (DirectoryIsFiltered(file.Directory, directoryFilter)) 
                        continue;

                    if (!LinesOfCodePerExtension.ContainsKey(file.Extension.ToLower()))
                        LinesOfCodePerExtension.Add(file.Extension.ToLower(), 0);

                    var codeFile = new CodeFile(file.FullName);
                    codeFile.CountLines();
                    NumberLines += codeFile.NumberLines;
                    NumberBlankLines += codeFile.NumberBlankLines;
                    NumberCommentsLines += codeFile.NumberCommentsLines;
                    NumberLinesInDesignerFiles += codeFile.NumberLinesInDesignerFiles;
                    var codeLines = codeFile.NumberLines - codeFile.NumberBlankLines - codeFile.NumberCommentsLines -
                                    codeFile.NumberLinesInDesignerFiles;
                    LinesOfCodePerExtension[file.Extension.ToLower()] += codeLines;
                    NumberCodeLines += codeLines;

                    if (codeFile.IsTestFile)
                    {
                        NumberTestCodeLines += codeLines;
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