using System;
using System.Collections.Generic;
using System.IO;

namespace GitStatistics
{
    public class LineCounter
    {
        public event EventHandler LinesOfCodeUpdated;

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

        private static bool DirectoryIsFiltered(string path, IEnumerable<string> directoryFilters)
        {
            foreach (var directoryFilter in directoryFilters)
            {
                if (path.EndsWith(directoryFilter, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        private IEnumerable<string> GetFiles(IEnumerable<string> dirs, string filter, string[] directoryFilter)
        {
            foreach (var directory in dirs)
            {
                IEnumerable<string> efs = null;
                try
                {
                    efs = Directory.EnumerateFileSystemEntries(directory, filter);
                }
                catch (System.Exception)
                {
                    // Mostly permission and unmapped drive errors.
                    // But we also skip anything else as unimportant.
                    continue;
                }

                foreach (var entry in efs)
                {
                    yield return entry;
                }
            }
        }

        public void FindAndAnalyzeCodeFiles(string filePattern, string directoriesToIgnore)
        {
            NumberLines = 0;
            NumberBlankLines = 0;
            NumberLinesInDesignerFiles = 0;
            NumberCommentsLines = 0;
            NumberCodeLines = 0;
            NumberTestCodeLines = 0;

            var filters = filePattern.Split(';');
            var directoryFilter = directoriesToIgnore.Split(';');
            List<string> dirs = new List<string>(1024);
            dirs.Add(_directory.FullName);
            foreach (var dir in Directory.EnumerateDirectories(_directory.FullName, "*", SearchOption.AllDirectories))
            {
                if (!DirectoryIsFiltered(dir, directoryFilter))
                {
                    dirs.Add(dir);
                }
            }

            var lastUpdate = DateTime.Now;
            var timer = new TimeSpan(0,0,0,0,500);

            foreach (var filter in filters)
            {
                foreach (var file in GetFiles(dirs, filter.Trim(), directoryFilter))
                {
                    var codeFile = new CodeFile(file);
                    codeFile.CountLines();

                    CalculateSums(codeFile);

                    if (LinesOfCodeUpdated != null && DateTime.Now - lastUpdate > timer)
                    {
                        lastUpdate = DateTime.Now;
                        LinesOfCodeUpdated(this, EventArgs.Empty);
                    }
                }
            }

            //Send 'changed' event when done
            if (LinesOfCodeUpdated != null)
                LinesOfCodeUpdated(this, EventArgs.Empty);
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
                LinesOfCodePerExtension.Add(extension, 0);

            LinesOfCodePerExtension[extension] += codeLines;
            NumberCodeLines += codeLines;

            if (codeFile.IsTestFile || codeFile.File.Directory.FullName.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                NumberTestCodeLines += codeLines;
            }


        }
    }
}