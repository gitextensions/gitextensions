using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.SettingsDialog
{
    public sealed partial class SettingsTreeViewUserControl : UserControl
    {
        private TreeNode _geRootNode;
        private TreeNode _pluginsRootNode;
        private readonly Font _origTextBoxFont;
        private Font _nodeFontBold;
        private Font _nodeFontItalic;
        private readonly IList<TreeNode> _treeNodesWithSettingsPage = new List<TreeNode>();
        private bool _isSelectionChangeTriggeredByGoto;
        private List<TreeNode> _nodesFoundByTextBox;
        private const string FindPrompt = "Type to find";

        public event EventHandler<SettingsPageSelectedEventArgs> SettingsPageSelected;

        public SettingsTreeViewUserControl()
        {
            InitializeComponent();

            Font = Settings.Font;

            _origTextBoxFont = textBoxFind.Font;
            SetFindPrompt(true);

            AddRootNodes();
        }

        /// <summary>
        /// ... and save fonts
        /// </summary>
        private void AddRootNodes()
        {
            _geRootNode = treeView1.Nodes.Add("Git Extensions");

            // create fonts
            var nodeFond = treeView1.Font;
            _nodeFontBold = new Font(nodeFond, FontStyle.Bold);
            _nodeFontItalic = new Font(nodeFond, FontStyle.Italic);

            _pluginsRootNode = treeView1.Nodes.Add("Plugins");
        }

        private void AddSettingsPages(IEnumerable<ISettingsPage> pages, TreeNode node)
        {
            node.Nodes.Clear();

            foreach (var settingsPage in pages)
            {
                var settingsPageNode = node.Nodes.Add(settingsPage.Text);
                _treeNodesWithSettingsPage.Add(settingsPageNode);
                settingsPageNode.Tag = settingsPage;
            }
        }

        public void SetSettingsPages(SettingsPageRegistry settingsPageRegistry)
        {
            AddSettingsPages(settingsPageRegistry.GetSettingsPages(), _geRootNode);
            
            AddSettingsPages(settingsPageRegistry.GetPluginSettingsPages(), _pluginsRootNode);

            RegisteringComplete();
        }

        private void RegisteringComplete()
        {
            _geRootNode.Expand();

            if (_geRootNode.Nodes.Count > 0)
            {
                treeView1.SelectedNode = _geRootNode.Nodes[0];
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!_isSelectionChangeTriggeredByGoto)
            {
                FireSettingsPageSelectedEvent(e.Node);
            }
        }

        private void FireSettingsPageSelectedEvent(TreeNode node)
        {
            if (SettingsPageSelected != null)
            {
                if (node.Tag as ISettingsPage != null)
                {
                    SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPage = (ISettingsPage)(node.Tag), IsTriggeredByGoto = _isSelectionChangeTriggeredByGoto });
                }
                else if (node == _geRootNode || node == _pluginsRootNode)
                {
                    var firstSubNode = node.FirstNode;
                    if (firstSubNode != null)
                    {
                        treeView1.SelectedNode = firstSubNode;
                    }
                }
                else
                {
                    SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPage = null });
                }
            }

            _isSelectionChangeTriggeredByGoto = false;
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {
            _nodesFoundByTextBox = new List<TreeNode>();

            if (textBoxFind.Text.IsNullOrEmpty() || textBoxFind.Text == FindPrompt)
            {
                ResetAllNodeHighlighting();
            }
            else
            {
                string searchFor = textBoxFind.Text.ToLowerInvariant();

                foreach (var node in GetNodesWithSettingsPage())
                {
                    var settingsPage = (ISettingsPage)node.Tag;

                    // search for title
                    if (settingsPage.Text.ToLowerInvariant().Contains(searchFor))
                    {
                        _nodesFoundByTextBox.Add(node);
                    }

                    // search for keywords (space combines as 'and')
                    var andKeywords = searchFor.Split(' ');
                    //// if (andKeywords.All(keyword => settingsPage.GetSearchKeywords().Contains(keyword))) // the whole keyword must match to have a match
                    if (andKeywords.All(keyword => settingsPage.GetSearchKeywords().Any(k => k.Contains(keyword)))) // only part of a keyword must match to have a match
                    {
                        if (!_nodesFoundByTextBox.Contains(node))
                        {
                            _nodesFoundByTextBox.Add(node);
                        }
                    }
                }

                ResetAllNodeHighlighting();

                foreach (var node in _nodesFoundByTextBox)
                {
                    HighlightNode(node, true);
                }

                labelNumFound.Text = _nodesFoundByTextBox.Count.ToString();
            }
        }

        private IEnumerable<TreeNode> GetNodesWithSettingsPage()
        {
            return _treeNodesWithSettingsPage;
        }

        private void HighlightNode(TreeNode treeNode, bool highlight)
        {
            treeNode.NodeFont = highlight ? _nodeFontBold : null;
        }

        private void ResetAllNodeHighlighting()
        {
            labelNumFound.Text = "";

            foreach (var node in GetNodesWithSettingsPage())
            {
                HighlightNode(node, false);
            }
        }

        ////private TreeNode FindNodeBySettingsPage(ISettingsPage settingsPage)
        ////{
        ////    return GetNodesWithSettingsPage().FirstOrDefault(te => te.Tag == settingsPage);
        ////}

        #region FindPrompt
        private void SetFindPrompt(bool show)
        {
            if (show)
            {
                //textBoxFind.Font = new Font("Calibri", textBoxFind.Font.Size, FontStyle.Italic);
                textBoxFind.Font = new Font(textBoxFind.Font, FontStyle.Italic);
                textBoxFind.Text = FindPrompt;
                textBoxFind.ForeColor = Color.Gray;
            }
            else
            {
                textBoxFind.Font = _origTextBoxFont;
                textBoxFind.ForeColor = Color.Black;
            }
        }

        private void textBoxFind_Enter(object sender, EventArgs e)
        {
            SetFindPrompt(false);

            if (textBoxFind.Text == FindPrompt)
            {
                textBoxFind.Text = "";
            }
        }

        private void textBoxFind_Leave(object sender, EventArgs e)
        {
            SetFindPrompt(true);
        }
        #endregion

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
            foreach (var node in GetNodesWithSettingsPage())
            {
                var settingsPage = (ISettingsPage)node.Tag;

                if (settingsPage.GetType() == settingsPageReference.SettingsPageType)
                {
                    _isSelectionChangeTriggeredByGoto = true;
                    treeView1.SelectedNode = node;
                    FireSettingsPageSelectedEvent(node);
                    return;
                }
            }
        }

        private void textBoxFind_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // TODO: how to avoid the windows sound when pressing ENTER?
                e.Handled = true;

                // each enter key press selects next highlighted node (cycle)
                int indexOfSelectedNode = _nodesFoundByTextBox.IndexOf(treeView1.SelectedNode);
                if (indexOfSelectedNode == -1 || indexOfSelectedNode + 1 == _nodesFoundByTextBox.Count)
                {
                    var firstFoundNode = _nodesFoundByTextBox.FirstOrDefault();
                    if (firstFoundNode != null)
                    {
                        treeView1.SelectedNode = firstFoundNode;
                    }
                }
                else
                {
                    treeView1.SelectedNode = _nodesFoundByTextBox[indexOfSelectedNode + 1];
                }
            }
        }
    }

    public class SettingsPageSelectedEventArgs : EventArgs
    {
        public bool IsTriggeredByGoto { get; internal set; }
        public ISettingsPage SettingsPage { get; internal set; }
    }
}
