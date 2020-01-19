@ECHO OFF
REM This script generates the directory/file structure required for
REM Advanced Installer to use as source files

cd /d "%~p0"
SET Configuration=%1
SET Output=InstallerSource\
SET ARCH=x64
rd /q /s %Output% 2>nul

ECHO ".NET Standard libraries"
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.Win32.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\netstandard.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.AppContext.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.Concurrent.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.Immutable.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.NonGeneric.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Collections.Specialized.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.EventBasedAsync.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ComponentModel.TypeConverter.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.AttributedModel.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.Convention.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.Hosting.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.Runtime.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Composition.TypedParts.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Console.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Data.Common.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Contracts.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Debug.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.FileVersionInfo.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Process.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.StackTrace.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.TextWriterTraceListener.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Tools.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.TraceSource.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Diagnostics.Tracing.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Drawing.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Dynamic.Runtime.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Globalization.Calendars.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Globalization.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Globalization.Extensions.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Compression.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Compression.ZipFile.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.DriveInfo.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.FileSystem.Watcher.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.IsolatedStorage.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.MemoryMappedFiles.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Pipes.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.UnmanagedMemoryStream.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.Expressions.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.Parallel.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Linq.Queryable.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Http.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.NameResolution.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.NetworkInformation.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Ping.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Requests.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Security.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.Sockets.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.WebHeaderCollection.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.WebSockets.Client.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Net.WebSockets.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ObjectModel.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reactive.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reactive.Interfaces.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reactive.Linq.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reflection.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reflection.Extensions.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Reflection.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Resources.Reader.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Resources.ResourceManager.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Resources.Writer.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.CompilerServices.VisualC.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Extensions.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Handles.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.InteropServices.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Numerics.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Formatters.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Json.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.Serialization.Xml.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Claims.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Algorithms.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Csp.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Encoding.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.Primitives.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Cryptography.X509Certificates.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.Principal.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Security.SecureString.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Text.Encoding.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Text.Encoding.Extensions.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Text.RegularExpressions.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Overlapped.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Tasks.Dataflow.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Tasks.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Tasks.Parallel.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Thread.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.ThreadPool.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Threading.Timer.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.ReaderWriter.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XDocument.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XmlDocument.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XmlSerializer.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XPath.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Xml.XPath.XDocument.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Resource libs"
xcopy /y /i ..\GitExtensions\bin\%Configuration%\cs\Microsoft.VisualStudio.Composition.resources.dll %Output%cs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\cs\Microsoft.VisualStudio.Threading.resources.dll %Output%cs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\cs\Microsoft.VisualStudio.Validation.resources.dll %Output%cs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\de\Microsoft.VisualStudio.Composition.resources.dll %Output%de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\de\Microsoft.VisualStudio.Threading.resources.dll %Output%de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\de\Microsoft.VisualStudio.Validation.resources.dll %Output%de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\es\Microsoft.VisualStudio.Composition.resources.dll %Output%es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\es\Microsoft.VisualStudio.Threading.resources.dll %Output%es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\es\Microsoft.VisualStudio.Validation.resources.dll %Output%es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\fr\Microsoft.VisualStudio.Composition.resources.dll %Output%fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\fr\Microsoft.VisualStudio.Threading.resources.dll %Output%fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\fr\Microsoft.VisualStudio.Validation.resources.dll %Output%fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\it\Microsoft.VisualStudio.Composition.resources.dll %Output%it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\it\Microsoft.VisualStudio.Threading.resources.dll %Output%it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\it\Microsoft.VisualStudio.Validation.resources.dll %Output%it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ja\Microsoft.VisualStudio.Composition.resources.dll %Output%ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ja\Microsoft.VisualStudio.Threading.resources.dll %Output%ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ja\Microsoft.VisualStudio.Validation.resources.dll %Output%ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ko\Microsoft.VisualStudio.Composition.resources.dll %Output%ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ko\Microsoft.VisualStudio.Threading.resources.dll %Output%ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ko\Microsoft.VisualStudio.Validation.resources.dll %Output%ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pl\Microsoft.VisualStudio.Composition.resources.dll %Output%pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pl\Microsoft.VisualStudio.Threading.resources.dll %Output%pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pl\Microsoft.VisualStudio.Validation.resources.dll %Output%pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\cs\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\cs\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\de\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\de\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\es\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\es\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\fr\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\fr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\it\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\it\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ja\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\ja\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ko\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\ko\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pl\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\pl\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\pt-BR\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\pt-BR\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\ru\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\tr\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hans\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.TeamFoundation.Common.resources.dll %Output%Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.TeamFoundation.Core.WebApi.resources.dll %Output%Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.TeamFoundation.Dashboards.WebApi.resources.dll %Output%Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.VisualStudio.Services.Common.resources.dll %Output%Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\zh-Hant\Microsoft.VisualStudio.Services.WebApi.resources.dll %Output%Plugins\zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pt-BR\Microsoft.VisualStudio.Composition.resources.dll %Output%pt-BR\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pt-BR\Microsoft.VisualStudio.Threading.resources.dll %Output%pt-BR\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\pt-BR\Microsoft.VisualStudio.Validation.resources.dll %Output%pt-BR\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ru\Microsoft.VisualStudio.Composition.resources.dll %Output%ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ru\Microsoft.VisualStudio.Threading.resources.dll %Output%ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ru\Microsoft.VisualStudio.Validation.resources.dll %Output%ru\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\tr\Microsoft.VisualStudio.Composition.resources.dll %Output%tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\tr\Microsoft.VisualStudio.Threading.resources.dll %Output%tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\tr\Microsoft.VisualStudio.Validation.resources.dll %Output%tr\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hans\Microsoft.VisualStudio.Composition.resources.dll %Output%zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hans\Microsoft.VisualStudio.Threading.resources.dll %Output%zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hans\Microsoft.VisualStudio.Validation.resources.dll %Output%zh-Hans\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hant\Microsoft.VisualStudio.Composition.resources.dll %Output%zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hant\Microsoft.VisualStudio.Threading.resources.dll %Output%zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\zh-Hant\Microsoft.VisualStudio.Validation.resources.dll %Output%zh-Hant\
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Main output"
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ConEmu %Output%ConEmu
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ConEmu.WinForms.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Git.hub.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitCommands.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtUtils.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\AdysTech.CredentialManager.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtensions.exe %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitExtensions.exe.config %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitUI.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.IO.Abstractions.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.ValueTuple.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\GitUIPluginInterfaces.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\ICSharpCode.SharpZipLib.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ICSharpCode.TextEditor.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\AppInsights.WindowsDesktop.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.ApplicationInsights.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.WindowsAPICodePack.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.WindowsAPICodePack.Shell.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\SmartFormat.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Ben.Demystifier.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\NetSpell.SpellChecker.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\PSTaskDialog.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\ResourceManager.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\RestSharp.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Composition.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Threading.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Microsoft.VisualStudio.Validation.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\System.Runtime.InteropServices.RuntimeInformation.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\EasyHook.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\EasyHook32.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\EasyHook64.dll %Output%
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Themes\dark.colors %Output%Themes\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Themes\win10default.colors %Output%Themes\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\gitex.cmd %Output%
IF ERRORLEVEL 1 EXIT /B 1
IF %ARCH%==x86 xcopy /y /i ..\GitExtensionsShellEx\%Configuration%\GitExtensionsShellEx32.dll %Output%
IF %ARCH%==x86 IF ERRORLEVEL 1 EXIT /B 1
IF %ARCH%==x64 xcopy /y /i ..\GitExtensionsShellEx\%Configuration%-x64\GitExtensionsShellEx64.dll %Output%
IF %ARCH%==x64 IF ERRORLEVEL 1 EXIT /B 1
IF %ARCH%==x86 xcopy /y /i ..\GitExtSshAskPass\%Configuration%\GitExtSshAskPass.exe %Output%
IF %ARCH%==x86 IF ERRORLEVEL 1 EXIT /B 1
IF %ARCH%==x64 xcopy /y /i ..\GitExtSshAskPass\%Configuration%-x64\GitExtSshAskPass.exe %Output%
IF %ARCH%==x64 IF ERRORLEVEL 1 EXIT /B 1

