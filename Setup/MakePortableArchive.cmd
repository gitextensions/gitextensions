@echo off
REM Run Prepare-Release.ps1 instead to make a portable build.  This script is called by that script
REM To make a portable build run either of these commands and then this script
REM .\Set-Portable.ps1 -IsPortable
REM .\Set-Portable.ps Debug -IsPortable

cd /d "%~p0"

SET Configuration=%1
IF "%Configuration%"=="" SET Configuration=Release

rd /q /s GitExtensions\ 2>nul

REM Some plugins are not included, like TeamFoundation/TfsIntegration with related dlls

REM .net Standard
echo ".NET Standard libraries"
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.Win32.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\netstandard.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.AppContext.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.Concurrent.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.NonGeneric.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.Specialized.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.EventBasedAsync.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.TypeConverter.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.AttributedModel.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.Convention.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.Hosting.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.Runtime.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.TypedParts.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Console.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Data.Common.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Contracts.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Debug.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.FileVersionInfo.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Process.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.StackTrace.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.TextWriterTraceListener.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Tools.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.TraceSource.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Tracing.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Drawing.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Dynamic.Runtime.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Globalization.Calendars.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Globalization.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Globalization.Extensions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Compression.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Compression.ZipFile.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.DriveInfo.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.Watcher.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.IsolatedStorage.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.MemoryMappedFiles.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Pipes.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.UnmanagedMemoryStream.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.Expressions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.Parallel.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.Queryable.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Http.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.NameResolution.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.NetworkInformation.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Ping.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Requests.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Security.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Sockets.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.WebHeaderCollection.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.WebSockets.Client.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.WebSockets.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ObjectModel.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reactive.Windows.Threading.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reflection.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reflection.Extensions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reflection.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Resources.Reader.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Resources.ResourceManager.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Resources.Writer.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.CompilerServices.VisualC.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Extensions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Handles.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.InteropServices.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Numerics.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Formatters.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Json.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Xml.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Claims.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Algorithms.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Csp.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Encoding.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Primitives.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.X509Certificates.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Principal.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.SecureString.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Text.Encoding.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Text.Encoding.Extensions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Text.RegularExpressions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Overlapped.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Tasks.Dataflow.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Tasks.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Tasks.Parallel.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Thread.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.ThreadPool.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Timer.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.ReaderWriter.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XDocument.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XmlDocument.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XmlSerializer.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XPath.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XPath.XDocument.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1

REM Resources from dependencies
echo "Resource libs"
xcopy /y /i ..\GitExtensions\bin\%Configuration%\cs\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\cs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\cs\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\cs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\cs\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\cs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\de\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\de\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\de\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\de\NBug.resources.dll GitExtensions\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\es\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\es\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\es\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\es-MX\NBug.resources.dll GitExtensions\es-MX\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\fi-FI\NBug.resources.dll GitExtensions\fi-FI\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\fr\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\fr\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\fr\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\hr\NBug.resources.dll GitExtensions\hr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\it\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\it\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\it\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ja\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ja\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ja\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ja\NBug.resources.dll GitExtensions\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ko\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ko\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ko\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ko-KR\NBug.resources.dll GitExtensions\ko-KR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pl\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\pl\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pl\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\pl\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pl\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.TeamFoundation.Common.resources.dll GitExtensions\Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.TeamFoundation.Core.WebApi.resources.dll GitExtensions\Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll GitExtensions\Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.VisualStudio.Services.Common.resources.dll GitExtensions\Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.VisualStudio.Services.WebApi.resources.dll GitExtensions\Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pt-BR\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pt-BR\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pt-BR\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pt-BR\NBug.resources.dll GitExtensions\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ru\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ru\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ru\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\ru\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ru-RU\NBug.resources.dll GitExtensions\ru-RU\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\tr\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\tr\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\tr\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\tr\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hans\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hans\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hans\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hant\Microsoft.VisualStudio.Composition.resources.dll GitExtensions\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hant\Microsoft.VisualStudio.Threading.resources.dll GitExtensions\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hant\Microsoft.VisualStudio.Validation.resources.dll GitExtensions\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1 


