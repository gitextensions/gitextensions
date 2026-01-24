#include "pch.h"

#include "ExplorerCommand.h"

// {5D6339FB-0BB5-4EA5-AC5F-56C20C18D6B1}
// ReSharper disable once CppInconsistentNaming
static constexpr GUID CLSID_GitExtensionsModernShellEx =
{ 0x5d6339fb, 0xbb5, 0x4ea5, { 0xac, 0x5f, 0x56, 0xc2, 0xc, 0x18, 0xd6, 0xb1 } };

// ReSharper disable CppClangTidyClangDiagnosticPadded
class GitExtensionsClassFactory final : public IClassFactory
{
public:
    GitExtensionsClassFactory() : m_ref(1)
    {
        GetDllRef().fetch_add(1);
    }

    ~GitExtensionsClassFactory()
    {
        GetDllRef().fetch_sub(1);
    }

    GitExtensionsClassFactory(
        const GitExtensionsClassFactory&) = delete;

    GitExtensionsClassFactory& operator=(
        const GitExtensionsClassFactory&) = delete;

    GitExtensionsClassFactory(
        GitExtensionsClassFactory&&) = delete;

    GitExtensionsClassFactory& operator=(
        GitExtensionsClassFactory&&) = delete;

    // IUnknown
    IFACEMETHODIMP QueryInterface(
        REFIID riid,
        void** ppv) override
    {
        if (!ppv) return E_POINTER;

        if (riid == IID_IUnknown || riid == IID_IClassFactory)
        {
            *ppv = static_cast<IClassFactory*>(this);
            AddRef();
            return S_OK;
        }

        *ppv = nullptr;
        return E_NOINTERFACE;
    }

    IFACEMETHODIMP_(ULONG) AddRef() override
    {
        return m_ref.fetch_add(1) + 1;
    }

    IFACEMETHODIMP_(ULONG) Release() override
    {
        const ULONG count = m_ref.fetch_sub(1) - 1;
        if (count == 0) delete this;

        return count;
    }

    // IClassFactory
    IFACEMETHODIMP CreateInstance(
        IUnknown* pUnkOuter,
        REFIID riid,
        void** ppv) override
    {
        if (!ppv) return E_POINTER;

        *ppv = nullptr;
        if (pUnkOuter) return CLASS_E_NOAGGREGATION;

        const auto command = new (std::nothrow) GitExtensionsRootCommand();
        if (!command) return E_OUTOFMEMORY;

        const HRESULT hr = command->QueryInterface(riid, ppv);
        command->Release();
        return hr;
    }

    IFACEMETHODIMP LockServer(const BOOL fLock) override
    {
        if (fLock) GetDllRef().fetch_add(1);
        else GetDllRef().fetch_sub(1);

        return S_OK;
    }

private:
    std::atomic_ulong m_ref;
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
    return GetDllRef().load() == 0
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
        const auto factory = new (std::nothrow) GitExtensionsClassFactory();
        if (!factory) return E_OUTOFMEMORY;

        const HRESULT hr = factory->QueryInterface(riid, ppv);
        factory->Release();
        return hr;
    }

    return CLASS_E_CLASSNOTAVAILABLE;
}
