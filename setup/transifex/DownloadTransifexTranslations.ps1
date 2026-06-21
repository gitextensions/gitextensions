Push-Location $PSScriptRoot

$repoRoot = Resolve-Path  "../../"
try {
    # Download tx.exe for Python 3 from https://github.com/transifex/transifex-client/releases/latest
    # or install it using pip install transifex-client

    $env:PYTHONIOENCODING='UTF-8'

    # 1. remove all existing translations
    Get-ChildItem -Path $repoRoot/src/app/GitUI/Translation/* -Filter *.xlf -Exclude English* | `
        Remove-Item -Force;

    # 2. download updated plugin translations
    ./tx.exe pull -a -f -r git-extensions.gitui-translation-english-plugins-xlf--master

    # 3. download updated translations
    ./tx.exe pull -a --minimum-perc 75 -f -r git-extensions.gitui-translation-english-xlf--master

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
        Move-Item -Destination $repoRoot/src/app/GitUI/Translation/$($_.Name) -Force

    # 6. stage deletions (step 1 + 4) and new/updated translation files (step 5)
    git -C "$repoRoot" add -A -- src/app/GitUI/Translation ':(exclude)src/app/GitUI/Translation/English*.xlf'

    # 7. replace English source files for verfication that Transifex is up to date (not to be committed)
    ./tx.exe pull --source -f -r git-extensions.gitui-translation-english-xlf--master
    ./tx.exe pull --source -f -r git-extensions.gitui-translation-english-plugins-xlf--master
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    $utf8WithBom = [System.Text.UTF8Encoding]::new($true)
    foreach ($fileName in @('English.xlf', 'English.Plugins.xlf')) {
        $filePath = Join-Path $PSScriptRoot $fileName
        $content = [System.IO.File]::ReadAllText($filePath, $utf8NoBom)
        # Transifex encodes ' and " as XML entities in source text; reverse that to reduce diff noise
        $content = $content -replace '&apos;', "'"
        $content = $content -replace '&quot;', '"'
        # Transifex omits <target /> from source files; add it back to match the repo format
        $content = $content -replace '(</source>(\r?\n)[ \t]*)\r?\n', '$1<target />$2'
        # Transifex omits encoding declaration and BOM, and joins declaration and root element on one line
        $content = $content -replace '<\?xml version="1\.0" \?><xliff', ("<?xml version=`"1.0`" encoding=`"utf-8`"?>`r`n<xliff")
        # Normalize line endings to CRLF to match the repo format
        $content = $content -replace '\r?\n', "`r`n"
        [System.IO.File]::WriteAllText($filePath, $content, $utf8WithBom)
        # Compare with the existing repo file; if different, mark as not to be committed
        $repoFilePath = "$repoRoot/src/app/GitUI/Translation/$fileName"
        if ((Get-FileHash $filePath).Hash -ne (Get-FileHash $repoFilePath).Hash) {
            $content = "DO NOT COMMIT! This is just for verification that Transifex is up to date.`r`n" + $content
            [System.IO.File]::WriteAllText($filePath, $content, $utf8WithBom)
            Write-Host("WARNING: '$fileName' from Transifex differs from the repo version. Please verify that Transifex is up to date and do not commit the file!")
        }
    }
    Move-Item -Path ./English.xlf         -Destination $repoRoot/src/app/GitUI/Translation/English.xlf         -Force
    Move-Item -Path ./English.Plugins.xlf -Destination $repoRoot/src/app/GitUI/Translation/English.Plugins.xlf -Force
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

Push-Location $repoRoot/setup/installer

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

    Get-ChildItem -Path $repoRoot/src/app/GitUI/Translation/* -Include *.xlf -Exclude English.*,*.Plugins.xlf,*pseudo* | `
        ForEach-Object {

            $componentTitle = $_.Name.Replace($_.Extension, '');
            $componentId = $componentTitle.Replace(' ', '').Replace(')', '').Replace('(', '_');

            [xml]$featureXml = @"
            <Feature Id="$componentId" Title="$componentTitle" Level="1">
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

    Get-ChildItem -Path $repoRoot/src/app/GitUI/Translation/* -Include *.xlf -Exclude *.Plugins.xlf,*pseudo* | `
        ForEach-Object {

            $language = $_.Name.Replace($_.Extension, '')

            @( "$language.gif", "$language.xlf", "$language.Plugins.xlf" ) | `
                ForEach-Object {
                    $fileName = $_;
                    $componentId = $fileName.Replace(' ', '').Replace(')', '').Replace('(', '_');

                    [xml]$componentXml = @"
                        <Component Id="$componentId" Guid="*">
                          <File Source="`$(var.ArtifactsPublishPath)\Translation\$fileName" />
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

}
finally {
    Pop-Location
}
