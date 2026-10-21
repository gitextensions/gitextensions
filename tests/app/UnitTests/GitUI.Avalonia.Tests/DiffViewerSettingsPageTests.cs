using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class DiffViewerSettingsPageTests
{
    [AvaloniaTest]
    public void FormSettings_should_register_diff_viewer_beneath_detailed_and_navigate_to_it()
    {
        GitUI.ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        FormSettings form = new();
        FormSettings.TestAccessor accessor = form.GetTestAccessor();
        accessor.InitializePages();
        DetailedSettingsPage detailed = accessor.SettingsTreeView.SettingsPages
            .OfType<DetailedSettingsPage>()
            .Single();
        DiffViewerSettingsPage diffViewer = accessor.SettingsTreeView.SettingsPages
            .OfType<DiffViewerSettingsPage>()
            .Single();

        TreeView tree = accessor.SettingsTreeView.FindControl<TreeView>("treeView1")
            ?? throw new InvalidOperationException("The settings tree was not created.");
        TreeViewItem detailedNode = tree.Items
            .OfType<TreeViewItem>()
            .SelectMany(node => node.Items.OfType<TreeViewItem>())
            .Single(node => node.Tag is DetailedSettingsPage);
        detailedNode.Tag.Should().BeSameAs(detailed);
        detailedNode.Items
            .OfType<TreeViewItem>()
            .Select(node => node.Tag)
            .Should().ContainSingle().Which.Should().BeSameAs(diffViewer);

        form.GotoPage(DiffViewerSettingsPage.GetPageReference());

        SettingsPageHeader header = accessor.CurrentPage.Should().BeOfType<SettingsPageHeader>().Subject;
        header.GetTestAccessor().Page.Should().BeSameAs(diffViewer);
        diffViewer.GetTitle().Should().Be("Diff viewer");
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void Diff_viewer_settings_should_roundtrip_all_values()
    {
        bool originalRememberWhitespace = AppSettings.RememberIgnoreWhiteSpacePreference;
        bool originalOmitUninteresting = AppSettings.OmitUninterestingDiff;
        bool originalRememberEntireFile = AppSettings.RememberShowEntireFilePreference;
        bool originalRememberAppearance = AppSettings.RememberDiffDisplayAppearance.Value;
        bool originalRememberNonPrinting = AppSettings.RememberShowNonPrintingCharsPreference;
        bool originalRememberContext = AppSettings.RememberNumberOfContextLines;
        bool originalRememberSyntax = AppSettings.RememberShowSyntaxHighlightingInDiff;
        bool originalOpenSubmodule = AppSettings.OpenSubmoduleDiffInSeparateWindow;
        bool originalContinuousScroll = AppSettings.AutomaticContinuousScroll;
        bool originalShowAllParents = AppSettings.ShowDiffForAllParents;
        bool originalShowDiffTools = AppSettings.ShowAvailableDiffTools;
        int originalRuler = AppSettings.DiffVerticalRulerPosition;
        bool originalGitColoring = AppSettings.UseGitColoring.Value;
        bool originalReverseColoring = AppSettings.ReverseGitColoring.Value;
        try
        {
            AppSettings.RememberIgnoreWhiteSpacePreference = true;
            AppSettings.OmitUninterestingDiff = false;
            AppSettings.RememberShowEntireFilePreference = true;
            AppSettings.RememberDiffDisplayAppearance.Value = false;
            AppSettings.RememberShowNonPrintingCharsPreference = true;
            AppSettings.RememberNumberOfContextLines = false;
            AppSettings.RememberShowSyntaxHighlightingInDiff = true;
            AppSettings.OpenSubmoduleDiffInSeparateWindow = false;
            AppSettings.AutomaticContinuousScroll = true;
            AppSettings.ShowDiffForAllParents = false;
            AppSettings.ShowAvailableDiffTools = true;
            AppSettings.DiffVerticalRulerPosition = 91;
            AppSettings.UseGitColoring.Value = true;
            AppSettings.ReverseGitColoring.Value = false;

            DiffViewerSettingsPage page = new();
            page.LoadSettings();
            DiffViewerSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.RememberIgnoreWhitespace.IsChecked.Should().BeTrue();
            accessor.OmitUninterestingDiff.IsChecked.Should().BeFalse();
            accessor.RememberEntireFile.IsChecked.Should().BeTrue();
            accessor.RememberDiffAppearance.IsChecked.Should().BeFalse();
            accessor.RememberNonPrinting.IsChecked.Should().BeTrue();
            accessor.RememberContextLines.IsChecked.Should().BeFalse();
            accessor.RememberSyntaxHighlighting.IsChecked.Should().BeTrue();
            accessor.OpenSubmoduleSeparately.IsChecked.Should().BeFalse();
            accessor.AutomaticContinuousScroll.IsChecked.Should().BeTrue();
            accessor.ShowAllParents.IsChecked.Should().BeFalse();
            accessor.ShowAllDiffTools.IsChecked.Should().BeTrue();
            accessor.VerticalRulerPosition.Value.Should().Be(91);
            accessor.UseGitColoring.IsChecked.Should().BeTrue();
            accessor.ReverseGitColoring.IsChecked.Should().BeFalse();
            accessor.ReverseGitColoring.IsEnabled.Should().BeTrue();

            accessor.RememberIgnoreWhitespace.IsChecked = false;
            accessor.OmitUninterestingDiff.IsChecked = true;
            accessor.RememberEntireFile.IsChecked = false;
            accessor.RememberDiffAppearance.IsChecked = true;
            accessor.RememberNonPrinting.IsChecked = false;
            accessor.RememberContextLines.IsChecked = true;
            accessor.RememberSyntaxHighlighting.IsChecked = false;
            accessor.OpenSubmoduleSeparately.IsChecked = true;
            accessor.AutomaticContinuousScroll.IsChecked = false;
            accessor.ShowAllParents.IsChecked = true;
            accessor.ShowAllDiffTools.IsChecked = false;
            accessor.VerticalRulerPosition.Value = 73;
            accessor.UseGitColoring.IsChecked = false;
            accessor.ReverseGitColoring.IsChecked = true;
            page.SaveSettings();

            AppSettings.RememberIgnoreWhiteSpacePreference.Should().BeFalse();
            AppSettings.OmitUninterestingDiff.Should().BeTrue();
            AppSettings.RememberShowEntireFilePreference.Should().BeFalse();
            AppSettings.RememberDiffDisplayAppearance.Value.Should().BeTrue();
            AppSettings.RememberShowNonPrintingCharsPreference.Should().BeFalse();
            AppSettings.RememberNumberOfContextLines.Should().BeTrue();
            AppSettings.RememberShowSyntaxHighlightingInDiff.Should().BeFalse();
            AppSettings.OpenSubmoduleDiffInSeparateWindow.Should().BeTrue();
            AppSettings.AutomaticContinuousScroll.Should().BeFalse();
            AppSettings.ShowDiffForAllParents.Should().BeTrue();
            AppSettings.ShowAvailableDiffTools.Should().BeFalse();
            AppSettings.DiffVerticalRulerPosition.Should().Be(73);
            AppSettings.UseGitColoring.Value.Should().BeFalse();
            AppSettings.ReverseGitColoring.Value.Should().BeTrue();
        }
        finally
        {
            AppSettings.RememberIgnoreWhiteSpacePreference = originalRememberWhitespace;
            AppSettings.OmitUninterestingDiff = originalOmitUninteresting;
            AppSettings.RememberShowEntireFilePreference = originalRememberEntireFile;
            AppSettings.RememberDiffDisplayAppearance.Value = originalRememberAppearance;
            AppSettings.RememberShowNonPrintingCharsPreference = originalRememberNonPrinting;
            AppSettings.RememberNumberOfContextLines = originalRememberContext;
            AppSettings.RememberShowSyntaxHighlightingInDiff = originalRememberSyntax;
            AppSettings.OpenSubmoduleDiffInSeparateWindow = originalOpenSubmodule;
            AppSettings.AutomaticContinuousScroll = originalContinuousScroll;
            AppSettings.ShowDiffForAllParents = originalShowAllParents;
            AppSettings.ShowAvailableDiffTools = originalShowDiffTools;
            AppSettings.DiffVerticalRulerPosition = originalRuler;
            AppSettings.UseGitColoring.Value = originalGitColoring;
            AppSettings.ReverseGitColoring.Value = originalReverseColoring;
        }
    }

    [AvaloniaTest]
    [NonParallelizable]
    public void Detailed_settings_should_roundtrip_its_original_values()
    {
        SettingsSource settings = AppSettings.SettingsContainer;
        bool originalMergeLanes = AppSettings.MergeGraphLanesHavingCommonParent.Value;
        bool originalDiagonals = AppSettings.RenderGraphWithDiagonals.Value;
        bool originalStraighten = AppSettings.StraightenGraphDiagonals.Value;
        bool? originalRemotes = DetailedSettings.GetRemoteBranchesDirectlyFromRemote[settings];
        bool? originalLogMessages = DetailedSettings.AddMergeLogMessages[settings];
        object? originalMessageCount = DetailedSettings.MergeLogMessagesCount[settings];
        try
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = true;
            AppSettings.RenderGraphWithDiagonals.Value = false;
            AppSettings.StraightenGraphDiagonals.Value = true;
            DetailedSettings.GetRemoteBranchesDirectlyFromRemote[settings] = false;
            DetailedSettings.AddMergeLogMessages[settings] = true;
            DetailedSettings.MergeLogMessagesCount[settings] = 12;

            DetailedSettingsPage page = new();
            page.LoadSettings();
            DetailedSettingsPage.TestAccessor accessor = page.GetTestAccessor();
            accessor.MergeGraphLanesHavingCommonParent.IsChecked.Should().BeTrue();
            accessor.RenderGraphWithDiagonals.IsChecked.Should().BeFalse();
            accessor.StraightenGraphDiagonals.IsChecked.Should().BeTrue();
            accessor.RemotesFromServer.IsChecked.Should().BeFalse();
            accessor.AddLogMessages.IsChecked.Should().BeTrue();
            accessor.NumberOfMessages.Text.Should().Be("12");

            accessor.MergeGraphLanesHavingCommonParent.IsChecked = false;
            accessor.RenderGraphWithDiagonals.IsChecked = true;
            accessor.StraightenGraphDiagonals.IsChecked = false;
            accessor.RemotesFromServer.IsChecked = true;
            accessor.AddLogMessages.IsChecked = false;
            accessor.NumberOfMessages.Text = "24";
            accessor.SaveSettings();

            AppSettings.MergeGraphLanesHavingCommonParent.Value.Should().BeFalse();
            AppSettings.RenderGraphWithDiagonals.Value.Should().BeTrue();
            AppSettings.StraightenGraphDiagonals.Value.Should().BeFalse();
            DetailedSettings.GetRemoteBranchesDirectlyFromRemote.ValueOrDefault(settings).Should().BeTrue();
            DetailedSettings.AddMergeLogMessages.ValueOrDefault(settings).Should().BeFalse();
            DetailedSettings.MergeLogMessagesCount.ValueOrDefault(settings).Should().Be(24);
        }
        finally
        {
            AppSettings.MergeGraphLanesHavingCommonParent.Value = originalMergeLanes;
            AppSettings.RenderGraphWithDiagonals.Value = originalDiagonals;
            AppSettings.StraightenGraphDiagonals.Value = originalStraighten;
            DetailedSettings.GetRemoteBranchesDirectlyFromRemote[settings] = originalRemotes;
            DetailedSettings.AddMergeLogMessages[settings] = originalLogMessages;
            DetailedSettings.MergeLogMessagesCount[settings] = originalMessageCount;
        }
    }

    [AvaloniaTest]
    public void Diff_viewer_settings_should_preserve_original_translation_keys_and_tooltips()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        DiffViewerSettingsPage page = new();

        page.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(DiffViewerSettingsPage), "gbGeneral", "Text", "General");
        translation.Received(1).AddTranslationItem(
            nameof(DiffViewerSettingsPage), "chkContScrollToNextFileOnlyWithAlt", "Text", GitUI.TranslatedStrings.ContScrollToNextFileOnlyWithAlt);
        translation.Received(1).AddTranslationItem(
            nameof(DiffViewerSettingsPage),
            "chkShowAllCustomDiffTools",
            "ToolTipText",
            "Show all configured difftools in a dropdown.\nThe primary difftool can still be selected by clicking the main menu entry.");
        translation.Received(1).AddTranslationItem(
            nameof(DiffViewerSettingsPage),
            "_saveCurrentViewSettingsAsDefaultTooltip",
            "Text",
            Arg.Any<string>());
        ToolTip.GetTip(page.GetTestAccessor().SaveCurrentViewSettingsAsDefault)
            .Should().BeOfType<string>().Which.Should().Contain("Saves all current view settings");
    }
}
