// GitExtensionsShellEx.cpp : Implementation of CGitExtensionsShellEx

#include "stdafx.h"
#include <vector>
#include "resource.h"
#include "Generated/GitExtensionsShellEx.h"
#include "GitExtensionsShellEx.h"

#ifdef _DEBUG
#include <stdio.h>

void XTrace(LPCTSTR lpszFormat, ...)
{
    va_list args;
    va_start(args, lpszFormat);
    int nBuf;
    TCHAR szBuffer[512]; // get rid of this hard-coded buffer
    nBuf = _vsntprintf_s(szBuffer, 512, 511, lpszFormat, args);
    ::OutputDebugString(szBuffer);
    va_end(args);
}

#define DBG_TRACE   XTrace
#else
#define DBG_TRACE   __noop
#endif // _DEBUG

#define MIIM_ID          0x00000002
#define MIIM_STRING      0x00000040
#define MIIM_BITMAP      0x00000080

const wchar_t* const gitExCommandNames[] =
{
    L"addfiles",
    L"applypatch",
    L"browse",
    L"branch",
    L"checkoutbranch",
    L"checkoutrevision",
    L"clone",
    L"commit",
    L"init",
    L"difftool",
    L"filehistory",
    L"pull",
    L"push",
    L"reset",
    L"revert",
    L"settings",
    L"stash",
    L"viewdiff"
};

C_ASSERT(gcMaxValue == _countof(gitExCommandNames));

// Forward declaration of functions not declared in the header
static CString GetRegistryValue (HKEY hOpenKey, LPCTSTR szKey, LPCTSTR path);
static bool GetRegistryBoolValue(HKEY hOpenKey, LPCTSTR szKey, LPCTSTR path);
static bool DisplayInSubmenu(CString settings, int id);

static HRESULT Create32BitHBITMAP(HDC hdc, const SIZE* psize, __deref_opt_out void** ppvBits, __out HBITMAP* phBmp);
static bool HasAlpha(__in ARGB* pargb, SIZE& sizImage, int cxRow);
static HRESULT ConvertToPARGB32(HDC hdc, __inout ARGB* pargb, HBITMAP hbmp, SIZE& sizImage, int cxRow);

static bool ValidWorkingDir(const std::wstring& dir);
static bool IsValidGitDir(TCHAR m_szFile[]);

/////////////////////////////////////////////////////////////////////////////
// CGitExtensionsShellEx

CGitExtensionsShellEx::CGitExtensionsShellEx()
    : BufferedPaintAvailable(false), BufferedPaintInitialized(false), hUxTheme(NULL),
    pfnGetBufferedPaintBits(NULL), pfnBeginBufferedPaint(NULL), pfnEndBufferedPaint(NULL)
{
    if (::GetModuleHandleEx(0, _T("UXTHEME.DLL"), &hUxTheme))
    {
        pfnBufferedPaintInit = reinterpret_cast<FN_BufferedPaintInit>(::GetProcAddress(hUxTheme, "BufferedPaintInit"));
        pfnBufferedPaintUnInit = reinterpret_cast<FN_BufferedPaintInit>(::GetProcAddress(hUxTheme, "BufferedPaintUnInit"));
        pfnGetBufferedPaintBits = reinterpret_cast<FN_GetBufferedPaintBits>(::GetProcAddress(hUxTheme, "GetBufferedPaintBits"));
        pfnBeginBufferedPaint = reinterpret_cast<FN_BeginBufferedPaint>(::GetProcAddress(hUxTheme, "BeginBufferedPaint"));
        pfnEndBufferedPaint = reinterpret_cast<FN_EndBufferedPaint>(::GetProcAddress(hUxTheme, "EndBufferedPaint"));
        BufferedPaintAvailable = pfnBufferedPaintInit && pfnBufferedPaintUnInit &&
            pfnGetBufferedPaintBits && pfnBeginBufferedPaint && pfnEndBufferedPaint;
        if (BufferedPaintAvailable && SUCCEEDED(pfnBufferedPaintInit()))
            BufferedPaintInitialized = true;
    }

    ZeroMemory(m_szFile, sizeof(m_szFile));
}

