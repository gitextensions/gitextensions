#include "pch.h"

#include "ExplorerCommand.h"

#include "SelectionContext.h"

using Microsoft::WRL::ComPtr;

namespace
{
    std::atomic_ulong g_dllRef{ 0 };

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

std::atomic_ulong& GetDllRef()
{
    return g_dllRef;
}

ExplorerCommandBase::ExplorerCommandBase(
    std::shared_ptr<const std::wstring> sitePath)
    : m_ref(1),
    m_sitePath(std::move(sitePath))
{
    GetDllRef().fetch_add(1);
}

ExplorerCommandBase::~ExplorerCommandBase()
{
    GetDllRef().fetch_sub(1);
}

ULONG ExplorerCommandBase::AddRef()
{
    return m_ref.fetch_add(1) + 1;
}

ULONG ExplorerCommandBase::Release()
{
    const ULONG count = m_ref.fetch_sub(1) - 1;

    if (count == 0)
        delete this;

    return count;
}

HRESULT ExplorerCommandBase::QueryInterface(
    REFIID riid,
    void** ppv)
{
    if (!ppv) return E_POINTER;

    if (riid == IID_IUnknown || riid == IID_IExplorerCommand)
    {
        *ppv = static_cast<IExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }

    if (riid == IID_IObjectWithSite)
    {
        *ppv = static_cast<IObjectWithSite*>(this);
        AddRef();
        return S_OK;
    }

    *ppv = nullptr;
    return E_NOINTERFACE;
}

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
    const auto flags = Definition().Flags;

    const bool disqualified =
        (HasFlag(flags, CommandFlags::RequiresGit) &&
            !selection.IsGitRepository) ||
        (HasFlag(flags, CommandFlags::HideInsideGitRepository) &&
            selection.IsGitRepository) ||
        (HasFlag(flags, CommandFlags::RequiresFolderSelection) &&
            !selection.PrimaryIsDirectory) ||
        (HasFlag(flags, CommandFlags::RequiresFileSelection) &&
            !selection.PrimaryIsFile) ||
        (HasFlag(flags, CommandFlags::SingleSelectionOnly) &&
            !selection.IsSingleSelection);

    if (disqualified) return false;

    return selection.HasSelection ||
        !HasFlag(flags, CommandFlags::RequiresGit);
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
{
}

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
{
}

GitExtensionsRootCommand::GitExtensionsRootCommand(
    std::shared_ptr<std::wstring> sitePathStorage)
    : ExplorerCommandBase(sitePathStorage),
    m_sitePathStorage(std::move(sitePathStorage))
{
    for (const auto& definition : GetCommandDefinitions())
    {
        GitExtensionsSubCommandPtr command;
        command.Attach(new (std::nothrow)
            GitExtensionsSubCommand(definition, m_sitePathStorage));

        if (command)
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

CommandRunner& GitExtensionsRootCommand::Runner()
{
    // The root command does not invoke GitExtensions.exe directly,
    // selection is handled by subcommands.
    return ExplorerCommandBase::Runner();
}

HRESULT GitExtensionsRootCommand::GetFlags(
    EXPCMDSTATE* pFlags)
{
    if (!pFlags) return E_POINTER;
    *pFlags = static_cast<EXPCMDSTATE>(
        ECS_ENABLED | ECF_HASSUBCOMMANDS);
    return S_OK;
}

HRESULT GitExtensionsRootCommand::EnumSubCommands(
    IEnumExplorerCommand** ppEnum)
{
    if (!ppEnum) return E_POINTER;

    const auto enumerator = new (std::nothrow)
        CommandEnumerator(m_subCommands);

    if (!enumerator) return E_OUTOFMEMORY;

    *ppEnum = enumerator;
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
    : m_ref(1),
    m_commands(commands),
    m_index(0)
{
    GetDllRef().fetch_add(1);
}

CommandEnumerator::~CommandEnumerator()
{
    GetDllRef().fetch_sub(1);
}

ULONG CommandEnumerator::AddRef()
{
    return m_ref.fetch_add(1) + 1;
}

ULONG CommandEnumerator::Release()
{
    const ULONG count = m_ref.fetch_sub(1) - 1;

    if (count == 0)
        delete this;

    return count;
}

HRESULT CommandEnumerator::QueryInterface(
    REFIID riid,
    void** ppv)
{
    if (!ppv) return E_POINTER;

    if (riid == IID_IUnknown || riid == IID_IEnumExplorerCommand)
    {
        *ppv = static_cast<IEnumExplorerCommand*>(this);
        AddRef();
        return S_OK;
    }

    *ppv = nullptr;
    return E_NOINTERFACE;
}

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
        if (auto& command = m_commands[m_index++])
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

    const auto clone = new (std::nothrow)
        CommandEnumerator(m_commands);

    if (!clone) return E_OUTOFMEMORY;

    clone->m_index = m_index;
    *ppenum = clone;
    return S_OK;
}
