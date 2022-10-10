using System.Runtime.InteropServices;
using EasyHook;
using GitExtUtils.GitUI.Theming;
using GitUI.UserControls;
using Microsoft;

namespace GitUI.Theming
{
    internal static class Win32ThemeHooks
    {
        private static Theme? _theme;

        private static GetSysColorDelegate? _getSysColorBypass;
        private static GetSysColorBrushDelegate? _getSysColorBrushBypass;
        private static DrawThemeBackgroundDelegate? _drawThemeBackgroundBypass;
        private static DrawThemeBackgroundExDelegate? _drawThemeBackgroundExBypass;
        private static GetThemeColorDelegate? _getThemeColorBypass;
        private static DrawThemeTextDelegate? _drawThemeTextBypass;
        private static DrawThemeTextExDelegate? _drawThemeTextExBypass;
        private static CreateWindowExDelegate? _createWindowExBypass;

        private static LocalHook? _getSysColorHook;
        private static LocalHook? _getSysColorBrushHook;
        private static LocalHook? _drawThemeBackgroundHook;
        private static LocalHook? _drawThemeBackgroundExHook;
        private static LocalHook? _getThemeColorHook;
        private static LocalHook? _drawThemeTextHook;
        private static LocalHook? _drawThemeTextExHook;
        private static LocalHook? _createWindowExHook;

        private static ThemeRenderer[]? _renderers;
        private static SystemDialogDetector? _systemDialogDetector;

        public static event Action<IntPtr>? WindowCreated;

        internal static ThemeSettings ThemeSettings { private get; set; } = ThemeSettings.Default;

        private static readonly HashSet<NativeListView> InitializingListViews = new();
        private static ScrollBarRenderer? _scrollBarRenderer;
        private static readonly SystemBrushesCache _systemBrushesCache = new();
        private static ListViewRenderer? _listViewRenderer;
        private static HeaderRenderer? _headerRenderer;
        private static TreeViewRenderer? _treeViewRenderer;

        private static bool BypassThemeRenderers =>
            ThemeSettings.UseSystemVisualStyle || BypassAnyHook;

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

            (_drawThemeBackgroundExHook, _drawThemeBackgroundExBypass) =
                InstallHook<DrawThemeBackgroundExDelegate>(
                    "uxtheme.dll",
                    "DrawThemeBackgroundEx",
                    DrawThemeBackgroundExHook);

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

            NativeListView.BeginCreateHandle += ListView_BeginCreateHandle;
            NativeListView.EndCreateHandle += ListView_EndCreateHandle;

            CreateRenderers();
        }

        private static void CreateRenderers()
        {
            _renderers = new ThemeRenderer[]
            {
                _scrollBarRenderer = new ScrollBarRenderer(),
                _listViewRenderer = new ListViewRenderer(),
                _headerRenderer = new HeaderRenderer(),
                _treeViewRenderer = new TreeViewRenderer(),
                new EditRenderer(),
                new SpinRenderer(),
                new ComboBoxRenderer(),
                new ButtonRenderer(),
                new TooltipRenderer(),
            };

            LoadThemeData();
        }

        public static void LoadThemeData()
        {
            Validates.NotNull(_renderers);
            Validates.NotNull(_scrollBarRenderer);
            Validates.NotNull(_headerRenderer);
            Validates.NotNull(_listViewRenderer);
            Validates.NotNull(_treeViewRenderer);

            foreach (ThemeRenderer renderer in _renderers)
            {
                renderer.AddThemeData(IntPtr.Zero);
            }

            var editorHandle = new ICSharpCode.TextEditor.TextEditorControl().Handle;
            var listViewHandle = new NativeListView().Handle;
            var treeViewHandle = new NativeTreeView().Handle;

            _scrollBarRenderer.AddThemeData(editorHandle);
            _scrollBarRenderer.AddThemeData(listViewHandle);
            _headerRenderer.AddThemeData(listViewHandle);
            _listViewRenderer.AddThemeData(listViewHandle);
            _treeViewRenderer.AddThemeData(treeViewHandle);
        }

        public static void Uninstall()
        {
            _getSysColorHook?.Dispose();
            _getSysColorBrushHook?.Dispose();
            _drawThemeBackgroundHook?.Dispose();
            _drawThemeBackgroundExHook?.Dispose();
            _getThemeColorHook?.Dispose();
            _drawThemeTextHook?.Dispose();
            _drawThemeTextExHook?.Dispose();
            _createWindowExHook?.Dispose();
            _systemBrushesCache.Dispose();

            if (_renderers is not null)
            {
                foreach (var renderer in _renderers)
                {
                    renderer.Dispose();
                }
            }

            NativeListView.BeginCreateHandle -= ListView_BeginCreateHandle;
            NativeListView.EndCreateHandle -= ListView_EndCreateHandle;

            InitializingListViews.Clear();
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
                var color = _theme!.GetColor(name);
                if (color != Color.Empty)
                {
                    return ColorTranslator.ToWin32(color);
                }
            }

