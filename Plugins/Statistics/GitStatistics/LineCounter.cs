using System;
using System.Collections.Generic;
using System.IO;

namespace GitStatistics
{
    public class LineCounter
    {
        private readonly DirectoryInfo _directory;

        public LineCounter(DirectoryInfo directory)
        {
            LinesOfCodePerExtension = new Dictionary<string, int>();
            _directory = directory;
        }

        public int NumberCommentsLines { get; private set; }
        public int NumberLines { get; private set; }
        public int NumberLinesInDesignerFiles { get; private set; }
        public int NumberTestCodeLines { get; private set; }
        public int NumberBlankLines { get; private set; }
        public int NumberCodeLines { get; private set; }
        public Dictionary<string, int> LinesOfCodePerExtension { get; private set; }

        private static bool DirectoryIsFiltered(FileSystemInfo dir, IEnumerable<string> directoryFilters)
        {
            foreach (var directoryFilter in directoryFilters)
            {
                if (dir.FullName.EndsWith(directoryFilter, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public void FindAndAnalyzeCodeFiles(string filePattern, string directoriesToIgnore)
        {
            var filters = filePattern.Split(';');
            var directoryFilter = directoriesToIgnore.Split(';');

            var codeFiles = new List<CodeFile>();
            foreach (var filter in filters)
            {
                foreach (var file in _directory.GetFiles(filter.Trim(), SearchOption.AllDirectories))
                {
                    if (DirectoryIsFiltered(file.Directory, directoryFilter))
                        continue;

                    var codeFile = new CodeFile(file.FullName);
                    codeFile.CountLines();
                    codeFiles.Add(codeFile);
                }
            }

            CalculateSums(codeFiles);
        }

        private void CalculateSums(IEnumerable<CodeFile> codeFiles)
        {
            NumberLines = 0;
            NumberBlankLines = 0;
            NumberLinesInDesignerFiles = 0;
            NumberCommentsLines = 0;
            NumberCodeLines = 0;
            NumberTestCodeLines = 0;

            foreach (var codeFile in codeFiles)
            {
                NumberLines += codeFile.NumberLines;
                NumberBlankLines += codeFile.NumberBlankLines;
                NumberCommentsLines += codeFile.NumberCommentsLines;
                NumberLinesInDesignerFiles += codeFile.NumberLinesInDesignerFiles;

                var codeLines =
                    codeFile.NumberLines -
                    codeFile.NumberBlankLines -
                    codeFile.NumberCommentsLines -
                    codeFile.NumberLinesInDesignerFiles;

                var extension = codeFile.File.Extension.ToLower();

                if (!LinesOfCodePerExtension.ContainsKey(extension))
                    LinesOfCodePerExtension.Add(extension, 0);

                LinesOfCodePerExtension[extension] += codeLines;
                NumberCodeLines += codeLines;

                if (codeFile.IsTestFile)
                {
                    NumberTestCodeLines += codeLines;
                }
            }
        }
    }
}