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
            List<GitRevision> revisions = GitCommands.GitCommands.GitRevisionGraph();

            if (revisions.Count > 0)
                LoadInTreeSingle(revisions[0], GitTree.Nodes);

            {
                Revisions.DataSource = revisions;
                Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);


                //revisions = GitCommands.GitCommands.GitRevisionGraph();

                int height = 22;
                int width = 8;
                int y = -height;

                graphImage = new Bitmap(500, (revisions.Count * height) + 50, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                Graphics graph = Graphics.FromImage(graphImage);
                graph.Clear(Color.White);


                string lastLine = "";
                string currentLine = "";
                //foreach (GitRevision revision in revisions)
                for (int r = 0 ; r < revisions.Count; r++)
                {
                    GitRevision revision = revisions[r];

                    GitRevision prevRevision = null;
                    GitRevision nextRevision = null;

                    if (r > 0)
                        prevRevision = revisions[r - 1];
                    if (revisions.Count > r+1)
                        nextRevision = revisions[r + 1];

                    y += height;
                    int nLine = 0;

                    char[] calc = new char[100];

                    for (int x = 0; x < 100; x++)
                    {
                        calc[x] = '|';
                    }

                    for (int n = 0; n < revision.GraphLines.Count; n++)
                    {
                        //if (revision.GraphLines[n] == currentLine)
                        //    continue;


                        string nextLine = "";

                        //if (n > 0)
                        //    lastLine = revision.GraphLines[n-1];

                        //currentLine = revision.GraphLines[n];

                        //if (n+1 < revision.GraphLines.Count)
                        //    nextLine = revision.GraphLines[n+1];

                        nextLine = revision.GraphLines[n];

                        nLine++;

                        int x = 0;
                        for (int nc = 0; nc < currentLine.Length; nc++)
                        {
                            //if (currentLine.LastIndexOfAny(new char[] { '*', '\\', '/' }) < 0)
                            //    break;
                            x += width;

                            char c = currentLine[nc];
                            int top = y;
                            int bottom = y + height;
                            int left = x;
                            int right = x + width;
                            int hcenter = x + (width / 2);
                            int vcenter = y + (height / 2);

                            if (c == '*')
                            {
                                graph.FillEllipse(new SolidBrush(Color.Red), hcenter - 3, vcenter - 3, 6, 6);

                                if (r == 0 && nextRevision.GraphLines[0].Length > nc && (nextRevision.GraphLines[0][nc] == '|' || nextRevision.GraphLines[0][nc] == '*'))
                                {
                                    graph.DrawLine(new Pen(Color.Red), hcenter, vcenter, hcenter, bottom);
                                }
                            }
                            if (c != '|' && c != '*')
                            {
                                calc[nc] = ' ';
                            }
                            if (c == '\\')
                            {
                                if ((nextLine.Length > nc && nextLine[nc] == '/' || nextLine.Length <= nc) ||
                                    (lastLine.Length > nc && lastLine[nc] == '/' || lastLine.Length <= nc))
                                {
                                    graph.DrawLine(new Pen(Color.Red), left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2));
                                }
                                else
                                {
                                    if ((nextLine.Length > nc + 2 && nextLine[nc + 2] != '\\') || nextLine.Length <= nc + 2)
                                    {
                                        //draw: 
                                        //      \
                                        graph.DrawLine(new Pen(Color.Red), right, bottom, right + (width / 2), bottom + (height / 2));
                                    }
                                    if (nc - 2 >= 0 && lastLine.Length > (nc - 2) && lastLine[nc - 2] == '\\')
                                    {
                                        //draw: _
                                        graph.DrawLine(new Pen(Color.Red), left, bottom, right, bottom);
                                    }
                                    else
                                    {
                                        // draw: \_
                                        graph.DrawLine(new Pen(Color.Red), left - (width / 2), vcenter, left, bottom);
                                        graph.DrawLine(new Pen(Color.Red), left, bottom, right, bottom);
                                    }
                                }
                            }
                            if (c == '/')
                            {
                                if ((nextLine.Length > nc && nextLine[nc] == '\\' || nextLine.Length <= nc) ||
                                    (lastLine.Length > nc && lastLine[nc] == '\\' || lastLine.Length <= nc))
                                {
                                    graph.DrawLine(new Pen(Color.Red), left - (width / 2), vcenter, left - (width / 2), bottom + (height / 2));
                                }
                                else
                                {



                                    if (lastLine.Length > nc + 2 && lastLine[nc + 2] != '/' || lastLine.Length <= nc + 2)
                                    {
                                        //draw: /
                                        //      
                                        graph.DrawLine(new Pen(Color.Red), right, bottom, right + (width / 2), bottom - (height / 2));
                                    }
                                    if (nc - 2 >= 0 && nextLine.Length > (nc - 2) && nextLine[nc - 2] == '/')
                                    {
                                        //draw: _
                                        //      
                                        graph.DrawLine(new Pen(Color.Red), left, bottom, right, bottom);
                                    }
                                    else
                                    {
                                        //draw:  _
                                        //      /
                                        graph.DrawLine(new Pen(Color.Red), left - (width / 2), bottom + (height / 2), left, bottom);
                                        graph.DrawLine(new Pen(Color.Red), left, bottom, right, bottom);
                                    }
                                }


                                //graph.DrawLine(new Pen(Color.Red), right + (with / 2), vcenter, left - (with / 2), bottom);
                            }

                            if (n == revision.GraphLines.Count - 1)
                            {
                                char prevChar = ' ';
                                char currentChar = calc[nc];//revision.GraphLines[n][nc];
                                char nextChar = ' ';

                                if (prevRevision != null && prevRevision.GraphLines[prevRevision.GraphLines.Count - 1].Length > nc)
                                    prevChar = prevRevision.GraphLines[prevRevision.GraphLines.Count - 1][nc];

                                if (nextRevision != null && nextRevision.GraphLines[0].Length > nc)
                                    nextChar = nextRevision.GraphLines[0][nc];

                                if ((prevChar == '|' && currentChar == '|') || (prevChar == '|' && currentChar == '*'))
                                {
                                    graph.DrawLine(new Pen(Color.Red), hcenter, top + (height / 2), hcenter, vcenter + (height / 2));
                                }
                                if ((nextChar == '|' && currentChar == '|') || (nextChar == '*' && currentChar == '|'))
                                {
                                    graph.DrawLine(new Pen(Color.Red), hcenter, vcenter + (height / 2), hcenter, bottom + (height / 2));
                                }

                            }
                        }
                        lastLine = currentLine;
                        currentLine = nextLine;



                    }
                }
            }
        }

        void Revisions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                //Bitmap cellImage = new Bitmapgraph, e.CellBounds.Height, graph);

                e.Graphics.DrawImage(graphImage, e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width, e.CellBounds.Y + e.CellBounds.Height);
                e.Graphics.DrawImage(graphImage, e.CellBounds, new Rectangle(0, e.RowIndex * 22, 200, 22), GraphicsUnit.Pixel);
                e.Handled = true;
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

        private void formatPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormFormatPath().ShowDialog();
        }


    }
}
