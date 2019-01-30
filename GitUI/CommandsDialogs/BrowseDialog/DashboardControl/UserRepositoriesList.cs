using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using GitUI.Properties;
using GitUI.UserControls;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class UserRepositoriesList : GitExtensionsControl
    {
        private readonly TranslationString _groupRecentRepositories = new TranslationString("Recent repositories");
        private readonly TranslationString _directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString _directoryIsNotAValidRepository = new TranslationString(
            "The selected item is not a valid git repository.\n\nDo you want to remove it from the recent repositories list?");

        private readonly TranslationString _deleteCategoryCaption = new TranslationString(
            "Delete Category");
        private readonly TranslationString _deleteCategoryQuestion = new TranslationString(
            "Do you want to delete category \"{0}\" with {1} repositories?\n\nThe action cannot be undone.");

        private readonly TranslationString _clearRecentCategoryCaption = new TranslationString(
            "Clear recent repositories");
        private readonly TranslationString _clearRecentCategoryQuestion = new TranslationString(
            "Do you want to clear the list of recent repositories?\n\nThe action cannot be undone.");

        private readonly Font _secondaryFont;
        private static readonly Color DefaultFavouriteColor = Color.DarkGoldenrod;
        private static readonly Color DefaultBranchNameColor = SystemColors.HotTrack;
        private Color _favouriteColor = DefaultFavouriteColor;
        private Color _branchNameColor = DefaultBranchNameColor;
        private Color _hoverColor;
        private Color _headerColor;
        private Color _headerBackColor;
        private Color _mainBackColor;
        private Brush _foreColorBrush;
        private Brush _branchNameColorBrush = new SolidBrush(DefaultBranchNameColor);
        private Brush _favouriteColorBrush = new SolidBrush(DefaultFavouriteColor);
        private Brush _hoverColorBrush = new SolidBrush(SystemColors.InactiveCaption);
        private ListViewItem _hoveredItem;
        private readonly ListViewGroup _lvgRecentRepositories;
        private readonly IUserRepositoriesListController _controller = new UserRepositoriesListController(RepositoryHistoryManager.Locals);
        private bool _hasInvalidRepos;

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;

        public UserRepositoriesList()
        {
            InitializeComponent();
            InitializeComplete();

            mnuTop.DropDownItems.Clear();

            _lvgRecentRepositories = new ListViewGroup(_groupRecentRepositories.Text, HorizontalAlignment.Left)
            {
                Name = string.Empty
            };

            _foreColorBrush = new SolidBrush(base.ForeColor);

            _secondaryFont = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints - 1f);
            lblRecentRepositories.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f);

            listView1.Items.Clear();
            listView1.Groups.Clear();
            listView1.AllowCollapseGroups = true;

            imageList1.Images.Clear();
            imageList1.ImageSize = DpiUtil.Scale(imageList1.ImageSize);
            imageList1.Images.Add(Images.DashboardFolderGit);
            imageList1.Images.Add(Images.DashboardFolderError);
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
                _branchNameColorBrush?.Dispose();
                _branchNameColorBrush = new SolidBrush(value);
                Invalidate();
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
                _favouriteColorBrush?.Dispose();
                _favouriteColorBrush = new SolidBrush(value);
                Invalidate();
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
                listView1.ForeColor = value;
                _foreColorBrush?.Dispose();
                _foreColorBrush = new SolidBrush(value);
                Invalidate();
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
                _hoverColorBrush?.Dispose();
                _hoverColorBrush = new SolidBrush(value);
                Invalidate();
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
                BackColor = value;
                listView1.BackColor = value;
            }
        }

        private ListViewItem HoveredItem
        {
            get => _hoveredItem;
            set
            {
                if (value == _hoveredItem)
                {
                    return;
                }

                if (_hoveredItem != null)
                {
                    // The previously hovered item may be already removed from the view, example:
                    // user locates mouse pointer over an item and triggers data refresh by pressing F5
                    if (listView1.Items.Contains(_hoveredItem))
                    {
                        listView1.Invalidate(listView1.GetItemRect(_hoveredItem.Index));
                    }
                }

                if (value != null)
                {
                    listView1.Invalidate(listView1.GetItemRect(value.Index));
                }

                _hoveredItem = value;
            }
        }

        private static StringComparer GroupHeaderComparer =>
            StringComparer.CurrentCulture;

        public void ShowRecentRepositories()
        {
            IReadOnlyList<RecentRepoInfo> recentRepositories;
            IReadOnlyList<RecentRepoInfo> favouriteRepositories;
            using (var graphics = CreateGraphics())
            {
                (recentRepositories, favouriteRepositories) = _controller.PreRenderRepositories(graphics);
            }

            try
            {
                listView1.BeginUpdate();
                listView1.Items.Clear();
                if (recentRepositories.Count > 0 || favouriteRepositories.Count > 0)
                {
                    listView1.TileSize = GetTileSize(recentRepositories, favouriteRepositories);
                }

                _hasInvalidRepos = false;

                var groups = Enumerable.Repeat(_lvgRecentRepositories, 1)
                    .Concat(recentRepositories.Concat(favouriteRepositories)
                        .Select(repo => repo.Repo.Category)
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct(GroupHeaderComparer)
                        .OrderBy(c => c)
                        .Select(c => new ListViewGroup(c, c)))
                    .AsReadOnlyList();

                listView1.SetGroups(groups, GroupHeaderComparer);
                BindRepositories(recentRepositories, isFavourite: false);
                BindRepositories(favouriteRepositories, isFavourite: true);
            }
            finally
            {
                listView1.EndUpdate();
            }

            void BindRepositories(IReadOnlyList<RecentRepoInfo> repos, bool isFavourite)
            {
                foreach (var repository in repos)
                {
                    var isInvalidRepo = !_controller.IsValidGitWorkingDir(repository.Repo.Path);
                    _hasInvalidRepos |= isInvalidRepo;

                    listView1.Items.Add(new ListViewItem(repository.Caption)
                    {
                        ForeColor = ForeColor,
                        Font = AppSettings.Font,
                        Group = isFavourite ? GetTileGroup(repository.Repo) : _lvgRecentRepositories,
                        ImageIndex = isInvalidRepo ? 1 : 0,
                        UseItemStyleForSubItems = false,
                        Tag = repository.Repo,
                        ToolTipText = repository.Repo.Path,
                        SubItems =
                        {
                            { _controller.GetCurrentBranchName(repository.Repo.Path), BranchNameColor, BackColor, _secondaryFont },
                            //// NB: we can add a 3rd row as well: { repository.Repo.Category, SystemColors.GrayText, BackColor, _secondaryFont }
                        }
                    });
                }
            }
        }

        protected virtual void OnModuleChanged(GitModuleEventArgs args)
        {
            var handler = GitModuleChanged;
            handler?.Invoke(this, args);
        }

        private List<string> GetCategories()
        {
            return GetRepositories()
                .Select(repository => repository.Category)
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .OrderBy(x => x)
                .Distinct()
                .ToList();
        }

        private IEnumerable<Repository> GetRepositories()
        {
            return listView1.Items.Cast<ListViewItem>()
                .Select(lvi => (Repository)lvi.Tag)
                .Where(_ => _ != null);
        }

        [CanBeNull]
        private static Repository GetSelectedRepository(ToolStripItem menuItem)
        {
            // Retrieve the ContextMenuStrip that owns this ToolStripItem
            var contextMenu = menuItem?.Owner as ContextMenuStrip;

            // Get the control that is displaying this context menu
            var selected = contextMenu?.Tag as Repository;
            if (string.IsNullOrWhiteSpace(selected?.Path))
            {
                return null;
            }

            return selected;
        }

        [CanBeNull]
        private Repository GetSelectedRepository()
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return null;
            }

            var selected = listView1.SelectedItems[0].Tag as Repository;
            if (string.IsNullOrWhiteSpace(selected?.Path))
            {
                return null;
            }

            return selected;
        }

        private ListViewGroup GetTileGroup(Repository repository)
        {
            return listView1.Groups.Cast<ListViewGroup>()
                .SingleOrDefault(x => GroupHeaderComparer.Equals(x.Header, repository.Category));
        }

        private Size GetTileSize(IEnumerable<RecentRepoInfo> recentRepositories, IEnumerable<RecentRepoInfo> favouriteRepositories)
        {
            var spacing1 = DpiUtil.Scale(1f);
            var spacing2 = DpiUtil.Scale(2f);

            var longestPath = recentRepositories.Union(favouriteRepositories)
                                                .Select(r =>
                                                {
                                                    var size = TextRenderer.MeasureText(r.Caption, AppSettings.Font);
                                                    return new
                                                    {
                                                        r.Caption,
                                                        size.Width,
                                                        size.Height
                                                    };
                                                })
                                               .OrderByDescending(r => r.Width)
                                               .First();
            var branchTextSize = TextRenderer.MeasureText("A", _secondaryFont);

            var width = AppSettings.RecentReposComboMinWidth;
            if (width < 1)
            {
                width = longestPath.Width + imageList1.ImageSize.Width;
            }

            var height = longestPath.Height + (2 * branchTextSize.Height) +
                /* offset from top and bottom */(2 * spacing2) +
                /* twice space between text */(2 * spacing1);

            return new Size((int)(width + DpiUtil.Scale(50f)), (int)Math.Max(height, DpiUtil.Scale(50f)));
        }

        private static void RepositoryContextAction(ToolStripItem menuItem, Action<Repository> action)
        {
            var selected = GetSelectedRepository(menuItem);
            if (selected != null)
            {
                action(selected);
            }
        }

        private static string ShortenText(string text, Font font, float maxWidth)
        {
            var ellipsis = '…';
            var width = TextRenderer.MeasureText(text, font).Width;
            if (width < maxWidth)
            {
                return text;
            }

            while (text.Length > 1)
            {
                width = TextRenderer.MeasureText(text + ellipsis, font).Width;
                if (width < maxWidth)
                {
                    break;
                }

                text = text.Substring(0, text.Length - 1);
            }

            return text + ellipsis;
        }

        private bool PromptCategoryName(List<string> categories, string originalName, out string name)
        {
            using (var dialog = new FormDashboardCategoryTitle(categories, originalName))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    name = dialog.Category;
                    return true;
                }

                name = null;
                return false;
            }
        }

        private bool PromptUserConfirm(string question, string caption)
        {
            var dialogResult = MessageBox.Show(this,
                question,
                caption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            return dialogResult == DialogResult.Yes;
        }

        private void UpdateCategoryName(string originalName, string newName)
        {
            foreach (var repository in GetRepositories().Where(r => r.Category == originalName))
            {
                ThreadHelper.JoinableTaskFactory.Run(() => _controller.AssignCategoryAsync(repository, newName));
            }

            ShowRecentRepositories();
        }

        private void contextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ShowRecentRepositories();
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selected = GetSelectedRepository();

            tsmiRemoveFromList.Visible =
                toolStripMenuItem1.Visible =
                    tsmiCategories.Visible =
                        toolStripMenuItem2.Visible =
                            tsmiOpenFolder.Visible = selected != null;

            tsmiRemoveMissingReposFromList.Visible = _hasInvalidRepos;

            if (selected == null)
            {
                e.Cancel = true;
                return;
            }

            // address a bug in context menu implementation
            // nested toolstrip items can't get source control
            // http://stackoverflow.com/questions/30534417/
            contextMenuStripRepository.Tag = selected;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var spacing1 = DpiUtil.Scale(1f);
            var spacing2 = DpiUtil.Scale(2f);
            var spacing4 = DpiUtil.Scale(4f);
            var spacing6 = DpiUtil.Scale(6f);

            var textOffset = spacing2 + imageList1.ImageSize.Width + spacing2;
            int textWidth = e.Bounds.Width - (int)textOffset;

            if (e.Item == HoveredItem)
            {
                e.Graphics.FillRectangle(_hoverColorBrush, e.Bounds);
            }
            else
            {
                e.DrawBackground();
            }

            var pointImage = new PointF(e.Bounds.Left + spacing4, e.Bounds.Top + (spacing2 * 4));

            // render anchor icon
            if (!string.IsNullOrWhiteSpace((e.Item.Tag as Repository)?.Category))
            {
                var pointImage1 = new PointF(pointImage.X + imageList1.ImageSize.Width - 12, e.Bounds.Top + spacing2);
                e.Graphics.DrawImage(Images.Star, pointImage1.X, pointImage1.Y, 16, 16);
            }

            // render icon
            e.Graphics.DrawImage(imageList1.Images[e.Item.ImageIndex], pointImage);

            // render path
            var textPadding = new PointF(e.Bounds.Left + spacing4, e.Bounds.Top + spacing6);
            var pointPath = new PointF(textPadding.X + textOffset, textPadding.Y);
            var pathBounds = DrawText(e.Graphics, e.Item.Text, AppSettings.Font, _foreColorBrush, textWidth, pointPath, spacing4 * 2);

            // render branch
            var pointBranch = new PointF(pointPath.X, pointPath.Y + pathBounds.Height + spacing1);
            var branchBounds = DrawText(e.Graphics, e.Item.SubItems[1].Text, _secondaryFont, _branchNameColorBrush, textWidth, pointBranch, spacing4 * 2);

            // render category
            if (e.Item.SubItems.Count > 2 && !string.IsNullOrWhiteSpace(e.Item.SubItems[2].Text))
            {
                var pointCategory = string.IsNullOrWhiteSpace(e.Item.SubItems[1].Text) ?
                                    pointBranch :
                                    new PointF(pointBranch.X, pointBranch.Y + branchBounds.Height + spacing1);
                DrawText(e.Graphics, e.Item.SubItems[2].Text, _secondaryFont, SystemBrushes.GrayText, textWidth, pointCategory, spacing4 * 2);
            }

            RectangleF DrawText(Graphics g, string text, Font font, Brush brush, int maxTextWidth, PointF location, float spacing)
            {
                var textBounds = TextRenderer.MeasureText(text, font);
                var minWidth = Math.Min(textBounds.Width + spacing, maxTextWidth);
                var bounds = new RectangleF(location, new SizeF(minWidth, textBounds.Height));
                var text1 = Math.Abs(maxTextWidth - minWidth) < float.Epsilon ? ShortenText(text, font, minWidth) : text;
                g.DrawString(text1, font, brush, bounds, StringFormat.GenericTypographic);

                return bounds;
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var selected = GetSelectedRepository();
                if (selected == null)
                {
                    return;
                }

                if (!_controller.IsValidGitWorkingDir(selected.Path))
                {
                    var dialogResult = MessageBox.Show(this, _directoryIsNotAValidRepository.Text,
                                                             _directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNo,
                                                             MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    if (dialogResult == DialogResult.Yes)
                    {
                        _controller.RemoveRepository(selected.Path);
                        ShowRecentRepositories();
                    }

                    return;
                }

                OnModuleChanged(new GitModuleEventArgs(new GitModule(selected.Path)));
            }
        }

        private void listView1_GroupMouseUp(object sender, ListViewGroupMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var groupHitInfo = listView1.GetGroupHitInfo(e.Location);

                bool isRecentRepositoriesGroup = groupHitInfo.Group == _lvgRecentRepositories;
                tsmiCategoryDelete.Visible = !isRecentRepositoriesGroup;
                tsmiCategoryRename.Visible = !isRecentRepositoriesGroup;
                tsmiCategoryClear.Visible = isRecentRepositoriesGroup;

                contextMenuStripCategory.Tag = groupHitInfo.Group;
                contextMenuStripCategory.Show(listView1, e.Location);
            }
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            HoveredItem = listView1.GetItemAt(e.X, e.Y);
        }

        private void listView1_MouseLeave(object sender, EventArgs e)
        {
            HoveredItem = null;
        }

        private void mnuConfigure_Click(object sender, EventArgs e)
        {
            using (var frm = new FormRecentReposSettings())
            {
                var result = frm.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    ShowRecentRepositories();
                }
            }
        }

        private void RecentRepositoriesList_Load(object sender, EventArgs e)
        {
            if (!(Parent.FindForm() is FormBrowse form))
            {
                return;
            }

            var menus = new ToolStripItem[] { mnuConfigure };
            var menuStrip = form.FindDescendantOfType<MenuStrip>(p => p.Name == "menuStrip1");
            var dashboardMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "dashboardToolStripMenuItem");
            dashboardMenu?.DropDownItems.AddRange(menus);
        }

        private void tsmiCategories_DropDownOpening(object sender, EventArgs e)
        {
            if (sender != tsmiCategories)
            {
                return;
            }

            tsmiCategories.DropDownItems.Clear();

            var categories = GetCategories();
            if (categories.Count > 0)
            {
                tsmiCategories.DropDownItems.Add(tsmiCategoryNone);
                tsmiCategories.DropDownItems.AddRange(categories.Select(category =>
                {
                    var item = new ToolStripMenuItem(category) { Tag = category };
                    item.Click += tsmiCategory_Click;
                    return item;
                }).ToArray<ToolStripItem>());
                tsmiCategories.DropDownItems.Add(new ToolStripSeparator());
            }

            tsmiCategories.DropDownItems.Add(tsmiCategoryAdd);

            RepositoryContextAction(tsmiCategories, repository =>
            {
                foreach (ToolStripItem item in tsmiCategories.DropDownItems)
                {
                    item.Enabled = item.Text != repository.Category;
                }

                if (string.IsNullOrWhiteSpace(repository.Category) && tsmiCategories.DropDownItems.Count > 1)
                {
                    tsmiCategories.DropDownItems[0].Enabled = false;
                }
            });
        }

        private void tsmiCategory_Click(object sender, EventArgs e)
        {
            var repository = GetSelectedRepository((sender as ToolStripMenuItem)?.OwnerItem);
            if (repository == null)
            {
                return;
            }

            var category = (sender as ToolStripMenuItem)?.Tag as string;
            ThreadHelper.JoinableTaskFactory.Run(() => _controller.AssignCategoryAsync(repository, category));
            ShowRecentRepositories();
        }

        private void tsmiCategoryAdd_Click(object sender, EventArgs e)
        {
            RepositoryContextAction((sender as ToolStripMenuItem)?.OwnerItem, repository =>
            {
                if (PromptCategoryName(GetCategories(), originalName: null, out string categoryName))
                {
                    ThreadHelper.JoinableTaskFactory.Run(() => _controller.AssignCategoryAsync(repository, categoryName));
                    ShowRecentRepositories();
                }
            });
        }

        private void tsmiOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                RepositoryContextAction(sender as ToolStripMenuItem, repository => Process.Start(repository.Path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void tsmiRemoveFromList_Click(object sender, EventArgs e)
        {
            RepositoryContextAction(sender as ToolStripMenuItem, repository =>
            {
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(repository.Path));
                ShowRecentRepositories();
            });
        }

        private void tsmiRemoveMissingReposFromList_Click(object sender, EventArgs e)
        {
            RepositoryContextAction(sender as ToolStripMenuItem, repository =>
            {
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(_controller.IsValidGitWorkingDir));
                ShowRecentRepositories();
            });
        }

        private void tsmiCategoryRename_Click(object sender, EventArgs e)
        {
            var categoryGroup = (ListViewGroup)contextMenuStripCategory.Tag;
            string originalName = categoryGroup.Name;

            var categories = GetCategories();
            categories.Remove(originalName);

            if (PromptCategoryName(categories, originalName, out string newName))
            {
                UpdateCategoryName(originalName, newName);
            }
        }

        private void tsmiCategoryDelete_Click(object sender, EventArgs e)
        {
            var categoryGroup = (ListViewGroup)contextMenuStripCategory.Tag;
            string name = categoryGroup.Name;
            string question = string.Format(_deleteCategoryQuestion.Text, name, categoryGroup.Items.Count);

            if (!PromptUserConfirm(question, _deleteCategoryCaption.Text))
            {
                return;
            }

            UpdateCategoryName(name, null);
        }

        private void tsmiCategoryClear_Click(object sender, EventArgs e)
        {
            var repositories = GetRepositories().ToList();
            string question = string.Format(_clearRecentCategoryQuestion.Text, repositories.Count);
            if (!PromptUserConfirm(question, _clearRecentCategoryCaption.Text))
            {
                return;
            }

            foreach (var repository in repositories)
            {
                ThreadHelper.JoinableTaskFactory.Run(
                    () => RepositoryHistoryManager.Locals.RemoveRecentAsync(repository.Path));
            }

            ShowRecentRepositories();
        }
    }
}
