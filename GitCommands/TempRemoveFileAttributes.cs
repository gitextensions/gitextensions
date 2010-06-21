using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GitCommands
{
    public class TempRemoveFileAttributes : IDisposable
    {
        private FileInfo file;
        private FileAttributes oldAttributes;

        public TempRemoveFileAttributes(string fileName)
        {
            //Remove all attributes that could cause the file to be read-only
            file = new FileInfo(fileName);
            oldAttributes = file.Attributes;
            file.Attributes = FileAttributes.Normal;
        }
        
        ~TempRemoveFileAttributes()
        {
            Dispose(false);
        }


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);   
        }

        #endregion

        public void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            file.Attributes = oldAttributes;
        }
    }
}
