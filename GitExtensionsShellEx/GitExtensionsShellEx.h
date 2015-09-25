// GitExtensionsShellEx.h : Declaration of the CGitExtensionsShellEx

#ifndef __GITEXTENSIONSSHELLEX_H_
#define __GITEXTENSIONSSHELLEX_H_

#include <atlstr.h>
#include <uxtheme.h>
#include <map>

typedef DWORD ARGB;

typedef HRESULT (WINAPI *FN_BufferedPaintInit) (void);
typedef HRESULT (WINAPI *FN_BufferedPaintUnInit) (void);
typedef HRESULT (WINAPI *FN_GetBufferedPaintBits) (HPAINTBUFFER hBufferedPaint, RGBQUAD **ppbBuffer, int *pcxRow);
typedef HPAINTBUFFER (WINAPI *FN_BeginBufferedPaint) (HDC hdcTarget, const RECT *prcTarget, BP_BUFFERFORMAT dwFormat, BP_PAINTPARAMS *pPaintParams, HDC *phdc);
typedef HRESULT (WINAPI *FN_EndBufferedPaint) (HPAINTBUFFER hBufferedPaint, BOOL fUpdateTarget);

/////////////////////////////////////////////////////////////////////////////
// CGitExtensionsShellEx

// don't change indexes because of FormSettings
enum GitExCommands
{
    gcAddFiles,
    gcApplyPatch,
    gcBrowse,
    gcCreateBranch,
    gcCheckoutBranch,
    gcCheckoutRevision,
    gcClone,
    gcCommit,
    gcCreateRepository,
    gcDiffTool,
    gcFileHistory,
    gcPull,
    gcPush,
    gcResetFileChanges,
    gcRevert,
    gcSettings,
    gcStash,
    gcViewDiff,
    gcMaxValue
};

class ATL_NO_VTABLE CGitExtensionsShellEx : 
    public CComObjectRootEx<CComSingleThreadModel>,
    public CComCoClass<CGitExtensionsShellEx, &CLSID_GitExtensionsShellEx>,
    public IShellExtInit,
    public IContextMenu3
{
public:
    CGitExtensionsShellEx();
    virtual ~CGitExtensionsShellEx();

    DECLARE_REGISTRY_RESOURCEID(IDR_GITEXTENSIONSSHELLEX)

    BEGIN_COM_MAP(CGitExtensionsShellEx)
        COM_INTERFACE_ENTRY(IShellExtInit)
        COM_INTERFACE_ENTRY(IContextMenu)
        COM_INTERFACE_ENTRY(IContextMenu2)
        COM_INTERFACE_ENTRY(IContextMenu3)
    END_COM_MAP()

public:
    // IShellExtInit
    STDMETHODIMP Initialize(LPCITEMIDLIST, LPDATAOBJECT, HKEY);

    // IContextMenu
    STDMETHODIMP GetCommandString(UINT_PTR, UINT, UINT*, LPSTR, UINT);
    STDMETHODIMP InvokeCommand(LPCMINVOKECOMMANDINFO);
    STDMETHODIMP QueryContextMenu(HMENU, UINT, UINT, UINT, UINT);

    // IContextMenu2
    STDMETHODIMP HandleMenuMsg(UINT uMsg, WPARAM wParam, LPARAM lParam);
    
    // IContextMenu3
    STDMETHODIMP HandleMenuMsg2(UINT uMsg, WPARAM wParam, LPARAM lParam, LRESULT* pResult);

private:
    void RunGitEx(const TCHAR* command);

    UINT AddMenuItem(HMENU hmenu, LPTSTR text, int resource, UINT firstId, UINT id, UINT position, bool isSubMenu);

    TCHAR m_szFile[MAX_PATH];
    std::map<UINT_PTR, int> myIDMap;
    std::map<UINT, HBITMAP> bitmaps;
    std::map<int, int> commandsId;

    HMODULE hUxTheme;
    FN_BufferedPaintInit pfnBufferedPaintInit;
    FN_BufferedPaintUnInit pfnBufferedPaintUnInit;
    FN_GetBufferedPaintBits pfnGetBufferedPaintBits;
    FN_BeginBufferedPaint pfnBeginBufferedPaint;
    FN_EndBufferedPaint pfnEndBufferedPaint;
    bool BufferedPaintAvailable;
    bool BufferedPaintInitialized;

    HBITMAP IconToBitmapPARGB32(UINT uIcon);
    HRESULT ConvertBufferToPARGB32(HPAINTBUFFER hPaintBuffer, HDC hdc, HICON hicon, SIZE& sizIcon);
};

#endif //__GITEXTENSIONSSHELLEX_H_