            return _getSysColorBypass!(nindex);
        }

        private static IntPtr GetSysColorBrushHook(int nindex)
        {
            if (!BypassAnyHook)
            {
                var name = Win32ColorTranslator.GetKnownColor(nindex);
                var color = _theme!.GetColor(name);
                if (color != Color.Empty)
                {
                    int colorref = ColorTranslator.ToWin32(color);
                    return _systemBrushesCache.GetBrush(colorref);
                }
            }

            return _getSysColorBrushBypass!(nindex);
        }

        private static int DrawThemeBackgroundHook(
            IntPtr htheme, IntPtr hdc,
            int partid, int stateid,
            NativeMethods.RECTCLS prect, NativeMethods.RECTCLS pcliprect)
        {
            if (!BypassThemeRenderers)
            {
                var renderer = _renderers.FirstOrDefault(_ => _.Supports(htheme));
                if (renderer?.RenderBackground(hdc, partid, stateid, prect, pcliprect) == ThemeRenderer.Handled)
                {
                    return ThemeRenderer.Handled;
                }
            }

            return _drawThemeBackgroundBypass!(htheme, hdc, partid, stateid, prect, pcliprect);
        }

        private static int DrawThemeBackgroundExHook(
            IntPtr htheme, IntPtr hdc,
            int partid, int stateid,
            NativeMethods.RECTCLS prect, ref NativeMethods.DTBGOPTS poptions)
        {
            if (!BypassThemeRenderers)
            {
                var renderer = _renderers.FirstOrDefault(_ => _.Supports(htheme));
                if (renderer is null && InitializingListViews.Any(_ => _.CheckBoxes))
                {
                    renderer = _renderers.OfType<ListViewRenderer>().SingleOrDefault();
                }

                if (renderer?.RenderBackgroundEx(htheme, hdc, partid, stateid, prect, ref poptions) == ThemeRenderer.Handled)
                {
                    return ThemeRenderer.Handled;
                }
            }

            return _drawThemeBackgroundExBypass!(htheme, hdc, partid, stateid, prect, ref poptions);
        }

        private static int GetThemeColorHook(IntPtr htheme, int ipartid, int istateid, int ipropid,
            out int pcolor)
        {
            if (!BypassThemeRenderers)
            {
                var renderer = _renderers.FirstOrDefault(_ => _.Supports(htheme));
                if (renderer is not null && renderer.GetThemeColor(ipartid, istateid, ipropid, out pcolor) == ThemeRenderer.Handled)
                {
                    return ThemeRenderer.Handled;
                }
            }

            return _getThemeColorBypass!(htheme, ipartid, istateid, ipropid, out pcolor);
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
                if (renderer is not null && renderer.ForceUseRenderTextEx)
                {
                    NativeMethods.DTTOPTS poptions = new()
                    {
                        dwSize = Marshal.SizeOf<NativeMethods.DTTOPTS>()
                    };

                    return _drawThemeTextExBypass!(
                        htheme, hdc,
                        partid, stateid,
                        psztext, cchtext, dwtextflags,
                        prect, ref poptions);
                }
            }

            return _drawThemeTextBypass!(
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
                if (renderer is not null && renderer.RenderTextEx(
                    htheme, hdc,
                    partid, stateid,
                    psztext, cchtext,
                    dwtextflags,
                    prect, ref poptions) == ThemeRenderer.Handled)
                {
                    return ThemeRenderer.Handled;
                }
            }

            return _drawThemeTextExBypass!(
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
            var hwnd = _createWindowExBypass!(
                dwexstyle, lpclassname, lpwindowname, dwstyle,
                x, y, nwidth, nheight,
                hwndparent, hmenu, hinstance, lpparam);

            WindowCreated?.Invoke(hwnd);
            return hwnd;
        }

        private static void ListView_BeginCreateHandle(object sender, EventArgs args)
        {
            InitializingListViews.Add((NativeListView)sender);
        }

        private static void ListView_EndCreateHandle(object sender, EventArgs args)
        {
            InitializingListViews.Remove((NativeListView)sender);
        }
    }
}
