Push-Location $PSScriptRoot
try {
    # Download tx.exe for Python 3 from https://github.com/transifex/transifex-client/releases/latest
    # or install it using pip install transifex-client

    # 1. remove all existing translations
    Get-ChildItem -Path ../../GitUI/Translation/* -Filter *.xlf -Exclude English* | `
        Remove-Item -Force;

    # 2. download updated plugin translations
    ./tx.exe pull -a --parallel --minimum-perc 95 -f -r git-extensions.plugins-master
    ./tx.exe pull -a --parallel --minimum-perc 95 -f -r git-extensions.plugins-master --pseudo

    # 3. download updated translations
    ./tx.exe pull -a --parallel --minimum-perc 95 -f -r git-extensions.ui-master
    ./tx.exe pull -a --parallel --minimum-perc 95 -f -r git-extensions.ui-master --pseudo

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
}
finally {
    Pop-Location
}

# update setup references

function Add-Attribute {
    [CmdletBinding()]
    Param(
        [System.Xml.XmlNode] $node,
        [string] $attributeName,
        [string] $attributeValue
    )

    $attrib = $node.OwnerDocument.CreateAttribute($attributeName);
    $attrib.Value = $attributeValue;
    $node.Attributes.Append($attrib) | Out-Null
}

Push-Location $PSScriptRoot/../../Setup

try {
    [xml]$productWxs = Get-Content Product.wxs

    $directoryRef = $productWxs.Wix.Product.DirectoryRef | ? Id -eq "TranslationsDir"
    if (!$directoryRef) {
        throw '/Wix/Product/DirectoryRef[Id="TranslationsDir"] is missing!'
    }

    $feature = ($productWxs.Wix.Product.Feature | ? Id -eq "GitExtensions").Feature | ? Id -eq "Translation"
    if (!$feature) {
        throw '/Wix/Product/Feature[Id="GitExtensions"]/Feature[Id="Translation"] is missing!'
    }



    $feature.RemoveAll();
    # RemoveAll removes all children and attributes
    # restore the attribute
    Add-Attribute -node $feature -attributeName 'Id' -attributeValue 'Translation';
    Add-Attribute -node $feature -attributeName 'Title' -attributeValue 'Translations';
    Add-Attribute -node $feature -attributeName 'Level' -attributeValue '1';

    Get-ChildItem -Path ../GitUI/Translation/* -Include *.xlf -Exclude English.*,*.Plugins.xlf,*pseudo* | `
        ForEach-Object {

            $componentId = $_.Name.Replace($_.Extension, '').Replace(' ', '');

            [xml]$featureXml = @"
            <Feature Id="$componentId" Title="$componentId" Level="1">
              <ComponentRef Id="$componentId.xlf" />
              <ComponentRef Id="$componentId.Plugins.xlf" />
              <ComponentRef Id="$componentId.gif" />
            </Feature>
"@

            [System.Xml.XmlNode]$newFeatureNode = $featureXml.SelectSingleNode("//Feature")
            if (!$newFeatureNode) {
                throw "`$newFeatureNode is null"
            }

            #add new node AFTER the configsections node
            $importedNode = $productWxs.ImportNode($newFeatureNode, $true);
            $node = $feature.AppendChild($importedNode)
        }



    $directoryRef.RemoveAll();
    # RemoveAll removes all children and attributes
    # restore the attribute
    Add-Attribute -node $directoryRef -attributeName 'Id' -attributeValue 'TranslationsDir';

    Get-ChildItem -Path ../GitUI/Translation/* -Include *.xlf -Exclude *.Plugins.xlf,*pseudo* | `
        ForEach-Object {

            $language = $_.Name.Replace($_.Extension, '')

            @( "$language.gif", "$language.xlf", "$language.Plugins.xlf" ) | `
                ForEach-Object {
                    $fileName = $_;
                    $componentId = $fileName.Replace(' ', '');

                    [xml]$componentXml = @"
                        <Component Id="$componentId" Guid="*">
                          <File Source="..\GitExtensions\bin\`$(var.Configuration)\Translation\$fileName" />
                        </Component>
"@

                    [System.Xml.XmlNode]$componentNode = $componentXml.SelectSingleNode("//Component")
                    if (!$componentNode) {
                        throw "`$componentNode is null"
                    }

                    #add new node AFTER the configsections node
                    $importedNode = $productWxs.ImportNode($componentNode, $true);
                    $node = $directoryRef.AppendChild($importedNode)
                }
        }

    # remove unwanted 'xmlns=""' attributes
    [xml]$productWxs1 = $productWxs.OuterXml.Replace(" xmlns=`"`"", "")

    #save file
    $currentFolder = $(pwd).Path;
    $productWxs1.Save("$currentFolder\Product.wxs")


    [string[]]$content = Get-Content 'MakePortableArchive.cmd'
    
    $start = $content.IndexOf("REM --## DO NOT MODIFY --")
    $end = $content.IndexOf("REM --## DO NOT MODIFY END --")
    if ($start -lt 1 -and $end -lt $start) {
        throw 'Malformed MakePortableArchive.cmd';
    }
    
    $content1 = @();
    for ($i = 0; $i -lt $start + 1; $i++) {
        $content1 += $content[$i];
    }

    Get-ChildItem -Path ../GitUI/Translation/* -Include *.xlf -Exclude *.Plugins.xlf,*pseudo* | `
        ForEach-Object {

            $language = $_.Name.Replace($_.Extension, '')

            @( "$language.gif", "$language.xlf", "$language.Plugins.xlf" ) | `
                ForEach-Object {
                    $fileName = $_;

                    $content1 += @"
xcopy /y /i "..\GitUI\Translation\$fileName" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
"@

                }
        }

    for ($i = $end; $i -lt $content.Length; $i++) {
        $content1 += $content[$i];
    }
    
    $currentFolder = $(pwd).Path;
    $content1 | Out-File "$currentFolder\MakePortableArchive.cmd" -Encoding ascii
}
finally {
    Pop-Location
}
