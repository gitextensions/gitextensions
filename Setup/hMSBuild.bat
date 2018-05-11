@echo off

:: hMSBuild - v1.2.2.62992 [ 3ee58c3 ]
:: Copyright (c) 2017  Denis Kuzmin [ entry.reg@gmail.com ]
:: -
:: Distributed under the MIT license
:: https://github.com/3F/hMSBuild

setlocal enableDelayedExpansion


:: - - -
:: Settings by default

set vswhereByDefault=1.0.62
set vswhereCache=%temp%\hMSBuild_vswhere

set /a notamd64=0
set /a novs=0
set /a nonet=0
set /a novswhere=0
set /a nocachevswhere=0
set /a hMSBuildDebug=0
set "displayOnlyPath="
set "vswVersion="

set ERROR_SUCCESS=0
set ERROR_FILE_NOT_FOUND=2
set ERROR_PATH_NOT_FOUND=3

set "args=%* "


:: - - -
:: Help command

set _hl=%args:"=%
set _hr=%_hl%

set _hr=%_hr:-help =%
set _hr=%_hr:-h =%
set _hr=%_hr:-? =%

if not "%_hl%"=="%_hr%" goto usage
goto commands

:usage

echo.
@echo :: hMSBuild - v1.2.2.62992 [ 3ee58c3 ]
@echo Copyright (c) 2017  Denis Kuzmin [ entry.reg@gmail.com :: github.com/3F ]
echo Distributed under the MIT license
@echo https://github.com/3F/hMSBuild 
echo.
@echo.
@echo Usage: hMSBuild [args to hMSBuild] [args to msbuild.exe or GetNuTool core]
echo ------
echo.
echo Arguments:
echo ----------
echo  -novswhere             - Do not search via vswhere.
echo  -novs                  - Disable searching from Visual Studio.
echo  -nonet                 - Disable searching from .NET Framework.
echo  -vswhere-version {num} - Specific version of vswhere. Where {num}:
echo                           * Versions: 1.0.50 ...
echo                           * Keywords: 
echo                             `latest` to get latest available version; 
echo                             `local`  to use only local versions: 
echo                                      (.bat;.exe /or from +15.2.26418.1 VS-build);
echo.
echo  -nocachevswhere        - Do not cache vswhere. Use this also for reset cache.
echo  -notamd64              - To use 32bit version of found msbuild.exe if it's possible.
echo  -eng                   - Try to use english language for all build messages.
echo  -GetNuTool {args}      - Access to GetNuTool core. https://github.com/3F/GetNuTool
echo  -only-path             - Only display fullpath to found MSBuild.
echo  -debug                 - To show additional information from hMSBuild.
echo  -version               - Display version of hMSBuild.
echo  -help                  - Display this help. Aliases: -help -h -?
echo.
echo. 
echo -------- 
echo Samples:
echo -------- 
echo hMSBuild -vswhere-version 1.0.50 -notamd64 "Conari.sln" /t:Rebuild
echo hMSBuild -vswhere-version latest "Conari.sln"
echo.
echo hMSBuild -novswhere -novs -notamd64 "Conari.sln"
echo hMSBuild -novs "DllExport.sln"
echo hMSBuild vsSolutionBuildEvent.sln
echo.
echo hMSBuild -GetNuTool -unpack
echo hMSBuild -GetNuTool /p:ngpackages="Conari;regXwild"
echo.
echo "hMSBuild -novs "DllExport.sln" || goto err"
echo.
echo ---------------------
echo Possible Error Codes: ERROR_FILE_NOT_FOUND (0x2), ERROR_PATH_NOT_FOUND (0x3), ERROR_SUCCESS (0x0)
echo ---------------------
echo.

exit /B 0

:: - - -
:: Handler of user commands

:commands

call :isEmptyOrWhitespace args _is
if [!_is!]==[1] goto action

