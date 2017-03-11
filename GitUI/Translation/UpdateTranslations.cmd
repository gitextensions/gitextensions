@echo off
REM Download tx.exe for Python 3 from https://github.com/transifex/transifex-client/releases/latest
REM or install it using pip install transifex-client
set PATH=..\..\Setup\tools\;%PATH%
tx.exe pull -f -r git-extensions.plugins-master
tx.exe pull -f --pseudo -r git-extensions.plugins-master
move /y en_pseudo.xlf en_pseudo.Plugins.xlf
tx.exe pull -f -r git-extensions.ui-master
tx.exe pull -f --pseudo -r git-extensions.ui-master
