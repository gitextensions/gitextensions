namespace GitExtensions.Plugins.GitStatistics
{
    public sealed class LineCounter
    {
        public event EventHandler? Updated;

        public int CommentLineCount { get; private set; }
        public int TotalLineCount { get; private set; }
        public int DesignerLineCount { get; private set; }
        public int TestCodeLineCount { get; private set; }
        public int BlankLineCount { get; private set; }
        public int CodeLineCount { get; private set; }

        public Dictionary<string, int> LinesOfCodePerExtension { get; } = [];

        public void FindAndAnalyzeCodeFiles(string filePattern, string directoriesToIgnore, IEnumerable<string> filesToCheck)
        {
            HashSet<string> extensions = filePattern.Replace("*", "").Split(';').ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            string[] directoryFilter = directoriesToIgnore.Split(';');
            DateTime lastUpdate = DateTime.Now;
            TimeSpan timer = TimeSpan.FromMilliseconds(500);

            foreach (FileInfo file in GetFiles())
            {
                if (DirectoryIsFiltered(file.Directory, directoryFilter))
                {
                    continue;
                }

                AddFile(file);

                if (DateTime.Now - lastUpdate > timer)
                {
                    lastUpdate = DateTime.Now;
                    Updated?.Invoke(this, EventArgs.Empty);
                }
            }

            return;

            bool DirectoryIsFiltered(FileSystemInfo dir, IEnumerable<string> directoryFilters)
            {
                return directoryFilters.Any(
                    filter => dir.FullName.EndsWith(filter, StringComparison.InvariantCultureIgnoreCase));
            }

            IEnumerable<FileInfo> GetFiles()
            {
                foreach (string file in filesToCheck)
                {
                    FileInfo fileInfo = null;
                    try
                    {
                        if (extensions.Contains(Path.GetExtension(file)))
                        {
                            fileInfo = new FileInfo(file);
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    if (fileInfo is not null)
                    {
                        yield return fileInfo;
                    }
                }
            }

            void AddFile(FileInfo file)
            {
                if (!file.Exists)
                {
                    return;
                }

                CodeFile codeFile = CodeFile.Parse(file);

                TotalLineCount += codeFile.TotalLineCount;
                BlankLineCount += codeFile.BlankLineCount;
                CommentLineCount += codeFile.CommentLineCount;
                DesignerLineCount += codeFile.DesignerLineCount;

                string extension = file.Extension.ToLower();

                LinesOfCodePerExtension.TryGetValue(extension, out int linesForExtensions);
                LinesOfCodePerExtension[extension] = linesForExtensions + codeFile.CodeLineCount;

                CodeLineCount += codeFile.CodeLineCount;

                if (codeFile.IsTestFile || file.Directory?.FullName.Contains("test", StringComparison.OrdinalIgnoreCase) == true)
                {
                    TestCodeLineCount += codeFile.CodeLineCount;
                }
            }
        }
    }
}
