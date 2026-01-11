#include "pch.h"

#include "CommandRunner.h"
#include "SelectionContext.h"

namespace
{
    std::wstring GetExternalPackageRoot()
    {
        // ReSharper disable once CppInconsistentNaming
        using PFN_GetCurrentPackageInfo2 =
            LONG(WINAPI*)(UINT32, UINT32, UINT32*, PBYTE, UINT32*);

        static const auto GetCurrentPackageInfo2 =
            reinterpret_cast<PFN_GetCurrentPackageInfo2>(  // NOLINT(clang-diagnostic-cast-function-type-strict)
                GetProcAddress(GetModuleHandleW(L"kernel32.dll"),
                    "GetCurrentPackageInfo2"));

        if (!GetCurrentPackageInfo2) return {};

        UINT32 length = 0;
        UINT32 count = 0;
        const LONG initial = GetCurrentPackageInfo2(PACKAGE_FILTER_HEAD,
            PackagePathType_EffectiveExternal, &length, nullptr, &count);
        if (initial != ERROR_INSUFFICIENT_BUFFER || length == 0) return {};

        std::vector<BYTE> buffer(length);
        if (GetCurrentPackageInfo2(PACKAGE_FILTER_HEAD,
            PackagePathType_EffectiveExternal, &length, buffer.data(),
            &count) != ERROR_SUCCESS) return {};

        const auto packageInfo = reinterpret_cast<PACKAGE_INFO*>(buffer.data());
        if (count == 0 || packageInfo->path == nullptr) return {};

        return { packageInfo->path };
    }

    std::wstring GetModuleDirectory()
    {
        wchar_t modulePath[MAX_PATH] = {};
        HMODULE module = nullptr;
        if (!GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
            GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, reinterpret_cast<LPCWSTR>(
                &GetExternalPackageRoot), &module)) return {};

        const DWORD length = GetModuleFileNameW(module, modulePath, _countof(modulePath));
        if (length == 0 || length >= _countof(modulePath)) return {};

        wchar_t* lastSlash = wcsrchr(modulePath, L'\\');
        if (!lastSlash) return {};

        *lastSlash = L'\0';
        return { modulePath };
    }
}

std::wstring CommandRunner::ReadRegistryString(
    const HKEY root,
    const std::wstring& subKey,
    const std::wstring& valueName)
{
    DWORD length = 0;
    DWORD type = 0;
    if (RegGetValueW(root, subKey.data(), valueName.data(), RRF_RT_REG_SZ, &type,
        nullptr, &length) != ERROR_SUCCESS || length == 0) return {};

    std::wstring buffer;
    buffer.resize(length / sizeof(wchar_t));
    if (RegGetValueW(root, subKey.data(), valueName.data(), RRF_RT_REG_SZ, &type,
        buffer.data(), &length) != ERROR_SUCCESS) return {};

    // RegGetValue includes the null terminator in length
    if (!buffer.empty() && buffer.back() == L'\0')
        buffer.pop_back();

    return { buffer };
}

std::wstring CommandRunner::ResolveInstallRoot()
{
    if (auto root = GetExternalPackageRoot(); !root.empty()) return root;

    if (auto moduleDir = GetModuleDirectory(); !moduleDir.empty()) return moduleDir;

    if (auto registry = ReadRegistryString(HKEY_CURRENT_USER, L"SOFTWARE\\GitExtensions",
        L"InstallDir"); !registry.empty()) return registry;

    if (auto registry = ReadRegistryString(HKEY_USERS, L"SOFTWARE\\GitExtensions",
        L"InstallDir"); !registry.empty()) return registry;

    if (auto registry = ReadRegistryString(HKEY_LOCAL_MACHINE, L"SOFTWARE\\GitExtensions",
        L"InstallDir"); !registry.empty()) return registry;

    return {};
}

std::wstring CommandRunner::BuildArguments(
    const CommandDefinition& definition,
    const SelectionContext& selection)
{
    std::wstring args(definition.Verb);
    if (HasFlag(definition.Flags, CommandFlags::UsesAllSelections))
    {
        args.append(QuoteAllPaths(selection.Paths));
        return args;
    }

    const auto primary = selection.PrimaryPath();
    if (!primary.empty())
        args.append(QuotePath(primary));

    return args;
}

HRESULT CommandRunner::Run(
    const CommandDefinition& definition,
    const SelectionContext& selection)
{
    const std::wstring installRoot = ResolveInstallRoot();
    if (installRoot.empty())
        return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);

    std::wstring exePath = installRoot;
    if (!exePath.empty() && exePath.back() != L'\\')
        exePath.push_back(L'\\');

    exePath.append(L"GitExtensions.exe");

    const std::wstring arguments = BuildArguments(definition, selection);
    const HINSTANCE result = ShellExecuteW(nullptr, L"open", exePath.c_str(),
        arguments.c_str(), nullptr, SW_SHOWNORMAL);
    if (reinterpret_cast<INT_PTR>(result) <= 32)
        return HRESULT_FROM_WIN32(GetLastError());

    return S_OK;
}
