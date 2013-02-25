using System;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    /// <summary>
    /// to jump to a specific page
    /// 
    /// TODO: extend with attributes to jump to specific control on settingspage
    /// </summary>
    public abstract class SettingsPageReference
    { 
    }

    public class SettingsPageReferenceByType : SettingsPageReference
    {
        private readonly Type _settingsPageType;

        public SettingsPageReferenceByType(Type settingsPageType)
        {
            _settingsPageType = settingsPageType;
        }

        public Type SettingsPageType { get { return _settingsPageType; } }

        public override bool Equals(object obj)
        {
            return obj is SettingsPageReferenceByType && ((SettingsPageReferenceByType)obj).SettingsPageType.Equals(SettingsPageType);
        }

        public override int GetHashCode()
        {
            return SettingsPageType.GetHashCode();
        }

        public override string ToString()
        {
            return SettingsPageType.ToString();
        }
    }
}
