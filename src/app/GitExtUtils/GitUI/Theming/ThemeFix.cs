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

            container.DescendantsToFix<DataGridView>()
                .ForEach(SetupDataGridView);
            container.DescendantsToFix<TreeView>()
                .ForEach(SetupTreeView);
            container.DescendantsToFix<TabControl>()
                .ForEach(SetupTabControl);
            container.DescendantsToFix<TextBoxBase>()
                 .ForEach(SetupTextBoxBase);
            container.DescendantsToFix<LinkLabel>()
                .ForEach(SetupLinkLabel);
            container.DescendantsToFix<ToolStrip>()
                .ForEach(SetupToolStrip);
            container.DescendantsToFix<Button>()
                .ForEach(SetupButton);
            container.ContextMenusToFix()
                .ForEach(SetupContextMenu);
        }

        private static IEnumerable<TControl> DescendantsToFix<TControl>(this Control c)
            where TControl : Control
        {
            return c.FindDescendantsOfType<TControl>(SkipThemeAware)
                .Where(control => TryAddToWeakTable(control, AlreadyFixedControls));
        }

        private static IEnumerable<ContextMenuStrip> ContextMenusToFix(this Control c)
        {
            return c.FindDescendantsOfType<Control>()
                .Where(control => TryAddToWeakTable(control, AlreadyFixedContextMenuOwners))
                .Select(_ => _.ContextMenuStrip)
                .Where(_ => _ is not null);
        }

        private static bool SkipThemeAware(Control c) =>
            c.GetType().GetCustomAttribute<ThemeAwareAttribute>() is not null;

        private static void SetupTextBoxBase(TextBoxBase textBox)
        {
            textBox.TouchBackColor();
            if (textBox.BorderStyle == BorderStyle.Fixed3D)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private static void SetupToolStrip(ToolStrip strip)
        {
            strip.UseExtendedThemeAwareRenderer();
        }

        private static void SetupContextMenu(ContextMenuStrip strip)
        {
            strip.UseExtendedThemeAwareRenderer();
        }

        private static void SetupLinkLabel(this LinkLabel label)
        {
            label.LinkColor = label.LinkColor.AdaptTextColor();
            label.VisitedLinkColor = label.VisitedLinkColor.AdaptTextColor();
            label.ActiveLinkColor = label.ActiveLinkColor.AdaptTextColor();
        }

        private static void SetupButton(this Button button)
        {
            // .net9 fix for https://github.com/dotnet/winforms/issues/11949 (only supposed to occur for 100%)
            if (Application.IsDarkModeEnabled && button.FlatStyle == FlatStyle.Standard)
            {
                // In addition to not setting the BackColor (TouchBackColor() will fix),
                // FlatStyle.Standard buttons look ugly in dark mode
                button.FlatStyle = FlatStyle.Flat;
            }
        }

        private static void SetupTabControl(TabControl tabControl)
        {
            new TabControlRenderer(tabControl).Setup();
            tabControl.TabPages.OfType<TabPage>()
                .ForEach(SetupTabPage);
        }

        private static void SetupTabPage(TabPage page)
        {
            if (page.BackColor.IsKnownColor)
            {
                page.TouchBackColor();
            }
        }

        private static void SetupDataGridView(DataGridView view)
        {
            view.EnableHeadersVisualStyles = false;
            view.ColumnHeadersDefaultCellStyle.BackColor = view.ColumnHeadersDefaultCellStyle.BackColor;
        }

        private static void SetupTreeView(TreeView view)
        {
        }

        private static void TouchBackColor(this Control c)
        {
            c.BackColor = c.BackColor;
        }

        private static void TouchForeColor(this Control c)
        {
            c.ForeColor = c.ForeColor;
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