CGitExtensionsShellEx::~CGitExtensionsShellEx()
{
    // Free any bitmaps we allocated; note that when the shell destroys the
    // menu, presumably via DestroyMenu, it won't free the bitmaps otherwise,
    // which would result in a resource leak.
    for (auto it = bitmaps.begin(); it != bitmaps.end(); ++it)
    {
        if (it->second)
            DeleteObject(it->second);
    }

    if (BufferedPaintInitialized)
        pfnBufferedPaintUnInit();
    if (hUxTheme)
        ::FreeLibrary(hUxTheme);
}

STDMETHODIMP CGitExtensionsShellEx::Initialize(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hProgID)
{
    FORMATETC fmt = { CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL };
    STGMEDIUM stg = { TYMED_HGLOBAL };
    HDROP     hDrop;

    /* store the folder, if provided */
    DBG_TRACE(L"CGitExtensionsShellEx::Initialize(pidlFolder=%p)", pidlFolder);
    m_szFile[0] = '\0';
    if (pidlFolder)
        SHGetPathFromIDList(pidlFolder, m_szFile);

    if (!pDataObj)
        return S_OK;

    // Look for CF_HDROP data in the data object.
    if (FAILED( pDataObj->GetData(&fmt, &stg)))
    {
        // Nope! Return an "invalid argument" error back to Explorer.
        return E_INVALIDARG;
    }

    // Get a pointer to the actual data.
    hDrop = (HDROP)GlobalLock(stg.hGlobal);

    // Make sure it worked.
    if (NULL == hDrop)
    {
        ReleaseStgMedium(&stg);
        return E_INVALIDARG;
    }

    // Sanity check - make sure there is at least one filename.
    UINT uNumFiles = DragQueryFile(hDrop, 0xFFFFFFFF, NULL, 0);
    HRESULT hr = S_OK;
    DBG_TRACE(L"uNumFiles=%u", uNumFiles);

    if (uNumFiles != 1)
        hr = E_INVALIDARG;
    // Get the name of the first file and store it in our member variable m_szFile.
    else if (!DragQueryFile(hDrop, 0, m_szFile, MAX_PATH))
        hr = E_INVALIDARG;
    DBG_TRACE(L"m_szFile=%s", m_szFile);

    GlobalUnlock(stg.hGlobal);
    ReleaseStgMedium(&stg);

    return hr;
}

HBITMAP CGitExtensionsShellEx::IconToBitmapPARGB32(UINT uIcon)
{
    std::map<UINT, HBITMAP>::iterator bitmap_it = bitmaps.lower_bound(uIcon);
    if (bitmap_it != bitmaps.end() && bitmap_it->first == uIcon)
        return bitmap_it->second;

    if (!BufferedPaintAvailable)
        return NULL;

    HICON hIcon = (HICON)LoadImage(_Module.GetModuleInstance(), MAKEINTRESOURCE(uIcon), IMAGE_ICON, 16, 16, LR_DEFAULTCOLOR);
    if (!hIcon)
        return NULL;

    SIZE sizIcon;
    sizIcon.cx = GetSystemMetrics(SM_CXSMICON);
    sizIcon.cy = GetSystemMetrics(SM_CYSMICON);

    RECT rcIcon;
    SetRect(&rcIcon, 0, 0, sizIcon.cx, sizIcon.cy);
    HBITMAP hBmp = NULL;

    HDC hdcDest = CreateCompatibleDC(NULL);
    if (hdcDest)
    {
        if (SUCCEEDED(Create32BitHBITMAP(hdcDest, &sizIcon, NULL, &hBmp)))
        {
            HBITMAP hbmpOld = (HBITMAP)SelectObject(hdcDest, hBmp);
            if (hbmpOld)
            {
                BLENDFUNCTION bfAlpha = { AC_SRC_OVER, 0, 255, AC_SRC_ALPHA };
                BP_PAINTPARAMS paintParams = {0};
                paintParams.cbSize = sizeof(paintParams);
                paintParams.dwFlags = BPPF_ERASE;
                paintParams.pBlendFunction = &bfAlpha;

                HDC hdcBuffer;
                HPAINTBUFFER hPaintBuffer = pfnBeginBufferedPaint(hdcDest, &rcIcon, BPBF_DIB, &paintParams, &hdcBuffer);
                if (hPaintBuffer)
                {
                    if (DrawIconEx(hdcBuffer, 0, 0, hIcon, sizIcon.cx, sizIcon.cy, 0, NULL, DI_NORMAL))
                    {
                        // If icon did not have an alpha channel we need to convert buffer to PARGB
                        ConvertBufferToPARGB32(hPaintBuffer, hdcDest, hIcon, sizIcon);
                    }

                    // This will write the buffer contents to the destination bitmap
                    pfnEndBufferedPaint(hPaintBuffer, TRUE);
                }

                SelectObject(hdcDest, hbmpOld);
            }
        }

        DeleteDC(hdcDest);
    }

    DestroyIcon(hIcon);

    if (hBmp)
        bitmaps.insert(bitmap_it, std::make_pair(uIcon, hBmp));
    return hBmp;
}

