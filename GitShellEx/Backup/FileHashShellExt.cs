// *******************************************************************************
//	Title:			BatchResults.cs
//	Description:	Sample shell extension
// *******************************************************************************
// Modified from original code by Dino Esposito:
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
using System.Diagnostics;
using ShellExt;
using System.Collections.Generic;
namespace FileHashShell
{
    [Guid("E79E003C-F495-46d3-96BA-B85CA13C3F54")]
	public class FileHashContextMenu: IShellExtInit, IContextMenu
	{
		#region Protected Members
        protected const string guid = "{E79E003C-F495-46d3-96BA-B85CA13C3F54}";
		protected string m_fileName;
        protected List<string> fileNames = new List<string>();
		protected uint m_hDrop = 0;
		#endregion

		#region IContextMenu
		// IContextMenu
        /// <summary>
        /// Add the "Calculate File Hash" menu item to the Context menu
        /// </summary>
        /// <param name="hMenu"></param>
        /// <param name="iMenu"></param>
        /// <param name="idCmdFirst"></param>
        /// <param name="idCmdLast"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
		int	IContextMenu.QueryContextMenu(uint hMenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags)
		{
			// Create the popup to insert
			uint hmnuPopup = Helpers.CreatePopupMenu();
			int id = 1;
			if ( (uFlags & 0xf) == 0 || (uFlags & (uint)CMF.CMF_EXPLORE) != 0)
			{
				uint nselected = Helpers.DragQueryFile(m_hDrop, 0xffffffff, null, 0);
                if (nselected > 0)
                {
                    for (uint i = 0; i < nselected; i++)
                    {
                        StringBuilder sb = new StringBuilder(1024);
                        Helpers.DragQueryFile(m_hDrop, i, sb, sb.Capacity + 1);
                        fileNames.Add(sb.ToString());
                    }
                    // Populate the popup menu with file-specific items
                    id = PopulateMenu(hmnuPopup, idCmdFirst + id);
                }
                else
                    return 0;
					
				// Add the popup to the context menu
				MENUITEMINFO mii = new MENUITEMINFO();
				mii.cbSize = 48;
				mii.fMask = (uint) MIIM.TYPE | (uint)MIIM.STATE | (uint) MIIM.SUBMENU;
				mii.hSubMenu = (int) hmnuPopup;
				mii.fType = (uint) MF.STRING;
				mii.dwTypeData = "Calculate File Hash";
				mii.fState = (uint) MF.ENABLED;
				Helpers.InsertMenuItem(hMenu, (uint)iMenu, 1, ref mii);

				// Add a separator
				MENUITEMINFO sep = new MENUITEMINFO();
				sep.cbSize = 48;
				sep.fMask = (uint )MIIM.TYPE;
				sep.fType = (uint) MF.SEPARATOR;
				Helpers.InsertMenuItem(hMenu, iMenu+1, 1, ref sep);
			
			}
			return id;
		}

		void AddMenuItem(uint hMenu, string text, int id, uint position)
		{
			MENUITEMINFO mii = new MENUITEMINFO();
			mii.cbSize = 48;
			mii.fMask = (uint)MIIM.ID | (uint)MIIM.TYPE | (uint)MIIM.STATE;
			mii.wID	= id;
			mii.fType = (uint)MF.STRING;
			mii.dwTypeData	= text;
			mii.fState = (uint)MF.ENABLED;
			Helpers.InsertMenuItem(hMenu, position, 1, ref mii);
		}

        int PopulateMenu(uint hMenu, int id)
        {
            AddMenuItem(hMenu, "SHA-1", id, 0);
            AddMenuItem(hMenu, "MD5", ++id, 1);
            AddMenuItem(hMenu, "SHA-256", ++id, 2);
            AddMenuItem(hMenu, "SHA-384", ++id, 3);
            AddMenuItem(hMenu, "SHA-512", ++id, 4);
            AddMenuItem(hMenu, "RIPEMD-160", ++id, 5);

            // Add a separator
            MENUITEMINFO sep = new MENUITEMINFO();
            sep.cbSize = 48;
            sep.fMask = (uint)MIIM.TYPE;
            sep.fType = (uint)MF.SEPARATOR;
            sep.wID = ++id;
            Helpers.InsertMenuItem(hMenu, (uint)6, 1, ref sep);

            //Add "Include File Date" date check indicator
            MENUITEMINFO mii = new MENUITEMINFO();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.ID | (uint)MIIM.TYPE | (uint)MIIM.STATE;
            mii.wID = ++id;
            mii.fType = (uint)MF.STRING;
            mii.dwTypeData = "Include File Date";
            if (Properties.Settings.Default.IncludeDate)
                mii.fState = (uint)MF.ENABLED | (uint)MF.CHECKED;
            else
                mii.fState = (uint)MF.ENABLED | (uint)MF.UNCHECKED;
            Helpers.InsertMenuItem(hMenu, (uint)7, 1, ref mii);

            //Add "Include File Size" check indicator
            mii = new MENUITEMINFO();
            mii.cbSize = 48;
            mii.fMask = (uint)MIIM.ID | (uint)MIIM.TYPE | (uint)MIIM.STATE;
            mii.wID = ++id;
            mii.fType = (uint)MF.STRING;
            mii.dwTypeData = "Include File Size";
            if (Properties.Settings.Default.IncludeFileSize)
                mii.fState = (uint)MF.ENABLED | (uint)MF.CHECKED;
            else
                mii.fState = (uint)MF.ENABLED | (uint)MF.UNCHECKED;
            Helpers.InsertMenuItem(hMenu, (uint)8, 1, ref mii);



            return id++;
        }
		
