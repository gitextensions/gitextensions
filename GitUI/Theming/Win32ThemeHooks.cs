using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using EasyHook;
using GitExtUtils.GitUI.Theming;
using GitUI.UserControls;

namespace GitUI.Theming
{
    internal static class Win32ThemeHooks
    {
        private static Theme _theme;

        private static GetSysColorDelegate _getSysColorBypass;
        private static GetSysColorBrushDelegate _getSysColorBrushBypass;
        private static DrawThemeBackgroundDelegate _drawThemeBackgroundBypass;
        private static GetThemeColorDelegate _getThemeColorBypass;
        private static DrawThemeTextDelegate _drawThemeTextBypass;
        private static DrawThemeTextExDelegate _drawThemeTextExBypass;
        private static CreateWindowExDelegate _createWindowExBypass;

        private static LocalHook _getSysColorHook;
        private static LocalHook _getSysColorBrushHook;
        private static LocalHook _drawThemeBackgroundHook;
        private static LocalHook _getThemeColorHook;
        private static LocalHook _drawThemeTextHook;
        private static LocalHook _drawThemeTextExHook;
        private static LocalHook _createWindowExHook;

        private static ThemeRenderer[] _renderers;
        private static SystemDialogDetector _systemDialogDetector;

        public static event Action<IntPtr> WindowCreated;

        private static bool BypassThemeRenderers =>
            ThemeModule.Controller.UseSystemVisualStyle || BypassAnyHook;

        private static bool BypassAnyHook =>
            _systemDialogDetector?.IsSystemDialogOpen == true;

        public static void InstallHooks(Theme theme, SystemDialogDetector systemDialogDetector)
        {
            _theme = theme;
            _systemDialogDetector = systemDialogDetector;

            (_getSysColorBrushHook, _getSysColorBrushBypass) = InstallHook<GetSysColorBrushDelegate>(
                "user32.dll",
                "GetSysColorBrush",
                GetSysColorBrushHook);

            (_getSysColorHook, _getSysColorBypass) = InstallHook<GetSysColorDelegate>(
                "user32.dll",
                "GetSysColor",
                GetSysColorHook);

            (_drawThemeBackgroundHook, _drawThemeBackgroundBypass) =
                InstallHook<DrawThemeBackgroundDelegate>(
                    "uxtheme.dll",
                    "DrawThemeBackground",
                    DrawThemeBackgroundHook);

            (_getThemeColorHook, _getThemeColorBypass) =
                InstallHook<GetThemeColorDelegate>(
                    "uxtheme.dll",
                    "GetThemeColor",
                    GetThemeColorHook);

            (_drawThemeTextHook, _drawThemeTextBypass) =
                InstallHook<DrawThemeTextDelegate>(
                    "uxtheme.dll",
                    "DrawThemeText",
                    DrawThemeTextHook);

            (_drawThemeTextExHook, _drawThemeTextExBypass) =
                InstallHook<DrawThemeTextExDelegate>(
                    "uxtheme.dll",
                    "DrawThemeTextEx",
                    DrawThemeTextExHook);

            (_createWindowExHook, _createWindowExBypass) =
                InstallHook<CreateWindowExDelegate>(
                    "user32.dll",
                    "CreateWindowExW",
                    CreateWindowExHook);

            CreateRenderers();
        }

        private static void CreateRenderers()
        {
            ScrollBarRenderer scrollBarRenderer;
            ListViewRenderer listViewRenderer;
            HeaderRenderer headerRenderer;
            TreeViewRenderer treeViewRenderer;

            _renderers = new ThemeRenderer[]
            {
                scrollBarRenderer = new ScrollBarRenderer(),
                listViewRenderer = new ListViewRenderer(),
                headerRenderer = new HeaderRenderer(),
                treeViewRenderer = new TreeViewRenderer(),
                new EditRenderer(),
                new SpinRenderer(),
                new ComboBoxRenderer(),
                new ButtonRenderer(),
                new TooltipRenderer(),
            };

            var editorHandle = new ICSharpCode.TextEditor.TextEditorControl().Handle;
            var listViewHandle = new NativeListView().Handle;
            var treeViewHandle = new NativeTreeView().Handle;
            scrollBarRenderer.AddThemeData(editorHandle);
            scrollBarRenderer.AddThemeData(listViewHandle);
            headerRenderer.AddThemeData(listViewHandle);
            listViewRenderer.AddThemeData(listViewHandle);
            treeViewRenderer.AddThemeData(treeViewHandle);
        }

        public static void Uninstall()
        {
            _getSysColorHook?.Dispose();
            _getSysColorBrushHook?.Dispose();
            _drawThemeBackgroundHook?.Dispose();
            _getThemeColorHook?.Dispose();
            _drawThemeTextHook?.Dispose();
            _drawThemeTextExHook?.Dispose();
            _createWindowExHook?.Dispose();

            if (_renderers != null)
            {
                foreach (var renderer in _renderers)
                {
                    renderer.Dispose();
                }
            }
        }

