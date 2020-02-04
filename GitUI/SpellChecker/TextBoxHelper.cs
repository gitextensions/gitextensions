using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Utils;
using GitExtUtils.GitUI;

namespace GitUI.SpellChecker
{
    /// <summary>
    ///   Wraps API calls for access to missing functionality
    ///   from the System.Windows.Forms text box.
    /// </summary>
    internal static class TextBoxHelper
    {
        private static readonly double AnInch = 14.4 / DpiUtil.ScaleX;

        internal static int GetBaselineOffsetAtCharIndex(TextBoxBase tb, int index)
        {
            if (!(tb is RichTextBox rtb) || !EnvUtils.RunningOnWindows())
            {
                return tb.Font.Height;
            }

            var lineNumber = rtb.GetLineFromCharIndex(index);
            var lineIndex = NativeMethods.SendMessageW(rtb.Handle, NativeMethods.EM_LINEINDEX, (IntPtr)lineNumber, IntPtr.Zero).ToInt32();
            var lineLength = NativeMethods.SendMessageW(rtb.Handle, NativeMethods.EM_LINELENGTH, (IntPtr)index, IntPtr.Zero).ToInt32();

            var charRange = new NativeMethods.CHARRANGE
            {
                cpMin = lineIndex,
                cpMax = lineIndex + lineLength
            };

            var rect = new NativeMethods.RECT
            {
                top = 0,
                bottom = (int)AnInch,
                left = 0,
                right = 10000000 ////(int)(rtb.Width * anInch + 20);
            };

            var rectPage = new NativeMethods.RECT
            {
                top = 0,
                bottom = (int)AnInch,
                left = 0,
                right = 10000000 ////(int)(rtb.Width * anInch + 20);
            };

            var canvas = Graphics.FromHwnd(rtb.Handle);
            var canvasHdc = canvas.GetHdc();

            var formatRange = new NativeMethods.FORMATRANGE
            {
                chrg = charRange,
                hdc = canvasHdc,
                hdcTarget = canvasHdc,
                rc = rect,
                rcPage = rectPage
            };

            NativeMethods.SendMessageW(rtb.Handle, NativeMethods.EM_FORMATRANGE, IntPtr.Zero, ref formatRange);

            canvas.ReleaseHdc(canvasHdc);
            canvas.Dispose();

            return (int)((formatRange.rc.bottom - formatRange.rc.top) / AnInch);
        }
    }
}