		void IContextMenu.GetCommandString(int idCmd, uint uFlags, int pwReserved, StringBuilder commandString, int cchMax)
		{
			switch(uFlags)
			{
			case (uint)GCS.VERB:
				commandString = new StringBuilder("...");
				break;
			case (uint)GCS.HELPTEXT:
				commandString = new StringBuilder("..."); 
				break;
			}
		}

		
		void IContextMenu.InvokeCommand (IntPtr pici)
		{
			try
			{
                HashHelper.HashType hashType;
				Type typINVOKECOMMANDINFO = Type.GetType("ShellExt.INVOKECOMMANDINFO");
				INVOKECOMMANDINFO ici = (INVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typINVOKECOMMANDINFO);
				switch (ici.verb-1)
				{
					case 0:
                    default:
                        hashType = HashHelper.HashType.SHA1;
						break;
					case 1:
                        hashType = HashHelper.HashType.MD5;
						break;
					case 2:
                        hashType = HashHelper.HashType.SHA256;
						break;
					case 3:
                        hashType = HashHelper.HashType.SHA384;
						break;
					case 4:
						hashType = HashHelper.HashType.SHA512;
						break;
                    case 5:
                        hashType = HashHelper.HashType.RIPEMD160;
                        break;
                    case 7:
                        Properties.Settings.Default.IncludeDate = !Properties.Settings.Default.IncludeDate;
                        Properties.Settings.Default.Save();
                        return;
                    case 8:
                        Properties.Settings.Default.IncludeFileSize = !Properties.Settings.Default.IncludeFileSize;
                        Properties.Settings.Default.Save();
                        return;
				}
                FileHashShell.ShellForm form = new FileHashShell.ShellForm(fileNames, hashType);
                form.Show();
			}
			catch(Exception exe)
			{
                EventLog.WriteEntry("FileHashShell", exe.ToString());
			}
		}
		#endregion

		#region IShellExtInit
		int	IShellExtInit.Initialize (IntPtr pidlFolder, IntPtr lpdobj, uint hKeyProgID)
		{
			try
			{
				if (lpdobj != (IntPtr)0)
				{
					// Get info about the directory
					IDataObject dataObject = (IDataObject)Marshal.GetObjectForIUnknown(lpdobj);
					FORMATETC fmt = new FORMATETC();
					fmt.cfFormat = CLIPFORMAT.CF_HDROP;
					fmt.ptd		 = 0;
					fmt.dwAspect = DVASPECT.DVASPECT_CONTENT;
					fmt.lindex	 = -1;
					fmt.tymed	 = TYMED.TYMED_HGLOBAL;
					STGMEDIUM medium = new STGMEDIUM();
					dataObject.GetData(ref fmt, ref medium);
					m_hDrop = medium.hGlobal;
				}
			}
			catch(Exception)
			{
			}
			return 0;
		}

		#endregion
		
        #region Registration
		[System.Runtime.InteropServices.ComRegisterFunctionAttribute()]
		static void RegisterServer(String str1)
		{
			try
			{
                string approved = string.Empty;
                string contextMenu = string.Empty;
				// For Winnt set me as an approved shellex
				RegistryKey root;
				RegistryKey rk;
				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
				rk.SetValue(guid.ToString(), "FileHash shell extension");
                approved = rk.ToString();
                rk.Flush();
				rk.Close();

                // Set "*\\shellex\\ContextMenuHandlers\\FileHash" regkey to my guid
				root = Registry.ClassesRoot;
				rk = root.CreateSubKey("*\\shellex\\ContextMenuHandlers\\FileHash");
                rk.Flush();
				rk.SetValue(null, guid.ToString());
                contextMenu = rk.ToString();
                rk.Flush();
				rk.Close();

                EventLog.WriteEntry("Application", "FileHashShellExt Registration Complete.\r\n" + approved + "\r\n" + contextMenu, EventLogEntryType.Information);

			}
			catch(Exception e)
			{
                EventLog.WriteEntry("Application", "FileHashShellExt Registration error.\r\n" + e.ToString(), EventLogEntryType.Error);
            }
        }

		[System.Runtime.InteropServices.ComUnregisterFunctionAttribute()]
		static void UnregisterServer(String str1)
		{

			try
			{
                string approved = string.Empty;
                string contextMenu = string.Empty;
				RegistryKey root;
				RegistryKey rk;

				// Remove ShellExtenstions registration
				root = Registry.LocalMachine;
				rk = root.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", true);
                approved = rk.ToString();
				rk.DeleteValue(guid);
				rk.Close();

				// Delete  regkey
				root = Registry.ClassesRoot;
                contextMenu = "*\\shellex\\ContextMenuHandlers\\FileHash";
                root.DeleteSubKey("*\\shellex\\ContextMenuHandlers\\FileHash");
                EventLog.WriteEntry("Application", "FileHashShellExt Unregister Complete.\r\n" + approved + "\r\n" + contextMenu, EventLogEntryType.Information);
            }
			catch(Exception e)
			{
                EventLog.WriteEntry("Application", "FileHashShellExt Unregister error.\r\n" + e.ToString(), EventLogEntryType.Error);
            }
        }
		#endregion

	}
}
