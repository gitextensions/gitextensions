using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public sealed partial class SettingsTreeViewUserControl : UserControl
    {
        private bool _isSelectionChangeTriggeredByGoto;
        private List<TreeNode> _nodesFoundByTextBox = new();
        private readonly Dictionary<SettingsPageReference, TreeNode> _pages2NodeMap = new();
        private readonly List<ISettingsPage> _settingsPages = new();

        public event EventHandler<SettingsPageSelectedEventArgs>? SettingsPageSelected;
        public IEnumerable<ISettingsPage> SettingsPages => _settingsPages;

        public SettingsTreeViewUserControl()
        {
            InitializeComponent();

            Font = AppSettings.Font;

            textBoxFind.PlaceholderText = TranslatedStrings.SettingsTypeToFind;

            treeView1.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = DpiUtil.Scale(new Size(16, 16))
            };

            // Scale ImageSize and images scale automatically
            treeView1.ImageList.Images.Add(Images.Blank);
            this.AdjustForDpiScaling();
        }

        /// <summary>Add page to settings tree.</summary>
        /// <param name="page">The settings page to add.</param>
        /// <param name="parentPageReference">An already added settings page to be a parent in the tree.</param>
        /// <param name="icon">An icon to display in tree node.</param>
        /// <param name="asRoot">only one page can be set as the root page (for the GitExt and Plugin root node).</param>
        public void AddSettingsPage(ISettingsPage page, SettingsPageReference? parentPageReference, Image? icon, bool asRoot = false)
        {
            TreeNode node;
            if (parentPageReference is null)
            {
                // add one of the root nodes (e. g. "Git Extensions" or "Plugins"
                node = AddPage(treeView1.Nodes, page, icon);
            }
            else
            {
                if (asRoot)
                {
                    // e. g. to set the Checklist on the "Git Extensions" node
                    node = _pages2NodeMap[parentPageReference];
                }
                else
                {
                    if (!_pages2NodeMap.TryGetValue(parentPageReference, out var parentNode))
                    {
                        throw new ArgumentException("You have to add parent page first: " + parentPageReference);
                    }

                    node = AddPage(parentNode.Nodes, page, icon);
                }
            }

            node.Tag = page;
            _pages2NodeMap.Add(page.PageReference, node);
            _settingsPages.Add(page);
        }

        private TreeNode AddPage(TreeNodeCollection treeNodeCollection, ISettingsPage page, Image? icon)
        {
            var node = treeNodeCollection.Add(page.GetTitle());
            if (icon is null)
            {
                return node;
            }

            treeView1.ImageList.Images.Add(icon);
            node.ImageIndex = node.SelectedImageIndex = treeView1.ImageList.Images.Count - 1;
            return node;
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
            if (SettingsPageSelected is not null)
            {
                var page = (ISettingsPage)node.Tag;

                if (page.GuiControl is null)
                {
                    var firstSubNode = node.FirstNode;
                    if (firstSubNode is not null)
                    {
                        treeView1.SelectedNode = firstSubNode;
                        return;
                    }
                }

                SettingsPageSelected?.Invoke(this, new SettingsPageSelectedEventArgs(page, _isSelectionChangeTriggeredByGoto));
            }
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {
            _nodesFoundByTextBox.Clear();

            if (string.IsNullOrWhiteSpace(textBoxFind.Text))
            {
                ResetAllNodeHighlighting();
                return;
            }

            string searchFor = textBoxFind.Text.ToLowerInvariant();
            var andKeywords = searchFor.LazySplit(' ');
            foreach (var node in treeView1.AllNodes())
            {
                var settingsPage = (ISettingsPage)node.Tag;

                // search for title
                if (settingsPage.GetTitle().Contains(searchFor, StringComparison.InvariantCultureIgnoreCase))
                {
                    _nodesFoundByTextBox.Add(node);
                    continue;
                }

                // search for keywords (space combines as 'and')
                if (andKeywords.All(keyword => settingsPage.GetSearchKeywords().Any(k => k.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))))
                {
                    // only part of a keyword must match to have a match
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
                node.EnsureVisible();
            }

            if (_nodesFoundByTextBox.Any())
            {
                // if visible: when searching, if the selected node is valid, it will still have grey background
                treeView1.HideSelection = true;
            }
        }

        /// <summary>Highlights a <see cref="TreeNode"/> or returns it to the default colors.</summary>
        private static void HighlightNode(TreeNode treeNode, bool highlight)
        {
            treeNode.ForeColor = highlight ? SystemColors.HighlightText : SystemColors.ControlText;
            treeNode.BackColor = highlight ? SystemColors.Highlight : new Color();
        }

        private void ResetAllNodeHighlighting()
        {
            treeView1.BeginUpdate();
            ResetHighlighting(treeView1.Nodes);
            treeView1.HideSelection = false;
            treeView1.EndUpdate();
            return;

            void ResetHighlighting(TreeNodeCollection nodes)
            {
                foreach (TreeNode node in nodes.Cast<TreeNode>())
                {
                    HighlightNode(node, false);
                    ResetHighlighting(node.Nodes);
                }
            }
        }

        public void GotoPage(SettingsPageReference? settingsPageReference)
        {
            TreeNode? node;
            if (settingsPageReference is null)
            {
                node = treeView1.Nodes.Count > 0 ? treeView1.Nodes[0] : null;
            }
            else
            {
                _pages2NodeMap.TryGetValue(settingsPageReference, out node);
            }

            if (node is not null)
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
                    if (firstFoundNode is not null)
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
        public ISettingsPage SettingsPage { get; }
        public bool IsTriggeredByGoto { get; }

        public SettingsPageSelectedEventArgs(ISettingsPage settingsPage, bool isTriggeredByGoto)
        {
            SettingsPage = settingsPage;
            IsTriggeredByGoto = isTriggeredByGoto;
        }
    }
}
