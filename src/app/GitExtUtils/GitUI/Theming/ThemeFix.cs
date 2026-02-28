using System.Runtime.CompilerServices;
using GitUI;

namespace GitExtUtils.GitUI.Theming;

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
        container.DescendantsToFix<DataGridView>()
            .ForEach(SetupDataGridView);
        container.DescendantsToFix<LinkLabel>()
            .ForEach(SetupLinkLabel);
        container.DescendantsToFix<TabControl>()
            .ForEach(SetupTabControl);
        container.DescendantsToFix<Button>()
            .ForEach(SetupButton);
    }

    private static IEnumerable<TControl> DescendantsToFix<TControl>(this Control c)
        where TControl : Control
    {
        return c.FindDescendantsOfType<TControl>()
            .Where(control => TryAddToWeakTable(control, AlreadyFixedControls));
    }

    // Note: TextBoxBase is not overridden, it looks a little of (requires custom paint).
    // Fixed3D is default, has thicker border than comboboxes and blue underline when input.
    // FixedSingle has thinner border than Comboboxes but is slightly more the same.

    private static void SetupToolStrip(ToolStrip strip)
    {
        // RenderMode seem to be required for two reasons:
        // * FormBrowse menubar background is not overridden.
        // * LinkColor override (in SetupToolStripStatusLabel()).
        strip.RenderMode = ToolStripRenderMode.Professional;
        foreach (ToolStripLabel item in strip.Items.OfType<ToolStripLabel>())
        {
            SetupToolStripStatusLabel(item);
        }
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
        // e.g. reset another branch where force is required
        // FlatStyle.Standard cannot set button color.
        if (Application.IsDarkModeEnabled && button.FlatStyle == FlatStyle.Standard)
        {
            button.FlatStyle = FlatStyle.Flat;
        }
    }

    private static void SetupTabControl(TabControl tabControl)
    {
        // e.g. FormBrowse tabs
        // The tabs have mostly the same color, hard to see the active tab otherwise
        new TabControlRenderer(tabControl).Setup();

        tabControl.TabPages.OfType<TabPage>()
            .ForEach(SetupTabPage);
    }

    private static void SetupTabPage(TabPage page)
    {
        // e.g. FormPush
        // upper part is not painted correctly
        if (page.BackColor.IsKnownColor)
        {
            page.TouchBackColor();
        }
    }

    private static void SetupDataGridView(DataGridView view)
    {
        // e.g. Settings - RevisionLinks
        // still light color header (but this workaround is not perfect)
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
