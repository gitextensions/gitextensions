using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GitStatistics
{
    public class LineCounter
    {
        public event EventHandler LinesOfCodeUpdated;

        private readonly DirectoryInfo _directory;
        private int _updatingUI = 0;

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
        public bool Counting { get; private set; }
        public bool Cancel { get; set; }

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

        private IEnumerable<string> GetFiles(IEnumerable<string> dirs, string filter)
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
            try
            {
                Counting = true;
                NumberLines = 0;
                NumberBlankLines = 0;
                NumberLinesInDesignerFiles = 0;
                NumberCommentsLines = 0;
                NumberCodeLines = 0;
                NumberTestCodeLines = 0;

                var timer = new TimeSpan(0, 0, 0, 0, 500);
                var directoryFilter = directoriesToIgnore.Split(';');
                string root = _directory.FullName;
                var dirs = new ConcurrentBag<string>();
                dirs.Add(root);
                Parallel.ForEach(Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories),
                    (dir) =>
                    {
                        if (dir.IndexOf(".git", root.Length, StringComparison.InvariantCultureIgnoreCase) < 0 &&
                            !DirectoryIsFiltered(dir, directoryFilter))
                        {
                            dirs.Add(dir);
                        }
                    });

                // Setup the parallel foreach.
                ParallelOptions po = new ParallelOptions();
                CancellationTokenSource cts = new CancellationTokenSource();
                po.CancellationToken = cts.Token;
                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;

                var lastUpdate = DateTime.Now;
                Parallel.ForEach(filePattern.Split(';'), po,
                    (filter) =>
                    {
                        Parallel.ForEach(GetFiles(dirs, filter.Trim()), po,
                            (file) =>
                            {
                                var codeFile = new CodeFile(file);
                                codeFile.CountLines();
                                CalculateSums(codeFile);
                                if (Cancel)
                                {
                                    cts.Cancel();
                                }

                                if (LinesOfCodeUpdated != null && DateTime.Now - lastUpdate > timer &&
                                    Interlocked.Exchange(ref _updatingUI, 1) == 0)
                                {
                                    LinesOfCodeUpdated(this, EventArgs.Empty);
                                    lastUpdate = DateTime.Now;
                                    Interlocked.Exchange(ref _updatingUI, 0);
                                }
                            });
                    });
            }
            finally
            {
                Counting = false;

                //Send 'changed' event when done
                if (LinesOfCodeUpdated != null && !Cancel)
                    LinesOfCodeUpdated(this, EventArgs.Empty);
            }
        }

        private void CalculateSums(CodeFile codeFile)
        {
            var codeLines =
                codeFile.NumberLines -
                codeFile.NumberBlankLines -
                codeFile.NumberCommentsLines -
                codeFile.NumberLinesInDesignerFiles;

            var extension = codeFile.File.Extension.ToLower();

            lock (LinesOfCodePerExtension)
            {
                NumberLines += codeFile.NumberLines;
                NumberBlankLines += codeFile.NumberBlankLines;
                NumberCommentsLines += codeFile.NumberCommentsLines;
                NumberLinesInDesignerFiles += codeFile.NumberLinesInDesignerFiles;

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
}