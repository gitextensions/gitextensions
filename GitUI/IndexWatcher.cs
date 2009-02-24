using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GitUI
{
    public class IndexWatcher
    {
        public DateTime IndexTime { get; set; }
        public long IndexSize { get; set; }



        private void SetIndexTime()
        {
            FileInfo fileInfo = new FileInfo(GitCommands.Settings.WorkingDirGitDir() + "\\index");
            if (fileInfo.Exists)
            {
                IndexTime = fileInfo.LastWriteTimeUtc;
                IndexSize = fileInfo.Length;
            }
        }

        public bool IndexChanged()
        {
            FileInfo fileInfo = new FileInfo(GitCommands.Settings.WorkingDirGitDir() + "\\index");
            if (fileInfo.Exists)
            {
                if (fileInfo.LastWriteTimeUtc == IndexTime &&
                    fileInfo.Length == IndexSize)
                    return false;
            }

            return true;
        }

        public void Reset()
        {
            SetIndexTime();
        }

        public void Clear()
        {
            IndexSize = 0;
        }
    }
}