static HRESULT Create32BitHBITMAP(HDC hdc, const SIZE* psize, __deref_opt_out void** ppvBits, __out HBITMAP* phBmp)
{
    *phBmp = NULL;

    BITMAPINFO bmi;
    ZeroMemory(&bmi, sizeof(bmi));
    bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biPlanes = 1;
    bmi.bmiHeader.biCompression = BI_RGB;

    bmi.bmiHeader.biWidth = psize->cx;
    bmi.bmiHeader.biHeight = psize->cy;
    bmi.bmiHeader.biBitCount = 32;

    HDC hdcUsed = hdc ? hdc : GetDC(NULL);
    if (hdcUsed)
    {
        *phBmp = CreateDIBSection(hdcUsed, &bmi, DIB_RGB_COLORS, ppvBits, NULL, 0);
        if (hdc != hdcUsed)
        {
            ReleaseDC(NULL, hdcUsed);
        }
    }
    return (NULL == *phBmp) ? E_OUTOFMEMORY : S_OK;
}

HRESULT CGitExtensionsShellEx::ConvertBufferToPARGB32(HPAINTBUFFER hPaintBuffer, HDC hdc, HICON hicon, SIZE& sizIcon)
{
    RGBQUAD* prgbQuad;
    int cxRow;
    HRESULT hr = pfnGetBufferedPaintBits(hPaintBuffer, &prgbQuad, &cxRow);
    if (SUCCEEDED(hr))
    {
        hr = E_FAIL; // assume failure until subsequent ConvertToPARGB32 call
        ARGB* pargb = reinterpret_cast<ARGB*>(prgbQuad);
        if (!HasAlpha(pargb, sizIcon, cxRow))
        {
            ICONINFO info;
            if (GetIconInfo(hicon, &info))
            {
                if (info.hbmMask)
                {
                    hr = ConvertToPARGB32(hdc, pargb, info.hbmMask, sizIcon, cxRow);
                }

                DeleteObject(info.hbmColor);
                DeleteObject(info.hbmMask);
            }
        }
    }

    return hr;
}

static bool HasAlpha(__in ARGB* pargb, SIZE& sizImage, int cxRow)
{
    ULONG cxDelta = cxRow - sizImage.cx;
    for (ULONG y = sizImage.cy; y; --y)
    {
        for (ULONG x = sizImage.cx; x; --x)
        {
            if (*pargb++ & 0xFF000000)
            {
                return true;
            }
        }

        pargb += cxDelta;
    }

    return false;
}

static HRESULT ConvertToPARGB32(HDC hdc, __inout ARGB* pargb, HBITMAP hbmp, SIZE& sizImage, int cxRow)
{
    BITMAPINFO bmi;
    ZeroMemory(&bmi, sizeof(bmi));
    bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biPlanes = 1;
    bmi.bmiHeader.biCompression = BI_RGB;

    bmi.bmiHeader.biWidth = sizImage.cx;
    bmi.bmiHeader.biHeight = sizImage.cy;
    bmi.bmiHeader.biBitCount = 32;

    HRESULT hr = E_OUTOFMEMORY;
    HANDLE hHeap = GetProcessHeap();
    if (!hHeap)
        return HRESULT_FROM_WIN32(GetLastError());
    void* pvBits = HeapAlloc(hHeap, 0, bmi.bmiHeader.biWidth * 4 * bmi.bmiHeader.biHeight);
    if (pvBits)
    {
        hr = E_UNEXPECTED;
        if (GetDIBits(hdc, hbmp, 0, bmi.bmiHeader.biHeight, pvBits, &bmi, DIB_RGB_COLORS) == bmi.bmiHeader.biHeight)
        {
            ULONG cxDelta = cxRow - bmi.bmiHeader.biWidth;
            ARGB* pargbMask = static_cast<ARGB*>(pvBits);

            for (ULONG y = bmi.bmiHeader.biHeight; y; --y)
            {
                for (ULONG x = bmi.bmiHeader.biWidth; x; --x)
                {
                    if (*pargbMask++)
                    {
                        // transparent pixel
                        *pargb++ = 0;
                    }
                    else
                    {
                        // opaque pixel
                        *pargb++ |= 0xFF000000;
                    }
                }

                pargb += cxDelta;
            }

            hr = S_OK;
        }

        HeapFree(hHeap, 0, pvBits);
    }

    return hr;
}

