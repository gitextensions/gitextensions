﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

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

            switch (m.Msg)
            {
                case NativeMethods.WM_NCPAINT:
                case NativeMethods.WM_PAINT:
                    var penColor = _borderDefaultColor;

                    if (Focused)
                    {
                        penColor = _borderFocusedColor;
                    }
                    else if (_hovered)
                    {
                        penColor = _borderHoveredColor;
                    }

                    var windowDC = PInvoke.GetWindowDC((HWND)Handle);
                    {
                        using var graphics = Graphics.FromHdc(windowDC);
                        using Pen pen = new(penColor);

                        ControlPaint.DrawBorder(graphics, ClientRectangle, penColor, ButtonBorderStyle.Solid);
                    }

                    break;

                case NativeMethods.WM_NCMOUSEHOVER when !_hovered:
                case NativeMethods.WM_MOUSEHOVER when !_hovered:
                    _hovered = true;

                    Invalidate();

                    break;
                case NativeMethods.WM_NCMOUSELEAVE:
                case NativeMethods.WM_MOUSELEAVE:
                    _hovered = false;

                    Invalidate();

                    break;
            }
        }
    }
}