set /a idx=1 & set cmdMax=12
:loopargs

    if "!args:~0,11!"=="-GetNuTool " (
        call :popars %1 & shift
        goto gntcall
    )
    
    if "!args:~0,11!"=="-novswhere " (
        call :popars %1 & shift
        set novswhere=1
    )
    
    if "!args:~0,16!"=="-nocachevswhere " (
        call :popars %1 & shift
        set nocachevswhere=1
    )
    
    if "!args:~0,6!"=="-novs " (
        call :popars %1 & shift
        set novs=1
    )
    
    if "!args:~0,7!"=="-nonet " (
        call :popars %1 & shift
        set nonet=1
    )

    :: backward compatibility - version 1.1
    if "!args:~0,16!"=="-vswhereVersion " set _OrConditionVSWVer=1
    if "!args:~0,17!"=="-vswhere-version " set _OrConditionVSWVer=1
    if defined _OrConditionVSWVer (
        set "_OrConditionVSWVer="
        call :popars %1 & shift
        set vswVersion=%2
        echo selected new vswhere version: !vswVersion!
        call :popars %2 & shift
    )

    if "!args:~0,10!"=="-notamd64 " (
        call :popars %1 & shift
        set notamd64=1
    )
    
    if "!args:~0,5!"=="-eng " (
        call :popars %1 & shift
        chcp 437 >nul
    )

    if "!args:~0,11!"=="-only-path " (
        call :popars %1 & shift
        set displayOnlyPath=1
    )

    if "!args:~0,7!"=="-debug " (
        call :popars %1 & shift
        set hMSBuildDebug=1
    )
    
    if "!args:~0,9!"=="-version " (
        @echo hMSBuild - v1.2.2.62992 [ 3ee58c3 ]
        exit /B 0
    )
    
set /a "idx+=1"
if !idx! LSS %cmdMax% goto loopargs

goto action

:popars
set args=!!args:%1 ^=!!
call :trim args
set "args=!args! "
exit /B 0

:: - - -
:: Main logic of searching
:action

if "!nocachevswhere!"=="1" (
    call :dbgprint "resetting cache of vswhere"
    rmdir /S/Q "%vswhereCache%" 2>nul
)

if not "!novswhere!"=="1" if not "!novs!"=="1" (
    call :vswhere
    if "!ERRORLEVEL!"=="0" goto runmsbuild
)

if not "!novs!"=="1" (
    call :msbvsold
    if "!ERRORLEVEL!"=="0" goto runmsbuild
)

if not "!nonet!"=="1" (
    call :msbnetf
    if "!ERRORLEVEL!"=="0" goto runmsbuild
)

echo MSBuild tools was not found. Try to use other settings. Use key `-help` for details.
exit /B %ERROR_FILE_NOT_FOUND%

:runmsbuild

call :isEmptyOrWhitespace msbuildPath _is
if [!_is!]==[1] (
    echo Something went wrong. Use `-debug` key for details.
    exit /B %ERROR_FILE_NOT_FOUND%
)

if defined displayOnlyPath (
    echo !msbuildPath!
    exit /B 0
)
set xMSBuild="!msbuildPath!"

echo hMSBuild: !xMSBuild! 
call :dbgprint "Arguments: !args!"

!xMSBuild! !args!
exit /B %ERRORLEVEL%

:: - - -
:: Tools from VS2017+
:vswhere

call :dbgprint "trying via vswhere..."

if defined vswVersion (
    if not "!vswVersion!"=="local" (
        call :vswhereRemote
        call :vswhereBin
        exit /B !ERRORLEVEL!
    )
)

call :vswhereLocal
if "!ERRORLEVEL!"=="%ERROR_PATH_NOT_FOUND%" (
    if "!vswVersion!"=="local" (
        exit /B %ERROR_PATH_NOT_FOUND%
    )
    call :vswhereRemote
)
call :vswhereBin
exit /B !ERRORLEVEL!

:vswhereLocal

:: Only with +15.2.26418.1 VS-build
:: https://github.com/3F/hMSBuild/issues/1

