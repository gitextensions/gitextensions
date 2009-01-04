// SimpleShlExt.h : Declaration of the CSimpleShlExt

#ifndef __SIMPLESHLEXT_H_
#define __SIMPLESHLEXT_H_

#include "atlstr.h"

/////////////////////////////////////////////////////////////////////////////
// CSimpleShlExt

class ATL_NO_VTABLE CSimpleShlExt : 
    public CComObjectRootEx<CComSingleThreadModel>,
    public CComCoClass<CSimpleShlExt, &CLSID_SimpleShlExt>,
    public IShellExtInit,
    public IContextMenu
{
public:
    CSimpleShlExt() { }

    DECLARE_REGISTRY_RESOURCEID(IDR_SIMPLESHLEXT)

    BEGIN_COM_MAP(CSimpleShlExt)
        COM_INTERFACE_ENTRY(IShellExtInit)
        COM_INTERFACE_ENTRY(IContextMenu)
    END_COM_MAP()

	int AddFilesId;
	int ApplyPatchId;
	int BrowseId;
	int CreateBranchId;
	int CheckoutBranchId;
	int CheckoutRevisionId;
	int CloneId;
	int CommitId;
	int FileHistoryId;
	int FormatPatchId;
	int PullId;
	int PushId;
	int SettingsId;
	int ViewDiffId;

public:
    // IShellExtInit
    STDMETHODIMP Initialize(LPCITEMIDLIST, LPDATAOBJECT, HKEY);

    // IContextMenu
    STDMETHODIMP GetCommandString(UINT_PTR, UINT, UINT*, LPSTR, UINT);
    STDMETHODIMP InvokeCommand(LPCMINVOKECOMMANDINFO);
    STDMETHODIMP QueryContextMenu(HMENU, UINT, UINT, UINT, UINT);

	void RunGitEx(const char * command);

	int PopulateMenu(HMENU hMenu, int id);
	void AddMenuItem(HMENU hmenu, LPSTR text, int id, UINT position);

protected:
    TCHAR m_szFile [MAX_PATH];


		CString  GetRegistryValue(
			HKEY	hOpenKey,
			LPCTSTR szKey,
			LPCTSTR path) {
	
	        CString result = "";
			HKEY key;
 
			unsigned char tempStr[512];
			unsigned long taille = sizeof(tempStr);
			unsigned long type;
			
			long res = RegOpenKeyEx(hOpenKey,szKey, 0, KEY_READ, &key);
			if (res != ERROR_SUCCESS) {
				return "";
			}
			if (RegQueryValueEx(key, path, 0, &type, (BYTE*)&tempStr[0], &taille) != ERROR_SUCCESS) {
				RegCloseKey(key);
				return "";
			}
			
			tempStr[taille] = 0;
 
			result = tempStr;
			
			RegCloseKey(key);
 
		
			return result;
	}
};

#endif //__SIMPLESHLEXT_H_
