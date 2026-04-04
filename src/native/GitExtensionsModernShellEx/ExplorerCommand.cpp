#include "pch.h"

#include "ExplorerCommand.h"
#include "SelectionContext.h"
#include <type_traits>

using Microsoft::WRL::ComPtr;
using Microsoft::WRL::Make;

namespace
{
    std::wstring ExtractSitePath(
        IUnknown* site)
    {
        if (!site) return {};

        ComPtr<IServiceProvider> serviceProvider;
        if (FAILED(site->QueryInterface(IID_PPV_ARGS(&serviceProvider))) ||
            !serviceProvider) return {};

        ComPtr<IFolderView> folderView;
        if (FAILED(serviceProvider->QueryService(SID_SFolderView,
            IID_PPV_ARGS(&folderView))) || !folderView) return {};

        ComPtr<IShellItem> folderItem;
        if (FAILED(folderView->GetFolder(IID_PPV_ARGS(&folderItem))) ||
            !folderItem) return {};

        PWSTR buffer = nullptr;
        std::wstring result;
        if (SUCCEEDED(folderItem->GetDisplayName(
            SIGDN_FILESYSPATH, &buffer)) &&
            buffer && buffer[0] != L'\0')
            result.assign(buffer);

        CoTaskMemFree(buffer);

        return result;
    }
}

ExplorerCommandBase::ExplorerCommandBase(
    std::shared_ptr<const std::wstring> sitePath)
    : m_sitePath(std::move(sitePath))
{}

HRESULT ExplorerCommandBase::GetTitle(
    [[maybe_unused]] IShellItemArray* psiItemArray,
    LPWSTR* ppszName)
{
    if (!ppszName) return E_POINTER;
    return SHStrDupW(Definition().Title, ppszName);
}

HRESULT ExplorerCommandBase::GetIcon(
    IShellItemArray* psiItemArray,
    LPWSTR* ppszIcon)
{
    UNREFERENCED_PARAMETER(psiItemArray);
    if (!ppszIcon) return E_POINTER;

    *ppszIcon = nullptr;

    const auto& definition = Definition();
    if (definition.IconResourceId <= 0) return E_NOTIMPL;

    const std::wstring installRoot = CommandRunner::ResolveInstallRoot();
    if (installRoot.empty()) return E_NOTIMPL;

    std::wstring iconPath = installRoot;
    if (!iconPath.empty() && iconPath.back() != L'\\')
        iconPath.push_back(L'\\');

    iconPath.append(L"GitExtensionsModernShellEx.dll");

    std::wstring iconReference = iconPath;
    iconReference.append(L",-");
    iconReference.append(std::to_wstring(definition.IconResourceId));

    return SUCCEEDED(SHStrDupW(iconReference.c_str(), ppszIcon))
        ? S_OK
        : E_NOTIMPL;
}

HRESULT ExplorerCommandBase::GetToolTip(
    [[maybe_unused]] IShellItemArray* psiItemArray,
    LPWSTR* ppszInfoTip)
{
    if (!ppszInfoTip) return E_POINTER;
    return SHStrDupW(Definition().Title, ppszInfoTip);
}

HRESULT ExplorerCommandBase::GetCanonicalName(
    GUID* pGuidCommandName)
{
    if (!pGuidCommandName) return E_POINTER;
    *pGuidCommandName = GUID_NULL;
    return S_OK;
}

HRESULT ExplorerCommandBase::GetState(
    IShellItemArray* psiItemArray,
    [[maybe_unused]] BOOL fOkToBeSlow,
    EXPCMDSTATE* pCmdState)
{
    if (!pCmdState) return E_POINTER;

    const auto selection =
        BuildSelectionContext(psiItemArray, SitePath());

    *pCmdState = ShouldDisplay(selection)
        ? ECS_ENABLED
        : ECS_HIDDEN;

    return S_OK;
}

HRESULT ExplorerCommandBase::Invoke(
    IShellItemArray* psiItemArray,
    [[maybe_unused]] IBindCtx* pbc)
{
    const auto selection =
        BuildSelectionContext(psiItemArray, SitePath());

    if (!ShouldDisplay(selection)) return E_FAIL;

    return CommandRunner::Run(Definition(), selection);
}

HRESULT ExplorerCommandBase::GetFlags(
    EXPCMDSTATE* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = ECS_ENABLED;
    return S_OK;
}

HRESULT ExplorerCommandBase::EnumSubCommands(
    IEnumExplorerCommand** ppEnum)
{
    if (ppEnum) *ppEnum = nullptr;
    return S_FALSE;
}

HRESULT ExplorerCommandBase::SetSite(
    IUnknown* site)
{
    m_site = site;
    return S_OK;
}

HRESULT ExplorerCommandBase::GetSite(
    REFIID riid,
    void** ppv)
{
    if (!ppv) return E_POINTER;
    *ppv = nullptr;

    return m_site
        ? m_site->QueryInterface(riid, ppv)
        : E_FAIL;
}

