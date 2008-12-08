using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Text.RegularExpressions;
using PatchApply;

namespace GitUI
{
    public partial class FormBrowse : Form
    {
        public FormBrowse()
        {
            InitializeComponent();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IGitItem item = (IGitItem)e.Node.Tag;
            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    EditorOptions.SetSyntax(FileText, ((GitItem)item).FileName);
                    FileText.Text = GitCommands.GitCommands.GetFileText(item.Guid);
                    FileText.Refresh();

                    FileChanges.DataSource = GitCommands.GitCommands.GetFileChanges(((GitItem)item).FileName);
                }



        }

        private void Browse_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public static Color GetLaneColor(Lane lane)
        {
            switch (lane.LaneNumber % 10)
            {
                case 0:
                    return Color.Red;
                case 1:
                    return Color.Blue;
                case 2:
                    return Color.Yellow;
                case 3:
                    return Color.Green;
                case 4:
                    return Color.Purple;
                case 5:
                    return Color.Silver;
                case 6:
                    return Color.Azure;
                case 7:
                    return Color.Aqua;
                case 8:
                    return Color.Brown;
                case 9:
                    return Color.Chocolate;
                default:
                    return Color.Black;
            }
        }

        //protected Graphics graph;
        protected Bitmap graphImage;

        protected void Initialize()
        {
            GitTree.Nodes.Clear();

            Branches.DisplayMember = "Name";
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            CurrentBranch.Text = "Current branch: " + selectedHead;
            List<GitHead> heads = GitCommands.GitCommands.GetHeads(false);
            Branches.DataSource = heads;
            foreach (GitHead head in heads)
            {
                if (head.Name == selectedHead)
                    Branches.SelectedItem = head;

            }
            //Branches.SelectedText = 

            ShowRevisions();

            Workingdir.Text = "Working dir: " + GitCommands.Settings.WorkingDir;
        }

