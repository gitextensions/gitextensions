// Code courtesy of Dino Esposito:
// "Manage with the Windows Shell: Write Shell Extensions with C#."
// http://www.theserverside.net/tt/articles/showarticle.tss?id=ShellExtensions

using System;
using System.Runtime.InteropServices;
using System.Text;


namespace ShellExt
{
	public class Helpers
	{
		#region Win32 Imports
		[DllImport("kernel32.dll")]
		internal static extern Boolean SetCurrentDirectory([MarshalAs(UnmanagedType.LPTStr)]string lpPathName);

		[DllImport("kernel32.dll")]
		internal static extern uint GetFileAttributes([MarshalAs(UnmanagedType.LPTStr)]string lpPathName);
		internal const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

		[DllImport("kernel32.dll")]
		internal static extern Boolean CreateProcess(
			string	lpApplicationName,
			string	lpCommandLine,
			uint	lpProcessAttributes,
			uint	lpThreadAttributes,
			Boolean bInheritHandles,
			uint	dwCreationFlags,
			uint	lpEnvironment,
			string	lpCurrentDirectory,
			StartupInfo lpStartupInfo,
			ProcessInformation lpProcessInformation);

		[DllImport("shell32")]
		internal static extern uint DragQueryFile(uint hDrop,uint iFile, StringBuilder buffer, int cch);

		[DllImport("user32")]
		internal static extern uint CreatePopupMenu();

		[DllImport("user32")]
		internal static extern int MessageBox(int hWnd, string text, string caption, int type);

		[DllImport("user32")]
		internal static extern int InsertMenuItem(uint hmenu, uint uposition, uint uflags, ref MENUITEMINFO mii);
	
		#endregion
	}
}
