using System.Runtime.CompilerServices;
using GitExtUtils.GitUI;

namespace GitUI
{
    public static class ToolStripExtensions
    {
        private static readonly ConditionalWeakTable<ToolStrip, IMenuItemBackgroundFilter> MenuItemBackgroundFilters = [];
        private static readonly ConditionalWeakTable<ToolStrip, ToolStripExSystemRenderer> ExtendedSystemRenderers = [];
        private static readonly ConditionalWeakTable<ToolStrip, ToolStripExProfessionalRenderer> ExtendedProfessionalRenderers = [];
        private static readonly ConditionalWeakTable<ToolStrip, ToolStripExThemeAwareRenderer> ExtendedThemeAwareRenderers = [];

        public static void AttachMenuItemBackgroundFilter(this ToolStrip toolStrip, IMenuItemBackgroundFilter? value)
        {
            toolStrip.UseExtendedRenderer();
            MenuItemBackgroundFilters.Remove(toolStrip);

            if (value is not null)
            {
                MenuItemBackgroundFilters.Add(toolStrip, value);
            }
        }

        private static void UseExtendedRenderer(this ToolStrip toolStrip)
        {
            if (toolStrip.Renderer is ToolStripSystemRenderer and not ToolStripExSystemRenderer)
            {
                toolStrip.Renderer = ExtendedSystemRenderers.GetOrCreateValue(toolStrip);
            }
            else if (toolStrip.Renderer is ToolStripProfessionalRenderer and not ToolStripExProfessionalRenderer)
            {
                toolStrip.Renderer = ExtendedProfessionalRenderers.GetOrCreateValue(toolStrip);
            }
        }

        internal static void UseExtendedThemeAwareRenderer(this ToolStrip toolStrip)
        {
            if (toolStrip.Renderer is not ToolStripExThemeAwareRenderer)
            {
                toolStrip.Renderer = ExtendedThemeAwareRenderers.GetOrCreateValue(toolStrip);
            }
        }

        internal static IMenuItemBackgroundFilter? GetMenuItemBackgroundFilter(this ToolStrip toolStrip)
        {
            if (MenuItemBackgroundFilters.TryGetValue(toolStrip, out IMenuItemBackgroundFilter filter))
            {
                return filter;
            }

            return null;
        }
    }
}
