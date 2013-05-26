﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitCommands.Utils;

namespace GitCommands.Settings
{
    public abstract class FileSettingsCache : SettingsCache
    {
        private const double SAVETIME = 2000;
        private DateTime? LastFileRead = null;
        private DateTime LastFileModificationDate = DateTime.MaxValue;
        private DateTime? LastModificationDate = null;
        private readonly FileSystemWatcher _fileWatcher = new FileSystemWatcher();

        private System.Timers.Timer SaveTimer = new System.Timers.Timer(SAVETIME);
        private bool _autoSave = true;

        public string SettingsFilePath { get; private set; }

        public FileSettingsCache(string aSettingsFilePath, bool autoSave = true)
        {
            SettingsFilePath = aSettingsFilePath;
            _autoSave = autoSave;

            SaveTimer.Enabled = false;
            SaveTimer.AutoReset = false;
            SaveTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnSaveTimer);

            _fileWatcher.Path = Path.GetDirectoryName(SettingsFilePath);
            _fileWatcher.Filter = Path.GetFileName(SettingsFilePath);
            _fileWatcher.IncludeSubdirectories = false;
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Changed += _fileWatcher_Changed;
            FileChanged();
        }

        protected override void DisposeImpl()
        {
            IDisposable timer = SaveTimer;
            if (timer != null)
            {
                timer.Dispose();
                SaveTimer = null;
                if (_autoSave)
                {
                    Save();
                }
            }

            _fileWatcher.Changed -= _fileWatcher_Changed;
            _fileWatcher.Dispose();

            base.DisposeImpl();
        }

        public static T FromCache<T>(string aSettingsFilePath, Lazy<T> createSettingsCache)
             where T : FileSettingsCache
        {
            return WeakRefCache.Default.Get(aSettingsFilePath + ":" + typeof(T).FullName, createSettingsCache);
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileChanged();
        }

        private void FileChanged()
        {
            _fileWatcher.EnableRaisingEvents = File.Exists(SettingsFilePath);
            LastFileModificationDate = GetLastFileModificationUTC();
        }

        private DateTime GetLastFileModificationUTC()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                    return File.GetLastWriteTimeUtc(SettingsFilePath);
                else
                    return DateTime.MaxValue;
            }
            catch (Exception)
            {
                return DateTime.MaxValue;
            }
        }

        protected abstract void WriteSettings(string fileName);
        protected abstract void ReadSettings(string fileName);

        protected override void SaveImpl()
        {
            try
            {
                var tmpFile = SettingsFilePath + ".tmp";

                if (!LastModificationDate.HasValue || (LastFileRead.HasValue
                        && LastModificationDate.Value < LastFileRead.Value))
                {
                    return;
                }

                WriteSettings(tmpFile);

                if (File.Exists(SettingsFilePath))
                {
                    File.Replace(tmpFile, SettingsFilePath, SettingsFilePath + ".backup", true);
                }
                else
                {
                    File.Move(tmpFile, SettingsFilePath);
                }

                LastFileModificationDate = GetLastFileModificationUTC();
                LastFileRead = DateTime.UtcNow;
            }

            catch (IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
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
                    LastFileRead = DateTime.UtcNow;
                }
                catch (IOException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    throw;
                }
            }
        }

        protected override bool NeedRefresh()
        {
            return !LastFileRead.HasValue || LastFileModificationDate > LastFileRead.Value;
        }

        protected override void SettingsChanged()
        {
            base.SettingsChanged();

            LastModificationDate = DateTime.UtcNow;

            if (_autoSave)
                StartSaveTimer();
        }

        //Used to eliminate multiple settings file open and close to save multiple values.  Settings will be saved SAVETIME milliseconds after the last setvalue is called
        private void OnSaveTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer t = (System.Timers.Timer)source;
            t.Stop();
            Save();
        }

        private void StartSaveTimer()
        {
            //Resets timer so that the last call will let the timer event run and will cause the settings to be saved.
            SaveTimer.Stop();
            SaveTimer.AutoReset = true;
            SaveTimer.Interval = SAVETIME;
            SaveTimer.AutoReset = false;

            SaveTimer.Start();
        }
    }
}