bool IsExists(const std::wstring& dir)
{
    DWORD dwAttrib = GetFileAttributes(dir.c_str());

    return (dwAttrib != INVALID_FILE_ATTRIBUTES);
}

bool IsFileExists(LPCWSTR str)
{
    DWORD dwAttrib = GetFileAttributes(str);

    return (dwAttrib != INVALID_FILE_ATTRIBUTES) && ((dwAttrib & FILE_ATTRIBUTE_DIRECTORY) == 0);
}

static bool ValidWorkingDir(const std::wstring& dir)
{
    if (dir.empty())
        return false;

    if (IsExists(dir + L"\\.git\\") || IsExists(dir + L"\\.git"))
        return true;

    return IsExists(dir + L"\\info\\") &&
        IsExists(dir + L"\\objects\\") &&
        IsExists(dir + L"\\refs\\");
}

static bool IsValidGitDir(TCHAR m_szFile[])
{
    if (m_szFile[0] == '\0')
        return false;

    std::wstring dir(m_szFile);

    bool continueToParentDirectory;
    do
    {
        if (ValidWorkingDir(dir))
            return true;

        size_t lastBackslashPos = dir.rfind('\\');

        // PathIsRoot returns true for "C:\" and "\\server\share" but false for "C:" and "\\server\share\"
        // => The right part of the conjunction won't stop the loop for "C:", but the left part will
        // because "C:".rfind('\\') == wstring::npos
        continueToParentDirectory = (lastBackslashPos != std::wstring::npos) && !PathIsRoot(dir.c_str());

        if (continueToParentDirectory)
            dir.resize(lastBackslashPos);
    } while (continueToParentDirectory);
    return false;
}

