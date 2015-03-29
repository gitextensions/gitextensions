@echo off
..\..\Setup\tools\tx.exe pull -f --pseudo -r git-extensions.plugins-master
move /y en_pseudo.xlf en_pseudo.Plugins.xlf
..\..\Setup\tools\tx.exe pull -f --pseudo -r git-extensions.ui-master