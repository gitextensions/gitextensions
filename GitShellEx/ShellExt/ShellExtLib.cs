// *********************************************************************************
// Title:		ShellExtLib.cs
// Description:	ShellExtension interfaces
// *********************************************************************************
// Code courtesy of Dino Esposito:
// "Manage with the Windows Shell: Write Shell Extensions with C#."
// http://www.theserverside.net/tt/articles/showarticle.tss?id=ShellExtensions

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;



namespace ShellExt
{
	public struct MenuItem
	{
		public string Text;
		public string Command;
		public string HelpText;
	}

	// Make these constants 
	public enum MIIM : uint
	{
		STATE =			0x00000001,
		ID =	        0x00000002,
		SUBMENU	=		0x00000004,
		CHECKMARKS =	0x00000008,
		TYPE =			0x00000010,
		DATA =			0x00000020,
		STRING =		0x00000040,
		BITMAP =		0x00000080,
		FTYPE =			0x00000100
	}

	public enum	MF : uint
	{
		INSERT =        0x00000000,
		CHANGE =        0x00000080,
		APPEND =        0x00000100,
		DELETE =        0x00000200,
		REMOVE =        0x00001000,
		BYCOMMAND =     0x00000000,
		BYPOSITION =    0x00000400,
		SEPARATOR =     0x00000800,
		ENABLED =       0x00000000,
		GRAYED =        0x00000001,
		DISABLED =      0x00000002,
		UNCHECKED =     0x00000000,
		CHECKED =       0x00000008,
		USECHECKBITMAPS=0x00000200,
		STRING =        0x00000000,
		BITMAP =        0x00000004,
		OWNERDRAW =     0x00000100,
		POPUP =         0x00000010,
		MENUBARBREAK =  0x00000020,
		MENUBREAK =     0x00000040,
		UNHILITE =      0x00000000,
		HILITE =        0x00000080,
		DEFAULT =       0x00001000,
		SYSMENU =       0x00002000,
		HELP =          0x00004000,
		RIGHTJUSTIFY =  0x00004000,
		MOUSESELECT =   0x00008000
	}

	public enum MFS : uint
	{
		GRAYED =        0x00000003,
		DISABLED =      MFS.GRAYED,
		CHECKED =       MF.CHECKED,
		HILITE =        MF.HILITE,
		ENABLED =       MF.ENABLED,
		UNCHECKED =     MF.UNCHECKED,
		UNHILITE =      MF.UNHILITE,
		DEFAULT =       MF.DEFAULT,
		MASK =          0x0000108B,
		HOTTRACKDRAWN = 0x10000000,
		CACHEDBMP =     0x20000000,
		BOTTOMGAPDROP = 0x40000000,
		TOPGAPDROP =    0x80000000,
		GAPDROP =       0xC0000000
	}

	public enum CLIPFORMAT : uint
	{
		CF_TEXT =		1,
		CF_BITMAP =		2,
		CF_METAFILEPICT= 3,
		CF_SYLK =		4,
		CF_DIF =		5,
		CF_TIFF =		6,
		CF_OEMTEXT =	7,
		CF_DIB =		8,
		CF_PALETTE =	9,
		CF_PENDATA =	10,
		CF_RIFF =		11,
		CF_WAVE =		12,
		CF_UNICODETEXT= 13,
		CF_ENHMETAFILE= 14,
		CF_HDROP =		15,
		CF_LOCALE =		16,
		CF_MAX =		17,

		CF_OWNERDISPLAY=0x0080,
		CF_DSPTEXT =	0x0081,
		CF_DSPBITMAP =	0x0082,
		CF_DSPMETAFILEPICT= 0x0083,
		CF_DSPENHMETAFILE = 0x008E,

		CF_PRIVATEFIRST=0x0200,
		CF_PRIVATELAST=	0x02FF,

		CF_GDIOBJFIRST =0x0300,
		CF_GDIOBJLAST =	0x03FF
	}


	public	enum DVASPECT: uint
	{
		DVASPECT_CONTENT = 1,
		DVASPECT_THUMBNAIL = 2,
		DVASPECT_ICON = 4,
		DVASPECT_DOCPRINT = 8
	}

	public	enum TYMED: uint
	{
		TYMED_HGLOBAL = 1,
		TYMED_FILE =	2,
		TYMED_ISTREAM = 4,
		TYMED_ISTORAGE= 8,
		TYMED_GDI =		16,
		TYMED_MFPICT =	32,
		TYMED_ENHMF	=	64,
		TYMED_NULL=		0
    }

	public	enum CMF: uint
	{
		CMF_NORMAL		= 0x00000000,
		CMF_DEFAULTONLY	= 0x00000001,
		CMF_VERBSONLY	= 0x00000002,
		CMF_EXPLORE		= 0x00000004,
		CMF_NOVERBS		= 0x00000008,
		CMF_CANRENAME	= 0x00000010,
		CMF_NODEFAULT	= 0x00000020,
		CMF_INCLUDESTATIC= 0x00000040,
		CMF_RESERVED	= 0xffff0000      // View specific
	}

