using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TeamCityIntegration.Settings
{
    public partial class TeamCityBuildChooser : Form
    {
        private readonly TeamCityAdapter _teamCityAdapter = new TeamCityAdapter();
        private TreeNode _previouslySelectedProject;
        public string TeamCityProjectName { get; private set; }
        public string TeamCityBuildIdFilter { get; private set; }

        public TeamCityBuildChooser(string teamCityServerUrl, string teamCityProjectName, string teamCityBuildIdFilter)
        {
            InitializeComponent();

            TeamCityProjectName = teamCityProjectName;
            TeamCityBuildIdFilter = teamCityBuildIdFilter;
            _teamCityAdapter.InitializeHttpClient(teamCityServerUrl);

            var rootProject = _teamCityAdapter.GetProjectsTree();
            var rootTreeNode = LoadTreeView(treeViewTeamCityProjects, rootProject);

            rootTreeNode.Expand();
        }

        private void TeamCityBuildChooser_Load(object sender, EventArgs e)
        {
            ReselectPreviouslySelectedBuild();
        }

        private void ReselectPreviouslySelectedBuild()
        {
            if (_previouslySelectedProject == null)
            {
                return;
            }

            _previouslySelectedProject.Expand();
            treeViewTeamCityProjects.SelectedNode = _previouslySelectedProject.Nodes.Find(TeamCityBuildIdFilter, false).FirstOrDefault()
                ?? _previouslySelectedProject;
        }

        private TreeNode LoadTreeView(TreeView treeView, Project rootProject)
        {
            treeView.Nodes.Clear();
            var rootNode = ConvertProjectInTreeNode(rootProject);
            treeView.Nodes.Add(rootNode);
            return rootNode;
        }

        private TreeNode ConvertProjectInTreeNode(Project project)
        {
            var projectNode = new TreeNode(project.Name)
            {
                Name = project.Name,
                Tag = project,
            };

            projectNode.Nodes.AddRange(project.SubProjects.Select(ConvertProjectInTreeNode).OrderBy(p => p.Name).ToArray());
            if (projectNode.Nodes.Count == 0)
            {
                projectNode.Nodes.Add(new TreeNode("Loading..."));
            }

            if (TeamCityProjectName == project.Id)
            {
                _previouslySelectedProject = projectNode;
            }

            return projectNode;
        }

        private void treeViewTeamCityProjects_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            LoadProjectBuilds(e.Node);
        }

        private void LoadProjectBuilds(TreeNode treeNode)
        {
            var project = (Project)treeNode.Tag;
            if (project.Builds == null)
            {
                project.Builds = _teamCityAdapter.GetProjectBuilds(project.Id);

                // Remove "Loading..." node
                if (treeNode.Nodes.Count == 1 && treeNode.Nodes[0].Tag == null)
                {
                    treeNode.Nodes.RemoveAt(0);
                }

                var buildNodes = project.Builds.Select(b => new TreeNode(b.DisplayName)
                {
                    Name = b.Id,
                    ForeColor = Color.Blue,
                    Tag = b
                }).OrderBy(b => b.Name).ToArray();
                treeNode.Nodes.AddRange(buildNodes);
            }
        }

        private void treeViewTeamCityProjects_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SelectBuild();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SelectBuild();
        }

        private void SelectBuild()
        {
            if (treeViewTeamCityProjects.SelectedNode?.Tag is Build build)
            {
                TeamCityProjectName = build.ParentProject;
                TeamCityBuildIdFilter = build.Id;

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private static bool IsBuildSelected(TreeNode selectedNode)
        {
            return selectedNode?.Tag is Build;
        }

        private void treeViewTeamCityProjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            buttonOK.Enabled = IsBuildSelected(e.Node);
        }
    }
}
