using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{
    public class ChangedFileManager
    {
        private List<ChangedFile> changedFiles = new List<ChangedFile>();

        public List<ChangedFile> ChangedFiles 
        { 
            get 
            {
                return changedFiles;
            }
        }

        public ChangedFile AddFile(string fileName)
        {
            var file = (from ChangedFile changedFile in ChangedFiles
                        where changedFile.FileName == fileName
                        select changedFile).FirstOrDefault();

            if (file == null)
            {
                file = new ChangedFile();
                file.FileName = fileName;

                ChangedFiles.Add(file);
            }

            return file;
        }
    }
}
