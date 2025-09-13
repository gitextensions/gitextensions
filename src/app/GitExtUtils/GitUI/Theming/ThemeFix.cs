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
#if !NET10_0_OR_GREATER
            // TODO No longer visual artifacts, but more consistent to force
            textBox.TouchBackColor();
            if (textBox.BorderStyle == BorderStyle.Fixed3D)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
#endif
        }

        private static void SetupToolStrip(ToolStrip strip)
        {
            // .NET10 Seem to be required for two reasons:
            // * sidepanel and browse branch icons has "marked" color, not just borders
            // * LinkColor is always dark blue (and cannot be overridden)
            strip.RenderMode = ToolStripRenderMode.Professional;
            foreach (ToolStripLabel item in strip.Items.OfType<ToolStripLabel>())
            {
                SetupToolStripStatusLabel(item);
            }
        }

        private static void SetupContextMenu(ContextMenuStrip strip)
        {
#if !NET10_0_OR_GREATER
            // TODO Confirm this is not needed
            strip.RenderMode = ToolStripRenderMode.Professional;
#endif
        }

        private static void SetupLinkLabel(this LinkLabel label)
        {
#if !NET10_0_OR_GREATER
            // TODO Confirm this is not needed
            // See also SetupToolStrip(), has this override an effect in .net10?
            label.LinkColor = Application.IsDarkModeEnabled ? Color.CornflowerBlue : label.LinkColor.AdaptTextColor();
            label.VisitedLinkColor = label.VisitedLinkColor.AdaptTextColor();
            label.ActiveLinkColor = label.ActiveLinkColor.AdaptTextColor();
#endif
        }

        private static void SetupToolStripStatusLabel(this ToolStripLabel label)
        {
            label.LinkColor = Application.IsDarkModeEnabled ? Color.CornflowerBlue : label.LinkColor.AdaptTextColor();
            label.VisitedLinkColor = label.VisitedLinkColor.AdaptTextColor();
            label.ActiveLinkColor = label.ActiveLinkColor.AdaptTextColor();
        }

        private static void SetupButton(this Button button)
        {
#if !NET10_0_OR_GREATER
            // TODO No longer visual artifacts, but more consistent to force?
            // .net9 fix for https://github.com/dotnet/winforms/issues/11949 (only supposed to occur for 100%)
            if (Application.IsDarkModeEnabled && button.FlatStyle == FlatStyle.Standard)
            {
                // In addition to not setting the BackColor (TouchBackColor() will fix),
                // FlatStyle.Standard buttons look ugly in dark mode
                button.FlatStyle = FlatStyle.Flat;
            }
#endif
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
