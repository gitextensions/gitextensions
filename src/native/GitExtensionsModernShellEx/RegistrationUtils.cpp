#include "pch.h"

using namespace winrt;
using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Management::Deployment;

namespace
{
    std::wstring g_lastRegistrationError;
    std::mutex g_registrationErrorMutex;

    void SetLastErrorMessage(
        const std::wstring& msg)
    {
        std::scoped_lock lock(g_registrationErrorMutex);
        g_lastRegistrationError = msg;
    }

    void SetLastErrorFromException(
        const winrt::hresult_error& e,
        const wchar_t* prefix)
    {
        std::wstring msg = prefix ? prefix : L"";
        if (!msg.empty()) msg += L": ";
        msg += e.message().c_str();

        wchar_t hrBuf[32];
        const auto errorCode = static_cast<unsigned int>(e.code());
        swprintf_s(hrBuf, L" (0x%08X)", errorCode);
        msg += hrBuf;

        SetLastErrorMessage(msg);
    }

    void Initialize()
    {
        try
        {
            init_apartment(apartment_type::multi_threaded);
        }
        catch (winrt::hresult_error const& e)
        {
            if (e.code() != RPC_E_CHANGED_MODE)
                throw;

            // Already initialized as STA on this thread; proceed or fallback.
        }
    }
}

STDAPI_(BOOL) IsExtensionRegistered(
    const wchar_t* packageFamilyName)
{
    try
    {
        Initialize();

        if (!packageFamilyName || !*packageFamilyName)
        {
            SetLastErrorMessage(L"PackageFamilyName is null or empty.");
            return FALSE;
        }

        const PackageManager pm;
        const auto packages = pm.FindPackagesForUser(L"", packageFamilyName);
        const auto it = packages.First();

        return it.HasCurrent()
            ? TRUE
            : FALSE;
    }
    catch (const winrt::hresult_error& e)
    {
        SetLastErrorFromException(e, L"IsExtensionRegistered failed");
        return FALSE;
    }
    catch (...)
    {
        SetLastErrorMessage(L"IsExtensionRegistered failed: unknown error.");
        return FALSE;
    }
}

STDAPI RegisterExtension(
    const wchar_t* packagePath,
    const wchar_t* externalLocation)
{
    try
    {
        Initialize();

        if (!packagePath || !*packagePath)
        {
            SetLastErrorMessage(L"packagePath is null or empty.");
            return E_INVALIDARG;
        }

        if (!externalLocation || !*externalLocation)
        {
            SetLastErrorMessage(L"externalLocation is null or empty.");
            return E_INVALIDARG;
        }

        const PackageManager pm;
        const AddPackageOptions options;
        options.ExternalLocationUri(Uri(externalLocation));

        const Uri packageUri(packagePath);
        const auto op = pm.AddPackageByUriAsync(packageUri, options);
        (void)op.get();

        // Some WinRT async failures surface via exception; this checks status too.
        if (op.Status() == AsyncStatus::Error)
        {
            // AsyncStatus::Error often pairs with op.ErrorCode()
            const auto hr = op.ErrorCode();
            SetLastErrorMessage(L"RegisterExtension failed (AsyncStatus::Error).");
            return hr;
        }

        SetLastErrorMessage(L"");
        return S_OK;
    }
    catch (const winrt::hresult_error& e)
    {
        SetLastErrorFromException(e, L"RegisterExtension failed");
        return e.code();
    }
    catch (...)
    {
        SetLastErrorMessage(L"RegisterExtension failed: unknown error.");
        return E_FAIL;
    }
}

STDAPI UnregisterExtension(
    const wchar_t* packageFamilyName)
{
    try
    {
        Initialize();

        if (!packageFamilyName || !*packageFamilyName)
        {
            SetLastErrorMessage(L"PackageFamilyName is null/empty.");
            return E_INVALIDARG;
        }

        const PackageManager pm;
        const auto packages = pm.FindPackagesForUser(L"", packageFamilyName);

        for (auto const& pkg : packages)
        {
            auto fullName = pkg.Id().FullName();
            auto op = pm.RemovePackageAsync(fullName);
            (void)op.get();

            if (op.Status() == AsyncStatus::Error)
            {
                const auto hr = op.ErrorCode();
                SetLastErrorMessage(L"UnregisterExtension failed (AsyncStatus::Error).");
                return hr;
            }
        }

        SetLastErrorMessage(L"");
        return S_OK;
    }
    catch (const winrt::hresult_error& e)
    {
        SetLastErrorFromException(e, L"UnregisterExtension failed");
        return e.code();
    }
    catch (...)
    {
        SetLastErrorMessage(L"UnregisterExtension failed: unknown error.");
        return E_FAIL;
    }
}

STDAPI_(const wchar_t*) GetLastRegistrationError()
{
    std::scoped_lock lock(g_registrationErrorMutex);
    return g_lastRegistrationError.c_str();
}
