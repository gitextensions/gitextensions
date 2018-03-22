using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Utils;

namespace GitUI.SpellChecker
{
    /// <summary>
    ///   Wraps API calls for access to missing functionality
    ///   from the System.Windows.Forms text box.
    /// </summary>
    internal static class TextBoxHelper
    {
        private const double AnInch = 14.4;

        internal static int GetTextWidthAtCharIndex(TextBoxBase textBoxBase, int index, int length)
        {
            // TODO!
            if (!(textBoxBase is RichTextBox richTextBox) || !EnvUtils.RunningOnWindows())
            {
                return textBoxBase.Font.Height;
            }

            NativeMethods.CHARRANGE charRange;
            charRange.cpMin = index;
            charRange.cpMax = index + length;

            NativeMethods.RECT rect;
            rect.Top = 0;
            rect.Bottom = (int)AnInch;
            rect.Left = 0;
            rect.Right = (int)(richTextBox.ClientSize.Width * AnInch);

            NativeMethods.RECT rectPage;
            rectPage.Top = 0;
            rectPage.Bottom = (int)AnInch;
            rectPage.Left = 0;
            rectPage.Right = (int)(richTextBox.ClientSize.Width * AnInch);

            var canvas = Graphics.FromHwnd(richTextBox.Handle);
            var canvasHdc = canvas.GetHdc();

            var formatRange = GetFormatRange(charRange, canvasHdc, rect, rectPage);

            NativeMethods.SendMessage(
                richTextBox.Handle,
                NativeMethods.EM_FORMATRANGE,
                IntPtr.Zero,
                ref formatRange);

            canvas.ReleaseHdc(canvasHdc);
            canvas.Dispose();

            return (int)((formatRange.rc.Right - formatRange.rc.Left) / AnInch);
        }

        internal static int GetBaselineOffsetAtCharIndex(TextBoxBase tb, int index)
        {
            if (!(tb is RichTextBox rtb) || !EnvUtils.RunningOnWindows())
            {
                return tb.Font.Height;
            }

            var lineNumber = rtb.GetLineFromCharIndex(index);
            var lineIndex =
                NativeMethods.SendMessageInt(rtb.Handle, NativeMethods.EM_LINEINDEX, new IntPtr(lineNumber), IntPtr.Zero)
                    .ToInt32();
            var lineLength =
                NativeMethods.SendMessageInt(rtb.Handle, NativeMethods.EM_LINELENGTH, new IntPtr(lineNumber),
                                             IntPtr.Zero).ToInt32();

            NativeMethods.CHARRANGE charRange;
            charRange.cpMin = lineIndex;
            charRange.cpMax = lineIndex + lineLength;

            NativeMethods.RECT rect;
            rect.Top = 0;
            rect.Bottom = (int)AnInch;
            rect.Left = 0;
            rect.Right = 10000000; ////(int)(rtb.Width * anInch + 20);

            NativeMethods.RECT rectPage;
            rectPage.Top = 0;
            rectPage.Bottom = (int)AnInch;
            rectPage.Left = 0;
            rectPage.Right = 10000000; ////(int)(rtb.Width * anInch + 20);

            var canvas = Graphics.FromHwnd(rtb.Handle);
            var canvasHdc = canvas.GetHdc();

            var formatRange = GetFormatRange(charRange, canvasHdc, rect, rectPage);

            NativeMethods.SendMessage(rtb.Handle, NativeMethods.EM_FORMATRANGE, IntPtr.Zero, ref formatRange);

            canvas.ReleaseHdc(canvasHdc);
            canvas.Dispose();

            return (int)((formatRange.rc.Bottom - formatRange.rc.Top) / AnInch);
        }

        private static NativeMethods.FORMATRANGE GetFormatRange(NativeMethods.CHARRANGE charRange, IntPtr canvasHdc,
                                                                NativeMethods.RECT rect, NativeMethods.RECT rectPage)
        {
            NativeMethods.FORMATRANGE formatRange;
            formatRange.chrg = charRange;
            formatRange.hdc = canvasHdc;
            formatRange.hdcTarget = canvasHdc;
            formatRange.rc = rect;
            formatRange.rcPage = rectPage;
            return formatRange;
        }

        /// <summary>
        ///   Attempts to make the caret visible in a TextBox control.
        ///   This will not always succeed since the TextBox control
        ///   appears to destroy its caret fairly frequently.
        /// </summary>
        /// <param name = "textBox">The text box to show the caret in.</param>
        internal static void ShowCaret(TextBox textBox)
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return;
            }

            var ret = false;
            var iter = 0;
            while (!ret && iter < 10)
            {
                ret = NativeMethods.ShowCaretAPI(textBox.Handle);
                iter++;
            }
        }

        /// <summary>
        ///   Returns the index of the character under the specified
        ///   point in the control, or the nearest character if there
        ///   is no character under the point.
        /// </summary>
        /// <param name = "textBox">The text box control to check.</param>
        /// <param name = "point">The point to find the character for,
        ///   specified relative to the client area of the text box.</param>
        private static int CharFromPos(TextBoxBase textBox, Point point)
        {
            unchecked
            {
                // Convert the point into a DWord with horizontal position
                // in the loword and vertical position in the hiword:
                var xy = (point.X & 0xFFFF) + ((point.Y & 0xFFFF) << 16);

                // Get the position from the text box.
                var res =
                    NativeMethods.SendMessageInt(
                        textBox.Handle,
                        NativeMethods.EM_CHARFROMPOS,
                        IntPtr.Zero,
                        new IntPtr(xy)).ToInt32();

                // the Platform SDK appears to be incorrect on this matter.
                // the hiword is the line number and the loword is the index
                // of the character on this line
                var lineNumber = (res & 0xFFFF) >> 16;
                var charIndex = res & 0xFFFF;

                // Find the index of the first character on the line within
                // the control:
                var lineStartIndex =
                    NativeMethods.SendMessageInt(
                        textBox.Handle,
                        NativeMethods.EM_LINEINDEX,
                        new IntPtr(lineNumber),
                        IntPtr.Zero).ToInt32();

                // Return the combined index:
                return lineStartIndex + charIndex;
            }
        }

        /// <summary>
        ///   Returns the position of the specified character
        /// </summary>
        /// <param name = "textBoxBase">The text box to find the character in.</param>
        /// <param name = "charIndex">The index of the character whose
        ///   position needs to be found.</param>
        /// <returns>The position of the character relative to the client
        ///   area of the control.</returns>
        private static Point PosFromChar(TextBoxBase textBoxBase, int charIndex)
        {
            var xy =
                NativeMethods.SendMessageInt(
                    textBoxBase.Handle,
                    NativeMethods.EM_POSFROMCHAR,
                    new IntPtr(charIndex),
                    IntPtr.Zero).ToInt32();
            return new Point(xy);
        }

        private static int GetFirstVisibleLine(TextBoxBase txt)
        {
            return
                NativeMethods.SendMessageInt(
                    txt.Handle,
                    NativeMethods.EM_GETFIRSTVISIBLELINE,
                    IntPtr.Zero,
                    IntPtr.Zero).ToInt32();
        }

        private static int GetLineIndex(TextBoxBase txt, int line)
        {
            return NativeMethods.SendMessageInt(txt.Handle, 0xbb, new IntPtr(line), IntPtr.Zero).ToInt32();
        }
    }
}