        private void ShowRevisions()
        {
            List<GitRevision> revisions = GitCommands.GitCommands.GitRevisions(Branches.Text);

            if (revisions.Count > 0)
                LoadInTreeSingle(revisions[0], GitTree.Nodes);

            {
                Revisions.DataSource = revisions;
                Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);

                /*
                LaneGraph laneGraph = LaneGraphManager.CreateLaneGraph(revisions);


                int grid = 22;

                graphImage = new Bitmap((laneGraph.Lanes.Count * grid) + grid, (laneGraph.Points.Count * grid) + grid);
                Graphics graph = Graphics.FromImage(graphImage);
                graph.Clear(Color.White);

                for (int n = 0; n < laneGraph.Points.Count; n++ )
                {
                    LanePoint lanePoint = laneGraph.Points[n];
                    int top = n * grid;
                    int bottom = (n * grid) + grid;
                    int vcenter = bottom - (grid / 2);

                    foreach (Lane lane in laneGraph.GetLanesForPointnumber(lanePoint.PointNumber))
                    {
                        int with = 6;

                        int left = laneGraph.GetOptimalLaneNumber(lane) * with;
                        int right = left + with;
                        int hcenter = right - (with / 2);

                        bool drawPoint = false;

                        if (lane.Points.Contains(lanePoint))
                            drawPoint = true;

                        if (lanePoint.BranchFrom == null || drawPoint == false)
                        {
                            graph.DrawLine(new Pen(GetLaneColor(lane)), hcenter, top, hcenter, bottom);
                        }
                        else
                        {
                            graph.DrawLine(new Pen(GetLaneColor(lane)), hcenter, vcenter, hcenter, bottom);
                            //graph.DrawLine(new Pen(GetLaneColor(lane)), (laneGraph.GetOptimalLaneNumber(lanePoint.BranchFrom) * with) + (with / 2), vcenter, hcenter, vcenter);
                        }

                        if (drawPoint)
                            graph.FillEllipse(new SolidBrush(GetLaneColor(lane)), hcenter - 3, vcenter - 3, 6, 6);


                    }
                }


                graphImage.Save(@"c:\temp\graph.bmp");
                //graphImage.RotateFlip(RotateFlipType.Rotate180FlipY);
                */
            }
        }

        void Revisions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                //Bitmap cellImage = new Bitmap(graph., e.CellBounds.Height, graph);


                //e.Graphics.DrawImage(graphImage, e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width, e.CellBounds.Y + e.CellBounds.Height);
                //e.Graphics.DrawImage(graphImage, e.CellBounds, new Rectangle(0, e.RowIndex * 22, 100, 22), GraphicsUnit.Pixel);
                //e.Handled = true;
            }
            else
            {
                //e.Handled = true;
            }
        }

        protected void LoadInTreeSingle(IGitItem item, TreeNodeCollection node)
        {
            List<IGitItem> list = new List<IGitItem>();
            list.Add(item);
            LoadInTree(list, node);
            if (node.Count > 0)
                node[0].Expand();
        }

        protected void LoadInTree(List<IGitItem> items, TreeNodeCollection node)
        {
            foreach (IGitItem item in items)
            {
                TreeNode subNode = node.Add(item.Name);
                subNode.Tag = item;

                if (item is GitItem)
                {
                    if (((GitItem)item).ItemType == "tree")
                        subNode.Nodes.Add(new TreeNode());
                }
                else
                {
                    subNode.Nodes.Add(new TreeNode());
                }
                //LoadInTree(item.SubItems, subNode.Nodes);
            }
        }

        private void GitTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            IGitItem item = (IGitItem)e.Node.Tag;

            e.Node.Nodes.Clear();
            //item.SubItems = GitCommands.GitCommands.GetTree(item.Guid);
            LoadInTree(item.SubItems, e.Node.Nodes);

        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Branches.SelectedItem is GitHead)
            {
                //GitHead head = (GitHead)Branches.SelectedItem;

                //List<GitItem> items = GitCommands.GitCommands.GetTree(head.Guid);
                //GitTree.Nodes.Clear();

                //LoadInTreeSingle(head, GitTree.Nodes);
                ShowRevisions();
            }
        }

        private void Revisions_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DiffFiles.DataSource = null;
                if (Revisions.SelectedRows.Count == 0) return;

                DiffFiles.DisplayMember = "FileNameB";

                if (Revisions.SelectedRows[0].DataBoundItem is GitRevision)
                {
                    IGitItem revision = (IGitItem)Revisions.SelectedRows[0].DataBoundItem;

                    //List<GitItem> items = GitCommands.GitCommands.GetTree(revision.TreeGuid);
                    GitTree.Nodes.Clear();
                    LoadInTreeSingle(revision, GitTree.Nodes);

                    if (Revisions.SelectedRows.Count == 1)
                        DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(((GitRevision)Revisions.SelectedRows[0].DataBoundItem).Guid, ((GitRevision)Revisions.SelectedRows[0].DataBoundItem).ParentGuids[0]);
                    //DiffFiles.DataSource = GitCommands.GitCommands.GetDiff(((GitRevision)Revisions.SelectedRows[0].DataBoundItem).Guid, ((GitRevision)Revisions.SelectedRows[0].DataBoundItem).parentGuid);
                }

                if (Revisions.SelectedRows.Count == 2)
                {
                    if (Revisions.SelectedRows[0].DataBoundItem is GitRevision &&
                        Revisions.SelectedRows[1].DataBoundItem is GitRevision)
                    {

                        DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(((GitRevision)Revisions.SelectedRows[0].DataBoundItem).Guid, ((GitRevision)Revisions.SelectedRows[1].DataBoundItem).Guid);
                        //DiffFiles.DataSource = GitCommands.GitCommands.GetDiff(((GitRevision)Revisions.SelectedRows[0].DataBoundItem).Guid, ((GitRevision)Revisions.SelectedRows[1].DataBoundItem).Guid);

                    }
                }

            }
            catch
            { 
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                Settings.WorkingDir = browseDialog.SelectedPath;

                Initialize();
            }
        }

        private void checkoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCheckout form = new FormCheckout();
            form.ShowDialog();
            Initialize();
        }

        private void FileText_TextChanged(object sender, EventArgs e)
        {
        }

        private void FileChanges_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FileChanges_SelectionChanged(object sender, EventArgs e)
        {
            if (FileChanges.SelectedRows.Count == 0) return;

            if (FileChanges.SelectedRows[0].DataBoundItem is IGitItem)
            {
                IGitItem revision = (IGitItem)FileChanges.SelectedRows[0].DataBoundItem;

                FileText.Text = GitCommands.GitCommands.GetFileText(revision.Guid);
                FileText.Refresh();
            }
        }

        private void GitTree_DoubleClick(object sender, EventArgs e)
        {
            if (GitTree.SelectedNode == null || !(GitTree.SelectedNode.Tag is IGitItem)) return;

            IGitItem item = (IGitItem)GitTree.SelectedNode.Tag;
            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    FormFileHistory form = new FormFileHistory(((GitItem)item).FileName);
                    form.ShowDialog();

                }

        }

        private void viewDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormDiff diff = new FormDiff();
            diff.ShowDialog();
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAddFiles addFiles = new FormAddFiles();
            addFiles.ShowDialog();
        }

        private void branchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBranch form = new FormBranch();
            form.ShowDialog();
            Initialize();
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormClone form = new FormClone();
            form.ShowDialog();
            Initialize();
        }

        private void commitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCommit form = new FormCommit();
            form.ShowDialog();
            Initialize();
        }

        private void initNewRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();

            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                Settings.WorkingDir = browseDialog.SelectedPath;

                GitCommands.Init cmd = new GitCommands.Init(new GitCommands.InitDto());
                cmd.Execute();
                MessageBox.Show(cmd.Dto.Result);

                Initialize();
            }
        }

        private void pushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GitCommands.Push cmd = new GitCommands.Push(new GitCommands.PushDto());
            //cmd.Execute();
            //MessageBox.Show(cmd.Dto.Result);
            //Initialize();
            new FormPush().ShowDialog();
            Initialize();
        }

        private void pullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GitCommands.Pull cmd = new GitCommands.Pull(new GitCommands.PullDto());
            //cmd.Execute();
            //MessageBox.Show(cmd.Dto.Result);
            //Initialize();

            new FormPull().ShowDialog();
            Initialize();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Initialize();
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is Patch)
            {
                {
                    Patch patch = (Patch)DiffFiles.SelectedItem;
                    DiffText.Text = patch.Text;
                    DiffText.Refresh();
                    EditorOptions.SetSyntax(DiffText, patch.FileNameB);
                }

                //string changedFile = (string)DiffFiles.SelectedItem;


                //DiffText.Text = changedFile.PatchText;
            }
            else
            if (DiffFiles.SelectedItem is string)
            {
                Patch selectedPatch = GitCommands.GitCommands.GetSingleDiff(((GitRevision)Revisions.SelectedRows[0].DataBoundItem).Guid, ((GitRevision)Revisions.SelectedRows[0].DataBoundItem).ParentGuids[0], (string)DiffFiles.SelectedItem);
                if (selectedPatch != null)
                {
                    EditorOptions.SetSyntax(DiffText, selectedPatch.FileNameB);
                    DiffText.Text = selectedPatch.Text;
                }
                else
                {
                    DiffText.Text = "";
                }
                DiffText.Refresh();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.ShowDialog();
        }

        private void patchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewPatch applyPatch = new ViewPatch();
            applyPatch.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettigns form = new FormSettigns();
            form.ShowDialog();
        }

        private void applyPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MergePatch form = new MergePatch();
            form.ShowDialog();
        }

        private void gitBashToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunBash();

        }

        private void gitGUIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunGui();
        }


    }
}
