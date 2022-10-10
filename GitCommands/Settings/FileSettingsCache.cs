﻿using System.Diagnostics;
using GitCommands.Utils;
using Timer = System.Timers.Timer;

namespace GitCommands.Settings
{
    public abstract class FileSettingsCache : SettingsCache
    {
        private const double SaveTime = 2000;
        private DateTime? _lastFileRead;
        private DateTime _lastFileModificationDate = DateTime.MaxValue;
        private DateTime? _lastModificationDate;
        private readonly FileSystemWatcher _fileWatcher = new();
        private readonly bool _canEnableFileWatcher;

        // The FileSystemWatcher is not reporting changes for WSL
        // https://github.com/microsoft/WSL/issues/4581
        private readonly bool _forceFileChangeChecks;

        private Timer? _saveTimer;
        private readonly bool _autoSave;

        public string SettingsFilePath { get; }

        protected FileSettingsCache(string settingsFilePath, bool autoSave = true)
        {
            SettingsFilePath = settingsFilePath;
            _autoSave = autoSave;

            _saveTimer = new Timer(SaveTime);
            _saveTimer.Enabled = false;
            _saveTimer.AutoReset = false;
            _saveTimer.Elapsed += OnSaveTimer;

            _fileWatcher.IncludeSubdirectories = false;
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Changed += _fileWatcher_Changed;
            _fileWatcher.Renamed += _fileWatcher_Renamed;
            _fileWatcher.Created += _fileWatcher_Created;

            string? dir = Path.GetDirectoryName(SettingsFilePath);
            if (Directory.Exists(dir) && File.Exists(SettingsFilePath))
            {
                _fileWatcher.Path = dir;
                _fileWatcher.Filter = Path.GetFileName(SettingsFilePath);
                _canEnableFileWatcher = true;
                _fileWatcher.EnableRaisingEvents = _canEnableFileWatcher;

                // The file exists, but notifications may not fire
                _forceFileChangeChecks = PathUtil.IsWslPath(SettingsFilePath);
            }

            FileChanged();
        }

        private void _fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _lastFileRead = null;
            FileChanged();
        }

        private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            FileChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LockedAction(() =>
                {
                    if (_saveTimer is not null)
                    {
                        _saveTimer.Dispose();
                        _saveTimer = null;
                        _fileWatcher.Changed -= _fileWatcher_Changed;
                        _fileWatcher.Renamed -= _fileWatcher_Renamed;
                        _fileWatcher.Created -= _fileWatcher_Created;

                        if (_autoSave)
                        {
                            Save();
                        }

                        _fileWatcher.Dispose();
                    }
                });
            }

            base.Dispose(disposing);
        }

        public static T FromCache<T>(string settingsFilePath, Lazy<T> createSettingsCache)
             where T : FileSettingsCache
        {
            return WeakRefCache.Default.Get(settingsFilePath + ":" + typeof(T).FullName, createSettingsCache);
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileChanged();
        }

        private void FileChanged()
        {
            _lastFileModificationDate = GetLastFileModificationUtc();
        }

        private DateTime GetLastFileModificationUtc()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    return File.GetLastWriteTimeUtc(SettingsFilePath);
                }
            }
            catch
            {
                // no-op
            }

            return DateTime.MaxValue;
        }

        protected abstract void WriteSettings(string fileName);
        protected abstract void ReadSettings(string fileName);

        protected override void SaveImpl()
        {
            try
            {
                if (!_lastModificationDate.HasValue ||
                    (_lastFileRead.HasValue && _lastModificationDate.Value < _lastFileRead.Value))
                {
                    return;
                }

                var tmpFile = Path.GetTempFileName();
                WriteSettings(tmpFile);

                if (File.Exists(SettingsFilePath))
                {
                    var backupName = SettingsFilePath + ".backup";
                    try
                    {
                        File.Copy(SettingsFilePath, backupName, true);
                    }
                    catch (Exception)
                    {
                        // Ignore errors for the backup file
                    }
                }

                try
                {
                    // ensure the directory structure exists
                    var parentFolder = Path.GetDirectoryName(SettingsFilePath);
                    if (!Directory.Exists(parentFolder))
                    {
                        Directory.CreateDirectory(parentFolder);
                    }

                    File.Copy(tmpFile, SettingsFilePath, true);
                    File.Delete(tmpFile);
                }
                catch (Exception ex)
                {
                    _lastFileRead = null;
                    throw new SaveSettingsException(ex);
                }

                _lastFileModificationDate = GetLastFileModificationUtc();
                _lastFileRead = DateTime.UtcNow;
                if (_saveTimer is not null)
                {
                    _fileWatcher.EnableRaisingEvents = _canEnableFileWatcher;
                }
            }
            catch (IOException ex)
            {
                throw new SaveSettingsException(ex);
            }
        }

        protected override void LoadImpl()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    if (_forceFileChangeChecks)
                    {
                        _lastFileModificationDate = GetLastFileModificationUtc();
                    }

                    ReadSettings(SettingsFilePath);
                    _lastFileRead = DateTime.UtcNow;
                    _fileWatcher.EnableRaisingEvents = _canEnableFileWatcher;
                }
                catch (Exception e)
                {
                    // TODO: should we report it to the user somehow?
                    Trace.WriteLine($"Failed to load {SettingsFilePath}: {e.Message}");
                }
            }
            else
            {
                _lastFileModificationDate = DateTime.UtcNow;
                _lastFileRead = _lastFileModificationDate;
            }
        }

        protected override bool NeedRefresh()
        {
            if (_forceFileChangeChecks)
            {
                _lastFileModificationDate = GetLastFileModificationUtc();
            }

            return !_lastFileRead.HasValue || _lastFileModificationDate > _lastFileRead.Value;
        }

        protected override void SettingsChanged()
        {
            base.SettingsChanged();

            _lastModificationDate = DateTime.UtcNow;

            if (_autoSave)
            {
                StartSaveTimer();
            }
        }

        // Used to eliminate multiple settings file open and close to save multiple values.  Settings will be saved SAVETIME milliseconds after the last setvalue is called
        private void OnSaveTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            ((Timer)source).Stop();
            Save();
        }

        private void StartSaveTimer()
        {
            // Resets timer so that the last call will let the timer event run and will cause the settings to be saved.
            if (_saveTimer is not null)
            {
                _saveTimer.Stop();
                _saveTimer.AutoReset = true;
                _saveTimer.Interval = SaveTime;
                _saveTimer.AutoReset = false;

                _saveTimer.Start();
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FileSettingsCache _fileSettingsCache;

            public TestAccessor(FileSettingsCache fileSettingsCache)
            {
                _fileSettingsCache = fileSettingsCache;
            }

            public FileSystemWatcher FileSystemWatcher => _fileSettingsCache._fileWatcher;
            public bool CanEnableFileWatcher => _fileSettingsCache._canEnableFileWatcher;
            public void SaveImpl() => _fileSettingsCache.SaveImpl();
            public void SetLastModificationDate(DateTime date) => _fileSettingsCache._lastModificationDate = date;
        }
    }
}
