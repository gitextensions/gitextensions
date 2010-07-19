using System;
using System.IO;

namespace GitCommands
{
    /// <summary>
    ///   Remove all attributes that could cause the file to be read-only 
    ///   and restores them later
    /// </summary>
    public class TempRemoveFileAttributes : IDisposable
    {
        private readonly FileInfo _file;
        private readonly FileAttributes _oldAttributes;

        public TempRemoveFileAttributes(string fileName)
        {
            _file = new FileInfo(fileName);
            if (!_file.Exists) return;
            _oldAttributes = _file.Attributes;
            _file.Attributes = FileAttributes.Normal;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        ~TempRemoveFileAttributes()
        {
            Dispose(false);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_file != null && _file.Exists)
                _file.Attributes = _oldAttributes;
        }
    }
}