	// GetCommandString uFlags
	public enum GCS: uint
	{
		VERBA =			0x00000000,     // canonical verb
		HELPTEXTA =		0x00000001,     // help text (for status bar)
		VALIDATEA =		0x00000002,     // validate command exists
		VERBW =			0x00000004,     // canonical verb (unicode)
		HELPTEXTW =		0x00000005,     // help text (unicode version)
		VALIDATEW =		0x00000006,     // validate command exists (unicode)
		UNICODE =		0x00000004,     // for bit testing - Unicode string
		VERB =			GCS.VERBA,
		HELPTEXT =		GCS.HELPTEXTA,
		VALIDATE =		GCS.VALIDATEA
	}

	[StructLayout(LayoutKind.Sequential)]
	public class StartupInfo
	{
		public int cb;
		public String lpReserved;
		public String lpDesktop;
		public String lpTitle;
		public int dwX;
		public int dwY;
		public int dwXSize;
		public int dwYSize;
		public int dwXCountChars;
		public int dwYCountChars;
		public int dwFillAttribute;
		public int dwFlags;
		public UInt16 wShowWindow;
		public UInt16 cbReserved2;
		public Byte  lpReserved2;
		public int hStdInput;
		public int hStdOutput;
		public int hStdError;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ProcessInformation
	{
		public int hProcess;
		public int hThread;
		public int dwProcessId;
		public int dwThreadId;
	}
	
	
	[StructLayout(LayoutKind.Sequential)]
	public struct MENUITEMINFO
	{
		public uint cbSize;
		public uint fMask;
		public uint fType;
		public uint fState;
		public int	wID;
		public int	/*HMENU*/	  hSubMenu;
 		public int	/*HBITMAP*/   hbmpChecked;
		public int	/*HBITMAP*/	  hbmpUnchecked;
		public int	/*ULONG_PTR*/ dwItemData;
		public String dwTypeData;
		public uint cch;
		public int /*HBITMAP*/ hbmpItem;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FORMATETC
	{
		public CLIPFORMAT	cfFormat;
		public uint			ptd;
		public DVASPECT		dwAspect;
		public int			lindex;
		public TYMED		tymed;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct STGMEDIUM
	{
	  public uint tymed;
	  public uint hGlobal;
	  public uint pUnkForRelease;
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	public struct INVOKECOMMANDINFO
	{
		//NOTE: When SEE_MASK_HMONITOR is set, hIcon is treated as hMonitor
		public uint cbSize;						// sizeof(CMINVOKECOMMANDINFO)
		public uint fMask;						// any combination of CMIC_MASK_*
		public uint wnd;						// might be NULL (indicating no owner window)
		public int verb;
		[MarshalAs(UnmanagedType.LPStr)]
		public string parameters;				// might be NULL (indicating no parameter)
		[MarshalAs(UnmanagedType.LPStr)]
		public string directory;				// might be NULL (indicating no specific directory)
		public int Show;						// one of SW_ values for ShowWindow() API
		public uint HotKey;
		public uint hIcon;
	}

	[ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), GuidAttribute("0000010e-0000-0000-C000-000000000046")]
	public interface IDataObject
	{
		[PreserveSig()]
		int GetData(ref FORMATETC a, ref STGMEDIUM b);
		[PreserveSig()]
		void GetDataHere(int a, ref STGMEDIUM b);
		[PreserveSig()]
		int QueryGetData(int a);
		[PreserveSig()]
		int GetCanonicalFormatEtc(int a, ref int b);
		[PreserveSig()]
		int SetData(int a, int b, int c);
		[PreserveSig()]
		int EnumFormatEtc(uint a, ref Object b);
		[PreserveSig()]
		int DAdvise(int a, uint b, Object c, ref uint d);
		[PreserveSig()]
		int DUnadvise(uint a);
		[PreserveSig()]
		int EnumDAdvise(ref Object a);
	}

	[ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), GuidAttribute("000214e8-0000-0000-c000-000000000046")]
	public interface IShellExtInit
	{
		[PreserveSig()]
		int Initialize (IntPtr pidlFolder, IntPtr lpdobj, uint /*HKEY*/ hKeyProgID);
	}


	[ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), GuidAttribute("000214e4-0000-0000-c000-000000000046")]
	public	interface IContextMenu
	{
		// IContextMenu methods
		[PreserveSig()]
		int	QueryContextMenu(uint hmenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags);
		[PreserveSig()]
		void	InvokeCommand (IntPtr pici);
		[PreserveSig()]
		void	GetCommandString(int idcmd, uint uflags, int reserved, StringBuilder commandstring, int cch);
	}





}
