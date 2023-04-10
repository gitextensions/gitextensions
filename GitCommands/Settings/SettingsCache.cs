using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GitCommands
{
    [DebuggerDisplay("{" + nameof(_byNameMap) + ".Count}")]
    public abstract class SettingsCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, string?> _byNameMap = new();

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
            LockedAction<object?>(() =>
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
        protected abstract void SetValueImpl(string key, string? value);
        protected abstract string? GetValueImpl(string key);
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
                        if (value is not null)
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

        private string? GetValue(string name)
        {
            return LockedAction(() =>
            {
                EnsureSettingsAreUpToDate();
                return GetValueImpl(name);
            });
        }

        public bool HasValue(string name)
        {
            return GetValue(name) is not null;
        }

        public bool HasADifferentValue(string name, string? value)
        {
            return LockedAction(() =>
            {
                string? inMemValue = GetValue(name);
                return inMemValue is not null && !string.Equals(inMemValue, value);
            });
        }

        public void SetValue(string name, string? value)
        {
            LockedAction(() =>
            {
                // will refresh EncodedNameMap if needed
                string? inMemValue = GetValue(name);

                if (string.Equals(inMemValue, value))
                {
                    return;
                }

                SetValueImpl(name, value);

                SettingsChanged();

                if (string.IsNullOrEmpty(value))
                {
                    _byNameMap.TryRemove(name, out _);
                }
                else
                {
                    _byNameMap.AddOrUpdate(name, value, (key, oldValue) => value);
                }
            });
        }

        // This method will attempt to get the value from cache first. If the setting is not cached, it will call GetValue.
        // GetValue will not look in the cache. This method doesn't require a lock. A lock is only required when GetValue is
        // called. GetValue will set the lock.
        public bool TryGetValue(string name, [NotNullWhen(true)] out string? value)
        {
            value = default;

            EnsureSettingsAreUpToDate();

            if (_byNameMap.TryGetValue(name, out string? o))
            {
                switch (o)
                {
                    case null:
                        return false;
                    default:
                        value = o;
                        return true;
                }
            }

            string? s = GetValue(name);

            if (s is null)
            {
                value = default;
                return false;
            }

            value = s;
            _byNameMap.AddOrUpdate(name, s, (key, oldValue) => s);
            return true;
        }
    }
}