ECHO "Plugins"
xcopy /y /i ..\Plugins\AutoCompileSubmodules\bin\%Configuration%\AutoCompileSubmodules.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BackgroundFetch\bin\%Configuration%\BackgroundFetch.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Bitbucket\bin\%Configuration%\Bitbucket.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\AppVeyorIntegration\bin\%Configuration%\AppVeyorIntegration.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\JenkinsIntegration\bin\%Configuration%\JenkinsIntegration.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\TeamCityIntegration\bin\%Configuration%\TeamCityIntegration.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\BuildServerIntegration\AzureDevOpsIntegration\bin\%Configuration%\AzureDevOpsIntegration.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\CreateLocalBranches\bin\%Configuration%\CreateLocalBranches.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\DeleteUnusedBranches\bin\%Configuration%\DeleteUnusedBranches.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\FindLargeFiles\bin\%Configuration%\FindLargeFiles.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gerrit\bin\%Configuration%\Gerrit.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gerrit\bin\%Configuration%\Newtonsoft.Json.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\GitFlow\bin\%Configuration%\GitFlow.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Github3\bin\%Configuration%\Github3.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Gource\bin\%Configuration%\Gource.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\Atlassian.Jira.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\JiraCommitHintPlugin.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\JiraCommitHintPlugin\bin\%Configuration%\NString.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\ProxySwitcher\bin\%Configuration%\ProxySwitcher.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\ReleaseNotesGenerator\bin\%Configuration%\ReleaseNotesGenerator.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Statistics\GitImpact\bin\%Configuration%\GitImpact.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\Plugins\Statistics\GitStatistics\bin\%Configuration%\GitStatistics.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Build2.WebApi.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Common.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.TeamFoundation.Core.WebApi.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.VisualStudio.Services.Common.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\Microsoft.VisualStudio.Services.WebApi.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\System.Net.Http.Formatting.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsIntegration.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2012.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitExtensions\bin\%Configuration%\Plugins\TfsInterop.Vs2015.dll %Output%Plugins\
IF ERRORLEVEL 1 EXIT /B 1

