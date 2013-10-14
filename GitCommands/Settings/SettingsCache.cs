using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GitCommands
{
    public abstract class SettingsCache : IDisposable
    {
        private readonly Dictionary<String, object> ByNameMap = new Dictionary<String, object>();

        public SettingsCache()
        {
        }

        public void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeImpl()
        {
        }

        ~SettingsCache()
        {
            DisposeImpl();
        }

        public void LockedAction(Action action)
        {
            LockedAction<object>(() =>
            {
                action();
                return null;
            });
        }

        public T LockedAction<T>(Func<T> action)
        {
            lock (ByNameMap)
            {
                return action();
            }
        }

        protected abstract void SaveImpl();
        protected abstract void LoadImpl();
        protected abstract void SetValueImpl(string key, string value);        
        protected abstract string GetValueImpl(string key);
        protected abstract bool NeedRefresh();

        protected virtual void ClearImpl()
        {
            ByNameMap.Clear();
        }

        private void Clear()
        {
            LockedAction(ClearImpl);
        }

        public void Save()
        {
            LockedAction(SaveImpl);
        }

        public void Load()
        {
            LockedAction(() =>
                {
                    Clear();
                    LoadImpl();
                });
        }

        public void Import(IEnumerable<Tuple<string, string>> keyValuePairs)
        {
                LockedAction(() =>
                {
                    foreach(var pair in keyValuePairs)
                    {
                        if (pair.Item2 != null)
                            SetValueImpl(pair.Item1, pair.Item2);
                    }

                    Save();
                });

        }

        protected void EnsureSettingsAreUpToDate()
        {
            if (NeedRefresh())
            {
                LockedAction(Load);
            }
        }

        protected virtual void SettingsChanged()
        { 
        }

        private void SetValue(string name, string value)
        {
            LockedAction(() =>
            {
                //will refresh EncodedNameMap if needed
                string inMemValue = GetValue(name);

                if (string.Equals(inMemValue, value))
                    return;

                SetValueImpl(name, value);

                SettingsChanged();
            });
        }

        private string GetValue(string name)
        {
            return LockedAction<string>(() =>
            {
                EnsureSettingsAreUpToDate();
                return GetValueImpl(name);
            });
        }

        public bool HasValue(string name)
        {
            return LockedAction<bool>(() =>
            {
                EnsureSettingsAreUpToDate();
                return GetValueImpl(name) != null;
            });
        }

        public void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            string s;

            if (value == null)
                s = null;
            else
                s = encode(value);

            LockedAction(() =>
            {
                SetValue(name, s);
                ByNameMap[name] = value;
            });
        }

        public bool TryGetValue<T>(string name, T defaultValue, Func<string, T> decode, out T value)
        {
            object o;
            T val = defaultValue;

            bool result = LockedAction<bool>(() =>
            {
                EnsureSettingsAreUpToDate();

                if (ByNameMap.TryGetValue(name, out o))
                {
                    if (o == null)
                    {
                        val = defaultValue;
                        return false;
                    }
                    else if (o is T)
                    {
                        val = (T)o;
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
                        val = defaultValue;
                        return false;
                    }

                    val = decode(s);
                    ByNameMap[name] = val;
                    return true;
                }
            });
            value = val;
            return result;
        }
    }
}
