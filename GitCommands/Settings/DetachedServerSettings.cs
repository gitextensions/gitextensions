using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;

namespace GitCommands.Settings
{
    internal sealed class DetachedSettings : IDetachedSettings
    {
        private const string DictionaryDefault = "en-US";
        private const bool NoFastForwardMergeDefault = false;

        private readonly ISettingsSource _settingsSource;

        public DetachedSettings(ISettingsSource settingsSource)
        {
            _settingsSource = settingsSource;
        }

        public string Dictionary
        {
            get => _settingsSource.GetString(nameof(Dictionary).ToLower(), DictionaryDefault);
            set
            {
                if (Dictionary == value)
                {
                    return;
                }

                _settingsSource.SetString(nameof(Dictionary).ToLower(), value);
            }
        }

        public bool NoFastForwardMerge
        {
            get => _settingsSource.GetBool(nameof(NoFastForwardMerge), NoFastForwardMergeDefault);
            set
            {
                if (NoFastForwardMerge == value)
                {
                    return;
                }

                _settingsSource.SetBool(nameof(NoFastForwardMerge), value);
            }
        }
    }
}
