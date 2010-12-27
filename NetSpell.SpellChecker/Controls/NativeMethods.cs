using System;
using System.Runtime.InteropServices;

namespace NetSpell.SpellChecker.Controls
{
	/// <summary>
	/// Summary description for NativeMethods.
	/// </summary>
	internal sealed class NativeMethods
	{

		private NativeMethods()
		{
		}

		// Windows Messages 
		internal const int WM_SETREDRAW				= 0x000B; 

		internal const int WM_PAINT					= 0x000F;
		internal const int WM_ERASEBKGND			= 0x0014;
		
		internal const int WM_NOTIFY				= 0x004E;
		
		internal const int WM_HSCROLL				= 0x0114;
		internal const int WM_VSCROLL				= 0x0115;

		internal const int WM_CAPTURECHANGED		= 0x0215;

		internal const int WM_USER					= 0x0400;
		
		// Win API declaration
		[DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
		internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam); 
 

	}
}