STDMETHODIMP CGitExtensionsShellEx::QueryContextMenu(
    HMENU hMenu, UINT menuIndex, UINT uidFirstCmd, UINT uidLastCmd, UINT uFlags)
{
    DBG_TRACE(L"CGitExtensionsShellEx::QueryContextMenu(menuIndex=%u,uidLastCmd=%u,uFlags=%u)", menuIndex, uidLastCmd, uFlags);
    // If the flags include CMF_DEFAULTONLY then we shouldn't do anything.
    if (uFlags & CMF_DEFAULTONLY)
        return S_OK;

    //check if we already added our menu entry for a folder.
    //we check that by iterating through all menu entries and check if
    //the dwItemData member points to our global ID string. That string is set
    //by our shell extension when the folder menu is inserted.
    TCHAR menubuf[MAX_PATH];
    int count = GetMenuItemCount(hMenu);
    for (int i = 0; i < count; ++i)
    {
        MENUITEMINFO miif;
        SecureZeroMemory(&miif, sizeof(MENUITEMINFO));
        miif.cbSize = sizeof(MENUITEMINFO);
        miif.fMask = MIIM_DATA;
        miif.dwTypeData = menubuf;
        miif.cch = _countof(menubuf);
        GetMenuItemInfo(hMenu, i, TRUE, &miif);
        if (miif.dwItemData == (ULONG_PTR) _Module.GetModuleInstance())
        {
            DBG_TRACE(L"Menu already added");
            return S_OK;
        }
    }

    // Check that enough menu item IDs are available
    if (uidLastCmd - uidFirstCmd < 50) { // one for every menu item below, plus more room to grow.
        DBG_TRACE(L"Not enough menu IDs available");
        return E_FAIL;
    }

    CString szCascadeShellMenuItems = GetRegistryValue(HKEY_CURRENT_USER, L"SOFTWARE\\GitExtensions", L"CascadeShellMenuItems");
    if (szCascadeShellMenuItems.IsEmpty())
        szCascadeShellMenuItems = "110111000111111111";
    bool cascadeContextMenu = szCascadeShellMenuItems.Find('1') != -1;
    SHORT keyState = GetKeyState(VK_SHIFT);
    bool alwaysShowAllCommands = (keyState & 0x8000) || GetRegistryBoolValue(HKEY_CURRENT_USER, L"SOFTWARE\\GitExtensions", L"AlwaysShowAllCommands");

    HMENU popupMenu = NULL;
    if (cascadeContextMenu)
    {
        popupMenu = CreateMenu();
        if (!popupMenu) {
            return HRESULT_FROM_WIN32(GetLastError());
        }
    }

    bool isValidDir = true;
    bool isFolder = true;
    if (!alwaysShowAllCommands)
    {
        isValidDir = IsValidGitDir(m_szFile);
        isFolder = !IsFileExists(m_szFile);
    }

    // preset values, if not used
    commandsId.clear();

    UINT submenuIndex = 0;
    int id = 0;
    int cmdid;
    bool isSubMenu;

    if (alwaysShowAllCommands || !isValidDir)
    {
        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcClone);
        cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Clone...", IDI_ICONCLONEREPOGIT, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcClone;

        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcCreateRepository);
        cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Create new repository...", IDI_ICONCREATEREPOSITORY, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcCreateRepository;
    }
    if (isValidDir)
    {
        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcBrowse);
        cmdid = AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Open repository", IDI_ICONBROWSEFILEEXPLORER, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcBrowse;

        if (isFolder)
        {
            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcCommit);
            cmdid = AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Commit...", IDI_ICONCOMMIT, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcCommit;

            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcPull);
            cmdid = AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Pull...", IDI_ICONPULL, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcPull;

            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcPush);
            cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Push...", IDI_ICONPUSH, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcPush;

            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcStash);
            cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"View stash", IDI_ICONSTASH, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcStash;

            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcViewDiff);
            cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"View changes", IDI_ICONVIEWCHANGES, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcViewDiff;

            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcCheckoutBranch);
            if (isSubMenu && submenuIndex > 0) {
                InsertMenu(popupMenu, submenuIndex++, MF_SEPARATOR|MF_BYPOSITION, 0, NULL); ++id;
            }
            cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Checkout branch...", IDI_ICONBRANCHCHECKOUT, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcCheckoutBranch;

            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcCheckoutRevision);
            cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Checkout revision...", IDI_ICONREVISIONCHECKOUT, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcCheckoutRevision;

            isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcCreateBranch);
            cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Create branch...", IDI_ICONBRANCHCREATE, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
            commandsId[cmdid]=gcCreateBranch;
        }

        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcDiffTool);
        if (isSubMenu && submenuIndex > 0) {
            InsertMenu(popupMenu, submenuIndex++, MF_SEPARATOR|MF_BYPOSITION, 0, NULL); ++id;
        }
        cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Open with difftool", IDI_ICONVIEWCHANGES, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcDiffTool;

        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcFileHistory);
        cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"File history", IDI_ICONFILEHISTORY, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcFileHistory;

        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcResetFileChanges);
        cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Reset file changes...", IDI_ICONTRESETFILETO, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcResetFileChanges;

        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcAddFiles);
        cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Add files...", IDI_ICONADDED, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcAddFiles;

        isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcApplyPatch);
        cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Apply patch...", IDI_ICONPATCHAPPLY, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
        commandsId[cmdid]=gcApplyPatch;
    }

    isSubMenu = DisplayInSubmenu(szCascadeShellMenuItems, gcSettings);
    if (isSubMenu && submenuIndex > 0) {
        InsertMenu(popupMenu, submenuIndex++, MF_SEPARATOR|MF_BYPOSITION, 0, NULL); ++id;
    }
    cmdid=AddMenuItem(!isSubMenu ? hMenu : popupMenu, L"Settings", IDI_ICONSETTINGS, uidFirstCmd, ++id, !isSubMenu ? menuIndex++ : submenuIndex++, isSubMenu);
    commandsId[cmdid]=gcSettings;

    if (cascadeContextMenu)
    {
        MENUITEMINFO info;
        info.cbSize = sizeof(MENUITEMINFO);
        info.fMask = MIIM_STRING | MIIM_ID | MIIM_BITMAP | MIIM_SUBMENU;
        ++id;
        info.wID = uidFirstCmd + id;
        info.hbmpItem = BufferedPaintAvailable ? IconToBitmapPARGB32(IDI_GITEXTENSIONS) : HBMMENU_CALLBACK;
        myIDMap[uidFirstCmd + id] = IDI_GITEXTENSIONS;
        info.dwTypeData = _T("Git Extensions");
        info.hSubMenu = popupMenu;
        if (!InsertMenuItem(hMenu, menuIndex, true, &info)) {
            DestroyMenu(popupMenu);
        }
    }

    // Returned HRESULT must have offset of last ID that was assigned, plus one.
    ++id; // Plus one...
    return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, id);
}

