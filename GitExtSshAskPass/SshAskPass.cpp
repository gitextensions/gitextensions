// Copyright (C) 2008-2013 - TortoiseGit
// Copyright (C) 2013-2015 - GitExtensions

// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation,
// 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//

// SshAskPass.cpp : Defines the entry point for the application.
//

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

// C RunTime Header Files
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>

#include <atlbase.h>

#include <commctrl.h>
#pragma comment(linker, "\"/manifestdependency:type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")

#include "resource.h"
#include <propsys.h>
#include <PropKey.h>

#define MAX_LOADSTRING 100

// Global Variables:
HINSTANCE hInst;								// current instance
TCHAR szTitle[MAX_LOADSTRING];					// The title bar text
TCHAR szWindowClass[MAX_LOADSTRING];			// the main window class name

// Forward declarations of functions included in this code module:
INT_PTR CALLBACK	About(HWND, UINT, WPARAM, LPARAM);

TCHAR g_Promptphrase[] = _T("Enter your OpenSSH passphrase:");
TCHAR *g_Prompt = NULL;

TCHAR g_PassWord[MAX_LOADSTRING];

int APIENTRY _tWinMain(HINSTANCE	/*hInstance*/,
					 HINSTANCE		/*hPrevInstance*/,
					 LPTSTR			lpCmdLine,
					 int			/*nCmdShow*/)
{
	SetDllDirectory(L"");

	InitCommonControls();

	if( _tcslen(lpCmdLine) == 0 )
	{
		g_Prompt = g_Promptphrase;
	}
	else
	{
		TCHAR *lpPrompt = lpCmdLine;
		size_t len = _tcslen(lpPrompt);
		if (len >= 2 && lpPrompt[0] == '\"' && lpPrompt[len - 1] == '\"')
		{
			lpPrompt[len - 1] = '\0';
			lpPrompt++;
		}
		g_Prompt = lpPrompt;
	}

	TCHAR *yesno=_T("(yes/no)");
	size_t lens = _tcslen(yesno);
	TCHAR *p = lpCmdLine;
	BOOL bYesNo=FALSE;

	while(*p)
	{
		if (_tcsncicmp(p, yesno, lens) == 0)
		{
			bYesNo = TRUE;
			break;
		}
		++p;
	}

	if (bYesNo)
	{
		if (::MessageBox(NULL, lpCmdLine, _T("Git Extensions - git CLI stdin wrapper"), MB_YESNO | MB_ICONQUESTION) == IDYES)
		{
			_tprintf(_T("yes"));
		}
		else
		{
			_tprintf(_T("no"));
		}
		return 0;
	}
	else
	{
		if(DialogBox(hInst, MAKEINTRESOURCE(IDD_ASK_PASSWORD), NULL, About) == IDOK)
		{
			_tprintf(_T("%s\n"), g_PassWord);
			return 0;
		}
		_tprintf(_T("\n"));
		return -1;
	}
}

void MarkWindowAsUnpinnable(HWND hWnd)
{
	typedef HRESULT (WINAPI *SHGPSFW) (HWND hwnd,REFIID riid,void** ppv);

	HMODULE hShell = ::LoadLibrary(_T("Shell32.dll"));

	if (hShell) {
		SHGPSFW pfnSHGPSFW = (SHGPSFW)::GetProcAddress(hShell, "SHGetPropertyStoreForWindow");
		if (pfnSHGPSFW) {
			IPropertyStore *pps;
			HRESULT hr = pfnSHGPSFW(hWnd, IID_PPV_ARGS(&pps));
			if (SUCCEEDED(hr)) {
				PROPVARIANT var;
				var.vt = VT_BOOL;
				var.boolVal = VARIANT_TRUE;
				hr = pps->SetValue(PKEY_AppUserModel_PreventPinning, var);
				pps->Release();
			}
		}
		FreeLibrary(hShell);
	}
}

// Message handler for password box.
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM /*lParam*/)
{
	switch (message)
	{
	case WM_INITDIALOG:
		{
			MarkWindowAsUnpinnable(hDlg);
			RECT rect;
			::GetWindowRect(hDlg,&rect);
			DWORD dwWidth = GetSystemMetrics(SM_CXSCREEN);
			DWORD dwHeight = GetSystemMetrics(SM_CYSCREEN);

			DWORD x,y;
			x=(dwWidth - (rect.right-rect.left))/2;
			y=(dwHeight - (rect.bottom-rect.top))/2;

			::MoveWindow(hDlg,x,y,rect.right-rect.left,rect.bottom-rect.top,TRUE);
			HWND title=::GetDlgItem(hDlg,IDC_STATIC_TITLE);
			::SetWindowText(title,g_Prompt);

			TCHAR *pass =_T("pass");
			size_t passlens = _tcslen(pass);
			TCHAR *p = g_Prompt;
			bool password = false;
			while (*p)
			{
				if (_tcsncicmp(p, pass, passlens) == 0)
				{
					password = true;
					break;
				}
				++p;
			}
			if (!password)
				SendMessage(::GetDlgItem(hDlg, IDC_PASSWORD), EM_SETPASSWORDCHAR, 0, 0);
		}
		return (INT_PTR)TRUE;

	case WM_COMMAND:

		if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
		{
			if( LOWORD(wParam) == IDOK )
			{
				HWND password = ::GetDlgItem(hDlg,IDC_PASSWORD);
				::GetWindowText(password,g_PassWord,MAX_LOADSTRING);
			}
			EndDialog(hDlg, LOWORD(wParam));
			return (INT_PTR)TRUE;
		}
		break;
	}
	return (INT_PTR)FALSE;
}
