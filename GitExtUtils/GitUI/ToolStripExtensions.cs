using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GitExtUtils.GitUI;

namespace GitUI
{
    public static class ToolStripExtensions
    {
        private static ConditionalWeakTable<ToolStrip, IMenuItemBackgroundFilter> MenuItemBackgroundFilters = new();
        private static ConditionalWeakTable<ToolStrip, ToolStripExRenderer> Renderers = new();
        private static ConditionalWeakTable<ToolStrip, ToolStripExThemeAwareRenderer> RenderersThemeAware = new();

        public static void AttachMenuItemBackgroundFilter(this ToolStrip toolStrip, IMenuItemBackgroundFilter? value)
        {
            toolStrip.UseCustomRenderer();
            MenuItemBackgroundFilters.Remove(toolStrip);

            if (value is not null)
            {
                MenuItemBackgroundFilters.Add(toolStrip, value);
            }
        }

        public static void UseCustomRenderer(this ToolStrip toolStrip)
        {
            // use either ToolStripExRenderer or ToolStripExThemeAwareRenderer
            toolStrip.EnableTheming(enable: toolStrip.IsThemingEnabled());
        }

        internal static void EnableTheming(this ToolStrip toolStrip, bool enable)
        {
            if (enable)
            {
                if (toolStrip.Renderer is not ToolStripExThemeAwareRenderer)
                {
                    toolStrip.Renderer = RenderersThemeAware.GetOrCreateValue(toolStrip);
                }
            }
            else
            {
                if (toolStrip.Renderer is not ToolStripExRenderer)
                {
                    toolStrip.Renderer = Renderers.GetOrCreateValue(toolStrip);
                }
            }
        }

        internal static IMenuItemBackgroundFilter? GetMenuItemBackgroundFilter(this ToolStrip toolStrip)
        {
            if (MenuItemBackgroundFilters.TryGetValue(toolStrip, out var filter))
            {
                return filter;
            }

            return null;
        }

        internal static bool IsThemingEnabled(this ToolStrip toolStrip) =>
            toolStrip.Renderer is ToolStripExThemeAwareRenderer;
    }
}
