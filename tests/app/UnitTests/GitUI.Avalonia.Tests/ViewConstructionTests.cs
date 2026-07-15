using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommitInfo;
using GitUI.Editor;
using GitUI.LeftPanel;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ViewConstructionTests
{
    [Test]
    public void Translator_should_find_the_shared_translations()
    {
        Translator.GetAllTranslations().Should().NotBeEmpty();
    }

    [AvaloniaTest]
    public void FormBrowse_should_construct()
    {
        FormBrowse form = new();
        form.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void RevisionGridControl_should_construct()
    {
        RevisionGridControl control = new();
        control.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FileStatusList_should_construct()
    {
        FileStatusList control = new();
        control.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FileViewer_should_construct_and_view_a_patch()
    {
        FileViewer control = new();
        control.ViewPatch("@@ -1,1 +1,1 @@\n-old\n+new\n");
        control.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void CommitInfo_should_construct()
    {
        CommitInfo control = new();
        control.Revision.Should().BeNull();
    }

    [AvaloniaTest]
    public void RepoObjectsTree_should_construct()
    {
        RepoObjectsTree control = new();
        control.SetRefs([]);
        control.SelectedRef.Should().BeNull();
    }
}
