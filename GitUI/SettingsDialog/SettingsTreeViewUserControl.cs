using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUI.SettingsDialog.Pages;

namespace GitUI.SettingsDialog
{
    public partial class SettingsTreeViewUserControl : UserControl
    {
        private TreeNode _geRootNode;
        private TreeNode _pluginsRootNode;
        private SettingsPageBase _blankSettingsPage = new BlankSettingsPage();
        private Font _origTextBoxFont;
        private Font _nodeFontBold;
        private Font _nodeFontItalic;
        ////private IList<SettingsPageBase> registeredSettingsPages = new List<SettingsPageBase>();
        private IList<TreeNode> _treeNodesWithSettingsPage = new List<TreeNode>();
        private SettingsPageBase _firstRegisteredSettingsPage;

        public event EventHandler<SettingsPageSelectedEventArgs> SettingsPageSelected;

        public SettingsTreeViewUserControl()
        {
            InitializeComponent();

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

        public void RegisterSettingsPage(SettingsPageBase settingsPage)
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

        public void RegisteringComplete()
        {
            _geRootNode.Nodes.Add("TODO: more");
            treeView1.ExpandAll();

            var firstRegistered = FindNodeBySettingsPage(_firstRegisteredSettingsPage);
            treeView1.SelectedNode = firstRegistered;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (SettingsPageSelected != null)
            {
                if (e.Node.Tag as SettingsPageBase != null)
                {
                    SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPageBase = (SettingsPageBase)(e.Node.Tag) });
                }
                else if (e.Node.Text == "Git Extensions")
                {
                    SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPageBase = null });
                }
                else
                {
                    SettingsPageSelected(this, new SettingsPageSelectedEventArgs { SettingsPageBase = _blankSettingsPage });
                }
            }
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

                foreach (var node in GetFindableNodes())
                {
                    if (searchFor.Contains(node.Text.ToLowerInvariant()))
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

        private IEnumerable<TreeNode> GetFindableNodes()
        {
            return _treeNodesWithSettingsPage;
        }

        private void HighlightNode(TreeNode treeNode, bool highlight)
        {
            treeNode.NodeFont = highlight ? _nodeFontBold : null;
        }

        private void ResetAllNodeHighlighting()
        {
            foreach (var node in GetFindableNodes())
            {
                HighlightNode(node, false);
            }
        }

        private TreeNode FindNodeBySettingsPage(SettingsPageBase settingsPage)
        {
            return GetFindableNodes().FirstOrDefault(te => te.Tag == settingsPage);
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
    }

    public class SettingsPageSelectedEventArgs : EventArgs
    {
        public SettingsPageBase SettingsPageBase { get; internal set; }
    }
}
