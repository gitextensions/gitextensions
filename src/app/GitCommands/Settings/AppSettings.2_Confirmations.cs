using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting ConfirmBranchCheckout { get; } = new(ConfirmationsSettingsPath, nameof(ConfirmBranchCheckout), defaultValue: false);
}
