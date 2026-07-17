using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Properties;
using ImagesResxToAvalonia;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ImageResourceTests
{
    [Test]
    public void Generated_images_api_should_match_Images_resx()
    {
        string repositoryRoot = FindRepositoryRoot();
        string resxPath = Path.Combine(repositoryRoot, "src", "app", "GitUI", "Properties", "Images.resx");
        string generatedPath = Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia", "Properties", "Images.g.cs");

        File.ReadAllText(generatedPath).ReplaceLineEndings("\n").Should().Be(ImagesGenerator.Generate(resxPath));
    }

    [AvaloniaTest]
    public void Every_generated_image_should_be_packaged_and_cached()
    {
        Images.ResourcePaths.Should().NotBeEmpty();

        Dictionary<string, PropertyInfo> bitmapProperties = typeof(Images)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(Bitmap))
            .ToDictionary(property => property.Name, StringComparer.Ordinal);
        bitmapProperties.Keys.Should().BeEquivalentTo(Images.ResourcePaths.Keys);

        foreach ((string name, string resourcePath) in Images.ResourcePaths)
        {
            Uri uri = new("avares://GitUI.Avalonia/" + resourcePath);
            using Stream stream = AssetLoader.Open(uri);
            stream.Length.Should().BeGreaterThan(0, $"{name} should resolve {uri}");

            Bitmap bitmap = (Bitmap)bitmapProperties[name].GetValue(null)!;
            bitmap.PixelSize.Width.Should().BeGreaterThan(0, $"{name} should decode {uri}");
            bitmap.PixelSize.Height.Should().BeGreaterThan(0, $"{name} should decode {uri}");
        }

        Images.Push.Should().BeSameAs(Images.Push);
        Images.Push.PixelSize.Width.Should().BeGreaterThan(0);
        Images.Push.PixelSize.Height.Should().BeGreaterThan(0);
        Images.ApplicationIcon.Should().BeSameAs(Images.ApplicationIcon);
    }

    [AvaloniaTest]
    public void Icon_controls_should_keep_translatable_content_and_render_the_cached_bitmap()
    {
        IconButton button = new()
        {
            Content = "_Push",
            Icon = Images.Push,
        };
        Window window = new()
        {
            Content = button,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            button.Content.Should().Be("_Push", "the original .Text translation path expects string content");
            button.GetVisualDescendants()
                .OfType<Image>()
                .Should().ContainSingle()
                .Which.Source.Should().BeSameAs(Images.Push);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Existing_views_should_use_shared_icons_for_menus_trees_statuses_and_windows()
    {
        FormBrowse form = new();
        RepoObjectsTree tree = new();
        FileStatusList fileStatusList = new();
        Window listWindow = new()
        {
            Width = 300,
            Height = 120,
            Content = fileStatusList,
        };

        try
        {
            form.Icon.Should().BeSameAs(Images.ApplicationIcon);
            MenuItem commit = form.FindControl<MenuItem>("commitToolStripMenuItem")!;
            commit.Icon.Should().BeOfType<Image>()
                .Which.Source.Should().BeSameAs(Images.RepoStateClean);

            tree.SetRefs([]);
            TreeView treeMain = tree.FindControl<TreeView>("treeMain")!;
            TreeViewItem branches = treeMain.Items.Cast<TreeViewItem>().First();
            branches.Header.Should().BeOfType<StackPanel>()
                .Which.Children.OfType<Image>().Should().ContainSingle()
                .Which.Source.Should().BeSameAs(Images.BranchLocalRoot);

            fileStatusList.SetDiffs([new GitItemStatus("new-file.txt") { IsNew = true }]);
            listWindow.Show();
            Dispatcher.UIThread.RunJobs();
            fileStatusList.GetVisualDescendants()
                .OfType<Image>()
                .Should().Contain(image => ReferenceEquals(image.Source, Images.FileStatusAdded));
        }
        finally
        {
            listWindow.Close();
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Dialog_icons_should_preserve_each_supported_semantic()
    {
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.None).Should().BeNull();
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Information)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Information);
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Warning)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Warning);
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Error)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.StatusBadgeError);
        DialogIconFactory.Create(WinFormsShims.MessageBoxIcon.Question).Should().BeOfType<Border>();

        DialogIconFactory.Create(TaskDialogIcon.Information)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Information);
        DialogIconFactory.Create(TaskDialogIcon.Warning)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.Warning);
        DialogIconFactory.Create(TaskDialogIcon.Error)
            .Should().BeOfType<Image>().Which.Source.Should().BeSameAs(Images.StatusBadgeError);
    }

    private static string FindRepositoryRoot([CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from {startPath}.");
    }
}
