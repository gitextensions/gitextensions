// stdafx.cpp : source file that includes just the standard includes
//  stdafx.pch will be the pre-compiled header
//  stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"

#ifdef _ATL_STATIC_REGISTRY
#include <statreg.h>
#if _ATL_VER < 0x0700
#include <statreg.cpp>
#endif
#endif

#if _ATL_VER < 0x0700
#include <atlimpl.cpp>
#endif
