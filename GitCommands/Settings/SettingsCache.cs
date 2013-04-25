using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;


namespace GitCommands
{
    public class SettingsCache : IDisposable
    {

        private const double SAVETIME = 2000;

        private DateTime? LastFileRead = null;
        private DateTime? LastModificationDate = null;

        private readonly Dictionary<String, object> ByNameMap = new Dictionary<String, object>();
        private readonly XmlSerializableDictionary<string, string> EncodedNameMap = new XmlSerializableDictionary<string, string>();
        private System.Timers.Timer SaveTimer = new System.Timers.Timer(SAVETIME);
        private bool UseTimer = true;

        public string SettingsFilePath { get; private set; }

        public SettingsCache(string aSettingsFilePath)
        {
            SettingsFilePath = aSettingsFilePath;

            SaveTimer.Enabled = false;
            SaveTimer.AutoReset = false;
            SaveTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnSaveTimer);
        }

        public void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }

        private void DisposeImpl()
        {
            IDisposable timer = SaveTimer;
            if (timer != null)
            {
                timer.Dispose();
                SaveTimer = null;
            }
        }

        ~SettingsCache()
        {
            DisposeImpl();
        }


        public static SettingsCache FromCache(string aSettingsFilePath)
        { 
        //TODO return from global cache
            return new SettingsCache(aSettingsFilePath);
        }

        public void LockedAction(Action action)
        {
            lock (EncodedNameMap)
            {
                action();
            }
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

        public void Save()
        {
            try
            {
                var tmpFile = SettingsFilePath + ".tmp";
                lock (EncodedNameMap)
                {
                    if (LastModificationDate.HasValue && LastFileRead.HasValue
                        && LastModificationDate.Value < LastFileRead.Value)
                    {
                        return;
                    }

                    using (System.Xml.XmlTextWriter xtw = new System.Xml.XmlTextWriter(tmpFile, Encoding.UTF8))
                    {
                        xtw.Formatting = Formatting.Indented;
                        xtw.WriteStartDocument();
                        xtw.WriteStartElement("dictionary");

                        EncodedNameMap.WriteXml(xtw);
                        xtw.WriteEndElement();
                    }
                    
                    if (File.Exists(SettingsFilePath))
                    {
                        File.Replace(tmpFile, SettingsFilePath, SettingsFilePath + ".backup", true);
                    }
                    else
                    {
                        File.Move(tmpFile, SettingsFilePath);
                    }

                    LastFileRead = GetLastFileModificationUTC();
                }
            }
            catch (IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                throw;
            }
        }

        public void Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    lock (EncodedNameMap)
                    {
                        XmlReaderSettings rSettings = new XmlReaderSettings
                        {
                            IgnoreWhitespace = true
                        };

                        using (System.Xml.XmlReader xr = XmlReader.Create(SettingsFilePath, rSettings))
                        {
                            EncodedNameMap.ReadXml(xr);
                         
                            LastFileRead = DateTime.UtcNow;
                        }
                    }
                }
                catch (IOException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    throw;
                }
            }
        }

        public void Import(IEnumerable<Tuple<string, string>> keyValuePairs)
        {
                LockedAction(() =>
                {
                    foreach(var pair in keyValuePairs)
                    {
                        if (pair.Item2 != null)
                            EncodedNameMap[pair.Item1] = pair.Item2;
                    }

                    Save();
                });

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
            LastModificationDate = DateTime.UtcNow;
            //Resets timer so that the last call will let the timer event run and will cause the settings to be saved.
            SaveTimer.Stop();
            SaveTimer.AutoReset = true;
            SaveTimer.Interval = SAVETIME;
            SaveTimer.AutoReset = false;

            SaveTimer.Start();
        }

        private bool NeedRefresh()
        {
            DateTime lastMod = GetLastFileModificationUTC();
            return !LastFileRead.HasValue || lastMod > LastFileRead.Value;
        }

        private void EnsureSettingsAreUpToDate()
        {
            if (NeedRefresh())
            {
                lock (EncodedNameMap)
                {
                    ByNameMap.Clear();
                    EncodedNameMap.Clear();
                    Load();
                }
            }
        }

        private void SetValue(string name, string value)
        {
            lock (EncodedNameMap)
            {
                //will refresh EncodedNameMap if needed
                string inMemValue = GetValue(name);

                if (string.Equals(inMemValue, value))
                    return;

                if (value == null)
                    EncodedNameMap.Remove(name);
                else
                    EncodedNameMap[name] = value;

                if (UseTimer)
                    StartSaveTimer();
            }
        }

        private string GetValue(string name)
        {
            lock (EncodedNameMap)
            {
                EnsureSettingsAreUpToDate();
                string o = null;
                EncodedNameMap.TryGetValue(name, out o);
                return o;
            }
        }

        public bool HasValue(string name)
        {
            lock (EncodedNameMap)
            {
                EnsureSettingsAreUpToDate();
                return EncodedNameMap.ContainsKey(name);
            }
        }

        public void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            string s;

            if (value == null)
                s = null;
            else
                s = encode(value);

            lock (EncodedNameMap)
            {
                SetValue(name, s);
                ByNameMap[name] = value;
            }
        }

        public bool TryGetValue<T>(string name, T defaultValue, Func<string, T> decode, out T value)
        {
            object o;
            lock (EncodedNameMap)
            {
                EnsureSettingsAreUpToDate();

                if (ByNameMap.TryGetValue(name, out o))
                {
                    if (o == null || o is T)
                    {
                        value = (T)o;
                        return true;
                    }
                    else
                    {
                        throw new Exception("Incompatible class for settings: " + name + ". Expected: " + typeof(T).FullName + ", found: " + o.GetType().FullName);
                    }
                }
                else
                {
                    if (decode == null)
                        throw new ArgumentNullException("decode", string.Format("The decode parameter for setting {0} is null.", name));

                    string s = GetValue(name);

                    if (s == null)
                    {
                        value = defaultValue;
                        return false;
                    }

                    value = decode(s);
                    ByNameMap[name] = value;
                    return true;
                }
            }
        }
    }
}
