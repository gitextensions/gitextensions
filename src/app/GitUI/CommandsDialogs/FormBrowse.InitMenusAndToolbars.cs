using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.Shells;
using GitUI.UserControls;
using ResourceManager.Hotkey;

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

            pullToolStripMenuItem1.Tag = GitPullAction.None;
            mergeToolStripMenuItem.Tag = GitPullAction.Merge;
            rebaseToolStripMenuItem1.Tag = GitPullAction.Rebase;
            fetchToolStripMenuItem.Tag = GitPullAction.Fetch;
            fetchAllToolStripMenuItem.Tag = GitPullAction.FetchAll;
            fetchPruneAllToolStripMenuItem.Tag = GitPullAction.FetchPruneAll;

            Color toolForeColor = SystemColors.WindowText;
            BackColor = SystemColors.Window;
            ForeColor = toolForeColor;
            mainMenuStrip.ForeColor = toolForeColor;
            InitToolStripStyles(toolForeColor, Color.Transparent);

            UpdateCommitButtonAndGetBrush(status: null, AppSettings.ShowGitStatusInBrowseToolbar);

            FillNextPullActionAsDefaultToolStripMenuItems();
            RefreshDefaultPullAction();

            FillUserShells(defaultShell: BashShell.ShellName);

            InsertFetchPullShortcuts();

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
                    DebugHelpers.Assert(toolStrip.Top == 0, $"{toolStrip.Name} must be placed on the 1st row");
                }

                // 4. Assert the correct order of toolbars
                for (int i = toolStrips.Length - 1; i > 0; i--)
                {
                    DebugHelpers.Assert(toolStrips[i].Left < toolStrips[i - 1].Left, $"{toolStrips[i - 1].Name} must be placed before {toolStrips[i].Name}");
                }
#endif
            }
        }

        private void UpdateTooltipWithShortcut(ToolStripItem button, Command command)
            => UpdateTooltipWithShortcut(button, GetShortcutKeys(command));

        private static void UpdateTooltipWithShortcut(ToolStripItem button, Keys keys)
            => button.ToolTipText = button.ToolTipText.UpdateSuffix(keys.ToShortcutKeyToolTipString());

        private void InsertFetchPullShortcuts()
        {
            int i = ToolStripMain.Items.IndexOf(toolStripButtonPull);
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchToolStripMenuItem, Command.QuickFetch));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchAllToolStripMenuItem));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchPruneAllToolStripMenuItem));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(mergeToolStripMenuItem, Command.QuickPull));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(rebaseToolStripMenuItem1));
            ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(pullToolStripMenuItem1, Command.PullOrFetch));

            ToolStripButton CreateCorrespondingToolbarButton(ToolStripMenuItem toolStripMenuItem, Command? command = null)
            {
                string toolTipText = toolStripMenuItem.Text.Replace("&", string.Empty);
                ToolStripButton clonedToolStripMenuItem = new()
                {
                    Image = toolStripMenuItem.Image,
                    ImageTransparentColor = toolStripMenuItem.ImageTransparentColor,
                    Name = FetchPullToolbarShortcutsPrefix + toolStripMenuItem.Name,
                    Size = toolStripMenuItem.Size,
                    Text = toolTipText,
                    ToolTipText = toolTipText.UpdateSuffix(command.HasValue ? GetShortcutKeyTooltipString(command.Value) : null),
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                };

                clonedToolStripMenuItem.Click += (_, _) => toolStripMenuItem.PerformClick();
                return clonedToolStripMenuItem;
            }
        }

        private void FillNextPullActionAsDefaultToolStripMenuItems()
        {
            ToolStripDropDownMenu setDefaultPullActionDropDown = (ToolStripDropDownMenu)setDefaultPullButtonActionToolStripMenuItem.DropDown;

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

            IEnumerable<ToolStripItem> setDefaultPullActionDropDownItems = toolStripButtonPull.DropDownItems
                .OfType<ToolStripMenuItem>()
                .Where(tsmi => tsmi.Tag is GitPullAction)
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
                ToolStripMenuItem clickedMenuItem = (ToolStripMenuItem)sender;
                AppSettings.DefaultPullAction = (GitPullAction)clickedMenuItem.Tag;
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
                IShellDescriptor shell = (IShellDescriptor)userShell.DropDownItems[0].Tag;
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

            GitPullAction defaultPullAction = AppSettings.DefaultPullAction;

            foreach (ToolStripMenuItem menuItem in setDefaultPullButtonActionToolStripMenuItem.DropDown.Items)
            {
                menuItem.Checked = (GitPullAction)menuItem.Tag == defaultPullAction;
            }

            switch (defaultPullAction)
            {
                case GitPullAction.Fetch:
                    toolStripButtonPull.Image = Images.PullFetch.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullFetch.Text;
                    break;

                case GitPullAction.FetchAll:
                    toolStripButtonPull.Image = Images.PullFetchAll.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                    break;

                case GitPullAction.FetchPruneAll:
                    toolStripButtonPull.Image = Images.PullFetchPruneAll.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullFetchPruneAll.Text;
                    break;

                case GitPullAction.Merge:
                    toolStripButtonPull.Image = Images.PullMerge.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullMerge.Text;
                    break;

                case GitPullAction.Rebase:
                    toolStripButtonPull.Image = Images.PullRebase.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullRebase.Text;
                    break;

                default:
                    toolStripButtonPull.Image = Images.Pull.AdaptLightness();
                    toolStripButtonPull.ToolTipText = _pullOpenDialog.Text;
                    break;
            }

            UpdateTooltipWithShortcut(toolStripButtonPull, Command.QuickPullOrFetch);
        }

        private Brush UpdateCommitButtonAndGetBrush(IReadOnlyList<GitItemStatus>? status, bool showCount)
        {
            try
            {
                ToolStripMain.SuspendLayout();
                RepoStateVisualiser repoStateVisualiser = new();
                (Image image, Brush brush) = repoStateVisualiser.Invoke(status);

                if (showCount)
                {
                    toolStripButtonCommit.Image = image;

                    if (status is not null)
                    {
                        toolStripButtonCommit.Text = $"{_commitButtonText} ({status.Count})";
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
            finally
            {
                ToolStripMain.ResumeLayout();
            }
        }
    }
}