REM Main output
echo "main output"
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ConEmu GitExtensions\ConEmu
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ConEmu.WinForms.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Git.hub.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitCommands.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtUtils.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtensions.exe GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtensions.exe.config GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitUI.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Abstractions.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ValueTuple.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitUIPluginInterfaces.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\ICSharpCode.SharpZipLib.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ICSharpCode.TextEditor.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.WindowsAPICodePack.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.WindowsAPICodePack.Shell.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\NBug.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\SmartFormat.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\NetSpell.SpellChecker.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\PSTaskDialog.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ResourceManager.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Composition.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Threading.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Validation.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.InteropServices.RuntimeInformation.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1

REM Plugins
echo "Plugins"

REM xcopy /y /e ..\Plugins\GitExtensions.PluginManager\Output GitExtensions\Plugins\
REM IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\AutoCompileSubmodules\bin\%Configuration%\AutoCompileSubmodules.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BackgroundFetch\bin\%Configuration%\BackgroundFetch.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.Core.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.Interfaces.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.Linq.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BackgroundFetch\bin\%Configuration%\System.Reactive.PlatformServices.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Bitbucket\bin\%Configuration%\Bitbucket.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\AppVeyorIntegration\bin\%Configuration%\AppVeyorIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\JenkinsIntegration\bin\%Configuration%\JenkinsIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\%Configuration%\TeamCityIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\AzureDevOpsIntegration\bin\%Configuration%\AzureDevOpsIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\CreateLocalBranches\bin\%Configuration%\CreateLocalBranches.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\DeleteUnusedBranches\bin\%Configuration%\DeleteUnusedBranches.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\FindLargeFiles\bin\%Configuration%\FindLargeFiles.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gerrit\bin\%Configuration%\Gerrit.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gerrit\bin\%Configuration%\Newtonsoft.Json.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\GitFlow\bin\%Configuration%\GitFlow.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Github3\bin\%Configuration%\RestSharp.dll GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Github3\bin\%Configuration%\Github3.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gource\bin\%Configuration%\Gource.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\Atlassian.Jira.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\JiraCommitHintPlugin.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\NString.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\System.Collections.Immutable.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\ProxySwitcher\bin\%Configuration%\ProxySwitcher.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\ReleaseNotesGenerator\bin\%Configuration%\ReleaseNotesGenerator.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Statistics\GitImpact\bin\%Configuration%\GitImpact.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Statistics\GitStatistics\bin\%Configuration%\GitStatistics.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\GitUIPluginInterfaces.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\JetBrains.Annotations.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Build.Client.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Build2.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Chat.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Client.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Common.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Core.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Dashboards.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.DistributedTask.Common.Contracts.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Policy.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.SourceControl.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Test.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.TestManagement.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Work.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.WorkItemTracking.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.VisualStudio.Services.Common.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.VisualStudio.Services.WebApi.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Newtonsoft.Json.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\System.Net.Http.Formatting.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsIntegration.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2012.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2013.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2015.dll GitExtensions\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 

REM Translation
echo "Translation"
xcopy /y /i ..\GitUI\Translation\English.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\English.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\English.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Czech.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Czech.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Czech.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Dutch.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Dutch.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Dutch.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\French.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\French.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\French.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\German.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\German.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\German.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Italian.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Italian.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Italian.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Japanese.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Japanese.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Japanese.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Korean.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Korean.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Korean.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Polish.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Polish.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Polish.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Russian.gif GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Russian.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Russian.Plugins.xlf GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i "..\GitUI\Translation\Simplified Chinese.gif" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i "..\GitUI\Translation\Simplified Chinese.xlf" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i "..\GitUI\Translation\Simplified Chinese.Plugins.xlf" GitExtensions\Translation\
IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Spanish.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Spanish.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i ..\GitUI\Translation\Spanish.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i "..\GitUI\Translation\Traditional Chinese.gif" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i "..\GitUI\Translation\Traditional Chinese.xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i "..\GitUI\Translation\Traditional Chinese.Plugins.xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1
REM xcopy /y /i "..\GitExtensions\bin\%Configuration%\Translation\Portuguese (Brazil).gif" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i "..\GitExtensions\bin\%Configuration%\Translation\Portuguese (Portugal).gif" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i ..\GitExtensions\bin\%Configuration%\Translation\Romanian.gif GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i "..\GitExtensions\bin\%Configuration%\Translation\Portuguese (Brazil).Plugins.xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i "..\GitExtensions\bin\%Configuration%\Translation\Portuguese (Brazil).xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i "..\GitExtensions\bin\%Configuration%\Translation\Portuguese (Portugal).Plugins.xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i "..\GitExtensions\bin\%Configuration%\Translation\Portuguese (Portugal).xlf" GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i ..\GitExtensions\bin\%Configuration%\Translation\Romanian.Plugins.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 
REM xcopy /y /i ..\GitExtensions\bin\%Configuration%\Translation\Romanian.xlf GitExtensions\Translation\
REM IF ERRORLEVEL 1 EXIT /B 1 



