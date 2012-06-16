// SimpleShlExt.cpp : Implementation of CSimpleShlExt

#include "stdafx.h"
#include "resource.h"
#include "Generated/SimpleExt.h"
#include "SimpleShlExt.h"
//#include "afx.h"


#define MIIM_STRING      0x00000040
#define MIIM_BITMAP      0x00000080
#define MIIM_FTYPE       0x00000100

/////////////////////////////////////////////////////////////////////////////
// CSimpleShlExt

STDMETHODIMP CSimpleShlExt::Initialize (
    LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hProgID )
{
    FORMATETC fmt = { CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL };
    STGMEDIUM stg = { TYMED_HGLOBAL };
    HDROP     hDrop;

    // Look for CF_HDROP data in the data object.
    if ( pDataObj == NULL || FAILED( pDataObj->GetData ( &fmt, &stg ) ))
    {
        // Nope! Return an "invalid argument" error back to Explorer.
        return E_INVALIDARG;
    }

    // Get a pointer to the actual data.
    hDrop = (HDROP) GlobalLock ( stg.hGlobal );

    // Make sure it worked.
    if ( NULL == hDrop )
        return E_INVALIDARG;

    // Sanity check - make sure there is at least one filename.
    UINT uNumFiles = DragQueryFile ( hDrop, 0xFFFFFFFF, NULL, 0 );
    HRESULT hr = S_OK;

    if ( 0 == uNumFiles )
    {
        GlobalUnlock ( stg.hGlobal );
        ReleaseStgMedium ( &stg );
        return E_INVALIDARG;
    }

    // Get the name of the first file and store it in our member variable m_szFile.
    if ( 0 == DragQueryFile ( hDrop, 0, m_szFile, MAX_PATH ) )
        hr = E_INVALIDARG;

    GlobalUnlock ( stg.hGlobal );
    ReleaseStgMedium ( &stg );

    return hr;
}

STDMETHODIMP CSimpleShlExt::QueryContextMenu  (
    HMENU hmenu, UINT uMenuIndex, UINT uidFirstCmd,
    UINT uidLastCmd, UINT uFlags )
{
    // If the flags include CMF_DEFAULTONLY then we shouldn't do anything.
    if ( uFlags & CMF_DEFAULTONLY )
        return MAKE_HRESULT ( SEVERITY_SUCCESS, FACILITY_NULL, 0 );

    //InsertMenu (hmenu, uMenuIndex, MF_BYPOSITION, uidFirstCmd, _T("GitEx") );

    int id = 0;

    CString szCascadeContextMenu = GetRegistryValue(HKEY_CURRENT_USER, "SOFTWARE\\GitExtensions\\GitExtensions", "ShellCascadeContextMenu");

    CascadeContextMenu = !(szCascadeContextMenu == "False");

    if (CascadeContextMenu)
    {
        // show context menu cascaded in submenu
        HMENU popupMenu = CreateMenu();

        id = PopulateMenu(popupMenu, uidFirstCmd + id, true);

        //InsertMenu(hmenu, uMenuIndex++, MF_STRING | MF_BYPOSITION | MF_POPUP, (int)popupMenu, "Git Extensions");    
        MENUITEMINFO info;

        info.cbSize = sizeof( MENUITEMINFO );
        info.fMask = MIIM_ID | MIIM_TYPE | MIIM_SUBMENU;
        info.fType = MFT_STRING;
        info.wID = uidFirstCmd + 1;
        info.dwTypeData = _T("Git Extensions");
        info.hSubMenu = popupMenu;
        InsertMenuItem(hmenu, 0, true, &info);
    }
    else
    {
        // show menu items directly
        id = PopulateMenu(hmenu, uidFirstCmd + id, false);
    }

    return MAKE_HRESULT ( SEVERITY_SUCCESS, FACILITY_NULL, id-uidFirstCmd );
}

void CSimpleShlExt::AddMenuItem(HMENU hMenu, LPSTR text, int id, UINT position)
{
    MENUITEMINFO mii;
    memset(&mii, 0, sizeof(mii));
    mii.cbSize = sizeof(mii);
    mii.fMask = 0x00000010|0x00000002|0x00000001;//MIIM_TYPE|MIIM_ID|MIIM_STATE;
    mii.wID	= id;
    mii.fType = 0x00000000;
    mii.dwTypeData	= text;
    mii.fState = (UINT)0x00000000;

    InsertMenuItem(hMenu, position, TRUE, &mii);

    //InsertMenu(hMenu, position, MF_BYPOSITION, id, _T("test"));
}

