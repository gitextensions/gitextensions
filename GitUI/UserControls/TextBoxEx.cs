using System.Drawing;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace GitUI.UserControls
{
    internal sealed class TextBoxEx : TextBox
    {
        private bool _hovered = false;

        private Color _borderDefaultColor = SystemColors.WindowFrame;
        private Color _borderHoveredColor = SystemColors.Highlight;
        private Color _borderFocusedColor = SystemColors.HotTrack;

        public TextBoxEx()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        public Color BorderDefaultColor
        {
            get => _borderDefaultColor;
            set
            {
                _borderDefaultColor = value;
                Invalidate();
            }
        }

        public Color BorderHoveredColor
        {
            get => _borderHoveredColor;
            set
            {
                _borderHoveredColor = value;
                Invalidate();
            }
        }

        public Color BorderFocusedColor
        {
            get => _borderFocusedColor;
            set
            {
                _borderFocusedColor = value;
                Invalidate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (BorderStyle != BorderStyle.FixedSingle)
            {
                return;
            }

            switch ((uint)m.Msg)
            {
                case Constants.WM_NCPAINT:
                case Constants.WM_PAINT:
                    Color penColor = Focused ? _borderFocusedColor
                        : _hovered ? _borderHoveredColor
                        : _borderDefaultColor;

                    HDC windowDC = PInvoke.GetWindowDC((HWND)Handle);
                    try
                    {
                        using Graphics graphics = Graphics.FromHdc(windowDC);
                        using Pen pen = new(penColor);

                        ControlPaint.DrawBorder(graphics, ClientRectangle, penColor, ButtonBorderStyle.Solid);
                    }
                    finally
                    {
                        PInvoke.ReleaseDC((HWND)Handle, windowDC);
                    }

                    break;

                case Constants.WM_NCMOUSEHOVER when !_hovered:
                case Constants.WM_MOUSEHOVER when !_hovered:
                    _hovered = true;

                    Invalidate();

                    break;
                case Constants.WM_NCMOUSELEAVE:
                case Constants.WM_MOUSELEAVE:
                    _hovered = false;

                    Invalidate();

                    break;
            }
        }
    }
}
