using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitStatistics
{
    public class LineCounter
    {
        public event EventHandler LinesOfCodeUpdated;

        public int NumberCommentsLines { get; private set; }
        public int NumberLines { get; private set; }
        public int NumberLinesInDesignerFiles { get; private set; }
        public int NumberTestCodeLines { get; private set; }
        public int NumberBlankLines { get; private set; }
        public int NumberCodeLines { get; private set; }

        public Dictionary<string, int> LinesOfCodePerExtension { get; } = new Dictionary<string, int>();

        private static bool DirectoryIsFiltered(FileSystemInfo dir, IEnumerable<string> directoryFilters)
        {
            return directoryFilters.Any(
                filter => dir.FullName.EndsWith(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        private static IEnumerable<FileInfo> GetFiles(List<string> filesToCheck, string[] codeFilePatterns)
        {
            foreach (var file in filesToCheck)
            {
                if (codeFilePatterns.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase))
                {
                    FileInfo fileInfo;
                    try
                    {
                        fileInfo = new FileInfo(file);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    yield return fileInfo;
                }
            }
        }

        public void FindAndAnalyzeCodeFiles(string filePattern, string directoriesToIgnore,
            List<string> filesToCheck)
        {
            var filters = filePattern.Replace("*", "").Split(';');
            var directoryFilter = directoriesToIgnore.Split(';');
            var lastUpdate = DateTime.Now;
            var timer = TimeSpan.FromMilliseconds(500);

            foreach (var file in GetFiles(filesToCheck, filters))
            {
                if (DirectoryIsFiltered(file.Directory, directoryFilter))
                {
                    continue;
                }

                var codeFile = new CodeFile(file.FullName);
                codeFile.CountLines();

                CalculateSums(codeFile);

                if (LinesOfCodeUpdated != null && DateTime.Now - lastUpdate > timer)
                {
                    lastUpdate = DateTime.Now;
                    LinesOfCodeUpdated(this, EventArgs.Empty);
                }
            }
        }

        private void CalculateSums(CodeFile codeFile)
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
            {
                LinesOfCodePerExtension.Add(extension, 0);
            }

            LinesOfCodePerExtension[extension] += codeLines;
            NumberCodeLines += codeLines;

            if (codeFile.IsTestFile || codeFile.File.Directory.FullName.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                NumberTestCodeLines += codeLines;
            }
        }
    }
}