bool ExplorerCommandBase::ShouldDisplay(
    const SelectionContext& selection) const
{
    using enum CommandFlags;
    const auto flags = Definition().Flags;

    if (const bool disqualified =
        (HasFlag(flags, RequiresGit) &&
            !selection.IsGitRepository) ||
        (HasFlag(flags, HideInsideGitRepository) &&
            selection.IsGitRepository) ||
        (HasFlag(flags, RequiresFolderSelection) &&
            !selection.PrimaryIsDirectory) ||
        (HasFlag(flags, RequiresFileSelection) &&
            !selection.PrimaryIsFile) ||
        (HasFlag(flags, SingleSelectionOnly) &&
            !selection.IsSingleSelection);
        disqualified)
        return false;

    return selection.HasSelection ||
        !HasFlag(flags, RequiresGit);
}

CommandRunner& ExplorerCommandBase::Runner()
{
    return m_runner;
}

const std::wstring& ExplorerCommandBase::SitePath() const
{
    static const std::wstring EmptyPath;
    return m_sitePath ? *m_sitePath : EmptyPath;
}

GitExtensionsSubCommand::GitExtensionsSubCommand(
    const CommandDefinition& definition,
    std::shared_ptr<const std::wstring> sitePath)
    : ExplorerCommandBase(std::move(sitePath)),
    m_definition(definition)
{}

GitExCommand GitExtensionsSubCommand::CommandId() const
{
    return m_definition.Id;
}

const CommandDefinition& GitExtensionsSubCommand::Definition() const
{
    return m_definition;
}

GitExtensionsRootCommand::GitExtensionsRootCommand()
    : GitExtensionsRootCommand(std::make_shared<std::wstring>())
{}

GitExtensionsRootCommand::GitExtensionsRootCommand(
    std::shared_ptr<std::wstring> sitePathStorage)
    : ExplorerCommandBase(sitePathStorage),
    m_sitePathStorage(std::move(sitePathStorage))
{
    for (const auto& definition : GetCommandDefinitions())
    {
        if (GitExtensionsSubCommandPtr command =
            Make<GitExtensionsSubCommand>(definition, m_sitePathStorage))
            m_subCommands.push_back(command);
    }
}

GitExCommand GitExtensionsRootCommand::CommandId() const
{
    return GitExCommand::Browse;
}

HRESULT GitExtensionsRootCommand::Invoke(
    [[maybe_unused]] IShellItemArray* psiItemArray,
    [[maybe_unused]] IBindCtx* pbc)
{
    // The root command only exposes sub-commands.
    return S_OK;
}

bool GitExtensionsRootCommand::ShouldDisplay(
    const SelectionContext& selection) const
{
    return std::any_of(
        m_subCommands.begin(),
        m_subCommands.end(),
        [&](const auto& cmd)
        {
            return cmd->ShouldDisplay(selection);
        });
}

const CommandDefinition& GitExtensionsRootCommand::Definition() const
{
    // ReSharper disable StringLiteralTypo
    static constexpr CommandDefinition RootDefinition
    {
        GitExCommand::Browse,
        L"gitextensions",
        L"Git Extensions",
        CommandFlags::None,
        IDI_GITEXTENSIONS
    };
    // ReSharper restore StringLiteralTypo

    return RootDefinition;
}

HRESULT GitExtensionsRootCommand::GetFlags(
    EXPCMDSTATE* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = static_cast<EXPCMDSTATE>(
        static_cast<int>(ECS_ENABLED) | static_cast<int>(ECF_HASSUBCOMMANDS));
    return S_OK;
}

HRESULT GitExtensionsRootCommand::EnumSubCommands(
    IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;

    ComPtr<CommandEnumerator> enumerator = Make<CommandEnumerator>(m_subCommands);
    if (!enumerator) return E_OUTOFMEMORY;

    *ppEnum = enumerator.Detach();
    return S_OK;
}

HRESULT GitExtensionsRootCommand::SetSite(
    IUnknown* site)
{
    const auto result = ExplorerCommandBase::SetSite(site);
    if (!m_sitePathStorage) return result;

    m_sitePathStorage->clear();
    if (site)
        *m_sitePathStorage = ExtractSitePath(site);

    return result;
}

CommandEnumerator::CommandEnumerator(
    const std::vector<GitExtensionsSubCommandPtr>& commands)
    : m_commands(commands)
{}

HRESULT CommandEnumerator::Next(
    const ULONG celt,
    // ReSharper disable once CppInconsistentNaming
    IExplorerCommand** pUICommand,
    ULONG* pceltFetched)
{
    if (!pUICommand) return E_POINTER;

    ULONG fetched = 0;
    while (fetched < celt && m_index < m_commands.size())
    {
        const auto& command = m_commands[m_index++];
        if (command)
        {
            command->AddRef();
            pUICommand[fetched] = command.Get();
            ++fetched;
        }
    }

    if (pceltFetched)
        *pceltFetched = fetched;

    return fetched == celt
        ? S_OK
        : S_FALSE;
}

HRESULT CommandEnumerator::Skip(
    const ULONG celt)
{
    m_index = std::min(m_commands.size(),
        m_index + static_cast<size_t>(celt));

    return m_index < m_commands.size()
        ? S_OK
        : S_FALSE;
}

HRESULT CommandEnumerator::Reset()
{
    m_index = 0;
    return S_OK;
}

HRESULT CommandEnumerator::Clone(
    IEnumExplorerCommand** ppenum)
{
    if (!ppenum) return E_POINTER;

    ComPtr<CommandEnumerator> clone = Make<CommandEnumerator>(m_commands);
    if (!clone) return E_OUTOFMEMORY;

    clone->m_index = m_index;
    *ppenum = clone.Detach();
    return S_OK;
}
