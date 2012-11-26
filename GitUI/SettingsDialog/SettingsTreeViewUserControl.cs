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

        public event EventHandler<SettingsPageSelectedEventArgs> SettingsPageSelected;

        public SettingsTreeViewUserControl()
        {
            InitializeComponent();

            _origTextBoxFont = textBoxFind.Font;
            SetFindPrompt(true);

            AddRootNodes();
        }

        private void AddRootNodes()
        {
            _geRootNode = treeView1.Nodes.Add("Git Extensions");

            _pluginsRootNode = treeView1.Nodes.Add("Plugins");
            var pluginPlaceHolderNode = _pluginsRootNode.Nodes.Add("todo");
            pluginPlaceHolderNode.NodeFont = new Font(treeView1.Font, FontStyle.Italic);
            pluginPlaceHolderNode.ForeColor = Color.Gray;
        }

        public void RegisterSettingsPage(SettingsPageBase settingsPage)
        {
            var settingsPageNode = _geRootNode.Nodes.Add(settingsPage.Text);
            settingsPageNode.Tag = settingsPage;
        }

        public void RegisteringComplete()
        {
            _geRootNode.Nodes.Add("TODO: more");
            treeView1.ExpandAll();
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
                string searchFor = textBoxFind.Text;

                // TODO: search
                // TODO: HighlightNode
            }
        }

        private void HighlightNode(TreeNode treeNode)
        {
            // TODO
        }

        private void ResetAllNodeHighlighting()
        {
            // TODO
        }

        #region FindPrompt
        private void SetFindPrompt(bool show)
        {
            if (show)
            {
                textBoxFind.Font = new Font("Calibri", textBoxFind.Font.Size, FontStyle.Italic);
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