UINT CGitExtensionsShellEx::AddMenuItem(HMENU hMenu, LPTSTR text, int resource, UINT uidFirstCmd, UINT id, UINT position, bool isSubMenu)
{
    MENUITEMINFO mii;
    memset(&mii, 0, sizeof(mii));
    mii.cbSize = sizeof(mii);
    mii.fMask = MIIM_STRING | MIIM_DATA | MIIM_ID;
    if (resource)
    {
        mii.fMask |= MIIM_BITMAP;
        mii.hbmpItem = BufferedPaintAvailable ? IconToBitmapPARGB32(resource) : HBMMENU_CALLBACK;
        myIDMap[uidFirstCmd + id] = resource;
    }
    mii.wID = uidFirstCmd + id;
    mii.dwItemData = (ULONG_PTR)_Module.GetModuleInstance();
    std::wstring textEx;
    if (isSubMenu)
        mii.dwTypeData = text;
    else
    {
        textEx = std::wstring(L"GitExt ") + text;
        mii.dwTypeData = &textEx[0];
    }

    InsertMenuItem(hMenu, position, TRUE, &mii);
    return id;
}

static bool DisplayInSubmenu(CString settings, int id)
{
    if (settings.GetLength() < id)
    {
        return true;
    }
    else
    {
        return (settings[id] != '0');
    }
}

STDMETHODIMP CGitExtensionsShellEx::GetCommandString(
    UINT_PTR idCmd, UINT uFlags, UINT* pwReserved, LPSTR pszName, UINT cchMax)
{
    USES_CONVERSION;

    // Check idCmd, it must be 0 since we have only one menu item.
    if (0 != idCmd)
        return E_INVALIDARG;

    // If Explorer is asking for a help string, copy our string into the
    // supplied buffer.
    if (uFlags & GCS_HELPTEXT)
    {
        LPCTSTR szText = _T("Git shell extensions");

        if (uFlags & GCS_UNICODE)
        {
            // We need to cast pszName to a Unicode string, and then use the
            // Unicode string copy API.
            lstrcpynW((LPWSTR)pszName, T2CW(szText), cchMax);
        }
        else
        {
            // Use the ANSI string copy API to return the help string.
            lstrcpynA (pszName, T2CA(szText), cchMax);
        }

        return S_OK;
    }

    return E_INVALIDARG;
}

void CGitExtensionsShellEx::RunGitEx(const TCHAR* command)
{
    CString szFile = m_szFile;
    CString szCommandName = command;
    CString args;

    if (szFile.GetLength() > 1 && szFile[szFile.GetLength() - 1] == '\\')
    {
        // Escape the final backslash to avoid escaping the quote.
        // This is a problem for drive roots on Windows, such as "C:\".
        szFile += '\\';
    }

    args += command;
    args += " \"";
    args += szFile;
    args += "\"";

    CString dir = "";

    if (dir.GetLength() == 0)
        dir = GetRegistryValue(HKEY_CURRENT_USER, L"SOFTWARE\\GitExtensions", L"InstallDir");
    if (dir.GetLength() == 0)
        dir = GetRegistryValue(HKEY_USERS, L"SOFTWARE\\GitExtensions", L"InstallDir");
    if (dir.GetLength() == 0)
        dir = GetRegistryValue(HKEY_LOCAL_MACHINE, L"SOFTWARE\\GitExtensions", L"InstallDir");

    ShellExecute(NULL, L"open", L"GitExtensions.exe", args, dir, SW_SHOWNORMAL);
}