bool CSimpleShlExt::IsMenuItemVisible(CString settings, int id)
{
    if (settings.GetLength() < id)
    {
        return true;
    } else
    {
        return (settings[id] != '0');
    }
}

int CSimpleShlExt::PopulateMenu(HMENU hMenu, int id, bool isSubMenu)
{
    CString szShellVisibleMenuItems = GetRegistryValue(HKEY_CURRENT_USER, "SOFTWARE\\GitExtensions\\GitExtensions", "ShellVisibleMenuItems");

    // preset values, if not used
    AddFilesId = -1;
    ApplyPatchId = -1;
    BrowseId = -1;
    CreateBranchId = -1;
    CheckoutBranchId = -1;
    CheckoutRevisionId = -1;
    CloneId = -1;
    CommitId = -1;
    FileHistoryId = -1;
    PullId = -1;
    PushId = -1;
    SettingsId = -1;
    ViewDiffId = -1;
    ResetFileChangesId = -1;

    int pos = 0;

    if (isSubMenu)
    {
        if (IsMenuItemVisible(szShellVisibleMenuItems, 0))
            AddMenuItem(hMenu, "Add files", ++id, AddFilesId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 1))
            AddMenuItem(hMenu, "Apply patch", ++id, ApplyPatchId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 2))
            AddMenuItem(hMenu, "Browse", ++id, BrowseId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 3))
            AddMenuItem(hMenu, "Create branch", ++id, CreateBranchId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 4))
            AddMenuItem(hMenu, "Checkout branch", ++id, CheckoutBranchId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 5))
            AddMenuItem(hMenu, "Checkout revision", ++id, CheckoutRevisionId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 6))
            AddMenuItem(hMenu, "Clone", ++id, CloneId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 7))
            AddMenuItem(hMenu, "Commit", ++id, CommitId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 8))
            AddMenuItem(hMenu, "File history", ++id, FileHistoryId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 9))
            AddMenuItem(hMenu, "Reset file changes", ++id, ResetFileChangesId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 10))
            AddMenuItem(hMenu, "Pull", ++id, PullId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 11))
            AddMenuItem(hMenu, "Push", ++id, PushId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 12))
            AddMenuItem(hMenu, "Settings", ++id, SettingsId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 13))
            AddMenuItem(hMenu, "View diff", ++id, ViewDiffId=pos++);
    }

    else
    {
        if (IsMenuItemVisible(szShellVisibleMenuItems, 0))
            AddMenuItem(hMenu, "GitExtensions Add files", ++id, AddFilesId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 1))
            AddMenuItem(hMenu, "GitExtensions Apply patch", ++id, ApplyPatchId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 2))
            AddMenuItem(hMenu, "GitExtensions Browse", ++id, BrowseId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 3))
            AddMenuItem(hMenu, "GitExtensions Create branch", ++id, CreateBranchId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 4))
            AddMenuItem(hMenu, "GitExtensions Checkout branch", ++id, CheckoutBranchId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 5))
            AddMenuItem(hMenu, "GitExtensions Checkout revision", ++id, CheckoutRevisionId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 6))
            AddMenuItem(hMenu, "GitExtensions Clone", ++id, CloneId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 7))
            AddMenuItem(hMenu, "GitExtensions Commit", ++id, CommitId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 8))
            AddMenuItem(hMenu, "GitExtensions File history", ++id, FileHistoryId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 9))
            AddMenuItem(hMenu, "GitExtensions Reset file changes", ++id, ResetFileChangesId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 10))
            AddMenuItem(hMenu, "GitExtensions Pull", ++id, PullId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 11))
            AddMenuItem(hMenu, "GitExtensions Push", ++id, PushId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 12))
            AddMenuItem(hMenu, "GitExtensions Settings", ++id, SettingsId=pos++);

        if (IsMenuItemVisible(szShellVisibleMenuItems, 13))
            AddMenuItem(hMenu, "GitExtensions View diff", ++id, ViewDiffId=pos++);
    }

    ++id;
    return id;
}

