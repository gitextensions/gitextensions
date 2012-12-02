using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUI.SettingsDialog.Pages;
using GitCommands;

namespace GitUI.SettingsDialog
{
    public partial class SettingsTreeViewUserControl : UserControl
    {
        private TreeNode _geRootNode;
        private TreeNode _pluginsRootNode;
        private ISettingsPage _blankSettingsPage = new BlankSettingsPage();
        private Font _origTextBoxFont;
        private Font _nodeFontBold;
        private Font _nodeFontItalic;
        ////private IList<SettingsPageBase> registeredSettingsPages = new List<SettingsPageBase>();
        private IList<TreeNode> _treeNodesWithSettingsPage = new List<TreeNode>();
        private ISettingsPage _firstRegisteredSettingsPage;
        private bool _isSelectionChangeTriggeredByGoto;

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
            var pluginPlaceHolderNode = _pluginsRootNode.Nodes.Add("todo");
            pluginPlaceHolderNode.NodeFont = _nodeFontItalic;
            pluginPlaceHolderNode.ForeColor = Color.Gray;
        }

        public void SetSettingsPages(SettingsPageRegistry settingsPageRegistry)
        {
            foreach (var settingsPage in settingsPageRegistry.GetSettingsPages())
            {
                ////registeredSettingsPages.Add(settingsPage);
                var settingsPageNode = _geRootNode.Nodes.Add(settingsPage.Text);
                _treeNodesWithSettingsPage.Add(settingsPageNode);
                settingsPageNode.Tag = settingsPage;

                if (_firstRegisteredSettingsPage == null)
                {
                    _firstRegisteredSettingsPage = settingsPage;
                }
            }

            RegisteringComplete();
        }

        private void RegisteringComplete()
        {
            treeView1.ExpandAll();

            var firstRegistered = FindNodeBySettingsPage(_firstRegisteredSettingsPage);
            treeView1.SelectedNode = firstRegistered;
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
                else if (node.Text == "Git Extensions")
                {
                    SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPage = null });
                }
                else
                {
                    SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPage = _blankSettingsPage });
                }
            }

            _isSelectionChangeTriggeredByGoto = false;        
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {
            if (textBoxFind.Text.IsNullOrEmpty() || textBoxFind.Text == "Type to find")
            {
                ResetAllNodeHighlighting();
            }
            else
            {
                string searchFor = textBoxFind.Text.ToLowerInvariant();

                var foundNodes = new List<TreeNode>();

                foreach (var node in GetNodesWithSettingsPage())
                {
                    var settingsPage = (ISettingsPage)node.Tag;

                    // search for title
                    if (searchFor.Contains(settingsPage.Text.ToLowerInvariant()))
                    {
                        foundNodes.Add(node);
                    }

                    // search for keywords (space combines as 'and')
                    var andKeywords = searchFor.Split(' ');
                    if (andKeywords.All(keyword => settingsPage.GetSearchKeywords().Contains(keyword)))
                    {
                        foundNodes.Add(node);
                    }
                }

                ResetAllNodeHighlighting();

                foreach (var node in foundNodes)
                {
                    HighlightNode(node, true);
                }
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
            foreach (var node in GetNodesWithSettingsPage())
            {
                HighlightNode(node, false);
            }
        }

        private TreeNode FindNodeBySettingsPage(ISettingsPage settingsPage)
        {
            return GetNodesWithSettingsPage().FirstOrDefault(te => te.Tag == settingsPage);
        }

        #region FindPrompt
        private void SetFindPrompt(bool show)
        {
            if (show)
            {
                //textBoxFind.Font = new Font("Calibri", textBoxFind.Font.Size, FontStyle.Italic);
                textBoxFind.Font = new Font(textBoxFind.Font, FontStyle.Italic);
                textBoxFind.Text = "Type to find";
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

            if (textBoxFind.Text == "Type to find")
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
    }

    public class SettingsPageSelectedEventArgs : EventArgs
    {
        public bool IsTriggeredByGoto { get; internal set; }
        public ISettingsPage SettingsPage { get; internal set; }
    }
}
