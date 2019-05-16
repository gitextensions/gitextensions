pushd $PSScriptRoot

# Download tx.exe for Python 3 from https://github.com/transifex/transifex-client/releases/latest
# or install it using pip install transifex-client

# 1. remove all existing translations
Get-ChildItem -Path ../../GitUI/Translation/* -Filter *.xlf -Exclude English* | `
    Remove-Item -Force;

# 2. download updated plugin translations
./tx.exe pull -a --parallel --minimum-perc 75 -f -r git-extensions.plugins-master
./tx.exe pull -a --parallel --minimum-perc 75 -f -r git-extensions.plugins-master --pseudo

# 3. download updated translations
./tx.exe pull -a --parallel --minimum-perc 75 -f -r git-extensions.ui-master
./tx.exe pull -a --parallel --minimum-perc 75 -f -r git-extensions.ui-master --pseudo

# 4. remove plugins translations without a main translation companion
Get-ChildItem -Path ./* -Include *.Plugins.xlf  -Exclude '*pseudo*' | `
    Where-Object {
        -not (Test-Path $_.Name.Replace('.Plugins.xlf', '.xlf'))
    } | `
    ForEach-Object {
        Write-Host "$($_.Name) does not have main UI translation available. Deleting..."
        Remove-Item $_ -Force;
   };

# 5. move translations to the destination folder
Get-ChildItem -Path ./* -Filter *.xlf | `
    Move-Item -Destination ../../GitUI/Translation/$($_.Name) -Force
