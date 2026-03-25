#pragma once

#include "pch.h"

#include "CommandInfo.h"
#include "CommandRunner.h"
#include "SelectionContext.h"

#include <memory>

class ExplorerCommandBase
    : public Microsoft::WRL::RuntimeClass<
        Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::ClassicCom>,
        IExplorerCommand,
        IObjectWithSite>
{
public:
    explicit ExplorerCommandBase(
        std::shared_ptr<const std::wstring> sitePath);

    virtual ~ExplorerCommandBase() = default;

    ExplorerCommandBase(const ExplorerCommandBase&) = delete;
    ExplorerCommandBase& operator=(const ExplorerCommandBase&) = delete;

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
    [[msvc::no_unique_address]] CommandRunner m_runner;
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

    GitExtensionsSubCommand(const GitExtensionsSubCommand&) = delete;
    GitExtensionsSubCommand& operator=(const GitExtensionsSubCommand&) = delete;

    GitExCommand CommandId() const override;

protected:
    const CommandDefinition& Definition() const override;

private:
    CommandDefinition m_definition;
};

using GitExtensionsSubCommandPtr = Microsoft::WRL::ComPtr<GitExtensionsSubCommand>;

class GitExtensionsRootCommand final
    : public ExplorerCommandBase
{
public:
    GitExtensionsRootCommand();
    ~GitExtensionsRootCommand() override = default;

    GitExtensionsRootCommand(const GitExtensionsRootCommand&) = delete;
    GitExtensionsRootCommand& operator=(const GitExtensionsRootCommand&) = delete;

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

private:
    explicit GitExtensionsRootCommand(
        std::shared_ptr<std::wstring> sitePathStorage);

    std::vector<GitExtensionsSubCommandPtr> m_subCommands;
    std::shared_ptr<std::wstring> m_sitePathStorage;
};

class CommandEnumerator final
    : public Microsoft::WRL::RuntimeClass<
        Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::ClassicCom>,
        IEnumExplorerCommand>
{
public:
    explicit CommandEnumerator(
        const std::vector<GitExtensionsSubCommandPtr>& commands);

    ~CommandEnumerator() = default;

    CommandEnumerator(const CommandEnumerator&) = delete;
    CommandEnumerator& operator=(const CommandEnumerator&) = delete;

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
    std::vector<GitExtensionsSubCommandPtr> m_commands;
    size_t m_index{ 0 };
};
