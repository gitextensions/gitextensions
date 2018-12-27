using System;
using System.Diagnostics;
using System.IO;
using GitCommands.Utils;

namespace GitCommands.Settings
{
    public abstract class FileSettingsCache : SettingsCache
    {
        private const double SaveTime = 2000;
        private DateTime? _lastFileRead;
        private DateTime _lastFileModificationDate = DateTime.MaxValue;
        private DateTime? _lastModificationDate;
        private readonly FileSystemWatcher _fileWatcher = new FileSystemWatcher();
        private readonly bool _canEnableFileWatcher;

        private System.Timers.Timer _saveTimer = new System.Timers.Timer(SaveTime);
        private readonly bool _autoSave;

        public string SettingsFilePath { get; }

        protected FileSettingsCache(string settingsFilePath, bool autoSave = true)
        {
            SettingsFilePath = settingsFilePath;
            _autoSave = autoSave;

            _saveTimer.Enabled = false;
            _saveTimer.AutoReset = false;
            _saveTimer.Elapsed += OnSaveTimer;

            _fileWatcher.IncludeSubdirectories = false;
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Changed += _fileWatcher_Changed;
            _fileWatcher.Renamed += _fileWatcher_Renamed;
            _fileWatcher.Created += _fileWatcher_Created;
            var dir = Path.GetDirectoryName(SettingsFilePath);
            if (Directory.Exists(dir))
            {
                _fileWatcher.Path = dir;
                _fileWatcher.Filter = Path.GetFileName(SettingsFilePath);
                _canEnableFileWatcher = true;
                _fileWatcher.EnableRaisingEvents = _canEnableFileWatcher;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "SaveTimer", Justification = "SaveTimer is disposed inside lambda but Code Analysis could not determine that")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_fileWatcher", Justification = "_fileWtcher is disposed inside lambda but Code Analysis could not determine that")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LockedAction(() =>
                {
                    if (_saveTimer != null)
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

                int currentProcessId;
                using (var currentProcess = Process.GetCurrentProcess())
                {
                    currentProcessId = currentProcess.Id;
                }

                var tmpFile = SettingsFilePath + currentProcessId + ".tmp";
                WriteSettings(tmpFile);

                if (File.Exists(SettingsFilePath))
                {
                    var backupName = SettingsFilePath + ".backup";
                    File.Copy(SettingsFilePath, backupName, true);
                }

                File.Copy(tmpFile, SettingsFilePath, true);
                File.Delete(tmpFile);

                _lastFileModificationDate = GetLastFileModificationUtc();
                _lastFileRead = DateTime.UtcNow;
                if (_saveTimer != null)
                {
                    _fileWatcher.EnableRaisingEvents = _canEnableFileWatcher;
                }
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        protected override void LoadImpl()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    ReadSettings(SettingsFilePath);
                    _lastFileRead = DateTime.UtcNow;
                    _fileWatcher.EnableRaisingEvents = _canEnableFileWatcher;
                }
                catch (Exception e)
                {
                    // TODO: should we report it to the user somehow?
                    Debug.WriteLine(e.Message);
                }
            }
            else
            {
                _lastFileRead = DateTime.UtcNow;
                _lastFileModificationDate = _lastFileRead.Value;
            }
        }

        protected override bool NeedRefresh()
        {
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
            var t = (System.Timers.Timer)source;
            t.Stop();
            Save();
        }

        private void StartSaveTimer()
        {
            // Resets timer so that the last call will let the timer event run and will cause the settings to be saved.
            _saveTimer.Stop();
            _saveTimer.AutoReset = true;
            _saveTimer.Interval = SaveTime;
            _saveTimer.AutoReset = false;

            _saveTimer.Start();
        }
    }
}
