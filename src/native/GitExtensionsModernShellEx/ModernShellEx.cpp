#include "pch.h"

#include "ExplorerCommand.h"

using Microsoft::WRL::ComPtr;
using Microsoft::WRL::Make;

// {5D6339FB-0BB5-4EA5-AC5F-56C20C18D6B1}
// ReSharper disable once CppInconsistentNaming
static constexpr GUID CLSID_GitExtensionsModernShellEx =
{ 0x5d6339fb, 0xbb5, 0x4ea5, { 0xac, 0x5f, 0x56, 0xc2, 0xc, 0x18, 0xd6, 0xb1 } };

// ReSharper disable CppClangTidyClangDiagnosticPadded
class GitExtensionsClassFactory final
    : public Microsoft::WRL::RuntimeClass<
    Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::ClassicCom>,
    IClassFactory>
{
public:
    GitExtensionsClassFactory() = default;
    ~GitExtensionsClassFactory() = default;

    GitExtensionsClassFactory(const GitExtensionsClassFactory&) = delete;
    GitExtensionsClassFactory& operator=(const GitExtensionsClassFactory&) = delete;

    // IClassFactory
    IFACEMETHODIMP CreateInstance(
        IUnknown* pUnkOuter,
        REFIID riid,
        void** ppv) override
    {
        if (!ppv) return E_POINTER;

        *ppv = nullptr;
        if (pUnkOuter) return CLASS_E_NOAGGREGATION;

        ComPtr<GitExtensionsRootCommand> command = Make<GitExtensionsRootCommand>();
        if (!command) return E_OUTOFMEMORY;

        return command->QueryInterface(riid, ppv);
    }

    IFACEMETHODIMP LockServer(const BOOL fLock) override
    {
        auto& module = Microsoft::WRL::Module<Microsoft::WRL::InProc>::GetModule();
        if (fLock) module.IncrementObjectCount();
        else module.DecrementObjectCount();

        return S_OK;
    }
};

namespace
{
    [[nodiscard]] [[maybe_unused]] BOOL APIENTRY DllMain(
        [[maybe_unused]] const HMODULE hModule,
        DWORD const ulReasonForCall,
        [[maybe_unused]] LPVOID lpReserved)
    {
        if (ulReasonForCall == DLL_PROCESS_ATTACH)
            DisableThreadLibraryCalls(hModule);

        return TRUE;
    }
}

STDAPI DllCanUnloadNow()
{
    return Microsoft::WRL::Module<Microsoft::WRL::InProc>::GetModule().GetObjectCount() == 0
        ? S_OK
        : S_FALSE;
}

STDAPI DllGetClassObject(
    REFCLSID rclsid,
    REFIID riid,
    LPVOID* ppv)
{
    if (!ppv) return E_POINTER;

    *ppv = nullptr;
    if (IsEqualCLSID(rclsid, CLSID_GitExtensionsModernShellEx))
    {
        ComPtr<GitExtensionsClassFactory> factory = Make<GitExtensionsClassFactory>();
        if (!factory) return E_OUTOFMEMORY;

        return factory->QueryInterface(riid, ppv);
    }

    return CLASS_E_CLASSNOTAVAILABLE;
}
