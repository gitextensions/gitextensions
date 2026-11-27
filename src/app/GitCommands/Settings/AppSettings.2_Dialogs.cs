using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting CheckoutOtherBranchAfterReset { get; } = new(DialogSettingsPath, nameof(CheckoutOtherBranchAfterReset), defaultValue: true);

    public static BooleanSetting CommitDialogSelectStagedOnEnterMessage { get; } = new(DialogSettingsPath, nameof(CommitDialogSelectStagedOnEnterMessage), defaultValue: true);

    public static BooleanSetting GitGrepIgnoreCase { get; } = new(DialogSettingsPath, nameof(GitGrepIgnoreCase), defaultValue: false);

    public static BooleanSetting GitGrepMatchWholeWord { get; } = new(DialogSettingsPath, nameof(GitGrepMatchWholeWord), defaultValue: false);

    public static TextSetting GitGrepUserArguments { get; } = new(DialogSettingsPath, nameof(GitGrepUserArguments), defaultValue: "");
}
