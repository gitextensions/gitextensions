using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands
{
    public abstract class SettingsCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, object> _byNameMap = new ConcurrentDictionary<string, object>();

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
                return default;
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
        [CanBeNull]
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

                _byNameMap.AddOrUpdate(name, value, (key, oldValue) => value);
            });
        }

        // This method will attempt to get the value from cache first. If the setting is not cached, it will call GetValue.
        // GetValue will not look in the cache. This method doesn't require a lock. A lock is only required when GetValue is
        // called. GetValue will set the lock.
        public bool TryGetValue<T>([NotNull] string name, T defaultValue, [NotNull] Func<string, T> decode, out T value)
        {
            if (decode == null)
            {
                throw new ArgumentNullException(nameof(decode), $"The decode parameter for setting {name} is null.");
            }

            value = defaultValue;

            EnsureSettingsAreUpToDate();

            if (_byNameMap.TryGetValue(name, out object o))
            {
                switch (o)
                {
                    case null:
                        return false;
                    case T t:
                        value = t;
                        return true;
                    default:
                        throw new Exception("Incompatible class for settings: " + name + ". Expected: " + typeof(T).FullName + ", found: " + o.GetType().FullName);
                }
            }

            string s = GetValue(name);

            if (s == null)
            {
                value = defaultValue;
                return false;
            }

            T decodedValue = decode(s);
            value = decodedValue;
            _byNameMap.AddOrUpdate(name, decodedValue, (key, oldValue) => decodedValue);
            return true;
        }
    }
}
