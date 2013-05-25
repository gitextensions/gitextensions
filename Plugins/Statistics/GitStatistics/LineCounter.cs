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

        private static bool DirectoryIsFiltered(FileSystemInfo dir, IEnumerable<string> directoryFilters)
        {
            foreach (var directoryFilter in directoryFilters)
            {
                if (dir.FullName.EndsWith(directoryFilter, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        private IEnumerable<FileInfo> GetFiles(DirectoryInfo startDirectory, string filter)
        {
            Queue<DirectoryInfo> queue = new Queue<DirectoryInfo>();
            queue.Enqueue(startDirectory);
            while(queue.Count != 0)
            {
                DirectoryInfo directory = queue.Dequeue();
                FileInfo[] files = null;
                try
                {
                    files = directory.GetFiles(filter);
                    DirectoryInfo[] directories = directory.GetDirectories();
                    foreach (var dir in directories)
                        queue.Enqueue(dir);
                }
                catch (System.UnauthorizedAccessException)
                {
                }
                if (files != null)
                {
                    foreach (var file in files)
                        yield return file;
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
            var lastUpdate = DateTime.Now;
            var timer = new TimeSpan(0,0,0,0,500);

            foreach (var filter in filters)
            {
                foreach (var file in GetFiles(_directory, filter.Trim()))
                {
                    if (DirectoryIsFiltered(file.Directory, directoryFilter))
                        continue;

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