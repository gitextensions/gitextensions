#pragma once

#include "pch.h"

struct SelectionContext
{
    std::vector<std::wstring> Paths;
    bool HasSelection;
    bool PrimaryIsDirectory;
    bool PrimaryIsFile;
    bool IsGitRepository;
    bool IsSingleSelection;

    std::wstring PrimaryPath() const
    {
        return Paths.empty()
            ? std::wstring()
            : Paths.front();
    }
};

SelectionContext BuildSelectionContext(
    _In_opt_ IShellItemArray* selection,
    const std::wstring& sitePath);