ECHO "UserPlugins"
xcopy /y /e ..\Plugins\GitExtensions.PluginManager\Output %Output%UserPlugins\GitExtensions.PluginManager\
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Translation"
xcopy /y /i ..\GitUI\Translation\English.gif %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\English.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\English.Plugins.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\French.gif %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\French.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\French.Plugins.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\German.gif %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\German.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\German.Plugins.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Polish.gif %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Polish.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Polish.Plugins.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Russian.gif %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Russian.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\GitUI\Translation\Russian.Plugins.xlf %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i "..\GitUI\Translation\Simplified Chinese.gif" %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i "..\GitUI\Translation\Simplified Chinese.xlf" %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i "..\GitUI\Translation\Simplified Chinese.Plugins.xlf" %Output%Translation\
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Dictionaries"
xcopy /y /i ..\bin\Dictionaries %Output%Dictionaries\
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Diff scripts"
xcopy /y /i ..\bin\Diff-Scripts\merge-* %Output%Diff-Scripts\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\Diff-Scripts\*.txt %Output%Diff-Scripts\
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Putty"
xcopy /y /i ..\bin\pageant.exe %Output%PuTTY\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\plink.exe %Output%PuTTY\
IF ERRORLEVEL 1 EXIT /B 1
xcopy /y /i ..\bin\puttygen.exe %Output%PuTTY\
IF ERRORLEVEL 1 EXIT /B 1

ECHO "Logo"
xcopy /y /i ..\Logo\git-extensions-logo.ico %Output%
IF ERRORLEVEL 1 EXIT /B 1