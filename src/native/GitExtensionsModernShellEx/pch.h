#pragma once

#ifndef NOMINMAX
#define NOMINMAX
#endif

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0A00
#endif

#include <windows.h>
#include <ShlObj.h>
#include <Shlwapi.h>
#include <appmodel.h>
#include <shellapi.h>
#include <pathcch.h>
#include <propvarutil.h>
#include <propkey.h>
#include <shobjidl.h>
#include <wrl.h>
#include <wrl/client.h>

#include <atomic>
#include <memory>
#include <string>
#include <string_view>
#include <vector>
#include <algorithm>
