using Avalonia.Controls;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;

namespace GitExtensionsTests;

internal static class RevisionRefLabelFixture
{
    public static IReadOnlyList<Control> Render(params IGitRef[] refs)
    {
        using AppSettingsTestScope settings = new();
        AppSettings.ShowTags = true;
        AppSettings.ShowRemoteBranches = true;
        AppSettings.FillRefLabels = false;
        AppSettings.ShowAnnotatedTagsMessages = false;

        RevisionGridControl grid = new();
        MessageColumnProvider provider = grid.ColumnProviders.OfType<MessageColumnProvider>().Single();
        provider.ApplySettings();
        GitRevision revision = new(ObjectId.Random()) { Refs = refs, Subject = string.Empty, Body = string.Empty };
        Control cell = provider.CreateCell();
        provider.UpdateCell(cell, revision);
        Control[] labels = cell.GetVisualDescendants().OfType<Control>().Where(control =>
            control is RevisionGridRefRenderer.NestledRefLabelPanel
            || (control is RevisionGridRefRenderer.RefLabelControl
                && control.GetVisualParent() is not RevisionGridRefRenderer.NestledRefLabelPanel)).ToArray();

        foreach (Control label in labels)
        {
            ((Panel)label.GetVisualParent()!).Children.Remove(label);
        }

        return labels;
    }
}