REM Dictionaries
echo "Dictionaries"
xcopy /y /i ..\bin\Dictionaries GitExtensions\Dictionaries\
IF ERRORLEVEL 1 EXIT /B 1
REM Diff scripts
echo "Diff scripts"
xcopy /y /i ..\bin\Diff-Scripts\merge-* GitExtensions\Diff-Scripts\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\Diff-Scripts\*.txt GitExtensions\Diff-Scripts\
IF ERRORLEVEL 1 EXIT /B 1

REM PUTTY
echo "Putty"
xcopy /y /i ..\bin\pageant.exe GitExtensions\PuTTY\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\plink.exe GitExtensions\PuTTY\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\puttygen.exe GitExtensions\PuTTY\
IF ERRORLEVEL 1 EXIT /B 1

REM LOGO
echo "logo"
xcopy /y /i ..\Logo\git-extensions-logo.ico GitExtensions\
IF ERRORLEVEL 1 EXIT /B 1

REM PDB's
echo "PDB files"
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ConEmu.WinForms.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Git.hub.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitCommands.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtUtils.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtensions.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitUI.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitUIPluginInterfaces.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\NBug.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\NetSpell.SpellChecker.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ResourceManager.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1

xcopy /y /i ..\Plugins\AutoCompileSubmodules\bin\%Configuration%\AutoCompileSubmodules.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BackgroundFetch\bin\%Configuration%\BackgroundFetch.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Bitbucket\bin\%Configuration%\Bitbucket.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\AppVeyorIntegration\bin\%Configuration%\AppVeyorIntegration.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\JenkinsIntegration\bin\%Configuration%\JenkinsIntegration.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\%Configuration%\TeamCityIntegration.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\AzureDevOpsIntegration\bin\%Configuration%\AzureDevOpsIntegration.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\CreateLocalBranches\bin\%Configuration%\CreateLocalBranches.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\DeleteUnusedBranches\bin\%Configuration%\DeleteUnusedBranches.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\FindLargeFiles\bin\%Configuration%\FindLargeFiles.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gerrit\bin\%Configuration%\Gerrit.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\GitFlow\bin\%Configuration%\GitFlow.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Github3\bin\%Configuration%\Github3.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gource\bin\%Configuration%\Gource.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\JiraCommitHintPlugin.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\ProxySwitcher\bin\%Configuration%\ProxySwitcher.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\ReleaseNotesGenerator\bin\%Configuration%\ReleaseNotesGenerator.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Statistics\GitImpact\bin\%Configuration%\GitImpact.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Statistics\GitStatistics\bin\%Configuration%\GitStatistics.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ICSharpCode.TextEditor.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\SmartFormat.pdb GitExtensions-pdbs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\GitUIPluginInterfaces.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsIntegration.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2012.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2013.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2015.pdb GitExtensions-pdbs\Plugins\
IF ERRORLEVEL 1 EXIT /B 1 

powershell.exe -executionpolicy Bypass -File  .\Get-Hashes.ps1 > Hashes.txt
move Hashes.txt GitExtensions\

set nuget=..\.nuget\nuget.exe
%nuget% update -self
%nuget% install ..\.nuget\packages.config -OutputDirectory ..\packages -Verbosity Quiet

SET version=%2
if not "%APPVEYOR_BUILD_VERSION%"=="" set version=%APPVEYOR_BUILD_VERSION%
set portable=GitExtensions-Portable-%version%.zip
set pdbs=GitExtensions-pdbs-%version%.zip

del %portable% 2>nul
del %pdbs% 2>nul

set szip="..\packages\7-Zip.CommandLine.9.20.0\tools\7za"
%szip% a -tzip %portable% GitExtensions
IF ERRORLEVEL 1 EXIT /B 1
%szip% a -tzip %pdbs% .\GitExtensions-pdbs\*
IF ERRORLEVEL 1 EXIT /B 1
