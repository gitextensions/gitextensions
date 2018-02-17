﻿using System;
using System.Collections.Generic;

namespace FindLargeFiles
{
    public class GitObject
    {
        public GitObject(string sha, string path, int size, string commit)
        {
            SHA = sha;
            Path = path;
            sizeInBytes = size;
            compressedSizeInBytes = -1;
            Commit = new HashSet<string> {commit};
        }

        public string SHA { get; set; }
        public string Path { get; set; }
        internal int sizeInBytes { get; set; }
        public string Size => string.Format("{0:F2} Mb", sizeInBytes / 1024.0f / 1024);
        internal int compressedSizeInBytes { get; set; }
        public string CompressedSize => compressedSizeInBytes >= 0 ? string.Format("{0:F2} Mb", compressedSizeInBytes / 1024.0f / 1024) : "<Unknown>";
        public int CommitCount => Commit.Count;
        public DateTime LastCommitDate { get; set; }
        public bool Delete { get; set; }
        internal HashSet<string> Commit { get; set; }
    }
}
