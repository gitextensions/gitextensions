#include "pch.h"

#include "SelectionContext.h"

namespace
{
    bool IsPathDirectory(const std::wstring& path)
    {
        const DWORD attributes = GetFileAttributesW(path.c_str());
        return attributes != INVALID_FILE_ATTRIBUTES &&
            (attributes & FILE_ATTRIBUTE_DIRECTORY);
    }

    bool IsPathFile(const std::wstring& path)
    {
        const DWORD attributes = GetFileAttributesW(path.c_str());
        return attributes != INVALID_FILE_ATTRIBUTES &&
            !(attributes & FILE_ATTRIBUTE_DIRECTORY);
    }

    bool ValidWorkingDir(const std::wstring& dir)
    {
        if (dir.empty()) return false;

        auto hasGitDirectory = [](const std::wstring& root) {
            return PathFileExistsW((root + L"\\.git\\").c_str()) ||
                PathFileExistsW((root + L"\\.git").c_str());
            };

        if (hasGitDirectory(dir)) return true;

        return PathFileExistsW((dir + L"\\info\\").c_str())
            && PathFileExistsW((dir + L"\\objects\\").c_str())
            && PathFileExistsW((dir + L"\\refs\\").c_str());
    }

    bool IsValidGitDir(const std::wstring& path)
    {
        if (path.empty()) return false;

        std::wstring current = path;
        bool continueToParent = true;
        while (continueToParent)
        {
            if (ValidWorkingDir(current)) return true;

            const size_t lastSlash = current.rfind(L'\\');
            continueToParent = lastSlash != std::wstring::npos &&
                !PathIsRootW(current.c_str());

            if (continueToParent)
                current.resize(lastSlash);
        }

        return false;
    }

    std::wstring GetShellItemPath(_In_ IShellItem* item)
    {
        PWSTR buffer = nullptr;
        std::wstring result;

        if (SUCCEEDED(item->GetDisplayName(SIGDN_FILESYSPATH, &buffer)))
            result.assign(buffer);

        CoTaskMemFree(buffer);
        return result;
    }

    std::wstring GetFolderPathFromProperties(_In_ IShellItemArray* selection)
    {
        Microsoft::WRL::ComPtr<IPropertyStore> store;
        if (FAILED(selection->GetPropertyStore(GPS_DEFAULT,
            IID_PPV_ARGS(&store))) || !store) return {};

        const PROPERTYKEY* keys[] =
        {
            &PKEY_ParsingPath,
            &PKEY_ItemFolderPathDisplay,
            &PKEY_ItemPathDisplay
        };

        for (const auto* key : keys)
        {
            PROPVARIANT variant;
            PropVariantInit(&variant);
            const HRESULT hr = store->GetValue(*key, &variant);
            if (SUCCEEDED(hr) && variant.vt == VT_LPWSTR && variant.pwszVal &&
                variant.pwszVal[0] != L'\0')
            {
                std::wstring value(variant.pwszVal);
                (void)PropVariantClear(&variant);
                return value;
            }

            (void)PropVariantClear(&variant);
        }

        return {};
    }
}

SelectionContext BuildSelectionContext(
    _In_opt_ IShellItemArray* selection,
    const std::wstring& sitePath)
{
    SelectionContext context{};
    context.HasSelection = false;
    context.PrimaryIsDirectory = false;
    context.PrimaryIsFile = false;
    context.IsGitRepository = false;
    context.IsSingleSelection = false;

    DWORD count = 0;

    if (!selection)
    {
        if (!sitePath.empty())
        {
            context.Paths.push_back(sitePath);
            context.HasSelection = true;
            context.IsSingleSelection = true;
        }

        if (context.Paths.empty())
            return context;

        const std::wstring primaryPath = context.PrimaryPath();
        context.PrimaryIsDirectory = IsPathDirectory(primaryPath);
        context.PrimaryIsFile = IsPathFile(primaryPath);
        context.IsGitRepository = IsValidGitDir(primaryPath);
        return context;
    }

    if (SUCCEEDED(selection->GetCount(&count)) && count > 0)
    {
        context.HasSelection = true;
        context.IsSingleSelection = count == 1;
        context.Paths.reserve(count);

        for (DWORD index = 0; index < count; ++index)
        {
            Microsoft::WRL::ComPtr<IShellItem> item;
            if (FAILED(selection->GetItemAt(index, &item))) continue;

            auto path = GetShellItemPath(item.Get());
            if (!path.empty())
                context.Paths.push_back(std::move(path));
        }
    }

    if (context.Paths.empty())
    {
        auto folderPath = GetFolderPathFromProperties(selection);

        if (!folderPath.empty())
        {
            context.Paths.push_back(std::move(folderPath));
            context.HasSelection = true;
            context.IsSingleSelection = true;
        }
    }

    if (context.Paths.empty())
    {
        context.HasSelection = false;
        context.IsSingleSelection = false;
        return context;
    }

    const std::wstring primaryPath = context.PrimaryPath();
    context.PrimaryIsDirectory = IsPathDirectory(primaryPath);
    context.PrimaryIsFile = IsPathFile(primaryPath);
    context.IsGitRepository = IsValidGitDir(primaryPath);

    return context;
}
