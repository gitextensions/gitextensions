@echo off
set PATH=..\..\Setup\tools\;%PATH%
tx.exe pull -f --pseudo -r git-extensions.plugins-master
move /y en_pseudo.xlf en_pseudo.Plugins.xlf
tx.exe pull -f --pseudo -r git-extensions.ui-master