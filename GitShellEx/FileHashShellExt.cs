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
using GitUI;
using System.Windows.Forms;
using PatchApply;
namespace FileHashShell
{
    [Guid("388C287C-2640-49ac-9FA6-AF4FCADB9485")]
	public class FileHashContextMenu: IShellExtInit, IContextMenu
	{
		#region Protected Members
        protected const string guid = "{388C287C-2640-49ac-9FA6-AF4FCADB9485}";
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
			//if ( (uFlags & 0xf) == 0 || (uFlags & (uint)CMF.CMF_EXPLORE) != 0)
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
                }
                //else
                    //return 0;

                id = PopulateMenu(hmnuPopup, idCmdFirst + id);

                // Add the popup to the context menu
				MENUITEMINFO mii = new MENUITEMINFO();
				mii.cbSize = 48;
				mii.fMask = (uint) MIIM.TYPE | (uint)MIIM.STATE | (uint) MIIM.SUBMENU;
				mii.hSubMenu = (int) hmnuPopup;
				mii.fType = (uint) MF.STRING;
				mii.dwTypeData = "GitEx";
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
            AddMenuItem(hMenu, "Add files (.)", id, 0);
            AddMenuItem(hMenu, "Branch", ++id, 1);
            AddMenuItem(hMenu, "Browse", ++id, 2);
            AddMenuItem(hMenu, "Checkout", ++id, 3);
            AddMenuItem(hMenu, "Clone", ++id, 4);
            AddMenuItem(hMenu, "Commit", ++id, 5);
            AddMenuItem(hMenu, "Diff", ++id, 6);
            AddMenuItem(hMenu, "Init new repository", ++id, 7);
            if (fileNames.Count > 0)
                AddMenuItem(hMenu, "File history", ++id, 8);
            AddMenuItem(hMenu, "View patch file", ++id, 9);
            AddMenuItem(hMenu, "Apply patch", ++id, 10);
            AddMenuItem(hMenu, "Push", ++id, 11);
            AddMenuItem(hMenu, "Pull", ++id, 12);
            AddMenuItem(hMenu, "Git bash", ++id, 13);
            AddMenuItem(hMenu, "Git GUI", ++id, 14);

            /*// Add a separator
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
            */


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
            if (fileNames.Count == 1)
                if (Directory.Exists(fileNames[0]))
                    GitCommands.Settings.WorkingDir = fileNames[0];

            if (string.IsNullOrEmpty(GitCommands.Settings.WorkingDir) && fileNames.Count > 0)
                GitCommands.Settings.WorkingDir = fileNames[0].Substring(0, fileNames[0].LastIndexOf('\\'));

            if (string.IsNullOrEmpty(GitCommands.Settings.WorkingDir))
                GitCommands.Settings.WorkingDir = Directory.GetCurrentDirectory();


