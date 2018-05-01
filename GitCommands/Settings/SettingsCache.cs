using System;
using System.Collections.Generic;

namespace GitCommands
{
    public abstract class SettingsCache : IDisposable
    {
        private readonly Dictionary<string, object> _byNameMap = new Dictionary<string, object>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void LockedAction(Action action)
        {
            LockedAction<object>(() =>
            {
                action();
                return null;
            });
        }

        protected T LockedAction<T>(Func<T> action)
        {
            lock (_byNameMap)
            {
                return action();
            }
        }

        protected abstract void SaveImpl();
        protected abstract void LoadImpl();
        protected abstract void SetValueImpl(string key, string value);
        protected abstract string GetValueImpl(string key);
        protected abstract bool NeedRefresh();
        protected abstract void ClearImpl();

        private void Clear()
        {
            LockedAction(() =>
            {
                ClearImpl();
                _byNameMap.Clear();
            });
        }

        public void Save()
        {
            LockedAction(SaveImpl);
        }

        private void Load()
        {
            LockedAction(() =>
                {
                    Clear();
                    LoadImpl();
                });
        }

        public void Import(IEnumerable<(string name, string value)> keyValuePairs)
        {
            LockedAction(() =>
                {
                    foreach (var (key, value) in keyValuePairs)
                    {
                        if (value != null)
                        {
                            SetValueImpl(key, value);
                        }
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
                // will refresh EncodedNameMap if needed
                string inMemValue = GetValue(name);

                if (string.Equals(inMemValue, value))
                {
                    return;
                }

                SetValueImpl(name, value);

                SettingsChanged();
            });
        }

        private string GetValue(string name)
        {
            return LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();
                return GetValueImpl(name);
            });
        }

        public bool HasValue(string name)
        {
            return GetValue(name) != null;
        }

        public bool HasADifferentValue<T>(string name, T value, Func<T, string> encode)
        {
            var s = value != null
                ? encode(value)
                : null;

            return LockedAction(() =>
            {
                string inMemValue = GetValue(name);
                return inMemValue != null && !string.Equals(inMemValue, s);
            });
        }

        public void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            var s = value != null
                ? encode(value)
                : null;

            LockedAction(() =>
            {
                SetValue(name, s);

                _byNameMap[name] = s == null ? (object)null : value;
            });
        }

        public bool TryGetValue<T>(string name, T defaultValue, Func<string, T> decode, out T value)
        {
            T val = defaultValue;

            bool result = LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();

                if (_byNameMap.TryGetValue(name, out object o))
                {
                    switch (o)
                    {
                        case null:
                            return false;
                        case T t:
                            val = t;
                            return true;
                        default:
                            throw new Exception("Incompatible class for settings: " + name + ". Expected: " + typeof(T).FullName + ", found: " + o.GetType().FullName);
                    }
                }

                if (decode == null)
                {
                    throw new ArgumentNullException(nameof(decode), string.Format("The decode parameter for setting {0} is null.", name));
                }

                string s = GetValue(name);

                if (s == null)
                {
                    val = defaultValue;
                    return false;
                }

                val = decode(s);
                _byNameMap[name] = val;
                return true;
            });
            value = val;
            return result;
        }
    }
}
