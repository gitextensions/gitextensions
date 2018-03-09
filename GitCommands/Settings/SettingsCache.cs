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

        public T LockedAction<T>(Func<T> action)
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
                    foreach (var pair in keyValuePairs)
                    {
                        if (pair.Item2 != null)
                        {
                            SetValueImpl(pair.Item1, pair.Item2);
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
            string s;

            if (value == null)
            {
                s = null;
            }
            else
            {
                s = encode(value);
            }

            return LockedAction(() =>
            {
                string inMemValue = GetValue(name);
                return inMemValue != null && !string.Equals(inMemValue, s);
            });
        }

        public void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            string s;

            if (value == null)
            {
                s = null;
            }
            else
            {
                s = encode(value);
            }

            LockedAction(() =>
            {
                SetValue(name, s);
                if (s == null)
                {
                    _byNameMap[name] = null;
                }
                else
                {
                    _byNameMap[name] = value;
                }
            });
        }

        public bool TryGetValue<T>(string name, T defaultValue, Func<string, T> decode, out T value)
        {
            object o;
            T val = defaultValue;

            bool result = LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();

                if (_byNameMap.TryGetValue(name, out o))
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
                }
            });
            value = val;
            return result;
        }
    }
}