        private static (LocalHook hook, TDelegate original) InstallHook<TDelegate>(string dll, string method,
            TDelegate hookImpl)
            where TDelegate : Delegate
        {
            var addr = LocalHook.GetProcAddress(dll, method);
            var original = Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
            var hook = LocalHook.Create(addr, hookImpl, null);

            try
            {
                hook.ThreadACL.SetExclusiveACL(new int[0]);
            }
            catch
            {
                hook.Dispose();
                throw;
            }

            return (hook, original);
        }

        private static int GetSysColorHook(int nindex)
        {
            if (!BypassAnyHook)
            {
                var name = Win32ColorTranslator.GetKnownColor(nindex);
                var color = _theme.GetColor(name);
                if (color != Color.Empty)
                {
                    return ColorTranslator.ToWin32(color);
                }
            }

            return _getSysColorBypass(nindex);
        }

        private static IntPtr GetSysColorBrushHook(int nindex)
        {
            if (!BypassAnyHook)
            {
                var name = Win32ColorTranslator.GetKnownColor(nindex);
                var color = _theme.GetColor(name);
                if (color != Color.Empty)
                {
                    int colorref = ColorTranslator.ToWin32(color);
                    var hbrush = NativeMethods.CreateSolidBrush(colorref);
                    return hbrush;
                }
            }

            return _getSysColorBrushBypass(nindex);
        }

        private static int DrawThemeBackgroundHook(
            IntPtr htheme, IntPtr hdc,
            int partid, int stateid,
            ref NativeMethods.RECT prect, ref NativeMethods.RECT pcliprect)
        {
            if (!BypassThemeRenderers)
            {
                var renderer = _renderers.FirstOrDefault(_ => _.Supports(htheme));
                if (renderer?.RenderBackground(hdc, partid, stateid, prect, pcliprect) == 0)
                {
                    return 0;
                }
            }

            return _drawThemeBackgroundBypass(htheme, hdc, partid, stateid, ref prect, ref pcliprect);
        }

        private static int GetThemeColorHook(IntPtr htheme, int ipartid, int istateid, int ipropid,
            out int pcolor)
        {
            if (!BypassThemeRenderers)
            {
                var renderer = _renderers.FirstOrDefault(_ => _.Supports(htheme));
                if (renderer != null && renderer.GetThemeColor(ipartid, istateid, ipropid, out pcolor) == 0)
                {
                    return 0;
                }
            }

            int result = _getThemeColorBypass(htheme, ipartid, istateid, ipropid, out pcolor);
            return result;
        }

        private static int DrawThemeTextHook(
            IntPtr htheme, IntPtr hdc,
            int partid, int stateid,
            string psztext, int cchtext,
            NativeMethods.DT dwtextflags, int dwtextflags2, IntPtr prect)
        {
            if (!BypassThemeRenderers)
            {
                var renderer = _renderers.FirstOrDefault(_ => _.Supports(htheme));
                if (renderer != null && renderer.ForceUseRenderTextEx)
                {
                    NativeMethods.DTTOPTS poptions = new NativeMethods.DTTOPTS
                    {
                        dwSize = Marshal.SizeOf<NativeMethods.DTTOPTS>()
                    };

                    return _drawThemeTextExBypass(
                        htheme, hdc,
                        partid, stateid,
                        psztext, cchtext, dwtextflags,
                        prect, ref poptions);
                }
            }

            return _drawThemeTextBypass(
                htheme, hdc,
                partid, stateid,
                psztext, cchtext,
                dwtextflags, dwtextflags2, prect);
        }

        private static int DrawThemeTextExHook(
            IntPtr htheme, IntPtr hdc,
            int partid, int stateid,
            string psztext, int cchtext,
            NativeMethods.DT dwtextflags,
            IntPtr prect, ref NativeMethods.DTTOPTS poptions)
        {
            if (!BypassThemeRenderers)
            {
                var renderer = _renderers.FirstOrDefault(_ => _.Supports(htheme));
                if (renderer != null && renderer.RenderTextEx(
                    htheme, hdc,
                    partid, stateid,
                    psztext, cchtext,
                    dwtextflags,
                    prect, ref poptions) == 0)
                {
                    return 0;
                }
            }

            return _drawThemeTextExBypass(
                htheme, hdc,
                partid, stateid,
                psztext, cchtext,
                dwtextflags,
                prect, ref poptions);
        }

        private static IntPtr CreateWindowExHook(
            int dwexstyle, IntPtr lpclassname, IntPtr lpwindowname, int dwstyle,
            int x, int y, int nwidth, int nheight,
            IntPtr hwndparent, IntPtr hmenu, IntPtr hinstance, IntPtr lpparam)
        {
            var hwnd = _createWindowExBypass(
                dwexstyle, lpclassname, lpwindowname, dwstyle,
                x, y, nwidth, nheight,
                hwndparent, hmenu, hinstance, lpparam);

            WindowCreated?.Invoke(hwnd);
            return hwnd;
        }
    }
}
