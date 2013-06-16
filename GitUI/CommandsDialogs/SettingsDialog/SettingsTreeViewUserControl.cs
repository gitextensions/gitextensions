﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public sealed partial class SettingsTreeViewUserControl : UserControl
    {
        private readonly Font _origTextBoxFont;
        private bool _isSelectionChangeTriggeredByGoto;
        private List<TreeNode> _nodesFoundByTextBox;
        private const string FindPrompt = "Type to find";
        private readonly Dictionary<SettingsPageReference, TreeNode> _Pages2NodeMap = new Dictionary<SettingsPageReference, TreeNode>();
        private readonly IList<ISettingsPage> _SettingsPages = new List<ISettingsPage>();

        public event EventHandler<SettingsPageSelectedEventArgs> SettingsPageSelected;
        public IEnumerable<ISettingsPage> SettingsPages { get { return _SettingsPages; } }

        public SettingsTreeViewUserControl()
        {
            InitializeComponent();

            Font = AppSettings.Font;

            _origTextBoxFont = textBoxFind.Font;
            SetFindPrompt(true);
        }

        /// <summary>
        /// </summary>
        /// <param name="page"></param>
        /// <param name="parentPageReference"></param>
        /// <param name="asRoot">only one page can be set as the root page (for the GitExt and Plugin root node)</param>
        public void AddSettingsPage(ISettingsPage page, SettingsPageReference parentPageReference, bool asRoot = false)
        {
            TreeNode node;
            if (parentPageReference == null)
            {
                // add one of the root nodes (e. g. "Git Extensions" or "Plugins"
                node = treeView1.Nodes.Add(page.GetTitle());
            }
            else
            {
                if (asRoot)
                {
                    // e. g. to set the Checklist on the "Git Extensions" node
                    node = _Pages2NodeMap[parentPageReference];
                }
                else
                {
                    TreeNode parentNode;
                    if (!_Pages2NodeMap.TryGetValue(parentPageReference, out parentNode))
                        throw new ArgumentException("You have to add parent page first: " + parentPageReference);

                    node = parentNode.Nodes.Add(page.GetTitle());
                }
            }

            node.Tag = page;
            _Pages2NodeMap.Add(page.PageReference, node);
            _SettingsPages.Add(page);
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
                ISettingsPage page = node.Tag as ISettingsPage;
                if (page.GuiControl == null)
                {
                    var firstSubNode = node.FirstNode;
                    if (firstSubNode != null)
                    {
                        treeView1.SelectedNode = firstSubNode;
                        return;
                    }
                }
                SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPage = page, IsTriggeredByGoto = _isSelectionChangeTriggeredByGoto });
            }
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

                foreach (var node in treeView1.AllNodes())
                {
                    var settingsPage = (ISettingsPage)node.Tag;

                    // search for title
                    if (settingsPage.GetTitle().ToLowerInvariant().Contains(searchFor))
                    {
                        _nodesFoundByTextBox.Add(node);
                    }

                    // search for keywords (space combines as 'and')
                    var andKeywords = searchFor.Split(' ');
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

                if (_nodesFoundByTextBox.Any())
                {
                    // if visible: when searching, if the selected node is valid, it will still have grey background
                    treeView1.HideSelection = true;
                }
            }
        }

        /// <summary>Highlights a <see cref="TreeNode"/> or returns it to the default colors.</summary>
        static void HighlightNode(TreeNode treeNode, bool highlight)
        {
            treeNode.ForeColor = highlight ? Color.White : Color.Black;
            treeNode.BackColor = highlight ? Color.SeaGreen : new Color();
        }

        private void ResetAllNodeHighlighting()
        {
            treeView1.BeginUpdate();
            ResetAllNodeHighlighting(treeView1.Nodes);
            treeView1.HideSelection = false;
            treeView1.EndUpdate();
        }

        private void ResetAllNodeHighlighting(TreeNodeCollection nodes)
        {
            labelNumFound.Text = "";

            foreach (TreeNode node in nodes.Cast<TreeNode>())
            {
                HighlightNode(node, false);
                ResetAllNodeHighlighting(node.Nodes);
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
            TreeNode node;
            if (settingsPageReference == null)
                node = treeView1.Nodes.Count > 0 ? treeView1.Nodes[0] : null;
            else
                _Pages2NodeMap.TryGetValue(settingsPageReference, out node);

            if (node != null)
            {
                _isSelectionChangeTriggeredByGoto = true;
                treeView1.SelectedNode = node;
                node.Expand();
                FireSettingsPageSelectedEvent(treeView1.SelectedNode);
                _isSelectionChangeTriggeredByGoto = false;
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
