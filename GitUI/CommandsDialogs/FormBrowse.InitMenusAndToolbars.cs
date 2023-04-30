using System.Diagnostics;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.Shells;
using GitUI.UserControls;

namespace GitUI.CommandsDialogs
{
    partial class FormBrowse
    {
        // This file is dedicated to init logic for FormBrowse menus and toolbars

        internal static readonly string FetchPullToolbarShortcutsPrefix = "pull_shortcut_";

        private void InitMenusAndToolbars(string? revFilter, string? pathFilter)
        {
            commandsToolStripMenuItem.DropDownOpening += CommandsToolStripMenuItem_DropDownOpening;

            InitFilters();

            toolPanel.TopToolStripPanel.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _formBrowseMenus.ShowToolStripContextMenu(Cursor.Position);
                }
            };

            new ToolStripItem[]
            {
                recoverLostObjectsToolStripMenuItem,
                branchSelect,
                toolStripButtonPull,
                pullToolStripMenuItem,
                pullToolStripMenuItem1,
                mergeToolStripMenuItem,
                rebaseToolStripMenuItem1,
                fetchToolStripMenuItem,
                fetchAllToolStripMenuItem,
                fetchPruneAllToolStripMenuItem,
                toolStripButtonPush,
                pushToolStripMenuItem,
                branchToolStripMenuItem,
            }.ForEach(ColorHelper.AdaptImageLightness);

            InsertFetchPullShortcuts();

            pullToolStripMenuItem1.Tag = AppSettings.PullAction.None;
            mergeToolStripMenuItem.Tag = AppSettings.PullAction.Merge;
            rebaseToolStripMenuItem1.Tag = AppSettings.PullAction.Rebase;
            fetchToolStripMenuItem.Tag = AppSettings.PullAction.Fetch;
            fetchAllToolStripMenuItem.Tag = AppSettings.PullAction.FetchAll;
            fetchPruneAllToolStripMenuItem.Tag = AppSettings.PullAction.FetchPruneAll;

            Color toolForeColor = SystemColors.WindowText;
            BackColor = SystemColors.Window;
            ForeColor = toolForeColor;
            mainMenuStrip.ForeColor = toolForeColor;
            InitToolStripStyles(toolForeColor, Color.Transparent);

            UpdateCommitButtonAndGetBrush(status: null, AppSettings.ShowGitStatusInBrowseToolbar);

            FillNextPullActionAsDefaultToolStripMenuItems();
            RefreshDefaultPullAction();

            FillUserShells(defaultShell: BashShell.ShellName);

            WorkaroundToolbarLocationBug();

            return;

            void InitToolStripStyles(Color toolForeColor, Color toolBackColor)
            {
                toolPanel.TopToolStripPanel.BackColor = toolBackColor;
                toolPanel.TopToolStripPanel.ForeColor = toolForeColor;

                mainMenuStrip.BackColor = toolBackColor;

                ToolStripMain.BackColor = toolBackColor;
                ToolStripMain.ForeColor = toolForeColor;

                ToolStripFilters.BackColor = toolBackColor;
                ToolStripFilters.ForeColor = toolForeColor;
                ToolStripFilters.InitToolStripStyles(toolForeColor, toolBackColor);

                ToolStripScripts.BackColor = toolBackColor;
                ToolStripScripts.ForeColor = toolForeColor;
            }

            void InitFilters()
            {
                // ToolStripFilters.RefreshRevisionFunction() is init in UICommands_PostRepositoryChanged

                if (!string.IsNullOrWhiteSpace(revFilter))
                {
                    ToolStripFilters.SetRevisionFilter(revFilter);
                }

                if (!string.IsNullOrWhiteSpace(pathFilter))
                {
                    SetPathFilter(pathFilter.QuoteNE());
                }
            }

