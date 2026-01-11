#pragma once

#include "pch.h"

#include "CommandInfo.h"
#include "SelectionContext.h"

class CommandRunner
{
public:
    [[nodiscard]] static HRESULT Run(
        const CommandDefinition& definition,
        const SelectionContext& selection);

    static std::wstring ResolveInstallRoot();

private:
    static std::wstring ReadRegistryString(
        HKEY root,
        const std::wstring& subKey,
        const std::wstring& valueName);

    static std::wstring BuildArguments(
        const CommandDefinition& definition,
        const SelectionContext& selection);
};
