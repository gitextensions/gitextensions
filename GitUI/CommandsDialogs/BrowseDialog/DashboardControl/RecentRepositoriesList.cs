using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class RecentRepositoriesList : GitExtensionsControl
    {
        private readonly TranslationString directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");

        private static readonly Color DeafultFavouriteColor = Color.DarkGoldenrod;
        private static readonly Color DeafultBranchNameColor = SystemColors.HotTrack;
        private Color _favouriteColor = DeafultFavouriteColor;
        private Color _branchNameColor = DeafultBranchNameColor;
        private Color _hoverColor;
        private Color _headerColor;
        private Color _headerBackColor;
        private Color _mainBackColor;

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;


        public RecentRepositoriesList()
        {
            InitializeComponent();
            Translate();
            flpnlBody.Controls.Clear();

            ApplyScaling();
        }


        [Category("Appearance")]
        [DefaultValue(typeof(SystemColors), "HotTrack")]
        public Color BranchNameColor
        {
            get { return _branchNameColor; }
            set
            {
                if (_branchNameColor == value)
                {
                    return;
                }
                _branchNameColor = value;
                ApplyColors();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DarkGoldenrod")]
        public Color FavouriteColor
        {
            get { return _favouriteColor; }
            set
            {
                if (_favouriteColor == value)
                {
                    return;
                }
                _favouriteColor = value;
                ApplyColors();
            }
        }

        [Category("Appearance")]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                if (base.ForeColor == value)
                {
                    return;
                }
                base.ForeColor = value;
                ApplyColors();
            }
        }

        [Category("Appearance")]
        public Color HeaderColor
        {
            get { return _headerColor; }
            set
            {
                if (_headerColor == value)
                {
                    return;
                }
                _headerColor = value;
                lblRecentRepositories.ForeColor = value;
                lblRecentRepositories.Invalidate();
            }
        }

        [Category("Appearance")]
        public Color HeaderBackColor
        {
            get { return _headerBackColor; }
            set
            {
                if (_headerBackColor == value)
                {
                    return;
                }
                _headerBackColor = value;
                pnlHeader.BackColor = value;
                pnlHeader.Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(50)]
        public int HeaderHeight
        {
            get { return pnlHeader.Height; }
            set
            {
                pnlHeader.Height = value;
                pnlHeader.Invalidate();
            }
        }

        [Category("Appearance")]
        public Color HoverColor
        {
            get { return _hoverColor; }
            set
            {
                if (_hoverColor == value)
                {
                    return;
                }
                _hoverColor = value;
                ApplyColors();
            }
        }

        [Category("Appearance")]
        public Color MainBackColor
        {
            get { return _mainBackColor; }
            set
            {
                if (_mainBackColor == value)
                {
                    return;
                }
                _mainBackColor = value;
                flpnlBody.BackColor = value;
                flpnlBody.Invalidate();
            }
        }


        public void ShowRecentRepositories(IEnumerable<Repository> repositories)
        {
            try
            {
                flpnlBody.SuspendLayout();
                flpnlBody.Controls.Clear();

                if (repositories == null || !Visible)
                {
                    return;
                }

                bool first = true;
                foreach (var repository in repositories)
                {
                    var dashboardItem = new RecentRepositoryItem(repository)
                    {
                        AutoSize = false,
                        Anchor = first ? AnchorStyles.Top | AnchorStyles.Left : AnchorStyles.Left | AnchorStyles.Right,
                        ContextMenuStrip = contextMenuStrip,
                        MinimumSize = new Size(flpnlBody.MinimumSize.Width - flpnlBody.Padding.Left - flpnlBody.Padding.Right, 0),
                        Width = flpnlBody.Width - flpnlBody.Padding.Left - flpnlBody.Padding.Right - SystemInformation.VerticalScrollBarWidth,
                    };
                    dashboardItem.RepositorySelected += dashboardItem_RepositorySelected;
                    dashboardItem.RepositoryFavouriteChanged += dashboardItem_RepositoryFavouriteChanged;

                    flpnlBody.Controls.Add(dashboardItem);
                    first = false;
                }

                // add an empty control to force a padding at the botton of the flowlayout panel
                flpnlBody.Controls.Add(new Label { Height = flpnlBody.Padding.Bottom });

                ApplyColors();
                RefreshCategories(false);
            }
            finally
            {
                ApplyColors();
                flpnlBody.ResumeLayout(true);
            }
        }


        protected virtual void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            var handler = GitModuleChanged;
            handler?.Invoke(this, e);
        }


        private void ApplyCategoryFilter(string category)
        {
            foreach (var item in flpnlBody.Controls.OfType<RecentRepositoryItem>())
            {
                item.Visible = string.IsNullOrWhiteSpace(category) || item.Category == category;
            }
        }

        private void ApplyColors()
        {
            foreach (var item in flpnlBody.Controls.OfType<RecentRepositoryItem>())
            {
                item.BackColor = BackColor;
                item.BranchNameColor = BranchNameColor;
                item.FavouriteColor = FavouriteColor;
                item.ForeColor = ForeColor;
                item.HoverColor = HoverColor;
            }

            cboRepoCategories.BackColor = HeaderBackColor;
            cboRepoCategories.ForeColor = ForeColor;
        }

        private void ApplyScaling()
        {
            var scaleFactor = GetScaleFactor();
            tableLayoutPanel1.ColumnStyles[1].Width *= scaleFactor;
            ApplyPaddingScaling(pnlHeader, scaleFactor);
        }

        private void DashboardItemContextAction(ToolStripItem menuItem, Action<RecentRepositoryItem> action)
        {
            var dashboardItem = GetDashboardItem(menuItem);
            if (dashboardItem != null)
            {
                action(dashboardItem);
            }
        }

        private static RecentRepositoryItem GetDashboardItem(ToolStripItem menuItem)
        {
            // Retrieve the ContextMenuStrip that owns this ToolStripItem
            var contextMenu = menuItem?.Owner as ContextMenuStrip;

            // Get the control that is displaying this context menu
            var dashboardItem = contextMenu?.Tag as RecentRepositoryItem;
            if (string.IsNullOrWhiteSpace(dashboardItem?.Path))
            {
                return null;
            }
            return dashboardItem;
        }

        private void RefreshCategories(bool preserveSelection = true)
        {
            var categories = Repositories.RepositoryCategories.OrderBy(x => x.Description).ToArray();

            var selectedItem = cboRepoCategories.SelectedItem as RepositoryCategory;

            cboRepoCategories.Visible = categories.Length > 0;
            cboRepoCategories.SelectedIndexChanged -= cboRepoCategories_SelectedIndexChanged;
            cboRepoCategories.Items.Clear();
            cboRepoCategories.Items.Add("All");
            cboRepoCategories.Items.AddRange(categories);
            if (preserveSelection && selectedItem != null)
            {
                cboRepoCategories.SelectedItem = cboRepoCategories.Items.OfType<RepositoryCategory>()
                                                                        .FirstOrDefault(x => x.Description == selectedItem.Description);
            }
            if (cboRepoCategories.SelectedItem == null)
            {
                cboRepoCategories.SelectedIndex = 0;
            }
            ApplyCategoryFilter((cboRepoCategories.SelectedItem as RepositoryCategory)?.Description);
            cboRepoCategories.SelectedIndexChanged += cboRepoCategories_SelectedIndexChanged;

            tsmiCategories.DropDownItems.Clear();
            if (categories.Length > 0)
            {
                tsmiCategories.DropDownItems.Add(tsmiCategoryNone);
                tsmiCategories.DropDownItems.AddRange(categories.Select(c =>
                {
                    var item = new ToolStripMenuItem(c.Description);
                    item.Tag = c;
                    item.Click += tsmiCategory_Click;
                    return item;
                }).ToArray());
                tsmiCategories.DropDownItems.Add(new ToolStripSeparator());
            }
            tsmiCategories.DropDownItems.Add(tsmiCategoryAdd);
        }


        private void btnConfigureRecent_Click(object sender, EventArgs e)
        {
            using (var frm = new FormRecentReposSettings())
            {
                var result = frm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    Refresh();
                }
            }
        }

        private void btnConfigureRecent_MouseEnter(object sender, EventArgs e)
        {
            btnConfigureRecent.Image = Resources.Settings;
        }

        private void btnConfigureRecent_MouseLeave(object sender, EventArgs e)
        {
            btnConfigureRecent.Image = Resources.SettingsBw;
        }

        private void cboRepoCategories_DropDown(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            int width = comboBox.DropDownWidth;
            using (var g = comboBox.CreateGraphics())
            {
                int vertScrollBarWidth = comboBox.Items.Count > comboBox.MaxDropDownItems ? SystemInformation.VerticalScrollBarWidth : 0;
                foreach (var item in comboBox.Items.OfType<RepositoryCategory>())
                {
                    var newWidth = (int)g.MeasureString(item.Description, comboBox.Font).Width + vertScrollBarWidth;
                    if (width < newWidth)
                    {
                        width = newWidth;
                    }
                }
            }
            comboBox.DropDownWidth = width;
        }

        private void cboRepoCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            var item = combo?.SelectedItem as RepositoryCategory;
            var category = item?.Description;
            ApplyCategoryFilter(category);
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var dashboardItem = (sender as ContextMenuStrip)?.SourceControl as RecentRepositoryItem;
            if (string.IsNullOrWhiteSpace(dashboardItem?.Path))
            {
                return;
            }
            tsmiAddToMostRecent.Visible = dashboardItem.Repository.Anchor != Repository.RepositoryAnchor.MostRecent;
            tsmiRemoveFromMostRecent.Visible = !tsmiAddToMostRecent.Visible;

            // address a bug in context menu implementation
            // nested toolstrip items can't get source control
            // http://stackoverflow.com/questions/30534417/
            contextMenuStrip.Tag = dashboardItem;
        }

        private void dashboardItem_RepositoryFavouriteChanged(object sender, RepositoryEventArgs e)
        {
            e.Repository.Anchor = e.Repository.Anchor == Repository.RepositoryAnchor.MostRecent ?
                                                Repository.RepositoryAnchor.LessRecent :
                                                Repository.RepositoryAnchor.MostRecent;
            AppSettings.SaveSettings();
        }

        private void dashboardItem_RepositorySelected(object sender, RepositoryEventArgs e)
        {
            var module = new GitModule(e.Repository.Path);
            if (!module.IsValidGitWorkingDir())
            {
                var dialogResult = MessageBox.Show(this, directoryIsNotAValidRepository.Text,
                                                   directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNoCancel,
                                                   MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
                if (dialogResult == DialogResult.Yes)
                {
                    Repositories.RepositoryHistory.RemoveRecentRepository(e.Repository.Path);

                    var recentRepository = flpnlBody.Controls.OfType<RecentRepositoryItem>().FirstOrDefault(x => x.Repository == e.Repository);
                    if (recentRepository != null)
                    {
                        flpnlBody.Controls.Remove(recentRepository);
                    }
                }
            }
            else
            {
                Repositories.AddMostRecentRepository(module.WorkingDir);
                OnModuleChanged(this, new GitModuleEventArgs(module));
            }
        }

        private void tsmiAddToMostRecent_Click(object sender, EventArgs e)
        {
            DashboardItemContextAction(sender as ToolStripMenuItem, dashboardItem => dashboardItem.Favourite = true);
        }

        private void tsmiCategories_DropDownOpening(object sender, EventArgs e)
        {
            if (sender != tsmiCategories)
            {
                return;
            }
            DashboardItemContextAction(tsmiCategories, dashboardItem =>
            {
                foreach (ToolStripItem item in tsmiCategories.DropDownItems)
                {
                    item.Enabled = item.Text != dashboardItem.Category;
                }
                if (string.IsNullOrWhiteSpace(dashboardItem.Category) && tsmiCategories.DropDownItems.Count > 1)
                {
                    tsmiCategories.DropDownItems[0].Enabled = false;
                }
            });
        }

        private void tsmiCategory_Click(object sender, EventArgs e)
        {
            var dashboardItem = GetDashboardItem((sender as ToolStripMenuItem)?.OwnerItem);
            if (dashboardItem == null)
            {
                return;
            }

            var originalCategory = dashboardItem.Repository.Category;

            var category = (sender as ToolStripMenuItem)?.Tag as RepositoryCategory;
            dashboardItem.Category = category?.Description;

            // TODO: this code can be made be obsolete if we don't use RepositoryCategory objects
            // Remove category if it is unused
            if (!string.IsNullOrWhiteSpace(originalCategory) &&
                flpnlBody.Controls.OfType<RecentRepositoryItem>().All(x => x.Category != originalCategory))
            {
                Repositories.RemoveCategory(originalCategory);
            }
            AppSettings.SaveSettings();
            RefreshCategories();
        }

        private void tsmiCategoryAdd_Click(object sender, EventArgs e)
        {
            DashboardItemContextAction((sender as ToolStripMenuItem)?.OwnerItem, dashboardItem =>
            {
                using (var dialog = new FormDashboardCategoryTitle(Repositories.RepositoryCategories))
                {
                    if (DialogResult.OK == dialog.ShowDialog(this))
                    {
                        var category = dialog.Category;

                        dashboardItem.Category = category;
                        Repositories.AddCategory(category);
                        AppSettings.SaveSettings();

                        RefreshCategories();
                    }
                }
            });
        }

        private void tsmiOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                DashboardItemContextAction(sender as ToolStripMenuItem, dashboardItem => Process.Start(dashboardItem.Path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void tsmiRemoveFromList_Click(object sender, EventArgs e)
        {
            DashboardItemContextAction(sender as ToolStripMenuItem, dashboardItem =>
            {
                Repositories.RepositoryHistory.Repositories.Remove(dashboardItem.Repository);
                flpnlBody.Controls.Remove(dashboardItem);
            });
        }

        private void tsmiRemoveFromMostRecent_Click(object sender, EventArgs e)
        {
            DashboardItemContextAction(sender as ToolStripMenuItem, dashboardItem => dashboardItem.Favourite = false);
        }

        private void flpnlBody_Layout(object sender, LayoutEventArgs e)
        {
            var items = flpnlBody.Controls.OfType<RecentRepositoryItem>().ToList();
            if (!items.Any())
            {
                return;
            }
            items[0].Width = flpnlBody.Width - flpnlBody.Padding.Left - flpnlBody.Padding.Right - SystemInformation.VerticalScrollBarWidth;
        }
    }
}
