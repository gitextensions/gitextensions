using System;
using System.Linq;
using System.Windows.Forms;

namespace TeamCityIntegration.Settings
{
    public partial class TeamCityBuildChooser : Form
    {
        private TeamCityAdapter _teamCityAdapter = new TeamCityAdapter();
        private TreeNode _previouslySelectedBuild;
        private string _teamCityServerUrl;

        public TeamCityBuildChooser(string teamCityServerUrl, string teamCityProjectName, string teamCityBuildIdFilter)
        {
            InitializeComponent();

            TeamCityProjectName = teamCityProjectName;
            TeamCityBuildIdFilter = teamCityBuildIdFilter;
            _teamCityServerUrl = teamCityServerUrl;
        }

        struct Node
        {
            public string Name;
            public bool Loaded;
            public bool IsProject;
            public string ParentProject;
        }

        public string TeamCityProjectName { get; set; }
        public string TeamCityBuildIdFilter { get; set; }

        public void LoadProjectsAndBuilds(string teamCityServerUrl)
        {
            _teamCityAdapter.InitializeHttpClient(teamCityServerUrl);
            var projects = _teamCityAdapter.GetAllProjects();
            var loadingNode = new TreeNode("loading...");
            treeViewTeamCityProjects.Nodes.Clear();
            treeViewTeamCityProjects.Nodes.AddRange(projects.Select(p => new TreeNode(p)
            {
                Name = p,
                Tag = new Node {IsProject = true, Loaded = false, Name = p}
            }).OrderBy(p=>p.Name).ToArray());

            foreach (TreeNode node in treeViewTeamCityProjects.Nodes)
            {
                node.Nodes.Add((TreeNode) loadingNode.Clone());
            }

            if(!string.IsNullOrWhiteSpace(TeamCityProjectName))
            {
                foreach (TreeNode node in treeViewTeamCityProjects.Nodes)
                {
                    if(node.Name == TeamCityProjectName)
                    {
                        treeViewTeamCityProjects.SelectedNode = node;
                        node.Expand();
                        break;
                    }
                }
            }
        }

        private void treeViewTeamCityProjects_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            LoadSubProjects(e.Node);
        }

        private void LoadSubProjects(TreeNode treeNode)
        {
            var node = (Node)treeNode.Tag;
            if (node.IsProject && !node.Loaded)
            {
                treeNode.Nodes.Clear();
                var project = _teamCityAdapter.GetProjectChildren(node.Name);

                var projectNodes = project.Projects.Select(p => new TreeNode(p)
                {
                    Name = p,
                    Tag = new Node { IsProject = true, Loaded = false, Name = p, ParentProject = node.Name }
                }).OrderBy(p => p.Name).ToArray();
                treeNode.Nodes.AddRange(projectNodes);

                var buildNodes = project.Builds.Select(b => new TreeNode(b.Name + " (" + b.Id + ")")
                {
                    Name = b.Id,
                    Tag = new Node { IsProject = false, Loaded = true, Name = b.Id, ParentProject = node.Name }
                }).OrderBy(p => p.Name).ToArray();
                treeNode.Nodes.AddRange(buildNodes);

                //Find previous already selected build in the treeview
                var previouslySelectedBuild = buildNodes.FirstOrDefault(n => n.Name == TeamCityBuildIdFilter
                    && n.Parent.Name == TeamCityProjectName);
                if (previouslySelectedBuild != null)
                    _previouslySelectedBuild = previouslySelectedBuild;
            }
        }

        private void treeViewTeamCityProjects_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SelectBuild();
        }

        private void SelectBuild()
        {
            if (treeViewTeamCityProjects.SelectedNode == null)
                return;

            var node = (Node) treeViewTeamCityProjects.SelectedNode.Tag;
            if (node.IsProject)
                return;
            TeamCityProjectName = node.ParentProject;
            TeamCityBuildIdFilter = node.Name;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SelectBuild();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool IsBuildSelected(TreeNode selectedNode)
        {
            if (selectedNode == null)
                return false;

            var node = (Node)selectedNode.Tag;
            return !node.IsProject;
        }

        private void treeViewTeamCityProjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            buttonOK.Enabled = IsBuildSelected(e.Node);
        }

        private void TeamCityBuildChooser_Load(object sender, EventArgs e)
        {
            LoadProjectsAndBuilds(_teamCityServerUrl);

            if (_previouslySelectedBuild != null)
            {
                treeViewTeamCityProjects.SelectedNode = _previouslySelectedBuild;
            }
        }
    }
}
