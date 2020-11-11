﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitStatistics
{
    public sealed class LineCounter
    {
        public event EventHandler Updated;

        public int CommentLineCount { get; private set; }
        public int TotalLineCount { get; private set; }
        public int DesignerLineCount { get; private set; }
        public int TestCodeLineCount { get; private set; }
        public int BlankLineCount { get; private set; }
        public int CodeLineCount { get; private set; }

        public Dictionary<string, int> LinesOfCodePerExtension { get; } = new Dictionary<string, int>();

        public void FindAndAnalyzeCodeFiles(string filePattern, string directoriesToIgnore, IEnumerable<string> filesToCheck)
        {
            var extensions = filePattern.Replace("*", "").Split(';').ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var directoryFilter = directoriesToIgnore.Split(';');
            var lastUpdate = DateTime.Now;
            var timer = TimeSpan.FromMilliseconds(500);

            foreach (var file in GetFiles())
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
                foreach (var file in filesToCheck)
                {
                    if (extensions.Contains(GetExtension(file)))
                    {
                        FileInfo fileInfo;
                        try
                        {
                            fileInfo = new FileInfo(file);
                        }
                        catch
                        {
                            continue;
                        }

                        yield return fileInfo;
                    }
                }
            }

            static string GetExtension(string path)
            {
                try
                {
                    return Path.GetExtension(path);
                }
                catch (ArgumentException)
                {
                    // This could return part after a '.' using string commands,
                    // but wait for .NET Core with this edge case
                    return string.Empty;
                }
            }

            void AddFile(FileInfo file)
            {
                if (!file.Exists)
                {
                    return;
                }

                var codeFile = CodeFile.Parse(file);

                TotalLineCount += codeFile.TotalLineCount;
                BlankLineCount += codeFile.BlankLineCount;
                CommentLineCount += codeFile.CommentLineCount;
                DesignerLineCount += codeFile.DesignerLineCount;

                var extension = file.Extension.ToLower();

                LinesOfCodePerExtension.TryGetValue(extension, out var linesForExtensions);
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
