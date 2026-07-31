using GitCommands;

namespace GitUI.AI;

/// <summary>
/// Which "noise" categories are currently enabled for filtering.
/// </summary>
public sealed record DiffNoiseFilterOptions(
    bool Imports,
    bool CallerSiteRename,
    bool SyncToAsync,
    bool StyleOnly)
{
    public bool AnyEnabled => Imports || CallerSiteRename || SyncToAsync || StyleOnly;

    /// <summary>Reads the current option values from <see cref="AppSettings"/>.</summary>
    public static DiffNoiseFilterOptions FromSettings()
        => new(
            Imports: AppSettings.AiFilterImports.Value,
            CallerSiteRename: AppSettings.AiFilterCallerSiteRenames.Value,
            SyncToAsync: AppSettings.AiFilterSyncToAsync.Value,
            StyleOnly: AppSettings.AiFilterStyleOnly.Value);

    /// <summary>Whether the given category should be hidden according to these options.</summary>
    public bool IsHidden(DiffNoiseCategory category)
        => category switch
        {
            DiffNoiseCategory.Imports => Imports,
            DiffNoiseCategory.CallerSiteRename => CallerSiteRename,
            DiffNoiseCategory.SyncToAsync => SyncToAsync,
            DiffNoiseCategory.StyleOnly => StyleOnly,
            _ => false
        };
}