if exist "%~dp0vswhere.bat" set vswbin="%~dp0vswhere" & exit /B 0
if exist "%~dp0vswhere.exe" set vswbin="%~dp0vswhere" & exit /B 0

set rlocalp=Microsoft Visual Studio\Installer
if exist "%ProgramFiles(x86)%\!rlocalp!" set vswbin="%ProgramFiles(x86)%\!rlocalp!\vswhere" & exit /B 0
if exist "%ProgramFiles%\!rlocalp!" set vswbin="%ProgramFiles%\!rlocalp!\vswhere" & exit /B 0

call :dbgprint "local vswhere is not found."
exit /B %ERROR_PATH_NOT_FOUND%

:vswhereRemote

if "!nocachevswhere!"=="1" (
    set tvswhere=%temp%\%random%%random%vswhere
) else (
    set tvswhere=%vswhereCache%
)

call :dbgprint "tvswhere: !tvswhere!"

if "!vswVersion!"=="latest" (
    set vswpkg=vswhere
) else (
    set vswpkg=vswhere/!vswVersion!
)

call :dbgprint "vswpkg: !vswpkg!"

if "!hMSBuildDebug!"=="1" (
    call :gntpoint /p:ngpackages="!vswpkg!:vswhere" /p:ngpath="!tvswhere!"
) else (
    call :gntpoint /p:ngpackages="!vswpkg!:vswhere" /p:ngpath="!tvswhere!" >nul
)
set vswbin="!tvswhere!\vswhere\tools\vswhere"

exit /B 0

:vswhereBin
call :dbgprint "vswbin: "!vswbin!""

for /f "usebackq tokens=1* delims=: " %%a in (`!vswbin! -latest -requires Microsoft.Component.MSBuild`) do (
    if /i "%%a"=="installationPath" set vspath=%%b
    if /i "%%a"=="installationVersion" set vsver=%%b
)

call :dbgprint "vspath: !vspath!"
call :dbgprint "vsver: !vsver!"

if defined tvswhere (
    if "!nocachevswhere!"=="1" (
        call :dbgprint "reset vswhere"
        rmdir /S/Q "!tvswhere!"
    )
)

if [!vsver!]==[] (
    call :dbgprint "VS2017+ was not found via vswhere"
    exit /B %ERROR_PATH_NOT_FOUND%
)

for /f "tokens=1,2 delims=." %%a in ("!vsver!") do (
    set vsver=%%a.0
)
set msbuildPath=!vspath!\MSBuild\!vsver!\Bin

call :dbgprint "found path to msbuild: !msbuildPath!"

if exist "!msbuildPath!\amd64" (
    call :dbgprint "found /amd64"
    set msbuildPath=!msbuildPath!\amd64
)
call :msbuildfind 
exit /B 0

:: - - -
:: Tools from Visual Studio - 2015, 2013, ...
:msbvsold
call :dbgprint "trying via MSBuild tools from Visual Studio - 2015, 2013, ..."

for %%v in (14.0, 12.0) do (
    call :rtools %%v Y & if [!Y!]==[1] exit /B 0
)
call :dbgprint "msbvsold: unfortunately we didn't find anything."
exit /B %ERROR_FILE_NOT_FOUND%

:: - - -
:: Tools from .NET Framework - .net 4.0, ...
:msbnetf

call :dbgprint "trying via MSBuild tools from .NET Framework - .net 4.0, ..."

for %%v in (4.0, 3.5, 2.0) do (
    call :rtools %%v Y & if [!Y!]==[1] exit /B 0
)
call :dbgprint "msbnetf: unfortunately we didn't find anything."
exit /B %ERROR_FILE_NOT_FOUND%

:rtools
call :dbgprint "checking of version: %1"
    
