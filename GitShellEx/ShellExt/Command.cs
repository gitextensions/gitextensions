// *******************************************************************************
//	Title:			Command.cs
//	Description:	Sample shell extension
// *******************************************************************************
// Code courtesy of Dino Esposito:
// "Manage with the Windows Shell: Write Shell Extensions with C#."
// http://www.theserverside.net/tt/articles/showarticle.tss?id=ShellExtensions

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.Security.Permissions;


		// Strong name keyfile for this assembly

namespace ShellExt
{
	[Guid("82C62DC5-1A4B-43AC-92C8-571410996B45")]
	public class CommandShellExtension: IShellExtInit, IContextMenu
	{
		const string guid = "{82C62DC5-1A4B-43ac-92C8-571410996B45}";

		/*
		#region Win32 Imports
		[DllImport("kernel32.dll")]
		static extern Boolean SetCurrentDirectory([MarshalAs(UnmanagedType.LPTStr)]string lpPathName);

		[DllImport("kernel32.dll")]
		static extern uint GetFileAttributes([MarshalAs(UnmanagedType.LPTStr)]string lpPathName);
		const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

		[DllImport("kernel32.dll")]
		static extern Boolean CreateProcess(
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
		static extern uint DragQueryFile(uint hDrop,uint iFile, StringBuilder buffer, int cch);

        [DllImport("user32")]
        static extern int MessageBox(int hWnd, string text, string caption, int type);

		[DllImport("user32")]
		static extern int InsertMenuItem(uint hmenu, uint uposition, uint uflags, ref MENUITEMINFO mii);
		#endregion
*/

		#region Protected Members
		protected IDataObject m_dataObject = null;
		uint m_hDrop = 0;
		MenuItem[] m_items;
		#endregion

		#region IContextMenu
		int	IContextMenu.QueryContextMenu(uint hMenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags)
		{
			int id = 1;
			if ( (uFlags & 0xf) == 0 || (uFlags & (uint)CMF.CMF_EXPLORE) != 0)
			{
				uint nselected = Helpers.DragQueryFile(m_hDrop, 0xffffffff, null, 0);
				if (nselected == 1)
				{
					StringBuilder sb = new StringBuilder(1024);
					Helpers.DragQueryFile(m_hDrop, 0, sb, sb.Capacity + 1);
					string directory = sb.ToString();
					uint attr = Helpers.GetFileAttributes(directory);
					if ((attr & Helpers.FILE_ATTRIBUTE_DIRECTORY) != 0)
					{
						// I have selected a bona-fide directory add to the menus.
						MENUITEMINFO mii = new MENUITEMINFO();
						mii.cbSize		= 48;
						mii.fMask		= (uint)MIIM.ID | (uint)MIIM.TYPE | (uint)MIIM.STATE;

						foreach(MenuItem item in m_items)
						{
							mii.wID			= idCmdFirst + id;
							mii.fType		= (uint)MF.STRING;
							mii.dwTypeData	= item.Text;
							mii.fState		= (uint)MF.ENABLED;
							Helpers.InsertMenuItem(hMenu, (uint)2, 1, ref mii);
							id++;
						}
						// Go and get the menus from the registry
						if (m_items.Length > 0)
						{
							mii.fType		= (uint)MF.SEPARATOR;
							mii.fState		= (uint)MF.ENABLED;
							Helpers.InsertMenuItem(hMenu, (uint)2, 1, ref mii);
						}
					}
				}
			}
			return id;
		}

		
		void IContextMenu.GetCommandString(int idCmd, uint uFlags, int pwReserved, StringBuilder commandString, int cchMax)
		{
			switch(uFlags)
			{
			case (uint)GCS.VERB:
				commandString = new StringBuilder(m_items[idCmd - 1].Command.Substring(1, cchMax-1));
				break;
			case (uint)GCS.HELPTEXT:
				commandString = new StringBuilder(m_items[idCmd - 1].HelpText.Substring(1, cchMax));
				break;
			case (uint)GCS.VALIDATE:
				break;
			}
		}

		
		void IContextMenu.InvokeCommand (IntPtr pici)
		{
			try
			{
				Type typINVOKECOMMANDINFO = Type.GetType("ShellExt.INVOKECOMMANDINFO");
				INVOKECOMMANDINFO ici = (INVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typINVOKECOMMANDINFO);
				if (ici.verb - 1 <= m_items.Length)
					ExecuteCommand(m_items[ici.verb - 1].Command);
			}
			catch(Exception)
			{
			}
		}
		#endregion

