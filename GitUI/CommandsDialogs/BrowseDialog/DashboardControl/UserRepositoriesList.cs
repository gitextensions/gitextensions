using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using GitUI.Properties;
using GitUI.UserControls;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class UserRepositoriesList : GitExtensionsControl
    {
        private readonly TranslationString _groupRecentRepositories = new TranslationString("Recent repositories");
        private readonly TranslationString _directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString _directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");

        private Font _secondaryFont;
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
        private ListViewItem _prevHoveredItem;
        private readonly ListViewGroup _lvgRecentRepositories;
        private readonly IUserRepositoriesListController _controller = new UserRepositoriesListController(RepositoryHistoryManager.Locals);

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;

        public UserRepositoriesList()
        {
            InitializeComponent();
            Translate();

            mnuTop.DropDownItems.Clear();

            _lvgRecentRepositories = new ListViewGroup(_groupRecentRepositories.Text, HorizontalAlignment.Left);

            _foreColorBrush = new SolidBrush(base.ForeColor);

            _secondaryFont = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints - 1f);
            lblRecentRepositories.Font = new Font(AppSettings.Font.FontFamily, AppSettings.Font.SizeInPoints + 5.5f);

            listView1.Items.Clear();
            listView1.ContextMenuStrip = contextMenuStrip;

            imageList1.Images.Clear();
            imageList1.ImageSize = DpiUtil.Scale(imageList1.ImageSize);
            imageList1.Images.Add(Resources.folder_git);
            imageList1.Images.Add(Resources.folder_error);

            this.AdjustForDpiScaling();
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
                if (recentRepositories.Count == 0 && favouriteRepositories.Count == 0)
                {
                    return;
                }

                listView1.TileSize = GetTileSize(recentRepositories, favouriteRepositories);
                Debug.WriteLine($"Tile size: {listView1.TileSize}");

                BindRepositories(recentRepositories);
                BindRepositories(favouriteRepositories);
            }
            finally
            {
                listView1.SetGroupState(ListViewGroupState.Collapsible);
                listView1.EndUpdate();
            }

            void BindRepositories(IReadOnlyList<RecentRepoInfo> repos)
            {
                SetupGroups(repos.Select(repo => repo.Repo.Category)
                                 .Where(c => !string.IsNullOrWhiteSpace(c))
                                 .Distinct()
                                 .OrderBy(x => x));

                foreach (var repository in repos)
                {
                    var item = new ListViewItem(repository.Caption)
                    {
                        ForeColor = ForeColor,
                        Font = AppSettings.Font,
                        Group = GetTileGroup(repository.Repo),
                        ImageIndex = _controller.IsValidGitWorkingDir(repository.Repo.Path) ? 0 : 1,
                        UseItemStyleForSubItems = false,
                        Tag = repository.Repo,
                        ToolTipText = repository.Repo.Path
                    };
                    item.SubItems.Add(_controller.GetCurrentBranchName(repository.Repo.Path), BranchNameColor, BackColor, _secondaryFont);
                    item.SubItems.Add(repository.Repo.Category, SystemColors.GrayText, BackColor, _secondaryFont);

                    listView1.Items.Add(item);
                }

                void SetupGroups(IEnumerable<string> categories)
                {
                    listView1.Groups.Clear();
                    listView1.Groups.Add(_lvgRecentRepositories);
                    categories.ToList().ForEach(c =>
                    {
                        listView1.Groups.Add(c, c);
                    });
                }
            }
        }

        protected virtual void OnModuleChanged(GitModuleEventArgs args)
        {
            var handler = GitModuleChanged;
            handler?.Invoke(this, args);
        }

        private static T FindControl<T>(IEnumerable controls, Func<T, bool> predicate) where T : Control
        {
            foreach (Control control in controls)
            {
                if (control is T result && predicate(result))
                {
                    return result;
                }

                result = FindControl(control.Controls, predicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private string[] GetCategories()
        {
            var categories = listView1.Items.Cast<ListViewItem>()
                                            .Select(lvi => (lvi.Tag as Repository)?.Category)
                                            .Where(category => !string.IsNullOrWhiteSpace(category))
                                            .OrderBy(x => x)
                                            .Distinct()
                                            .ToArray();
            return categories;
        }

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
            var groups = listView1.Groups.Cast<ListViewGroup>().ToArray();
            var category = groups.SingleOrDefault(x => x.Header == repository.Category);
            if (category != null)
            {
                return category;
            }

            return groups.Last();
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
                width = longestPath.Width;
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
            while (text.Length > 1)
            {
                var width = TextRenderer.MeasureText(text, font).Width;
                if (width < maxWidth)
                {
                    break;
                }

                text = text.Substring(0, text.Length - 1);
            }

            return text;
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

            if (selected == null)
            {
                e.Cancel = true;
                return;
            }

            // address a bug in context menu implementation
            // nested toolstrip items can't get source control
            // http://stackoverflow.com/questions/30534417/
            contextMenuStrip.Tag = selected;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var spacing1 = DpiUtil.Scale(1f);
            var spacing2 = DpiUtil.Scale(2f);
            var spacing4 = DpiUtil.Scale(4f);
            var spacing6 = DpiUtil.Scale(6f);

            var textOffset = spacing2 + imageList1.ImageSize.Width + spacing2;
            int textWidth = AppSettings.RecentReposComboMinWidth > 0 ? AppSettings.RecentReposComboMinWidth : e.Bounds.Width;

            if (e.Item == _prevHoveredItem)
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
                e.Graphics.DrawImage(Resources.Star, pointImage1.X, pointImage1.Y, 16, 16);
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
            if (!string.IsNullOrWhiteSpace(e.Item.SubItems[2].Text))
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

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            _prevHoveredItem = item;
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
            var form = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name == nameof(FormBrowse));
            if (form == null)
            {
                return;
            }

            var menus = new ToolStripItem[] { mnuConfigure };
            var menuStrip = FindControl<MenuStrip>(form.Controls, p => p.Name == "menuStrip1");
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
            if (categories.Length > 0)
            {
                tsmiCategories.DropDownItems.Add(tsmiCategoryNone);
                tsmiCategories.DropDownItems.AddRange(categories.Select(category =>
                {
                    var item = new ToolStripMenuItem(category);
                    item.Tag = category;
                    item.Click += tsmiCategory_Click;
                    return item;
                }).ToArray());
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
                using (var dialog = new FormDashboardCategoryTitle(GetCategories()))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    ThreadHelper.JoinableTaskFactory.Run(() => _controller.AssignCategoryAsync(repository, dialog.Category));
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
    }
}