for /F "usebackq tokens=2* skip=2" %%a in (
    `reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\%1" /v MSBuildToolsPath 2^> nul`
) do if exist %%b (
    call :dbgprint "found: %%b"
        
    set msbuildPath=%%b
    call :msbuildfind
    set /a %2=1
    exit /B 0
)
set /a %2=0
exit /B 0

:gntcall
call :dbgprint "direct access to GetNuTool..."
call :gntpoint !args!
exit /B 0

:msbuildfind

set msbuildPath=!msbuildPath!\MSBuild.exe

if not "!notamd64!" == "1" (
    exit /B 0
)

:: 7z & amd64\msbuild - https://github.com/3F/vsSolutionBuildEvent/issues/38
set _amd=!msbuildPath:Framework64=Framework!
set _amd=!_amd:amd64=!

if exist "!_amd!" (
    call :dbgprint "Return 32bit version of MSBuild.exe because you wanted this via -notamd64"
    set msbuildPath=!_amd!
    exit /B 0
)

call :dbgprint "We know that 32bit version of MSBuild.exe is important for you, but we found only this."
exit /B 0

:: =

:dbgprint
if "!hMSBuildDebug!"=="1" (
    set msgfmt=%1
    set msgfmt=!msgfmt:~0,-1! 
    set msgfmt=!msgfmt:~1!
    echo.[%TIME% ] !msgfmt!
)
exit /B 0

:trim
:: Usage: call :trim variable
call :_v %%%1%%
set %1=%_trimv%
exit /B 0
:_v
set "_trimv=%*"
exit /B 0

:isEmptyOrWhitespace
:: Usage: call :isEmptyOrWhitespace input output(1/0)
setlocal enableDelayedExpansion
set "_v=!%1!"

if not defined _v endlocal & set /a %2=1 & exit /B 0
 
set _v=%_v: =%
set "_v= %_v%"
if [^%_v:~1,1%]==[] endlocal & set /a %2=1 & exit /B 0
 
endlocal & set /a %2=0
exit /B 0

:gntpoint
setlocal disableDelayedExpansion 

:: ========================= GetNuTool =========================

@echo off
:: GetNuTool - Executable version
:: Copyright (c) 2015-2017  Denis Kuzmin [ entry.reg@gmail.com ]
:: https://github.com/3F/GetNuTool

set gntcore=gnt.core
set tgnt="%temp%\%random%%random%%gntcore%"

set "args=%* "
set a=%args:~0,30%
set a=%a:"=%

if "%a:~0,8%"=="-unpack " goto unpack
if "%a:~0,9%"=="-msbuild " goto ufound

for %%v in (4.0, 14.0, 12.0, 3.5, 2.0) do (
    for /F "usebackq tokens=2* skip=2" %%a in (
        `reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\%%v" /v MSBuildToolsPath 2^> nul`
    ) do if exist %%b (
        set msbuildexe="%%b\MSBuild.exe"
        goto found
    )
)
echo MSBuild was not found, try: gnt -msbuild "fullpath" args 1>&2
exit /B 2

:ufound
call :popa %1
shift
set msbuildexe=%1
call :popa %1

:found
call :core
%msbuildexe% %tgnt% /nologo /p:wpath="%~dp0/" /v:m %args%
del /Q/F %tgnt%
exit /B 0

:popa
call set args=%%args:%1 ^=%%
exit /B 0

:unpack
set tgnt="%~dp0\%gntcore%"
echo Generate minified version in %tgnt% ...

