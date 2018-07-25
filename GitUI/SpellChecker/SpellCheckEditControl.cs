using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;

namespace GitUI.SpellChecker
{
    public sealed class SpellCheckEditControl : NativeWindow, IDisposable
    {
        public bool IsImeStartingComposition { get; private set; }

        private readonly RichTextBox _richTextBox;
        public List<TextPos> IllFormedLines { get; } = new List<TextPos>();
        public List<TextPos> Lines { get; } = new List<TextPos>();
        private Bitmap _bitmap;
        private Graphics _bufferGraphics;
        private int _lineHeight;
        private Graphics _textBoxGraphics;

        public SpellCheckEditControl(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;

            // Start receiving messages
            AssignHandle(richTextBox.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_IME_STARTCOMPOSITION = 0x010D;
            const int WM_IME_ENDCOMPOSITION = 0x010E;

            switch (m.Msg)
            {
                case 15: // this is the WM_PAINT message
                    // invalidate the TextBox so that it gets refreshed properly
                    _richTextBox.Invalidate();

                    // call the default win32 Paint method for the TextBox first
                    base.WndProc(ref m);

                    // now use our code to draw extra stuff over the TextBox
                    CustomPaint();

                    break;
                case WM_IME_STARTCOMPOSITION:
                    IsImeStartingComposition = true;
                    base.WndProc(ref m);
                    break;
                case WM_IME_ENDCOMPOSITION:
                    base.WndProc(ref m);
                    IsImeStartingComposition = false;
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void CustomPaint()
        {
            if (IsImeStartingComposition)
            {
                return;
            }

            if (_bitmap == null || (_bitmap.Width != _richTextBox.Width || _bitmap.Height != _richTextBox.Height))
            {
                _bitmap = new Bitmap(_richTextBox.Width, _richTextBox.Height, PixelFormat.Format32bppPArgb);
                _bufferGraphics = Graphics.FromImage(_bitmap);
                _bufferGraphics.Clip = new Region(_richTextBox.ClientRectangle);
                _textBoxGraphics = Graphics.FromHwnd(_richTextBox.Handle);
            }

            // clear the graphics buffer
            _bufferGraphics.Clear(Color.Transparent);

            // * Here’s where the magic happens

            // Mark ill formed parts of commit message
            DrawLines(IllFormedLines, DrawType.Mark);

            // Mark first line if it is blank
            var lh = LineHeight();
            var ypos = _richTextBox.GetPositionFromCharIndex(0).Y;
            if (_richTextBox.Text.Length > 1 &&

                // check for textBox.Text.Length>1 instead of textBox.Text.Length!=0 because there might be only a \n
                _richTextBox.Lines.Length > 0 && _richTextBox.Lines[0].Length == 0
                && ypos >= -lh && AppSettings.MarkIllFormedLinesInCommitMsg)
            {
                DrawMark(new Point(0, lh + ypos), new Point(_richTextBox.Width - 3, lh + ypos));
            }

            // Mark misspelled words
            DrawLines(Lines, DrawType.Wave);

            // Now we just draw our internal buffer on top of the TextBox.
            // Everything should be at the right place.
            _textBoxGraphics.DrawImageUnscaled(_bitmap, 0, 0);
        }

        private void DrawLines(IEnumerable<TextPos> list, DrawType type)
        {
            foreach (var textPos in list)
            {
                var start = _richTextBox.GetPositionFromCharIndex(textPos.Start);
                var end = _richTextBox.GetPositionFromCharIndex(textPos.End);

                // The position above now points to the top left corner of the character.
                // We need to account for the character height so the underlines go
                // to the right place.
                end.X += 1;
                start.Y += TextBoxHelper.GetBaselineOffsetAtCharIndex(_richTextBox, textPos.Start);
                end.Y += TextBoxHelper.GetBaselineOffsetAtCharIndex(_richTextBox, textPos.End);

                if (start.X == -1 || end.X == -1)
                {
                    continue;
                }

                // Draw the wavy underline/mark
                if (start.Y < end.Y)
                {
                    switch (type)
                    {
                        case DrawType.Wave:
                            DrawWave(start, new Point(_richTextBox.Width - 3, start.Y));
                            DrawWave(new Point(3, end.Y), end);
                            break;
                        case DrawType.Mark:
                            DrawMark(start, new Point(_richTextBox.Width - 3, start.Y));
                            DrawMark(new Point(0, end.Y), end);
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case DrawType.Wave:
                            DrawWave(start, end);
                            break;
                        case DrawType.Mark:
                            DrawMark(start, end);
                            break;
                    }
                }
            }
        }

        private void DrawWave(Point start, Point end)
        {
            using (var pen = new Pen(Color.Red, DpiUtil.ScaleX))
            {
                var waveWidth = DpiUtil.Scale(4);
                var waveHalfWidth = waveWidth >> 1;
                if ((end.X - start.X) > waveWidth)
                {
                    var pl = new List<Point>();
                    for (var i = start.X; i <= (end.X - waveHalfWidth); i += waveWidth)
                    {
                        pl.Add(new Point(i, start.Y));
                        pl.Add(new Point(i + waveHalfWidth, start.Y + waveHalfWidth));
                    }

                    var p = pl.ToArray();
                    _bufferGraphics.DrawLines(pen, p);
                }
                else
                {
                    _bufferGraphics.DrawLine(pen, start, end);
                }
            }
        }

        private void DrawMark(Point start, Point end)
        {
            var col = Color.FromArgb(120, 255, 255, 0);
            var lineHeight = LineHeight();
            using (var pen = new Pen(col, lineHeight))
            {
                start.Offset(0, -lineHeight / 2);
                end.Offset(0, -lineHeight / 2);
                _bufferGraphics.DrawLine(pen, start, end);
            }
        }

        private int LineHeight()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return 12;
            }

            if (_lineHeight == 0)
            {
                if (_richTextBox.Lines.Any(line => line.Length != 0))
                {
                    _lineHeight = TextBoxHelper.GetBaselineOffsetAtCharIndex(_richTextBox, 0);
                }
            }

            return _lineHeight == 0 ? 12 : _lineHeight;
        }

        public void Dispose()
        {
            ReleaseHandle();

            _bitmap?.Dispose();
            _bufferGraphics?.Dispose();
            _textBoxGraphics?.Dispose();
        }

        private enum DrawType
        {
            Wave,
            Mark
        }
    }
}