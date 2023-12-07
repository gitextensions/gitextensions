using System.Diagnostics.CodeAnalysis;
using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings
{
    public class SettingsContainer<TLowerPriority, TCache> : SettingsSource
        where TLowerPriority : SettingsContainer<TLowerPriority, TCache>
        where TCache : SettingsCache
    {
        private readonly ICredentialsManager _credentialsManager = new CredentialsManager();

        public TLowerPriority? LowerPriority { get; }
        public TCache SettingsCache { get; }

        public SettingsContainer(TLowerPriority? lowerPriority, TCache settingsCache)
        {
            LowerPriority = lowerPriority;
            SettingsCache = settingsCache;
        }

        public void LockedAction(Action action)
        {
            SettingsCache.LockedAction(() =>
                {
                    if (LowerPriority is not null)
                    {
                        LowerPriority.LockedAction(action);
                    }
                    else
                    {
                        action();
                    }
                });
        }

        public void Save()
        {
            _credentialsManager.Save();
            SettingsCache.Save();
            LowerPriority?.Save();
        }

        public override string? GetValue(string name)
        {
            TryGetValue(name, out string? value);
            return value;
        }

        /// <summary>
        /// sets given value at the possible lowest priority level
        /// </summary>
        public override void SetValue(string name, string? value)
        {
            if (LowerPriority is null || SettingsCache.HasValue(name))
            {
                SettingsCache.SetValue(name, value);
            }
            else
            {
                LowerPriority.SetValue(name, value);
            }
        }

        public virtual bool TryGetValue(string name, [NotNullWhen(true)] out string? value)
            => SettingsCache.TryGetValue(name, out value) || LowerPriority?.TryGetValue(name, out value) is true;
    }
}
