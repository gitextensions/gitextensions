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

        public event EventHandler<SettingsPageSelectedEventArgs> SettingsPageSelected;

        public SettingsTreeViewUserControl()
        {
            InitializeComponent();

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
    }

    public class SettingsPageSelectedEventArgs : EventArgs
    {
        public SettingsPageBase SettingsPageBase { get; internal set; }
    }
}
