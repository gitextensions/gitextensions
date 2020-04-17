using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GitUI;

namespace GitExtUtils.GitUI.Theming
{
    public static class ThemeFix
    {
        private static readonly ConditionalWeakTable<IWin32Window, IWin32Window> AlreadyFixedControls =
            new ConditionalWeakTable<IWin32Window, IWin32Window>();

        private static readonly ConditionalWeakTable<IWin32Window, IWin32Window> AlreadyFixedContextMenuOwners =
            new ConditionalWeakTable<IWin32Window, IWin32Window>();

        public static ThemeSettings ThemeSettings { private get; set; } = ThemeSettings.Default;

        public static void FixVisualStyle(this Control container)
        {
            if (ThemeSettings.UseSystemVisualStyle)
            {
                return;
            }

            container.DescendantsToFix<GroupBox>()
                 .ForEach(SetupGroupBox);
            container.DescendantsToFix<TreeView>()
                .ForEach(SetupTreeView);
            container.DescendantsToFix<ListBox>()
                .ForEach(SetupListBox);
            container.DescendantsToFix<TabControl>()
                .ForEach(SetupTabControl);
            container.DescendantsToFix<TextBoxBase>()
                 .ForEach(SetupTextBoxBase);
            container.DescendantsToFix<ComboBox>()
                 .ForEach(SetupComboBox);
            container.DescendantsToFix<LinkLabel>()
                .ForEach(SetupLinkLabel);
            container.DescendantsToFix<ToolStrip>()
                .ForEach(SetupToolStrip);
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
                .Where(_ => _ != null);
        }

        private static bool SkipThemeAware(Control c) =>
            c.GetType().GetCustomAttribute<ThemeAwareAttribute>() != null;

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
            strip.Renderer = new ThemeAwareToolStripRenderer();
            strip.Items.OfType<ToolStripLabel>()
                .ForEach(SetupToolStripLabel);
        }

        private static void SetupContextMenu(ContextMenuStrip strip)
        {
            strip.Renderer = new ThemeAwareToolStripRenderer();
        }

        private static void SetupToolStripLabel(ToolStripLabel label)
        {
            label.LinkColor = label.LinkColor.AdaptTextColor();
            label.VisitedLinkColor = label.VisitedLinkColor.AdaptTextColor();
            label.ActiveLinkColor = label.ActiveLinkColor.AdaptTextColor();
        }

        private static void SetupLinkLabel(this LinkLabel label)
        {
            label.LinkColor = label.LinkColor.AdaptTextColor();
            label.VisitedLinkColor = label.VisitedLinkColor.AdaptTextColor();
            label.ActiveLinkColor = label.ActiveLinkColor.AdaptTextColor();
        }

        private static void SetupGroupBox(this GroupBox box)
        {
            box.TouchForeColor();
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

        private static void SetupTreeView(TreeView view)
        {
            var unused = view.Handle; // force handle creation
            view.TouchBackColor();
            view.TouchForeColor();
            view.LineColor = SystemColors.ControlDark;
        }

        private static void SetupListBox(ListBox view)
        {
            if (view.BorderStyle == BorderStyle.Fixed3D)
            {
                view.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private static void SetupComboBox(this ComboBox menu)
        {
            menu.TouchBackColor();
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