:core
<nul set /P ="">%tgnt%
<nul set /P =^<!-- GetNuTool - github.com/3F/GetNuTool --^>^<!-- Copyright (c) 2015-2017  Denis Kuzmin [ entry.reg@gmail.com ] --^>^<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003"^>^<PropertyGroup^>^<ngconfig Condition="'$(ngconfig)' == ''"^>packages.config^</ngconfig^>^<ngserver Condition="'$(ngserver)' == ''"^>https://www.nuget.org/api/v2/package/^</ngserver^>^<ngpackages Condition="'$(ngpackages)' == ''"^>^</ngpackages^>^<ngpath Condition="'$(ngpath)' == ''"^>packages^</ngpath^>^</PropertyGroup^>^<Target Name="get" BeforeTargets="Build" DependsOnTargets="header"^>^<PrepareList config="$(ngconfig)" plist="$(ngpackages)" wpath="$(wpath)"^>^<Output PropertyName="plist" TaskParameter="Result"/^>^</PrepareList^>^<NGDownload plist="$(plist)" url="$(ngserver)" wpath="$(wpath)" defpath="$(ngpath)" debug="$(debug)"/^>^</Target^>^<Target Name="pack" DependsOnTargets="header"^>^<NGPack dir="$(ngin)" dout="$(ngout)" wpath="$(wpath)" vtool="$(GetNuTool)" debug="$(debug)"/^>^</Target^>^<PropertyGroup^>^<TaskCoreDllPath Condition="Exists('$(MSBuildToolsPath)\Microsoft.Build.Tasks.v$(MSBuildToolsVersion).dll')"^>$(MSBuildToolsPath)\Microsoft.Build.Tasks.v$(MSBuildToolsVersion).dll^</TaskCoreDllPath^>^<TaskCoreDllPath Condition="'$(TaskCoreDllPath)' == '' and Exists('$(MSBuildToolsPath)\Microsoft.Build.Tasks.Core.dll')"^>$(MSBuildToolsPath)\Microsoft.Build.Tasks.Core.dll^</TaskCoreDllPath^>^</PropertyGroup^>^<UsingTask TaskName="PrepareList" TaskFactory="CodeTaskFactory" AssemblyFile="$(TaskCoreDllPath)"^>^<ParameterGroup^>^<config Parame>> %tgnt%
<nul set /P =terType="System.String" Required="true"/^>^<plist ParameterType="System.String"/^>^<wpath ParameterType="System.String"/^>^<Result ParameterType="System.String" Output="true"/^>^</ParameterGroup^>^<Task^>^<Reference Include="System.Xml"/^>^<Reference Include="System.Xml.Linq"/^>^<Using Namespace="System"/^>^<Using Namespace="System.Collections.Generic"/^>^<Using Namespace="System.IO"/^>^<Using Namespace="System.Xml.Linq"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[if(!String.IsNullOrEmpty(plist)){Result=plist;return true;}var _err=Console.Error;Action^<string,Queue^<string^>^> h=delegate(string cfg,Queue^<string^> list){foreach(var pkg in XDocument.Load(cfg).Descendants("package")){var id=pkg.Attribute("id");var version=pkg.Attribute("version");var output=pkg.Attribute("output");if(id==null){_err.WriteLine("Some 'id' does not exist in '{0}'",cfg);return;}var link=id.Value;if(version!=null){link+="/"+version.Value;}if(output!=null){list.Enqueue(link+":"+output.Value);continue;}list.Enqueue(link);}};var ret=new Queue^<string^>();foreach(var cfg in config.Split('^|',';')){var lcfg=Path.Combine(wpath,cfg??"");if(File.Exists(lcfg)){h(lcfg,ret);}else{_err.WriteLine(".config '{0}' was not found.",lcfg);}}if(ret.Count ^< 1){_err.WriteLine("List of packages is empty. Use .config or /p:ngpackages=\"...\"\n");}else{Result=String.Join(";",ret.ToArray());}]]^>^</Code^>^</Task^>^</UsingTask^>^<UsingTask TaskName="NGDownload" TaskFactory="CodeTaskFactory" AssemblyFile="$(TaskCoreDllPath)"^>^<ParameterGroup^>^<plist ParameterType="System.String"/^>^<url Paramet>> %tgnt%
<nul set /P =erType="System.String" Required="true"/^>^<wpath ParameterType="System.String"/^>^<defpath ParameterType="System.String"/^>^<debug ParameterType="System.Boolean"/^>^</ParameterGroup^>^<Task^>^<Reference Include="WindowsBase"/^>^<Using Namespace="System"/^>^<Using Namespace="System.IO"/^>^<Using Namespace="System.IO.Packaging"/^>^<Using Namespace="System.Net"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[if(plist==null){return false;}var ignore=new string[]{"/_rels/","/package/","/[Content_Types].xml"};Action^<string,object^> dbg=delegate(string s,object p){if(debug){Console.WriteLine(s,p);}};Func^<string,string^> loc=delegate(string p){return Path.Combine(wpath,p??"");};Action^<string,string,string^> get=delegate(string link,string name,string path){var to=Path.GetFullPath(loc(path??name));if(Directory.Exists(to)){Console.WriteLine("`{0}` is already exists: \"{1}\"",name,to);return;}Console.Write("Getting `{0}` ... ",link);var tmp=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());using(var l=new WebClient()){try{l.Headers.Add("User-Agent","GetNuTool");l.UseDefaultCredentials=true;l.DownloadFile(url+link,tmp);}catch(Exception ex){Console.Error.WriteLine(ex.Message);return;}}Console.WriteLine("Extracting into \"{0}\"",to);using(var pkg=ZipPackage.Open(tmp,FileMode.Open,FileAccess.Read)){foreach(var part in pkg.GetParts()){var uri=Uri.UnescapeDataString(part.Uri.OriginalString);if(ignore.Any(x=^> uri.StartsWith(x,StringComparison.Ordinal))){continue;}var dest=Path.Combine(to,uri.TrimStart('/'));dbg("- `{0}`",uri);var dir=Path.GetDirectoryNam>> %tgnt%
<nul set /P =e(dest);if(!Directory.Exists(dir)){Directory.CreateDirectory(dir);}using(var src=part.GetStream(FileMode.Open,FileAccess.Read))using(var target=File.OpenWrite(dest)){try{src.CopyTo(target);}catch(FileFormatException ex){dbg("[x]?crc: {0}",dest);}}}}File.Delete(tmp);};foreach(var pkg in plist.Split(';')){var ident=pkg.Split(':');var link=ident[0];var path=(ident.Length ^> 1)?ident[1]: null;var name=link.Replace('/','.');if(!String.IsNullOrEmpty(defpath)){path=Path.Combine(defpath,path??name);}get(link,name,path);}]]^>^</Code^>^</Task^>^</UsingTask^>^<UsingTask TaskName="NGPack" TaskFactory="CodeTaskFactory" AssemblyFile="$(TaskCoreDllPath)"^>^<ParameterGroup^>^<dir ParameterType="System.String" Required="true"/^>^<dout ParameterType="System.String"/^>^<wpath ParameterType="System.String"/^>^<vtool ParameterType="System.String" Required="true"/^>^<debug ParameterType="System.Boolean"/^>^</ParameterGroup^>^<Task^>^<Reference Include="System.Xml"/^>^<Reference Include="System.Xml.Linq"/^>^<Reference Include="WindowsBase"/^>^<Using Namespace="System"/^>^<Using Namespace="System.Collections.Generic"/^>^<Using Namespace="System.IO"/^>^<Using Namespace="System.Linq"/^>^<Using Namespace="System.IO.Packaging"/^>^<Using Namespace="System.Xml.Linq"/^>^<Using Namespace="System.Text.RegularExpressions"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[var EXT_NUSPEC=".nuspec";var EXT_NUPKG=".nupkg";var TAG_META="metadata";var DEF_CONTENT_TYPE="application/octet";var MANIFEST_URL="http://schemas.microsoft.com/packaging/2010/07/manifest";var ID="id";var VER="version">> %tgnt%
<nul set /P =;Action^<string,object^> dbg=delegate(string s,object p){if(debug){Console.WriteLine(s,p);}};var _err=Console.Error;dir=Path.Combine(wpath,dir);if(!Directory.Exists(dir)){_err.WriteLine("`{0}` was not found.",dir);return false;}dout=Path.Combine(wpath,dout??"");var nuspec=Directory.GetFiles(dir,"*"+EXT_NUSPEC,SearchOption.TopDirectoryOnly).FirstOrDefault();if(nuspec==null){_err.WriteLine("{0} was not found in `{1}`",EXT_NUSPEC,dir);return false;}Console.WriteLine("Found {0}: `{1}`",EXT_NUSPEC,nuspec);var root=XDocument.Load(nuspec).Root.Elements().FirstOrDefault(x=^> x.Name.LocalName==TAG_META);if(root==null){_err.WriteLine("{0} does not contain {1}.",nuspec,TAG_META);return false;}var metadata=new Dictionary^<string,string^>();foreach(var tag in root.Elements()){metadata[tag.Name.LocalName.ToLower()]=tag.Value;}if(metadata[ID].Length ^> 100 ^|^|!Regex.IsMatch(metadata[ID],@"^\w+([_.-]\w+)*$",RegexOptions.IgnoreCase ^| RegexOptions.ExplicitCapture)){_err.WriteLine("The format of `{0}` is not correct.",ID);return false;}var ignore=new string[]{Path.Combine(dir,"_rels"),Path.Combine(dir,"package"),Path.Combine(dir,"[Content_Types].xml")};var pout=String.Format("{0}.{1}{2}",metadata[ID],metadata[VER],EXT_NUPKG);if(!String.IsNullOrWhiteSpace(dout)){if(!Directory.Exists(dout)){Directory.CreateDirectory(dout);}pout=Path.Combine(dout,pout);}Console.WriteLine("Started packing `{0}` ...",pout);using(var pkg=Package.Open(pout,FileMode.Create)){var manifestUri=new Uri(String.Format("/{0}{1}",metadata[ID],EXT_NUSPEC),UriKind.Relative);pkg.CreateRelationship(manif>> %tgnt%
<nul set /P =estUri,TargetMode.Internal,MANIFEST_URL);foreach(var file in Directory.GetFiles(dir,"*.*",SearchOption.AllDirectories)){if(ignore.Any(x=^> file.StartsWith(x,StringComparison.Ordinal))){continue;}string pUri;if(file.StartsWith(dir,StringComparison.OrdinalIgnoreCase)){pUri=file.Substring(dir.Length).TrimStart(Path.DirectorySeparatorChar);}else{pUri=file;}dbg("- `{0}`",pUri);var escaped=String.Join("/",pUri.Split('\\','/').Select(p=^> Uri.EscapeDataString(p)));var uri=PackUriHelper.CreatePartUri(new Uri(escaped,UriKind.Relative));var part=pkg.CreatePart(uri,DEF_CONTENT_TYPE,CompressionOption.Maximum);using(var tstream=part.GetStream())using(var fs=new FileStream(file,FileMode.Open,FileAccess.Read)){fs.CopyTo(tstream);}}Func^<string,string^> getmeta=delegate(string key){return(metadata.ContainsKey(key))?metadata[key]:"";};var _p=pkg.PackageProperties;_p.Creator=getmeta("authors");_p.Description=getmeta("description");_p.Identifier=metadata[ID];_p.Version=metadata[VER];_p.Keywords=getmeta("tags");_p.Title=getmeta("title");_p.LastModifiedBy="GetNuTool v"+vtool;}]]^>^</Code^>^</Task^>^</UsingTask^>^<Target Name="Build" DependsOnTargets="get"/^>^<PropertyGroup^>^<GetNuTool^>1.6.1.10620_bde3e50^</GetNuTool^>^<wpath Condition="'$(wpath)' == ''"^>$(MSBuildProjectDirectory)^</wpath^>^</PropertyGroup^>^<Target Name="header"^>^<Message Text="%%0D%%0AGetNuTool v$(GetNuTool) - github.com/3F%%0D%%0A=========%%0D%%0A" Importance="high"/^>^</Target^>^</Project^>>> %tgnt%


exit /B 0