			try
			{
				Type typINVOKECOMMANDINFO = Type.GetType("ShellExt.INVOKECOMMANDINFO");
				INVOKECOMMANDINFO ici = (INVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typINVOKECOMMANDINFO);
				switch (ici.verb-1)
				{
                    case 0://Add file
                        {
                            FormAddFiles form = new FormAddFiles();
                            form.Show();

                            break;
                        }
                    case 1://Branch
                        {
                            FormBranch form = new FormBranch();
                            form.Show();
                            break;
                        }
                    case 2://Browse
                        {
                            FormBrowse form = new FormBrowse();
                            form.Show();
                            break;
                        }
                    case 3://Checkout
                        {
                            FormCheckout form = new FormCheckout();
                            form.Show();
                            break;
                        }
                    case 4://Clone
                        {
                            FormClone form = new FormClone();
                            form.Show();
                            break;
                        }
                    case 5://Commit
                        {
                            FormCommit form = new FormCommit();
                            form.Show();
                            break;
                        }
                    case 6://Diff
                        {
                            FormDiff form = new FormDiff();
                            form.Show();
                            break;
                        }
                    case 7://Init
                        {
                            GitCommands.Init cmd = new GitCommands.Init(new GitCommands.InitDto());
                            cmd.Execute();
                            MessageBox.Show(cmd.Dto.Result);
                            break;
                        }
                    case 8://File history
                        {
                            if (fileNames.Count > 0)
                            {
                                FormFileHistory form = new FormFileHistory(fileNames[0]);
                                form.Show();
                            }
                            break;
                        }
                    case 9://ViewPatch
                        {
                            ViewPatch patchapply = new ViewPatch();
                            patchapply.Show();
                            break;
                        }
                    case 10://MergePatch
                        {
                            MergePatch form = new MergePatch();
                            form.Show();
                            break;
                        }
                    case 11://Push
                        {
                            FormPush form = new FormPush();
                            form.Show(); 
                            break;
                        }
                    case 12://Pull
                        {
                            FormPull form = new FormPull();
                            form.Show();
                            break;
                        }
                    case 13://Bash
                        {
                            GitCommands.GitCommands.RunBash();
                            break;
                        }
                    case 14://Gui
                        {
                            GitCommands.GitCommands.RunGui();
                            break;
                        }
                    default:
                        break;

                }

			}
			catch(Exception exe)
			{
                EventLog.WriteEntry("FileHashShell", exe.ToString());
			}

            //fileNames
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
					ShellExt.IDataObject dataObject = (ShellExt.IDataObject)Marshal.GetObjectForIUnknown(lpdobj);
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
                rk.SetValue(guid.ToString(), "GitExtensions shell extension");
                approved = rk.ToString();
                rk.Flush();
				rk.Close();

                // Set "*\\shellex\\ContextMenuHandlers\\FileHash" regkey to my guid
				root = Registry.ClassesRoot;
				rk = root.CreateSubKey("*\\shellex\\ContextMenuHandlers\\GitExtensions");
                rk.Flush();
				rk.SetValue(null, guid.ToString());
                contextMenu = rk.ToString();
                rk.Flush();
				rk.Close();

                root = Registry.ClassesRoot;
                rk = root.CreateSubKey("Directory\\shellex\\ContextMenuHandlers\\GitExtensions");
                rk.Flush();
                rk.SetValue(null, guid.ToString());
                contextMenu = rk.ToString();
                rk.Flush();
                rk.Close();

                root = Registry.ClassesRoot;
                rk = root.CreateSubKey("Directory\\Background\\shellex\\ContextMenuHandlers\\GitExtensions");
                rk.Flush();
                rk.SetValue(null, guid.ToString());
                contextMenu = rk.ToString();
                rk.Flush();
                rk.Close();


                EventLog.WriteEntry("Application", "GitExtensions Registration Complete.\r\n" + approved + "\r\n" + contextMenu, EventLogEntryType.Information);

			}
			catch(Exception e)
			{
                EventLog.WriteEntry("Application", "GitExtensions Registration error.\r\n" + e.ToString(), EventLogEntryType.Error);
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
                contextMenu = "*\\shellex\\ContextMenuHandlers\\GitExtensions";
                root.DeleteSubKey("*\\shellex\\ContextMenuHandlers\\GitExtensions");

                // Delete  regkey
                root = Registry.ClassesRoot;
                contextMenu = "Directory\\shellex\\ContextMenuHandlers\\GitExtensions";
                root.DeleteSubKey("Directory\\shellex\\ContextMenuHandlers\\GitExtensions");
                // Delete  regkey
                root = Registry.ClassesRoot;
                contextMenu = "Directory\\Background\\shellex\\ContextMenuHandlers\\GitExtensions";
                root.DeleteSubKey("Directory\\Background\\shellex\\ContextMenuHandlers\\GitExtensions");

                EventLog.WriteEntry("Application", "GitExtensions Unregister Complete.\r\n" + approved + "\r\n" + contextMenu, EventLogEntryType.Information);
            }
			catch(Exception e)
			{
                EventLog.WriteEntry("Application", "GitExtensions Unregister error.\r\n" + e.ToString(), EventLogEntryType.Error);
            }
        }
		#endregion

	}
}