STDMETHODIMP CGitExtensionsShellEx::InvokeCommand(LPCMINVOKECOMMANDINFO pCmdInfo)
{
    // If lpVerb really points to a string, ignore this function call and bail out.
    if (pCmdInfo == NULL || !IS_INTRESOURCE(pCmdInfo->lpVerb))
        return E_INVALIDARG;

    int invokeId = LOWORD(pCmdInfo->lpVerb);

    auto it = commandsId.find(invokeId);
    if (it != commandsId.end()) {
        RunGitEx(gitExCommandNames[it->second]);
        return S_OK;
    }
    return E_INVALIDARG;
}

STDMETHODIMP CGitExtensionsShellEx::HandleMenuMsg(UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    LRESULT res;
    return HandleMenuMsg2(uMsg, wParam, lParam, &res);
}

STDMETHODIMP CGitExtensionsShellEx::HandleMenuMsg2(UINT uMsg, WPARAM wParam, LPARAM lParam, LRESULT* pResult)
{
    switch (uMsg)
    {
    case WM_MEASUREITEM:
        {
            MEASUREITEMSTRUCT* lpmis = (MEASUREITEMSTRUCT*)lParam;
            if (lpmis == NULL)
                break;
            lpmis->itemWidth = 16;
            lpmis->itemHeight = 16;
            if (pResult)
                *pResult = TRUE;
        }
        break;
    case WM_DRAWITEM:
        {
            LPCTSTR resource;
            DRAWITEMSTRUCT* lpdis = (DRAWITEMSTRUCT*)lParam;
            if (lpdis == NULL || lpdis->CtlType != ODT_MENU)
                return S_OK;  // not for a menu
            auto it = myIDMap.find(lpdis->itemID);
            if (it == myIDMap.end())
                return S_OK;
            resource = MAKEINTRESOURCE(it->second);
            if (resource == NULL)
                return S_OK;
            HICON hIcon = (HICON)LoadImage(_Module.GetModuleInstance(), resource, IMAGE_ICON, 16, 16, LR_DEFAULTCOLOR);
            if (hIcon == NULL)
                return S_OK;
            DrawIconEx(lpdis->hDC,
                lpdis->rcItem.left,
                lpdis->rcItem.top + (lpdis->rcItem.bottom - lpdis->rcItem.top - 16) / 2,
                hIcon, 16, 16,
                0, NULL, DI_NORMAL);
            DestroyIcon(hIcon);
            if (pResult)
                *pResult = TRUE;
        }
        break;
    default:
        return S_OK;
    }

    return S_OK;
}

static CString GetRegistryValue(HKEY hOpenKey, LPCTSTR szKey, LPCTSTR path)
{
    HKEY key;
    long res = RegOpenKeyEx(hOpenKey,szKey, 0, KEY_READ | KEY_WOW64_32KEY, &key);
    if (res != ERROR_SUCCESS)
    {
        return "";
    }

    TCHAR tempStr[512];
    // RegQueryValueEx doesn't promise to add a trailing NULL.
    DWORD bufsize = sizeof(tempStr) - sizeof(TCHAR);
    ZeroMemory(&tempStr[0], sizeof(tempStr));
    DWORD type;
    if (RegQueryValueEx(key, path, 0, &type, (BYTE*)&tempStr[0], &bufsize) != ERROR_SUCCESS)
    {
        RegCloseKey(key);
        return "";
    }

    // Verify returned type
    if (type != REG_SZ && type != REG_EXPAND_SZ)
    {
        RegCloseKey(key);
        return "";
    }

    RegCloseKey(key);
    return tempStr;
}

static bool GetRegistryBoolValue(HKEY hOpenKey, LPCTSTR szKey, LPCTSTR path)
{
    CString value = GetRegistryValue(hOpenKey, szKey, path);
    if (value.IsEmpty())
        return false;
    return value.CompareNoCase(L"true") == 0;
}