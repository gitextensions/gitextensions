using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GitUI.Theming
{
    /// <summary>
    /// Defines customization of win32 api theming methods, see
    /// https://docs.microsoft.com/en-us/windows/win32/api/uxtheme/ and
    /// https://docs.microsoft.com/en-us/windows/win32/controls/parts-and-states
    /// </summary>
    internal abstract class ThemeRenderer : IDisposable
    {
        /// <summary>
        /// Result code indicating theming method's task was not achieved.
        /// In this case original win32 theming api method is applied by <see cref="Win32ThemeHooks"/>
        /// </summary>
        protected const int Unhandled = 1;

        /// <summary>
        /// Result code indicating successful completion of theming method's task
        /// </summary>
        protected const int Handled = 0;

        private readonly HashSet<IntPtr> _themeDataHandles = new HashSet<IntPtr>();

        protected ThemeRenderer()
        {
            AddThemeData(IntPtr.Zero);
        }

        protected abstract string Clsid { get; }

        public virtual bool ForceUseRenderTextEx => false;

        public virtual int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect,
            NativeMethods.RECT pcliprect)
        {
            return Unhandled;
        }

        public virtual int RenderTextEx(IntPtr htheme,
            IntPtr hdc,
            int partid, int stateid,
            string psztext, int cchtext,
            NativeMethods.DT dwtextflags,
            IntPtr prect, ref NativeMethods.DTTOPTS poptions)
        {
            return Unhandled;
        }

        public virtual int GetThemeColor(int ipartid, int istateid, int ipropid, out int pcolor)
        {
            pcolor = 0;
            return Unhandled;
        }

        /// <summary>
        /// By using this method we find which theme data handle corresponds to a given CLSID e.g.
        /// "SCROLLBAR". The result depends on window class, e.g. ListView or NativeListView will
        /// have different theme data.
        /// </summary>
        /// <param name="hwnd">win32 window handle</param>
        public void AddThemeData(IntPtr hwnd)
        {
            var htheme = NativeMethods.OpenThemeData(hwnd, Clsid);
            _themeDataHandles.Add(htheme);
        }

        public bool Supports(IntPtr htheme) =>
            _themeDataHandles.Contains(htheme);

        public void Dispose()
        {
            foreach (var htheme in _themeDataHandles)
            {
                NativeMethods.CloseThemeData(htheme);
            }
        }

        protected Context CreateRenderContext(IntPtr hdc, NativeMethods.RECT clip) =>
            new Context(hdc, clip);

        protected class Context : IDisposable
        {
            private readonly IntPtr _hdc;
            private readonly NativeMethods.RECT _clip;
            private readonly Lazy<Graphics> _graphicsLazy;

            private bool _clipChanged;
            private Region _originalClip;

            public Graphics Graphics => _graphicsLazy.Value;

            public Context(IntPtr hdc, NativeMethods.RECT clip)
            {
                _hdc = hdc;
                _clip = clip;
                _graphicsLazy = new Lazy<Graphics>(CreateGraphics);
            }

            private Graphics CreateGraphics()
            {
                var graphics = Graphics.FromHdcInternal(_hdc);
                _originalClip = graphics.Clip;
                graphics.SetClip((Rectangle)_clip);
                _clipChanged = true;

                return graphics;
            }

            public HighQualityScope HighQuality() =>
                new HighQualityScope(Graphics);

            public void Dispose()
            {
                if (!_graphicsLazy.IsValueCreated)
                {
                    return;
                }

                if (_clipChanged)
                {
                    Graphics.Clip = _originalClip;
                }

                Graphics.Dispose();
            }
        }

        protected class HighQualityScope : IDisposable
        {
            private readonly Graphics _g;
            private readonly SmoothingMode _smoothing;

            public HighQualityScope(Graphics g)
            {
                _g = g;
                _smoothing = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.HighQuality;
            }

            public void Dispose()
            {
                _g.SmoothingMode = _smoothing;
            }
        }
    }
}
