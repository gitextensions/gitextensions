using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class AvatarColumnProvider : ColumnProvider
{
    public AvatarColumnProvider()
        : base("Avatar", new GridLength(32), minimumWidth: 32, resizable: false)
    {
        // The provider slot is intentionally real but unavailable until the avatar service is ported.
        Column.IsAvailable = false;
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowAuthorAvatarColumn;
    }

    public override Control CreateCell()
    {
        Image image = new() { Margin = new Thickness(2) };
        image.Classes.Add("revision-avatar-cell");
        return image;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        ((Image)control).Source = null;
        UpdateToolTip(control, revision);
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (revision.IsArtificial)
        {
            toolTip = null;
            return false;
        }

        toolTip = AuthorNameColumnProvider.GetAuthorAndCommiterToolTip(revision);
        return true;
    }
}
