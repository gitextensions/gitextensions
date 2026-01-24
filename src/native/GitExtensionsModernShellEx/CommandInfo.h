#pragma once

#include "pch.h"
#include "resource.h"

enum class GitExCommand : std::uint8_t
{
    AddFiles,
    ApplyPatch,
    Browse,
    CreateBranch,
    CheckoutBranch,
    CheckoutRevision,
    Clone,
    Commit,
    CreateRepository,
    DiffTool,
    FileHistory,
    Pull,
    Push,
    ResetFileChanges,
    Revert,
    Settings,
    Stash,
    ViewDiff,
};

enum class CommandFlags : uint8_t
{
    None = 0,
    RequiresGit = 1u << 0,
    HideInsideGitRepository = 1u << 1,
    RequiresFolderSelection = 1u << 2,
    RequiresFileSelection = 1u << 3,
    SingleSelectionOnly = 1u << 4,
    UsesAllSelections = 1u << 5,
};

constexpr CommandFlags operator|(
    CommandFlags a,
    CommandFlags b)
{
    return static_cast<CommandFlags>(
        static_cast<uint8_t>(a) | static_cast<uint8_t>(b));
}

constexpr CommandFlags& operator|=(
    CommandFlags& a,
    const CommandFlags b)
{
    return a = a | b;
}

constexpr bool HasFlag(
    CommandFlags value,
    CommandFlags flag)
{
    return (static_cast<uint8_t>(value) &
        static_cast<uint8_t>(flag)) != 0;
}

struct CommandDefinition
{
    GitExCommand Id;
    const wchar_t* Verb;  // NOLINT(clang-diagnostic-padded)
    const wchar_t* Title;
    CommandFlags Flags;
    int IconResourceId;  // NOLINT(clang-diagnostic-padded)
};

inline const std::vector<CommandDefinition>& GetCommandDefinitions()
{
    constexpr CommandFlags requiresGitAndSingleSelection =
        CommandFlags::RequiresGit |
        CommandFlags::SingleSelectionOnly;

    constexpr CommandFlags requiresGitAndFolderSelection =
        CommandFlags::RequiresGit |
        CommandFlags::RequiresFolderSelection;

    constexpr CommandFlags requiresGitAndUsesAllSelections =
        CommandFlags::RequiresGit |
        CommandFlags::UsesAllSelections;

    // ReSharper disable StringLiteralTypo
    static const std::vector<CommandDefinition> Definitions
    {
        {
            GitExCommand::Clone,
            L"clone",
            L"Clone...",
            CommandFlags::HideInsideGitRepository,
            IDI_ICONCLONEREPOGIT
        },
        {
            GitExCommand::CreateRepository,
            L"init",
            L"Create new repository...",
            CommandFlags::HideInsideGitRepository,
            IDI_ICONCREATEREPOSITORY
        },
        {
            GitExCommand::Browse,
            L"browse",
            L"Open repository",
            CommandFlags::RequiresGit,
            IDI_ICONBROWSEFILEEXPLORER
        },
        {
            GitExCommand::Commit,
            L"commit",
            L"Commit...",
            requiresGitAndFolderSelection,
            IDI_ICONCOMMIT
        },
        {
            GitExCommand::Pull,
            L"pull",
            L"Pull...",
            requiresGitAndFolderSelection,
            IDI_ICONPULL
        },
        {
            GitExCommand::Push,
            L"push",
            L"Push...",
            requiresGitAndFolderSelection,
            IDI_ICONPUSH
        },
        {
            GitExCommand::Stash,
            L"stash",
            L"View stash",
            requiresGitAndFolderSelection,
            IDI_ICONSTASH
        },
        {
            GitExCommand::ViewDiff,
            L"viewdiff",
            L"View changes",
            requiresGitAndFolderSelection,
            IDI_ICONVIEWCHANGES
        },
        {
            GitExCommand::CheckoutBranch,
            L"checkoutbranch",
            L"Checkout branch...",
            requiresGitAndFolderSelection,
            IDI_ICONBRANCHCHECKOUT
        },
        {
            GitExCommand::CheckoutRevision,
            L"checkoutrevision",
            L"Checkout revision...",
            requiresGitAndFolderSelection,
            IDI_ICONREVISIONCHECKOUT
        },
        {
            GitExCommand::CreateBranch,
            L"branch",
            L"Create branch...",
            requiresGitAndFolderSelection,
            IDI_ICONBRANCHCREATE
        },
        {
            GitExCommand::DiffTool,
            L"difftool",
            L"Open with difftool",
            requiresGitAndSingleSelection,
            IDI_ICONVIEWCHANGES
        },
        {
            GitExCommand::FileHistory,
            L"filehistory",
            L"File history",
            requiresGitAndSingleSelection,
            IDI_ICONFILEHISTORY
        },
        {
            GitExCommand::ResetFileChanges,
            L"reset",
            L"Reset file changes...",
            requiresGitAndUsesAllSelections,
            IDI_ICONTRESETFILETO
        },
        {
            GitExCommand::AddFiles,
            L"addfiles",
            L"Add files...",
            requiresGitAndUsesAllSelections,
            IDI_ICONADDED
        },
        {
            GitExCommand::ApplyPatch,
            L"applypatch",
            L"Apply patch...",
            requiresGitAndSingleSelection,
            IDI_ICONPATCHAPPLY
        },
        {
            GitExCommand::Settings,
            L"settings",
            L"Settings",
            CommandFlags::None,
            IDI_ICONSETTINGS
        },
    };
    // ReSharper restore StringLiteralTypo

    return Definitions;
}