STDMETHODIMP CSimpleShlExt::GetCommandString (
    UINT_PTR idCmd, UINT uFlags, UINT* pwReserved, LPSTR pszName, UINT cchMax )
{
    USES_CONVERSION;

    // Check idCmd, it must be 0 since we have only one menu item.
    if ( 0 != idCmd )
        return E_INVALIDARG;

    // If Explorer is asking for a help string, copy our string into the
    // supplied buffer.
    if ( uFlags & GCS_HELPTEXT )
    {
        LPCTSTR szText = _T("Git shell extensions");

        if ( uFlags & GCS_UNICODE )
        {
            // We need to cast pszName to a Unicode string, and then use the
            // Unicode string copy API.
            lstrcpynW ( (LPWSTR) pszName, T2CW(szText), cchMax );
        }
        else
        {
            // Use the ANSI string copy API to return the help string.
            lstrcpynA ( pszName, T2CA(szText), cchMax );	
        }

        return S_OK;
    }

    return E_INVALIDARG;
}

void CSimpleShlExt::RunGitEx(const char * command)
{
    CString szFile = m_szFile;
    CString szCommandName = command;
    CString args;

    args += command;
    args += " \"";
    args += m_szFile;
    args += "\"";

    CString dir = "";

    if (dir.GetLength() == 0)
        dir = GetRegistryValue(HKEY_CURRENT_USER, "SOFTWARE\\GitExtensions\\GitExtensions", "InstallDir");
    if (dir.GetLength() == 0)
        dir = GetRegistryValue(HKEY_USERS, "SOFTWARE\\GitExtensions\\GitExtensions", "InstallDir");
    if (dir.GetLength() == 0)
        dir = GetRegistryValue(HKEY_LOCAL_MACHINE, "SOFTWARE\\GitExtensions\\GitExtensions", "InstallDir");

    ShellExecute(NULL, "open", "GitExtensions.exe", args, dir, SW_SHOWNORMAL); 
    //system(szMsg);
}

STDMETHODIMP CSimpleShlExt::InvokeCommand ( LPCMINVOKECOMMANDINFO pCmdInfo )
{
    // If lpVerb really points to a string, ignore this function call and bail out.
    if ( pCmdInfo == NULL ||0 != HIWORD( pCmdInfo->lpVerb ) )
        return E_INVALIDARG;

    int invokeId = LOWORD( pCmdInfo->lpVerb) - 1;

    // Get the command index - the only valid one is 0.
    if (invokeId == AddFilesId)
    {
        RunGitEx(_T("addfiles"));
        return S_OK;
    } else
    // Get the command index - the only valid one is 0.
    if (invokeId == ApplyPatchId)
    {
        RunGitEx(_T("applypatch"));
        return S_OK;
    } else
    if (invokeId == BrowseId)
    {
        RunGitEx(_T("browse"));
        return S_OK;
    } else
    if (invokeId == CreateBranchId)
    {
        RunGitEx(_T("branch"));
        return S_OK;
    } else
    if (invokeId == CheckoutBranchId)
    {
        RunGitEx(_T("checkoutbranch"));
        return S_OK;
    } else
    if (invokeId == CheckoutRevisionId)
    {
        RunGitEx(_T("checkoutrevision"));
        return S_OK;
    } else
    if (invokeId == CloneId)
    {
        RunGitEx(_T("clone"));
        return S_OK;
    } else
    if (invokeId == CommitId)
    {
        RunGitEx(_T("commit"));
        return S_OK;
    } else
    if (invokeId == FileHistoryId)
    {
        RunGitEx(_T("filehistory"));
        return S_OK;
    } else
    if (invokeId == PullId)
    {
        RunGitEx(_T("pull"));
        return S_OK;
    } else
    if (invokeId == PushId)
    {
        RunGitEx(_T("push"));
        return S_OK;
    } else
    if (invokeId == SettingsId)
    {
        RunGitEx(_T("settings"));
        return S_OK;
    } else
    if (invokeId == ViewDiffId)
    {
        RunGitEx(_T("viewdiff"));
        return S_OK;
    } else
    if (invokeId == ResetFileChangesId)
    {
        RunGitEx(_T("revert"));
        return S_OK;
    }
    return E_INVALIDARG;
}
