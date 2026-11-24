using GitCommands.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    public static BooleanSetting CheckoutOtherBranchAfterReset { get; } = new(_dialogSettingsPath, nameof(CheckoutOtherBranchAfterReset), defaultValue: true);

    public static BooleanSetting CommitDialogSelectStagedOnEnterMessage { get; } = new(_dialogSettingsPath, nameof(CommitDialogSelectStagedOnEnterMessage), defaultValue: true);

    public static BooleanSetting GitGrepIgnoreCase { get; } = new(_dialogSettingsPath, nameof(GitGrepIgnoreCase), defaultValue: false);

    public static BooleanSetting GitGrepMatchWholeWord { get; } = new(_dialogSettingsPath, nameof(GitGrepMatchWholeWord), defaultValue: false);

    public static TextSetting GitGrepUserArguments { get; } = new(_dialogSettingsPath, nameof(GitGrepUserArguments), defaultValue: "");
}
