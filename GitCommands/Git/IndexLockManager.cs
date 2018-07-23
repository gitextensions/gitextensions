using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public interface IIndexLockManager
    {
        /// <summary>
        /// Determines whether the given repository has index.lock file.
        /// </summary>
        /// <returns><see langword="true"/> if index is locked; otherwise <see langword="false"/>.</returns>
        bool IsIndexLocked();

        /// <summary>
        /// Delete index.lock in the current working folder.
        /// </summary>
        /// <param name="includeSubmodules">
        ///     If <see langword="true"/> all submodules will be scanned for index.lock files and have them delete, if found.
        /// </param>
        /// <exception cref="FileDeleteException">Unable to delete specific index.lock.</exception>
        void UnlockIndex(bool includeSubmodules = true);
    }

    /// <summary>
    /// Facilitates detection and deletion of index.lock files.
    /// </summary>
    public sealed class IndexLockManager : IIndexLockManager
    {
        private const string IndexLock = "index.lock";
        private readonly IGitModule _module;
        private readonly IGitDirectoryResolver _gitDirectoryResolver;
        private readonly IFileSystem _fileSystem;

        public IndexLockManager(IGitModule module, IGitDirectoryResolver gitDirectoryResolver, IFileSystem fileSystem)
        {
            _module = module;
            _gitDirectoryResolver = gitDirectoryResolver;
            _fileSystem = fileSystem;
        }

        public IndexLockManager(IGitModule module)
            : this(module, new GitDirectoryResolver(), new FileSystem())
        {
        }

        /// <summary>
        /// Determines whether the given repository has index.lock file.
        /// </summary>
        /// <returns><see langword="true"/> if index is locked; otherwise <see langword="false"/>.</returns>
        public bool IsIndexLocked()
        {
            var indexLockFile = Path.Combine(_gitDirectoryResolver.Resolve(_module.WorkingDir), IndexLock);
            return _fileSystem.File.Exists(indexLockFile);
        }

        /// <summary>
        /// Delete index.lock in the current working folder.
        /// </summary>
        /// <param name="includeSubmodules">
        ///     If <see langword="true"/> all submodules will be scanned for index.lock files and have them delete, if found.
        /// </param>
        /// <exception cref="FileDeleteException">Unable to delete specific index.lock.</exception>
        public void UnlockIndex(bool includeSubmodules = true)
        {
            var workingFolderIndexLock = Path.Combine(_gitDirectoryResolver.Resolve(_module.WorkingDir), IndexLock);
            if (!includeSubmodules)
            {
                DeleteIndexLock(workingFolderIndexLock);
                return;
            }

            // get the list of files to delete
            var submodules = _module.GetSubmodulesLocalPaths();
            var list = submodules.Select(sm =>
            {
                var submodulePath = _module.GetSubmoduleFullPath(sm);
                var submoduleIndexLock = Path.Combine(_gitDirectoryResolver.Resolve(submodulePath), IndexLock);
                return submoduleIndexLock;
            }).Union(new[] { workingFolderIndexLock });

            foreach (var indexLock in list)
            {
                DeleteIndexLock(indexLock);
            }
        }

        private void DeleteIndexLock(string fileName)
        {
            if (!_fileSystem.File.Exists(fileName))
            {
                return;
            }

            try
            {
                _fileSystem.File.Delete(fileName);
            }
            catch (Exception ex)
            {
                throw new FileDeleteException(fileName, ex);
            }
        }
    }
}
