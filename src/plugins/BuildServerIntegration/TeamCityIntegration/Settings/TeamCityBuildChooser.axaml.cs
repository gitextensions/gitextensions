using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace TeamCityIntegration.Settings;

public partial class TeamCityBuildChooser : GitExtensionsFormBase
{
    private readonly TeamCityAdapter _teamCityAdapter = new();
    private TreeViewItem? _previouslySelectedProject;
    public string TeamCityProjectName { get; private set; } = string.Empty;
    public string TeamCityBuildIdFilter { get; private set; } = string.Empty;

    public TeamCityBuildChooser()
    {
        InitializeComponent();
        treeViewTeamCityProjects.SelectionChanged += treeViewTeamCityProjects_AfterSelect;
        treeViewTeamCityProjects.DoubleTapped += treeViewTeamCityProjects_MouseDoubleClick;
        buttonOK.Click += buttonOK_Click;
        buttonCancel.Click += buttonCancel_Click;
        Opened += TeamCityBuildChooser_Load;
        InitializeComplete();
    }

    public TeamCityBuildChooser(string teamCityServerUrl, string teamCityProjectName, string teamCityBuildIdFilter)
        : this()
    {
        TeamCityProjectName = teamCityProjectName;
        TeamCityBuildIdFilter = teamCityBuildIdFilter;
        _teamCityAdapter.InitializeHttpClient(teamCityServerUrl);

        Project? rootProject = _teamCityAdapter.GetProjectsTree();

        if (rootProject is not null)
        {
            TreeViewItem rootTreeNode = LoadTreeView(treeViewTeamCityProjects, rootProject);

            rootTreeNode.IsExpanded = true;
        }
    }

    private void TeamCityBuildChooser_Load(object? sender, EventArgs e)
    {
        ReselectPreviouslySelectedBuild();
    }

    private void ReselectPreviouslySelectedBuild()
    {
        if (_previouslySelectedProject is null)
        {
            return;
        }

        _previouslySelectedProject.IsExpanded = true;
        TreeViewItem selectedNode = _previouslySelectedProject.Items
            .OfType<TreeViewItem>()
            .FirstOrDefault(node => node.Tag is Build build && build.Id == TeamCityBuildIdFilter)
            ?? _previouslySelectedProject;
        selectedNode.IsSelected = true;
    }

    private TreeViewItem LoadTreeView(TreeView treeView, Project rootProject)
    {
        treeView.Items.Clear();
        TreeViewItem rootNode = ConvertProjectInTreeNode(rootProject);
        treeView.Items.Add(rootNode);
        return rootNode;
    }

    private TreeViewItem ConvertProjectInTreeNode(Project project)
    {
        TreeViewItem projectNode = new()
        {
            Header = project.Name,
            Tag = project,
        };
        projectNode.Expanded += treeViewTeamCityProjects_BeforeExpand;

        foreach (TreeViewItem child in project.SubProjects!
                     .Select(ConvertProjectInTreeNode)
                     .OrderBy(item => item.Header?.ToString()))
        {
            projectNode.Items.Add(child);
        }

        if (projectNode.Items.Count == 0)
        {
            projectNode.Items.Add(new TreeViewItem { Header = "Loading..." });
        }

        if (TeamCityProjectName == project.Id)
        {
            _previouslySelectedProject = projectNode;
        }

        return projectNode;
    }

    private void treeViewTeamCityProjects_BeforeExpand(object? sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, e.Source) && sender is TreeViewItem treeNode)
        {
            LoadProjectBuilds(treeNode);
        }
    }

    private void LoadProjectBuilds(TreeViewItem treeNode)
    {
        Project project = (Project)treeNode.Tag!;
        if (project.Builds is null)
        {
            Validates.NotNull(project.Id);

            project.Builds = _teamCityAdapter.GetProjectBuilds(project.Id);

            // Remove "Loading..." node
            if (treeNode.Items.Count == 1 && treeNode.Items[0] is TreeViewItem { Tag: null })
            {
                treeNode.Items.RemoveAt(0);
            }

            IBrush buildForeground = GetBuildForeground();
            foreach (Build build in project.Builds.OrderBy(build => build.Id))
            {
                treeNode.Items.Add(new TreeViewItem
                {
                    Header = new TextBlock
                    {
                        Text = build.DisplayName,
                        Foreground = buildForeground,
                    },
                    Tag = build,
                });
            }
        }
    }

    private void treeViewTeamCityProjects_MouseDoubleClick(object? sender, TappedEventArgs e)
    {
        SelectBuild();
    }

    private void buttonOK_Click(object? sender, EventArgs e)
    {
        SelectBuild();
    }

    private void SelectBuild()
    {
        if (treeViewTeamCityProjects.SelectedItem is TreeViewItem { Tag: Build build })
        {
            Validates.NotNull(build.ParentProject);
            Validates.NotNull(build.Id);

            TeamCityProjectName = build.ParentProject;
            TeamCityBuildIdFilter = build.Id;

            DialogResult = WinFormsShims.DialogResult.OK;
        }
    }

    private void buttonCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DialogResult == WinFormsShims.DialogResult.None)
        {
            SetDialogResultOnClose(WinFormsShims.DialogResult.Cancel);
        }

        base.OnClosing(e);
    }

    private static bool IsBuildSelected(TreeViewItem? selectedNode)
    {
        return selectedNode?.Tag is Build;
    }

    private void treeViewTeamCityProjects_AfterSelect(object? sender, SelectionChangedEventArgs e)
    {
        buttonOK.IsEnabled = IsBuildSelected(treeViewTeamCityProjects.SelectedItem as TreeViewItem);
    }

    private IBrush GetBuildForeground()
        => Avalonia.Application.Current?.TryGetResource(
            "GitExtensionsSelectionBackgroundBrush",
            ActualThemeVariant,
            out object? resource) == true
            && resource is IBrush brush
                ? brush
                : Brushes.Blue;
}