            void WorkaroundToolbarLocationBug()
            {
                // Layout engine bug (?) which may change the order of toolbars
                // if the 1st one becomes longer than the 2nd toolbar's Location.X
                // the layout engine will be place the 2nd toolbar first

                // 1. Clear all toolbars
                toolPanel.TopToolStripPanel.Controls.Clear();

                // 2. Add all the toolbars back in a reverse order, every added toolbar pushing existing ones to the right
                ToolStrip[] toolStrips = new[] { ToolStripScripts, ToolStripFilters, ToolStripMain };
                foreach (ToolStrip toolStrip in toolStrips)
                {
                    toolPanel.TopToolStripPanel.Controls.Add(toolStrip);
                }

#if DEBUG
                // 3. Assert all toolbars on the same row
                foreach (ToolStrip toolStrip in toolStrips)
                {
                    Debug.Assert(toolStrip.Top == 0, $"{toolStrip.Name} must be placed on the 1st row");
                }

                // 4. Assert the correct order of toolbars
                for (int i = toolStrips.Length - 1; i > 0; i--)
                {
                    Debug.Assert(toolStrips[i].Left < toolStrips[i - 1].Left, $"{toolStrips[i - 1].Name} must be placed before {toolStrips[i].Name}");
                }
#endif
            }
        }

        private void InsertFetchPullShortcuts()
        {
            int i = ToolStripMain.Items.IndexOf(toolStripButtonPull);
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchToolStripMenuItem));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchAllToolStripMenuItem));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchPruneAllToolStripMenuItem));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(mergeToolStripMenuItem));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(rebaseToolStripMenuItem1));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(pullToolStripMenuItem1));

            ToolStripButton CreateCorrespondingToolbarButton(ToolStripMenuItem toolStripMenuItem)
            {
                string toolTipText = toolStripMenuItem.Text.Replace("&", string.Empty);
                ToolStripButton clonedToolStripMenuItem = new()
                {
                    Image = toolStripMenuItem.Image,
                    ImageTransparentColor = toolStripMenuItem.ImageTransparentColor,
                    Name = FetchPullToolbarShortcutsPrefix + toolStripMenuItem.Name,
                    Size = toolStripMenuItem.Size,
                    Text = toolTipText,
                    ToolTipText = toolTipText,
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                };

                clonedToolStripMenuItem.Click += (_, _) => toolStripMenuItem.PerformClick();
                return clonedToolStripMenuItem;
            }
        }

        private void FillNextPullActionAsDefaultToolStripMenuItems()
        {
            var setDefaultPullActionDropDown = (ToolStripDropDownMenu)setDefaultPullButtonActionToolStripMenuItem.DropDown;

            // Show both Check and Image margins in a menu
            setDefaultPullActionDropDown.ShowImageMargin = true;
            setDefaultPullActionDropDown.ShowCheckMargin = true;

            // Prevent submenu from closing while options are changed
            setDefaultPullActionDropDown.Closing += (sender, args) =>
            {
                if (args.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    args.Cancel = true;
                }
            };

            var setDefaultPullActionDropDownItems = toolStripButtonPull.DropDownItems
                .OfType<ToolStripMenuItem>()
                .Where(tsmi => tsmi.Tag is AppSettings.PullAction)
                .Select(tsmi =>
                {
                    ToolStripItem tsi = new ToolStripMenuItem
                    {
                        Name = $"{tsmi.Name}SetDefault",
                        Text = tsmi.Text,
                        CheckOnClick = true,
                        Image = tsmi.Image,
                        Tag = tsmi.Tag
                    };

                    tsi.Click += SetDefaultPullActionMenuItemClick;

                    return tsi;
                });

            setDefaultPullActionDropDown.Items.AddRange(setDefaultPullActionDropDownItems.ToArray());

            void SetDefaultPullActionMenuItemClick(object sender, EventArgs eventArgs)
            {
                var clickedMenuItem = (ToolStripMenuItem)sender;
                AppSettings.DefaultPullAction = (AppSettings.PullAction)clickedMenuItem.Tag;
                RefreshDefaultPullAction();
            }
        }

        private void FillUserShells(string defaultShell)
        {
            userShell.DropDownItems.Clear();

            bool userShellAccessible = false;
            ToolStripMenuItem? selectedDefaultShell = null;
            foreach (IShellDescriptor shell in _shellProvider.GetShells())
            {
                if (!shell.HasExecutable)
                {
                    continue;
                }

                ToolStripMenuItem toolStripMenuItem = new(shell.Name);
                userShell.DropDownItems.Add(toolStripMenuItem);
                toolStripMenuItem.Tag = shell;
                toolStripMenuItem.Image = shell.Icon;
                toolStripMenuItem.ToolTipText = shell.Name;
                toolStripMenuItem.Click += userShell_Click;

                if (selectedDefaultShell is null || string.Equals(shell.Name, defaultShell, StringComparison.InvariantCultureIgnoreCase))
                {
                    userShellAccessible = true;
                    selectedDefaultShell = toolStripMenuItem;
                }
            }

            if (selectedDefaultShell is not null)
            {
                userShell.Image = selectedDefaultShell.Image;
                userShell.ToolTipText = selectedDefaultShell.ToolTipText;
                userShell.Tag = selectedDefaultShell.Tag;
            }

            userShell.Visible = userShell.DropDownItems.Count > 0;

            // a user may have a specific shell configured in settings, but the shell is no longer available
            // set the first available shell as default
            if (userShell.Visible && !userShellAccessible)
            {
                var shell = (IShellDescriptor)userShell.DropDownItems[0].Tag;
                userShell.Image = shell.Icon;
                userShell.ToolTipText = shell.Name;
                userShell.Tag = shell;
            }
        }

        private void RefreshDefaultPullAction()
        {
            if (setDefaultPullButtonActionToolStripMenuItem is null)
            {
                // We may get called while instantiating the form
                return;
            }

            var defaultPullAction = AppSettings.DefaultPullAction;

            foreach (ToolStripMenuItem menuItem in setDefaultPullButtonActionToolStripMenuItem.DropDown.Items)
            {
                menuItem.Checked = (AppSettings.PullAction)menuItem.Tag == defaultPullAction;
            }

            switch (defaultPullAction)
            {
                case AppSettings.PullAction.Fetch:
                    toolStripButtonPull.Image = Images.PullFetch.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullFetch.Text;
                    break;

                case AppSettings.PullAction.FetchAll:
                    toolStripButtonPull.Image = Images.PullFetchAll.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                    break;

                case AppSettings.PullAction.FetchPruneAll:
                    toolStripButtonPull.Image = Images.PullFetchPruneAll.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullFetchPruneAll.Text;
                    break;

                case AppSettings.PullAction.Merge:
                    toolStripButtonPull.Image = Images.PullMerge.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullMerge.Text;
                    break;

                case AppSettings.PullAction.Rebase:
                    toolStripButtonPull.Image = Images.PullRebase.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullRebase.Text;
                    break;

                default:
                    toolStripButtonPull.Image = Images.Pull.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullOpenDialog.Text;
                    break;
            }
        }

        private Brush UpdateCommitButtonAndGetBrush(IReadOnlyList<GitItemStatus>? status, bool showCount)
        {
            RepoStateVisualiser repoStateVisualiser = new();
            var (image, brush) = repoStateVisualiser.Invoke(status);

            if (showCount)
            {
                toolStripButtonCommit.Image = image;

                if (status is not null)
                {
                    toolStripButtonCommit.Text = string.Format("{0} ({1})", _commitButtonText, status.Count);
                    toolStripButtonCommit.AutoSize = true;
                }
                else
                {
                    int width = toolStripButtonCommit.Width;
                    toolStripButtonCommit.Text = _commitButtonText.Text;
                    if (width > toolStripButtonCommit.Width)
                    {
                        toolStripButtonCommit.AutoSize = false;
                        toolStripButtonCommit.Width = width;
                    }
                }
            }
            else
            {
                toolStripButtonCommit.Image = repoStateVisualiser.Invoke(new List<GitItemStatus>()).image;

                toolStripButtonCommit.Text = _commitButtonText.Text;
                toolStripButtonCommit.AutoSize = true;
            }

            return brush;
        }
    }
}
