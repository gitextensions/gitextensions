using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Controls;

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
            var lineIndex = PInvoke.SendMessage((HWND)rtb.Handle, Constants.EM_LINEINDEX, (nuint)lineNumber, IntPtr.Zero);
            var lineLength = PInvoke.SendMessage((HWND)rtb.Handle, Constants.EM_LINELENGTH, (nuint)index, IntPtr.Zero);

            CHARRANGE charRange = new()
            {
                cpMin = lineIndex,
                cpMax = lineIndex + lineLength
            };

            RECT rect = new()
            {
                top = 0,
                bottom = (int)AnInch,
                left = 0,
                right = 10000000 ////(int)(rtb.Width * anInch + 20);
            };

            RECT rectPage = new()
            {
                top = 0,
                bottom = (int)AnInch,
                left = 0,
                right = 10000000 ////(int)(rtb.Width * anInch + 20);
            };

            var canvas = Graphics.FromHwnd(rtb.Handle);
            var canvasHdc = canvas.GetHdc();

            NativeMethods.FORMATRANGE formatRange = new()
            {
                chrg = charRange,
                hdc = canvasHdc,
                hdcTarget = canvasHdc,
                rc = rect,
                rcPage = rectPage
            };

            PInvoke.SendMessage((HWND)rtb.Handle, Constants.EM_FORMATRANGE, UIntPtr.Zero, ref formatRange);

            canvas.ReleaseHdc(canvasHdc);
            canvas.Dispose();

            return (int)((formatRange.rc.bottom - formatRange.rc.top) / AnInch);
        }
    }
}
