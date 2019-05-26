@echo off
:: hMSBuild - 2.1.0.26714 [ 4b368c9 ]
:: Copyright (c) 2017-2019  Denis Kuzmin [ entry.reg@gmail.com ] GitHub/3F
:: Copyright (c) the hMSBuild contributors
set "aa=%~dp0"
set ab=%*
if not defined ab setlocal enableDelayedExpansion & goto bs
if not defined __p_call set ab=%ab:^=^^%
set ac=%ab:!= #__b_ECL## %
set ac=%ac:^= #__b_CRT## %
setlocal enableDelayedExpansion
set "ad=^"
set "ac=!ac:%%=%%%%!"
set "ac=!ac:&=%%ad%%&!"
:bs
set "ae=2.6.7"
set af=%temp%\hMSBuild_vswhere
set "ag="
set "ah="
set "ai="
set "aj="
set "ak="
set "al="
set "am="
set "an="
set "ao="
set "ap="
set "aq="
set "ar="
set /a as=0
if not defined ab goto bt
set ac=!ac:/?=/h!
call :bu bn ac bo
goto bv
:bw
echo.
@echo hMSBuild - 2.1.0.26714 [ 4b368c9 ]
@echo Copyright (c) 2017-2019  Denis Kuzmin [ entry.reg@gmail.com ] GitHub/3F
@echo Copyright (c) the hMSBuild contributors
echo.
echo Licensed under the MIT License
@echo https://github.com/3F/hMSBuild
echo.
@echo.
@echo Usage: hMSBuild [args to hMSBuild] [args to msbuild.exe or GetNuTool core]
echo ------
echo.
echo Arguments:
echo ----------
echo  -no-vs        - Disable searching from Visual Studio.
echo  -no-netfx     - Disable searching from .NET Framework.
echo  -no-vswhere   - Do not search via vswhere.
echo.
echo  -vsw-priority {IDs} - Non-strict components preference: C++ etc.
echo                        Separated by space: https://aka.ms/vs/workloads
echo.
echo  -vsw-version {arg}  - Specific version of vswhere. Where {arg}:
echo      * 2.5.2 ...
echo      * Keywords:
echo        `latest` - To get latest remote version;
echo        `local`  - To use only local versions;
echo                   (.bat;.exe /or from +15.2.26418.1 VS-build)
echo.
echo  -no-cache         - Do not cache vswhere for this request.
echo  -reset-cache      - To reset all cached vswhere versions before processing.
echo  -notamd64         - To use 32bit version of found msbuild.exe if it's possible.
echo  -stable           - It will ignore possible beta releases in last attempts.
echo  -eng              - Try to use english language for all build messages.
echo  -GetNuTool {args} - Access to GetNuTool core. https://github.com/3F/GetNuTool
echo  -only-path        - Only display fullpath to found MSBuild.
echo  -force            - Aggressive behavior for -vsw-priority, -notamd64, etc.
echo  -debug            - To show additional information from hMSBuild.
echo  -version          - Display version of hMSBuild.
echo  -help             - Display this help. Aliases: -help -h
echo.
echo.
echo ------
echo Flags:
echo ------
echo  __p_call - Tries to eliminate the difference for the call-type invoking %~nx0
echo.
echo --------
echo Samples:
echo --------
echo hMSBuild -notamd64 -vsw-version 2.5.2 "Conari.sln" /t:Rebuild
echo hMSBuild -vsw-version latest "Conari.sln"
echo.
echo hMSBuild -no-vswhere -no-vs -notamd64 "Conari.sln"
echo hMSBuild -no-vs "DllExport.sln"
echo hMSBuild vsSolutionBuildEvent.sln
echo.
echo hMSBuild -GetNuTool -unpack
echo hMSBuild -GetNuTool /p:ngpackages="Conari;regXwild"
echo.
echo hMSBuild -no-vs "DllExport.sln" ^|^| goto bx
goto by
:bv
set "at="
set /a au=0
:bz
set av=!bn[%au%]!
if [!av!]==[-help] ( goto bw ) else if [!av!]==[-h] ( goto bw ) else if [!av!]==[-?] ( goto bw )
if [!av!]==[-nocachevswhere] (
call :b0 -nocachevswhere -no-cache -reset-cache
set av=-no-cache
) else if [!av!]==[-novswhere] (
call :b0 -novswhere -no-vswhere
set av=-no-vswhere
) else if [!av!]==[-novs] (
call :b0 -novs -no-vs
set av=-no-vs
) else if [!av!]==[-nonet] (
call :b0 -nonet -no-netfx
set av=-no-netfx
) else if [!av!]==[-vswhere-version] (
call :b0 -vswhere-version -vsw-version
set av=-vsw-version
)
if [!av!]==[-debug] (
set am=1
goto b1
) else if [!av!]==[-GetNuTool] (
call :b2 "accessing to GetNuTool ..."
for /L %%p IN (0,1,8181) DO (
if "!escg:~%%p,10!"=="-GetNuTool" (
set aw=!escg:~%%p!
call :b3 !aw:~10!
set /a as=%ERRORLEVEL%
goto by
)
)
call :b2 "!av! is corrupted: !escg!"
set /a as=1
goto by
) else if [!av!]==[-no-vswhere] (
set aj=1
goto b1
) else if [!av!]==[-no-cache] (
set ak=1
goto b1
) else if [!av!]==[-reset-cache] (
set al=1
goto b1
) else if [!av!]==[-no-vs] (
set ah=1
goto b1
) else if [!av!]==[-no-netfx] (
set ai=1
goto b1
) else if [!av!]==[-notamd64] (
set ag=1
goto b1
) else if [!av!]==[-only-path] (
set an=1
goto b1
) else if [!av!]==[-eng] (
chcp 437 >nul
goto b1
) else if [!av!]==[-vsw-version] ( set /a "au+=1" & call :b4 bn[!au!] v
set ae=!v!
call :b2 "selected vswhere version:" v
set ao=1
goto b1
) else if [!av!]==[-version] (
@echo 2.1.0.26714 [ 4b368c9 ]
goto by
) else if [!av!]==[-vsw-priority] ( set /a "au+=1" & call :b4 bn[!au!] v
set ap=!v!
goto b1
) else if [!av!]==[-stable] (
set aq=1
goto b1
) else if [!av!]==[-force] (
set ar=1
goto b1
) else (
call :b2 "non-handled key:" bn{%au%}
set at=!at! !bn{%au%}!
)
:b1
set /a "au+=1" & if %au% LSS !bo! goto bz
:bt
if defined al (
call :b2 "resetting vswhere cache"
rmdir /S/Q "%af%" 2>nul
)
if not defined aj if not defined ah (
call :b5 bp
if defined bp goto b6
)
if not defined ah (
call :b7 bp
if defined bp goto b6
)
if not defined ai (
call :b8 bp
if defined bp goto b6
)
echo MSBuild tools was not found. Use `-debug` key for details.
set /a as=2
goto by
:b6
if defined an (
echo !bp!
goto by
)
set ax="!bp!"
echo hMSBuild: !ax!
if not defined at goto b9
set at=%at: #__b_CRT## =^%
set at=%at: #__b_ECL## =^!%
set at=!at: #__b_EQ## ==!
:b9
call :b2 "Arguments: " at
!ax! !at!
set /a as=%ERRORLEVEL%
goto by
:by
exit/B !as!
:b5
call :b2 "trying via vswhere..."
if defined ao if not "!ae!"=="local" (
call :b_ a4 ay
call :ca a4 bq ay
set %1=!bq!
exit/B 0
)
call :cb a4
set "ay="
if not defined a4 (
if "!ae!"=="local" (
set "%1=" & exit/B 2
)
call :b_ a4 ay
)
call :ca a4 bq ay
set %1=!bq!
exit/B 0
:cb
set az=!aa!vswhere
call :cc az br
if defined br set "%1=!az!" & exit/B 0
set a0=Microsoft Visual Studio\Installer
if exist "%ProgramFiles(x86)%\!a0!" set "%1=%ProgramFiles(x86)%\!a0!\vswhere" & exit/B 0
if exist "%ProgramFiles%\!a0!" set "%1=%ProgramFiles%\!a0!\vswhere" & exit/B 0
call :b2 "local vswhere is not found."
set "%1="
exit/B 3
:b_
if defined ak (
set a1=!af!\_mta\%random%%random%vswhere
) else (
set a1=!af!
if defined ae (
set a1=!a1!\!ae!
)
)
call :b2 "tvswhere: " a1
if "!ae!"=="latest" (
set a2=vswhere
) else (
set a2=vswhere/!ae!
)
set a3=/p:ngpackages="!a2!:vswhere" /p:ngpath="!a1!"
call :b2 "GetNuTool call: " a3
setlocal
set __p_call=1
if defined am (
call :b3 !a3!
) else (
call :b3 !a3! >nul
)
endlocal
set "%1=!a1!\vswhere\tools\vswhere"
set "%2=!a1!"
exit/B 0
:ca
set "a4=!%1!"
set "a5=!%3!"
call :cc a4 a4
if not defined a4 (
call :b2 "vswhere tool does not exist"
set "%2=" & exit/B 1
)
call :b2 "vswbin: " a4
set "a6="
set "a7="
set a8=!ap!
:cd
call :b2 "attempts with filter: " a8 a6
set "a9=" & set "a_="
for /F "usebackq tokens=1* delims=: " %%a in (`"!a4!" -nologo !a6! -requires !a8! Microsoft.Component.MSBuild`) do (
if /I "%%~a"=="installationPath" set a9=%%~b
if /I "%%~a"=="installationVersion" set a_=%%~b
if defined a9 if defined a_ (
call :ce a9 a_ a7
if defined a7 goto cf
set "a9=" & set "a_="
)
)
if not defined aq if not defined a6 (
set a6=-prerelease
goto cd
)
if defined a8 (
set ba=Tools was not found for: !a8!
if defined ar (
call :b2 "Ignored via -force. !ba!"
set "a7=" & goto cf
)
call :cg "!ba!"
set "a8=" & set "a6="
goto cd
)
:cf
if defined a5 if defined ak (
call :b2 "reset vswhere " a5
rmdir /S/Q "!a5!"
)
set %2=!a7!
exit/B 0
:ce
set a9=!%1!
set a_=!%2!
call :b2 "vspath: " a9
call :b2 "vsver: " a_
if not defined a_ (
call :b2 "nothing to see via vswhere"
set "%3=" & exit/B 3
)
for /F "tokens=1,2 delims=." %%a in ("!a_!") do (
set a_=%%~a.0
)
if !a_! geq 16 set a_=Current
if not exist "!a9!\MSBuild\!a_!\Bin" set "%3=" & exit/B 3
set bb=!a9!\MSBuild\!a_!\Bin
call :b2 "found path via vswhere: " bb
if exist "!bb!\amd64" (
call :b2 "found /amd64"
set bb=!bb!\amd64
)
call :ch bb bb
set %3=!bb!
exit/B 0
:b7
call :b2 "Searching from Visual Studio - 2015, 2013, ..."
for %%v in (14.0, 12.0) do (
call :ci %%v Y & if defined Y (
set %1=!Y!
exit/B 0
)
)
call :b2 "-vs: not found"
set "%1="
exit/B 0
:b8
call :b2 "Searching from .NET Framework - .NET 4.0, ..."
for %%v in (4.0, 3.5, 2.0) do (
call :ci %%v Y & if defined Y (
set %1=!Y!
exit/B 0
)
)
call :b2 "-netfx: not found"
set "%1="
exit/B 0
:ci
call :b2 "check %1"
for /F "usebackq tokens=2* skip=2" %%a in (
`reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\%1" /v MSBuildToolsPath 2^> nul`
) do if exist %%b (
set bb=%%~b
call :b2 ":msbfound " bb
call :ch bb bq
set %2=!bq!
exit/B 0
)
set "%2="
exit/B 0
:ch
set bb=!%~1!\MSBuild.exe
set %2=!bb!
if not defined ag (
exit/B 0
)
set bc=!bb:Framework64=Framework!
set bc=!bc:\amd64=!
if exist "!bc!" (
call :b2 "Return 32bit version because of -notamd64 key."
set %2=!bc!
exit/B 0
)
if defined ar (
call :b2 "Ignored via -force. Only 64bit version was found for -notamd64"
set "%2=" & exit/B 0
)
call :cg "Return 64bit version. Found only this."
exit/B 0
:cc
call :b2 "bat/exe: " %1
if exist "!%1!.bat" set %2="!%1!.bat" & exit/B 0
if exist "!%1!.exe" set %2="!%1!.exe" & exit/B 0
set "%2="
exit/B 0
:b0
call :cg "'%~1' is obsolete. Use alternative: %~2 %~3"
exit/B 0
:cg
echo   [*] WARN: %~1
exit/B 0
:b2
if defined am (
set bd=%1
set bd=!bd:~0,-1!
set bd=!bd:~1!
echo.[%TIME% ] !bd! !%2! !%3!
)
exit/B 0
:bu
set be=!%2!
:cj
for /F "tokens=1* delims==" %%a in ("!be!") do (
if "%%~b"=="" (
call :ck %1 !be! %3
exit/B 0
)
set be=%%a #__b_EQ## %%b
)
goto cj
:ck
set "bf=%~1"
set /a au=-1
:cl
set /a au+=1
set %bf%[!au!]=%~2
set %bf%{!au!}=%2
shift & if not "%~3"=="" goto cl
set /a au-=1
set %1=!au!
exit/B 0
:b4
set bg=!%1!
set "bg=%bg: #__b_CRT## =^%"
set "bg=%bg: #__b_ECL## =^!%"
set bg=!bg: #__b_EQ## ==!
set %2=!bg!
exit/B 0
:b3
setlocal disableDelayedExpansion
@echo off
:: GetNuTool - Executable version
:: Copyright (c) 2015-2018  Denis Kuzmin [ entry.reg@gmail.com ]
:: https://github.com/3F/GetNuTool
set bh=gnt.core
set bi="%temp%\%random%%random%%bh%"
if "%~1"=="-unpack" goto cm
set bj=%*
if defined __p_call if defined bj set bj=%bj:^^=^%
set bk=%__p_msb%
if defined bk goto cn
if "%~1"=="-msbuild" goto co
for %%v in (4.0, 14.0, 12.0, 3.5, 2.0) do (
for /F "usebackq tokens=2* skip=2" %%a in (
`reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\%%v" /v MSBuildToolsPath 2^> nul`
) do if exist %%b (
set bk="%%~b\MSBuild.exe"
goto cn
)
)
echo MSBuild was not found. Try -msbuild "fullpath" args 1>&2
exit/B 2
:co
shift
set bk=%1
shift
set bl=%bj:!= #__b_ECL## %
setlocal enableDelayedExpansion
set bl=!bl:%%=%%%%!
:cp
for /F "tokens=1* delims==" %%a in ("!bl!") do (
if "%%~b"=="" (
call :cq !bl!
exit/B %ERRORLEVEL%
)
set bl=%%a #__b_EQ## %%b
)
goto cp
:cq
shift & shift
set "bj="
:cr
set bj=!bj! %1
shift & if not "%~2"=="" goto cr
set bj=!bj: #__b_EQ## ==!
setlocal disableDelayedExpansion
set bj=%bj: #__b_ECL## =!%
:cn
call :cs
%bk% %bi% /nologo /p:wpath="%~dp0/" /v:m /m:4 %bj%
set "bk="
set bm=%ERRORLEVEL%
del /Q/F %bi%
exit/B %bm%
:cm
set bi="%~dp0\%bh%"
echo Generating minified version in %bi% ...
:cs
<nul set /P ="">%bi%
set a=PropertyGroup&set b=Condition&set c=ngpackages&set d=Target&set e=DependsOnTargets&set f=TaskCoreDllPath&set g=MSBuildToolsPath&set h=UsingTask&set i=CodeTaskFactory&set j=ParameterGroup&set k=Reference&set l=Include&set m=System&set n=Using&set o=Namespace&set p=IsNullOrEmpty&set q=return&set r=string&set s=delegate&set t=foreach&set u=WriteLine&set v=Combine&set w=Console.WriteLine&set x=Directory&set y=GetNuTool&set z=StringComparison&set _=EXT_NUSPEC
<nul set /P =^<!-- GetNuTool - github.com/3F/GetNuTool --^>^<!-- Copyright (c) 2015-2018  Denis Kuzmin [ entry.reg@gmail.com ] --^>^<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003"^>^<%a%^>^<ngconfig %b%="'$(ngconfig)'==''"^>packages.config^</ngconfig^>^<ngserver %b%="'$(ngserver)'==''"^>https://www.nuget.org/api/v2/package/^</ngserver^>^<%c% %b%="'$(%c%)'==''"^>^</%c%^>^<ngpath %b%="'$(ngpath)'==''"^>packages^</ngpath^>^</%a%^>^<%d% Name="get" BeforeTargets="Build" %e%="header"^>^<a^>^<Output PropertyName="plist" TaskParameter="Result"/^>^</a^>^<b plist="$(plist)"/^>^</%d%^>^<%d% Name="pack" %e%="header"^>^<c/^>^</%d%^>^<%a%^>^<%f% %b%="Exists('$(%g%)\Microsoft.Build.Tasks.v$(MSBuildToolsVersion).dll')"^>$(%g%)\Microsoft.Build.Tasks.v$(MSBuildToolsVersion).dll^</%f%^>^<%f% %b%="'$(%f%)'=='' and Exists('$(%g%)\Microsoft.Build.Tasks.Core.dll')"^>$(%g%)\Microsoft.Build.Tasks.Core.dll^</%f%^>^</%a%^>^<%h% TaskName="a" TaskFactory="%i%" AssemblyFile="$(%f%)"^>^<%j%^>^<Result Output="true"/^>^</%j%^>^<Task^>^<%k% %l%="%m%.Xml"/^>^<%k% %l%="%m%.Xml.Linq"/^>^<%n% %o%="%m%"/^>^<%n% %o%="%m%.Collections.Generic"/^>^<%n% %o%="%m%.IO"/^>^<%n% %o%="%m%.Xml.Linq"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[var a=@"$(ngconfig)";var b=@"$(%c%)";var c=@"$(wpath)";if(!String.%p%(b)){Result=b;%q% true;}var d=Console.Error;Action^<%r%,Queue^<%r%^>^>e=%s%(%r% f,Queue^<%r%^>g){%t%(var h in XDocument.Load(f).Descendants("package")){var i=h.Attribute("id");var j=h.Attribute("version");var k=h.Attribute("output");if(i==null){d.%u%("'id' does not exist in '{0}'",f);%q%;}var l=i.Value;if(j!=null){l+="/"+j.Value;}if(k!=null){g.Enqueue(l+":"+k.Value);continue;}g.Enqueue(l);}};var m=new Queue^<%r%^>();%t%(var f in a.Split(new char[]{a.IndexOf('^|')!=-1?'^|':';'},(StringSplitOptions)1)){>>%bi%
<nul set /P =var n=Path.%v%(c,f);if(File.Exists(n)){e(n,m);}else{d.%u%(".config '{0}' was not found.",n);}}if(m.Count^<1){d.%u%("Empty list. Use .config or /p:%c%=\"...\"\n");}else{Result=%r%.Join("|",m.ToArray());}]]^>^</Code^>^</Task^>^</%h%^>^<%h% TaskName="b" TaskFactory="%i%" AssemblyFile="$(%f%)"^>^<%j%^>^<plist/^>^</%j%^>^<Task^>^<%k% %l%="WindowsBase"/^>^<%n% %o%="%m%"/^>^<%n% %o%="%m%.IO"/^>^<%n% %o%="%m%.IO.Packaging"/^>^<%n% %o%="%m%.Net"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[var a=@"$(ngserver)";var b=@"$(wpath)";var c=@"$(ngpath)";var d=@"$(proxycfg)";var e=@"$(debug)"=="true";if(plist==null){%q% false;}ServicePointManager.SecurityProtocol^|=SecurityProtocolType.Tls11^|SecurityProtocolType.Tls12;var f=new %r%[]{"/_rels/","/package/","/[Content_Types].xml"};Action^<%r%,object^>g=%s%(%r% h,object i){if(e){%w%(h,i);}};Func^<%r%,WebProxy^>j=%s%(%r% k){var l=k.Split('@');if(l.Length^<=1){%q% new WebProxy(l[0],false);}var m=l[0].Split(':');%q% new WebProxy(l[1],false){Credentials=new NetworkCredential(m[0],(m.Length^>1)?m[1]:null)};};Func^<%r%,%r%^>n=%s%(%r% i){%q% Path.%v%(b,i??"");};Action^<%r%,%r%,%r%^>o=%s%(%r% p,%r% q,%r% r){var s=Path.GetFullPath(n(r??q));if(%x%.Exists(s)){%w%("`{0}` is already exists: \"{1}\"",q,s);%q%;}Console.Write("Getting `{0}` ... ",p);var t=Path.%v%(Path.GetTempPath(),Guid.NewGuid().ToString());using(var u=new WebClient()){try{if(!String.%p%(d)){u.Proxy=j(d);}u.Headers.Add("User-Agent","%y% $(%y%)");u.UseDefaultCredentials=true;u.DownloadFile(a+p,t);}catch(Exception v){Console.Error.%u%(v.Message);%q%;}}%w%("Extracting into \"{0}\"",s);using(var w=ZipPackage.Open(t,FileMode.Open,FileAccess.Read)){%t%(var x in w.GetParts()){var y=Uri.UnescapeDataString(x.Uri.OriginalString);if(f.Any(z=^>y.StartsWith(z,%z%.Ordinal))){continue;}var _=Path.%v%(s,y.TrimStart(>>%bi%
<nul set /P ='/'));g("- `{0}`",y);var aa=Path.GetDirectoryName(_);if(!%x%.Exists(aa)){%x%.CreateDirectory(aa);}using(Stream ab=x.GetStream(FileMode.Open,FileAccess.Read))using(var ac=File.OpenWrite(_)){try{ab.CopyTo(ac);}catch(FileFormatException v){g("[x]?crc: {0}",_);}}}}File.Delete(t);};%t%(var w in plist.Split(new char[]{plist.IndexOf('^|')!=-1?'^|':';'},(StringSplitOptions)1)){var ad=w.Split(new char[]{':'},2);var p=ad[0];var r=(ad.Length^>1)?ad[1]:null;var q=p.Replace('/','.');if(!String.%p%(c)){r=Path.%v%(c,r??q);}o(p,q,r);}]]^>^</Code^>^</Task^>^</%h%^>^<%h% TaskName="c" TaskFactory="%i%" AssemblyFile="$(%f%)"^>^<Task^>^<%k% %l%="%m%.Xml"/^>^<%k% %l%="%m%.Xml.Linq"/^>^<%k% %l%="WindowsBase"/^>^<%n% %o%="%m%"/^>^<%n% %o%="%m%.Collections.Generic"/^>^<%n% %o%="%m%.IO"/^>^<%n% %o%="%m%.Linq"/^>^<%n% %o%="%m%.IO.Packaging"/^>^<%n% %o%="%m%.Xml.Linq"/^>^<%n% %o%="%m%.Text.RegularExpressions"/^>^<Code Type="Fragment" Language="cs"^>^<![CDATA[var a=@"$(ngin)";var b=@"$(ngout)";var c=@"$(wpath)";var d=@"$(debug)"=="true";var %_%=".nuspec";var EXT_NUPKG=".nupkg";var TAG_META="metadata";var DEF_CONTENT_TYPE="application/octet";var MANIFEST_URL="http://schemas.microsoft.com/packaging/2010/07/manifest";var ID="id";var VER="version";Action^<%r%,object^>e=%s%(%r% f,object g){if(d){%w%(f,g);}};var h=Console.Error;a=Path.%v%(c,a);if(!%x%.Exists(a)){h.%u%("`{0}` was not found.",a);%q% false;}b=Path.%v%(c,b);var i=%x%.GetFiles(a,"*"+%_%,SearchOption.TopDirectoryOnly).FirstOrDefault();if(i==null){h.%u%("{0} was not found in `{1}`",%_%,a);%q% false;}%w%("Found {0}: `{1}`",%_%,i);var j=XDocument.Load(i).Root.Elements().FirstOrDefault(k=^>k.Name.LocalName==TAG_META);if(j==null){h.%u%("{0} does not contain {1}.",i,TAG_META);%q% false;}var l=new Dictionary^<%r%,%r%^>();%t%(var m in j.Elements()){l[m.Name.LocalName.ToL>>%bi%
<nul set /P =ower()]=m.Value;}if(l[ID].Length^>100^|^|!Regex.IsMatch(l[ID],@"^\w+([_.-]\w+)*$",RegexOptions.IgnoreCase^|RegexOptions.ExplicitCapture)){h.%u%("The format of `{0}` is not correct.",ID);%q% false;}var n=new %r%[]{Path.%v%(a,"_rels"),Path.%v%(a,"package"),Path.%v%(a,"[Content_Types].xml")};var o=%r%.Format("{0}.{1}{2}",l[ID],l[VER],EXT_NUPKG);if(!String.IsNullOrWhiteSpace(b)){if(!%x%.Exists(b)){%x%.CreateDirectory(b);}o=Path.%v%(b,o);}%w%("Started packing `{0}` ...",o);using(var p=Package.Open(o,FileMode.Create)){Uri q=new Uri(String.Format("/{0}{1}",l[ID],%_%),UriKind.Relative);p.CreateRelationship(q,TargetMode.Internal,MANIFEST_URL);%t%(var r in %x%.GetFiles(a,"*.*",SearchOption.AllDirectories)){if(n.Any(k=^>r.StartsWith(k,%z%.Ordinal))){continue;}%r% s;if(r.StartsWith(a,%z%.OrdinalIgnoreCase)){s=r.Substring(a.Length).TrimStart(Path.DirectorySeparatorChar);}else{s=r;}e("- `{0}`",s);var t=%r%.Join("/",s.Split('\\','/').Select(g=^>Uri.EscapeDataString(g)));Uri u=PackUriHelper.CreatePartUri(new Uri(t,UriKind.Relative));var v=p.CreatePart(u,DEF_CONTENT_TYPE,CompressionOption.Maximum);using(Stream w=v.GetStream())using(var x=new FileStream(r,FileMode.Open,FileAccess.Read)){x.CopyTo(w);}}Func^<%r%,%r%^>y=%s%(%r% z){%q%(l.ContainsKey(z))?l[z]:"";};var _=p.PackageProperties;_.Creator=y("authors");_.Description=y("description");_.Identifier=l[ID];_.Version=l[VER];_.Keywords=y("tags");_.Title=y("title");_.LastModifiedBy="%y% $(%y%)";}]]^>^</Code^>^</Task^>^</%h%^>^<%d% Name="Build" %e%="get"/^>^<%a%^>^<%y%^>1.7.0.38876_4bc1dfb^</%y%^>^<wpath %b%="'$(wpath)'==''"^>$(MSBuildProjectDirectory)^</wpath^>^</%a%^>^<%d% Name="header"^>^<Message Text="%%0D%%0A%y% $(%y%) - github.com/3F%%0D%%0A=========%%0D%%0A" Importance="high"/^>^</%d%^>^</Project^>>>%bi%
exit/B 0