		#region IShellExtInit
		int	IShellExtInit.Initialize (IntPtr pidlFolder, IntPtr lpdobj, uint hKeyProgID)
		{
			try
			{
				m_dataObject = null;
				if (lpdobj != (IntPtr)0)
				{
					// Get info about the directory
					m_dataObject = (IDataObject)Marshal.GetObjectForIUnknown(lpdobj);
					FORMATETC fmt = new FORMATETC();
					fmt.cfFormat = CLIPFORMAT.CF_HDROP;
					fmt.ptd		 = 0;
					fmt.dwAspect = DVASPECT.DVASPECT_CONTENT;
					fmt.lindex	 = -1;
					fmt.tymed	 = TYMED.TYMED_HGLOBAL;
					STGMEDIUM medium = new STGMEDIUM();
					m_dataObject.GetData(ref fmt, ref medium);
					m_hDrop = medium.hGlobal;


					// Now retrieve the menu information from the registry
					RegistryKey sc = Registry.LocalMachine;
					sc = sc.OpenSubKey("Software\\Microsoft\\ShellCmd", true);
					if (sc.SubKeyCount > 0)
					{
						m_items = new MenuItem[sc.SubKeyCount];
						int	i=0;
						foreach(string name in sc.GetSubKeyNames())
						{
							try
							{
								RegistryKey item = sc.OpenSubKey(name, true);
								string command = (string)item.GetValue("");
								MenuItem m = new MenuItem();
								m.Text = name;
								m.Command = command;
								m_items[i] = m;
								++i;
							}
							catch(Exception)
							{
							}
						}
					}
					else
					{
						m_items = new MenuItem[0];
					}
				}
			}
			catch(Exception)
			{
			}
			return 0;
		}

		#endregion
		
		#region Private Members
		private	void ExecuteCommand(string command)
		{
			StartupInfo si = new StartupInfo();
			ProcessInformation pi = new ProcessInformation();
			si.cb = 68; //sizeof(si);
			try
			{
				// Get the directory name
				StringBuilder sb = new StringBuilder(1024);
				Helpers.DragQueryFile(m_hDrop, 0, sb, sb.Capacity + 1);
				string directory = sb.ToString();

				if (!Helpers.CreateProcess (null, command, 0, 0, false, 0, 0, directory, si, pi))
				{
					Helpers.MessageBox(0, "Unable to execute : " + command, "Error in ShellCmd.exe", 0);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		#endregion

		#region Registration
		[System.Runtime.InteropServices.ComRegisterFunctionAttribute()]
		static void RegisterServer(String str1)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;
				root = Registry.CurrentUser;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer", true);
				rk.SetValue("DesktopProcess", 1);
				rk.Close();

				// For Winnt set me as an approved shellex
				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.SetValue(guid.ToString(), "ShellCmd shell extension");
				rk.Close();

				// Set "Folder\\shellex\\ContextMenuHandlers\\ShellCmd" regkey to my guid
				root = Registry.ClassesRoot;
				rk = root.CreateSubKey("Folder\\shellex\\ContextMenuHandlers\\ShellCmd");
				rk.SetValue("", guid.ToString());
				rk.Close();

				// Add setting for Command.com
				root = Registry.LocalMachine;
				rk = root.CreateSubKey("Software\\Microsoft\\ShellCmd");
				rk = root.CreateSubKey("Software\\Microsoft\\ShellCmd\\Command");
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					rk.SetValue("", "cmd.exe");
				else
					rk.SetValue("", "command.com");

				rk.Close();
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}

		[System.Runtime.InteropServices.ComUnregisterFunctionAttribute()]
		static void UnregisterServer(String str1)
		{
			try
			{
				RegistryKey root;
				RegistryKey rk;

				// Remove ShellExtenstions registration
				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.DeleteValue(guid);
				rk.Close();

				// Delete ShellCmd regkey
				root = Registry.ClassesRoot;
				root.DeleteSubKey("Folder\\shellex\\ContextMenuHandlers\\ShellCmd");

				// Delete Command.com setting
				root = Registry.LocalMachine;
				root.DeleteSubKey("Software\\Microsoft\\ShellCmd\\Command");
				root.DeleteSubKey("Software\\Microsoft\\ShellCmd");
			}
			catch(Exception e)
			{
				System.Console.WriteLine(e.ToString());
			}
		}
		#endregion

	}
}
