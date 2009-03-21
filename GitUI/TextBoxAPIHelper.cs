using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI
{
	/// <summary>
	/// Wraps API calls for access to missing functionality
	/// from the System.Windows.Forms text box.
	/// </summary>
	internal class TextBoxAPIHelper
	{
		private const double anInch = 14.4;

		internal static int GetTextWidthAtCharIndex(System.Windows.Forms.TextBoxBase tb, int index, int length) 
		{ 
			System.Windows.Forms.RichTextBox rtb = tb as RichTextBox;
			
			//TODO!
			if (rtb==null) { return tb.Font.Height; }

			NativeMethods.CHARRANGE charRange; 
			charRange.cpMin = index; 
			charRange.cpMax = index + length;

            NativeMethods.RECT rect; 
			rect.Top = 0; 
			rect.Bottom = (int)anInch; 
			rect.Left = 0; 
			rect.Right = (int)(rtb.ClientSize.Width * anInch);

            NativeMethods.RECT rectPage; 
			rectPage.Top = 0; 
			rectPage.Bottom = (int)anInch; 
			rectPage.Left = 0; 
			rectPage.Right = (int)(rtb.ClientSize.Width * anInch); 

			Graphics canvas = Graphics.FromHwnd(rtb.Handle); 
			IntPtr canvasHdc = canvas.GetHdc();

            NativeMethods.FORMATRANGE formatRange; 
			formatRange.chrg = charRange; 
			formatRange.hdc = canvasHdc; 
			formatRange.hdcTarget = canvasHdc; 
			formatRange.rc = rect; 
			formatRange.rcPage = rectPage;

            NativeMethods.SendMessage(rtb.Handle, NativeMethods.EM_FORMATRANGE, IntPtr.Zero, ref formatRange); 

			canvas.ReleaseHdc(canvasHdc); 
			canvas.Dispose();
			
			return (int)((formatRange.rc.Right - formatRange.rc.Left) / anInch); 

		}

        internal static int GetBaselineOffsetAtCharIndex(System.Windows.Forms.TextBoxBase tb, int index) 
		{ 
			System.Windows.Forms.RichTextBox rtb = tb as RichTextBox;
			if (rtb==null) { return tb.Font.Height; }

			int lineNumber = rtb.GetLineFromCharIndex(index);
            int lineIndex = NativeMethods.SendMessageInt(rtb.Handle, NativeMethods.EM_LINEINDEX, new IntPtr(lineNumber), IntPtr.Zero).ToInt32();
            int lineLength = NativeMethods.SendMessageInt(rtb.Handle, NativeMethods.EM_LINELENGTH, new IntPtr(lineNumber), IntPtr.Zero).ToInt32();


            NativeMethods.CHARRANGE charRange; 
			charRange.cpMin = lineIndex; 
			charRange.cpMax = lineIndex + lineLength; 
//			charRange.cpMin = index; 
//			charRange.cpMax = index + 1; 


            NativeMethods.RECT rect; 
			rect.Top = 0; 
			rect.Bottom = (int)anInch; 
			rect.Left = 0; 
			rect.Right = 10000000;//(int)(rtb.Width * anInch + 20); 


            NativeMethods.RECT rectPage; 
			rectPage.Top = 0; 
			rectPage.Bottom = (int)anInch; 
			rectPage.Left = 0; 
			rectPage.Right = 10000000;//(int)(rtb.Width * anInch + 20); 

			Graphics canvas = Graphics.FromHwnd(rtb.Handle); 
			IntPtr canvasHdc = canvas.GetHdc();

            NativeMethods.FORMATRANGE formatRange; 
			formatRange.chrg = charRange; 
			formatRange.hdc = canvasHdc; 
			formatRange.hdcTarget = canvasHdc; 
			formatRange.rc = rect; 
			formatRange.rcPage = rectPage;

            NativeMethods.SendMessage(rtb.Handle, NativeMethods.EM_FORMATRANGE, IntPtr.Zero, ref formatRange).ToInt32(); 

			canvas.ReleaseHdc(canvasHdc); 
			canvas.Dispose();
			
			return (int)((formatRange.rc.Bottom - formatRange.rc.Top) / anInch); 
		} 




		/// <summary>
		/// Attempts to make the caret visible in a TextBox control.
		/// This will not always succeed since the TextBox control
		/// appears to destroy its caret fairly frequently.
		/// </summary>
		/// <param name="txt">The text box to show the caret in.</param>
        internal static void ShowCaret(
			System.Windows.Forms.TextBox txt
			)
		{
			bool ret = false;
			int iter = 0;
			while (!ret && iter < 10)
			{
                ret = NativeMethods.ShowCaretAPI(txt.Handle);
				iter++;
			}
		}

		/// <summary>
		/// Returns the index of the character under the specified 
		/// point in the control, or the nearest character if there
		/// is no character under the point.
		/// </summary>
		/// <param name="txt">The text box control to check.</param>
		/// <param name="pt">The point to find the character for, 
		/// specified relative to the client area of the text box.</param>
		/// <returns></returns>
        internal static int CharFromPos(
			System.Windows.Forms.TextBoxBase txt,
			Point pt
			)
		{
			unchecked
			{
				// Convert the point into a DWord with horizontal position
				// in the loword and vertical position in the hiword:
				int xy = (pt.X & 0xFFFF) + ((pt.Y & 0xFFFF) << 16);
				// Get the position from the text box.
                int res = NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_CHARFROMPOS, IntPtr.Zero, new IntPtr(xy)).ToInt32();
				// the Platform SDK appears to be incorrect on this matter.
				// the hiword is the line number and the loword is the index
				// of the character on this line
				int lineNumber = ((res & 0xFFFF) >> 16);
				int charIndex = (res & 0xFFFF);
        
				// Find the index of the first character on the line within 
				// the control:
                int lineStartIndex = NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_LINEINDEX,
					new IntPtr(lineNumber), IntPtr.Zero).ToInt32();
				// Return the combined index:
				return lineStartIndex + charIndex;
			}
		}

		/// <summary>
		/// Returns the position of the specified character
		/// </summary>
		/// <param name="txt">The text box to find the character in.</param>
		/// <param name="charIndex">The index of the character whose
		/// position needs to be found.</param>
		/// <returns>The position of the character relative to the client
		/// area of the control.</returns>
        internal static Point PosFromChar(
			System.Windows.Forms.TextBoxBase txt,
			int charIndex
			)
		{
			unchecked
			{
                int xy = NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_POSFROMCHAR, new IntPtr(charIndex), IntPtr.Zero).ToInt32();
				return new Point(xy);
			}
		}

        internal static int GetFirstVisibleLine(System.Windows.Forms.TextBoxBase txt)
		{
            return NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32();
		}

        internal static int GetLineIndex(TextBoxBase txt, int line)
		{
            return NativeMethods.SendMessageInt(txt.Handle, 0xbb, new IntPtr(line), IntPtr.Zero).ToInt32();
		}

		// private constructor, methods are static
		private TextBoxAPIHelper()
		{
			// intentionally left blank
		}
	}
}
