#pragma once

#include "pch.h"

#include "CommandInfo.h"
#include "CommandRunner.h"
#include "SelectionContext.h"

#include <memory>

std::atomic_ulong& GetDllRef();

class ExplorerCommandBase
    : public IExplorerCommand,
    public IObjectWithSite
{
public:
    explicit ExplorerCommandBase(
        std::shared_ptr<const std::wstring> sitePath);
    virtual ~ExplorerCommandBase();

    // IUnknown
    IFACEMETHODIMP QueryInterface(
        REFIID riid,
        void** ppv) override;

    IFACEMETHODIMP_(ULONG) AddRef() override;
    IFACEMETHODIMP_(ULONG) Release() override;

    // IExplorerCommand
    IFACEMETHODIMP GetTitle(
        IShellItemArray* psiItemArray,
        LPWSTR* ppszName) override;

    IFACEMETHODIMP GetIcon(
        IShellItemArray* psiItemArray,
        LPWSTR* ppszIcon) override;

    IFACEMETHODIMP GetToolTip(
        IShellItemArray* psiItemArray,
        LPWSTR* ppszInfoTip) override;

    IFACEMETHODIMP GetCanonicalName(
        GUID* pGuidCommandName) override;

    IFACEMETHODIMP GetState(
        IShellItemArray* psiItemArray,
        BOOL fOkToBeSlow,
        EXPCMDSTATE* pCmdState) override;

    IFACEMETHODIMP Invoke(
        IShellItemArray* psiItemArray,
        IBindCtx* pbc) override;

    IFACEMETHODIMP GetFlags(
        EXPCMDSTATE* pFlags) override;

    IFACEMETHODIMP EnumSubCommands(
        IEnumExplorerCommand** ppEnum) override;

    // IObjectWithSite
    IFACEMETHODIMP SetSite(
        IUnknown* site) override;

    IFACEMETHODIMP GetSite(
        REFIID riid,
        void** ppv) override;

    virtual bool ShouldDisplay(
        const SelectionContext& selection) const;

    virtual GitExCommand CommandId() const = 0;

protected:
    virtual const CommandDefinition& Definition() const = 0;
    virtual CommandRunner& Runner();

    const std::wstring& SitePath() const;

private:
    std::atomic_ulong m_ref;
    CommandRunner m_runner;
    Microsoft::WRL::ComPtr<IUnknown> m_site;
    std::shared_ptr<const std::wstring> m_sitePath;
};

class GitExtensionsSubCommand final
    : public ExplorerCommandBase
{
public:
    explicit GitExtensionsSubCommand(
        const CommandDefinition& definition,
        std::shared_ptr<const std::wstring> sitePath);

    ~GitExtensionsSubCommand() override = default;

    GitExCommand CommandId() const override;

protected:
    const CommandDefinition& Definition() const override;

private:
    CommandDefinition m_definition;
};

typedef
Microsoft::WRL::ComPtr<GitExtensionsSubCommand>
GitExtensionsSubCommandPtr;

class GitExtensionsRootCommand final
    : public ExplorerCommandBase
{
public:
    GitExtensionsRootCommand();
    ~GitExtensionsRootCommand() override = default;

    GitExCommand CommandId() const override;

    bool ShouldDisplay(
        const SelectionContext& selection) const override;

    IFACEMETHODIMP Invoke(
        IShellItemArray* psiItemArray,
        IBindCtx* pbc) override;

    IFACEMETHODIMP GetFlags(
        EXPCMDSTATE* pFlags) override;

    IFACEMETHODIMP EnumSubCommands(
        IEnumExplorerCommand** ppEnum) override;

    IFACEMETHODIMP SetSite(
        IUnknown* site) override;

protected:
    const CommandDefinition& Definition() const override;
    CommandRunner& Runner() override;

private:
    explicit GitExtensionsRootCommand(
        std::shared_ptr<std::wstring> sitePathStorage);

    std::vector<GitExtensionsSubCommandPtr> m_subCommands;
    std::shared_ptr<std::wstring> m_sitePathStorage;
};

class CommandEnumerator final
    : public IEnumExplorerCommand
{
public:
    explicit CommandEnumerator(
        const std::vector<GitExtensionsSubCommandPtr>& commands);

    ~CommandEnumerator();

    // IUnknown
    IFACEMETHODIMP QueryInterface(
        REFIID riid,
        void** ppv) override;

    IFACEMETHODIMP_(ULONG) AddRef() override;
    IFACEMETHODIMP_(ULONG) Release() override;

    // IEnumExplorerCommand
    IFACEMETHODIMP Next(
        ULONG celt,
        IExplorerCommand** pUICommand,
        ULONG* pceltFetched) override;

    IFACEMETHODIMP Skip(
        ULONG celt) override;

    IFACEMETHODIMP Reset() override;

    IFACEMETHODIMP Clone(
        IEnumExplorerCommand** ppenum) override;

private:
    std::atomic_ulong m_ref;
    std::vector<GitExtensionsSubCommandPtr> m_commands;
    size_t m_index;
};
