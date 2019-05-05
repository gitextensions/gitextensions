pushd $PSScriptRoot

# Download tx.exe for Python 3 from https://github.com/transifex/transifex-client/releases/latest
# or install it using pip install transifex-client

./tx.exe pull -a --parallel --minimum-perc 95 -f -b release/3.1
./tx.exe pull -a --parallel --minimum-perc 95 -f -b release/3.1 --pseudo
