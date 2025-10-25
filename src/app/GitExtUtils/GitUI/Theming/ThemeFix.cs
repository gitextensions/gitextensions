using System.Reflection;
using System.Runtime.CompilerServices;
using GitUI;

namespace GitExtUtils.GitUI.Theming
{
    public static class ThemeFix
    {
        private static readonly ConditionalWeakTable<IWin32Window, IWin32Window> AlreadyFixedControls =
            [];

        private static readonly ConditionalWeakTable<IWin32Window, IWin32Window> AlreadyFixedContextMenuOwners =
            [];

        public static ThemeSettings ThemeSettings { private get; set; } = ThemeSettings.Default;

        public static void FixVisualStyle(this Control container)
        {
            if (ThemeSettings.UseSystemVisualStyle)
            {
                return;
            }

            container.DescendantsToFix<ToolStrip>()
                .ForEach(SetupToolStrip);
            container.ContextMenusToFix()
                .ForEach(SetupContextMenu);
            container.DescendantsToFix<DataGridView>()
                .ForEach(SetupDataGridView);
            container.DescendantsToFix<LinkLabel>()
                .ForEach(SetupLinkLabel);
            container.DescendantsToFix<TabControl>()
                .ForEach(SetupTabControl);
            container.DescendantsToFix<TextBoxBase>()
                 .ForEach(SetupTextBoxBase);
            container.DescendantsToFix<Button>()
                .ForEach(SetupButton);
        }

        private static IEnumerable<TControl> DescendantsToFix<TControl>(this Control c)
            where TControl : Control
        {
            return c.FindDescendantsOfType<TControl>()
                .Where(control => TryAddToWeakTable(control, AlreadyFixedControls));
        }

        private static IEnumerable<ContextMenuStrip> ContextMenusToFix(this Control c)
        {
            return c.FindDescendantsOfType<Control>()
                .Where(control => TryAddToWeakTable(control, AlreadyFixedContextMenuOwners))
                .Select(_ => _.ContextMenuStrip)
                .Where(_ => _ is not null);
        }

        private static void SetupTextBoxBase(TextBoxBase textBox)
        {
            // TODO use custom Paint?
            // Fixed3D is default, has thicker border than comboboxes and blue underline when input
            // FixedSingle has thinner border than Comboboxes but is slightly more the same
            ////if (textBox.BorderStyle == BorderStyle.Fixed3D)
            ////{
            ////    textBox.BorderStyle = BorderStyle.FixedSingle;
            ////}
        }

        private static void SetupToolStrip(ToolStrip strip)
        {
            // TOODO .NET10 Seem to be required for two reasons:
            // * sidepanel and browse branch icons has "marked" color, not just borders
            // * LinkColor is always dark blue (and cannot be overridden with other RenderMode)
            strip.RenderMode = ToolStripRenderMode.Professional;
            foreach (ToolStripLabel item in strip.Items.OfType<ToolStripLabel>())
            {
                SetupToolStripStatusLabel(item);
            }
        }

        private static void SetupContextMenu(ContextMenuStrip strip)
        {
            // TODO No changes detected by this override in e.g. FormGitCommandLog
            // strip.RenderMode = ToolStripRenderMode.Professional;
        }

        private static void SetupLinkLabel(this LinkLabel label)
        {
            // e.g. FormAbout
            label.LinkColor = Application.IsDarkModeEnabled ? Color.CornflowerBlue : label.LinkColor.AdaptTextColor();
            label.VisitedLinkColor = label.VisitedLinkColor.AdaptTextColor();
            label.ActiveLinkColor = label.ActiveLinkColor.AdaptTextColor();
        }

        private static void SetupToolStripStatusLabel(this ToolStripLabel label)
        {
            // e.g. FormCommit
            label.LinkColor = Application.IsDarkModeEnabled ? Color.CornflowerBlue : label.LinkColor.AdaptTextColor();
            label.VisitedLinkColor = label.VisitedLinkColor.AdaptTextColor();
            label.ActiveLinkColor = label.ActiveLinkColor.AdaptTextColor();
        }

        private static void SetupButton(this Button button)
        {
            // FlatStyle.Standard cannot set button color.
            if (Application.IsDarkModeEnabled && button.FlatStyle == FlatStyle.Standard)
            {
                button.FlatStyle = FlatStyle.Flat;
            }
        }

        private static void SetupTabControl(TabControl tabControl)
        {
            // TODO The tabs have mostly the same color, hard to see the active tab otherwise
            new TabControlRenderer(tabControl).Setup();

            tabControl.TabPages.OfType<TabPage>()
                .ForEach(SetupTabPage);
        }

        private static void SetupTabPage(TabPage page)
        {
            // FormPush upper part is not painted correctly
            if (page.BackColor.IsKnownColor)
            {
                page.TouchBackColor();
            }
        }

        private static void SetupDataGridView(DataGridView view)
        {
            // NET10 still light color header (but this workaround is not perfect)
            view.EnableHeadersVisualStyles = false;
        }

        private static void TouchBackColor(this Control c)
        {
            c.BackColor = c.BackColor;
        }

        private static bool TryAddToWeakTable(IWin32Window element, ConditionalWeakTable<IWin32Window, IWin32Window> weakTable)
        {
            if (weakTable.TryGetValue(element, out _))
            {
                return false;
            }

            weakTable.Add(element, element);
            return true;
        }
    }
}
