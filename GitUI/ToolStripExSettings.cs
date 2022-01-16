#nullable enable

using System.Configuration;
using System.Drawing;

namespace GitUI
{
    // Inspired by https://github.com/dotnet/winforms/blob/main/src/System.Windows.Forms/src/System/Windows/Forms/ToolStripSettings.cs

    /// <summary>
    ///  A settings class used by the ToolStripManager to save toolstrip settings.
    /// </summary>
    internal partial class ToolStripExSettings : ApplicationSettingsBase
    {
        internal ToolStripExSettings(string settingsKey)
            : base(settingsKey)
        {
        }

        // Indicates whether the settings was persisted, and not a "default" settigns object when there's no settings in user.config.
        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool IsDefault
        {
            get => (bool)this[nameof(IsDefault)];
            set => this[nameof(IsDefault)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("0,0")]
        public Point Location
        {
            get => (Point)this[nameof(Location)];
            set => this[nameof(Location)] = value;
        }

        [UserScopedSetting]
        public string? ToolStripPanelName
        {
            get => this[nameof(ToolStripPanelName)] as string;
            set => this[nameof(ToolStripPanelName)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool Visible
        {
            get => (bool)this[nameof(Visible)];
            set => this[nameof(Visible)] = value;
        }

        public override void Save()
        {
            IsDefault = false;
            base.Save();
        }
    }
}
