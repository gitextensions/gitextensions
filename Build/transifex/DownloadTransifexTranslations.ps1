pushd $PSScriptRoot

# Download tx.exe for Python 3 from https://github.com/transifex/transifex-client/releases/latest
# or install it using pip install transifex-client

# 1. download updated plugin translations
./tx.exe pull -a --parallel --minimum-perc 95 -f -b release/3.1 -r git-extensions.plugins-master
./tx.exe pull -a --parallel --minimum-perc 95 -f -b release/3.1 -r git-extensions.ui-master

# 2. move translations to the destination folder
Get-ChildItem -Path ./ -Filter *.xlf | `
    Move-Item -Destination ../../GitUI/Translation/$($_.Name) -Force
